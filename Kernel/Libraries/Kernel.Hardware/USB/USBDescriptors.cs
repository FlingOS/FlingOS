using System;

namespace Kernel.Hardware.USB
{
    public enum EndpointType
    {
        EP_OUT, EP_IN, EP_BIDIR
    }
    public class Endpoint : FOS_System.Object
    {
        public ushort mps;
        public bool toggle;
        public EndpointType type;
        public byte interval;
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct DeviceDescriptor
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
    public struct ConfigurationDescriptor
    {
        public byte length;            // 9
        public byte descriptorType;    // 2
        public ushort totalLength;
        public byte numInterfaces;
        public byte configurationValue;
        public byte configuration;
        public byte attributes;
        public byte maxPower;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct InterfaceDescriptor
    {
        public byte length;            // 9
        public byte descriptorType;    // 4
        public byte interfaceNumber;
        public byte alternateSetting;
        public byte numEndpoints;
        public byte interfaceClass;
        public byte interfaceSubclass;
        public byte interfaceProtocol;
        public byte Interface;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct EndpointDescriptor
    {
        public byte length;            // 7
        public byte descriptorType;    // 5
        public byte endpointAddress;
        public byte attributes;
        public ushort maxPacketSize;
        public byte interval;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public unsafe struct StringDescriptor
    {
        public byte length;            // ?
        public byte descriptorType;    // 3
        public fixed ushort languageID[10];    // n = 10 test-wise
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public unsafe struct StringDescriptorUnicode
    {
        public byte length;            // 2 + 2 * numUnicodeCharacters
        public byte descriptorType;    // 3
        public fixed byte widechar[60];      // n = 30 test-wise (60, because we use byte as type)
    }
}