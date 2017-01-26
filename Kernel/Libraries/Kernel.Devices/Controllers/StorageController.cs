using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Exceptions;
using Kernel.Framework.Processes;
using Kernel.Framework.Processes.Requests.Pipes;
using Kernel.Pipes.Storage;
using Kernel.Utilities;

namespace Kernel.Devices.Controllers
{
    /// <summary>
    ///     A storage controller is the interconnect between processes wishing to read/write storage mediums (clients accessing disks)
    ///     and the drivers managing those disks.
    /// </summary>
    /// <remarks>
    ///     A storage controller serialises the requests from multiple clients to the disk driver
    ///     and handles receiving any read data from the disk followed by sending it to the relevant client.
    /// 
    ///     A storage controller executes within the process for the disk drivers it is managing. It creates a separate thread for each
    ///     disk to act as the command processing thread for that disk. It also creates a separate thread for each client to act as the
    ///     command processing thread for that client. It uses simple semaphore locking to ensure no two clients can attempt to access a 
    ///     single disk's command queue simultaneously. 
    /// 
    ///     Due to limitations in the FlingOS threading system, the storage controller class is currently a static class meaning it 
    ///     essentially exists once per process. 
    /// 
    ///     TODO: Once FlingOS supports thread start arguments, this should become an instance class.
    /// </remarks>
    public static class StorageController
    {
        /// <summary>
        ///     The commands that can be sent to a disk.
        /// </summary>
        private enum DiskCommands
        {
            Invalid = -1,
            None = 0,
            Read,
            Write,
            CleanCaches
        }

        /// <summary>
        ///     The list of disks being managed by the storage controller.
        /// </summary>
        private static List DiskList;
        /// <summary>
        ///     The Id of the semaphore used to control exclusive read-modify access to the disk list.
        /// </summary>
        private static int DiskListSemaphoreId;

        /// <summary>
        ///     Whether the controller has been initialised or not.
        /// </summary>
        private static bool Initialised;
        /// <summary>
        ///     Whether the controller is terminating or not. 
        /// </summary>
        /// <remarks>
        ///     If the controller is terminating, all worker threads (for disks and clients) should
        ///     also terminate. In practice, this will not work yet because of the use of blocking
        ///     calls to semaphores and pipes.
        /// </remarks>
        private static bool Terminating;

        /// <summary>
        ///     The list of clients connected to the storage controller.
        /// </summary>
        private static List Clients;
        /// <summary>
        ///     The Id of the thread which is listening for client connections.
        /// </summary>
        /// <remarks>
        ///     The listening thread is a single thread listening on a single outpoint. When a client attempts to
        ///     connect, a new pipe is created and the system wakes the listener thread, passing it the information
        ///     about the new pipe. The listener thread then creates and starts a new client worker thread and then
        ///     returns to waiting for clients. The upshot of this, is that if two independent processes attempted 
        ///     to connect simultaneously, one of them would be missed. This is essentially a flaw in the FlingOS
        ///     pipes design that will need to be rectified in future.
        /// </remarks>
        private static uint WaitForClientsThreadId;
        /// <summary>
        ///     The data outpoint to which clients can connect and through which, data read from disks is sent to 
        ///     clients.
        /// </summary>
        private static StorageDataOutpoint DataOutpoint;

        /// <summary>
        ///     Intialises the storage controller.
        /// </summary>
        public static void Init()
        {
            if (!Initialised)
            {
                Initialised = true;
                DiskList = new List(5);
                Terminating = false;
                Clients = new List(5);
                DataOutpoint = new StorageDataOutpoint(PipeConstants.UnlimitedConnections, true);

                if (SystemCalls.CreateSemaphore(1, out DiskListSemaphoreId) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Storage Controller > Failed to create a semaphore! (1)");
                    ExceptionMethods.Throw(new NullReferenceException());
                }

                if (SystemCalls.StartThread(WaitForClients, out WaitForClientsThreadId) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Storage Controller > Failed to create client listener thread!");
                }
            }
        }

