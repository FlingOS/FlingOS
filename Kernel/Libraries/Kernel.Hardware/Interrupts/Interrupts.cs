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

using Kernel.FOS_System.Collections;
using Kernel.Hardware.Processes;

namespace Kernel.Hardware.Interrupts
{
    /// <summary>
    /// Strcture for an interrupt descriptor in the Interrupts Descriptor Table (IDT).
    /// </summary>
    /// <remarks>
    /// See the <a href="http://www.flingos.co.uk/docs/reference/Interrupt-Descriptors-Table/">Interrupt Descriptors Table</a> article for details.
    /// </remarks>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)] 
    //TODO: Work out whether this attribute is necessary given that the FlingOS Compiler doesnt look for it. It assumes packing of 1 for everything.
    public struct InterruptDescriptor
    {
        /// <summary>
        /// Handler address low-bytes.
        /// </summary>
        public ushort OffsetLo;
        /// <summary>
        /// Segment Selector for destination code segment (i.e. selector for Code Segment that contains the interrupt handler). 
        /// In most systems this will always be 0.
        /// </summary>
        public ushort Selector;
        /// <summary>
        /// Always 0.
        /// </summary>
        public byte UNUSED;
        /// <summary>
        /// Gate type, Storage Segment, Descriptor Privilege Level and Present bits.
        /// </summary>
        public byte Type_S_DPL_P;
        /// <summary>
        /// Handler address high-bytes.
        /// </summary>
        public ushort OffsetHi;
    }
    /// <summary>
    /// Provides methods for handling hardware and software interrupts (excluding interrupts 0 through 16).
    /// </summary>
    public unsafe static class Interrupts
    {
        //TODO: This lot is all x86 specific. It needs to be abstracted into a separate x86 interrupts class to support new architectures.

        public static bool insideCriticalHandler = false;
        public static bool InsideCriticalHandler
        {
            [Drivers.Compiler.Attributes.NoDebug]
            get
            {
                return insideCriticalHandler;
            }
            [Drivers.Compiler.Attributes.NoDebug]
            set
            {
                insideCriticalHandler = value;

                FOS_System.Heap.PreventAllocation = value;
                if (value)
                {
                    FOS_System.Heap.PreventReason = "Inside critical interrupt handler.";
                }
                else
                {
                    FOS_System.Heap.PreventReason = "[NONE]";
                }
                if (value)
                {
                    FOS_System.GC.Disable("InsideCriticalHandler");
                }
                else
                {
                    FOS_System.GC.Enable("InsideCriticalHandler");
                }
            }
        }

        /// <summary>
        /// Used to disable attempts to process switch while the OS is initialising.
        /// </summary>
        public static bool EnableProcessSwitching = false;

        /// <summary>
        /// Exception state for during interrupt handlers.
        /// </summary>
        public static ExceptionState* InterruptsExState;

