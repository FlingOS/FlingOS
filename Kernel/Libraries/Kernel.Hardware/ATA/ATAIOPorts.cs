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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.ATA
{
    /// <summary>
    /// Wraps the IO Ports for communicating with ATA devices.
    /// </summary>
    public class ATAIOPorts : FOS_System.Object
    {
        /// <summary>
        /// The data port.
        /// </summary>
        public readonly IO.IOPort Data;
        
        // Error Register: BAR0 + 1; // Read Only
        // Features Register: BAR0 + 1; // Write Only
        
        /// <summary>
        /// The sector count.
        /// </summary>
        public readonly IO.IOPort SectorCount;

        /********* HOB = High Order Byte = LBA48 mode *********/

        // ATA_REG_SECCOUNT1  0x08 - HOB
        /// <summary>
        /// LBA0 port.
        /// </summary>
        public readonly IO.IOPort LBA0;
        /// <summary>
        /// LBA1 port.
        /// </summary>
        public readonly IO.IOPort LBA1;
        /// <summary>
        /// LBA2 port.
        /// </summary>
        public readonly IO.IOPort LBA2;
        // ATA_REG_LBA3       0x09 - HOB
        // ATA_REG_LBA4       0x0A - HOB
        // ATA_REG_LBA5       0x0B - HOB
        /// <summary>
        /// Device select port.
        /// </summary>
        public readonly IO.IOPort DeviceSelect;
        /// <summary>
        /// Command port.
        /// </summary>
        public readonly IO.IOPort Command;
        /// <summary>
        /// Status port.
        /// </summary>
        public readonly IO.IOPort Status;
        
        /// <summary>
        /// Control port. Read only.
        /// </summary>
        /// <remarks>
        /// Control port is also the Alternate Status Register: BAR1 + 2.
        /// </remarks>
        public readonly IO.IOPort Control;
        //* DEVADDRESS: BAR1 + 3; //Don't know what this register is for

        /// <summary>
        /// Initialises a new ATA IO device including the various ports.
        /// </summary>
        /// <param name="isSecondary">Whether the device is a secondary ATA device.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        internal ATAIOPorts(bool isSecondary)
        {
            //BAR of main registers
            UInt16 xBAR0 = (UInt16)(isSecondary ? 0x0170 : 0x01F0);
            //BAR of alternative registers
            UInt16 xBAR1 = (UInt16)(isSecondary ? 0x0374 : 0x03F4);
            Data = new IO.IOPort(xBAR0);
            SectorCount = new IO.IOPort(xBAR0, 2);
            //Logical block address
            LBA0 = new IO.IOPort(xBAR0, 3); //Lo-bits
            LBA1 = new IO.IOPort(xBAR0, 4); //Mid-bits
            LBA2 = new IO.IOPort(xBAR0, 5); //Hi-bits
            DeviceSelect = new IO.IOPort(xBAR0, 6);
            //Write - command
            Command = new IO.IOPort(xBAR0, 7);
            //Read - status
            Status = new IO.IOPort(xBAR0, 7);

            Control = new IO.IOPort(xBAR1, 2);
        }
    }
}
