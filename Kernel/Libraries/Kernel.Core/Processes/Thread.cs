using System;

namespace Kernel.Core.Processes
{
    public unsafe class Thread : FOS_System.Object
    {
        public uint Id;
        
        public uint EIP;
        public uint ESP;
        public uint SS;
        public byte* KernelStackTop;
        public byte* ThreadStackTop;

        public bool Started;
        public uint TimeToRun;

        public Thread(void* StartMethodPtr, uint AnId)
        {
            // Init Id and EIP
            //  Set EIP to the first instruction of the main method
            Id = AnId;
            EIP = (uint)StartMethodPtr;

            // Allocate kernel memory for the kernel stack for this thread
            //  Used when this thread is preempted or does a sys call. Stack is switched to
            //  this thread-specific kernel stack
            KernelStackTop = (byte*)FOS_System.Heap.Alloc(0x1000, 4); //1KiB, 4-byte aligned

            // Allocate free memory for the user stack for this thread
            //  Used by this thread in normal execution
            ThreadStackTop = (byte*)Hardware.VirtMemManager.MapFreePage(); //4 KiB, page-aligned

            // Set ESP to the top of the stack - 4 byte aligned, high address since x86 stack works
            //  downwards
            ESP = (uint)(ThreadStackTop + 4092);

            // Init TimeToRun
            //  - 5 timer events? Not sure how long that actually is. Adjust this...
            TimeToRun = 5;

            // Init SS
            //  Stack Segment = User or Kernel space data segment selector offset
            //  Kernel data segment selector offset (offset in GDT) = 0x10 (16)
            SS = 16;

            // Init Started
            //  Not started yet so set to false
            Started = false;
        }
    }
}
