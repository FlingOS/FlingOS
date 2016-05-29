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

using Kernel.IO;

namespace Kernel.Devices.Keyboards
{
    /// <summary>
    ///     Represents a PS2 keyboard device.
    /// </summary>
    /// <remarks>
    ///     Since only one PS2 keyboard can exist at a time, the singleton pattern is used in this class
    ///     to ensure only a since PS2 keyboard driver can be created.
    /// 
    ///     This does not, of course, limit multiple instances being created in different processes. This
    ///     only applies within a given process. 
    /// 
    ///     TODO: The PS2 controller should be added to the system's devices list at startup (if it's available)
    ///     TODO: This driver should attempt to claim control of the PS2 device from the system. 
    ///           Failure to claim will indicate the PS2 controller is either already in use or does not exist.
    /// </remarks>
    public sealed class PS2 : Keyboard
    {
        /// <summary>
        ///     The (only) PS2 keyboard instance.
        /// </summary>
        public static PS2 SingletonPS2;

        /// <summary>
        ///     The keyboard command port.
        /// </summary>
        private readonly IOPort CommandPort = new IOPort(0x64);

        /// <summary>
        ///     The keyboard data port.
        /// </summary>
        private readonly IOPort DataPort = new IOPort(0x60);

        /// <summary>
        ///     Initialises a new PS2 instance.
        /// </summary>
        /// <remarks>
        ///     The constructor is made private so that a PS2 object can
        ///     only be created within this class. This allows us to 
        ///     enforce the singleton requirement.
        /// </remarks>
        private PS2()
        {
        }

        /// <summary>
        ///     Enables the PS2 keyboard.
        /// </summary>
        public override void Enable()
        {
            if (!Enabled)
            {
                Enabled = true;
            }
        }

        /// <summary>
        ///     Disables the PS2 keyboard.
        /// </summary>
        public override void Disable()
        {
            if (Enabled)
            {
                //TODO: This should be done through a DeviceManager.Deregister system call.
                //TODO: This needs un-commenting and fixing
                //TODO: Hmm.. perhaps this should actually be in the Clean function? To reflect what happens in Init
                //DeviceManager.Devices.Remove(this);
                Enabled = false;
            }
        }

        /// <summary>
        ///     The internal interrupt handler.
        /// </summary>
        public void InterruptHandler()
        {
            byte Scancode = DataPort.Read_Byte();
            HandleScancode(Scancode);
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

        /// <summary>
        ///     Pulses the CPU Reset line using the PS2 controller chip. 
        ///     This will cause a physical system reset which should result in a reboot but the actual
        ///     effect will depend upon the BIOS configuration.
        /// </summary>
        /// <remarks>
        ///     This is not the recommended way to reboot an x86 system. It is, however, the easiest.
        ///     Proper reboot is done via ACPI (Advanced Configuration and Power Interface) but that
        ///     is tricky to implement. 
        /// 
        ///     Note that on some very new or specialist systems, this method will not work as the PS2 
        ///     controller will not exist. However, it should work on most systems (even those without
        ///     a usable PS2 port) because, for legacy reasons, they still contain a PS2 controller 
        ///     (or at least an emulator of one).
        /// </remarks>
        public void Reset()
        {
            // If the driver is enabled
            if (Enabled)
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
            if (SingletonPS2 == null)
            {
                SingletonPS2 = new PS2();
                DeviceManager.RegisterDevice(SingletonPS2);
            }
            SingletonPS2.Enable();
        }

        /// <summary>
        ///     Cleans up the (only) PS2 instance.
        /// </summary>
        public static void Clean() => SingletonPS2?.Disable();
    }
}