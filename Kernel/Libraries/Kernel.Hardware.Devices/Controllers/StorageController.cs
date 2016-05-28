using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.Exceptions;
using Kernel.FOS_System.Processes;
using Kernel.FOS_System.Processes.Requests.Pipes;
using Kernel.Hardware.Devices;
using Kernel.Pipes.Storage;

namespace Kernel.Hardware.Controllers
{
    public static class StorageController
    {
        private static List DiskList;
        private static int DiskListSemaphoreId;
        private static bool Initialised = false;

        private static bool Terminating;

        private static List Clients;
        private static int ClientListSemaphoreId;
        private static uint WaitForClientsThreadId;
        private static StorageDataOutpoint DataOutpoint;

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

                if (SystemCalls.CreateSemaphore(1, out ClientListSemaphoreId) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Storage Controller > Failed to create a semaphore! (2)");
                    ExceptionMethods.Throw(new NullReferenceException());
                }

                if (SystemCalls.StartThread(WaitForClients, out WaitForClientsThreadId) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Storage Controller > Failed to create client listener thread!");
                }
            }
        }

        private static void WaitForClients()
        {
            while (!Terminating)
            {
                BasicConsole.WriteLine("Storage Controller > Waiting for client to connect...");

                uint InProcessId;
                int PipeId = DataOutpoint.WaitForConnect(out InProcessId);
                BasicConsole.WriteLine("Storage Controller > Client connected.");

                if (SystemCalls.WaitSemaphore(ClientListSemaphoreId) == SystemCallResults.OK)
                {
                    ClientInfo NewInfo = new ClientInfo()
                    {
                        InProcessId = InProcessId,
                        DataOutPipeId = PipeId
                    };

                    Clients.Add(NewInfo);

                    uint NewThreadId;
                    if (SystemCalls.StartThread(ManageClient, out NewThreadId) == SystemCallResults.OK)
                    {
                        NewInfo.ManagementThreadId = NewThreadId;
                    }
                    else
                    {
                        BasicConsole.WriteLine("Storage Controller > Failed to create client management thread!");
                    }

                    if (SystemCalls.SignalSemaphore(ClientListSemaphoreId) != SystemCallResults.OK)
                    {
                        BasicConsole.WriteLine("Storage Controller > Failed to signal a semaphore! (WFC 1)");
                    }
                }
                else
                {
                    BasicConsole.WriteLine("Storage Controller > Failed to wait on a semaphore! (WFC 2)");
                }
            }
        }

        private static void ManageClient()
        {
            ClientInfo TheClient = null;
            if (SystemCalls.WaitSemaphore(ClientListSemaphoreId) == SystemCallResults.OK)
            {
                TheClient = (ClientInfo) Clients[Clients.Count - 1];

                if (SystemCalls.SignalSemaphore(ClientListSemaphoreId) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Storage Controller > Failed to signal a semaphore! (MC 1)");
                }

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

                                    switch ((StorageCommands) CommandPtr->Command)
                                    {
                                        case StorageCommands.DiskList:
                                        {
                                            if (SystemCalls.WaitSemaphore(DiskListSemaphoreId) == SystemCallResults.OK)
                                            {
                                                ulong[] DiskIds = new ulong[DiskList.Count];

                                                for (int i = 0; i < DiskList.Count; i++)
                                                {
                                                    DiskIds[i] = ((DiskInfo) DiskList[i]).TheDevice.Id;
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
                                                    DiskInfo ADiskInfo = (DiskInfo) DiskList[i];
                                                    if (ADiskInfo.TheDevice.Id == CommandPtr->DiskId)
                                                    {
                                                        TheDiskInfo = ADiskInfo;
                                                        break;
                                                    }
                                                }

                                                SystemCalls.SignalSemaphore(DiskListSemaphoreId);

                                                if (TheDiskInfo != null)
                                                {
                                                    DiskCommand NewCommand = new DiskCommand()
                                                    {
                                                        Command = DiskCommands.Read,
                                                        BlockCount = CommandPtr->BlockCount,
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
                                                    DiskInfo ADiskInfo = (DiskInfo) DiskList[i];
                                                    if (ADiskInfo.TheDevice.Id == CommandPtr->DiskId)
                                                    {
                                                        TheDiskInfo = ADiskInfo;
                                                        break;
                                                    }
                                                }

                                                SystemCalls.SignalSemaphore(DiskListSemaphoreId);

                                                if (TheDiskInfo != null)
                                                {
                                                    DiskCommand NewCommand = new DiskCommand()
                                                    {
                                                        Command = DiskCommands.Write,
                                                        BlockCount = CommandPtr->BlockCount,
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
                                                    DiskInfo ADiskInfo = (DiskInfo) DiskList[i];
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
                                                    data[0] = (byte) (BlockSize >> 0);
                                                    data[1] = (byte) (BlockSize >> 8);
                                                    data[2] = (byte) (BlockSize >> 16);
                                                    data[3] = (byte) (BlockSize >> 24);
                                                    data[4] = (byte) (BlockSize >> 32);
                                                    data[5] = (byte) (BlockSize >> 40);
                                                    data[6] = (byte) (BlockSize >> 48);
                                                    data[7] = (byte) (BlockSize >> 56);
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
                                                    DiskInfo ADiskInfo = (DiskInfo) DiskList[i];
                                                    if (ADiskInfo.TheDevice.Id == CommandPtr->DiskId)
                                                    {
                                                        TheDiskInfo = ADiskInfo;
                                                        break;
                                                    }
                                                }

                                                SystemCalls.SignalSemaphore(DiskListSemaphoreId);

                                                if (TheDiskInfo != null)
                                                {
                                                    DiskCommand NewCommand = new DiskCommand()
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
                                                DiskInfo TheInfo = (DiskInfo) DiskList[0];

                                                SystemCalls.SignalSemaphore(DiskListSemaphoreId);

                                                if (SystemCalls.WaitSemaphore(TheInfo.CommandQueueAccessSemaphoreId) ==
                                                    SystemCallResults.OK)
                                                {
                                                    TheInfo.CommandQueue.Push(new DiskCommand()
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
            else
            {
                BasicConsole.WriteLine("Storage Controller > Failed to wait on a semaphore! (MC 2)");
            }
        }

        public static void AddDisk(DiskDevice TheDisk)
        {
            if (SystemCalls.WaitSemaphore(DiskListSemaphoreId) == SystemCallResults.OK)
            {
                DiskInfo NewInfo = new DiskInfo()
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
                if (SystemCalls.StartThread(ManageDisk, out NewThreadId) == SystemCallResults.OK)
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

        public static void RemoveDisk(DiskDevice TheDisk)
        {
            //TODO: Removal of disk from controller
        }

        public static void Terminate()
        {
            //TODO: Termination of controller
        }

        private static void ManageDisk()
        {
            if (SystemCalls.WaitSemaphore(DiskListSemaphoreId) == SystemCallResults.OK)
            {
                DiskInfo TheInfo = (DiskInfo) DiskList[DiskList.Count - 1];

                if (SystemCalls.SignalSemaphore(DiskListSemaphoreId) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Storage Controller failed to signal a semaphore! (MD 1)");
                }

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
                                ACommand = (DiskCommand) TheInfo.CommandQueue.Pop();
                                SystemCalls.SignalSemaphore(TheInfo.CommandQueueAccessSemaphoreId);

                                //BasicConsole.WriteLine("Storage controller > Disk command received: " + (String)(uint)ACommand.Command);

                                switch (ACommand.Command)
                                {
                                    case DiskCommands.Read:
                                    {
                                        //BasicConsole.WriteLine("Storage controller > Reading " + (String)ACommand.BlockCount + " blocks from " + (String)ACommand.BlockNo + " blocks offset.");
                                        //BasicConsole.WriteLine("Storage controller > Disk block size: " + (FOS_System.String)TheInfo.TheDevice.BlockSize);
                                        byte[] data = TheInfo.TheDevice.NewBlockArray(ACommand.BlockCount);
                                        TheInfo.TheDevice.ReadBlock(ACommand.BlockNo, ACommand.BlockCount, data);
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
                                                               (String) ACommand.BlockCount + " blocks from " +
                                                               (String) ACommand.BlockNo + " blocks offset.");
                                        byte[] data = TheInfo.TheDevice.NewBlockArray(ACommand.BlockCount);
                                        int BytesRead = ACommand.DataInPipe.Read(data, 0, data.Length, true);
                                        int DesiredBytesRead =
                                            (int) (ACommand.BlockCount*(uint) TheInfo.TheDevice.BlockSize);
                                        if (BytesRead < DesiredBytesRead)
                                        {
                                            BasicConsole.WriteLine(
                                                "Storage controller > Error! Virtual disk controller (Storage Controller Disk) did not provide enough data for Write command.");
                                        }
                                        TheInfo.TheDevice.WriteBlock(ACommand.BlockNo, ACommand.BlockCount, data);
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
            else
            {
                BasicConsole.WriteLine("Storage Controller > Failed to wait on a semaphore! (2)");
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

        private enum DiskCommands
        {
            Invalid = -1,
            None = 0,
            Read,
            Write,
            CleanCaches
        }

        private class DiskCommand : Object
        {
            public uint BlockCount;
            public ulong BlockNo;
            public DiskCommands Command;
            public int CompletedSemaphoreId;
            public StorageDataInpoint DataInPipe;
            public int DataOutPipeId;
        }
    }
}