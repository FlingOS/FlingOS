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

namespace Kernel.Devices.Keyboards
{
    /// <summary>
    ///     Represents a virtual keyboard. Virtual keyboards can be used to treat incoming data as key strokes
    ///     thus allowing you to process and access them like any other keyboard input. This is currently 
    ///     mostly used when connecting a process to the Window Manager.
    /// </summary>
    /// <remarks>
    ///     This class is currently very limiting and relies on the Window Manager to handle certain output
    ///     commands to a physical keyboard e.g. caps lock light. 
    /// 
    ///     TODO: Future work should allow the virtual keyboard to send commands back to the remote process
    ///           e.g. indicator light controls. 
    /// </remarks>
    public class VirtualKeyboard : Keyboard
    {
        /// <summary>
        ///     Enables the virtual keyboard.
        /// </summary>
        public override void Enable()
        {
            if (!Enabled)
            {
                Enabled = true;
            }
        }

        /// <summary>
        ///     Disables the virtual keyboard.
        /// </summary>
        public override void Disable()
        {
            if (Enabled)
            {
                Enabled = false;
            }
        }

        /// <summary>
        ///     Handles an incoming scancode by inspecting it and queuing it if necessary.
        /// </summary>
        /// <param name="Scancode">The incoming scancode.</param>
        public override void HandleScancode(uint Scancode)
        {
            //Determine whether the key has been released or not
            bool Released = (Scancode & 0x80) == 0x80;
            //If it has:
            if (Released)
            {
                //Clear the released bit so we get the correct key scancode
                Scancode = (byte)(Scancode ^ 0x80);
            }

            //And handle the (now corrected) scancode

            switch (Scancode)
            {
                //Left and right shift keys
                case 0x36:
                case 0x2A:
                {
                    ShiftPressed = !Released;
                    break;
                }
                //Ctrl key
                case 0x1D:
                {
                    CtrlPressed = !Released;
                    break;
                }
                //Alt key
                case 0x38:
                {
                    AltPressed = !Released;
                    break;
                }
                //All other keys
                default:
                {
                    //If the key was just pressed, enqueue it
                    if (!Released)
                    {
                        //If shift pressed, adjust the scancode appropriately.
                        if (ShiftPressed)
                        {
                            Scancode = Scancode << 16;
                        }

                        Enqueue(Scancode);
                    }
                    break;
                }
            }
        }
    }
}