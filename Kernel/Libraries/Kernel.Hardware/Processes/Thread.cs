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
    
//#define THREAD_TRACE

using System;

namespace Kernel.Hardware.Processes
{
    public delegate void ThreadStartMethod();

    public unsafe class Thread : FOS_System.Collections.Comparable
    {
        public enum ActiveStates
        {
            NotStarted,
            Suspended,
            Inactive,
            Active,
            Terminated
        }

        public Process Owner;
        private bool debug_Suspend;
        public bool Debug_Suspend
        {
            get
            {
                return debug_Suspend;
            }
            set
            {
                LastActiveState = ActiveState;
                debug_Suspend = value;
                Scheduler.UpdateList(this);
            }
        }

        public const int IndefiniteSleep = -1;

        public uint Id;
        public FOS_System.String Name;
        
        public ThreadState* State;

        /// <remarks>
        /// Units of [time period of scheduler]
        /// </remarks>
        public int TimeToRun;
        /// <remarks>
        /// Units of [time period of scheduler]
        /// </remarks>
        public int TimeToRunReload;

        protected int timeToSleep = 0;
        /// <remarks>
        /// Units of ms
        /// </remarks>
        public int TimeToSleep
        {
            get
            {
                return timeToSleep;
            }
            set
            {
                LastActiveState = ActiveState;
                timeToSleep = value;
            }
        }

        public bool Suspended
        {
            get
            {
                return Debug_Suspend || TimeToSleep == IndefiniteSleep;
            }
        }
        public bool Active
        {
            get
            {
                return TimeToSleep == 0;
            }
        }
        public ActiveStates ActiveState
        {
            get
            {
                return 
                    !State->Started ? ActiveStates.NotStarted : 
                    State->Terminated ? ActiveStates.Terminated : 
                    Suspended ? ActiveStates.Suspended : 
                    Active ? ActiveStates.Active : 
                    ActiveStates.Inactive;
            }
        }
        public ActiveStates LastActiveState
        {
            get;
            set;
        }
        public override int Key
        {
            get
            {
                if (TimeToSleep > 0)
                {
                    return TimeToSleep;
                }
                else
                {
                    return TimeToRun;
                }
            }
            set
            {
                if (!Debug_Suspend)
                {
                    if (TimeToSleep > 0)
                    {
                        TimeToSleep = value;
                    }
                    // Zero Timed processes never have their time to run decremented
                    else if (Owner.Priority != Scheduler.Priority.ZeroTimed)
                    {
                        TimeToRun = value;
                    }
                }
            }
        }
        
        public Thread(Process AnOwner, ThreadStartMethod StartMethod, uint AnId, bool UserMode, FOS_System.String AName)
        {
#if THREAD_TRACE
            BasicConsole.WriteLine("Constructing thread object...");
#endif
            LastActiveState = ActiveStates.NotStarted;
            Owner = AnOwner;

            //Init thread state
            #if THREAD_TRACE
            BasicConsole.WriteLine("Allocating state memory...");
#endif
            State = (ThreadState*)FOS_System.Heap.Alloc((uint)sizeof(ThreadState), "Thread : Thread() (1)");

            // Init Id and EIP
            //  Set EIP to the first instruction of the main method
#if THREAD_TRACE
            BasicConsole.WriteLine("Setting thread info...");
#endif
            Id = AnId;
            Name = AName;
            State->StartEIP = (uint)Utilities.ObjectUtilities.GetHandle(StartMethod);

            // Allocate kernel memory for the kernel stack for this thread
            //  Used when this thread is preempted or does a sys call. Stack is switched to
            //  this thread-specific kernel stack
#if THREAD_TRACE
            BasicConsole.WriteLine("Allocating kernel stack...");
#endif
            State->KernelStackTop = (byte*)FOS_System.Heap.Alloc(0x1000, 4) + 0xFFC; //4KiB, 4-byte aligned
            
            // Allocate free memory for the user stack for this thread
            //  Used by this thread in normal execution
#if THREAD_TRACE
            BasicConsole.WriteLine("Mapping thread stack page...");
#endif
            State->UserMode = UserMode;
            State->ThreadStackTop = (byte*)Hardware.VirtMemManager.MapFreePage(
                UserMode ? Hardware.VirtMem.VirtMemImpl.PageFlags.None :
                           Hardware.VirtMem.VirtMemImpl.PageFlags.KernelOnly) + 4092; //4 KiB, page-aligned
            
            // Set ESP to the top of the stack - 4 byte aligned, high address since x86 stack works
            //  downwards
#if THREAD_TRACE
            BasicConsole.WriteLine("Setting ESP...");
#endif
            State->ESP = (uint)State->ThreadStackTop;

            // TimeToRun and TimeToRunReload are set up in Scheduler.InitProcess which
            //      is called when a process is registered.

            // Init SS
            //  Stack Segment = User or Kernel space data segment selector offset
            //  Kernel data segment selector offset (offset in GDT) = 0x10 (16)
            //  User   data segment selector offset (offset in GDT) = 0x23 (32|3)
            //          User data segment selector must also be or'ed with 3 for User Privilege level
#if THREAD_TRACE
            BasicConsole.WriteLine("Setting SS...");
#endif
            State->SS = UserMode ? (ushort)0x23 : (ushort)0x10;

            // Init Started
            //  Not started yet so set to false
#if THREAD_TRACE
            BasicConsole.WriteLine("Setting started...");
#endif
            State->Started = false;

#if THREAD_TRACE
            BasicConsole.WriteLine("Allocating exception state...");
#endif
            //TODO: This is currently incorrectly allocated from the current process's heap instead of the heap of the owner process
            // Init Exception State
            State->ExState = (ExceptionState*)FOS_System.Heap.AllocZeroed((uint)sizeof(ExceptionState), "Thread : Thread() (2)");

#if THREAD_TRACE
            BasicConsole.WriteLine("Done.");
#endif
        }

