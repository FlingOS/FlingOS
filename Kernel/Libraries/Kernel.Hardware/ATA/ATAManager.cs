#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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
    /// Provides methods for managing ATA access.
    /// </summary>
    public static class ATAManager
    {
        /// <summary>
        /// ATA primary IO device.
        /// </summary>
        private static readonly ATAIO ATAIO1 = new ATAIO(false);
        /// <summary>
        /// ATA secondary IO device.
        /// </summary>
        private static readonly ATAIO ATAIO2 = new ATAIO(true);

        /// <summary>
        /// Initialises all available ATA devices on the primary bus.
        /// </summary>
        public static void Init()
        {
            //Try to initialise primary IDE:ATA drives.
            InitDrive(ATA.ControllerID.Primary, ATA.BusPosition.Slave);
            InitDrive(ATA.ControllerID.Primary, ATA.BusPosition.Master);
            
            //TODO: Detect if secondary drives present and init them if they are
        }

        /// <summary>
        /// Initialises a particular drive on the ATA bus.
        /// </summary>
        /// <param name="ctrlId">The controller ID of the device.</param>
        /// <param name="busPos">The bus position of the device.</param>
        public static void InitDrive(ATA.ControllerID ctrlId, ATA.BusPosition busPos)
        {
            //Get the IO ports for the correct bus
            ATAIO theIO = ctrlId == ATA.ControllerID.Primary ? ATAIO1 : ATAIO2;
            //Create / init the device on the bus
            ATAPio theATAPio = new ATAPio(theIO, ctrlId, busPos);
            //If the device was detected as present:
            if (theATAPio.DriveType != ATAPio.SpecLevel.Null)
            {
                //Add it to the list of devices.
                DeviceManager.Devices.Add(theATAPio);
            }
        }
    }
}
