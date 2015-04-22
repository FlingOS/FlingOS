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
        private static readonly ATAIOPorts ATAIO1 = new ATAIOPorts(false);
        /// <summary>
        /// ATA secondary IO device.
        /// </summary>
        private static readonly ATAIOPorts ATAIO2 = new ATAIOPorts(true);

        /// <summary>
        /// Initialises all available ATA devices on the primary bus.
        /// </summary>
        public static void Init()
        {
            //Try to initialise primary IDE:PATA/PATAPI drives.
            InitDrive(ATA.ControllerID.Primary, ATA.BusPosition.Slave);
            InitDrive(ATA.ControllerID.Primary, ATA.BusPosition.Master);

            InitDrive(ATA.ControllerID.Secondary, ATA.BusPosition.Slave);
            InitDrive(ATA.ControllerID.Secondary, ATA.BusPosition.Master);

            //TODO: Init SATA/SATAPI devices by enumerating the PCI bus.
        }

        /// <summary>
        /// Initialises a particular drive on the ATA bus.
        /// </summary>
        /// <param name="ctrlId">The controller ID of the device.</param>
        /// <param name="busPos">The bus position of the device.</param>
        public static void InitDrive(ATA.ControllerID ctrlId, ATA.BusPosition busPos)
        {
            //Get the IO ports for the correct bus
            ATAIOPorts theIO = ctrlId == ATA.ControllerID.Primary ? ATAIO1 : ATAIO2;
            //Create / init the device on the bus
            PATABase ThePATABase = new PATABase(theIO, ctrlId, busPos);
            //If the device was detected as present:
            if (ThePATABase.DriveType != PATABase.SpecLevel.Null)
            {
                //If the device was actually a PATA device:
                if (ThePATABase.DriveType == PATABase.SpecLevel.PATA)
                {
                    //Add it to the list of devices.
                    DeviceManager.AddDevice(new PATA(ThePATABase));
                }
                else if (ThePATABase.DriveType == PATABase.SpecLevel.PATAPI)
                {
                    // Add a PATAPI device
                    DeviceManager.AddDevice(new PATAPI(ThePATABase));
                }
                //TODO: Remove the SATA/SATAPI initialisation from here. It should be done
                //  in the ATAManager.Init method.
                else if (ThePATABase.DriveType == PATABase.SpecLevel.SATA)
                {
                    // Add a SATA device
                    DeviceManager.AddDevice(new SATA());
                }
                else if (ThePATABase.DriveType == PATABase.SpecLevel.SATAPI)
                {
                    // Add a SATAPI device
                    DeviceManager.AddDevice(new SATAPI());
                }
            }
        }
    }
}