        /* 
         * For offsets from ESP used in the properties below, see Scheduler.SetupThreadForStart.
         * Specifically, the User Mode thread initialisation section has commented offset values.
         * 
         * Note: "ESP after switch to UM" and "SS after switch to UM" do not exist for KM processes.
         *       Be careful not to accidentally cause stack underflow when accessing these.
         */

        public UInt32 EAXFromInterruptStack
        {
            [Drivers.Compiler.Attributes.NoDebug]
            get
            {
                return *(UInt32*)(State->ESP + 44);
            }
            [Drivers.Compiler.Attributes.NoDebug]
            set
            {
                *(UInt32*)(State->ESP + 44) = value;
            }
        }
        public UInt32 EBXFromInterruptStack
        {
            [Drivers.Compiler.Attributes.NoDebug]
            get
            {
                return *(UInt32*)(State->ESP + 32);
            }
            [Drivers.Compiler.Attributes.NoDebug]
            set
            {
                *(UInt32*)(State->ESP + 32) = value;
            }
        }
        public UInt32 ECXFromInterruptStack
        {
            [Drivers.Compiler.Attributes.NoDebug]
            get
            {
                return *(UInt32*)(State->ESP + 40);
            }
            [Drivers.Compiler.Attributes.NoDebug]
            set
            {
                *(UInt32*)(State->ESP + 40) = value;
            }
        }
        public UInt32 EDXFromInterruptStack
        {
            [Drivers.Compiler.Attributes.NoDebug]
            get
            {
                return *(UInt32*)(State->ESP + 36);
            }
            [Drivers.Compiler.Attributes.NoDebug]
            set
            {
                *(UInt32*)(State->ESP + 36) = value;
            }
        }

        public UInt32 ESPFromInterruptStack
        {
            [Drivers.Compiler.Attributes.NoDebug]
            get
            {
                // +12 to get over return pointer, CS and EFLAGS pushed by hardware on interrupt
                return *(UInt32*)(State->ESP + 28) + 12;
            }
        }
        public UInt32 EBPFromInterruptStack
        {
            [Drivers.Compiler.Attributes.NoDebug]
            get
            {
                return *(UInt32*)(State->ESP + 24);
            }
            [Drivers.Compiler.Attributes.NoDebug]
            set
            {
                *(UInt32*)(State->ESP + 24) = value;
            }
        }
        public UInt32 EIPFromInterruptStack
        {
            [Drivers.Compiler.Attributes.NoDebug]
            get
            {
                return *(UInt32*)(State->ESP + 48);
            }
            [Drivers.Compiler.Attributes.NoDebug]
            set
            {
                *(UInt32*)(State->ESP + 48) = value;
            }
        }
        public UInt32 EFLAGSFromInterruptStack
        {
            [Drivers.Compiler.Attributes.NoDebug]
            get
            {
                return *(UInt32*)(State->ESP + 56);
            }
            [Drivers.Compiler.Attributes.NoDebug]
            set
            {
                *(UInt32*)(State->ESP + 56) = value;
            }
        }
        
        public UInt32 SysCallNumber
        {
            get
            {
                return EAXFromInterruptStack;
            }
        }
        public UInt32 Param1
        {
            get
            {
                return EBXFromInterruptStack;
            }
        }
        public UInt32 Param2
        {
            get
            {
                return ECXFromInterruptStack;
            }
        }
        public UInt32 Param3
        {
            get
            {
                return EDXFromInterruptStack;
            }
        }
        public UInt32 Return1
        {
            get
            {
                return EAXFromInterruptStack;
            }
            set
            {
                EAXFromInterruptStack = value;
            }
        }
        public UInt32 Return2
        {
            get
            {
                return EBXFromInterruptStack;
            }
            set
            {
                EBXFromInterruptStack = value;
            }
        }
        public UInt32 Return3
        {
            get
            {
                return ECXFromInterruptStack;
            }
            set
            {
                ECXFromInterruptStack = value;
            }
        }
        public UInt32 Return4
        {
            get
            {
                return EDXFromInterruptStack;
            }
            set
            {
                EDXFromInterruptStack = value;
            }
        }

