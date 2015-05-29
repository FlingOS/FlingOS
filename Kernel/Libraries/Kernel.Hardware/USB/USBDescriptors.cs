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

namespace Kernel.Hardware.USB
{
    /// <summary>
    /// The types of USB endpoint.
    /// </summary>
    public enum EndpointType
    {
        /// <summary>
        /// An endpoint that sends data.
        /// </summary>
        OUT,
        /// <summary>
        /// An endpoint that receives data.
        /// </summary>
        IN, 
        /// <summary>
        /// A bidirectional endpoint that sends or receives data.
        /// </summary>
        BIDIR
    }
    /// <summary>
    /// Represents a device endpoint.
    /// </summary>
    public class Endpoint : FOS_System.Object
    {
        /// <summary>
        /// The maximum packet size to use when transferring data to-from 
        /// the endpoint.
        /// </summary>
        public ushort mps;
        /// <summary>
        /// The toggle state of the last transaction sent to the endpoint.
        /// </summary>
        public bool toggle;
        /// <summary>
        /// The endpoint type.
        /// </summary>
        public EndpointType type;
    }
    /// <summary>
    /// The USB Device Descriptor structure received from a device.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct DeviceDescriptor
    {
        /// <summary>
        /// The length of the descriptor. Should be 18.
        /// </summary>
        public byte length;
        /// <summary>
        /// The descriptor type. Should be 1.
        /// </summary>
        public byte descriptorType;
        /// <summary>
        /// Maximum supported USB version where 0x0210 means 2.10
        /// </summary>
        public ushort bcdUSB;
        /// <summary>
        /// USB device class of the device.
        /// </summary>
        /// <see cref="!:http://www.usb.org/developers/defined_class" />
        public byte deviceClass;
        /// <summary>
        /// USB device sub-class of the device.
        /// </summary>
        /// <see cref="!:http://www.usb.org/developers/defined_class" />
        public byte deviceSubclass;
        /// <summary>
        /// USB device protocol of the device.
        /// </summary>
        /// <see cref="!:http://www.usb.org/developers/defined_class" />
        public byte deviceProtocol;
        /// <summary>
        /// The maximum packet size to use when transferring data to/from
        /// the device. This value must be 8, 16, 32 or 64.
        /// </summary>
        public byte MaxPacketSize;
        /// <summary>
        /// The device's vendor Id.
        /// </summary>
        public ushort VendorId;
        /// <summary>
        /// The device's product Id.
        /// </summary>
        public ushort ProductId;
        /// <summary>
        /// The release version of the device where 0x3102 means 31.02
        /// </summary>
        public ushort bcdDevice;
        /// <summary>
        /// Index of the manufacturer string descriptor.
        /// </summary>
        public byte manufacturer;
        /// <summary>
        /// Index of the product string descriptor.
        /// </summary>
        public byte product;
        /// <summary>
        /// Index of the serial number string descriptor.
        /// </summary>
        public byte serialNumber;
        /// <summary>
        /// The number of possible configurations.
        /// </summary>
        public byte numConfigurations;
    }
    /// <summary>
    /// The USB Configuration Descriptor structure received from a device.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct ConfigurationDescriptor
    {
        /// <summary>
        /// The length of the descriptor. Should be 9.
        /// </summary>
        public byte length;
        /// <summary>
        /// The descriptor type. Should be 2.
        /// </summary>
        public byte descriptorType;
        /// <summary>
        /// The total length, in bytes, of the data returned.
        /// </summary>
        public ushort totalLength;
        /// <summary>
        /// The number of interfaces for this configuration.
        /// </summary>
        public byte numInterfaces;
        /// <summary>
        /// The value to use as an argument to select this configuration.
        /// </summary>
        public byte configurationValue;
        /// <summary>
        /// Index of the string descriptor describing this configuration.
        /// </summary>
        public byte configuration;
        /// <summary>
        /// Bit 7: Reserved, set to 1. (USB 1.0 Bus Powered).
        /// Bit 6: Self Powered.
        /// Bit 5: Remote Wakeup.
        /// Bits 4..0: Reserved, set to 0.
        /// </summary>
        public byte attributes;
        /// <summary>
        /// Maximum power consumption as a multiple of 2mA units.
        /// </summary>
        public byte maxPower;
    }
    /// <summary>
    /// The USB Interface Descriptor structure received from a device.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct InterfaceDescriptor
    {
        /// <summary>
        /// The length of the descriptor. Should be 9.
        /// </summary>
        public byte length;
        /// <summary>
        /// The descriptor type. Should be 4.
        /// </summary>
        public byte descriptorType;
        /// <summary>
        /// The index of this interface.
        /// </summary>
        public byte interfaceNumber;
        /// <summary>
        /// A value used to select the alternative setting.
        /// </summary>
        public byte alternateSetting;
        /// <summary>
        /// The number of endpoints the interface has.
        /// </summary>
        public byte numEndpoints;
        /// <summary>
        /// The interface class code.
        /// </summary>
        /// <see cref="!:http://www.usb.org/developers/defined_class" />
        public byte interfaceClass;
        /// <summary>
        /// The interface sub-class code.
        /// </summary>
        /// <see cref="!:http://www.usb.org/developers/defined_class" />
        public byte interfaceSubclass;
        /// <summary>
        /// The interface protocol code.
        /// </summary>
        /// <see cref="!:http://www.usb.org/developers/defined_class" />
        public byte interfaceProtocol;
        /// <summary>
        /// Index of the string descriptor describing this interface.
        /// </summary>
        public byte Interface;
    }
    /// <summary>
    /// The USB Endpoint Descriptor structure received from a device.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct EndpointDescriptor
    {
        /// <summary>
        /// The length of the descriptor. Should be 7.
        /// </summary>
        public byte length;
        /// <summary>
        /// The descriptor type. Should be 5.
        /// </summary>
        public byte descriptorType;
        /// <summary>
        /// The endpoint's address.
        /// Bit 7: Direction (0 = Out, 1 = In, ignored for Control endpoints). 
        /// Bits 4..6: Reserved. Set to Zero. 
        /// Bits 0..3: Endpoint number. 
        /// </summary>
        public byte endpointAddress;
        /// <summary>
        /// Endpoint attributes. See remarks for details.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        ///     <item>
        ///         <term>Bits 0..1 - Transfer Type</term>
        ///         <description>
        ///             <list type="bullet">
        ///                 <item><term>00</term><description>Control</description></item>
        ///                 <item><term>01</term><description>Isochronous</description></item>
        ///                 <item><term>10</term><description>Bulk</description></item>
        ///                 <item><term>11</term><description>Interrupt</description></item>
        ///             </list>
        ///         </description>
        ///     </item>
        ///     
        ///     <item>
        ///         <term>Bits 2..7</term>
        ///         <description>Reserved except if isochronous endpoint (see below).</description>
        ///     </item>
        ///     
        ///     <item>
        ///         <term>(If Isochronous endpoint) - Bits 3..2 - Synchronisation Type</term>
        ///         <description>
        ///             <list type="bullet">
        ///                 <item><term>00</term><description>No Synchonisation</description></item>
        ///                 <item><term>01</term><description>Asynchronous</description></item>
        ///                 <item><term>10</term><description>Adaptive</description></item>
        ///                 <item><term>11</term><description>Synchronous</description></item>
        ///             </list>
        ///         </description>
        ///     </item>
        ///     
        ///     <item>
        ///         <term>(If Isochronous endpoint) - Bits 5..4 - Usage Type</term>
        ///         <description>
        ///             <list type="bullet">
        ///                 <item><term>00</term><description>Data Endpoint</description></item>
        ///                 <item><term>01</term><description>Feedback Endpoint</description></item>
        ///                 <item><term>10</term><description>Explicit Feedback Data Endpoint</description></item>
        ///                 <item><term>11</term><description>Reserved</description></item>
        ///             </list>
        ///         </description>
        ///     </item>
        /// </list>
        /// </remarks>
        public byte attributes;
        /// <summary>
        /// The maximum packet size to use when transferring data to/from
        /// the endpoint. This value must be 8, 16, 32 or 64.
        /// </summary>
        public ushort maxPacketSize;
        /// <summary>
        /// Interval for polling endpoint data transfers. Value in frame counts. See remarks for more info.
        /// </summary>
        /// <remarks>
        /// Ignored for Bulk &amp; Control Endpoints. Isochronous must equal 1 and field may range from 1 to 255 for interrupt endpoints.
        /// </remarks>
        public byte interval;
    }
    /// <summary>
    /// The USB String Descriptor structure received from a device. Use this or the unicode version as appropriate.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public unsafe struct StringDescriptor
    {
        /// <summary>
        /// The length of the descriptor. Value is variable.
        /// </summary>
        public byte length;
        /// <summary>
        /// The descriptor type. Should be 3.
        /// </summary>
        public byte descriptorType;
        /// <summary>
        /// The language IDs or value bytes of the string. 
        /// This has been determined, through testing, to require a max length 10 bytes. Not sure if this is proper though.
        /// </summary>
        public fixed ushort languageID[10];
    }
    /// <summary>
    /// A USB Unicode String Description structure received from a device. Use this or the unicode version as appropriate.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public unsafe struct StringDescriptorUnicode
    {
        /// <summary>
        /// The length of the descriptor. Should equal: 2 + (2 * numUnicodeCharacters)
        /// </summary>
        public byte length;
        /// <summary>
        /// The descriptor type. Should be 3.
        /// </summary>
        public byte descriptorType;
        /// <summary>
        /// The unicode value bytes of the string. 
        /// This has been determined, through testing, to require a max length 60 bytes (30 characters). Not sure if this is proper though.
        /// </summary>
        public fixed byte widechar[60];
    }
}