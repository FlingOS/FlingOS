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
    
#define INTERRUPTS_TRACE
#undef INTERRUPTS_TRACE

using System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware.Processes;

namespace Kernel.Hardware.Interrupts
{
    /// <summary>
    /// Strcture for an interrupt descriptor in the Interrupts Descriptor Table (IDT).
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct InterruptDescriptor
    {
        /// <summary>
        /// Handler address low-byte.
        /// </summary>
        public ushort OffsetLo;
        /// <summary>
        /// TODO
        /// Selector...hmm...I dunno and don't have an internet connection at the moment so 
        /// can't look up what the spec says. Meh, I'm sure it's not too important :)
        /// </summary>
        public ushort Selector;
        /// <summary>
        /// Like the name says...
        /// </summary>
        public byte UNUSED;
        /// <summary>
        /// Interrupt type.
        /// </summary>
        public byte Type_S_DPL_P;
        /// <summary>
        /// Handler address high-byte.
        /// </summary>
        public ushort OffsetHi;
    }
    /// <summary>
    /// Stores information about the handlers for a given interrupt.
    /// </summary>
    public class InterruptHandlers : FOS_System.Object
    {
        /// <summary>
        /// The list of handlers for the interrupt.
        /// </summary>
        public List HandlerDescrips = new List(1);
        /// <summary>
        /// Used to generate a unique Id number for each interrupt handler for this interrupt.
        /// </summary>
        /// <remarks>
        /// As a safety precaution, all Ids start at one and go upwards. 0 is reserved as an 
        /// invalid Id. After removing a handler, any temporary store of handler Id should be
        /// set to 0 so code cannot attempt to remove a handler it does not own by accidentally
        /// trying to remove its handler twice.
        /// </remarks>
        public int IdGenerator = 1;

        public int QueuedOccurrences = 0; 
        public int QueuedOccurrencesOld = 0;
    }
    /// <summary>
    /// Represents a handler for an interrupt.
    /// </summary>
    public class HandlerDescriptor : FOS_System.Object
    {
        /// <summary>
        /// The (static) method to call to handle the interrupt.
        /// </summary>
        public InterruptHandler handler;
        /// <summary>
        /// The state object to pass the handler.
        /// </summary>
        public FOS_System.Object data;
        /// <summary>
        /// The Id of this handler. Used primarily for removal.
        /// </summary>
        /// <remarks>
        /// As a safety precaution, all Ids start at one and go upwards. 0 is reserved as an 
        /// invalid Id. After removing a handler, any temporary store of handler Id should be
        /// set to 0 so code cannot attempt to remove a handler it does not own by accidentally
        /// trying to remove its handler twice.
        /// </remarks>
        public int id;
        
        public FOS_System.String Name;
    }
    /// <summary>
    /// Delegate type for an interrupt handler. Interrupt handlers must be static, like all methods used in 
    /// delegates in the core kernel.
    /// </summary>
    /// <param name="data"></param>
    public delegate void InterruptHandler(FOS_System.Object data);
    /// <summary>
    /// Provides methods for handling hardware and software interrupts (excluding interrupts 0 through 16).
    /// </summary>
    public unsafe static class Interrupts
    {
        //TODO - This lot is all x86 specific. It needs to be abstracted into a separate x86
        //       interrupts class to support new architectures.

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

        public static bool EnableProcessSwitching = false;

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
//        /// <summary>
//        /// Adds a handler to the specified IRQ and enables the IRQ.
//        /// </summary>
//        /// <param name="num">The IRQ number (0-15) to add a handler for.</param>
//        /// <param name="handler">The handler method to call when the interrupt occurs (must be a static).</param>
//        /// <param name="data">The state object to pass the handler when the interrupt occurs.</param>
//        /// <returns>The Id of the new handler. Save and use for removal. An Id of 0 s invalid.</returns>
//        public static int AddIRQHandler(int num, InterruptHandler handler, FOS_System.Object data, FOS_System.String Name)
//        {
//            //In this OS's implementation, IRQs 0-15 are mapped to ISRs 32-47
//            int result = AddISRHandler(num + 32, handler, data, Name);
//            EnableIRQ((byte)num);
//            return result;
//        }
//        /// <summary>
//        /// Removes the handler with the specified Id and disables the IRQ if there are no handlers left.
//        /// You should set any temporary store of <paramref name="id"/> to 0 since 0 is an invalid Id
//        /// it will prevent you from accidentally trying to remove the handler twice.
//        /// </summary>
//        /// <param name="num">The IRQ number to remove from.</param>
//        /// <param name="id">The id of the handler to remove.</param>
//        public static void RemoveIRQHandler(int num, int id)
//        {
//            //In this OS's implementation, IRQs 0-15 are mapped to ISRs 32-47
//            RemoveISRHandler(num + 32, id);

//            //We only want to disable the IRQ if nothing is handling it
//            if (Handlers[num + 32].HandlerDescrips.Count == 0)
//            {
//                DisableIRQ((byte)num);
//            }
//        }
//        /// <summary>
//        /// Adds a handler to the specified interrupt number.
//        /// </summary>
//        /// <param name="num">The interrupt to add a handler for.</param>
//        /// <param name="handler">The handler method to call when the interrupt occurs (must be a static).</param>
//        /// <param name="data">The state object to pass the handler when the interrupt occurs.</param>
//        /// <returns>The Id of the new handler. Save and use for removal.</returns>
//        public static int AddISRHandler(int num, InterruptHandler handler, FOS_System.Object data, FOS_System.String Name)
//        {
//#if INTERRUPTS_TRACE
//            BasicConsole.Write("Adding ISR handler for ");
//            BasicConsole.WriteLine(Name);
//            BasicConsole.DelayOutput(20);
//#endif 

//            if (Handlers[num] == null)
//            {
//#if INTERRUPTS_TRACE
//                BasicConsole.WriteLine("Creating new InterruptHandlers...");
//#endif
//                Handlers[num] = new InterruptHandlers();
//            }

//#if INTERRUPTS_TRACE
//            BasicConsole.WriteLine(((FOS_System.String)"Adding new HandlerDescriptor... ISR: ") + num);
//#endif

//            InterruptHandlers handlers = Handlers[num];
//            int id = handlers.IdGenerator++;
//            handlers.HandlerDescrips.Add(new HandlerDescriptor()
//            {
//                handler = handler,
//                data = data,
//                id = id,
//                Name = Name
//            });

//#if INTERRUPTS_TRACE
//            BasicConsole.WriteLine("Added.");
//#endif

//            return id;
//        }
//        /// <summary>
//        /// Removes the handler with the specified Id.
//        /// </summary>
//        /// <param name="num">The interrupt number to remove from.</param>
//        /// <param name="id">The id of the handler to remove.</param>
//        public static void RemoveISRHandler(int num, int id)
//        {
//            if (Handlers[num] != null)
//            {
//                InterruptHandlers handlers = Handlers[num];

//                //Search for the handler with the specified id.
//                //  Note: Id does not correspond to index since we could have removed
//                //        handlers with lower ids already.

//                for (int i = 0; i < handlers.HandlerDescrips.Count; i++)
//                {
//                    HandlerDescriptor descrip = (HandlerDescriptor)handlers.HandlerDescrips[i];
//                    if (descrip.id == id)
//                    {
//                        handlers.HandlerDescrips.RemoveAt(i);
//                    }
//                }
//            }
//        }
        
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

#if INTERRUPTS_TRACE
            //if (Processes.ProcessManager.Processes.Count > 1)
            //if (ISRNum == 33)
                BasicConsole.WriteLine("Interrupts: 19");
#endif
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
