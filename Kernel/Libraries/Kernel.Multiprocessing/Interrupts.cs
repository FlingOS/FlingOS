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

using System.Runtime.InteropServices;
using Drivers.Compiler.Attributes;
using Kernel.Framework;
using Kernel.Framework.Processes;
using Kernel.IO;
using Kernel.Multiprocessing;

namespace Kernel.Interrupts
{
    /// <summary>
    ///     Strcture for an interrupt descriptor in the Interrupts Descriptor Table (IDT).
    /// </summary>
    /// <remarks>
    ///     See the
    ///     <a href="http://www.flingos.co.uk/docs/reference/Interrupt-Descriptors-Table/">Interrupt Descriptors Table</a>
    ///     article for details.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    //TODO: Work out whether this attribute is necessary given that the FlingOS Compiler doesnt look for it. It assumes packing of 1 for everything.
    public struct InterruptDescriptor
    {
        /// <summary>
        ///     Handler address low-bytes.
        /// </summary>
        public ushort OffsetLo;

        /// <summary>
        ///     Segment Selector for destination code segment (i.e. selector for Code Segment that contains the interrupt handler).
        ///     In most systems this will always be 0.
        /// </summary>
        public ushort Selector;

        /// <summary>
        ///     Always 0.
        /// </summary>
        public byte UNUSED;

        /// <summary>
        ///     Gate type, Storage Segment, Descriptor Privilege Level and Present bits.
        /// </summary>
        public byte Type_S_DPL_P;

        /// <summary>
        ///     Handler address high-bytes.
        /// </summary>
        public ushort OffsetHi;
    }

    /// <summary>
    ///     Provides methods for handling hardware and software interrupts (excluding interrupts 0 through 16).
    /// </summary>
    public static unsafe class Interrupts
    {
        //TODO: This lot is all x86 specific. It needs to be abstracted into a separate x86 interrupts class to support new architectures.

        [Group(Name = "IsolatedKernel_Hardware_Multiprocessing")] public static bool wasPrintingMessages = false;

        [Group(Name = "IsolatedKernel_Hardware_Multiprocessing")] public static bool insideCriticalHandler;

        /// <summary>
        ///     Used to disable attempts to process switch while the OS is initialising.
        /// </summary>
        [Group(Name = "IsolatedKernel_Hardware_Multiprocessing")] public static bool EnableProcessSwitching = false;

        /// <summary>
        ///     Exception state for during interrupt handlers.
        /// </summary>
        [Group(Name = "IsolatedKernel_Hardware_Multiprocessing")] public static ExceptionState* InterruptsExState;

        public static bool InsideCriticalHandler
        {
            [NoDebug] [NoGC] get { return insideCriticalHandler; }
            [NoDebug]
            [NoGC]
            set
            {
                if (value)
                {
                    insideCriticalHandler = true;
                    GC.UseCurrentState = false;
                    ExceptionMethods.UseCurrentState = false;

                    //wasPrintingMessages = ExceptionMethods.PrintMessages;
                    //ExceptionMethods.PrintMessages = false;

                    Heap.PreventAllocation = true;
                    Heap.PreventReason = "Inside critical interrupt handler.";
                    GC.Disable("InsideCriticalHandler");
                }
                else
                {
                    GC.Enable("InsideCriticalHandler");
                    Heap.PreventReason = "[NONE]";
                    Heap.PreventAllocation = false;

                    //ExceptionMethods.PrintMessages = wasPrintingMessages;

                    ExceptionMethods.UseCurrentState = true;
                    GC.UseCurrentState = true;
                    insideCriticalHandler = false;
                }
            }
        }

        static Interrupts()
        {
            InterruptsExState =
                (ExceptionState*)Heap.AllocZeroed((uint)sizeof(ExceptionState), "Interrupts : Interrupts()");
        }

        [PluggedMethod(ASMFilePath = null)]
        public static void EnableInterrupts()
        {
        }

        [PluggedMethod(ASMFilePath = null)]
        public static void DisableInterrupts()
        {
        }

        /// <summary>
        ///     Enables the specified IRQ number (0-15)
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
                byte mask = IOPort.doRead_Byte(0xA1);
                byte bitMask = (byte)~(1u << num);
                mask &= bitMask;
                //Port 0xA1 for slave PIC
                IOPort.doWrite_Byte(0xA1, mask);
                
