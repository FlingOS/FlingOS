using System;
using Kernel.FOS_System;
using Kernel.Core.Processes;

namespace Kernel.Core.Tasks
{
    public static unsafe class DeviceManagerTask
    {
        public static bool Terminating = false;

        public static void Main()
        {
            BasicConsole.WriteLine("Device Manager started.");

            Hardware.Processes.ProcessManager.CurrentProcess.InitHeap();
            SystemCallResults SysCallResult = SystemCallMethods.StartThread(GCCleanupTask.Main);
            if (SysCallResult != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Device Manager: GC thread failed to create!");
            }

            SysCallResult = SystemCallMethods.RegisterPipeOutpoint(Pipes.PipeClasses.Display, Pipes.PipeSubclasses.Display_Text_ASCII, 1);
            switch (SysCallResult)
            {
                case SystemCallResults.Unhandled:
                    BasicConsole.WriteLine("DM > RegisterPipeOutpoint: Unhandled!");
                    break;
                case SystemCallResults.Fail:
                    BasicConsole.WriteLine("DM > RegisterPipeOutpoint: Failed!");
                    break;
                case SystemCallResults.OK:
                    BasicConsole.WriteLine("DM > RegisterPipeOutpoint: Succeeded.");
                    break;
                default:
                    BasicConsole.WriteLine("DM > RegisterPipeOutpoint: Unexpected system call result!");
                    break;
            }

            int numOutpoints;
            SysCallResult = SystemCallMethods.GetNumPipeOutpoints(Pipes.PipeClasses.Display, Pipes.PipeSubclasses.Display_Text_ASCII, out numOutpoints);
            switch (SysCallResult)
            {
                case SystemCallResults.Unhandled:
                    BasicConsole.WriteLine("DM > GetNumPipeOutpoints: Unhandled!");
                    break;
                case SystemCallResults.Fail:
                    BasicConsole.WriteLine("DM > GetNumPipeOutpoints: Failed!");
                    break;
                case SystemCallResults.OK:
                    BasicConsole.WriteLine("DM > GetNumPipeOutpoints: Succeeded.");
                    break;
                default:
                    BasicConsole.WriteLine("DM > GetNumPipeOutpoints: Unexpected system call result!");
                    break;
            }
            BasicConsole.Write("DM > Num pipe outpoints of class: Display, subclass: Display_Text_ASCII = ");
            BasicConsole.WriteLine(numOutpoints);

            Pipes.PipeOutpointDescriptor WantedOutpoint;
            if (SysCallResult == SystemCallResults.OK && numOutpoints > 0)
            {
                Pipes.PipeOutpointsRequest* RequestPtr = (Pipes.PipeOutpointsRequest*)Heap.AllocZeroed((uint)sizeof(Pipes.PipeOutpointsRequest), "Device Manager : Alloc PipeOutpointsRequest");
                if (RequestPtr != null)
                {
                    try
                    {
                        RequestPtr->MaxDescriptors = numOutpoints;
                        RequestPtr->Outpoints = (Pipes.PipeOutpointDescriptor*)Heap.AllocZeroed((uint)numOutpoints * (uint)sizeof(Pipes.PipeOutpointDescriptor), "Device Manager : Alloc PipeOutpointDescriptor(s)");
                        if (RequestPtr->Outpoints != null)
                        {
                            try
                            {
                                SysCallResult = SystemCallMethods.GetPipeOutpoints(Pipes.PipeClasses.Display, Pipes.PipeSubclasses.Display_Text_ASCII, RequestPtr);
                                switch (SysCallResult)
                                {
                                    case SystemCallResults.Unhandled:
                                        BasicConsole.WriteLine("DM > GetPipeOutpoints: Unhandled!");
                                        break;
                                    case SystemCallResults.Fail:
                                        BasicConsole.WriteLine("DM > GetPipeOutpoints: Failed!");
                                        break;
                                    case SystemCallResults.OK:
                                        BasicConsole.WriteLine("DM > GetPipeOutpoints: Succeeded.");
                                        WantedOutpoint = RequestPtr->Outpoints[0];

                                        BasicConsole.Write("DM > Outpoint[0].ProcessId: ");
                                        BasicConsole.WriteLine(WantedOutpoint.ProcessId);
                                        break;
                                    default:
                                        BasicConsole.WriteLine("DM > GetPipeOutpoints: Unexpected system call result!");
                                        break;
                                }
                            }
                            finally
                            {
                                Heap.Free(RequestPtr->Outpoints);
                            }
                        }
                        else
                        {
                            BasicConsole.WriteLine("DM > RequestPtr->Outpoints null! No memory allocated.");
                        }
                    }
                    finally
                    {
                        Heap.Free(RequestPtr);
                    }
                }
                else
                {
                    BasicConsole.WriteLine("DM > RequestPtr null! No memory allocated.");
                }
            }
            else
            {
                BasicConsole.WriteLine("DM > Cannot get outpoints!");
            }

            while (!Terminating)
            {
                //TODO
                testclass x = new testclass();

                SystemCallMethods.SleepThread(SystemCallMethods.IndefiniteSleepThread);
            }
        }
    }

    class testclass : FOS_System.Object
    {
    }
}