        //public static bool EnterSleepPrint = false;

        /// <remarks>
        /// Call this instead of Thread.Sleep when inside an interrupt handler.
        /// 
        /// If inside an interrupt handler, you probably want to call 
        /// Kernel.Hardware.Processes.Scheduler.UpdateCurrentState()
        /// after calling this to immediately update the thread to return to.
        /// </remarks>
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        public void _EnterSleep(int ms)
        {
            //if (EnterSleepPrint)
            //{
            //    BasicConsole.WriteLine("Getting enabled...");
            //    bool reenable = Scheduler.Enabled;
            //    if (reenable)
            //    {
            //        BasicConsole.WriteLine("Disabling scheduler...");
            //        Scheduler.Disable();
            //    }
            //    BasicConsole.WriteLine("Checking current thread...");
            //    if (ProcessManager.CurrentThread == null)
            //    {
            //        BasicConsole.WriteLine("Massive problem! The current thread is null! Can't sleep null thread.");
            //        BasicConsole.DelayOutput(5);
            //    }
            //    BasicConsole.WriteLine("Setting time to sleep...");
            //    ProcessManager.CurrentThread.TimeToSleep = ms /* x * 1ms / [Scheduler period in ns] = x * 1 = x */;
            //    BasicConsole.WriteLine("Setting time to run...");
            //    ProcessManager.CurrentThread.TimeToRun = 0;
            //    BasicConsole.WriteLine("Checking re-enable...");
            //    if (reenable)
            //    {
            //        BasicConsole.WriteLine("Re-enabling scheduler...");
            //        Scheduler.Enable();
            //    }
            //    BasicConsole.WriteLine("Sleep method finished.");
            //}
            //else
            //{
            //bool reenable = Scheduler.Enabled;
            //if (reenable)
            //{
            //    Scheduler.Disable();
            //}

            this.TimeToSleep = ms /* x * 1ms / [Scheduler period in ns] = x * 1 = x */;
            Scheduler.UpdateList(this);
            
            //if (reenable)
            //{
            //    Scheduler.Enable();
            //}
            //}
        }
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        public bool _Sleep(int ms)
        {
            //Prevent getting stuck forever.
            //  This may cause other problems later but at least we don't end up in the infinite
            //  while loop.
            if (!Scheduler.Enabled)
            {
                return false;
            }

            this._EnterSleep(ms);
            // Busy wait for the scheduler to interrupt the thread, sleep it and
            //  then as soon as the sleep is over this condition will go false
            //  so the thread will continue
            while (ProcessManager.CurrentThread.TimeToSleep != 0)
            {
                ;
            }

            return true;
        }
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        public bool _Sleep_Indefinitely()
        {
            return this._Sleep(IndefiniteSleep);
        }
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        public void _Wake()
        {
            //bool reenable = Scheduler.Enabled;
            //if (reenable)
            //{
            //    Scheduler.Disable();
            //}

            this.TimeToSleep = 0;
            this.TimeToRun = this.TimeToRunReload;
            Scheduler.UpdateList(this);
            
            //if (reenable)
            //{
            //    Scheduler.Enable();
            //}
        }

        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        public static void EnterSleep(int ms)
        {
            if (ProcessManager.CurrentThread == null)
            {
                BasicConsole.WriteLine("Massive problem! The current thread is null! Can't sleep null thread.");
                BasicConsole.DelayOutput(5);
            }
            ProcessManager.CurrentThread._EnterSleep(ms);
        }
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        public static bool Sleep(int ms)
        {
            if (ProcessManager.CurrentThread == null)
            {
                BasicConsole.WriteLine("Massive problem! The current thread is null! Can't sleep null thread.");
                BasicConsole.DelayOutput(5);
            }
            return ProcessManager.CurrentThread._Sleep(ms);
        }
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        public static bool Sleep_Indefinitely()
        {
            if (ProcessManager.CurrentThread == null)
            {
                BasicConsole.WriteLine("Massive problem! The current thread is null! Can't sleep null thread.");
                BasicConsole.DelayOutput(5);
            }
            return ProcessManager.CurrentThread._Sleep_Indefinitely();
        }
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        public static void Wake()
        {
            if (ProcessManager.CurrentThread == null)
            {
                BasicConsole.WriteLine("Massive problem! The current thread is null! Can't wake null thread.");
                BasicConsole.DelayOutput(5);
            }
            ProcessManager.CurrentThread._Wake();
        }
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ThreadState
    {
        /* Do not re-order the fields in the structure. */

        public bool Started;            // Offset: 0
        
        public uint ESP;                // Offset: 1
        public ushort SS;               // Offset: 5
        public byte* KernelStackTop;    // Offset: 7
        public byte* ThreadStackTop;    // Offset: 11
        
        public uint StartEIP;           // Offset: 15
        public bool Terminated;         // Offset: 19

        public bool UserMode;           // Offset: 20

        public ExceptionState* ExState; // Offset: 21
    }
}
