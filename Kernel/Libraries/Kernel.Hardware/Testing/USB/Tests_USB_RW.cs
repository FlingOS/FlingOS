using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardwareATA = Kernel.Hardware.ATA;

namespace Kernel.Hardware.Testing
{
    public partial class USBTests : Test
    {
        public void Test_LongRead(OutputMessageDel OutputMessage, OutputWarningDel OutputWarning, OutputErrorDel OutputError)
        {
            OutputMessage("USBTests : Test_LongRead", "Test started.");

            // The device we are going to test
            USB.Devices.MassStorageDevice_DiskDevice TestDevice = null;

            // Search for USB device
            OutputMessage("USBTests : Test_LongRead", "Searching for USB device...");
            for (int i = 0; i < DeviceManager.Devices.Count; i++)
            {
                Device ADevice = (Device)DeviceManager.Devices[i];
                if (ADevice is USB.Devices.MassStorageDevice_DiskDevice)
                {
                    TestDevice = (USB.Devices.MassStorageDevice_DiskDevice)ADevice;
                    break;
                }
            }

            // Check that we found a device
            if (TestDevice == null)
            {
                OutputWarning("USBTests : Test_LongRead", "No USB device found. Aborting test.");
                return;
            }
            else
            {
                OutputMessage("USBTests : Test_LongRead", "Device found.");
            }

            // Create a buffer for storing up to 16 blocks of data
            OutputMessage("USBTests : Test_LongRead", "Creating data buffer...");
            byte[] buffer = new byte[32 * (int)(uint)TestDevice.BlockSize];
            OutputMessage("USBTests : Test_LongRead", "done.");

            try
            {
                OutputMessage("USBTests : Test_LongRead", "Calculating statistical data...");
                ulong FractionOfDisk = TestDevice.BlockCount;// FOS_System.Math.Divide(TestDevice.BlockCount, 10);
                ulong PercentileOfFraction = FOS_System.Math.Divide(FractionOfDisk, 100);
                ulong dist = 0;
                bool a = true;
                OutputMessage("USBTests : Test_LongRead", "done.");

                OutputMessage("USBTests : Test_LongRead", "Reading disk one sector at a time...");
                // Attempt to read every sector of the disk 1 at a time
                for (ulong i = 0; i < FractionOfDisk; i += 1, dist += 1)
                {
                    TestDevice.ReadBlock(i, 1, buffer);

                    if (dist >= PercentileOfFraction)
                    {
                        dist -= PercentileOfFraction;
                        if (a)
                        {
                            OutputMessage("USBTests : Test_LongRead", "[+1% complete] (a)");
                        }
                        else
                        {
                            OutputMessage("USBTests : Test_LongRead", "[+1% complete] (b)");
                        }

                        a = !a;
                    }
                }
                OutputMessage("USBTests : Test_LongRead", "done.");
            }
            catch
            {
                OutputError("USBTests : Test_LongRead", ExceptionMethods.CurrentException.Message);
            }


            OutputMessage("USBTests : Test_LongRead", "Test complete.");
        }
    }
}
