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

//#define SCHEDULER_TRACE
//#define SCHEDULER_HANDLER_TRACE
//#define SCHEDULER_HANDLER_MIN_TRACE
//#define SCHEDULER_UPDATE_LIST_TRACE

using Drivers.Compiler.Attributes;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Utilities;
using Kernel.VirtualMemory.Implementations;

namespace Kernel.Multiprocessing.Scheduling
{
    public unsafe class PriorityQueueScheduler : Object, IScheduler
    {
        [Group(Name = "IsolatedKernel_Hardware_Multiprocessing")] private static PriorityQueueScheduler ThePQScheduler;

        private readonly PriorityQueue ActiveQueue = new PriorityQueue(1024);

        private readonly PriorityQueue InactiveQueue = new PriorityQueue(1024);

        //private List SuspendedList = new List(1024);

        private bool Enabled;
        private bool Started;

        private const int MSFreq = 5;
        private readonly int UpdatePeriod = MSFreq;
        private int UpdateCountdown;

        public long PreemptionPeriod
        {
            get
            {
                /*Multiply by 1000000 to get from ms to ns*/
                return MSFreq*1000000;
            }
        }

        //private int LockupCounter = 0;

        public void InitProcess(Process process, Scheduler.Priority priority)
        {
            process.Priority = priority;

            for (int i = 0; i < process.Threads.Count; i++)
            {
                Thread t = (Thread)process.Threads[i];

                InitThread(process, t);
            }

            process.Registered = true;
        }

        public void InitThread(Process process, Thread t)
        {
            t.TimeToRunReload = (int)process.Priority;
            t.TimeToRun = t.TimeToRunReload;

            UpdateList(t, false);
        }

        [NoDebug]
        public void Init()
        {
            ThePQScheduler = this;

            ActiveQueue.Name = "Active Queue";
            InactiveQueue.Name = "Inactive Queue";
        }

        [NoDebug]
        public PreemptionHandler Start()
        {
            //Load first process and first thread
#if SCHEDULER_TRACE
            BasicConsole.WriteLine(" > Setting current process...");
#endif
            ProcessManager.CurrentProcess = (Process)ProcessManager.Processes[0];
#if SCHEDULER_TRACE
            BasicConsole.WriteLine(" > Setting current thread...");
#endif
            ProcessManager.CurrentThread = (Thread)ProcessManager.CurrentProcess.Threads[0];
#if SCHEDULER_TRACE
            BasicConsole.WriteLine(" > Setting current thread state...");
#endif
            ProcessManager.CurrentThread_State = ProcessManager.CurrentThread.State;

            //Init TSS
#if SCHEDULER_TRACE
            BasicConsole.WriteLine(" > Getting TSS pointer...");
#endif
            TSS* tss = Scheduler.GetTSSPointer();
#if SCHEDULER_TRACE
            BasicConsole.WriteLine(" > Setting esp0...");
#endif
            tss->esp0 = (uint)ProcessManager.CurrentThread_State->KernelStackTop;
#if SCHEDULER_TRACE
            BasicConsole.WriteLine(" > Setting ss0...");
#endif
            //Note: KM CS is loaded from IDT entry on ring 3 -> 0 switch
            tss->ss0 = 0x10; //Kernel mode stack segment = KM Data segment (same for all processes)

#if SCHEDULER_TRACE
            BasicConsole.WriteLine(" > Setting cr3...");
#endif
            tss->cr3 = X86VirtualMemoryImplementation.GetCR3();

            //Load Task Register
#if SCHEDULER_TRACE
            BasicConsole.WriteLine(" > Loading TR...");
#endif
            Scheduler.LoadTR();

#if SCHEDULER_TRACE
            BasicConsole.WriteLine(" > Enabling process switching...");
#endif
            Interrupts.Interrupts.EnableProcessSwitching = true;

            //#if SCHEDULER_TRACE
            BasicConsole.WriteLine(" > Enabling scheduler...");
            //#endif
            UpdateCountdown = UpdatePeriod;

            Started = true;

            return OnTimerInterrupt;
        }

