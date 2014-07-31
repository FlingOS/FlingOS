#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
using System;
using Kernel.FOS_System.Collections;

namespace Kernel.Hardware.USB.Devices
{
    public class USBDevice : Device
    {
        protected USBDeviceInfo DeviceInfo;

        public USBDevice(USBDeviceInfo aDeviceInfo)
        {
            DeviceInfo = aDeviceInfo;

            DeviceManager.Devices.Add(this);
            USBManager.Devices.Add(this);
        }

        public virtual void Destroy()
        {
            DeviceManager.Devices.Remove(this);
            USBManager.Devices.Remove(this);
        }
    }
    public class USBDeviceInfo : FOS_System.Object
    {
        public byte portNum;
        public byte num;
        public HCIs.HCI hc;
        public List Endpoints;

        public ushort usbSpec;
        public byte usbClass;
        public byte usbSubclass;
        public byte usbProtocol;
        public ushort vendor;
        public ushort product;
        public ushort releaseNumber;
        public byte manufacturerStringID;
        public byte productStringID;
        public byte serNumberStringID;
        public byte numConfigurations;
        public byte maxLUN;

        public byte InterfaceClass;
        public byte InterfaceSubclass;

        public FOS_System.String SerialNumber;

        public byte MSD_InterfaceNum;
        public byte MSD_INEndpointID;
        public byte MSD_OUTEndpointID;

        public USBDeviceInfo(byte aPortNum, HCIs.HCI aHC)
        {
            portNum = aPortNum;
            hc = aHC;
        }

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
namespace Kernel.Hardware.USB
{
    public enum USBEndpointType
    {
        EP_OUT, EP_IN, EP_BIDIR
    }
    public class USBEndpoint : FOS_System.Object
    {
        public ushort mps;
        public bool toggle;
        public USBEndpointType type;
        public byte interval;
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack=1)]
    public struct USBDeviceDescriptor
    {
        public byte length;            // 18
        public byte descriptorType;    // 1
        public ushort bcdUSB;            // e.g. 0x0210 means 2.10
        public byte deviceClass;
        public byte deviceSubclass;
        public byte deviceProtocol;
        public byte maxPacketSize;     // MPS0, must be 8,16,32,64
        public ushort idVendor;
        public ushort idProduct;
        public ushort bcdDevice;         // release of the device
        public byte manufacturer;
        public byte product;
        public byte serialNumber;
        public byte numConfigurations; // number of possible configurations
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct usb_configurationDescriptor
    {
        public byte  length;            // 9
        public byte  descriptorType;    // 2
        public ushort totalLength;
        public byte  numInterfaces;
        public byte  configurationValue;
        public byte  configuration;
        public byte  attributes;
        public byte  maxPower;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct usb_interfaceDescriptor
    {
        public byte  length;            // 9
        public byte  descriptorType;    // 4
        public byte  interfaceNumber;
        public byte  alternateSetting;
        public byte  numEndpoints;
        public byte  interfaceClass;
        public byte  interfaceSubclass;
        public byte  interfaceProtocol;
        public byte  Interface;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct usb_endpointDescriptor
    {
        public byte  length;            // 7
        public byte  descriptorType;    // 5
        public byte  endpointAddress;
        public byte  attributes;
        public ushort maxPacketSize;
        public byte  interval;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public unsafe struct usb_stringDescriptor
    {
        public byte  length;            // ?
        public byte  descriptorType;    // 3
        public fixed ushort languageID[10];    // n = 10 test-wise
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public unsafe struct usb_stringDescriptorUnicode
    {
        public byte  length;            // 2 + 2 * numUnicodeCharacters
        public byte  descriptorType;    // 3
        public fixed byte  widechar[60];      // n = 30 test-wise (60, because we use byte as type)
    }
}