        /// <summary>
        ///     Worker thread that waits for clients and start a client manager thread (see <see cref="ManageClient"/>) when a client
        ///     connects.
        /// </summary>
        /// <seealso cref="ManageClient"/>
        private static unsafe void WaitForClients()
        {
            while (!Terminating)
            {
                BasicConsole.WriteLine("Storage Controller > Waiting for client to connect...");

                uint InProcessId;
                int PipeId = DataOutpoint.WaitForConnect(out InProcessId);
                BasicConsole.WriteLine("Storage Controller > Client connected.");
                
                ClientInfo NewInfo = new ClientInfo
                {
                    InProcessId = InProcessId,
                    DataOutPipeId = PipeId
                };

                Clients.Add(NewInfo);
                
                uint NewThreadId;
                if (SystemCalls.StartThread(ObjectUtilities.GetHandle((ManageClientDelegate)ManageClient), out NewThreadId, new uint[] { (uint)ObjectUtilities.GetHandle(NewInfo) }) == SystemCallResults.OK)
                {
                    NewInfo.ManagementThreadId = NewThreadId;
                }
                else
                {
                    BasicConsole.WriteLine("Storage Controller > Failed to create client management thread!");
                }
            }
        }


        private delegate void ManageClientDelegate(ClientInfo TheClient, uint CS);
        /// <summary>
        ///     Main method for client management worker threads.
        /// </summary>
        private static void ManageClient(ClientInfo TheClient, uint CS)
        {
            BasicConsole.WriteLine("Storage Controller > Client manager started.");

            try
            {
                BasicConsole.WriteLine("Storage Controller > Connecting command pipe...");
                StorageCmdInpoint CmdInpoint = new StorageCmdInpoint(TheClient.InProcessId);
                BasicConsole.WriteLine("Storage Controller > Command pipe connected.");
                try
                {
                    BasicConsole.WriteLine("Storage Controller > Connecting data pipe...");
                    StorageDataInpoint DataInpoint = new StorageDataInpoint(TheClient.InProcessId, false);
                    BasicConsole.WriteLine("Storage Controller > Data pipe connected.");
                    while (!Terminating)
                    {
                        try
                        {
                            unsafe
                            {
                                //BasicConsole.WriteLine("Storage Controller > Wait for command from client...");
                                StoragePipeCommand* CommandPtr = CmdInpoint.Read();
                                //BasicConsole.WriteLine("Storage Controller > Command received from client: " + (String)CommandPtr->Command);

                                switch ((StorageCommands)CommandPtr->Command)
                                {
                                    case StorageCommands.DiskList:
                                    {
                                        if (SystemCalls.WaitSemaphore(DiskListSemaphoreId) == SystemCallResults.OK)
                                        {
                                            ulong[] DiskIds = new ulong[DiskList.Count];

                                            for (int i = 0; i < DiskList.Count; i++)
                                            {
                                                DiskIds[i] = ((DiskInfo)DiskList[i]).TheDevice.Id;
                                            }

                                            SystemCalls.SignalSemaphore(DiskListSemaphoreId);

                                            DataOutpoint.WriteDiskInfos(TheClient.DataOutPipeId, DiskIds);
                                        }
                                    }
                                        break;
                                    case StorageCommands.Read:
                                    {
                                        //BasicConsole.WriteLine("Storage controller > Issuing read (disk cmd) " + (String)CommandPtr->BlockCount + " blocks from " + (String)CommandPtr->BlockNo + " blocks offset.");

                                        if (SystemCalls.WaitSemaphore(DiskListSemaphoreId) == SystemCallResults.OK)
                                        {
                                            DiskInfo TheDiskInfo = null;

                                            for (int i = 0; i < DiskList.Count; i++)
                                            {
                                                DiskInfo ADiskInfo = (DiskInfo)DiskList[i];
                                                if (ADiskInfo.TheDevice.Id == CommandPtr->DiskId)
                                                {
                                                    TheDiskInfo = ADiskInfo;
                                                    break;
                                                }
                                            }

                                            SystemCalls.SignalSemaphore(DiskListSemaphoreId);

                                            if (TheDiskInfo != null)
                                            {
                                                DiskCommand NewCommand = new DiskCommand
                                                {
                                                    Command = DiskCommands.Read,
                                                    Blocks = CommandPtr->BlockCount,
                                                    BlockNo = CommandPtr->BlockNo,
                                                    DataOutPipeId = TheClient.DataOutPipeId
                                                };

                                                int NewSemaphoreId;
                                                if (SystemCalls.CreateSemaphore(-1, out NewSemaphoreId) !=
                                                    SystemCallResults.OK)
                                                {
                                                    BasicConsole.WriteLine(
                                                        "Storage Controller > Failed to create a semaphore for read command!");
                                                    ExceptionMethods.Throw(new NullReferenceException());
                                                }
                                                NewCommand.CompletedSemaphoreId = NewSemaphoreId;

                                                if (
                                                    SystemCalls.WaitSemaphore(
                                                        TheDiskInfo.CommandQueueAccessSemaphoreId) ==
                                                    SystemCallResults.OK)
                                                {
                                                    TheDiskInfo.CommandQueue.Push(NewCommand);

                                                    SystemCalls.SignalSemaphore(
                                                        TheDiskInfo.CommandQueueAccessSemaphoreId);
                                                    SystemCalls.SignalSemaphore(TheDiskInfo.CommandQueuedSemaphoreId);

                                                    SystemCalls.WaitSemaphore(NewCommand.CompletedSemaphoreId);
                                                }

                                                SystemCalls.ReleaseSemaphore(NewCommand.CompletedSemaphoreId);
                                            }
                                        }
                                    }
                                        break;
                                    case StorageCommands.Write:
                                    {
                                        if (SystemCalls.WaitSemaphore(DiskListSemaphoreId) == SystemCallResults.OK)
                                        {
                                            DiskInfo TheDiskInfo = null;

                                            for (int i = 0; i < DiskList.Count; i++)
                                            {
                                                DiskInfo ADiskInfo = (DiskInfo)DiskList[i];
                                                if (ADiskInfo.TheDevice.Id == CommandPtr->DiskId)
                                                {
                                                    TheDiskInfo = ADiskInfo;
                                                    break;
                                                }
                                            }

                                            SystemCalls.SignalSemaphore(DiskListSemaphoreId);

                                            if (TheDiskInfo != null)
                                            {
                                                DiskCommand NewCommand = new DiskCommand
                                                {
                                                    Command = DiskCommands.Write,
                                                    Blocks = CommandPtr->BlockCount,
                                                    BlockNo = CommandPtr->BlockNo,
                                                    DataInPipe = DataInpoint
                                                };

                                                int NewSemaphoreId;
                                                if (SystemCalls.CreateSemaphore(-1, out NewSemaphoreId) !=
                                                    SystemCallResults.OK)
                                                {
                                                    BasicConsole.WriteLine(
                                                        "Storage Controller > Failed to create a semaphore for write command!");
                                                    ExceptionMethods.Throw(new NullReferenceException());
                                                }
                                                NewCommand.CompletedSemaphoreId = NewSemaphoreId;

                                                if (
                                                    SystemCalls.WaitSemaphore(
                                                        TheDiskInfo.CommandQueueAccessSemaphoreId) ==
                                                    SystemCallResults.OK)
                                                {
                                                    TheDiskInfo.CommandQueue.Push(NewCommand);

                                                    SystemCalls.SignalSemaphore(
                                                        TheDiskInfo.CommandQueueAccessSemaphoreId);
                                                    SystemCalls.SignalSemaphore(TheDiskInfo.CommandQueuedSemaphoreId);

                                                    SystemCalls.WaitSemaphore(NewCommand.CompletedSemaphoreId);
                                                }

                                                SystemCalls.ReleaseSemaphore(NewCommand.CompletedSemaphoreId);
                                            }
                                        }
                                    }
                                        break;
                                    case StorageCommands.BlockSize:
                                    {
                                        if (SystemCalls.WaitSemaphore(DiskListSemaphoreId) == SystemCallResults.OK)
                                        {
                                            DiskInfo TheDiskInfo = null;

                                            for (int i = 0; i < DiskList.Count; i++)
                                            {
                                                DiskInfo ADiskInfo = (DiskInfo)DiskList[i];
                                                if (ADiskInfo.TheDevice.Id == CommandPtr->DiskId)
                                                {
                                                    TheDiskInfo = ADiskInfo;
                                                    break;
                                                }
                                            }

                                            SystemCalls.SignalSemaphore(DiskListSemaphoreId);

                                            if (TheDiskInfo != null)
                                            {
                                                byte[] data = new byte[8];
                                                ulong BlockSize = TheDiskInfo.TheDevice.BlockSize;
                                                data[0] = (byte)(BlockSize >> 0);
                                                data[1] = (byte)(BlockSize >> 8);
                                                data[2] = (byte)(BlockSize >> 16);
                                                data[3] = (byte)(BlockSize >> 24);
                                                data[4] = (byte)(BlockSize >> 32);
                                                data[5] = (byte)(BlockSize >> 40);
                                                data[6] = (byte)(BlockSize >> 48);
                                                data[7] = (byte)(BlockSize >> 56);
                                                DataOutpoint.Write(TheClient.DataOutPipeId, data, 0, 8, true);
                                            }
                                        }
                                    }
                                        break;
                                    case StorageCommands.CleanCaches:
                                    {
                                        if (SystemCalls.WaitSemaphore(DiskListSemaphoreId) == SystemCallResults.OK)
                                        {
                                            DiskInfo TheDiskInfo = null;

                                            for (int i = 0; i < DiskList.Count; i++)
                                            {
                                                DiskInfo ADiskInfo = (DiskInfo)DiskList[i];
                                                if (ADiskInfo.TheDevice.Id == CommandPtr->DiskId)
                                                {
                                                    TheDiskInfo = ADiskInfo;
                                                    break;
                                                }
                                            }

                                            SystemCalls.SignalSemaphore(DiskListSemaphoreId);

                                            if (TheDiskInfo != null)
                                            {
                                                DiskCommand NewCommand = new DiskCommand
                                                {
                                                    Command = DiskCommands.CleanCaches
                                                };

                                                int NewSemaphoreId;
                                                if (SystemCalls.CreateSemaphore(-1, out NewSemaphoreId) !=
                                                    SystemCallResults.OK)
                                                {
                                                    BasicConsole.WriteLine(
                                                        "Storage Controller > Failed to create a semaphore for clean caches command!");
                                                    ExceptionMethods.Throw(new NullReferenceException());
                                                }
                                                NewCommand.CompletedSemaphoreId = NewSemaphoreId;

                                                if (
                                                    SystemCalls.WaitSemaphore(
                                                        TheDiskInfo.CommandQueueAccessSemaphoreId) ==
                                                    SystemCallResults.OK)
                                                {
                                                    TheDiskInfo.CommandQueue.Push(NewCommand);

                                                    SystemCalls.SignalSemaphore(
                                                        TheDiskInfo.CommandQueueAccessSemaphoreId);
                                                    SystemCalls.SignalSemaphore(TheDiskInfo.CommandQueuedSemaphoreId);

                                                    SystemCalls.WaitSemaphore(NewCommand.CompletedSemaphoreId);
                                                }

                                                SystemCalls.ReleaseSemaphore(NewCommand.CompletedSemaphoreId);
                                            }
                                        }
                                    }
                                        break;
                                    default:
                                        if (SystemCalls.WaitSemaphore(DiskListSemaphoreId) == SystemCallResults.OK)
                                        {
                                            DiskInfo TheInfo = (DiskInfo)DiskList[0];

                                            SystemCalls.SignalSemaphore(DiskListSemaphoreId);

                                            if (SystemCalls.WaitSemaphore(TheInfo.CommandQueueAccessSemaphoreId) ==
                                                SystemCallResults.OK)
                                            {
                                                TheInfo.CommandQueue.Push(new DiskCommand
                                                {
                                                    Command = DiskCommands.Invalid
                                                });

                                                SystemCalls.SignalSemaphore(TheInfo.CommandQueueAccessSemaphoreId);
                                                SystemCalls.SignalSemaphore(TheInfo.CommandQueuedSemaphoreId);
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        catch
                        {
                            BasicConsole.WriteLine(
                                "Storage Controller > An error occurred while processing a command for a client!");
                            BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                        }
                    }
                }
                catch
                {
                    BasicConsole.WriteLine(
                        "Storage Controller > An error occurred trying to connect a data pipe from a storage controller!");
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }
            }
            catch
            {
                BasicConsole.WriteLine(
                    "Storage Controller > An error occurred trying to connect a command pipe from a storage controller!");
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
            }
        }

        /// <summary>
        ///     Adds the specified disk device to the list of disks being managed by the controller and
        ///     starts a new management thread for it (see <see cref="ManageDisk"/>).
        /// </summary>
        /// <param name="TheDisk">The disk to manage.</param>
        /// <seealso cref="ManageDisk"/>
        public static unsafe void AddDisk(DiskDevice TheDisk)
        {
            if (SystemCalls.WaitSemaphore(DiskListSemaphoreId) == SystemCallResults.OK)
            {
                DiskInfo NewInfo = new DiskInfo
                {
                    TheDevice = TheDisk,
                    CommandQueue = new Queue(5, true)
                };

                int NewSemaphoreId;
                if (SystemCalls.CreateSemaphore(1, out NewSemaphoreId) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Storage Controller > Failed to create a semaphore! (AD 1)");
                    ExceptionMethods.Throw(new NullReferenceException());
                }
                NewInfo.CommandQueueAccessSemaphoreId = NewSemaphoreId;

                if (SystemCalls.CreateSemaphore(-1, out NewSemaphoreId) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Storage Controller > Failed to create a semaphore! (AD 2)");
                    ExceptionMethods.Throw(new NullReferenceException());
                }
                NewInfo.CommandQueuedSemaphoreId = NewSemaphoreId;

                DiskList.Add(NewInfo);

                uint NewThreadId;
                if (SystemCalls.StartThread(ObjectUtilities.GetHandle((ManageDiskDelegate)ManageDisk), out NewThreadId, new uint[] { (uint)ObjectUtilities.GetHandle(NewInfo) }) == SystemCallResults.OK)
                {
                    NewInfo.ManagementThreadId = NewThreadId;
                }
                else
                {
                    BasicConsole.WriteLine("Storage Controller > Failed to create disk management thread!");
                }

                if (SystemCalls.SignalSemaphore(DiskListSemaphoreId) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Storage Controller > Failed to signal a semaphore! (AD 3)");
                }
            }
            else
            {
                BasicConsole.WriteLine("Storage Controller > Failed to wait on a semaphore! (AD 4)");
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="TheDisk"></param>
        /// <exception cref="NotImplementedException">This method has not been implemented yet.</exception>
        public static void RemoveDisk(DiskDevice TheDisk) => ExceptionMethods.Throw(new NotImplementedException("Remove a disk from a storage controller has not been implemented yet."));
        
        /// <summary>
        ///     Terminates the storage controller, including all worker threads, closes all client connections and relinquishes control of the disks. 
        /// </summary>
        /// <exception cref="NotImplementedException">This method has not been implemented yet.</exception>
        public static void Terminate() => ExceptionMethods.Throw(new NotImplementedException("Termination of a storage controller has not been implemented yet."));

        private delegate void ManageDiskDelegate(DiskInfo TheInfo, uint CS);
        /// <summary>
        ///     Main method for disk management worker threads.
        /// </summary>
        private static void ManageDisk(DiskInfo TheInfo, uint CS)
        {
            BasicConsole.WriteLine("Storage Controller > Disk manager started.");

            while (!Terminating)
            {
                try
                {
                    if (SystemCalls.WaitSemaphore(TheInfo.CommandQueuedSemaphoreId) == SystemCallResults.OK)
                    {
                        DiskCommand ACommand = null;
                        if (SystemCalls.WaitSemaphore(TheInfo.CommandQueueAccessSemaphoreId) == SystemCallResults.OK)
                        {
                            ACommand = (DiskCommand)TheInfo.CommandQueue.Pop();
                            SystemCalls.SignalSemaphore(TheInfo.CommandQueueAccessSemaphoreId);

                            //BasicConsole.WriteLine("Storage controller > Disk command received: " + (String)(uint)ACommand.Command);

                            switch (ACommand.Command)
                            {
                                case DiskCommands.Read:
                                {
                                    //BasicConsole.WriteLine("Storage controller > Reading " + (String)ACommand.BlockCount + " blocks from " + (String)ACommand.BlockNo + " blocks offset.");
                                    //BasicConsole.WriteLine("Storage controller > Disk block size: " + (Framework.String)TheInfo.TheDevice.BlockSize);
                                    byte[] data = TheInfo.TheDevice.NewBlockArray(ACommand.Blocks);
                                    TheInfo.TheDevice.ReadBlock(ACommand.BlockNo, ACommand.Blocks, data);
                                    //BasicConsole.WriteLine("Data read: ");
                                    //for (int i = 0; i < data.Length; i++)
                                    //{
                                    //    BasicConsole.Write(data[i]);
                                    //    BasicConsole.Write(" ");
                                    //}
                                    //BasicConsole.WriteLine("---------------------------------------------");
                                    DataOutpoint.Write(ACommand.DataOutPipeId, data, 0, data.Length, true);
                                }
                                    break;
                                case DiskCommands.Write:
                                {
                                    BasicConsole.WriteLine("Storage controller > Writing " +
                                                            (String)ACommand.Blocks + " blocks from " +
                                                            ACommand.BlockNo + " blocks offset.");
                                    byte[] data = TheInfo.TheDevice.NewBlockArray(ACommand.Blocks);
                                    int BytesRead = ACommand.DataInPipe.Read(data, 0, data.Length, true);
                                    int DesiredBytesRead =
                                        (int)(ACommand.Blocks*(uint)TheInfo.TheDevice.BlockSize);
                                    if (BytesRead < DesiredBytesRead)
                                    {
                                        BasicConsole.WriteLine(
                                            "Storage controller > Error! Virtual disk controller (Storage Controller Disk) did not provide enough data for Write command.");
                                    }
                                    TheInfo.TheDevice.WriteBlock(ACommand.BlockNo, ACommand.Blocks, data);
                                }
                                    break;
                                case DiskCommands.CleanCaches:
                                    BasicConsole.WriteLine("Storage controller > Cleaning caches.");
                                    TheInfo.TheDevice.CleanCaches();
                                    break;
                            }

                            SystemCalls.SignalSemaphore(ACommand.CompletedSemaphoreId);
                        }
                    }
                }
                catch
                {
                    BasicConsole.WriteLine("Storage Controller > An error occurred while processing!");
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }
            }
        }

        //TODO: Release of semaphores
        //TODO: Termination of drivers
        //TODO: Removal of devices

        private class ClientInfo : Object
        {
            public int DataOutPipeId;
            public uint InProcessId;
            public uint ManagementThreadId;
        }

        private class DiskInfo : Object
        {
            public Queue CommandQueue;
            public int CommandQueueAccessSemaphoreId;
            public int CommandQueuedSemaphoreId;
            public uint ManagementThreadId;
            public DiskDevice TheDevice;
        }

        private class DiskCommand : Object
        {
            public uint Blocks;
            public ulong BlockNo;
            public DiskCommands Command;
            public int CompletedSemaphoreId;
            public StorageDataInpoint DataInPipe;
            public int DataOutPipeId;
        }
    }
}