        [NoDebug]
        [NoGC]
        public void HandlePageFault(uint eip, uint errorCode, uint address)
        {
            /*Hardware.VirtMem.MemoryLayout memLayout = ProcessManager.CurrentProcess.TheMemoryLayout;
            BasicConsole.WriteLine("Code pages:");
            Framework.String TempDisplayString = "0x        ";

            BasicConsole.WriteLine("Get iterator");
            UInt32Dictionary.Iterator iterator = memLayout.CodePages.GetIterator();
            BasicConsole.WriteLine("Check for next");
            while(iterator.HasNext())
            {
                BasicConsole.WriteLine("Get pair");
                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                BasicConsole.WriteLine("Get key");
                uint vAddr = pair.Key;
                BasicConsole.WriteLine("Setup string");
                WriteNumber(TempDisplayString, vAddr);
                BasicConsole.WriteLine(TempDisplayString);
            }
            BasicConsole.WriteLine("Data pages:");
            iterator = memLayout.DataPages.GetIterator();
            while (iterator.HasNext())
            {
                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                uint vAddr = pair.Key;
                WriteNumber(TempDisplayString, vAddr);
                BasicConsole.WriteLine(TempDisplayString);
            }*/

            BasicConsole.WriteLine(ProcessManager.CurrentProcess.Name);
            BasicConsole.WriteLine(ProcessManager.CurrentThread.Name);

            BasicConsole.WriteLine("Suspending current thread.");
            ProcessManager.CurrentThread.Suspend = true;
        }

        [NoDebug]
        [NoGC]
        private void WriteNumber(String str, uint val)
        {
            int offset = 9;

            #region Address

            while (offset > 1)
            {
                uint rem = val & 0xFu;
                switch (rem)
                {
                    case 0:
                        str[offset] = '0';
                        break;
                    case 1:
                        str[offset] = '1';
                        break;
                    case 2:
                        str[offset] = '2';
                        break;
                    case 3:
                        str[offset] = '3';
                        break;
                    case 4:
                        str[offset] = '4';
                        break;
                    case 5:
                        str[offset] = '5';
                        break;
                    case 6:
                        str[offset] = '6';
                        break;
                    case 7:
                        str[offset] = '7';
                        break;
                    case 8:
                        str[offset] = '8';
                        break;
                    case 9:
                        str[offset] = '9';
                        break;
                    case 10:
                        str[offset] = 'A';
                        break;
                    case 11:
                        str[offset] = 'B';
                        break;
                    case 12:
                        str[offset] = 'C';
                        break;
                    case 13:
                        str[offset] = 'D';
                        break;
                    case 14:
                        str[offset] = 'E';
                        break;
                    case 15:
                        str[offset] = 'F';
                        break;
                }
                val >>= 4;
                offset--;
            }

            #endregion
        }

#if SCHEDULER_TRACE
        bool wasum = false;
        public bool print = false;
#endif

        [NoDebug]
        [NoGC]
        private static void OnTimerInterrupt(Object state)
        {
#if SCHEDULER_HANDLER_TRACE || SCHEDULER_HANDLER_MIN_TRACE
            BasicConsole.Write("T");
#endif

            //LockupCounter++;

            //if (LockupCounter > 2000)
            //{
            //    Enable();
            //}

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 3");
            //}

            if (!((PriorityQueueScheduler)state).Enabled || !((PriorityQueueScheduler)state).Started)
            {
                //BasicConsole.Write("D");
                return;
            }

            //LockupCounter = 0;

#if SCHEDULER_HANDLER_TRACE || SCHEDULER_HANDLER_MIN_TRACE
            BasicConsole.Write("E");
#endif

            //UpdateCountdown -= MSFreq;

            //if (UpdateCountdown <= 0)
            //{
            //    UpdateCountdown = UpdatePeriod;

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 4");
            //}

            ((PriorityQueueScheduler)state).UpdateCurrentState();
            //}
        }

