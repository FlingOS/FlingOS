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
            Normal = 5,
            High = 1000
        }

        public static void InitProcess(Process process, Priority priority)
        {
            process.Priority = priority;

            for (int i = 0; i < process.Threads.Count; i++)
            {
                Thread thread = ((Thread)process.Threads[i]);
                thread.TimeToRun = thread.TimeToRunReload = (int)priority;
            }
        }

        public static void Init()
        {
            //Disable interrupts - critical section
#if SCHEDULER_TRACE
            BasicConsole.WriteLine(" > Disabling interrupts...");
#endif
            Hardware.Interrupts.Interrupts.DisableInterrupts();

            //Load first process and first thread (ManagedMain process)
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
            TSS* tss = GetTSSPointer();
#if SCHEDULER_TRACE
            BasicConsole.WriteLine(" > Setting esp0...");
#endif
            tss->esp0 = (uint)ProcessManager.CurrentThread_State->KernelStackTop;
#if SCHEDULER_TRACE
            BasicConsole.WriteLine(" > Setting ss0...");
#endif
            tss->ss0 = ProcessManager.CurrentThread_State->SS;
#if SCHEDULER_TRACE
            BasicConsole.WriteLine(" > Setting cr3...");
#endif
            tss->cr3 = ProcessManager.CurrentProcess.TheMemoryLayout.CR3;

            //Load Task Register
#if SCHEDULER_TRACE
            BasicConsole.WriteLine(" > Loading TR...");
#endif
            LoadTR();

            //Enable timer
#if SCHEDULER_TRACE
            BasicConsole.WriteLine(" > Adding timer handler...");
#endif
            Hardware.Devices.Timer.Default.RegisterHandler(OnTimerInterrupt, 100, true, null);

            Hardware.Interrupts.Interrupts.EnableInterrupts();
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
            BasicConsole.WriteLine("Scheduler interrupt started...");
#endif
            
#if SCHEDULER_TRACE
            BasicConsole.WriteLine("Getting current thread index...");
#endif
            ProcessManager.CurrentThread.TimeToRun--;
            if (ProcessManager.CurrentThread.TimeToRun == 0)
            {
                ProcessManager.CurrentThread.TimeToRun = ProcessManager.CurrentThread.TimeToRunReload;

                int threadIdx = ProcessManager.CurrentProcess.Threads.IndexOf(ProcessManager.CurrentThread);
#if SCHEDULER_TRACE
                BasicConsole.WriteLine("Moving to next thread index...");
#endif
                threadIdx++;
#if SCHEDULER_TRACE
                BasicConsole.WriteLine("Checking thread count...");
#endif
                if (threadIdx >= ProcessManager.CurrentProcess.Threads.Count)
                {
#if SCHEDULER_TRACE
                    BasicConsole.WriteLine("Getting process index...");
#endif
                    int processIdx = ProcessManager.Processes.IndexOf(ProcessManager.CurrentProcess);
#if SCHEDULER_TRACE
                    BasicConsole.WriteLine("Moving to next process...");
#endif
                    processIdx++;
#if SCHEDULER_TRACE
                    BasicConsole.WriteLine("Setting thread index to first thread...");
#endif
                    threadIdx = 0;
#if SCHEDULER_TRACE
                    BasicConsole.WriteLine("Checking process count...");
#endif
                    if (processIdx >= ProcessManager.Processes.Count)
                    {
#if SCHEDULER_TRACE
                        BasicConsole.WriteLine("Setting process index to first process...");
#endif
                        Go back to start of list
                        processIdx = 0;
                    }

#if SCHEDULER_TRACE
                    BasicConsole.WriteLine("Setting current process...");
#endif
                    ProcessManager.CurrentProcess = (Process)ProcessManager.Processes[processIdx];
                }

#if SCHEDULER_TRACE
                BasicConsole.WriteLine("Setting current thread...");
#endif
                ProcessManager.CurrentThread = (Thread)ProcessManager.CurrentProcess.Threads[threadIdx];
#if SCHEDULER_TRACE
                BasicConsole.WriteLine("Setting current thread state...");
#endif
                ProcessManager.CurrentThread_State = ProcessManager.CurrentThread.State;

#if SCHEDULER_TRACE
                BasicConsole.WriteLine("Checking thread started...");
#endif
            }

            if (!ProcessManager.CurrentThread_State->Started)
            {
#if SCHEDULER_TRACE
                BasicConsole.WriteLine("Marking thread as started...");
#endif
                ProcessManager.CurrentThread_State->Started = true;
            
#if SCHEDULER_TRACE
                BasicConsole.WriteLine("Initialising thread stack...");
#endif
                uint* stackPtr = (uint*)ProcessManager.CurrentThread_State->ThreadStackTop;
                stackPtr--;
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
                BasicConsole.WriteLine("Updating thread stack...");
#endif
                ProcessManager.CurrentThread_State->ESP = (uint)stackPtr;
            }
            
#if SCHEDULER_TRACE
            BasicConsole.WriteLine("Scheduler interrupt ended.");
            BasicConsole.DelayOutput(1);
#endif
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
