#define SCHEDULER_TRACE
#undef SCHEDULER_TRACE

using System;

namespace Kernel.Core.Processes
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
            tss->ss0 = ProcessManager.CurrentThread_State->SS;
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
            Hardware.Devices.Timer.Default.RegisterHandler(OnTimerInterrupt, 1000000, true, null);

            Enable();
        }
        [Compiler.PluggedMethod(ASMFilePath=@"ASM\Processes\Scheduler")]
        private static void LoadTR()
        {
        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static TSS* GetTSSPointer()
        {
            return null;
        }
        
        private static void OnTimerInterrupt(FOS_System.Object state)
        {
#if SCHEDULER_TRACE
            Console.Default.WriteLine("Scheduler interrupt started...");
#endif

#if SCHEDULER_TRACE
            Console.Default.WriteLine("Updating sleeping threads...");
#endif
            UpdateSleepingThreads();


#if SCHEDULER_TRACE
            Console.Default.WriteLine("Testing time to sleep...");
#endif
            if (ProcessManager.CurrentThread.TimeToSleep <= 0)
            {
#if SCHEDULER_TRACE
                Console.Default.WriteLine("Decrementing time to run...");
#endif
                ProcessManager.CurrentThread.TimeToRun--;
            }
#if SCHEDULER_TRACE
            Console.Default.WriteLine("Checking time to run and terminated...");
#endif
            if (ProcessManager.CurrentThread.TimeToRun <= 0 ||
                ProcessManager.CurrentThread.TimeToSleep > 0 ||
                ProcessManager.CurrentThread_State->Terminated)
            {
#if SCHEDULER_TRACE
                Console.Default.WriteLine("Reloading time to run...");
#endif
                ProcessManager.CurrentThread.TimeToRun = ProcessManager.CurrentThread.TimeToRunReload;

#if SCHEDULER_TRACE
                Console.Default.WriteLine("Getting current thread index...");
#endif
                int threadIdx = ProcessManager.CurrentProcess.Threads.IndexOf(ProcessManager.CurrentThread);
#if SCHEDULER_TRACE
                Console.Default.WriteLine("Moving to next thread index...");
#endif
                threadIdx++;
#if SCHEDULER_TRACE
                Console.Default.WriteLine("Looping threads (1)...");
                Console.Default.Write("Thread count: ");
                Console.Default.WriteLine_AsDecimal(ProcessManager.CurrentProcess.Threads.Count);
#endif
                while (threadIdx < ProcessManager.CurrentProcess.Threads.Count &&
                      (((Thread)ProcessManager.CurrentProcess.Threads[threadIdx]).State->Terminated ||
                       ((Thread)ProcessManager.CurrentProcess.Threads[threadIdx]).TimeToSleep > 0))
                {
#if SCHEDULER_TRACE
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
#endif

                    threadIdx++;
                }

#if SCHEDULER_TRACE
                Console.Default.WriteLine("Getting current process index...");
#endif
                int processIdx = ProcessManager.Processes.IndexOf(ProcessManager.CurrentProcess);
#if SCHEDULER_TRACE
                Console.Default.WriteLine("Looping processes...");
#endif
                while (threadIdx >= ProcessManager.CurrentProcess.Threads.Count)
                {

#if SCHEDULER_TRACE
                    Console.Default.WriteLine("Moving to next process...");
#endif
                    processIdx++;
#if SCHEDULER_TRACE
                    Console.Default.WriteLine("Checking process count...");
#endif
                    if (processIdx >= ProcessManager.Processes.Count)
                    {
#if SCHEDULER_TRACE
                        Console.Default.WriteLine("Setting process index to first process...");
#endif
                        //Go back to start of list
                        processIdx = 0;
                    }

#if SCHEDULER_TRACE
                    Console.Default.WriteLine("Setting current process...");
#endif
                    ProcessManager.CurrentProcess = (Process)ProcessManager.Processes[processIdx];

#if SCHEDULER_TRACE
                    Console.Default.WriteLine("Process name: " + ProcessManager.CurrentProcess.Name);
                    Console.Default.WriteLine("Setting thread index to first thread...");
#endif
                    threadIdx = 0;
#if SCHEDULER_TRACE
                    Console.Default.WriteLine("Looping threads (2)...");
                    Console.Default.Write("Thread count: ");
                    Console.Default.WriteLine_AsDecimal(ProcessManager.CurrentProcess.Threads.Count);
#endif
                    while (threadIdx < ProcessManager.CurrentProcess.Threads.Count &&
                          (((Thread)ProcessManager.CurrentProcess.Threads[threadIdx]).State->Terminated ||
                           ((Thread)ProcessManager.CurrentProcess.Threads[threadIdx]).TimeToSleep > 0))
                    {
#if SCHEDULER_TRACE
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
#endif

                        threadIdx++;
                    }
                }

#if SCHEDULER_TRACE
                Console.Default.WriteLine("Setting current thread...");
#endif
                ProcessManager.CurrentThread = (Thread)ProcessManager.CurrentProcess.Threads[threadIdx];
#if SCHEDULER_TRACE
                Console.Default.WriteLine("Setting current thread state...");
#endif
                ProcessManager.CurrentThread_State = ProcessManager.CurrentThread.State;
            }

#if SCHEDULER_TRACE
            Console.Default.WriteLine("Checking thread started...");
#endif
            if (!ProcessManager.CurrentThread_State->Started)
            {
                SetupThreadForStart();
            }
            
#if SCHEDULER_TRACE
            Console.Default.WriteLine("Scheduler interrupt ended.");
            for (int i = 0; i < 5000000; i++)
            {
                if (i % 500000 == 0)
                {
                    Console.Default.Write(".");
                }
            }
            Console.Default.WriteLine();
#endif
        }
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
                        Console.Default.WriteLine("Decrementing sleep counter...");
#endif
                        t.TimeToSleep--;
                    }
                }
            }
        }
        private static void SetupThreadForStart()
        {
#if SCHEDULER_TRACE
                Console.Default.WriteLine("Marking thread as started...");
#endif
            ProcessManager.CurrentThread.TimeToRunReload = (int)ProcessManager.CurrentProcess.Priority;
            ProcessManager.CurrentThread.TimeToRun = ProcessManager.CurrentThread.TimeToRunReload;

            ProcessManager.CurrentThread_State->Started = true;
            ProcessManager.CurrentThread_State->Terminated = false;

#if SCHEDULER_TRACE
                Console.Default.WriteLine("Initialising thread stack...");
#endif
            uint* stackPtr = (uint*)ProcessManager.CurrentThread_State->ThreadStackTop;
            // Process terminate CS selector
            *stackPtr-- = 0x08;
            // Process terminate return pointer
            *stackPtr-- = (uint)Utilities.ObjectUtilities.GetHandle((TerminateMethod)ThreadTerminated);
            *stackPtr-- = 0x0202; // EFLAGS - IF and mandatory bit set
            *stackPtr-- = 0x08;   // CS
            *stackPtr-- = ProcessManager.CurrentThread_State->StartEIP;
            *stackPtr-- = 0;    //eax
            *stackPtr-- = 0;    //ecx
            *stackPtr-- = 0;    //edx
            *stackPtr-- = 0;    //ebx
            *stackPtr-- = (uint)ProcessManager.CurrentThread_State->ThreadStackTop;    //esp
            *stackPtr-- = (uint)ProcessManager.CurrentThread_State->ThreadStackTop;    //ebp
            *stackPtr-- = 0;    //esi
            *stackPtr-- = 0;    //edi 
            *stackPtr-- = 0x10; //ds
            *stackPtr-- = 0x10; //es
            *stackPtr-- = 0x10; //fs
            *stackPtr = 0x10; //gs

#if SCHEDULER_TRACE
                Console.Default.WriteLine("Updating thread stack...");
#endif
            ProcessManager.CurrentThread_State->ESP = (uint)stackPtr;
        }

        private delegate void TerminateMethod();
        private static void ThreadTerminated()
        {
            
#if SCHEDULER_TRACE
            // START - Trace code
            Disable();
            
            Console.Default.WriteLine("Thread terminated.");
            Console.Default.WriteLine("Process Name: " + ProcessManager.CurrentProcess.Name + ", Thread Id: " + ProcessManager.CurrentThread.Id);

            Enable();

            for (int i = 0; i < 3; i++)
            {
                Console.Default.Write(".");
                Thread.Sleep(1000);
            }
            // END - Trace code
#endif

            Disable();
            
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

        public static void Enable()
        {
            Hardware.Interrupts.Interrupts.EnableInterrupts();
        }
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
