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
    
using System;
using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.Processes;
using Kernel.Hardware.Processes;
using Kernel.Hardware.VirtMem;

namespace Kernel.Tasks
{
    public unsafe static class KernelTask
    {
        private class DeferredSyscallInfo : FOS_System.Object
        {
            public uint ProcessId;
            public uint ThreadId;
        }

        public static bool Terminating = false;
        private static Queue DeferredSyscallsInfo_Unqueued;
        private static Queue DeferredSyscallsInfo_Queued;

        private static Thread DeferredSyscallsThread;

        private static Pipes.Standard.StandardOutpoint StdOut;
        
        private static uint WindowManagerTask_ProcessId;

        private static Hardware.Keyboards.VirtualKeyboard keyboard;
        private static Consoles.VirtualConsole console;

        public static void Main()
        {
            BasicConsole.WriteLine("Kernel task! ");
            BasicConsole.WriteLine(" > Executing normally...");
            
            DeferredSyscallsInfo_Unqueued = new Queue(256, false);
            DeferredSyscallsInfo_Queued = new Queue(DeferredSyscallsInfo_Unqueued.Capacity, false);
            for (int i = 0; i < DeferredSyscallsInfo_Unqueued.Capacity; i++)
            {
                DeferredSyscallsInfo_Unqueued.Push(new DeferredSyscallInfo());
            }

            try
            {
                BasicConsole.WriteLine(" > Initialising system calls...");
                ProcessManager.CurrentProcess.SyscallHandler = SyscallHandler;
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.RegisterSyscallHandler);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.DeregisterSyscallHandler);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.StartThread);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.SleepThread);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.WakeThread);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.RegisterPipeOutpoint);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.GetNumPipeOutpoints);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.GetPipeOutpoints);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.WaitOnPipeCreate);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.ReadPipe);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.WritePipe);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.SendMessage);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.ReceiveMessage);

                //ProcessManager.CurrentProcess.OutputMemTrace = true;

                BasicConsole.WriteLine(" > Starting Idle process...");
                Process IdleProcess = ProcessManager.CreateProcess(Tasks.IdleTask.Main, "Idle", false, true);
                ProcessManager.RegisterProcess(IdleProcess, Scheduler.Priority.ZeroTimed);
                
                BasicConsole.WriteLine(" > Starting deferred syscalls thread...");
                DeferredSyscallsThread = ProcessManager.CurrentProcess.CreateThread(DeferredSyscallsThread_Main, "Deferred Sys Calls");
                
#if DEBUG
                BasicConsole.WriteLine(" > Starting Debugger thread...");
                Debug.Debugger.MainThread = ProcessManager.CurrentProcess.CreateThread(Debug.Debugger.Main, "Debugger");
