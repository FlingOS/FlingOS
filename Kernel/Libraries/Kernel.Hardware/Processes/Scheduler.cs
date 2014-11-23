#define SCHEDULER_TRACE
#undef SCHEDULER_TRACE

using System;

namespace Kernel.Hardware.Processes
{
    [Compiler.PluggedClass]
    public static unsafe class Scheduler
    {
        public enum Priority : int 
        {
            Low = 1,
            Normal = 2,
            High = 5
        }
        
        public static void InitProcess(Process process, Priority priority)
        {
            process.Priority = priority;
        }

        public static void Init()
        {
            //Disable interrupts - critical section
#if SCHEDULER_TRACE
            Console.Default.WriteLine(" > Disabling interrupts...");
#endif
            Disable();

            //Load first process and first thread (ManagedMain process)
#if SCHEDULER_TRACE
            Console.Default.WriteLine(" > Setting current process...");
#endif
            ProcessManager.CurrentProcess = (Process)ProcessManager.Processes[0];
#if SCHEDULER_TRACE
            Console.Default.WriteLine(" > Setting current thread...");
#endif
            ProcessManager.CurrentThread = (Thread)ProcessManager.CurrentProcess.Threads[0];
#if SCHEDULER_TRACE
            Console.Default.WriteLine(" > Setting current thread state...");
#endif
            ProcessManager.CurrentThread_State = ProcessManager.CurrentThread.State;
                        
            //Init TSS
#if SCHEDULER_TRACE
            Console.Default.WriteLine(" > Getting TSS pointer...");
#endif
            TSS* tss = GetTSSPointer();
#if SCHEDULER_TRACE
            Console.Default.WriteLine(" > Setting esp0...");
#endif
            tss->esp0 = (uint)ProcessManager.CurrentThread_State->KernelStackTop;
#if SCHEDULER_TRACE
            Console.Default.WriteLine(" > Setting ss0...");
#endif
            //Note: KM CS is loaded from IDT entry on ring 3 -> 0 switch
            tss->ss0 = 0x10; //Kernel mode stack segment = KM Data segment (same for all processes)
            
#if SCHEDULER_TRACE
            Console.Default.WriteLine(" > Setting cr3...");
#endif
            tss->cr3 = ProcessManager.CurrentProcess.TheMemoryLayout.CR3;

            //Load Task Register
#if SCHEDULER_TRACE
            Console.Default.WriteLine(" > Loading TR...");
#endif
            LoadTR();

            //Enable timer
#if SCHEDULER_TRACE
            Console.Default.WriteLine(" > Adding timer handler...");
#endif
            /*1000000*/
            Hardware.Devices.Timer.Default.RegisterHandler(OnTimerInterrupt, 1000000, true, null);

            Enable();
        }
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\Processes\Scheduler")]
        private static void LoadTR()
        {
        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static TSS* GetTSSPointer()
        {
            return null;
        }

#if SCHEDULER_TRACE
        static bool wasum = false;
        public static bool print = false;
#endif
        [Compiler.NoDebug]
        private static void OnTimerInterrupt(FOS_System.Object state)
        {
#if SCHEDULER_TRACE
            if (print)
            {
                Console.Default.WriteLine("Scheduler interrupt started...");
                Console.Default.WriteLine("Updating sleeping threads...");
            }
#endif
            UpdateSleepingThreads();


#if SCHEDULER_TRACE
            if (print)
            {
                Console.Default.WriteLine("Testing time to sleep...");
            }
#endif
            if (ProcessManager.CurrentThread.TimeToSleep <= 0)
            {
#if SCHEDULER_TRACE
                if (print)
                {
                    Console.Default.WriteLine("Decrementing time to run...");
                }
#endif
                ProcessManager.CurrentThread.TimeToRun--;
            }
#if SCHEDULER_TRACE
            if (print)
            {
                Console.Default.WriteLine("Checking time to run and terminated...");
            }
#endif
            if (ProcessManager.CurrentThread.TimeToRun <= 0 ||
                ProcessManager.CurrentThread.TimeToSleep > 0 ||
                ProcessManager.CurrentThread_State->Terminated)
            {
                #region Decrement Time to Run
#if SCHEDULER_TRACE
                if (print)
                {
                    Console.Default.WriteLine("Reloading time to run...");
                }
#endif
                ProcessManager.CurrentThread.TimeToRun = ProcessManager.CurrentThread.TimeToRunReload;

                #endregion

                #region Next Thread
#if SCHEDULER_TRACE
                    if (print)
                    {
                        Console.Default.WriteLine("Getting current thread index...");
                    }
#endif
                int threadIdx = ProcessManager.CurrentProcess.Threads.IndexOf(ProcessManager.CurrentThread);
#if SCHEDULER_TRACE
                    if (print)
                    {
                        Console.Default.WriteLine("Moving to next thread index...");
                    }
#endif
                threadIdx++;
#if SCHEDULER_TRACE
                    if (print)
                    {
                        Console.Default.WriteLine("Looping threads (1)...");
                        Console.Default.Write("Thread count: ");
                        Console.Default.WriteLine_AsDecimal(ProcessManager.CurrentProcess.Threads.Count);
                    }
#endif
                while (threadIdx < ProcessManager.CurrentProcess.Threads.Count &&
                      (((Thread)ProcessManager.CurrentProcess.Threads[threadIdx]).State->Terminated ||
                       ((Thread)ProcessManager.CurrentProcess.Threads[threadIdx]).TimeToSleep > 0))
                {
#if SCHEDULER_TRACE
                        if (print)
                        {
                            Console.Default.Write("Time to sleep: ");
                            Console.Default.WriteLine_AsDecimal(((Thread)ProcessManager.CurrentProcess.Threads[threadIdx]).TimeToSleep);
                            if (((Thread)ProcessManager.CurrentProcess.Threads[threadIdx]).State->Terminated)
                            {
                                Console.Default.WriteLine("Terminated.");
                            }
                            else
                            {
                                Console.Default.WriteLine("Not terminated.");
                            }
                        }
#endif

                    threadIdx++;
                }

                #endregion

#if SCHEDULER_TRACE
                    if (print)
                    {
                        Console.Default.WriteLine("Getting current process index...");
                    }
#endif
                if (ProcessManager.Processes.Count > 1)
                {
                    #region Next Process

                    int processIdx = ProcessManager.Processes.IndexOf(ProcessManager.CurrentProcess);
#if SCHEDULER_TRACE
                    if (print)
                    {
                        Console.Default.WriteLine("Looping processes...");
                    }
#endif
                    while (threadIdx >= ProcessManager.CurrentProcess.Threads.Count)
                    {

#if SCHEDULER_TRACE
                        if (print)
                        {
                            Console.Default.WriteLine("Moving to next process...");
                        }
#endif
                        processIdx++;
#if SCHEDULER_TRACE
                        if (print)
                        {
                            Console.Default.WriteLine("Checking process count...");
                        }
#endif
                        if (processIdx >= ProcessManager.Processes.Count)
                        {
#if SCHEDULER_TRACE
                            if (print)
                            {
                                Console.Default.WriteLine("Setting process index to first process...");
                            }
#endif
                            //Go back to start of list
                            processIdx = 0;
                        }

#if SCHEDULER_TRACE
                        if (print)
                        {
                            Console.Default.WriteLine("Setting current process...");
                        }
#endif
                        ProcessManager.CurrentProcess = (Process)ProcessManager.Processes[processIdx];

#if SCHEDULER_TRACE
                        if (print)
                        {
                            Console.Default.WriteLine("Process name: " + ProcessManager.CurrentProcess.Name);
                            Console.Default.WriteLine("Setting thread index to first thread...");
                        }
#endif
                        threadIdx = 0;
#if SCHEDULER_TRACE
                        if (print)
                        {
                            Console.Default.WriteLine("Looping threads (2)...");
                            Console.Default.Write("Thread count: ");
                            Console.Default.WriteLine_AsDecimal(ProcessManager.CurrentProcess.Threads.Count);
                        }
#endif
                        while (threadIdx < ProcessManager.CurrentProcess.Threads.Count &&
                              (((Thread)ProcessManager.CurrentProcess.Threads[threadIdx]).State->Terminated ||
                               ((Thread)ProcessManager.CurrentProcess.Threads[threadIdx]).TimeToSleep > 0))
                        {
#if SCHEDULER_TRACE
                            if (print)
                            {
                                Console.Default.Write("Time to sleep: ");
                                Console.Default.WriteLine_AsDecimal(((Thread)ProcessManager.CurrentProcess.Threads[threadIdx]).TimeToSleep);
                                if (((Thread)ProcessManager.CurrentProcess.Threads[threadIdx]).State->Terminated)
                                {
                                    Console.Default.WriteLine("Terminated.");
                                }
                                else
                                {
                                    Console.Default.WriteLine("Not terminated.");
                                }
                            }
#endif

                            threadIdx++;
                        }
                    }

                    #endregion
                }
                else if (threadIdx >= ProcessManager.CurrentProcess.Threads.Count)
                {
                    threadIdx = 0;
                }

                #region  Set current thread and thread state
#if SCHEDULER_TRACE
                if (print)
                {
                    Console.Default.WriteLine("Setting current thread...");
                }
#endif
                ProcessManager.CurrentThread = (Thread)ProcessManager.CurrentProcess.Threads[threadIdx];
#if SCHEDULER_TRACE
                    if (print)
                    {
                        Console.Default.WriteLine("Setting current thread state...");
                    }
#endif
                ProcessManager.CurrentThread_State = ProcessManager.CurrentThread.State;

                #endregion
            }

#if SCHEDULER_TRACE
            if (print)
            {
                if (wasum != ProcessManager.CurrentProcess.UserMode)
                {
                    if (wasum)
                    {
                        Console.Default.WriteLine("Switched to KM process.");
                    }
                    else
                    {
                        Console.Default.WriteLine("Switched to UM process.");
                    }
                }
                wasum = ProcessManager.CurrentProcess.UserMode;

                Console.Default.WriteLine("Checking thread started...");
            }
#endif
            if (!ProcessManager.CurrentThread_State->Started)
            {
                SetupThreadForStart();
            }

#if SCHEDULER_TRACE
            if (print)
            {
                Console.Default.WriteLine("Scheduler interrupt ended.");
                for (int i = 0; i < 5000000; i++)
                {
                    if (i % 500000 == 0)
                    {
                        Console.Default.Write(".");
                    }
                }
                Console.Default.WriteLine();
            }
#endif
        }
        [Compiler.NoDebug]
        private static void UpdateSleepingThreads()
        {
            for (int pIdx = 0; pIdx < ProcessManager.Processes.Count; pIdx++)
            {
                Process p = (Process)ProcessManager.Processes[pIdx];
                for (int tIdx = 0; tIdx < p.Threads.Count; tIdx++)
                {
                    Thread t = (Thread)p.Threads[tIdx];
                    if (t.TimeToSleep > 0)
                    {
#if SCHEDULER_TRACE
                        if (print)
                        {
                            Console.Default.WriteLine("Decrementing sleep counter...");
                        }
#endif
                        t.TimeToSleep--;
                    }
                }
            }
        }
        [Compiler.NoDebug]
        private static void SetupThreadForStart()
        {
#if SCHEDULER_TRACE
            if (print)
            {
                Console.Default.WriteLine("Marking thread as started...");
            }
#endif
            ProcessManager.CurrentThread.TimeToRunReload = (int)ProcessManager.CurrentProcess.Priority;
            ProcessManager.CurrentThread.TimeToRun = ProcessManager.CurrentThread.TimeToRunReload;

            ProcessManager.CurrentThread_State->Started = true;
            ProcessManager.CurrentThread_State->Terminated = false;

#if SCHEDULER_TRACE
            if (print)
            {
                Console.Default.WriteLine("Initialising thread stack...");
            }
#endif
            // Selectors for user-mode must be or'ed with 3 to set privilege level

            uint* stackPtr = (uint*)ProcessManager.CurrentThread_State->ThreadStackTop;
            // Process terminate CS selector
            //      Process terminate should always be jumping into kernel-mode code (for now)
            *stackPtr-- = 8u;
            // Process terminate return pointer
            *stackPtr-- = (uint)Utilities.ObjectUtilities.GetHandle((TerminateMethod)ThreadTerminated);
            if (ProcessManager.CurrentProcess.UserMode)
            {
#if SCHEDULER_TRACE
                if (print)
                {
                    Console.Default.WriteLine("Setting up UM start stack...");
                }
#endif

                uint WantedStackPtr = (uint)stackPtr;
                *stackPtr-- = 0x23;    //64 - SS after switch to UM
                *stackPtr-- = WantedStackPtr; // 60 - ESP after switch to UM
                *stackPtr-- = 0x0202u; // - 56 - IOPL=0
                *stackPtr-- = 0x1Bu;   // CS - 52
                *stackPtr-- = ProcessManager.CurrentThread_State->StartEIP; // - 48

                *stackPtr-- = 0;    //eax - 44
                *stackPtr-- = 0;    //ecx - 40
                *stackPtr-- = 0;    //edx - 36
                *stackPtr-- = 0;    //ebx - 32
                *stackPtr-- = 0xDEADBEEF;    //esp - 28 - This is actually ignored by Popad instruction
                *stackPtr-- = WantedStackPtr; //ebp - 24
                *stackPtr-- = 0;    //esi - 20
                *stackPtr-- = 0;    //edi - 16

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
                    Console.Default.WriteLine("Setting up KM start stack...");
                }
#endif

                *stackPtr-- = 0x0202u; // - IOPL=0
                *stackPtr-- = 0x08u;   // CS
                *stackPtr-- = ProcessManager.CurrentThread_State->StartEIP;

                *stackPtr-- = 0;    //eax
                *stackPtr-- = 0;    //ecx
                *stackPtr-- = 0;    //edx
                *stackPtr-- = 0;    //ebx
                *stackPtr-- = 0xDEADBEEF;    //esp - This is actually ignored by Popad instruction
                *stackPtr-- = (uint)ProcessManager.CurrentThread_State->ThreadStackTop;    //ebp
                *stackPtr-- = 0;    //esi
                *stackPtr-- = 0;    //edi 

                *stackPtr-- = 0x10u; //ds
                *stackPtr-- = 0x10u; //es
                *stackPtr-- = 0x10u; //fs
                *stackPtr = 0x10u; //gs
            }

#if SCHEDULER_TRACE
            if (print)
            {
                Console.Default.WriteLine("Updating thread stack...");
            }
#endif
            ProcessManager.CurrentThread_State->ESP = (uint)stackPtr;

#if SCHEDULER_TRACE
            if (print)
            {
                if (ProcessManager.CurrentProcess.UserMode)
                {
                    Console.Default.WriteLine("Starting UM thread...");
                }
            }
#endif
        }

