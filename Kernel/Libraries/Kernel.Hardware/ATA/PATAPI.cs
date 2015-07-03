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
    public class PATAPI : Devices.DiskDevice
    {
        protected PATABase BaseDevice;

        public FOS_System.String SerialNo
        {
            get { return BaseDevice.SerialNo; }
        }
        public FOS_System.String FirmwareRev
        {
            get { return BaseDevice.FirmwareRev; }
        }
        public FOS_System.String ModelNo
        {
            get { return BaseDevice.ModelNo; }
        }

        public override ulong BlockCount
        {
            get
            {
                return BaseDevice.BlockCount;
            }
        }
        public override ulong BlockSize
        {
            get
            {
                return BaseDevice.BlockSize;
            }
        }

        private bool IRQInvoked = false;

        public PATAPI(PATABase baseDevice)
        {
            BaseDevice = baseDevice;

            blockSize = BaseDevice.BlockSize;

            // Enable IRQs - required for PATAPI
            BaseDevice.SelectDrive(0, false);
            BaseDevice.IO.Control.Write_Byte((byte)0x00);

            if (BaseDevice.controllerId == ATA.ControllerID.Primary)
            {
                Interrupts.Interrupts.AddIRQHandler(14, PATAPI.IRQHandler, this, true, true, "PATAPI IRQ 14");
            }
            else
            {
                Interrupts.Interrupts.AddIRQHandler(15, PATAPI.IRQHandler, this, true, true, "PATAPI IRQ 15");
            }
        }

        private static void IRQHandler(FOS_System.Object state)
        {
            ((PATAPI)state).IRQHandler();
        }
        private void IRQHandler()
        {
            //BasicConsole.WriteLine("PATAPI IRQ occurred!");
            //BasicConsole.DelayOutput(10);
            IRQInvoked = true;
        }

        private bool WaitForIRQ()
        {
            int timeout = 20;
            while (!IRQInvoked && timeout-- > 0)
                Processes.Thread.Sleep(5);

            return timeout == 0;
        }

        public override void ReadBlock(ulong aBlockNo, uint aBlockCount, byte[] aData)
        {
            ExceptionMethods.Throw(new FOS_System.Exceptions.NotSupportedException("Cannot read from PATAPI device (yet)!"));

            // Reset IRQ (by reading status register)
            //BasicConsole.WriteLine("Reset IRQ");
            BaseDevice.IO.Status.Read_Byte();
            IRQInvoked = false;

            // Select the drive
            //BasicConsole.WriteLine("Select drive");
            BaseDevice.SelectDrive(0, false);

            // Read the data
            for(uint i = 0; i < aBlockCount; i++)
            {
                //BasicConsole.WriteLine("Read block");
                _ReadBlock(aBlockNo + i, aData, (uint)(i * blockSize));
            }
        }
        private void _ReadBlock(ulong aBlockNo, byte[] aData, uint DataOffset)
        {
            // Setup the packet
            //BasicConsole.WriteLine("Setup ATAPI packet");
            byte[] atapi_packet = new byte[12];
            atapi_packet[0] = 0xA8;
            atapi_packet[1] = 0x0;
            atapi_packet[2] = (byte)(aBlockNo >> 24);
            atapi_packet[3] = (byte)(aBlockNo >> 16);
            atapi_packet[4] = (byte)(aBlockNo >> 8);
            atapi_packet[5] = (byte)(aBlockNo >> 0);
            atapi_packet[6] = 0x0;
            atapi_packet[7] = 0x0;
            atapi_packet[8] = 0x0;
            atapi_packet[9] = 1;
            atapi_packet[10] = 0x0;
            atapi_packet[11] = 0x0;

            // Tell the drive the buffer size
            //BasicConsole.WriteLine("Tell drive the buffer size");
            BaseDevice.IO.LBA1.Write_Byte((byte)blockSize); // Low byte
            BaseDevice.IO.LBA1.Write_Byte((byte)(blockSize >> 8)); // High byte

            // Send the packet command
            //BasicConsole.WriteLine("Send Packet command");
            BaseDevice.SendCmd(PATABase.Cmd.Packet);

            // Wait till the device is not busy
            //BasicConsole.WriteLine("Wait till not busy");
            PATABase.Status xStatus;
            int timeout = 1000;
            do
            {
                BaseDevice.Wait();
                xStatus = (PATABase.Status)BaseDevice.IO.Control.Read_Byte();
            } while ((xStatus & PATABase.Status.Busy) != 0 &&
                     (xStatus & PATABase.Status.Error) == 0 &&
                     timeout-- > 0);

            // Error occurred
            //BasicConsole.WriteLine("Check for error");
            if ((xStatus & PATABase.Status.Error) != 0 || timeout == 0)
            {
                //BasicConsole.WriteLine("Error detected");
                ExceptionMethods.Throw(new FOS_System.Exception("ATAPI read error! Status bits incorrect in first check."));
            }

            // Check if that invoke an IRQ - it shouldn't have
            //BasicConsole.WriteLine("Check if IRQ invoked");
            if (IRQInvoked)
            {
                //BasicConsole.WriteLine("IRQ had been invoked");
                // Allow future IRQs by reading Status register
                BaseDevice.IO.Status.Read_Byte();
                IRQInvoked = false;
            }

            // Send the data
            //BasicConsole.WriteLine("Write packet data");
            BaseDevice.IO.Data.Write_UInt16s(atapi_packet);

            // Wait a bit
            //BasicConsole.WriteLine("Brief wait");
            BaseDevice.Wait();

            // Wait for the IRQ
            //BasicConsole.WriteLine("Wait for IRQ");
            if (WaitForIRQ())
            {
                //BasicConsole.WriteLine("Error! Wait for IRQ timed out.");
                //BasicConsole.DelayOutput(5);
            }

            // Wait for Busy to clear and check alternate status
            //BasicConsole.WriteLine("Wait till not busy");
            timeout = 1000;
            do
            {
                BaseDevice.Wait();
                xStatus = (PATABase.Status)BaseDevice.IO.Control.Read_Byte();
            } while ((xStatus & PATABase.Status.Busy) != 0 &&
                     (xStatus & PATABase.Status.Error) == 0 &&
                     timeout-- > 0);

            // Read status reg to clear IRQ
            //BasicConsole.WriteLine("Read status");
            xStatus = (PATABase.Status)BaseDevice.IO.Status.Read_Byte();
            IRQInvoked = false;

            // Error occurred
            //BasicConsole.WriteLine("Check for error");
            if ((xStatus & (PATABase.Status.Error | PATABase.Status.ATA_SR_DF)) != 0 ||
                (xStatus & PATABase.Status.DRQ) == 0)
            {
                //BasicConsole.WriteLine("Error detected");
                ExceptionMethods.Throw(new FOS_System.Exception("ATAPI read error! Status bits incorrect in first check."));
            }

            // Read the data
            //BasicConsole.WriteLine("Read the data");
            //BasicConsole.WriteLine("Length: " + (FOS_System.String)aData.Length);
            uint i = 0;
            for(; i < blockSize && i < aData.Length; i += 2)
            {
                UInt16 val = BaseDevice.IO.Data.Read_UInt16();
                //BasicConsole.WriteLine(i + 1);
                aData[DataOffset + i] = (byte)(val);
                aData[DataOffset + i + 1] = (byte)(val >> 8);
            }
            // Clear out any remaining data
            for (; i < blockSize; i++)
            {
                BaseDevice.IO.Data.Read_UInt16();
            }

            // Wait for IRQ
            //BasicConsole.WriteLine("Wait for IRQ");
            if(WaitForIRQ())
            {
                //BasicConsole.WriteLine("Error! Wait for IRQ timed out. (1)");
                //BasicConsole.DelayOutput(5);
            }

            // Wait for Busy and DRQ to clear and check status
            //BasicConsole.WriteLine("Wait till not busy");
            timeout = 1000;
            do
            {
                BaseDevice.Wait();
                xStatus = (PATABase.Status)BaseDevice.IO.Control.Read_Byte();
            } while ((xStatus & (PATABase.Status.Busy | PATABase.Status.DRQ)) != 0 &&
                     (xStatus & PATABase.Status.Error) == 0 &&
                     timeout-- > 0);

            // Error occurred
            //BasicConsole.WriteLine("Check for error");
            if ((xStatus & (PATABase.Status.Error | PATABase.Status.ATA_SR_DF)) != 0 ||
                (xStatus & (PATABase.Status.DRQ)) == 0)
            {
                //BasicConsole.WriteLine("Error detected");
                ExceptionMethods.Throw(new FOS_System.Exception("ATAPI read error! Status bits incorrect in second check."));
            }

            //BasicConsole.WriteLine("Complete");
            //BasicConsole.DelayOutput(10);
        }

        public override void WriteBlock(ulong aBlockNo, uint aBlockCount, byte[] aData)
        {
            ExceptionMethods.Throw(new FOS_System.Exceptions.NotSupportedException("Cannot write to PATAPI device (yet)!"));
        }

        public override void CleanCaches()
        {
        }
    }
}