        [NoDebug]
        [NoGC]
        public void UpdateCurrentState()
        {
#if SCHEDULER_HANDLER_TRACE
            BasicConsole.SetTextColour(BasicConsole.warning_colour);

            if (Processes.ProcessManager.Processes.Count > 1)
                BasicConsole.WriteLine("Scheduler interrupt started...");
#endif

            if (ProcessManager.CurrentProcess == null ||
                ProcessManager.CurrentThread == null ||
                ProcessManager.CurrentThread_State == null)
            {
                return;
            }

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 5");
            //}

            UpdateInactiveThreads();

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 6");
            //}

            UpdateActiveThreads();

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 7");
            //}

            while (ActiveQueue.Count == 0)
            {
#if SCHEDULER_HANDLER_TRACE || SCHEDULER_HANDLER_MIN_TRACE
    //BasicConsole.WriteLine("WARNING: Scheduler preventing infinite loop by early-updating sleeping threads.");
#endif
                UpdateInactiveThreads();
            }

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 8");
            //}

            Thread nextThread = (Thread)ActiveQueue.PeekMin();
#if SCHEDULER_HANDLER_TRACE || SCHEDULER_HANDLER_MIN_TRACE
            BasicConsole.Write("Active: ");
            BasicConsole.Write(nextThread.Owner.Name);
            BasicConsole.Write(" - ");
            BasicConsole.WriteLine(nextThread.Name);
#endif
            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9");
            //}

            ProcessManager.SwitchProcess(nextThread.Owner.Id, (int)nextThread.Id);

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 10");
            //}

            if (!ProcessManager.CurrentThread_State->Started)
            {
                //if (Processes.Scheduler.OutputMessages)
                //{
                //    BasicConsole.WriteLine("Debug Point 11");
                //}

                SetupThreadForStart();
            }

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 12");
            //}

#if SCHEDULER_HANDLER_TRACE
            if (Processes.ProcessManager.Processes.Count > 1)
                BasicConsole.WriteLine("Scheduler interrupt ended.");

            BasicConsole.SetTextColour(BasicConsole.default_colour);
#endif
        }

        //[Drivers.Compiler.Attributes.NoDebug]
        //[Drivers.Compiler.Attributes.NoGC]
        //private void NextProcess(ref int threadIdx, ref int processIdx)
        //{
        //    processIdx++;

        //    if (processIdx >= ProcessManager.Processes.Count)
        //    {
        //        //Go back to start of list
        //        processIdx = 0;
        //    }

        //    threadIdx = NextThread(ProcessManager.THREAD_DONT_CARE, processIdx);
        //}
        //[Drivers.Compiler.Attributes.NoDebug]
        //[Drivers.Compiler.Attributes.NoGC]
        //private int NextThread(int threadIdx, int processIdx)
        //{
        //    threadIdx++;

        //    Process cProcess = ((Process)ProcessManager.Processes[processIdx]);

        //    while (threadIdx < cProcess.Threads.Count &&
        //          (((Thread)cProcess.Threads[threadIdx]).State->Terminated ||
        //           ((Thread)cProcess.Threads[threadIdx]).TimeToSleep != 0 ||
        //           ((Thread)cProcess.Threads[threadIdx]).Debug_Suspend))
        //    {
        //        threadIdx++;
        //    }
        //    return threadIdx;
        //}
        [NoDebug]
        [NoGC]
        private void UpdateActiveThreads()
        {
#if SCHEDULER_HANDLER_TRACE
            BasicConsole.WriteLine("Scheduler > Updating active threads...");
            Framework.String NumberOfActiveThreadsStr = "Scheduler > Number of active threads: 0x        ";
            ExceptionMethods.FillString((uint)ActiveQueue.Count, 47, NumberOfActiveThreadsStr);
            BasicConsole.WriteLine(NumberOfActiveThreadsStr);

            BasicConsole.WriteLine("Scheduler > Peeking min active thread...");
#endif
            Thread minThread = (Thread)ActiveQueue.PeekMin();
            if (minThread != null &&
                minThread.Key == 0)
            {
#if SCHEDULER_HANDLER_TRACE
                BasicConsole.WriteLine("Scheduler > Extracting min active thread...");
#endif
                ActiveQueue.ExtractMin();
#if SCHEDULER_HANDLER_TRACE
                BasicConsole.WriteLine("Scheduler > Setting time to run of min active thread...");
#endif
                minThread.TimeToRun = minThread.TimeToRunReload;
#if SCHEDULER_HANDLER_TRACE
                BasicConsole.WriteLine("Scheduler > Re-inserting active thread...");
#endif
                ActiveQueue.Insert(minThread);
            }

#if SCHEDULER_HANDLER_TRACE
            BasicConsole.WriteLine("Scheduler > Decreasing keys of all active threads...");
#endif
            ActiveQueue.DecreaseAllKeys(1, 0);

#if SCHEDULER_HANDLER_TRACE
            BasicConsole.WriteLine("Scheduler > Updated active threads.");
#endif
        }

