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
            //We wouldn't want to accidentally add the IRQ handler multiple times
            //  because then any one scancode would be processed multiple times!
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
                //As per requirements, set temp sote store of id to 0 to prevent
                //  accidental multiple removal.
                InterruptHandlerId = 0;
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
            //Get the scancode we are being notified of
            byte scanCode = DataPort.Read_Byte();
            //Determine whether the key has been released or not
            bool released = (scanCode & 0x80) == 0x80;
            //If it has:
            if (released)
            {
                //Clear the released bit so we get the correct key scancode
                scanCode = (byte)(scanCode ^ 0x80);
            }
            //And handle the (now corrected) scancode and pass in whether the key was
            //  released or not.
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
                //Left and right shift keys
                case 0x36:
                case 0x2A:
                    {
                        shiftPressed = !released;
                        break;
                    }
                //Ctrl key
                case 0x1D:
                    {
                        ctrlPressed = !released;
                        break;
                    }
                //Alt key
                case 0x38:
                    {
                        altPressed = !released;
                        break;
                    }
                //All other keys
                default:
                    {
                        //If the key was just pressed, enqueue it
                        if (!released)
                        {
                            //If shift pressed, adjust the scancode appropriately.
                            if (shiftPressed)
                            {
                                scancode = scancode << 16;
                            }

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
