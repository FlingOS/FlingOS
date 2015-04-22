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
            BasicConsole.WriteLine("PATAPI IRQ occurred!");
            BasicConsole.DelayOutput(10);
            IRQInvoked = true;
        }

        private void WaitForIRQ()
        {
            IRQInvoked = false;
            while (!IRQInvoked)
                Processes.Thread.Sleep(5);
        }

        public override void ReadBlock(ulong aBlockNo, uint aBlockCount, byte[] aData)
        {
            ExceptionMethods.Throw(new FOS_System.Exceptions.NotSupportedException("Cannot read from PATAPI device (yet)!"));

            // Select the drive
            BaseDevice.SelectDrive(0, false);

            // Read the data
            for(uint i = 0; i < aBlockCount; i++)
            {
                _ReadBlock(aBlockNo + i, aData, (uint)(i * blockSize));
            }
        }
        private void _ReadBlock(ulong aBlockNo, byte[] aData, uint DataOffset)
        {
            // Setup the packet
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
            BaseDevice.IO.LBA1.Write_Byte((byte)blockSize); // Low byte
            BaseDevice.IO.LBA1.Write_Byte((byte)(blockSize >> 8)); // High byte

            // Send the packet command
            BaseDevice.SendCmd(PATABase.Cmd.Packet);

            // Send the data
            BaseDevice.IO.Data.Write_UInt16s(atapi_packet);

            // Wait for the IRQ
            WaitForIRQ();

            // Wait for Busy to clear and check status
            PATABase.Status xStatus;
            int timeout = 1000;
            do
            {
                BaseDevice.Wait();
                xStatus = (PATABase.Status)BaseDevice.IO.Status.Read_Byte();
            } while ((xStatus & PATABase.Status.Busy) != 0 &&
                     (xStatus & PATABase.Status.Error) == 0 &&
                     timeout-- > 0);

            // Error occurred
            if ((xStatus & (PATABase.Status.Error | PATABase.Status.ATA_SR_DF)) != 0 ||
                (xStatus & (PATABase.Status.DRQ)) == 0)
            {
                ExceptionMethods.Throw(new FOS_System.Exception("ATAPI read error! Status bits incorrect in first check."));
            }

            // Read the data
            for(uint i = 0; i < aData.Length; i++)
            {
                UInt16 val = BaseDevice.IO.Data.Read_UInt16();
                aData[DataOffset + (i * 2)] = (byte)(val);
                aData[DataOffset + (i * 2) + 1] = (byte)(val >> 8);
            }

            // Wait for IRQ
            WaitForIRQ();

            // Wait for Busy and DRQ to clear and check status
            timeout = 1000;
            do
            {
                BaseDevice.Wait();
                xStatus = (PATABase.Status)BaseDevice.IO.Status.Read_Byte();
            } while ((xStatus & (PATABase.Status.Busy | PATABase.Status.DRQ)) != 0 &&
                     (xStatus & PATABase.Status.Error) == 0 &&
                     timeout-- > 0);

            // Error occurred
            if ((xStatus & (PATABase.Status.Error | PATABase.Status.ATA_SR_DF)) != 0 ||
                (xStatus & (PATABase.Status.DRQ)) == 0)
            {
                ExceptionMethods.Throw(new FOS_System.Exception("ATAPI read error! Status bits incorrect in second check."));
            }
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