        [NoDebug]
        [NoGC]
        private void UpdateInactiveThreads()
        {
#if SCHEDULER_HANDLER_TRACE
            BasicConsole.WriteLine("Scheduler > Updating inactive threads...");
            Framework.String NumberOfInactiveThreadsStr = "Scheduler > Number of inactive threads: 0x        ";
            ExceptionMethods.FillString((uint)InactiveQueue.Count, 49, NumberOfInactiveThreadsStr);
            BasicConsole.WriteLine(NumberOfInactiveThreadsStr);

            BasicConsole.WriteLine("Scheduler > Decreasing keys of all inactive threads...");
#endif
            InactiveQueue.DecreaseAllKeys(UpdatePeriod, 0);

#if SCHEDULER_HANDLER_TRACE
            BasicConsole.WriteLine("Scheduler > Peeking min inactive thread...");
#endif
            Thread cThread = (Thread)InactiveQueue.PeekMin();
            while (cThread != null && cThread.ActiveState != Thread.ActiveStates.Inactive)
            {
#if SCHEDULER_HANDLER_TRACE
                BasicConsole.WriteLine("Scheduler > Extracting min inactive thread...");
#endif
                InactiveQueue.ExtractMin();

#if SCHEDULER_HANDLER_TRACE
                BasicConsole.WriteLine("Scheduler > Updating min inactive thread's list...");
#endif
                UpdateList(cThread, true);

#if SCHEDULER_HANDLER_TRACE
                BasicConsole.WriteLine("Scheduler > Moving to next min inactive thread...");
#endif
                cThread = (Thread)InactiveQueue.PeekMin();
            }

#if SCHEDULER_HANDLER_TRACE
            BasicConsole.WriteLine("Scheduler > Updated inactive threads.");
#endif
        }

        [NoDebug]
        private void SetupThreadForStart()
        {
#if SCHEDULER_TRACE
            if (print)
            {
                BasicConsole.WriteLine("Marking thread as started...");
            }
#endif
            ProcessManager.CurrentThread_State->Started = true;
            ProcessManager.CurrentThread_State->Terminated = false;

#if SCHEDULER_TRACE
            if (print)
            {
                BasicConsole.WriteLine("Initialising thread stack...");
            }
#endif
            // Selectors for user-mode must be or'ed with 3 to set privilege level

            uint* stackPtr = (uint*)ProcessManager.CurrentThread_State->ThreadStackTop;
            
            uint[] args = ProcessManager.CurrentThread.StartArgs;
            for (int i = 0; i < args.Length; i++)
            {
                *stackPtr-- = args[i];
            }

            // Process terminate CS selector
            //      Process terminate should always be jumping into kernel-mode code (for now)
            *stackPtr-- = 8u;
            // Process terminate return pointer
            *stackPtr-- = (uint)ObjectUtilities.GetHandle((TerminateMethod)ThreadTerminated);
            if (ProcessManager.CurrentProcess.UserMode)
            {
#if SCHEDULER_TRACE
                if (print)
                {
                    BasicConsole.WriteLine("Setting up UM start stack...");
                }
#endif

                uint WantedStackPtr = (uint)stackPtr;
                *stackPtr-- = 0x23; //64 - SS after switch to UM
                *stackPtr-- = WantedStackPtr; // 60 - ESP after switch to UM
                *stackPtr-- = 0x0202u; // - 56 - Reserved=1, Interrupt Enable=1, IOPL=0
                *stackPtr-- = 0x1Bu; // CS - 52
                *stackPtr-- = ProcessManager.CurrentThread_State->StartEIP; // - 48

                *stackPtr-- = 0; //eax - 44
                *stackPtr-- = 0; //ecx - 40
                *stackPtr-- = 0; //edx - 36
                *stackPtr-- = 0; //ebx - 32
                *stackPtr-- = 0xDEADBEEF; //esp - 28 - This is actually ignored by Popad instruction
                *stackPtr-- = WantedStackPtr; //ebp - 24
                *stackPtr-- = 0; //esi - 20
                *stackPtr-- = 0; //edi - 16

                *stackPtr-- = 0x23u; //ds - 12
                *stackPtr-- = 0x23u; //es - 8
                *stackPtr-- = 0x23u; //fs - 4
                *stackPtr = 0x23u; //gs - 0
            }
            else
            {
#if SCHEDULER_TRACE
                if (print)
                {
                    BasicConsole.WriteLine("Setting up KM start stack...");
                }
#endif

                *stackPtr-- = 0x0202u; // - IOPL=0
                *stackPtr-- = 0x08u; // CS
                *stackPtr-- = ProcessManager.CurrentThread_State->StartEIP;

                *stackPtr-- = 0; //eax
                *stackPtr-- = 0; //ecx
                *stackPtr-- = 0; //edx
                *stackPtr-- = 0; //ebx
                *stackPtr-- = 0xDEADBEEF; //esp - This is actually ignored by Popad instruction
                *stackPtr-- = (uint)ProcessManager.CurrentThread_State->ThreadStackTop; //ebp
                *stackPtr-- = 0; //esi
                *stackPtr-- = 0; //edi 

                *stackPtr-- = 0x10u; //ds
                *stackPtr-- = 0x10u; //es
                *stackPtr-- = 0x10u; //fs
                *stackPtr = 0x10u; //gs
            }

#if SCHEDULER_TRACE
            if (print)
            {
                BasicConsole.WriteLine("Updating thread stack...");
            }
#endif
            ProcessManager.CurrentThread_State->ESP = (uint)stackPtr;

#if SCHEDULER_TRACE
            if (print)
            {
                if (ProcessManager.CurrentProcess.UserMode)
                {
                    BasicConsole.WriteLine("Starting UM thread...");
                }
            }
#endif
        }

