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

using System.Runtime.InteropServices;
using Drivers.Compiler.Attributes;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Processes;
using Kernel.Utilities;
using Kernel.VirtualMemory;

namespace Kernel.Multiprocessing
{
    public unsafe class Thread : Comparable
    {
        public enum ActiveStates
        {
            NotStarted,
            Suspended,
            Inactive,
            Active,
            Terminated
        }

        public const int IndefiniteSleep = -1;

        private bool debug_Suspend;

        public uint Id;
        public String Name;

        public Process Owner;

        public ThreadState* State;
        private bool suspend;

        public uint[] StartArgs
        {
            get; private set;
        }

        /// <remarks>
        ///     Units of [time period of scheduler]
        /// </remarks>
        public int TimeToRun;

        /// <remarks>
        ///     Units of [time period of scheduler]
        /// </remarks>
        public int TimeToRunReload;

        protected int timeToSleep;

        public bool Debug_Suspend
        {
            get { return debug_Suspend; }
            set
            {
                LastActiveState = ActiveState;
                debug_Suspend = value;
                Scheduler.UpdateList(this);
            }
        }

        public bool Suspend
        {
            get { return suspend; }
            set
            {
                LastActiveState = ActiveState;
                suspend = value;
                Scheduler.UpdateList(this);
            }
        }

        /// <remarks>
        ///     Units of ms
        /// </remarks>
        public int TimeToSleep
        {
            get { return timeToSleep; }
            set
            {
                LastActiveState = ActiveState;
                timeToSleep = value;
            }
        }

        public bool IsSuspended
        {
            get { return Debug_Suspend || Suspend || TimeToSleep == IndefiniteSleep; }
        }

        public bool IsActive
        {
            get { return TimeToSleep == 0; }
        }

        public ActiveStates ActiveState
        {
            get
            {
                return
                    !State->Started
                        ? ActiveStates.NotStarted
                        : State->Terminated
                            ? ActiveStates.Terminated
                            : IsSuspended
                                ? ActiveStates.Suspended
                                : IsActive
                                    ? ActiveStates.Active
                                    : ActiveStates.Inactive;
            }
        }

        public ActiveStates LastActiveState { get; set; }

