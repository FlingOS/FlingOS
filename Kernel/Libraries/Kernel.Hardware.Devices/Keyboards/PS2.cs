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

using Kernel.Hardware.Devices;
using Kernel.Hardware.IO;

namespace Kernel.Hardware.Keyboards
{
    /// <summary>
    ///     Represents a PS2 keyboard device.
    /// </summary>
    public class PS2 : Keyboard
    {
        /// <summary>
        ///     The (only) PS2 keyboard instance.
        /// </summary>
        public static PS2 ThePS2 = null;

        /// <summary>
        ///     The keyboard command port.
        /// </summary>
        protected IOPort CommandPort = new IOPort(0x64);

        /// <summary>
        ///     The keyboard data port.
        /// </summary>
        protected IOPort DataPort = new IOPort(0x60);

        /// <summary>
        ///     Enables the PS2 keyboard.
        /// </summary>
        public override void Enable()
        {
            if (!enabled)
            {
                enabled = true;
            }
        }

        /// <summary>
        ///     Disables the PS2 keyboard.
        /// </summary>
        public override void Disable()
        {
            if (enabled)
            {
                //TODO: This should be done through a DeviceManager.Deregister system call.
                //TODO: This needs un-commenting and fixing
                //TODO: Hmm.. perhaps this should actually be in the Clean function? To reflect what happens in Init
                //DeviceManager.Devices.Remove(this);
                enabled = false;
            }
        }

        /// <summary>
        ///     The internal interrupt handler.
        /// </summary>
        public void InterruptHandler()
        {
            byte scancode = DataPort.Read_Byte();
            HandleScancode(scancode);
        }

        /// <summary>
        ///     Handles the specified scancode.
        /// </summary>
        /// <param name="scancode">The scancode to handle.</param>
        /// <param name="released">Whether the key has been released or not.</param>
        public override void HandleScancode(uint scancode)
        {
            //Determine whether the key has been released or not
            bool released = (scancode & 0x80) == 0x80;
            //If it has:
            if (released)
            {
                //Clear the released bit so we get the correct key scancode
                scancode = (byte) (scancode ^ 0x80);
            }

            //And handle the (now corrected) scancode
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

        public void Reset()
        {
            // If the driver is enabled
            if (enabled)
            {
                // Wait for the Input Buffer Full flag to clear
                byte StatusRegValue = 0x02;
                while ((StatusRegValue & 0x02) != 0)
                {
                    StatusRegValue = CommandPort.Read_Byte();
                }

                // Send the command | options 
                //          (0xF0   | 0x0E    - pulse only line 0 - CPU reset line)
                CommandPort.Write_Byte(0xFE);
            }
        }

        /// <summary>
        ///     Initialises the (only) PS2 instance.
        /// </summary>
        public static void Init()
        {
            if (ThePS2 == null)
            {
                ThePS2 = new PS2();
                DeviceManager.RegisterDevice(ThePS2);
            }
            ThePS2.Enable();
        }

        /// <summary>
        ///     Cleans up the (only) PS2 instance.
        /// </summary>
        public static void Clean()
        {
            if (ThePS2 != null)
            {
                ThePS2.Disable();
            }
        }
    }
}