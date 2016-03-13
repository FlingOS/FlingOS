using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.Processes;
using Kernel.FOS_System.Processes.Requests.Pipes;
using Kernel.Pipes.Storage;
using Kernel.Hardware.Devices;

namespace Kernel.Hardware.Controllers
{
    public static class StorageController
    {
        private class ClientInfo : FOS_System.Object
        {
            public uint InProcessId;
            public int PipeId;
            public uint ManagementThreadId;
        }
        private class DiskInfo : Object
        {
            public DiskDevice TheDevice;
            public uint ManagementThreadId;
            public Queue CommandQueue;
            public int CommandQueueAccessSemaphoreId;
            public int CommandQueuedSemaphoreId;
        }
        private enum DiskCommands
        {
            Invalid = -1,
            None = 0
        }
        private class DiskCommand : Object
        {
            public DiskCommands Command;
        }

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
                DataOutpoint = new StorageDataOutpoint(PipeConstants.UnlimitedConnections);

                if (SystemCalls.CreateSemaphore(1, out DiskListSemaphoreId) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Storage Controller > Failed to create a semaphore! (1)");
                    ExceptionMethods.Throw(new FOS_System.Exceptions.NullReferenceException());
                }

                if (SystemCalls.CreateSemaphore(1, out ClientListSemaphoreId) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Storage Controller > Failed to create a semaphore! (2)");
                    ExceptionMethods.Throw(new FOS_System.Exceptions.NullReferenceException());
                }

                if (SystemCalls.StartThread(WaitForClients, out WaitForClientsThreadId) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Storage Controller > Failed to create data pipe listener thread!");
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
                //TODO: Proper handling
                BasicConsole.WriteLine("Storage Controller > Client connected.");
                
                if (SystemCalls.WaitSemaphore(ClientListSemaphoreId) == SystemCallResults.OK)
                {
                    ClientInfo NewInfo = new ClientInfo()
                    {
                        InProcessId = InProcessId,
                        PipeId = PipeId
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
                TheClient = (ClientInfo)Clients[Clients.Count - 1];

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
                        StorageDataInpoint DataInpoint = new StorageDataInpoint(TheClient.InProcessId);
                        BasicConsole.WriteLine("Storage Controller > Data pipe connected.");
                        while (!Terminating)
                        {
                            try
                            {
                                //TODO: Receive & respond to commands
                                unsafe
                                {
                                    BasicConsole.WriteLine("Storage Controller > Wait for command from client...");
                                    StoragePipeCommand* CommandPtr = CmdInpoint.Read();
                                    BasicConsole.WriteLine("Storage Controller > Command received from client: " + (String)CommandPtr->Command);
                                }


                                if (SystemCalls.WaitSemaphore(DiskListSemaphoreId) == SystemCallResults.OK)
                                {
                                    DiskInfo TheInfo = (DiskInfo)DiskList[0];

                                    SystemCalls.SignalSemaphore(DiskListSemaphoreId);

                                    if (SystemCalls.WaitSemaphore(TheInfo.CommandQueueAccessSemaphoreId) == SystemCallResults.OK)
                                    {
                                        TheInfo.CommandQueue.Push(new DiskCommand()
                                        {
                                            Command = DiskCommands.Invalid
                                        });

                                        SystemCalls.SignalSemaphore(TheInfo.CommandQueueAccessSemaphoreId);
                                        SystemCalls.SignalSemaphore(TheInfo.CommandQueuedSemaphoreId);
                                    }
                                }
                            }
                            catch
                            {
                                BasicConsole.WriteLine("Storage Controller > An error occurred while processing a command for a client!");
                                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                            }
                        }
                    }
                    catch
                    {
                        BasicConsole.WriteLine("Storage Controller > An error occurred trying to connect a data pipe from a storage controller!");
                        BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                    }
                }
                catch
                {
                    BasicConsole.WriteLine("Storage Controller > An error occurred trying to connect a command pipe from a storage controller!");
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
                    ExceptionMethods.Throw(new FOS_System.Exceptions.NullReferenceException());
                }
                NewInfo.CommandQueueAccessSemaphoreId = NewSemaphoreId;
                
                if (SystemCalls.CreateSemaphore(0, out NewSemaphoreId) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Storage Controller > Failed to create a semaphore! (AD 2)");
                    ExceptionMethods.Throw(new FOS_System.Exceptions.NullReferenceException());
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
                DiskInfo TheInfo = (DiskInfo)DiskList[DiskList.Count - 1];

                if (SystemCalls.SignalSemaphore(DiskListSemaphoreId) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Storage Controller failed to signal a semaphore! (MD 1)");
                }

                BasicConsole.WriteLine("Storage Controller > Disk manager started.");

                while (!Terminating)
                {
                    try
                    {
                        //TODO: Receive & respond to commands

                        if (SystemCalls.WaitSemaphore(TheInfo.CommandQueuedSemaphoreId) == SystemCallResults.OK)
                        {
                            DiskCommand ACommand = null;
                            if (SystemCalls.WaitSemaphore(TheInfo.CommandQueueAccessSemaphoreId) == SystemCallResults.OK)
                            {
                                ACommand = (DiskCommand)TheInfo.CommandQueue.Pop();
                                SystemCalls.SignalSemaphore(TheInfo.CommandQueueAccessSemaphoreId);
                            }

                            BasicConsole.WriteLine("Storage controller > Disk command received: " + (String)(uint)ACommand.Command);
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
    }
}
