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
using Kernel.FOS_System.Collections;
using Kernel.Hardware.Processes;
using Kernel.Shared;

namespace Kernel.Core.Processes
{
    public static class SystemCalls
    {
        private static Thread DeferredSystemCallsHandlerThread;
        private static bool DeferredSystemCallsHandlerThread_Awake = false;
        private static bool DeferredSystemCallsHandlerThread_Terminate = false;
        private static Process DeferredSystemCalls_CurrentProcess;
        private static Thread DeferredSystemCalls_CurrentThread;
        private static bool DeferredSystemCalls_WakeCurrentThread = true;

        private static int Int48HandlerId = 0;
        public static void Init()
        {
            if (Int48HandlerId == 0)
            {
                // We want to ignore process state so that we handle the interrupt in the context of
                //  the calling process.
                Int48HandlerId = Hardware.Interrupts.Interrupts.AddISRHandler(48, Int48, null, true, true, "Sys Call");
            }

            if (DeferredSystemCallsHandlerThread == null)
            {
                DeferredSystemCallsHandlerThread = ProcessManager.CurrentProcess.CreateThread(DeferredSystemCallsHandler);
            }
        }

        private static void Int48(FOS_System.Object state)
        {
            //FOS_System.GC.Enable("System calls : Int48 (1)");
            //FOS_System.Heap.PreventAllocation = false;
            //BasicConsole.WriteLine("WARNING: Enabled GC/Heap inside critical interrupt! (SysCalls: Int48 : 1)");

            //BasicConsole.Write("Sys call ");
            //BasicConsole.Write(ProcessManager.CurrentThread.SysCallNumber);
            //BasicConsole.Write(" : ");
            //BasicConsole.WriteLine(ProcessManager.CurrentProcess.Name);
            //BasicConsole.WriteLine(((FOS_System.String)" > Param1: ") + ProcessManager.CurrentThread.Param1);
            //BasicConsole.WriteLine(((FOS_System.String)" > Param2: ") + ProcessManager.CurrentThread.Param2);
            //BasicConsole.WriteLine(((FOS_System.String)" > Param3: ") + ProcessManager.CurrentThread.Param3);

            //FOS_System.GC.Disable("System calls : Int48 (1)");
            //FOS_System.Heap.PreventAllocation = true;

            BasicConsole.WriteLine(ProcessManager.CurrentProcess.Name);
            switch ((Kernel.Shared.SystemCalls)ProcessManager.CurrentThread.SysCallNumber)
            {
                case Kernel.Shared.SystemCalls.INVALID:
                    BasicConsole.WriteLine("Error! INVALID System Call made.");
                    break;
                case Kernel.Shared.SystemCalls.Sleep:
                    BasicConsole.WriteLine("System call : Sleep");
                    SysCall_Sleep((int)ProcessManager.CurrentThread.Param1);
                    break;
                case Kernel.Shared.SystemCalls.PlayNote:
                    //BasicConsole.WriteLine("System call : PlayNote");
                    DeferSystemCall();
                    break;
                case Kernel.Shared.SystemCalls.Semaphore:
                    //BasicConsole.WriteLine("System call : Semaphore");
                    DeferSystemCall();
                    break;
                case Kernel.Shared.SystemCalls.Thread:
                    //BasicConsole.WriteLine("System call : Thread");
                    DeferSystemCall();
                    break;
                default:
                    BasicConsole.WriteLine("System call : Unrecognised");
                    break;
            }

            //FOS_System.GC.Enable("System calls : Int48 (2)");
            //FOS_System.Heap.PreventAllocation = false;
            //BasicConsole.WriteLine("WARNING: Enabled GC/Heap inside critical interrupt! (SysCalls: Int48 : 2)");

            //BasicConsole.WriteLine(((FOS_System.String)" > Return 1: ") + ProcessManager.CurrentThread.Return1);
            //BasicConsole.WriteLine(((FOS_System.String)" > Return 2: ") + ProcessManager.CurrentThread.Return2);
            //BasicConsole.WriteLine(((FOS_System.String)" > Return 3: ") + ProcessManager.CurrentThread.Return3);
            //BasicConsole.WriteLine(((FOS_System.String)" > Return 4: ") + ProcessManager.CurrentThread.Return4);

            //FOS_System.GC.Disable("System calls : Int48 (2)");
            //FOS_System.Heap.PreventAllocation = true;
        }