        private delegate void TerminateMethod();
        private static void ThreadTerminated()
        {
            Disable();

#if SCHEDULER_TRACE
            // START - Trace code
            
            Console.Default.WriteLine("Thread terminated.");
            Console.Default.WriteLine("Process Name: " + ProcessManager.CurrentProcess.Name + ", Thread Id: " + ProcessManager.CurrentThread.Id);

            //Enable();

            //for (int i = 0; i < 3; i++)
            //{
            //    Console.Default.Write(".");
            //    Thread.Sleep(1000);
            //}
            // END - Trace code

            //Disable();
#endif

            // Mark thread as terminated. Leave it to the scheduler to stop running
            //  and the process manager can destroy it later.
            ProcessManager.CurrentThread_State->Terminated = true;

            Enable();

            //Wait for the scheduler to interrupt us. We will never return here again (inside this thread)
            //  since it has now been terminated.
            while (true)
            {
#if SCHEDULER_TRACE
                Console.Default.WriteLine("Still running!");
#endif
            }
        }

        [Compiler.NoDebug]
        public static void Enable()
        {
            Hardware.Interrupts.Interrupts.EnableInterrupts();
        }
        [Compiler.NoDebug]
        public static void Disable()
        {
            Hardware.Interrupts.Interrupts.DisableInterrupts();
        }
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct TSS
    {
        /* For obvious reasons, do not reorder the fields. */

        public ushort backlink, __blh;
        public uint esp0;
        public ushort ss0, __ss0h;
        public uint esp1;
        public ushort ss1, __ss1h;
        public uint esp2;
        public ushort ss2, __ss2h;
        public uint cr3;
        public uint eip;
        public uint eflags;
        public uint eax, ecx, edx, ebx;
        public uint esp, ebp, esi, edi;
        public ushort es, __esh;
        public ushort cs, __csh;
        public ushort ss, __ssh;
        public ushort ds, __dsh;
        public ushort fs, __fsh;
        public ushort gs, __gsh;
        public ushort ldt, __ldth;
        public ushort trace, bitmap;
    }
}