#endif

                BasicConsole.WriteLine(" > Starting GC Cleanup thread...");
                ProcessManager.CurrentProcess.CreateThread(Tasks.GCCleanupTask.Main, "GC Cleanup");

                BasicConsole.WriteLine(" > Starting Window Manager...");
                Process WindowManagerProcess = ProcessManager.CreateProcess(WindowManagerTask.Main, "Window Manager", false, true);
                WindowManagerTask_ProcessId = WindowManagerProcess.Id;
                //WindowManagerProcess.OutputMemTrace = true;
                ProcessManager.RegisterProcess(WindowManagerProcess, Scheduler.Priority.Normal);
                
                BasicConsole.WriteLine(" > Waiting for Window Manager to be ready...");
                while (!WindowManagerTask.Ready)
                {
                    BasicConsole.WriteLine(" > [Wait pause]");
                    SystemCalls.SleepThread(1000);
                }
                BasicConsole.WriteLine(" > Window Manager reported ready.");

                BasicConsole.WriteLine(" > Starting Device Manager...");
                Process DeviceManagerProcess = ProcessManager.CreateProcess(DeviceManagerTask.Main, "Device Manager", false, true);
                //DeviceManagerProcess.OutputMemTrace = true;
                ProcessManager.RegisterProcess(DeviceManagerProcess, Scheduler.Priority.Normal);
                

                //TODO: Main task for commands etc

                BasicConsole.WriteLine("Started.");

                BasicConsole.PrimaryOutputEnabled = false;
                //BasicConsole.SecondaryOutputEnabled = false;

                try
                {
                    BasicConsole.WriteLine("KT > Creating virtual keyboard...");
                    keyboard = new Hardware.Keyboards.VirtualKeyboard();

                    BasicConsole.WriteLine("KT > Creating virtual console...");
                    console = new Consoles.VirtualConsole();

                    BasicConsole.WriteLine("KT > Connecting virtual console...");
                    console.Connect();

                    BasicConsole.WriteLine("KT > Creating main shell...");
                    Shells.MainShell shell = new Shells.MainShell(console, keyboard);
                                        
                    BasicConsole.WriteLine("KT > Running...");

                    uint loops = 0;
                    while (!Terminating)
                    {
                        try
                        {
                            //FOS_System.String msg = "KT > Hello, world! (" + (FOS_System.String)loops++ + ")";
                            //console.WriteLine(msg);
                            //BasicConsole.WriteLine(msg);
                            //SystemCalls.SleepThread(1000);
                            shell.Execute();
                        }
                        catch
                        {
                            BasicConsole.WriteLine("KT > Error executing MainShell!");
                            BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                        }
                    }
                }
                catch
                {
                    BasicConsole.WriteLine("KT > Error initialising!");
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }

                //BasicConsole.WriteLine(" > Starting Play Notes task...");
                //ProcessManager.CurrentProcess.CreateThread(Hardware.Tasks.PlayNotesTask.Main);

                //Console.InitDefault();
                //Shell.InitDefault();

                //BasicConsole.PrimaryOutputEnabled = false;
                //Shell.Default.Execute();
                //BasicConsole.PrimaryOutputEnabled = true;

                //if (!Shell.Default.Terminating)
                //{
                //    Console.Default.WarningColour();
                //    Console.Default.WriteLine("Abnormal shell shutdown!");
                //    Console.Default.DefaultColour();
                //}
                //else
                //{
                //    Console.Default.Clear();
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
                    SystemCalls.SleepThread(SystemCalls.IndefiniteSleepThread);
                }

                while (DeferredSyscallsInfo_Queued.Count > 0)
                {
                    // Scheduler must be disabled during pop/push from circular buffer or we can
                    //  end up in an infinite lock. Consider what happens if a process invokes 
                    //  a deferred system call during the pop/push here and at the end of this loop.
                    //BasicConsole.WriteLine("DSC: Pausing scheduler...");
                    Scheduler.Disable(/*"DSC 1"*/);
                    //BasicConsole.WriteLine("DSC: Popping queued info object...");
                    DeferredSyscallInfo info = (DeferredSyscallInfo)DeferredSyscallsInfo_Queued.Pop();
                    //BasicConsole.WriteLine("DSC: Resuming scheduler...");
                    Scheduler.Enable();

                    //BasicConsole.WriteLine("DSC: Getting process & thread...");
                    Process CallerProcess = ProcessManager.GetProcessById(info.ProcessId);
                    Thread CallerThread = ProcessManager.GetThreadById(info.ThreadId, CallerProcess);

                    //BasicConsole.WriteLine("DSC: Getting data & calling...");
                    uint Return2 = CallerThread.Return2;
                    uint Return3 = CallerThread.Return3;
                    uint Return4 = CallerThread.Return4;
                    SystemCallResults result = HandleDeferredSystemCall(
                        CallerProcess, CallerThread,
                        (SystemCallNumbers)CallerThread.SysCallNumber, CallerThread.Param1, CallerThread.Param2, CallerThread.Param3,
                        ref Return2, ref Return3, ref Return4);

                    //BasicConsole.WriteLine("DSC: Ending call...");
                    if (result != SystemCallResults.Deferred)
                    {
                        EndDeferredSystemCall(CallerThread, result, Return2, Return3, Return4);
                    }

                    //BasicConsole.WriteLine("DSC: Resetting info object...");
                    info.ProcessId = 0;
                    info.ThreadId = 0;

                    // See comment at top of loop for why this is necessary
                    //BasicConsole.WriteLine("DSC: Pausing scheduler...");
                    Scheduler.Disable(/*"DSC 2"*/);
                    //BasicConsole.WriteLine("DSC: Queuing info object...");
                    DeferredSyscallsInfo_Unqueued.Push(info);
                    //BasicConsole.WriteLine("DSC: Resuming scheduler...");
                    Scheduler.Enable();
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
                    //BasicConsole.WriteLine("DSC: Start Thread");
                    Return2 = CallerProcess.CreateThread((ThreadStartMethod)Utilities.ObjectUtilities.GetObject((void*)Param1), "[From sys call]").Id;
                    //BasicConsole.WriteLine("DSC: Start Thread - done.");
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.RegisterPipeOutpoint:
                    {
                        //BasicConsole.WriteLine("DSC: Register Pipe Outpoint");
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
                        //BasicConsole.WriteLine("DSC: Register Pipe Outpoint - done.");
                    }
                    break;
                case SystemCallNumbers.GetNumPipeOutpoints:
                    {
                        //BasicConsole.WriteLine("DSC: Get Num Pipe Outpoints");
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
                        //BasicConsole.WriteLine("DSC: Get Num Pipe Outpoints - done");
                    }
                    break;
                case SystemCallNumbers.GetPipeOutpoints:
                    {
                        //BasicConsole.WriteLine("DSC: Get Pipe Outpoints");
                        
                        bool obtained = Pipes.PipeManager.GetPipeOutpoints(CallerProcess, (Pipes.PipeClasses)Param1, (Pipes.PipeSubclasses)Param2, (Pipes.PipeOutpointsRequest*)Param3);
                        if (obtained)
                        {
                            result = SystemCallResults.OK;
                        }
                        else
                        {
                            result = SystemCallResults.Fail;
                        }
                        
                        //BasicConsole.WriteLine("DSC: Get Pipe Outpoints - done");
                    }
                    break;
                case SystemCallNumbers.CreatePipe:
                    {
                        //BasicConsole.WriteLine("DSC: Create Pipe");

                        bool created = Pipes.PipeManager.CreatePipe(CallerProcess.Id, Param1, (Pipes.CreatePipeRequest*)Param2);
                        if (created)
                        {
                            result = SystemCallResults.OK;
                        }
                        else
                        {
                            result = SystemCallResults.Fail;
                        }

                        //BasicConsole.WriteLine("DSC: Create Pipe - done");
                    }
                    break;
                case SystemCallNumbers.WaitOnPipeCreate:
                    {
                        //BasicConsole.WriteLine("DSC: Wait On Pipe Create");

                        bool waiting = Pipes.PipeManager.WaitOnPipeCreate(CallerProcess.Id, CallerThread.Id, (Pipes.PipeClasses)Param1, (Pipes.PipeSubclasses)Param2);
                        if (waiting)
                        {
                            result = SystemCallResults.Deferred;
                        }
                        else
                        {
                            result = SystemCallResults.Fail;
                        }

                        //BasicConsole.WriteLine("DSC: Wait On Pipe Create - done");
                    }
                    break;
                case SystemCallNumbers.ReadPipe:
                    {
                        //BasicConsole.WriteLine("DSC: Read Pipe");

                        // Need access to calling process' memory to be able to set values in request structure(s)
                        MemoryLayout OriginalMemoryLayout = SystemCallsHelpers.EnableAccessToMemoryOfProcess(CallerProcess);

                        Pipes.ReadPipeRequest* RequestPtr = (Pipes.ReadPipeRequest*)Param1;
                        Pipes.PipeManager.RWResults RWResult = Pipes.PipeManager.ReadPipe(RequestPtr->PipeId, RequestPtr->Blocking, CallerProcess, CallerThread);

                        if (RWResult == Pipes.PipeManager.RWResults.Error)
                        {
                            result = SystemCallResults.Fail;
                        }
                        else
                        {
                            // Returning Deferred state from here will leave the caller thread
                            //  in whatever state ReadPipe decided it should be in.
                            result = SystemCallResults.Deferred;
                        }
                        
                        SystemCallsHelpers.DisableAccessToMemoryOfProcess(OriginalMemoryLayout);

                        //BasicConsole.WriteLine("DSC: Read Pipe - done");
                    }
                    break;
                case SystemCallNumbers.WritePipe:
                    {
                        //BasicConsole.WriteLine("DSC: Write Pipe");

                        // Need access to calling process' memory to be able to set values in request structure(s)
                        MemoryLayout OriginalMemoryLayout = SystemCallsHelpers.EnableAccessToMemoryOfProcess(CallerProcess);

                        Pipes.WritePipeRequest* RequestPtr = (Pipes.WritePipeRequest*)Param1;
                        Pipes.PipeManager.RWResults RWResult = Pipes.PipeManager.WritePipe(RequestPtr->PipeId, RequestPtr->Blocking, CallerProcess, CallerThread);

                        if (RWResult == Pipes.PipeManager.RWResults.Error)
                        {
                            result = SystemCallResults.Fail;
                        }
                        else
                        {
                            // Returning Deferred state from here will leave the caller thread
                            //  in whatever state WritePipe decided it should be in.
                            result = SystemCallResults.Deferred;
                        }
                        
                        SystemCallsHelpers.DisableAccessToMemoryOfProcess(OriginalMemoryLayout);

                        //BasicConsole.WriteLine("DSC: Write Pipe - done");
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

        public static int HandleISR(uint ISRNum)
        {
            if (ISRNum == 48)
            {
                SystemCalls.InterruptHandler();
                return 0;
            }
            return -1;
        }
        public static int HandleIRQ(uint IRQNum)
        {
            if (IRQNum == 0)
            {
                Hardware.Timers.PIT.ThePIT.InterruptHandler();
                return 0;
            }
            return -1;
        }

        public static int SyscallHandler(uint syscallNumber, uint param1, uint param2, uint param3, 
            ref uint Return2, ref uint Return3, ref uint Return4,
            uint callerProcessId, uint callerThreadId)
        {
            SystemCallResults result = HandleSystemCallForKernel(syscallNumber, 
                param1, param2, param3, 
                ref Return2, ref Return3, ref Return4,
                callerProcessId, callerThreadId);

            if (result == SystemCallResults.Deferred || result == SystemCallResults.Deferred_PermitActions)
            {
                //BasicConsole.WriteLine("Deferring syscall...");
                //BasicConsole.WriteLine("Popping unqueued info object...");
                DeferredSyscallInfo info = (DeferredSyscallInfo)DeferredSyscallsInfo_Unqueued.Pop();
                //BasicConsole.WriteLine("Setting info...");
                info.ProcessId = callerProcessId;
                info.ThreadId = callerThreadId;

                //BasicConsole.WriteLine("Queuing info object...");
                DeferredSyscallsInfo_Queued.Push(info);

                //BasicConsole.WriteLine("Waking deferred syscalls thread...");
                DeferredSyscallsThread._Wake();
            }

            return (int)result;
        }

        /// <summary>
        /// Special handler method for system calls recognised/handlded by the kernel task.
        /// </summary>
        /// <param name="syscallNumber">The system call number that has been invoked.</param>
        /// <param name="param1">The value of the first parameter.</param>
        /// <param name="param2">The value of the second parameter.</param>
        /// <param name="param3">The value of the third parameter.</param>
        /// <param name="Return2">Reference to the second return value.</param>
        /// <param name="Return3">Reference to the third return value.</param>
        /// <param name="Return4">Reference to the fourth return value.</param>
        /// <param name="callerProcesId">The Id of the process which invoked the system call.</param>
        /// <param name="callerThreadId">The Id of the thread which invoked the system call.</param>
        /// <returns>A system call result indicating what has occurred and what should occur next.</returns>
        /// <remarks>
        /// Executes within the interrupt handler. Usual restrictions apply.
        /// </remarks>
        public static SystemCallResults HandleSystemCallForKernel(uint syscallNumber,
            uint param1, uint param2, uint param3,
            ref uint Return2, ref uint Return3, ref uint Return4,
            uint callerProcesId, uint callerThreadId)
        {
            SystemCallResults result = SystemCallResults.Unhandled;

            switch ((SystemCallNumbers)syscallNumber)
            {
                case SystemCallNumbers.SleepThread:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Sleep Thread");
#endif
                    SysCall_Sleep((int)param1, callerProcesId, callerThreadId);
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.WakeThread:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Wake Thread");
#endif
                    SysCall_Wake(callerProcesId, param1);
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.RegisterISRHandler:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Register ISR Handler");
#endif
                    if (SysCall_RegisterISRHandler((int)param1, param2, callerProcesId))
                    {
                        result = SystemCallResults.OK;
                    }
                    else
                    {
                        result = SystemCallResults.Fail;
                    }
                    break;
                case SystemCallNumbers.DeregisterISRHandler:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Deregister ISR Handler");
#endif
                    SysCall_DeregisterISRHandler((int)param1, callerProcesId);
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.RegisterIRQHandler:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Register IRQ Handler");
#endif
                    if (SysCall_RegisterIRQHandler((int)param1, param2, callerProcesId))
                    {
                        result = SystemCallResults.OK;
                    }
                    else
                    {
                        result = SystemCallResults.Fail;
                    }
                    break;
                case SystemCallNumbers.DeregisterIRQHandler:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Deregister IRQ Handler");
#endif
                    SysCall_DeregisterIRQHandler((int)param1, callerProcesId);
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.RegisterSyscallHandler:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Register Syscall Handler");
#endif
                    if (SysCall_RegisterSyscallHandler((int)param1, param2, callerProcesId))
                    {
                        result = SystemCallResults.OK;
                    }
                    else
                    {
                        result = SystemCallResults.Fail;
                    }
                    break;
                case SystemCallNumbers.DeregisterSyscallHandler:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Deregister Syscall Handler");
#endif
                    SysCall_DeregisterSyscallHandler((int)param1, callerProcesId);
                    result = SystemCallResults.OK;
                    break;
                case SystemCallNumbers.StartThread:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Start Thread");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.RegisterPipeOutpoint:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Register Pipe Outpoint");
#endif
                    result = SystemCallResults.Deferred_PermitActions;
                    break;
                case SystemCallNumbers.GetNumPipeOutpoints:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Get Num Pipe Outpoints");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.GetPipeOutpoints:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Get Pipe Outpoints");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.CreatePipe:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Create Pipe");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.WaitOnPipeCreate:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Wait On Pipe Create");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.ReadPipe:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Read Pipe");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.WritePipe:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("System call : Write Pipe");
#endif
                    result = SystemCallResults.Deferred;
                    break;
                case SystemCallNumbers.SendMessage:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("Syscall: Send message");
#endif
                    result = SysCall_SendMessage(callerProcesId, callerThreadId, param1, param2, param3) ? SystemCallResults.OK : SystemCallResults.Fail;
                    break;
                case SystemCallNumbers.ReceiveMessage:
#if SYSCALLS_TRACE
                    BasicConsole.WriteLine("Syscall: Receive message");
#endif
                    Tasks.KernelTask.ReceiveMessage(callerProcesId, param1, param2);
                    break;
                //#if SYSCALLS_TRACE
                default:
                    BasicConsole.WriteLine("System call unrecognised/unhandled by Kernel Task.");
                    break;
                //#endif
            }

            return result;
        }

        /// <summary>
        /// Performs Sleep system call processing for the Kernel Task.
        /// </summary>
        /// <param name="ms">The number of milliseconds to sleep for.</param>
        /// <param name="callerProcessId">The Id of the process to sleep.</param>
        /// <param name="callerThreadId">The Id of the thread to sleep.</param>
        private static void SysCall_Sleep(int ms, uint callerProcessId, uint callerThreadId)
        {
#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Sleeping thread...");
#endif
            ProcessManager.GetThreadById(callerThreadId, ProcessManager.GetProcessById(callerProcessId))._EnterSleep(ms);
        }
        /// <summary>
        /// Performs Wake system call processing for the Kernel Task.
        /// </summary>
        /// <param name="callerProcessId">The Id of the process to wake.</param>
        /// <param name="threadToWakeId">The Id of the thread to wake.</param>
        private static void SysCall_Wake(uint callerProcessId, uint threadToWakeId)
        {
#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Waking thread...");
#endif
            ProcessManager.GetThreadById(threadToWakeId, ProcessManager.GetProcessById(callerProcessId))._Wake();
        }
        /// <summary>
        /// Performs Register ISR Handler system call processing for the Kernel Task.
        /// </summary>
        /// <param name="ISRNum">The ISR number to register the handler for.</param>
        /// <param name="handlerAddr">The address of the handler function.</param>
        /// <param name="callerProcessId">The Id of the process to register the handler for.</param>
        /// <returns>True if the handler was registered successfully. Otherwise, false.</returns>
        private static bool SysCall_RegisterISRHandler(int ISRNum, uint handlerAddr, uint callerProcessId)
        {
            if (ISRNum < 49)
            {
                return false;
            }

#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Registering ISR handler...");
#endif
            Process theProcess = ProcessManager.GetProcessById(callerProcessId);

            if (handlerAddr != 0xFFFFFFFF)
            {
                theProcess.ISRHandler = (Hardware.Processes.ISRHanderDelegate)Utilities.ObjectUtilities.GetObject((void*)handlerAddr);
            }

            theProcess.ISRsToHandle.Set(ISRNum);

            return true;
        }
        /// <summary>
        /// Performs Deregister ISR Handler system call processing for the Kernel Task.
        /// </summary>
        /// <param name="ISRNum">The ISR number to deregister.</param>
        /// <param name="callerProcessId">The Id of the process to deregister the handler of.</param>
        private static void SysCall_DeregisterISRHandler(int ISRNum, uint callerProcessId)
        {
#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Deregistering ISR handler...");
#endif
            ProcessManager.GetProcessById(callerProcessId).ISRsToHandle.Clear(ISRNum);
        }
        /// <summary>
        /// Performs Register IRQ Handler system call processing for the Kernel Task.
        /// </summary>
        /// <param name="IRQNum">The IRQ number to register the handler for.</param>
        /// <param name="handlerAddr">The address of the handler function.</param>
        /// <param name="callerProcessId">The Id of the process to register the handler for.</param>
        /// <returns>True if the handler was registered successfully. Otherwise, false.</returns>
        private static bool SysCall_RegisterIRQHandler(int IRQNum, uint handlerAddr, uint callerProcessId)
        {
            if (IRQNum > 15)
            {
                return false;
            }

#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Registering IRQ handler...");
#endif
            Process theProcess = ProcessManager.GetProcessById(callerProcessId);

            if (handlerAddr != 0xFFFFFFFF)
            {
                theProcess.IRQHandler = (Hardware.Processes.IRQHanderDelegate)Utilities.ObjectUtilities.GetObject((void*)handlerAddr);
            }

            theProcess.IRQsToHandle.Set(IRQNum);

            Hardware.Interrupts.Interrupts.EnableIRQ((byte)IRQNum);

            return true;
        }
        /// <summary>
        /// Performs Deregister IRQ Handler system call processing for the Kernel Task.
        /// </summary>
        /// <param name="IRQNum">The IRQ number to deregister.</param>
        /// <param name="callerProcessId">The Id of the process to deregister the handler of.</param>
        private static void SysCall_DeregisterIRQHandler(int IRQNum, uint callerProcessId)
        {
#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Deregistering IRQ handler...");
#endif
            ProcessManager.GetProcessById(callerProcessId).IRQsToHandle.Clear(IRQNum);
        }
        /// <summary>
        /// Performs Register System Call Handler system call processing for the Kernel Task.
        /// </summary>
        /// <param name="syscallNum">The system call number to register the handler for.</param>
        /// <param name="handlerAddr">The address of the handler function.</param>
        /// <param name="callerProcessId">The Id of the process to register the handler for.</param>
        /// <returns>True if the handler was registered successfully. Otherwise, false.</returns>
        private static bool SysCall_RegisterSyscallHandler(int syscallNum, uint handlerAddr, uint callerProcessId)
        {
#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Registering syscall handler...");
#endif
            Process theProcess = ProcessManager.GetProcessById(callerProcessId);

            if (handlerAddr != 0xFFFFFFFF)
            {
                theProcess.SyscallHandler = (Hardware.Processes.SyscallHanderDelegate)Utilities.ObjectUtilities.GetObject((void*)handlerAddr);
            }

            theProcess.SyscallsToHandle.Set(syscallNum);

            return true;
        }
        /// <summary>
        /// Performs Deregister System Call Handler system call processing for the Kernel Task.
        /// </summary>
        /// <param name="syscallNum">The system call number to deregister.</param>
        /// <param name="callerProcessId">The Id of the process to deregister the handler of.</param>
        private static void SysCall_DeregisterSyscallHandler(int syscallNum, uint callerProcessId)
        {
#if SYSCALLS_TRACE
            BasicConsole.WriteLine("Deregistering syscall handler...");
#endif
            ProcessManager.GetProcessById(callerProcessId).SyscallsToHandle.Clear(syscallNum);
        }
        /// <summary>
        /// Performs Send Message system call processing for the Kernel Task.
        /// </summary>
        /// <param name="callerProcessId">The Id of the process sending the message.</param>
        /// <param name="callerThreadId">The Id of the thread sending the message.</param>
        /// <param name="targetProcessId">The Id of the process to send the message to.</param>
        /// <param name="message1">The first value of the message.</param>
        /// <param name="message2">The second value of the message.</param>
        /// <returns>True if the message was accepted.</returns>
        private static bool SysCall_SendMessage(uint callerProcessId, uint callerThreadId, uint targetProcessId, uint message1, uint message2)
        {
            bool Result = false;

            Process CallerProcess = ProcessManager.GetProcessById(callerProcessId);
            Process TargetProcess = ProcessManager.GetProcessById(targetProcessId);

            ProcessManager.SwitchProcess(targetProcessId, -1);

            if (TargetProcess.SyscallsToHandle.IsSet((int)SystemCallNumbers.ReceiveMessage) && TargetProcess.SyscallHandler != null)
            {
                uint Return2 = 0;
                uint Return3 = 0;
                uint Return4 = 0;
                TargetProcess.SyscallHandler((uint)SystemCallNumbers.ReceiveMessage, message1, message2, 0, ref Return2, ref Return3, ref Return4, callerProcessId, 0xFFFFFFFF);

                Result = true;
            }

            ProcessManager.SwitchProcess(callerProcessId, (int)callerThreadId);

            return Result;
        }


        public static void ReceiveMessage(uint CallerProcessId, uint Message1, uint Message2)
        {
            if (CallerProcessId == WindowManagerTask_ProcessId)
            {
                ReceiveKey(Message1);
            }
        }
        public static void ReceiveKey(uint ScanCode)
        {
            if (keyboard != null)
            {
                keyboard.HandleScancode(ScanCode);
            }
        }
    }

    public class TestComparable : FOS_System.Collections.Comparable
    {
        public TestComparable()
        {
        }
        public TestComparable(int key)
             : base(key)
        {
        }
    }
}