        private static void DeferSystemCall()
        {
            //BasicConsole.WriteLine("Deferring system call...");

            //BasicConsole.Write("Sleep indefinite: ");
            Thread.EnterSleep(Thread.IndefiniteSleep);
            //BasicConsole.WriteLine("Done.");

            //BasicConsole.Write("Update thread deferred statuses: ");
            ProcessManager.CurrentProcess.ContainsThreadsWaitingOnDeferredSystemCall = true;
            ProcessManager.CurrentThread.WaitingOnDeferredSystemCall = true;
            //BasicConsole.WriteLine("Done.");

            //BasicConsole.Write("Waken deferred thread: ");
            DeferredSystemCallsHandlerThread_Awake = true;
            DeferredSystemCallsHandlerThread._Wake();
            //BasicConsole.WriteLine("Done.");

            //BasicConsole.Write("Update current thread state: ");
            Scheduler.UpdateCurrentState();
            //BasicConsole.WriteLine("Done.");
        }
        private static void DeferredSystemCallsHandler()
        {
            while (!DeferredSystemCallsHandlerThread_Terminate)
            {
                if (!DeferredSystemCallsHandlerThread_Awake)
                {
                    Thread.Sleep_Indefinitely();
                }

                DeferredSystemCallsHandlerThread_Awake = false;

                Hardware.VirtMem.MemoryLayout OriginalMemoryLayout = ProcessManager.CurrentProcess.TheMemoryLayout;

                //BasicConsole.WriteLine("Handling deferred system calls.");

                //BasicConsole.WriteLine("Enumerating processes...");
                for (int i = 0; i < ProcessManager.Processes.Count; i++)
                {
                    Process aProcess = (Process)ProcessManager.Processes[i];
                    if (aProcess.ContainsThreadsWaitingOnDeferredSystemCall)
                    {
                        DeferredSystemCalls_CurrentProcess = aProcess;

                        //BasicConsole.WriteLine(aProcess.TheMemoryLayout.ToString());
                        ProcessManager.CurrentProcess.TheMemoryLayout = ProcessManager.CurrentProcess.TheMemoryLayout.Merge(aProcess.TheMemoryLayout);
                        //BasicConsole.WriteLine(ProcessManager.CurrentProcess.TheMemoryLayout.ToString());
                        
                        Scheduler.Disable();
                        ProcessManager.CurrentProcess.TheMemoryLayout.Load(false);
                        Scheduler.Enable();

                        //BasicConsole.WriteLine("Found marked process. Enumerating threads...");
                        for (int j = 0; j < aProcess.Threads.Count; j++)
                        {
                            Thread aThread = (Thread)aProcess.Threads[j];
                            if (aThread.WaitingOnDeferredSystemCall)
                            {
                                //BasicConsole.WriteLine("Found marked thread. Processing...");
                                aThread.WaitingOnDeferredSystemCall = false;

                                DeferredSystemCalls_CurrentThread = aThread;
                                DeferredSystemCalls_WakeCurrentThread = true;

                                //BasicConsole.WriteLine("Gathering info...");
                                //BasicConsole.WriteLine(ProcessManager.CurrentProcess.TheMemoryLayout.ToString());
                                //Temp store because return value 1 is put in EAX
                                uint sysCallNum = DeferredSystemCalls_CurrentThread.SysCallNumber;
                                uint param1 = DeferredSystemCalls_CurrentThread.Param1;
                                uint param2 = DeferredSystemCalls_CurrentThread.Param2;
                                uint param3 = DeferredSystemCalls_CurrentThread.Param3;

                                // Unlike non-critical interrupts, it doesn't matter fi we switch process here
                                //  and corrupt the general purpose registers since they are the return values.
                                //  The caller should not expect any registers to remain constant during a system
                                //  call (aside from EBP, ESP, EIP and related registers)

                                //BasicConsole.WriteLine("Evaluating system call...");

                                //BasicConsole.Write("Deferred sys call ");
                                //BasicConsole.Write(sysCallNum);
                                //BasicConsole.Write(" : ");
                                //BasicConsole.WriteLine(DeferredSystemCalls_CurrentProcess.Name);
                                //BasicConsole.WriteLine(((FOS_System.String)" > Param1: ") + DeferredSystemCalls_CurrentThread.Param1);
                                //BasicConsole.WriteLine(((FOS_System.String)" > Param2: ") + DeferredSystemCalls_CurrentThread.Param2);
                                //BasicConsole.WriteLine(((FOS_System.String)" > Param3: ") + DeferredSystemCalls_CurrentThread.Param3);

                                switch ((Kernel.Shared.SystemCalls)sysCallNum)
                                {
                                    case Kernel.Shared.SystemCalls.INVALID:
                                        BasicConsole.WriteLine("Error! INVALID (deferred) system call made.");
                                        break;
                                    case Kernel.Shared.SystemCalls.PlayNote:
                                        BasicConsole.WriteLine("PlayNote deferred system calls.");
                                        SysCall_PlayNote((Hardware.Timers.PIT.MusicalNote)param1, (Hardware.Timers.PIT.MusicalNoteValue)param2, param3);
                                        break;
                                    case Kernel.Shared.SystemCalls.Semaphore:
                                        BasicConsole.WriteLine("Semaphore deferred system calls.");
                                        SysCall_Semaphore((SemaphoreRequests)param1, (int)param2, param3);
                                        break;
                                    case Kernel.Shared.SystemCalls.Thread:
                                        BasicConsole.WriteLine("Thread deferred system calls.");
                                        SysCall_Thread((ThreadRequests)param1, param2);
                                        break;
                                    default:
                                        BasicConsole.WriteLine("Unrecognised deferred system call.");
                                        break;
                                }

                                //BasicConsole.WriteLine(((FOS_System.String)" > Return 1: ") + DeferredSystemCalls_CurrentThread.Return1);
                                //BasicConsole.WriteLine(((FOS_System.String)" > Return 2: ") + DeferredSystemCalls_CurrentThread.Return2);
                                //BasicConsole.WriteLine(((FOS_System.String)" > Return 3: ") + DeferredSystemCalls_CurrentThread.Return3);
                                //BasicConsole.WriteLine(((FOS_System.String)" > Return 4: ") + DeferredSystemCalls_CurrentThread.Return4);

                                if (DeferredSystemCalls_WakeCurrentThread)
                                {
                                    DeferredSystemCalls_CurrentThread._Wake();
                                }

                                // Prevent cross-contamination / this would count as a security consideration
                                sysCallNum = 0;
                                param1 = 0;
                                param2 = 0;
                                param3 = 0;
                            }
                        }

                        aProcess.TheMemoryLayout.Unload();
                    }
                }

                ProcessManager.CurrentProcess.TheMemoryLayout = OriginalMemoryLayout;
                Scheduler.Disable();
                ProcessManager.CurrentProcess.TheMemoryLayout.Load(false);
                Scheduler.Enable();

                //BasicConsole.WriteLine("Completed deferred system call handling.");
            }
        }

