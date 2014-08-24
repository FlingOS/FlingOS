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
        public int IdGenerator = 0;
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
        public int id;
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
        /// <summary>
        /// The full list of interrupt handlers. This array has an entry for all 256 interrupt numbers.
        /// If an entry is empty, then it does not have (/has never had) any handlers attached.
        /// Note: Interrupts 0 to 16 (inclusive) are set up in the IDT assembler and do not call the 
        /// common handler in this class so cannot be handled without code-modifications.
        /// </summary>
        private static InterruptHandlers[] Handlers = new InterruptHandlers[256];

        /// <summary>
        /// Enables the specified IRQ number (0-15)
        /// </summary>
        /// <param name="num">The IRQ to enable.</param>
        public static void EnableIRQ(byte num)
        {
            if (num > 7)
            {
                num -= 8;

                byte mask = IO.IOPort.doRead_Byte(0xA1);
                byte bitMask = (byte)(~(1u << num));
                mask &= bitMask;
                IO.IOPort.doWrite_Byte(0xA1, mask);
            }
            else
            {
                byte mask = IO.IOPort.doRead_Byte(0x21);
                byte bitMask = (byte)(~(1u << num));
                mask &= bitMask;
                IO.IOPort.doWrite_Byte(0x21, mask);
            }
        }
        /// <summary>
        /// Disables the specified IRQ number (0-15)
        /// </summary>
        /// <param name="num">The IRQ to disable.</param>
        public static void DisableIRQ(byte num)
        {
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
        /// <returns>The Id of the new handler. Save and use for removal.</returns>
        public static int AddIRQHandler(int num, InterruptHandler handler,
                                         FOS_System.Object data)
        {
            int result = AddISRHandler(num + 32, handler, data);
            EnableIRQ((byte)num);
            return result;
        }
        /// <summary>
        /// Removes the handler with the specified Id and disables the IRQ if there are no handlers left.
        /// </summary>
        /// <param name="num">The IRQ number to remove from.</param>
        /// <param name="id">The id of the handler to remove.</param>
        public static void RemoveIRQHandler(int num, int id)
        {
            RemoveISRHandler(num + 32, id);

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
                                         FOS_System.Object data)
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
                id = id
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

        /// <summary>
        /// Common method called to handle all interrupts (excluding numbers 0-16 inclusive).
        /// </summary>
        /// <param name="ISRNum">The number of the interrupt which occurred.</param>
        private static void CommonISR(uint ISRNum)
        {
            try
            {
#if INTERRUPTS_TRACE
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine(((FOS_System.String)"ISR: ") + ISRNum);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
#endif

                InterruptHandlers handlers = Handlers[ISRNum];
                if (handlers != null)
                {
                    for (int i = 0; i < handlers.HandlerDescrips.Count; i++)
                    {
                        HandlerDescriptor descrip = (HandlerDescriptor)handlers.HandlerDescrips[i];
                        InterruptHandler func = descrip.handler;
                        func(descrip.data);
                    }
                }

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
