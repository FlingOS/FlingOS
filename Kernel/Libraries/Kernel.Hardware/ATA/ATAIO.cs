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

namespace Kernel.Hardware.ATA
{
    /// <summary>
    /// Represents an ATA IO device.
    /// </summary>
    public class ATAIO : FOS_System.Object
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
        //* Alternate Status Register: BAR1 + 2; // Read Only.
        /// <summary>
        /// Control port.
        /// </summary>
        public readonly IO.IOPort Control;
        //* DEVADDRESS: BAR1 + 2; //Don't know what this register is for

        /// <summary>
        /// Initialises a new ATA IO device including the various ports.
        /// </summary>
        /// <param name="isSecondary">Whether the device is a secondary ATA device.</param>
        [Compiler.NoDebug]
        internal ATAIO(bool isSecondary)
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