        private delegate void TerminateMethod();

        private static void ThreadTerminated()
        {
            //#if SCHEDULER_TRACE
            // START - Trace code

            BasicConsole.WriteLine("Thread terminated.");
            BasicConsole.WriteLine("Process Name: " + ProcessManager.CurrentProcess.Name + ", Thread Id: " +
                                   ProcessManager.CurrentThread.Id);

            //if(reenable)
            //{
            //    Enable();
            //}

            //for (int i = 0; i < 3; i++)
            //{
            //    BasicConsole.Write(".");
            //    SystemCalls.SleepThread(1000);
            //}
            // END - Trace code

            //if(reenable)
            //{
            //    Disable();
            //}
            //#endif

            // Mark thread as terminated. Leave it to the scheduler to stop running
            //  and the process manager can destroy it later.
            ProcessManager.CurrentThread.LastActiveState = ProcessManager.CurrentThread.ActiveState;
            ProcessManager.CurrentThread_State->Terminated = true;
            ThePQScheduler.UpdateList(ProcessManager.CurrentThread);

            //Wait for the scheduler to interrupt us. We will never return here again (inside this thread)
            //  since it has now been terminated.
            while (true)
            {
                //#if SCHEDULER_TRACE
                BasicConsole.WriteLine("Still running!");
                //#endif
            }
        }

        [NoDebug]
        [NoGC]
        public void Enable()
        {
            //BasicConsole.WriteLine("Enabling scheduler...");
            //BasicConsole.DelayOutput(1);
            Enabled = true;
            //Hardware.Interrupts.Interrupts.EnableInterrupts();
        }

        [NoDebug]
        [NoGC]
        public void Disable( /*Framework.String disabler*/)
        {
            //Hardware.Interrupts.Interrupts.DisableInterrupts();
            Enabled = false;
            //BasicConsole.Write("Disabled scheduler: ");
            //BasicConsole.WriteLine(disabler);
            //BasicConsole.DelayOutput(1);
        }

        [NoDebug]
        [NoGC]
        public bool IsEnabled()
        {
            return Enabled;
        }

        public void UpdateList(Thread t)
        {
            UpdateList(t, false);
        }