        private static void SysCall_Sleep(int ms)
        {
            Thread.EnterSleep(ms);
            Scheduler.UpdateCurrentState();
        }

        private static void SysCall_PlayNote(Hardware.Timers.PIT.MusicalNote note, Hardware.Timers.PIT.MusicalNoteValue duration, uint bpm)
        {
            Hardware.Tasks.PlayNotesTask.RequestNote(note, duration, bpm);
        }

        private static void SysCall_Semaphore(SemaphoreRequests request, int id, uint limitOrProcessId)
        {
            SemaphoreResponses response = SemaphoreResponses.INVALID;
            switch (request)
            {
                case SemaphoreRequests.INVALID:
                    BasicConsole.WriteLine("Error! INVALID semaphore request made.");
                    response = SemaphoreResponses.INVALID;
                    break;
                case SemaphoreRequests.Allocate:
                    BasicConsole.WriteLine("Allocate Semaphore Request");
                    id = ProcessManager.Semaphore_Allocate((int)limitOrProcessId, DeferredSystemCalls_CurrentProcess);
                    BasicConsole.Write("Allocated id: ");
                    BasicConsole.WriteLine(id);
                    response = id != -1 ? SemaphoreResponses.Success : SemaphoreResponses.Error;
                    break;
                case SemaphoreRequests.Deallocate:
                    BasicConsole.WriteLine("Deallocate Semaphore Request");
                    response = ProcessManager.Semaphore_Deallocate(id, DeferredSystemCalls_CurrentProcess) ? SemaphoreResponses.Success : SemaphoreResponses.Error;
                    break;
                case SemaphoreRequests.Wait:
                    BasicConsole.WriteLine("Wait on a Semaphore Request");
                    int result = ProcessManager.Semaphore_Wait(id, DeferredSystemCalls_CurrentProcess, DeferredSystemCalls_CurrentThread);
                    DeferredSystemCalls_WakeCurrentThread = result != 0;
                    response = result == 1 ? SemaphoreResponses.Success : (result == 0 ? SemaphoreResponses.Fail : SemaphoreResponses.Error);
                    break;
                case SemaphoreRequests.Signal:
                    BasicConsole.WriteLine("Signal a Semaphore Request");
                    response = ProcessManager.Semaphore_Signal(id, DeferredSystemCalls_CurrentProcess) ? SemaphoreResponses.Success : SemaphoreResponses.Error;
                    break;
                case SemaphoreRequests.AddOwner:
                    BasicConsole.WriteLine("Add Owner to a Semaphore Request");
                    response = ProcessManager.Semaphore_AddOwner(id, limitOrProcessId, DeferredSystemCalls_CurrentProcess) ? SemaphoreResponses.Success : SemaphoreResponses.Error;
                    break;
                default:
                    BasicConsole.WriteLine("Error! Unrecognised semaphore request made.");
                    response = SemaphoreResponses.INVALID;
                    break;
            }

            //BasicConsole.WriteLine("----- Completed -----");
            
            DeferredSystemCalls_CurrentThread.Return1 = (uint)response;
            DeferredSystemCalls_CurrentThread.Return2 = (uint)id;
            DeferredSystemCalls_CurrentThread.Return3 = 0;
            DeferredSystemCalls_CurrentThread.Return4 = 0;
        }

