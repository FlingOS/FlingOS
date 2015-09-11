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
using Kernel.FOS_System.Collections;

namespace Kernel.Hardware.USB.Devices
{
    /// <summary>
    /// Represents a USB device.
    /// </summary>
    public class USBDevice : Device
    {
        /// <summary>
        /// The device info that specifies the physical device to communicate with.
        /// </summary>
        public USBDeviceInfo DeviceInfo
        {
            get;
            protected set;
        }

        /// <summary>
        /// Initialises a new USB device with specified USB device info. Includes adding it to Devices lists in
        /// device manager and USB manager.
        /// </summary>
        /// <param name="aDeviceInfo">The device info of the physical device.</param>
        public USBDevice(USBDeviceInfo aDeviceInfo)
        {
            DeviceInfo = aDeviceInfo;

            DeviceManager.AddDevice(this);
            USBManager.Devices.Add(this);
        }

        /// <summary>
        /// Destroys the USB device including removing it from Devices lists in device manager and USB manager.
        /// </summary>
        public virtual void Destroy()
        {
            DeviceManager.Devices.Remove(this);
            USBManager.Devices.Remove(this);
        }
    }
    /// <summary>
    /// Stores information which describes a particular, physical USB device.
    /// </summary>
    public class USBDeviceInfo : FOS_System.Object
    {
        /// <summary>
        /// The port number of the device.
        /// </summary>
        public byte portNum;
        /// <summary>
        /// The device address.
        /// </summary>
        public byte address;
        /// <summary>
        /// The host controller to which the device is attached.
        /// </summary>
        public HCIs.HCI hc;

        public List Configurations;
        public List Interfaces;
        /// <summary>
        /// The list of "Endpoint"s that has been detected on the USB device.
        /// </summary>
        public List Endpoints;

        /// <summary>
        /// The USb specification number reported by the device.
        /// </summary>
        public ushort usbSpec;
        /// <summary>
        /// The USb class code reported by the device.
        /// </summary>
        public byte usbClass;
        /// <summary>
        /// The USB sub-class code reported by the device.
        /// </summary>
        public byte usbSubclass;
        /// <summary>
        /// The USB protocol version number reported by the device.
        /// </summary>
        public byte usbProtocol;
        /// <summary>
        /// The device Vendor ID.
        /// </summary>
        public ushort vendor;
        /// <summary>
        /// The device product ID.
        /// </summary>
        public ushort product;
        /// <summary>
        /// The device release number.
        /// </summary>
        public ushort releaseNumber;
        /// <summary>
        /// The manufacturer's string Id.
        /// </summary>
        public byte manufacturerStringID;
        /// <summary>
        /// The product's string Id.
        /// </summary>
        public byte productStringID;
        /// <summary>
        /// The serial number string Id.
        /// </summary>
        public byte serialNumberStringID;
        /// <summary>
        /// The number of configurations reported by the device.
        /// </summary>
        public byte numConfigurations;

        /// <summary>
        /// The interface class code reported by the device.
        /// </summary>
        public byte InterfaceClass;
        /// <summary>
        /// The interface sub-class code reported by the device.
        /// </summary>
        public byte InterfaceSubclass;

        public UnicodeString ManufacturerString;
        public UnicodeString ProductString;
        public UnicodeString SerialNumberString;

        /// <summary>
        /// The mass storage bulk-transfer interface number, if any and only if the device is an MSD.
        /// </summary>
        public byte MSD_InterfaceNum;
        /// <summary>
        /// The mass storage bulk-transfer IN-direction enpdoint Id, if any and only if the device is an MSD.
        /// </summary>
        public byte MSD_INEndpointID;
        /// <summary>
        /// The mass storage bulk-transfer OUT-direction enpdoint Id, if any and only if the device is an MSD.
        /// </summary>
        public byte MSD_OUTEndpointID;

        /// <summary>
        /// Initialises a new USB Device Info but does not attempt to get/fill out the information.
        /// That job is left to the USB manager.
        /// </summary>
        /// <param name="aPortNum">The port number of the physical device.</param>
        /// <param name="aHC">The host controller which the device is connected to.</param>
        public USBDeviceInfo(byte aPortNum, HCIs.HCI aHC)
        {
            portNum = aPortNum;
            hc = aHC;
        }

        /// <summary>
        /// Frees the port and destroys the USB device instance.
        /// </summary>
        public void FreePort()
        {
            HCIs.HCPort port = hc.GetPort(portNum);
            if (port.device != null)
            {
                port.device.Destroy();
                port.device = null;
            }
            port.deviceInfo = null;
            port.connected = false;
            port.speed = HCIs.USBPortSpeed.UNSET;
        }
    }
}