                // Enable IRQ2 to enable interrupts from slave PIC
                EnableIRQ(2);
            }
            //Else we send the info to the master PIC
            else
            {
                //See above.
                byte mask = IOPort.doRead_Byte(0x21);
                byte bitMask = (byte)~(1u << num);
                mask &= bitMask;
                //Port 0x21 for master PIC
                IOPort.doWrite_Byte(0x21, mask);
            }
        }

        /// <summary>
        ///     Disables the specified IRQ number (0-15)
        /// </summary>
        /// <param name="num">The IRQ to disable.</param>
        public static void DisableIRQ(byte num)
        {
            //This functions the same as EnableIRQ except it sets 
            //  the corresponding bit instead of clearing it.
            if (num > 7)
            {
                num -= 8;

                byte mask = IOPort.doRead_Byte(0xA1);
                byte bitMask = (byte)(1u << num);
                mask |= bitMask;
                IOPort.doWrite_Byte(0xA1, mask);

                if (mask == 0xFF)
                {
                    // Disable IRQ2 to disable interrupts from slave PIC
                    DisableIRQ(2);
                }
            }
            else
            {
                byte mask = IOPort.doRead_Byte(0x21);
                byte bitMask = (byte)(1u << num);
                mask |= bitMask;
                IOPort.doWrite_Byte(0x21, mask);
            }
        }

        /// <summary>
        ///     Common method called to handle all interrupts (excluding numbers 0-16 inclusive).
        /// </summary>
        /// <param name="ISRNum">The number of the interrupt which occurred.</param>
        [NoGC]
        [NoDebug]
        private static void CommonISR(uint ISRNum)
        {
            InsideCriticalHandler = true;

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
                if (ExceptionMethods.CurrentException != null)
                {
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }
            }

            InsideCriticalHandler = false;
        }

        [NoDebug]
        [NoGC]
        private static void HandleISR(uint ISRNum)
        {
            Process currProcess = ProcessManager.CurrentProcess;
            Thread currThread = ProcessManager.CurrentThread;
            bool switched = false;

            try
            {
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
                        if (handlerProcess.ISRHandler == null)
                        {
                            BasicConsole.WriteLine(
                                "Error! handlerProcess.ISRHandler is null but is set to handle the ISR.");
                        }
                        else if (HandleResult(handlerProcess.ISRHandler(ISRNum), handlerProcess))
                        {
                            break;
                        }
                    }
                }
            }
            finally
            {
                if (switched)
                {
                    ProcessManager.SwitchProcess(currProcess.Id, (int)currThread.Id);
                }
            }
        }

        [NoDebug]
        [NoGC]
        private static void HandleIRQ(uint IRQNum)
        {
            Process currProcess = ProcessManager.CurrentProcess;
            Thread currThread = ProcessManager.CurrentThread;
            bool switched = false;

            try
            {
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

                        if (handlerProcess.IRQHandler == null)
                        {
                            BasicConsole.WriteLine(
                                "Error! handlerProcess.IRQHandler is null but is set to handle the IRQ.");
                        }
                        else if (HandleResult(handlerProcess.IRQHandler(IRQNum), handlerProcess))
                        {
                            break;
                        }
                    }
                }
            }
            finally
            {
                if (switched)
                {
                    ProcessManager.SwitchProcess(currProcess.Id, (int)currThread.Id);
                }

                EndIRQ(IRQNum > 7);
            }
        }

        private static bool HandleResult(int result, Process handlerProcess)
        {
            byte ActionByte = (byte)(result & 0xFF);
            uint ValueWord = (uint)(result & 0xFFFFFF00) >> 8;
            SystemCallResults ResultCall = (SystemCallResults)ActionByte;

            if (ResultCall == SystemCallResults.RequestAction_SignalSemaphore)
            {
                ProcessManager.Semaphore_Signal((int)ValueWord, handlerProcess);
            }
            else if (ResultCall == SystemCallResults.RequestAction_WakeThread)
            {
                ProcessManager.WakeThread(handlerProcess, ValueWord);
            }

            return ResultCall != SystemCallResults.OK_PermitActions && ResultCall != SystemCallResults.Deferred_PermitActions;
        }

        /// <summary>
        ///     Sends the End of Interrupt to the PIC to signify the end of an IRQ.
        /// </summary>
        /// <param name="slave">Whether to send the EOI to the slave PIC too.</param>
        [PluggedMethod(ASMFilePath = null)]
        private static void EndIRQ(bool slave)
        {
        }

        /// <summary>
        ///     Gets a pointer to the interrupt descriptor table.
        /// </summary>
        /// <returns>The pointer to the IDT.</returns>
        [PluggedMethod(ASMFilePath = @"ASM\Interrupts\Interrupts")]
        private static InterruptDescriptor* GetIDTPtr()
        {
            return null;
        }

        /// <summary>
        ///     Invokes the specified interrupt number. Warning: Invoking interrupts 0 through 16 has the same
        ///     affect as if the actual hardware exception occurred. Invoking interrupts 0 through 16 will result
        ///     in exception logic or page fault logic being executed!
        /// </summary>
        /// <param name="IntNum">The interrupt number to fire.</param>
        [PluggedMethod(ASMFilePath = null)]
        public static void InvokeInterrupt(uint IntNum)
        {
        }
    }
}