        private static void SysCall_Thread(ThreadRequests request, uint startMethod)
        {
            ThreadResponses response = ThreadResponses.INVALID;
            switch (request)
            {
                case ThreadRequests.INVALID:
                    BasicConsole.WriteLine("Error! INVALID thread request made.");
                    response = ThreadResponses.INVALID;
                    break;
                case ThreadRequests.Create:
                    BasicConsole.WriteLine("Create Thread Request");
                    BasicConsole.Write("Start method: ");
                    BasicConsole.WriteLine(startMethod);
                    unsafe
                    {
                        Scheduler.Disable();
                        Thread newThread = DeferredSystemCalls_CurrentProcess.CreateThread((ThreadStartMethod)Utilities.ObjectUtilities.GetObject((void*)startMethod));
                        newThread._EnterSleep(5000);
                        Scheduler.Enable();
                        response = ThreadResponses.Success;
                    
                        //BasicConsole.WriteLine("Thread created.");
                        //BasicConsole.Write("    > Stack : ");
                        //BasicConsole.WriteLine((uint)newThread.State->ThreadStackTop);
                    }
                    break;
                default:
                    BasicConsole.WriteLine("Error! Unrecognised thread request made.");
                    response = ThreadResponses.INVALID;
                    break;
            }

            DeferredSystemCalls_CurrentThread.Return1 = (uint)response;
            DeferredSystemCalls_CurrentThread.Return2 = 0;
            DeferredSystemCalls_CurrentThread.Return3 = 0;
            DeferredSystemCalls_CurrentThread.Return4 = 0;
        }
    }
}
