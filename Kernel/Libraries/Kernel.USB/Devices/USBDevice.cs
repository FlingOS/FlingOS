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
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Processes.Requests.Devices;
using Kernel.USB.HCIs;

namespace Kernel.USB.Devices
{
    /// <summary>
    ///     Represents a USB device.
    /// </summary>
    public class USBDevice : Device
    {
        /// <summary>
        ///     The device info that specifies the physical device to communicate with.
        /// </summary>
        public USBDeviceInfo DeviceInfo { get; protected set; }

        /// <summary>
        ///     Initialises a new USB device with specified USB device info. Includes adding it to Devices lists in
        ///     device manager and USB manager.
        /// </summary>
        /// <param name="aDeviceInfo">The device info of the physical device.</param>
        public USBDevice(USBDeviceInfo aDeviceInfo, DeviceGroup group, DeviceClass @class, DeviceSubClass subClass,
            String name, bool IsClaimed)
            : base(group, @class, subClass, name, new uint[5], IsClaimed)
        {
            DeviceInfo = aDeviceInfo;

            Info[0] = aDeviceInfo.hc.Info[0];
            Info[1] = aDeviceInfo.hc.Info[1];
            Info[2] = aDeviceInfo.hc.Info[2];
            Info[3] = aDeviceInfo.portNum;
            Info[4] = aDeviceInfo.address;
        }

        /// <summary>
        ///     Destroys the USB device including removing it from Devices lists in device manager and USB manager.
        /// </summary>
        public virtual void Destroy()
        {
            //TODO: This should be done through a DeviceManager.Deregister system call.
            //TODO: This needs un-commenting and fixing
            //DeviceManager.Devices.Remove(this);
            USBManager.Devices.Remove(this);
        }
    }

    /// <summary>
    ///     Stores information which describes a particular, physical USB device.
    /// </summary>
    public class USBDeviceInfo : Object
    {
        /// <summary>
        ///     The device address.
        /// </summary>
        public byte address;

        /// <summary>
        ///     The list of Configurations that have been detected on the USB device.
        /// </summary>
        public List Configurations;

        /// <summary>
        ///     The list of Endpoints that have been detected on the USB device.
        /// </summary>
        public List Endpoints;

        /// <summary>
        ///     The host controller to which the device is attached.
        /// </summary>
        public HCI hc;

        /// <summary>
        ///     The interface class code reported by the device.
        /// </summary>
        public byte InterfaceClass;

        /// <summary>
        ///     The list of Interfaces that have been detected on the USB device.
        /// </summary>
        public List Interfaces;

        /// <summary>
        ///     The interface sub-class code reported by the device.
        /// </summary>
        public byte InterfaceSubclass;

        public UnicodeString ManufacturerString;

        /// <summary>
        ///     The manufacturer's string Id.
        /// </summary>
        public byte manufacturerStringID;

        /// <summary>
        ///     The mass storage bulk-transfer IN-direction enpdoint Id, if any and only if the device is an MSD.
        /// </summary>
        public byte MSD_INEndpointID;

        /// <summary>
        ///     The mass storage bulk-transfer interface number, if any and only if the device is an MSD.
        /// </summary>
        public byte MSD_InterfaceNum;

        /// <summary>
        ///     The mass storage bulk-transfer OUT-direction enpdoint Id, if any and only if the device is an MSD.
        /// </summary>
        public byte MSD_OUTEndpointID;

        /// <summary>
        ///     The number of configurations reported by the device.
        /// </summary>
        public byte numConfigurations;

        /// <summary>
        ///     The port number of the device.
        /// </summary>
        public byte portNum;

        /// <summary>
        ///     The device product ID.
        /// </summary>
        public ushort product;

        public UnicodeString ProductString;

        /// <summary>
        ///     The product's string Id.
        /// </summary>
        public byte productStringID;

        /// <summary>
        ///     The device release number.
        /// </summary>
        public ushort releaseNumber;

        public UnicodeString SerialNumberString;

        /// <summary>
        ///     The serial number string Id.
        /// </summary>
        public byte serialNumberStringID;

        /// <summary>
        ///     The USb class code reported by the device.
        /// </summary>
        public byte usbClass;

        /// <summary>
        ///     The USB protocol version number reported by the device.
        /// </summary>
        public byte usbProtocol;

        /// <summary>
        ///     The USb specification number reported by the device.
        /// </summary>
        public ushort usbSpec;

        /// <summary>
        ///     The USB sub-class code reported by the device.
        /// </summary>
        public byte usbSubclass;

        /// <summary>
        ///     The device Vendor ID.
        /// </summary>
        public ushort vendor;

        /// <summary>
        ///     Initialises a new USB Device Info but does not attempt to get/fill out the information.
        ///     That job is left to the USB manager.
        /// </summary>
        /// <param name="aPortNum">The port number of the physical device.</param>
        /// <param name="aHC">The host controller which the device is connected to.</param>
        public USBDeviceInfo(byte aPortNum, HCI aHC)
        {
            portNum = aPortNum;
            hc = aHC;
        }

        /// <summary>
        ///     Frees the port and destroys the USB device instance.
        /// </summary>
        public void FreePort()
        {
            HCPort port = hc.GetPort(portNum);
            if (port.device != null)
            {
                port.device.Destroy();
                port.device = null;
            }
            port.deviceInfo = null;
            port.connected = false;
            port.speed = USBPortSpeed.UNSET;
        }
    }
}