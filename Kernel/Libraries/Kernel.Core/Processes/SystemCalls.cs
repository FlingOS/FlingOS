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

namespace Kernel.Core.Processes
{
    public enum SystemCall : uint
    {
        INVALID = 0,
        Sleep = 1,
        PlayNote = 2,
        Semaphore = 3
    }

    public static class SystemCalls
    {
        private static Thread DeferredSystemCallsHandlerThread;
        private static bool DeferredSystemCallsHandlerThread_Awake = false;
        private static bool DeferredSystemCallsHandlerThread_Terminate = false;
        private static Thread DeferredSystemCalls_CurrentThread;

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

            switch ((SystemCall)ProcessManager.CurrentThread.SysCallNumber)
            {
                case SystemCall.INVALID:
                    BasicConsole.WriteLine("Error! INVALID System Call made.");
                    break;
                case SystemCall.Sleep:
                    //BasicConsole.WriteLine("System call : Sleep");
                    SysCall_Sleep((int)ProcessManager.CurrentThread.Param1);
                    break;
                case SystemCall.PlayNote:
                    //BasicConsole.WriteLine("System call : PlayNote");
                    DeferSystemCall();
                    break;
                case SystemCall.Semaphore:
                    BasicConsole.WriteLine("System call : Semaphore");
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

                //BasicConsole.WriteLine("Handling deferred system calls.");

                //BasicConsole.WriteLine("Enumerating processes...");
                for (int i = 0; i < ProcessManager.Processes.Count; i++)
                {
                    Process aProcess = (Process)ProcessManager.Processes[i];
                    if (aProcess.ContainsThreadsWaitingOnDeferredSystemCall)
                    {
                        aProcess.TheMemoryLayout.Load(aProcess.UserMode);

                        //BasicConsole.WriteLine("Found marked process. Enumerating threads...");
                        for (int j = 0; j < aProcess.Threads.Count; j++)
                        {
                            Thread aThread = (Thread)aProcess.Threads[j];
                            if (aThread.WaitingOnDeferredSystemCall)
                            {
                                //BasicConsole.WriteLine("Found marked thread. Processing...");
                                aThread.WaitingOnDeferredSystemCall = false;

                                DeferredSystemCalls_CurrentThread = aThread;

                                //BasicConsole.WriteLine("Gathering info...");
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

                                //BasicConsole.Write("Sys call ");
                                //BasicConsole.Write(sysCallNum);
                                //BasicConsole.Write(" : ");
                                //BasicConsole.WriteLine(ProcessManager.CurrentProcess.Name);
                                //BasicConsole.WriteLine(((FOS_System.String)" > Param1: ") + DeferredSystemCalls_CurrentThread.Param1);
                                //BasicConsole.WriteLine(((FOS_System.String)" > Param2: ") + DeferredSystemCalls_CurrentThread.Param2);
                                //BasicConsole.WriteLine(((FOS_System.String)" > Param3: ") + DeferredSystemCalls_CurrentThread.Param3);
                                
                                switch ((SystemCall)sysCallNum)
                                {
                                    case SystemCall.INVALID:
                                        BasicConsole.WriteLine("Error! INVALID (deferred) system call made.");
                                        break;
                                    case SystemCall.PlayNote:
                                        //BasicConsole.WriteLine("PlayNote deferred system calls.");
                                        SysCall_PlayNote((Hardware.Timers.PIT.MusicalNote)param1, (Hardware.Timers.PIT.MusicalNoteValue)param2, param3);
                                        break;
                                    case SystemCall.Semaphore:
                                        //BasicConsole.WriteLine("Semaphore deferred system calls.");
                                        SysCall_Semaphore((SemaphoreRequests)param1, (int)param2, param3);
                                        break;
                                    default:
                                        BasicConsole.WriteLine("Unrecognised deferred system call.");
                                        break;
                                }

                                //BasicConsole.WriteLine(((FOS_System.String)" > Return 1: ") + DeferredSystemCalls_CurrentThread.Return1);
                                //BasicConsole.WriteLine(((FOS_System.String)" > Return 2: ") + DeferredSystemCalls_CurrentThread.Return2);
                                //BasicConsole.WriteLine(((FOS_System.String)" > Return 3: ") + DeferredSystemCalls_CurrentThread.Return3);
                                //BasicConsole.WriteLine(((FOS_System.String)" > Return 4: ") + DeferredSystemCalls_CurrentThread.Return4);

                                DeferredSystemCalls_CurrentThread._Wake();

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

        public enum SemaphoreRequests
        {
            INVALID = 0,
            Allocate = 1,
            Deallocate = 2,
            Wait = 3,
            Signal = 4,
            AddOwner = 5
        }
        public enum SemaphoreResponses
        {
            Error = -1,
            INVALID = 0,
            Success = 1
        }
        private static void SysCall_Semaphore(SemaphoreRequests request, int id, uint limitOrProcessId)
        {
            SemaphoreResponses response = SemaphoreResponses.INVALID;
            switch (request)
            {
                case SemaphoreRequests.INVALID:
                    Console.Default.WriteLine("Error! INVALID semaphore request made.");
                    response = SemaphoreResponses.INVALID;
                    break;
                case SemaphoreRequests.Allocate:
                    //BasicConsole.WriteLine("Allocate Semaphore Request");
                    id = ProcessManager.Semaphore_Allocate((int)limitOrProcessId);
                    //BasicConsole.Write("Allocated id: ");
                    //BasicConsole.WriteLine(id);
                    response = id != -1 ? SemaphoreResponses.Success : SemaphoreResponses.Error;
                    break;
                case SemaphoreRequests.Deallocate:
                    BasicConsole.WriteLine("Deallocate Semaphore Request");
                    response = ProcessManager.Semaphore_Deallocate(id) ? SemaphoreResponses.Success : SemaphoreResponses.Error;
                    break;
                case SemaphoreRequests.Wait:
                    BasicConsole.WriteLine("Wait on a Semaphore Request");
                    response = ProcessManager.Semaphore_Wait(id) ? SemaphoreResponses.Success : SemaphoreResponses.Error;
                    break;
                case SemaphoreRequests.Signal:
                    BasicConsole.WriteLine("Signal a Semaphore Request");
                    response = ProcessManager.Semaphore_Signal(id) ? SemaphoreResponses.Success : SemaphoreResponses.Error;
                    break;
                case SemaphoreRequests.AddOwner:
                    BasicConsole.WriteLine("Add Owner to a Semaphore Request");
                    response = ProcessManager.Semaphore_AddOwner(id, limitOrProcessId) ? SemaphoreResponses.Success : SemaphoreResponses.Error;
                    break;
                default:
                    Console.Default.WriteLine("Error! Unrecognised semaphore request made.");
                    response = SemaphoreResponses.INVALID;
                    break;
            }
            
            DeferredSystemCalls_CurrentThread.Return1 = (uint)response;
            DeferredSystemCalls_CurrentThread.Return2 = (uint)id;
            DeferredSystemCalls_CurrentThread.Return3 = 0;
            DeferredSystemCalls_CurrentThread.Return4 = 0;
        }

    }
}