        public override int Key
        {
            get
            {
                if (TimeToSleep > 0)
                {
                    return TimeToSleep;
                }
                return TimeToRun;
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

        public static uint ThreadStackTopOffset
        {
            get { return (uint)(4096 - sizeof(ExceptionState) - 4); }
        }

        public static uint KernelStackTopOffset
        {
            get { return 4096 - 4; }
        }

        /* 
         * For offsets from ESP used in the properties below, see Scheduler.SetupThreadForStart.
         * Specifically, the User Mode thread initialisation section has commented offset values.
         * 
         * Note: "ESP after switch to UM" and "SS after switch to UM" do not exist for KM processes.
         *       Be careful not to accidentally cause stack underflow when accessing these.
         */

        public uint EAXFromInterruptStack
        {
            [NoDebug] get { return *(uint*)(State->ESP + 44); }
            [NoDebug] set { *(uint*)(State->ESP + 44) = value; }
        }

        public uint EBXFromInterruptStack
        {
            [NoDebug] get { return *(uint*)(State->ESP + 32); }
            [NoDebug] set { *(uint*)(State->ESP + 32) = value; }
        }

        public uint ECXFromInterruptStack
        {
            [NoDebug] get { return *(uint*)(State->ESP + 40); }
            [NoDebug] set { *(uint*)(State->ESP + 40) = value; }
        }

        public uint EDXFromInterruptStack
        {
            [NoDebug] get { return *(uint*)(State->ESP + 36); }
            [NoDebug] set { *(uint*)(State->ESP + 36) = value; }
        }

        public uint ESPFromInterruptStack
        {
            [NoDebug]
            get
            {
                // +12 to get over return pointer, CS and EFLAGS pushed by hardware on interrupt
                return *(uint*)(State->ESP + 28) + 12;
            }
        }

        public uint EBPFromInterruptStack
        {
            [NoDebug] get { return *(uint*)(State->ESP + 24); }
            [NoDebug] set { *(uint*)(State->ESP + 24) = value; }
        }

        public uint EIPFromInterruptStack
        {
            [NoDebug] get { return *(uint*)(State->ESP + 48); }
            [NoDebug] set { *(uint*)(State->ESP + 48) = value; }
        }

        public uint EFLAGSFromInterruptStack
        {
            [NoDebug] get { return *(uint*)(State->ESP + 56); }
            [NoDebug] set { *(uint*)(State->ESP + 56) = value; }
        }

        public uint SysCallNumber
        {
            get { return EAXFromInterruptStack; }
        }

        public uint Param1
        {
            get { return EBXFromInterruptStack; }
        }

        public uint Param2
        {
            get { return ECXFromInterruptStack; }
        }

        public uint Param3
        {
            get { return EDXFromInterruptStack; }
        }

        public uint Return1
        {
            get { return EAXFromInterruptStack; }
            set { EAXFromInterruptStack = value; }
        }

        public uint Return2
        {
            get { return EBXFromInterruptStack; }
            set { EBXFromInterruptStack = value; }
        }

        public uint Return3
        {
            get { return ECXFromInterruptStack; }
            set { ECXFromInterruptStack = value; }
        }

        public uint Return4
        {
            get { return EDXFromInterruptStack; }
            set { EDXFromInterruptStack = value; }
        }

        public Thread(Process AnOwner, ThreadStartPoint StartPoint, uint AnId, bool UserMode, String AName,
            out void* ThreadStackBottomPAddr, out void* KernelStackBottomPAddr, uint[] SomeStartArgs)
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
            State = (ThreadState*)Heap.AllocZeroed((uint)sizeof(ThreadState), "Thread : Thread() (1)");

            // Init Id and EIP
            //  Set EIP to the first instruction of the main method
#if THREAD_TRACE
            BasicConsole.WriteLine("Setting thread info...");
#endif
            Id = AnId;
            Name = AName;
            State->StartEIP = (uint)ObjectUtilities.GetHandle(StartPoint);

            // Allocate kernel memory for the kernel stack for this thread
            //  Used when this thread is preempted or does a sys call. Stack is switched to
            //  this thread-specific kernel stack
#if THREAD_TRACE
            BasicConsole.WriteLine("Allocating kernel stack...");
#endif
            State->KernelStackTop = (byte*)VirtualMemoryManager.MapFreePageForKernel(
                UserMode
                    ? PageFlags.None
                    : PageFlags.KernelOnly, out KernelStackBottomPAddr) +
                                    KernelStackTopOffset;
            //4KiB, page-aligned

            // Allocate free memory for the user stack for this thread
            //  Used by this thread in normal execution
#if THREAD_TRACE
            BasicConsole.WriteLine("Mapping thread stack page...");
#endif
            State->UserMode = UserMode;
            if (AnOwner == ProcessManager.KernelProcess)
            {
                State->ThreadStackTop = (byte*)VirtualMemoryManager.MapFreePageForKernel(
                    UserMode
                        ? PageFlags.None
                        : PageFlags.KernelOnly, out ThreadStackBottomPAddr) +
                                        ThreadStackTopOffset; //4KiB, page-aligned
            }
            else
            {
                State->ThreadStackTop = (byte*)VirtualMemoryManager.MapFreePage(
                    UserMode
                        ? PageFlags.None
                        : PageFlags.KernelOnly, out ThreadStackBottomPAddr) +
                                        ThreadStackTopOffset; //4KiB, page-aligned
            }

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
            State->ExState = (ExceptionState*)(State->ThreadStackTop + 4);
            byte* exStateBytePtr = (byte*)State->ExState;
            for (int i = 0; i < sizeof(ExceptionState); i++)
            {
                *exStateBytePtr++ = 0;
            }

#if THREAD_TRACE
            BasicConsole.WriteLine("Done.");
#endif

            StartArgs = SomeStartArgs;
            if (StartArgs == null)
            {
                StartArgs = new uint[0];
            }
        }

        //public static bool EnterSleepPrint = false;

        /// <remarks>
        ///     Call this instead of Thread.Sleep when inside an interrupt handler.
        ///     If inside an interrupt handler, you probably want to call
        ///     Kernel.Multiprocessing.Scheduler.UpdateCurrentState()
        ///     after calling this to immediately update the thread to return to.
        /// </remarks>
        [NoGC]
        [NoDebug]
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

            TimeToSleep = ms /* x * 1ms / [Scheduler period in ns] = x * 1 = x */;
            Scheduler.UpdateList(this);

            //if (reenable)
            //{
            //    Scheduler.Enable();
            //}
            //}
        }

        [NoGC]
        [NoDebug]
        public bool _Sleep(int ms)
        {
            //Prevent getting stuck forever.
            //  This may cause other problems later but at least we don't end up in the infinite
            //  while loop.
            if (!Scheduler.IsEnabled())
            {
                return false;
            }

            _EnterSleep(ms);
            // Busy wait for the scheduler to interrupt the thread, sleep it and
            //  then as soon as the sleep is over this condition will go false
            //  so the thread will continue
            while (ProcessManager.CurrentThread.TimeToSleep != 0)
            {
                //  Note: Due to the way Deferred Sys Calls work, we would expect the
                //      deferred syscalls thread to go through this loop at least once
                //      fairly frequently. 
                ;
            }

            return true;
        }

        [NoGC]
        [NoDebug]
        public bool _Sleep_Indefinitely()
        {
            return _Sleep(IndefiniteSleep);
        }

        [NoGC]
        [NoDebug]
        public void _Wake()
        {
            //bool reenable = Scheduler.Enabled;
            //if (reenable)
            //{
            //    Scheduler.Disable();
            //}

            TimeToSleep = 0;
            TimeToRun = TimeToRunReload;
            Scheduler.UpdateList(this);

            //if (reenable)
            //{
            //    Scheduler.Enable();
            //}
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ThreadState
    {
        /* Do not re-order the fields in the structure. */

        public bool Started; // Offset: 0

        public uint ESP; // Offset: 1
        public ushort SS; // Offset: 5
        public byte* KernelStackTop; // Offset: 7
        public byte* ThreadStackTop; // Offset: 11

        public uint StartEIP; // Offset: 15
        public bool Terminated; // Offset: 19

        public bool UserMode; // Offset: 20

        public ExceptionState* ExState; // Offset: 21
    }
}