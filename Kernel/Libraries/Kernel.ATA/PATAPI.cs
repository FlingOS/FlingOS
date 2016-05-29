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

//#define PATAPI_TRACE

using Kernel.Devices;
using Kernel.ATA.Exceptions;
using Kernel.Framework;
using Kernel.Framework.Exceptions;
using Kernel.Framework.Processes;
using Kernel.Framework.Processes.Requests.Devices;

namespace Kernel.ATA
{
    /// <summary>
    ///     Driver for handling PATAPI disks.
    /// </summary>
    public class PATAPI : DiskDevice
    {
        /// <summary>
        ///     Whether IRQ 14 has been invoked or not.
        /// </summary>
        private static bool _IRQ14Invoked;
        /// <summary>
        ///     Whether IRQ 15 has been invoked or not.
        /// </summary>
        private static bool _IRQ15Invoked;
        /// <summary>
        ///     The underlying PATA device that this PATAPI driver is wrapping.
        /// </summary>
        protected PATABase BaseDevice;

        /// <summary>
        ///     Initialises a new PATAPI driver for the specified device.
        /// </summary>
        /// <param name="BaseDevice">The PATAPI device to be wrapped.</param>
        public PATAPI(PATABase BaseDevice)
            : base(DeviceGroup.Storage, DeviceClass.Storage, DeviceSubClass.ATA, "PATAPI Disk", BaseDevice.Info, true)
        {
            this.BaseDevice = BaseDevice;

            // Enable IRQs - required for PATAPI
            this.BaseDevice.SelectDrive(0, false);
            this.BaseDevice.IO.Control.Write_Byte(0x00);

            //Note: IRQHandler is hooked from DeviceManagerTask
            //TODO: Delegate for the handler should be passed up to Device Manager from this class
        }

        /// <summary>
        ///     The serial number of the device. <seealso cref="PATABase.SerialNo"/>
        /// </summary>
        public String SerialNo => BaseDevice.SerialNo;

        public String FirmwareRev => BaseDevice.FirmwareRev;

        public String ModelNo => BaseDevice.ModelNo;

        public override ulong BlockCount => BaseDevice.BlockCount;

        public override ulong BlockSize => BaseDevice.BlockSize;

        public uint MaxWritePioBlocks => BaseDevice.MaxWritePioBlocks;

        private bool IRQInvoked
        {
            get { return BaseDevice.ControllerId == ATA.ControllerIds.Primary ? _IRQ14Invoked : _IRQ15Invoked; }
            set
            {
                if (BaseDevice.ControllerId == ATA.ControllerIds.Primary)
                {
                    _IRQ14Invoked = value;
                }
                else
                {
                    _IRQ15Invoked = value;
                }
            }
        }

        public static void IRQHandler(uint irqNumber)
        {
            if (irqNumber == 14)
            {
                _IRQ14Invoked = true;
            }
            else if (irqNumber == 15)
            {
                _IRQ15Invoked = true;
            }
        }

        private bool WaitForIRQ()
        {
            int timeout = 20;
            while (!IRQInvoked && timeout-- > 0)
                SystemCalls.SleepThread(5);

            return timeout == 0;
        }

        public override void ReadBlock(ulong aBlockNo, uint aBlockCount, byte[] aData)
        {
            //ExceptionMethods.Throw(new Framework.Exceptions.NotSupportedException("Cannot read from PATAPI device (yet)!"));

            // Reset IRQ (by reading status register)
#if PATAPI_TRACE
            BasicConsole.WriteLine("Reset IRQ");
#endif
            BaseDevice.IO.Status.Read_Byte();
            IRQInvoked = false;

            // Select the drive
#if PATAPI_TRACE
            BasicConsole.WriteLine("Select drive");
#endif
            BaseDevice.SelectDrive(0, false);

            // Read the data
            for (uint i = 0; i < aBlockCount; i++)
            {
#if PATAPI_TRACE
                BasicConsole.WriteLine("Read block");
#endif
                _ReadBlock(aBlockNo + i, aData, (uint) (i*BlockSize));
            }
        }

