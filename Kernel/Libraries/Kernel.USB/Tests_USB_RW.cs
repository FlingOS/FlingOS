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

using Kernel.Devices;
using Kernel.Devices.Testing;
using Kernel.Framework;
using Kernel.USB.Devices;

namespace Kernel.USB
{
    public class USBTests : Test
    {
        public void Test_LongRead(OutputMessageDelegate OutputMessage, OutputWarningDelegate OutputWarning,
            OutputErrorDelegate OutputError)
        {
            OutputMessage("USBTests : Test_LongRead", "Test started.");

            // The device we are going to test
            MassStorageDevice_DiskDevice TestDevice = null;

            // Search for USB device
            OutputMessage("USBTests : Test_LongRead", "Searching for USB device...");
            for (int i = 0; i < USBManager.Devices.Count; i++)
            {
                Device ADevice = (Device)USBManager.Devices[i];
                if (ADevice is MassStorageDevice_DiskDevice)
                {
                    TestDevice = (MassStorageDevice_DiskDevice)ADevice;
                    break;
                }
            }

            // Check that we found a device
            if (TestDevice == null)
            {
                OutputWarning("USBTests : Test_LongRead", "No USB device found. Aborting test.");
                return;
            }
            OutputMessage("USBTests : Test_LongRead", "Device found.");

            // Create a buffer for storing up to 16 blocks of data
            OutputMessage("USBTests : Test_LongRead", "Creating data buffer...");
            byte[] buffer = new byte[32*(int)(uint)TestDevice.BlockSize];
            OutputMessage("USBTests : Test_LongRead", "done.");

            try
            {
                OutputMessage("USBTests : Test_LongRead", "Calculating statistical data...");
                ulong FractionOfDisk = TestDevice.Blocks; // Framework.Math.Divide(TestDevice.BlockCount, 10);
                ulong PercentileOfFraction = Math.Divide(FractionOfDisk, 100);
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