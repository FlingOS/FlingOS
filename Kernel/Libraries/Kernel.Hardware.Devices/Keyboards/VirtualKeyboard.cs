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
    
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.Keyboards
{
    public class VirtualKeyboard : Devices.Keyboard
    {
        /// <summary>
        /// Enables the virtual keyboard.
        /// </summary>
        public override void Enable()
        {
            if (!enabled)
            {
                enabled = true;
            }
        }
        /// <summary>
        /// Disables the virtual keyboard.
        /// </summary>
        public override void Disable()
        {
            if (enabled)
            {
                enabled = false;
            }
        }

        public override void HandleScancode(uint scancode)
        {
            //Determine whether the key has been released or not
            bool released = (scancode & 0x80) == 0x80;
            //If it has:
            if (released)
            {
                //Clear the released bit so we get the correct key scancode
                scancode = (byte)(scancode ^ 0x80);
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
    }
}
