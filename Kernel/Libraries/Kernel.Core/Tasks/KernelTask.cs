using System;
using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.Core.Processes;
using Kernel.Hardware.Processes;
using Kernel.Hardware.VirtMem;

namespace Kernel.Core.Tasks
{
    public unsafe static class KernelTask
    {
        public static bool Terminating = false;
        private static CircularBuffer DeferredSyscallsInfo_Unqueued;
        private static CircularBuffer DeferredSyscallsInfo_Queued;

        private static Thread DeferredSyscallsThread;
        
        private class DeferredSyscallInfo : FOS_System.Object
        {
            public uint ProcessId; 
            public uint ThreadId;
        }

        public static void Main()
        {
            BasicConsole.WriteLine("Kernel task! ");
            BasicConsole.WriteLine(" > Executing normally...");

            DeferredSyscallsInfo_Unqueued = new CircularBuffer(256, false);
            DeferredSyscallsInfo_Queued = new CircularBuffer(DeferredSyscallsInfo_Unqueued.Size, false);
            for (int i = 0; i < DeferredSyscallsInfo_Unqueued.Size; i++)
            {
                DeferredSyscallsInfo_Unqueued.Push(new DeferredSyscallInfo());
            }

            try
            {
                BasicConsole.WriteLine(" > Initialising system calls...");
                Core.Processes.SystemCalls.Init();

                ProcessManager.CurrentProcess.SyscallHandler = SyscallHandler;
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.RegisterSyscallHandler);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.DeregisterSyscallHandler);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.StartThread);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.SleepThread);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.RegisterPipeOutpoint);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.GetNumPipeOutpoints);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.GetPipeOutpoints);
                
                //ProcessManager.CurrentProcess.OutputMemTrace = true;

                BasicConsole.WriteLine(" > Starting deferred syscalls thread...");
                DeferredSyscallsThread = ProcessManager.CurrentProcess.CreateThread(DeferredSyscallsThread_Main);

                BasicConsole.WriteLine(" > Starting GC Cleanup thread...");
                ProcessManager.CurrentProcess.CreateThread(Core.Tasks.GCCleanupTask.Main);

                BasicConsole.WriteLine(" > Starting Idle thread...");
                ProcessManager.CurrentProcess.CreateThread(Core.Tasks.IdleTask.Main);

                BasicConsole.WriteLine(" > Starting Device Manager...");
                Process DeviceManagerProcess = ProcessManager.CreateProcess(DeviceManagerTask.Main, "Device Manager", false, true);
                //DeviceManagerProcess.OutputMemTrace = true;
                ProcessManager.RegisterProcess(DeviceManagerProcess, Scheduler.Priority.Normal);

                BasicConsole.WriteLine(" > Starting Window Manager...");
                Process WindowManagerProcess = ProcessManager.CreateProcess(WindowManagerTask.Main, "Window Manager", false, true);
                //WindowManagerProcess.OutputMemTrace = true;
                ProcessManager.RegisterProcess(WindowManagerProcess, Scheduler.Priority.Normal);

                //TODO: Main task for commands etc

                BasicConsole.WriteLine("Started.");

                BasicConsole.PrimaryOutputEnabled = false;

                int x = 0;
                while (!Terminating)
                {
                    SystemCallMethods.SleepThread(1000);
                }

                //BasicConsole.WriteLine(" > Starting Non-critical interrupts task...");
                //ProcessManager.CurrentProcess.CreateThread(Hardware.Tasks.NonCriticalInterruptsTask.Main);

                //BasicConsole.WriteLine(" > Starting System Status task...");
                //ProcessManager.CurrentProcess.CreateThread(Core.Tasks.SystemStatusTask.Main);

                //BasicConsole.WriteLine(" > Starting Device Manager task...");
                //ProcessManager.CurrentProcess.CreateThread(Hardware.Tasks.DeviceManagerTask.Main);

                //BasicConsole.WriteLine(" > Starting Play Notes task...");
                //ProcessManager.CurrentProcess.CreateThread(Hardware.Tasks.PlayNotesTask.Main);

                //Hardware.Devices.Keyboard.InitDefault();
                //Core.Console.InitDefault();
                //Core.Shell.InitDefault();

                //BasicConsole.PrimaryOutputEnabled = false;
                //Core.Shell.Default.Execute();
                //BasicConsole.PrimaryOutputEnabled = true;

                //if (!Core.Shell.Default.Terminating)
                //{
                //    Core.Console.Default.WarningColour();
                //    Core.Console.Default.WriteLine("Abnormal shell shutdown!");
                //    Core.Console.Default.DefaultColour();
                //}
                //else
                //{
                //    Core.Console.Default.Clear();
                //}
            }
            catch
            {
                BasicConsole.PrimaryOutputEnabled = true;
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            BasicConsole.WriteLine();
            BasicConsole.WriteLine("---------------------");
            BasicConsole.WriteLine();
            BasicConsole.WriteLine("End of kernel task.");

            ExceptionMethods.HaltReason = "Managed main thread ended.";

            //TODO: Exit syscall
        }

        public static void DeferredSyscallsThread_Main()
        {
            while (!Terminating)
            {
                if (DeferredSyscallsInfo_Queued.Count == 0)
                {
                    Thread.Sleep_Indefinitely();
                }

                while (DeferredSyscallsInfo_Queued.Count > 0)
                {
                    BasicConsole.WriteLine("DSC: Popping queued info object...");
                    DeferredSyscallInfo info = (DeferredSyscallInfo)DeferredSyscallsInfo_Queued.Pop();

                    BasicConsole.WriteLine("DSC: Getting process & thread...");
                    Process CallerProcess = ProcessManager.GetProcessById(info.ProcessId);
                    Thread CallerThread = ProcessManager.GetThreadById(info.ThreadId, CallerProcess);

                    BasicConsole.WriteLine("DSC: Getting data & calling...");
                    uint Return2 = CallerThread.Return2;
                    uint Return3 = CallerThread.Return3;
                    uint Return4 = CallerThread.Return4;
                    SystemCallResults result = HandleDeferredSystemCall(
                        CallerProcess, CallerThread,
                        (SystemCallNumbers)CallerThread.SysCallNumber, CallerThread.Param1, CallerThread.Param2, CallerThread.Param3,
                        ref Return2, ref Return3, ref Return4);

                    BasicConsole.WriteLine("DSC: Ending call...");
                    EndDeferredSystemCall(CallerThread, result, Return2, Return3, Return4);

                    BasicConsole.WriteLine("DSC: Resetting & queuing info object...");
                    info.ProcessId = 0;
                    info.ThreadId = 0;
                    DeferredSyscallsInfo_Unqueued.Push(info);
                }
            }
        }
        public static unsafe SystemCallResults HandleDeferredSystemCall(
            Process CallerProcess, Thread CallerThread,
            SystemCallNumbers syscallNumber, uint Param1, uint Param2, uint Param3,
            ref uint Return2, ref uint Return3, ref uint Return4)
        {
            SystemCallResults result = SystemCallResults.Unhandled;

            switch (syscallNumber)
            {
                case SystemCallNumbers.StartThread:
                    BasicConsole.WriteLine("DSC: Create Thread");
                    CallerProcess.CreateThread((ThreadStartMethod)Utilities.ObjectUtilities.GetObject((void*)Param1));
                    BasicConsole.WriteLine("DSC: Create Thread - done.");
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.RegisterPipeOutpoint:
                    {
                        BasicConsole.WriteLine("DSC: Register Pipe Outpoint");
                        Pipes.PipeOutpoint outpoint;
                        bool registered = Pipes.PipeManager.RegisterPipeOutpoint(CallerProcess.Id, (Pipes.PipeClasses)Param1, (Pipes.PipeSubclasses)Param2, (int)Param3, out outpoint);
                        if (registered)
                        {
                            result = SystemCallResults.OK;
                        }
                        else
                        {
                            result = SystemCallResults.Fail;
                        }
                        BasicConsole.WriteLine("DSC: Register Pipe Outpoint - done.");
                    }
                    break;
                case SystemCallNumbers.GetNumPipeOutpoints:
                    {
                        BasicConsole.WriteLine("DSC: Get Num Pipe Outpoints");
                        int numOutpoints;
                        bool obtained = Pipes.PipeManager.GetNumPipeOutpoints((Pipes.PipeClasses)Param1, (Pipes.PipeSubclasses)Param2, out numOutpoints);
                        if (obtained)
                        {
                            result = SystemCallResults.OK;
                            Return2 = (uint)numOutpoints;
                        }
                        else
                        {
                            result = SystemCallResults.Fail;
                        }
                        BasicConsole.WriteLine("DSC: Get Num Pipe Outpoints - done");
                    }
                    break;
                case SystemCallNumbers.GetPipeOutpoints:
                    {
                        BasicConsole.WriteLine("DSC: Get Pipe Outpoints");
                        
                        // Need access to calling process' memory to be able to set values in request structure(s)
                        MemoryLayout OriginalMemoryLayout = EnableAccessToMemoryOfProcess(CallerProcess);

                        bool obtained = Pipes.PipeManager.GetPipeOutpoints((Pipes.PipeClasses)Param1, (Pipes.PipeSubclasses)Param2, (Pipes.PipeOutpointsRequest*)Param3);
                        if (obtained)
                        {
                            result = SystemCallResults.OK;
                        }
                        else
                        {
                            result = SystemCallResults.Fail;
                        }
                        
                        DisableAccessToMemoryOfProcess(OriginalMemoryLayout);
                        
                        BasicConsole.WriteLine("DSC: Get Pipe Outpoints - done");
                    }
                    break;
                case SystemCallNumbers.CreatePipe:
                    {
                        BasicConsole.WriteLine("DSC: Create Pipe");

                        // Need access to calling process' memory to be able to set values in request structure(s)
                        MemoryLayout OriginalMemoryLayout = EnableAccessToMemoryOfProcess(CallerProcess);

                        bool created = Pipes.PipeManager.CreatePipe(CallerProcess.Id, Param1, (Pipes.CreatePipeRequest*)Param2);
                        if (created)
                        {
                            result = SystemCallResults.OK;
                        }
                        else
                        {
                            result = SystemCallResults.Fail;
                        }

                        DisableAccessToMemoryOfProcess(OriginalMemoryLayout);

                        BasicConsole.WriteLine("DSC: Get Pipe Outpoints - done");
                    }
                    break;
                default:
                    BasicConsole.WriteLine("DSC: Unrecognised call number.");
                    BasicConsole.WriteLine((uint)syscallNumber);
                    break;
            }

            return result;
        }
        public static void EndDeferredSystemCall(Thread CallerThread, SystemCallResults result, uint Return2, uint Return3, uint Return4)
        {
            CallerThread.Return1 = (uint)result;
            CallerThread.Return2 = Return2;
            CallerThread.Return3 = Return3;
            CallerThread.Return4 = Return4;

            CallerThread._Wake();
        }
        public static MemoryLayout EnableAccessToMemoryOfProcess(Process ProcessToAccess)
        {
            MemoryLayout OriginalMemoryLayout = ProcessManager.CurrentProcess.TheMemoryLayout;
            MemoryLayout NewMemoryLayout = ProcessManager.CurrentProcess.TheMemoryLayout.Merge(ProcessToAccess.TheMemoryLayout);
            ProcessManager.CurrentProcess.TheMemoryLayout = NewMemoryLayout;
            NewMemoryLayout.Load(ProcessManager.CurrentProcess.UserMode);
            return OriginalMemoryLayout;
        }
        public static void DisableAccessToMemoryOfProcess(MemoryLayout OriginalMemoryLayout)
        {
            ProcessManager.CurrentProcess.TheMemoryLayout = OriginalMemoryLayout;
            OriginalMemoryLayout.Load(ProcessManager.CurrentProcess.UserMode);
        }

        public static int SyscallHandler(uint syscallNumber, uint param1, uint param2, uint param3, 
            ref uint Return2, ref uint Return3, ref uint Return4,
            uint callerProcessId, uint callerThreadId)
        {
            SystemCallResults result = SystemCalls.HandleSystemCallForKernel(syscallNumber, 
                param1, param2, param3, 
                ref Return2, ref Return3, ref Return4,
                callerProcessId, callerThreadId);

            if (result == SystemCallResults.Deferred)
            {
                BasicConsole.WriteLine("Deferring syscall...");
                BasicConsole.WriteLine("Popping unqueued info object...");
                DeferredSyscallInfo info = (DeferredSyscallInfo)DeferredSyscallsInfo_Unqueued.Pop();
                BasicConsole.WriteLine("Setting info...");
                info.ProcessId = callerProcessId;
                info.ThreadId = callerThreadId;

                BasicConsole.WriteLine("Queuing info object...");
                DeferredSyscallsInfo_Queued.Push(info);

                BasicConsole.WriteLine("Waking deferred syscalls thread...");
                DeferredSyscallsThread._Wake();
            }

            return (int)result;
        }
    }
}
