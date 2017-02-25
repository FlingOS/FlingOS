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

using Drivers.Compiler.Attributes;
using Kernel.Framework;
using Kernel.Devices;

namespace Kernel.ATA
{
    /// <summary>
    ///     Wraps the IO Ports for communicating with ATA devices.
    /// </summary>
    public class ATAIOPorts : Object
    {
        /// <summary>
        ///     Command port.
        /// </summary>
        public readonly IOPort Command;

        /// <summary>
        ///     Control port. Read only.
        /// </summary>
        /// <remarks>
        ///     Control port is also the Alternate Status Register: BAR1 + 2.
        /// </remarks>
        public readonly IOPort Control;

        /// <summary>
        ///     The data port.
        /// </summary>
        public readonly IOPort Data;

        /// <summary>
        ///     Device select port.
        /// </summary>
        public readonly IOPort DeviceSelect;

        /// <summary>
        ///     Error register - read only.
        /// </summary>
        public readonly IOPort Error;

        /// <summary>
        ///     Features register - write only.
        /// </summary>
        public readonly IOPort Features;

        /// <summary>
        ///     LBA0 port.
        /// </summary>
        public readonly IOPort LBA0;

        /// <summary>
        ///     LBA1 port.
        /// </summary>
        public readonly IOPort LBA1;

        /// <summary>
        ///     LBA2 port.
        /// </summary>
        public readonly IOPort LBA2;

        /* TODO: LBA48 mode
         ********* HOB = High Order Byte = LBA48 mode *********
         *  ATA_REG_SECCOUNT1  0x08 - HOB
         *  ATA_REG_LBA3       0x09 - HOB
         *  ATA_REG_LBA4       0x0A - HOB
         *  ATA_REG_LBA5       0x0B - HOB
         ******************************************************/

        /// <summary>
        ///     The sector count.
        /// </summary>
        public readonly IOPort SectorCount;

        /// <summary>
        ///     Status port.
        /// </summary>
        public readonly IOPort Status;

        //* DEVADDRESS: BAR1 + 3; //Don't know what this register is for

        /// <summary>
        ///     Initialises a new ATA IO device including the various ports.
        /// </summary>
        /// <param name="IsSecondary">Whether the device is a secondary ATA device.</param>
        [NoDebug]
        internal ATAIOPorts(bool IsSecondary)
        {
            //BAR of main registers
            ushort BAR0Address = (ushort)(IsSecondary ? 0x0170 : 0x01F0);
            //BAR of alternative registers
            ushort BAR1Address = (ushort)(IsSecondary ? 0x0374 : 0x03F4);
            Data = new IOPort(BAR0Address);
            Error = new IOPort(BAR0Address, 1);
            Features = Error;
            SectorCount = new IOPort(BAR0Address, 2);
            //Logical block address
            LBA0 = new IOPort(BAR0Address, 3); //Lo-bits
            LBA1 = new IOPort(BAR0Address, 4); //Mid-bits
            LBA2 = new IOPort(BAR0Address, 5); //Hi-bits
            DeviceSelect = new IOPort(BAR0Address, 6);
            //Write - command
            Command = new IOPort(BAR0Address, 7);
            //Read - status
            Status = Command;

            Control = new IOPort(BAR1Address, 2);
        }
    }
}