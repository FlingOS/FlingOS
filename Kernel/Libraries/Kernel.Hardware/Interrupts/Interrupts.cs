#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
#define INTERRUPTS_TRACE
#undef INTERRUPTS_TRACE

using System;
using Kernel.FOS_System.Collections;

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

        public bool IgnoreProcessId;
        public uint ProcessId;
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
    [Compiler.PluggedClass]
    public unsafe static class Interrupts
    {
        //TODO - This lot is all x86 specific. It needs to be abstracted into a separate x86
        //       interrupts class to support new architectures.

        /// <summary>
        /// The full list of interrupt handlers. This array has an entry for all 256 interrupt numbers.
        /// If an entry is empty, then it does not have (/has never had) any handlers attached.
        /// Note: Interrupts 0 to 16 (inclusive) are set up in the IDT assembler and do not call the 
        /// common handler in this class so cannot be handled without code-modifications.
        /// </summary>
        private static InterruptHandlers[] Handlers = new InterruptHandlers[256];

        [Compiler.PluggedMethod(ASMFilePath=null)]
        public static void EnableInterrupts()
        {
        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
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
        /// Adds a handler to the specified IRQ and enables the IRQ.
        /// </summary>
        /// <param name="num">The IRQ number (0-15) to add a handler for.</param>
        /// <param name="handler">The handler method to call when the interrupt occurs (must be a static).</param>
        /// <param name="data">The state object to pass the handler when the interrupt occurs.</param>
        /// <returns>The Id of the new handler. Save and use for removal. An Id of 0 s invalid.</returns>
        public static int AddIRQHandler(int num, InterruptHandler handler,
                                         FOS_System.Object data, bool IgnoreProcessState)
        {
            //In this OS's implementation, IRQs 0-15 are mapped to ISRs 32-47
            int result = AddISRHandler(num + 32, handler, data, IgnoreProcessState);
            EnableIRQ((byte)num);
            return result;
        }
        /// <summary>
        /// Removes the handler with the specified Id and disables the IRQ if there are no handlers left.
        /// You should set any temporary store of <paramref name="id"/> to 0 since 0 is an invalid Id
        /// it will prevent you from accidentally trying to remove the handler twice.
        /// </summary>
        /// <param name="num">The IRQ number to remove from.</param>
        /// <param name="id">The id of the handler to remove.</param>
        public static void RemoveIRQHandler(int num, int id)
        {
            //In this OS's implementation, IRQs 0-15 are mapped to ISRs 32-47
            RemoveISRHandler(num + 32, id);

            //We only want to disable the IRQ if nothing is handling it
            if (Handlers[num + 32].HandlerDescrips.Count == 0)
            {
                DisableIRQ((byte)num);
            }
        }
        /// <summary>
        /// Adds a handler to the specified interrupt number.
        /// </summary>
        /// <param name="num">The interrupt to add a handler for.</param>
        /// <param name="handler">The handler method to call when the interrupt occurs (must be a static).</param>
        /// <param name="data">The state object to pass the handler when the interrupt occurs.</param>
        /// <returns>The Id of the new handler. Save and use for removal.</returns>
        public static int AddISRHandler(int num, InterruptHandler handler,
                                         FOS_System.Object data, bool IgnoreProcessState)
        {
            if (Handlers[num] == null)
            {
#if INTERRUPTS_TRACE
                BasicConsole.WriteLine("Creating new InterruptHandlers...");
#endif
                Handlers[num] = new InterruptHandlers();
            }
#if INTERRUPTS_TRACE
            BasicConsole.WriteLine(((FOS_System.String)"Adding new HandlerDescriptor... ISR: ") + num);
#endif
            InterruptHandlers handlers = Handlers[num];
            int id = handlers.IdGenerator++;
            handlers.HandlerDescrips.Add(new HandlerDescriptor()
            {
                handler = handler,
                data = data,
                id = id,
                IgnoreProcessId = IgnoreProcessState,
                ProcessId = Processes.ProcessManager.CurrentProcess.Id
            });
#if INTERRUPTS_TRACE
            BasicConsole.WriteLine("Added.");
#endif
            return id;
        }
        /// <summary>
        /// Removes the handler with the specified Id.
        /// </summary>
        /// <param name="num">The interrupt number to remove from.</param>
        /// <param name="id">The id of the handler to remove.</param>
        public static void RemoveISRHandler(int num, int id)
        {
            if (Handlers[num] != null)
            {
                InterruptHandlers handlers = Handlers[num];

                //Search for the handler with the specified id.
                //  Note: Id does not correspond to index since we could have removed
                //        handlers with lower ids already.

                for (int i = 0; i < handlers.HandlerDescrips.Count; i++)
                {
                    HandlerDescriptor descrip = (HandlerDescriptor)handlers.HandlerDescrips[i];
                    if (descrip.id == id)
                    {
                        handlers.HandlerDescrips.RemoveAt(i);
                    }
                }
            }
        }
        
#if INTERRUPTS_TRACE
        public static bool print = false;
        public static uint lastisr = 0;
#endif

        /// <summary>
        /// Common method called to handle all interrupts (excluding numbers 0-16 inclusive).
        /// </summary>
        /// <param name="ISRNum">The number of the interrupt which occurred.</param>
        private static void CommonISR(uint ISRNum)
        {
            try
            {
#if INTERRUPTS_TRACE
                if (print && lastisr != ISRNum || ISRNum != 0x20)
                {
                    lastisr = ISRNum;
                    BasicConsole.SetTextColour(BasicConsole.warning_colour);
                    if (ISRNum == 0x20)
                    {
                        BasicConsole.WriteLine("ISR: 0x20");
                    }
                    else if (ISRNum == 0x21)
                    {
                        BasicConsole.WriteLine("ISR: 0x21");
                    }
                    else
                    {
                        BasicConsole.WriteLine("ISR: Unrecognised");
                    }
                    //BasicConsole.WriteLine(((FOS_System.String)"ISR: ") + ISRNum);
                    BasicConsole.SetTextColour(BasicConsole.default_colour);
                }
#endif
                uint currProcessId = 0;
                if (Processes.ProcessManager.CurrentProcess != null)
                {
                    currProcessId = Processes.ProcessManager.CurrentProcess.Id;
                }
                bool switched = false;

                //Go through any handlers and fire them
                InterruptHandlers handlers = Handlers[ISRNum];
                if (handlers != null)
                {
                    for (int i = 0; i < handlers.HandlerDescrips.Count; i++)
                    {
                        HandlerDescriptor descrip = (HandlerDescriptor)handlers.HandlerDescrips[i];
                        InterruptHandler func = descrip.handler;

                        if (Processes.ProcessManager.CurrentProcess != null)
                        {
                            if (!descrip.IgnoreProcessId)
                            {
                                Processes.ProcessManager.SwitchProcess(descrip.ProcessId, -1);
                                switched = true;
                            }
                        }
                        
                        func(descrip.data);
                    }
                }

                if (switched)
                {
                    Processes.ProcessManager.SwitchProcess(currProcessId, -1);
                }

                //If the ISR is actually an IRQ, we must also notify the PIC(s)
                //  that the IRQ has completed / been handled by sending the 
                //  End IRQ notification.
                if (ISRNum >= 32 && ISRNum <= 47)
                {
                    EndIRQ(ISRNum > 39);
                }
            }
            catch
            {
#if DEBUG
                BasicConsole.SetTextColour(BasicConsole.error_colour);
                BasicConsole.WriteLine(((FOS_System.String)"Error processing ISR: ") + ISRNum);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
#endif
            }
        }
        /// <summary>
        /// Sends the End of Interrupt to the PIC to signify the end of an IRQ.
        /// </summary>
        /// <param name="slave">Whether to send the EOI to the slave PIC too.</param>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static void EndIRQ(bool slave)
        {
        }

        /// <summary>
        /// Gets a pointer to the interrupt descriptor table.
        /// </summary>
        /// <returns>The pointer to the IDT.</returns>
        [Compiler.PluggedMethod(ASMFilePath=@"ASM\Interrupts\Interrupts")]
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
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static void InvokeInterrupt(uint IntNum)
        {
        }
    }
}