        private void _ReadBlock(ulong aBlockNo, byte[] aData, uint DataOffset)
        {
            // Setup the packet
#if PATAPI_TRACE
            BasicConsole.WriteLine("Setup ATAPI packet");
#endif
            byte[] atapi_packet = new byte[12];
            atapi_packet[0] = 0xA8;
            atapi_packet[1] = 0x0;
            atapi_packet[2] = (byte) (aBlockNo >> 24);
            atapi_packet[3] = (byte) (aBlockNo >> 16);
            atapi_packet[4] = (byte) (aBlockNo >> 8);
            atapi_packet[5] = (byte) (aBlockNo >> 0);
            atapi_packet[6] = 0x0;
            atapi_packet[7] = 0x0;
            atapi_packet[8] = 0x0;
            atapi_packet[9] = 1;
            atapi_packet[10] = 0x0;
            atapi_packet[11] = 0x0;

            // Inform the controller we are using PIO mode
#if PATAPI_TRACE
            BasicConsole.WriteLine("Tell controller we are using PIO mode");
#endif
            BaseDevice.IO.Features.Write_Byte(0);

            // Tell the drive the buffer size
#if PATAPI_TRACE
            BasicConsole.WriteLine("Tell drive the buffer size");
#endif
            BaseDevice.IO.LBA1.Write_Byte((byte) BlockSize); // Low byte
            BaseDevice.IO.LBA1.Write_Byte((byte) (BlockSize >> 8)); // High byte

            // Send the packet command (includes the wait)
#if PATAPI_TRACE
            BasicConsole.WriteLine("Send Packet command");
#endif
            PATABase.Status xStatus = BaseDevice.SendCmd(PATABase.Cmd.Packet);

            // Error occurred
#if PATAPI_TRACE
            BasicConsole.WriteLine("Check for error");
#endif
            if ((xStatus & PATABase.Status.Error) != 0)
            {
#if PATAPI_TRACE
                BasicConsole.WriteLine("Error detected");
#endif
                ExceptionMethods.Throw(new NoDiskException("ATAPI read error! Status bits incorrect in first check."));
            }

            // Check if that invoked an IRQ - it shouldn't have
#if PATAPI_TRACE
            BasicConsole.WriteLine("Check if IRQ invoked");
#endif
            if (IRQInvoked)
            {
#if PATAPI_TRACE
                BasicConsole.WriteLine("IRQ had been invoked");
#endif
                // Allow future IRQs by reading Status register
                BaseDevice.IO.Status.Read_Byte();
                IRQInvoked = false;
            }

            // Send the data
#if PATAPI_TRACE
            BasicConsole.WriteLine("Write packet data");
#endif
            BaseDevice.IO.Data.Write_UInt16s(atapi_packet);

            // Wait a bit
#if PATAPI_TRACE
            BasicConsole.WriteLine("Brief wait");
#endif
            BaseDevice.Wait();

            // Wait for the IRQ
#if PATAPI_TRACE
            BasicConsole.WriteLine("Wait for IRQ");
#endif
            if (WaitForIRQ())
            {
#if PATAPI_TRACE
                BasicConsole.WriteLine("Error! Wait for IRQ timed out.");
                BasicConsole.DelayOutput(5);
#endif
            }

            // Wait for Busy to clear and check alternate status
#if PATAPI_TRACE
            BasicConsole.WriteLine("Wait till not busy");
#endif
            uint timeout = 0xF0000000;
            do
            {
                BaseDevice.Wait();
                xStatus = (PATABase.Status) BaseDevice.IO.Control.Read_Byte();
            } while ((xStatus & PATABase.Status.Busy) != 0 &&
                     (xStatus & PATABase.Status.Error) == 0 &&
                     timeout-- > 0);

            // Read status reg to clear IRQ
#if PATAPI_TRACE
            BasicConsole.WriteLine("Read status");
#endif
            xStatus = (PATABase.Status) BaseDevice.IO.Status.Read_Byte();
            IRQInvoked = false;

            // Error occurred
#if PATAPI_TRACE
            BasicConsole.WriteLine("Check for error");
#endif
            if ((xStatus & (PATABase.Status.Error | PATABase.Status.ATA_SR_DF)) != 0 ||
                (xStatus & PATABase.Status.DRQ) == 0)
            {
#if PATAPI_TRACE
                BasicConsole.WriteLine("Error detected");
#endif
                ExceptionMethods.Throw(new NoDiskException("ATAPI read error! Status bits incorrect in second check."));
            }

            // Read the data
#if PATAPI_TRACE
            BasicConsole.WriteLine("Read the data");
            BasicConsole.WriteLine("Length: " + (Framework.String)aData.Length);
#endif
            uint offset = DataOffset + 1;
            uint i = 0;
            for (; i < BlockSize && offset < aData.Length; i += 2, offset += 2)
            {
                ushort val = BaseDevice.IO.Data.Read_UInt16();
                aData[offset - 1] = (byte) val;
                aData[offset] = (byte) (val >> 8);
            }
            // Clear out any remaining data
            for (; i < BlockSize; i++)
            {
                BaseDevice.IO.Data.Read_UInt16();
            }

#if PATAPI_TRACE
            unsafe
            {
                BasicConsole.DumpMemory((byte*)Utilities.ObjectUtilities.GetHandle(aData) + Framework.Array.FieldsBytesSize, aData.Length);
            }

            BasicConsole.WriteLine("Wait for IRQ");
#endif
            // Wait for IRQ
            if (WaitForIRQ())
            {
#if PATAPI_TRACE
                BasicConsole.WriteLine("Error! Wait for IRQ timed out. (1)");
                BasicConsole.DelayOutput(5);
#endif
            }

            // Wait for Busy and DRQ to clear and check status
#if PATAPI_TRACE
            BasicConsole.WriteLine("Wait till not busy");
#endif
            timeout = 0xF0000000;
            do
            {
                BaseDevice.Wait();
                xStatus = (PATABase.Status) BaseDevice.IO.Control.Read_Byte();
            } while ((xStatus & (PATABase.Status.Busy | PATABase.Status.DRQ)) != 0 &&
                     (xStatus & PATABase.Status.Error) == 0 &&
                     timeout-- > 0);

            // Error occurred
#if PATAPI_TRACE
            BasicConsole.WriteLine("Check for error");
#endif
            if ((xStatus & (PATABase.Status.Error | PATABase.Status.ATA_SR_DF)) != 0 ||
                (xStatus & PATABase.Status.DRQ) != 0)
            {
#if PATAPI_TRACE
                BasicConsole.WriteLine("Error detected");
#endif
                ExceptionMethods.Throw(new Exception("ATAPI read error! Status bits incorrect in third check."));
            }

#if PATAPI_TRACE
            BasicConsole.WriteLine("Complete");
            BasicConsole.DelayOutput(10);
#endif
        }

        public override void WriteBlock(ulong aBlockNo, uint aBlockCount, byte[] aData)
        {
            //TODO: Implement PATAPI.WriteBlock
            ExceptionMethods.Throw(new NotSupportedException("Cannot write to PATAPI device!"));
        }

        public override void CleanCaches()
        {
            //TODO: Implement PATAPI.CleanCaches when PATAPI.WriteBlock is implemented
        }
    }
}