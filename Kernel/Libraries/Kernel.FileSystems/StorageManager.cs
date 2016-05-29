#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //

#endregion

using Kernel.Devices.Controllers;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Exceptions;
using Kernel.Framework.Processes;
using Kernel.Framework.Processes.Requests.Pipes;
using Kernel.Pipes;
using Kernel.Pipes.Storage;

namespace Kernel.FileSystems
{
    public class StorageManager : Object
    {
        private static StorageCmdOutpoint Strg_CmdOutpoint;
        private static StorageDataOutpoint Strg_DataOutpoint;

        private static int Strg_ConnectSemaphoreId;
        private static int Strg_CmdOutPipesSemaphoreId;
        private static int Strg_DataOutPipesSemaphoreId;
        private static UInt32List Strg_CmdOutPipes;
        private static UInt32List Strg_DataOutPipes;
        private static List Strg_DataInpoints;

        private static UInt64List DiskIdsBeingManaged;
        private static List StorageControllers;

        public static List StorageControllerDisks;

        public static bool Terminating;

        public static void Init()
        {
            Strg_DataInpoints = new List();
            Strg_CmdOutPipes = new UInt32List();
            Strg_DataOutPipes = new UInt32List();
            Strg_CmdOutpoint = new StorageCmdOutpoint(PipeConstants.UnlimitedConnections);
            Strg_DataOutpoint = new StorageDataOutpoint(PipeConstants.UnlimitedConnections, false);

            DiskIdsBeingManaged = new UInt64List();
            StorageControllers = new List();
            StorageControllerDisks = new List();

            Terminating = false;

            if (SystemCalls.CreateSemaphore(1, out Strg_ConnectSemaphoreId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Storage Manager > Failed to create a semaphore! (1)");
                ExceptionMethods.Throw(new NullReferenceException());
            }

            if (SystemCalls.CreateSemaphore(-1, out Strg_CmdOutPipesSemaphoreId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Storage Manager > Failed to create a semaphore! (2)");
                ExceptionMethods.Throw(new NullReferenceException());
            }

            if (SystemCalls.CreateSemaphore(-1, out Strg_DataOutPipesSemaphoreId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Storage Manager > Failed to create a semaphore! (3)");
                ExceptionMethods.Throw(new NullReferenceException());
            }

            uint NewThreadId;
            if (SystemCalls.StartThread(WaitForStrgCmdPipes, out NewThreadId) == SystemCallResults.OK)
            {
                //TODO: Store thread id
            }
            else
            {
                BasicConsole.WriteLine("Storage Manager > Failed to create command pipe listener thread!");
            }
            if (SystemCalls.StartThread(WaitForStrgDataPipes, out NewThreadId) == SystemCallResults.OK)
            {
                //TODO: Store thread id
            }
            else
            {
                BasicConsole.WriteLine("Storage Manager > Failed to create data pipe listener thread!");
            }
        }

        private static void WaitForStrgCmdPipes()
        {
            while (!Terminating)
            {
                uint InProcessId;
                int PipeId = Strg_CmdOutpoint.WaitForConnect(out InProcessId);
                BasicConsole.WriteLine("Storage Manager > Storage command output connected.");
                Strg_CmdOutPipes.Add((uint)PipeId);
                SystemCalls.SignalSemaphore(Strg_CmdOutPipesSemaphoreId);
            }
        }

        private static void WaitForStrgDataPipes()
        {
            while (!Terminating)
            {
                uint InProcessId;
                int PipeId = Strg_DataOutpoint.WaitForConnect(out InProcessId);
                BasicConsole.WriteLine("Storage Manager > Storage data output connected.");
                Strg_DataOutPipes.Add((uint)PipeId);
                SystemCalls.SignalSemaphore(Strg_DataOutPipesSemaphoreId);
            }
        }

        public static void InitStoragePipes()
        {
            int numOutpoints;
            SystemCallResults SysCallResult;
            BasicOutpoint.GetNumPipeOutpoints(out numOutpoints, out SysCallResult, PipeClasses.Storage,
                PipeSubclasses.Storage_Data_Out);

            if (SysCallResult == SystemCallResults.OK && numOutpoints > 0)
            {
                PipeOutpointDescriptor[] OutpointDescriptors;
                BasicOutpoint.GetOutpointDescriptors(numOutpoints, out SysCallResult, out OutpointDescriptors,
                    PipeClasses.Storage, PipeSubclasses.Storage_Data_Out);

                if (SysCallResult == SystemCallResults.OK)
                {
                    for (int i = 0; i < OutpointDescriptors.Length; i++)
                    {
                        PipeOutpointDescriptor Descriptor = OutpointDescriptors[i];
                        bool PipeExists = false;

                        for (int j = 0; j < Strg_DataInpoints.Count; j++)
                        {
                            StorageDataInpoint ExistingPipeInfo = (StorageDataInpoint)Strg_DataInpoints[j];
                            if (ExistingPipeInfo.OutProcessId == Descriptor.ProcessId)
                            {
                                PipeExists = true;
                                break;
                            }
                        }

                        if (!PipeExists)
                        {
                            try
                            {
                                if (SystemCalls.WaitSemaphore(Strg_ConnectSemaphoreId) == SystemCallResults.OK)
                                {
                                    BasicConsole.WriteLine("Storage Manager > Connecting to: " +
                                                           (String)Descriptor.ProcessId);
                                    StorageDataInpoint DataIn = new StorageDataInpoint(Descriptor.ProcessId, true);
                                    Strg_DataInpoints.Add(DataIn);

                                    BasicConsole.WriteLine("Storage Manager > Connected.");

                                    try
                                    {
                                        //TODO: Ought to store InProcessId (see WaitForStrgCmdPipes or WaitForStrgDataPipes)
                                        //  then check that against the Descriptor.ProcessId - wait until the correct process has connected
                                        if (SystemCalls.WaitSemaphore(Strg_CmdOutPipesSemaphoreId) ==
                                            SystemCallResults.OK)
                                        {
                                            int CmdPipeId = (int)Strg_CmdOutPipes[Strg_CmdOutPipes.Count - 1];

                                            BasicConsole.WriteLine("Storage Manager > Got command output pipe id.");

                                            //TODO: As above w.r.t. process ids
                                            if (SystemCalls.WaitSemaphore(Strg_DataOutPipesSemaphoreId) ==
                                                SystemCallResults.OK)
                                            {
                                                int DataPipeId = (int)Strg_DataOutPipes[Strg_DataOutPipes.Count - 1];

                                                BasicConsole.WriteLine("Storage Manager > Got data output pipe id.");

                                                StorageControllers.Add(new StorageControllerInfo
                                                {
                                                    RemoteProcessId = Descriptor.ProcessId,
                                                    CmdPipeId = CmdPipeId,
                                                    DataOutPipeId = DataPipeId,
                                                    DataInPipe = DataIn
                                                });
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        BasicConsole.WriteLine("Storage Manager > Error probing storage controller!");
                                        BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                                    }

                                    SystemCalls.SignalSemaphore(Strg_ConnectSemaphoreId);
                                }
                            }
                            catch
                            {
                                BasicConsole.WriteLine("Storage Manager > Error creating new pipe!");
                                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                            }
                        }
                    }
                }
                else
                {
                    BasicConsole.WriteLine("Storage Manager > Couldn't get outpoint descriptors!");
                }
            }
            else if (numOutpoints == 0)
            {
                BasicConsole.WriteLine("Storage Manager > Num outpoints is zero.");
            }
            else
            {
                BasicConsole.WriteLine("Storage Manager > Cannot get outpoints!");
            }
        }

        public static void InitControllers()
        {
            for (int i = 0; i < StorageControllers.Count; i++)
            {
                StorageControllerInfo st = (StorageControllerInfo)StorageControllers[i];

                Strg_CmdOutpoint.Send_DiskList(st.CmdPipeId);
                ulong[] DiskIds = st.DataInPipe.ReadDiskInfos(true);
                if (DiskIds.Length == 0)
                {
                    BasicConsole.WriteLine("Storage Manager > Storage controller is not managing any disks!");
                }
                else
                {
                    for (int j = 0; j < DiskIds.Length; j++)
                    {
                        if (!DiskIdsBeingManaged.ContainsItemInRange(DiskIds[j], DiskIds[j] + 1))
                        {
                            BasicConsole.WriteLine("Storage Manager > Storage controller is managing disk device: " +
                                                   (String)DiskIds[j]);

                            try
                            {
                                StorageControllerDisk disk = new StorageControllerDisk(DiskIds[j], st.RemoteProcessId,
                                    st.CmdPipeId, Strg_CmdOutpoint, st.DataOutPipeId, Strg_DataOutpoint, st.DataInPipe);

                                DiskIdsBeingManaged.Add(DiskIds[j]);
                                StorageControllerDisks.Add(disk);

                                PartitionManager.InitDisk(disk);
                            }
                            catch
                            {
                                BasicConsole.WriteLine("Storage Manager > Error initialising storage!");
                                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                            }
                        }
                    }
                }
            }
        }

        private class StorageControllerInfo : Object
        {
            public int CmdPipeId;
            public StorageDataInpoint DataInPipe;
            public int DataOutPipeId;
            public uint RemoteProcessId;
        }
    }
}