        static Interrupts()
        {
            /*ExceptionMethods.InterruptsState = */InterruptsExState = (ExceptionState*)FOS_System.Heap.AllocZeroed((uint)sizeof(ExceptionState), "Interrupts : Interrupts()");
        }

        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath=null)]
        public static void EnableInterrupts()
        {
        }
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static void DisableInterrupts()
        {
        }

        /// <summary>
        /// Enables the specified IRQ number (0-15)
        /// </summary>
        /// <param name="num">The IRQ to enable.</param>
        public static void EnableIRQ(byte num)
        {
            //For x86 Programmable Interrupts Controller (PIC)
            //  there is the master PIC and the slave PIC.

            //The master PIC controls IRQs 0-7, 
            // the slave PIC controls IRQs 8-15
            //However, when sending commands to the slave PIC
            //  you index IRQs 8-15 as indices 0-7 as that is 
            //  how the slave PIC sees them.

            //So, if Num > 7, we must send the info to the slave PIC
            if (num > 7)
            {
                //-= 8 converts IRQ number to index on slave PIC
                num -= 8;

                //Bit mask - set bit indicates IRQ disabled.
                // We want to enable, so we must clear the corresponding bit.
                byte mask = IO.IOPort.doRead_Byte(0xA1);
                byte bitMask = (byte)(~(1u << num));
                mask &= bitMask;
                //Port 0xA1 for slave PIC
                IO.IOPort.doWrite_Byte(0xA1, mask);
            }
            //Else we send the info to the master PIC
            else
            {
                //See above.
                byte mask = IO.IOPort.doRead_Byte(0x21);
                byte bitMask = (byte)(~(1u << num));
                mask &= bitMask;
                //Port 0x21 for master PIC
                IO.IOPort.doWrite_Byte(0x21, mask);
            }
        }
        /// <summary>
        /// Disables the specified IRQ number (0-15)
        /// </summary>
        /// <param name="num">The IRQ to disable.</param>
        public static void DisableIRQ(byte num)
        {
            //This functions the same as EnableIRQ except it sets 
            //  the corresponding bit instead of clearing it.
            if (num > 7)
            {
                num -= 8;

                byte mask = IO.IOPort.doRead_Byte(0xA1);
                byte bitMask = (byte)(1u << num);
                mask |= bitMask;
                IO.IOPort.doWrite_Byte(0xA1, mask);
            }
            else
            {
                byte mask = IO.IOPort.doRead_Byte(0x21);
                byte bitMask = (byte)(1u << num);
                mask |= bitMask;
                IO.IOPort.doWrite_Byte(0x21, mask);
            }
        }
        
        /// <summary>
        /// Common method called to handle all interrupts (excluding numbers 0-16 inclusive).
        /// </summary>
        /// <param name="ISRNum">The number of the interrupt which occurred.</param>
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        private static void CommonISR(uint ISRNum)
        {
            InsideCriticalHandler = true;

            try
            {
                try
                {
                    if (ISRNum > 31 && ISRNum < 48)
                    {
                        HandleIRQ(ISRNum - 32);
                    }
                    else
                    {
                        HandleISR(ISRNum);
                    }
                }
                catch
                {
                    BasicConsole.WriteLine("Error processing ISR/IRQ!");
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }
            }
            finally
            {
                InsideCriticalHandler = false;
            }
        }
        [Drivers.Compiler.Attributes.NoDebug]
        private static void HandleISR(uint ISRNum)
        {
            Process currProcess = ProcessManager.CurrentProcess;
            Thread currThread = ProcessManager.CurrentThread;
            bool switched = false;

            Process handlerProcess = null;
            for (int i = 0; i < ProcessManager.Processes.Count; i++)
            {
                handlerProcess = (Process)ProcessManager.Processes[i];
                if (handlerProcess.ISRsToHandle.IsSet((int)ISRNum))
                {
                    if (handlerProcess.SwitchProcessForISRs && EnableProcessSwitching)
                    {
                        ProcessManager.SwitchProcess(handlerProcess.Id, ProcessManager.THREAD_DONT_CARE);
                        switched = true;
                    }
                    if (handlerProcess.ISRHandler(ISRNum) == 0)
                    {
                        break;
                    }
                }
            }

            if (switched)
            {
                ProcessManager.SwitchProcess(currProcess.Id, (int)currThread.Id);
            }
        }
        [Drivers.Compiler.Attributes.NoDebug]
        private static void HandleIRQ(uint IRQNum)
        {
            Process currProcess = ProcessManager.CurrentProcess;
            Thread currThread = ProcessManager.CurrentThread;
            bool switched = false;
            
            Process handlerProcess = null;
            for (int i = 0; i < ProcessManager.Processes.Count; i++)
            {
                handlerProcess = (Process)ProcessManager.Processes[i];
                if (handlerProcess.IRQsToHandle.IsSet((int)IRQNum))
                {
                    if (handlerProcess.SwitchProcessForIRQs && EnableProcessSwitching)
                    {
                        ProcessManager.SwitchProcess(handlerProcess.Id, ProcessManager.THREAD_DONT_CARE);
                        switched = true;
                    }

                    if (handlerProcess.IRQHandler(IRQNum) == 0)
                    {
                        break;
                    }
                }
            }

            if (switched)
            {
                ProcessManager.SwitchProcess(currProcess.Id, (int)currThread.Id);
            }

            EndIRQ(IRQNum > 7);
        }
        /// <summary>
        /// Sends the End of Interrupt to the PIC to signify the end of an IRQ.
        /// </summary>
        /// <param name="slave">Whether to send the EOI to the slave PIC too.</param>
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        private static void EndIRQ(bool slave)
        {
        }

        /// <summary>
        /// Gets a pointer to the interrupt descriptor table.
        /// </summary>
        /// <returns>The pointer to the IDT.</returns>
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath=@"ASM\Interrupts\Interrupts")]
        private static InterruptDescriptor* GetIDTPtr()
        {
            return null;
        }

        /// <summary>
        /// Invokes the specified interrupt number. Warning: Invoking interrupts 0 through 16 has the same
        /// affect as if the actual hardware exception occurred. Invoking interrupts 0 through 16 will result 
        /// in exception logic or page fault logic being executed!
        /// </summary>
        /// <param name="IntNum">The interrupt number to fire.</param>
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static void InvokeInterrupt(uint IntNum)
        {
        }
    }
}
