using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Core.Processes
{
    [Compiler.PluggedClass]
    public static unsafe class Scheduler
    {
        public static void Init()
        {
            //Disable interrupts - critical section
            BasicConsole.WriteLine(" > Disabling interrupts...");
            Hardware.Interrupts.Interrupts.DisableInterrupts();

            //Load first process and first thread (ManagedMain process)
            BasicConsole.WriteLine(" > Setting current process...");
            ProcessManager.CurrentProcess = (Process)ProcessManager.Processes[0];
            BasicConsole.WriteLine(" > Setting current thread...");
            ProcessManager.CurrentThread = (Thread)ProcessManager.CurrentProcess.Threads[0];
            BasicConsole.WriteLine(" > Setting current thread state...");
            ProcessManager.CurrentThread_State = ProcessManager.CurrentThread.State;

            //Init TSS
            BasicConsole.WriteLine(" > Getting TSS pointer...");
            TSS* tss = GetTSSPointer();
            BasicConsole.WriteLine(" > Setting esp0...");
            tss->esp0 = (uint)ProcessManager.CurrentThread_State->KernelStackTop;
            BasicConsole.WriteLine(" > Setting ss0...");
            tss->ss0 = ProcessManager.CurrentThread_State->SS;
            BasicConsole.WriteLine(" > Setting cr3...");
            tss->cr3 = ProcessManager.CurrentProcess.TheMemoryLayout.CR3;

            //Load Task Register
            BasicConsole.WriteLine(" > Loading TR...");
            LoadTR();

            //Enable timer
            BasicConsole.WriteLine(" > Adding timer handler...");
            Hardware.Devices.Timer.Default.RegisterHandler(OnTimerInterrupt, 1000, true, null);

            //Jump to ManagedMain start point
            //  - ManagedMain will re-enable interrupts so the scheduler can work
            BasicConsole.WriteLine(" > Jumping to main method...");
            JumpToMainMethod(ProcessManager.CurrentThread.StartEIP, (uint)ProcessManager.CurrentThread.State->ThreadStackTop);
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
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static void JumpToMainMethod(uint eip, uint esp)
        {
        }

        private static void OnTimerInterrupt(FOS_System.Object state)
        {
            BasicConsole.WriteLine("Scheduler interrupt.");
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