        private void UpdateList(Thread t, bool skipRemove)
        {
            Disable( /*"Scheduler UpdateList"*/);

#if SCHEDULER_UPDATE_LIST_TRACE
            BasicConsole.Write("S > UpdateList: ");
            BasicConsole.WriteLine(t.Name);
#endif
            if (!skipRemove)
            {
#if SCHEDULER_UPDATE_LIST_TRACE
                    BasicConsole.WriteLine("S > UpdateList: No skip remove");
#endif
                switch (t.LastActiveState)
                {
                    case Thread.ActiveStates.NotStarted:
#if SCHEDULER_UPDATE_LIST_TRACE
                            BasicConsole.WriteLine("S > UpdateList: LastActiveState: NotStarted");
#endif
                        ActiveQueue.Delete(t);
                        break;
                    case Thread.ActiveStates.Terminated:
#if SCHEDULER_UPDATE_LIST_TRACE
                            BasicConsole.WriteLine("S > UpdateList: LastActiveState: Terminated");
#endif
                        break;
                    case Thread.ActiveStates.Active:
#if SCHEDULER_TRACE
                        BasicConsole.WriteLine("Scheduler > Deleting thread from Active queue...");
#endif
                        //try
                        //{
#if SCHEDULER_UPDATE_LIST_TRACE
                                BasicConsole.WriteLine("S > UpdateList: LastActiveState: Active (1x1)");
#endif
                        ActiveQueue.Delete(t);
#if SCHEDULER_UPDATE_LIST_TRACE
                                BasicConsole.WriteLine("S > UpdateList: LastActiveState: Active - done.");
#endif
                        //}
                        //catch
                        //{
                        //    BasicConsole.WriteLine("Error removing from active list:");
                        //    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                        //}
                        break;
                    case Thread.ActiveStates.Inactive:
#if SCHEDULER_UPDATE_LIST_TRACE
                            BasicConsole.WriteLine("S > UpdateList: LastActiveState: Inactive");
#endif
                        InactiveQueue.Delete(t);
                        break;
                    case Thread.ActiveStates.Suspended:
#if SCHEDULER_UPDATE_LIST_TRACE
                            BasicConsole.WriteLine("S > UpdateList: LastActiveState: Suspended");
#endif
                        //SuspendedList.Remove(t);
                        break;
                }
            }

#if SCHEDULER_UPDATE_LIST_TRACE
                BasicConsole.WriteLine("S > UpdateList: Handling active state");
#endif
            switch (t.ActiveState)
            {
                case Thread.ActiveStates.NotStarted:
#if SCHEDULER_UPDATE_LIST_TRACE
                        BasicConsole.WriteLine("S > UpdateList: ActiveState: Not Started");
#endif
                    //if (!ActiveQueue.Insert(t))
                    //{
                    //    BasicConsole.WriteLine(t.Name);
                    //}
                    ActiveQueue.Insert(t);
                    break;
                case Thread.ActiveStates.Terminated:
#if SCHEDULER_UPDATE_LIST_TRACE
                        BasicConsole.WriteLine("S > UpdateList: ActiveState: Terminated");
#endif
                    break;
                case Thread.ActiveStates.Active:
#if SCHEDULER_UPDATE_LIST_TRACE
                        BasicConsole.WriteLine("S > UpdateList: ActiveState: Active");
#endif
                    //if (!ActiveQueue.Insert(t))
                    //{
                    //    BasicConsole.WriteLine(t.Name);
                    //}
                    ActiveQueue.Insert(t);
                    break;
                case Thread.ActiveStates.Inactive:
#if SCHEDULER_UPDATE_LIST_TRACE
                        BasicConsole.WriteLine("S > UpdateList: ActiveState: Inactive");
#endif
                    //if (!InactiveQueue.Insert(t))
                    //{
                    //    BasicConsole.WriteLine(t.Name);
                    //}
                    InactiveQueue.Insert(t);
                    break;
                case Thread.ActiveStates.Suspended:
#if SCHEDULER_UPDATE_LIST_TRACE
                        BasicConsole.WriteLine("S > UpdateList: ActiveState: Suspended");
#endif
                    //SuspendedList.Add(t);
                    break;
            }
#if SCHEDULER_UPDATE_LIST_TRACE
                BasicConsole.WriteLine("S > UpdateList: Done.");
#endif
            Enable();
        }
    }
}