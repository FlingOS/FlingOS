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

namespace Kernel.ATA
{
    /// <summary>
    ///     Test methods for testing the ATA drivers.
    /// </summary>
    public class ATATests : Test
    {
        /// <summary>
        ///     Tests reading a lot of data from the first available PATA device.
        /// </summary>
        /// <param name="OutputMessage">Delegate method for outputting informational messages.</param>
        /// <param name="OutputWarning">Delegate method for outputting warning messages.</param>
        /// <param name="OutputError">Delegate method for outputting error messages.</param>
        public void Test_LongRead(OutputMessageDelegate OutputMessage, OutputWarningDelegate OutputWarning,
            OutputErrorDelegate OutputError)
        {
            OutputMessage("ATATests : Test_LongRead", "Test started.");

            // The device we are going to test
            PATA TestDevice = null;

            // Search for PATA device
            OutputMessage("ATATests : Test_LongRead", "Searching for PATA device...");
            for (int i = 0; i < ATAManager.Devices.Count; i++)
            {
                Device ADevice = (Device)ATAManager.Devices[i];
                if (ADevice is PATA)
                {
                    TestDevice = (PATA)ADevice;
                    break;
                }
            }

            // Check that we found a device
            if (TestDevice == null)
            {
                OutputWarning("ATATests : Test_LongRead", "No PATA device found. Aborting test.");
                return;
            }
            OutputMessage("ATATests : Test_LongRead", (String)"Device found. Controller ID: " +
                                                      (TestDevice.ControllerIds == ATA.ControllerIds.Primary
                                                          ? "Primary"
                                                          : "Secondary") + " , Position: " +
                                                      (TestDevice.BusPosition == ATA.BusPositions.Master
                                                          ? "Master"
                                                          : "Slave"));

            // Create a buffer for storing up to 16 blocks of data
            OutputMessage("ATATests : Test_LongRead", "Creating data buffer...");
            byte[] Buffer = new byte[32*(int)(uint)TestDevice.BlockSize];
            OutputMessage("ATATests : Test_LongRead", "done.");

            try
            {
                OutputMessage("ATATests : Test_LongRead", "Calculating statistical data...");
                ulong FractionOfDisk = Math.Divide(TestDevice.Blocks, 10);
                ulong PercentileOfFraction = Math.Divide(FractionOfDisk, 100);
                ulong Dist = 0;
                bool A = true;
                OutputMessage("ATATests : Test_LongRead", "done.");

                OutputMessage("ATATests : Test_LongRead", "Reading disk 32 sectors at a time...");
                // Attempt to read every sector of the disk 32 at a time
                for (ulong i = 0; i < FractionOfDisk; i += 32, Dist += 32)
                {
                    TestDevice.ReadBlock(i, 32, Buffer);

                    if (Dist >= PercentileOfFraction)
                    {
                        Dist -= PercentileOfFraction;

                        OutputMessage("ATATests : Test_LongRead", A ? "[+1% complete] (a)" : "[+1% complete] (b)");
                        A = !A;
                    }
                }
                OutputMessage("ATATests : Test_LongRead", "done.");
            }
            catch
            {
                OutputError("ATATests : Test_LongRead", ExceptionMethods.CurrentException.Message);
            }


            OutputMessage("ATATests : Test_LongRead", "Test complete.");
        }
    }
}