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
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.Keyboards
{
    /// <summary>
    /// Represents a PS2 keyboard device.
    /// </summary>
    public class PS2 : Devices.Keyboard
    {
        /// <summary>
        /// The keyboard data port.
        /// </summary>
        protected IO.IOPort DataPort = new IO.IOPort(0x60);
        /// <summary>
        /// The interrupt handler Id returned when the interrupt handler is set.
        /// Use to remove the interrupt handler when disabling.
        /// </summary>
        protected int InterruptHandlerId;
        
        /// <summary>
        /// Enables the PS2 keyboard.
        /// </summary>
        public override void Enable()
        {
            if (!enabled)
            {
                InterruptHandlerId = Interrupts.Interrupts.AddIRQHandler(1, InterruptHandler, this);
                DeviceManager.Devices.Add(this);
                enabled = true;
            }
        }
        /// <summary>
        /// Disables the PS2 keyboard.
        /// </summary>
        public override void Disable()
        {
            if (enabled)
            {
                DeviceManager.Devices.Remove(this);
                Interrupts.Interrupts.RemoveIRQHandler(1, InterruptHandlerId);
                enabled = false;
            }
        }

        /// <summary>
        /// The internal interrupt handler static wrapper.
        /// </summary>
        /// <param name="data">The PS2 keyboard state object.</param>
        private static void InterruptHandler(FOS_System.Object data)
        {
            ((PS2)data).InterruptHandler();
        }
        /// <summary>
        /// The internal interrupt handler.
        /// </summary>
        private void InterruptHandler()
        {
            byte scanCode = DataPort.Read_Byte();
            bool released = (scanCode & 0x80) == 0x80;
            if (released)
            {
                scanCode = (byte)(scanCode ^ 0x80);
            }
            HandleScancode(scanCode, released);
        }
        /// <summary>
        /// Handles the specified scancode.
        /// </summary>
        /// <param name="scancode">The scancode to handle.</param>
        /// <param name="released">Whether the key has been released or not.</param>
        private void HandleScancode(uint scancode, bool released)
        {
            switch (scancode)
            {
                case 0x36:
                case 0x2A:
                    {
                        shiftPressed = !released;
                        break;
                    }
                case 0x1D:
                    {
                        ctrlPressed = !released;
                        break;
                    }
                case 0x38:
                    {
                        altPressed = !released;
                        break;
                    }
                default:
                    {
                        if ((ctrlPressed) && (altPressed) && (scancode == 0x53))
                        {
                            //TODO: Remove this Ctrl+Alt+Delete hack
                            Console.WriteLine("Detected Ctrl-Alt-Delete! Disabling keyboard.");
                            Disable();
                        }
                        if (shiftPressed)
                        {
                            scancode = scancode << 16;
                        }
                        if (!released)
                        {
                            Enqueue(scancode);
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// The (only) PS2 keyboard instance.
        /// </summary>
        public static PS2 ThePS2 = null;
        /// <summary>
        /// Initialises the (only) PS2 instance.
        /// </summary>
        public static void Init()
        {
            if (ThePS2 == null)
            {
                ThePS2 = new PS2();
            }
            ThePS2.Enable();
        }
        /// <summary>
        /// Cleans up the (only) PS2 instance.
        /// </summary>
        public static void Clean()
        {
            if(ThePS2 != null)
            {
                ThePS2.Disable();
            }
        }
    }
}
