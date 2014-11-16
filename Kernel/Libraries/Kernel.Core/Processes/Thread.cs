using System;

namespace Kernel.Core.Processes
{
    public unsafe class Thread : FOS_System.Object
    {
        public uint Id;
        public uint StartEIP;

        public ThreadState* State;

        public uint TimeToRun;

        public Thread(void* StartMethodPtr, uint AnId)
        {
            BasicConsole.WriteLine(" > > > Constructing thread object...");
            //Init thread state
            BasicConsole.WriteLine(" > > > Allocating state memory...");
            State = (ThreadState*)FOS_System.Heap.Alloc((uint)sizeof(ThreadState));

            // Init Id and EIP
            //  Set EIP to the first instruction of the main method
            BasicConsole.WriteLine(" > > > Setting info...");
            Id = AnId;
            StartEIP = (uint)StartMethodPtr;

            // Allocate kernel memory for the kernel stack for this thread
            //  Used when this thread is preempted or does a sys call. Stack is switched to
            //  this thread-specific kernel stack
            BasicConsole.WriteLine(" > > > Allocating kernel stack...");
            State->KernelStackTop = (byte*)FOS_System.Heap.Alloc(0x1000, 4); //1KiB, 4-byte aligned

            // Allocate free memory for the user stack for this thread
            //  Used by this thread in normal execution
            BasicConsole.WriteLine(" > > > Mapping thread stack page...");
            State->ThreadStackTop = (byte*)Hardware.VirtMemManager.MapFreePage() + 4092; //4 KiB, page-aligned

            // Set ESP to the top of the stack - 4 byte aligned, high address since x86 stack works
            //  downwards
            BasicConsole.WriteLine(" > > > Setting ESP...");
            State->ESP = (uint)State->ThreadStackTop;

            // Init TimeToRun
            //  - 5 timer events? Not sure how long that actually is. Adjust this...
            BasicConsole.WriteLine(" > > > Setting time to run...");
            TimeToRun = 5;

            // Init SS
            //  Stack Segment = User or Kernel space data segment selector offset
            //  Kernel data segment selector offset (offset in GDT) = 0x10 (16)
            BasicConsole.WriteLine(" > > > Setting SS...");
            State->SS = 16;

            // Init Started
            //  Not started yet so set to false
            BasicConsole.WriteLine(" > > > Setting started...");
            State->Started = false;
        }
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ThreadState
    {
        /* Do not re-order the fields in the structure. */

        public bool Started;
        
        public uint ESP;
        public ushort SS;
        public byte* KernelStackTop;
        public byte* ThreadStackTop;
    }
}
