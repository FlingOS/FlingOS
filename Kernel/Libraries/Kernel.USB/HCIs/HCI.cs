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

//#define HCI_TRACE

using Kernel.Devices;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Processes.Requests.Devices;
using Kernel.PCI;
using Kernel.USB.Devices;

namespace Kernel.USB.HCIs
{
    /// <summary>
    ///     Represents a generic USB Host Controller Interface.
    /// </summary>
    public abstract unsafe class HCI : Device
    {
        public enum HCIStatus
        {
            Dead = -1,
            Unset = 0,
            Active = 1
        }

        /// <summary>
        ///     Any other ports attached to the host controller.
        /// </summary>
        protected List OtherPorts = new List(4);

        /// <summary>
        ///     The underlying PCI device for the host controller.
        /// </summary>
        protected PCIVirtualNormalDevice pciDevice;

        /// <summary>
        ///     The number of root ports read from the host controller information.
        /// </summary>
        protected byte RootPortCount = 0;

        /// <summary>
        ///     The root ports (that make up the root hub) of the host controller.
        /// </summary>
        protected List RootPorts = new List(4);

        public HCIStatus Status { get; protected set; }

        public PCIVirtualNormalDevice ThePCIDevice
        {
            get { return pciDevice; }
        }

        /// <summary>
        ///     Initializes a new generic host controller interface using the specified PCI device.
        /// </summary>
        /// <param name="aPCIDevice">The PCI device that represents the HCI device.</param>
        public HCI(PCIVirtualNormalDevice aPCIDevice, string name)
            : base(DeviceGroup.USB, DeviceClass.Controller, DeviceSubClass.USB, name, new uint[3], true)
        {
            Status = HCIStatus.Unset;

            pciDevice = aPCIDevice;
            Info[0] = pciDevice.bus;
            Info[1] = pciDevice.slot;
            Info[2] = pciDevice.function;

            for (byte i = 0; i < RootPortCount; i++)
            {
                RootPorts.Add(new HCPort
                {
                    portNum = i
                });
            }
        }

        internal abstract void Start();

        /// <summary>
        ///     Sets up a USB transfer for sending via the EHCI.
        /// </summary>
        /// <param name="usbDevice">The USb device to send the transfer to.</param>
        /// <param name="transfer">The transfer to send.</param>
        /// <param name="type">The type of USB transfer.</param>
        /// <param name="endpoint">The endpoint of the device to send the transfer to.</param>
        /// <param name="maxLength">The maximum packet size to use when transferring.</param>
        public void SetupTransfer(USBDeviceInfo usbDevice, USBTransfer transfer, USBTransferType type, byte endpoint,
            ushort maxLength)
        {
            transfer.device = usbDevice;
            transfer.endpoint = endpoint;
            transfer.type = type;
#if HCI_TRACE
            BasicConsole.WriteLine(((Framework.String)"SetupTransfer: maxLength=") + maxLength + ", endpoint=" + endpoint + ", mps=" + ((Endpoint)usbDevice.Endpoints[endpoint]).MPS);
#endif
            transfer.packetSize = Math.Min(maxLength, ((Endpoint)usbDevice.Endpoints[endpoint]).MPS);
#if HCI_TRACE
            BasicConsole.WriteLine(((Framework.String)"SetupTransfer: packetSize=") + transfer.packetSize);
#endif
            transfer.success = false;
            transfer.transactions = new List(3);

            _SetupTransfer(transfer);
        }

        /// <summary>
        ///     Sets up a SETUP transaction and adds it to the specified transfer.
        /// </summary>
        /// <param name="transfer">The transfer to which the transaction should be added.</param>
        /// <param name="tokenBytes">The number of bytes to send.</param>
        /// <param name="type">The type of the USB Request.</param>
        /// <param name="req">The specific USB Request.</param>
        /// <param name="hiVal">The USB Request Hi-Val.</param>
        /// <param name="loVal">The USB Request Lo-Val.</param>
        /// <param name="index">The USB request index.</param>
        /// <param name="length">The length of the USB request.</param>
        public void SETUPTransaction(USBTransfer transfer, ushort tokenBytes, byte type, byte req, byte hiVal,
            byte loVal,
            ushort index, ushort length)
        {
            USBTransaction transaction = new USBTransaction();
            transaction.type = USBTransactionType.SETUP;

            _SETUPTransaction(transfer, transaction, false, tokenBytes, type, req, hiVal, loVal, index, length);

            transfer.transactions.Add(transaction);

            ((Endpoint)transfer.device.Endpoints[transfer.endpoint]).Toggle = true;
        }

        /// <summary>
        ///     Sets up an IN transaction and adds it to the specified transfer.
        /// </summary>
        /// <param name="transfer">The transfer to which the transaction should be added.</param>
        /// <param name="controlHandshake">Whether the transaction is part of a control handshake or not.</param>
        /// <param name="buffer">The buffer to store the incoming data in.</param>
        /// <param name="length">The length of the buffer.</param>
        public void INTransaction(USBTransfer transfer, bool controlHandshake, void* buffer, ushort length)
        {
#if HCI_TRACE || USB_TRACE
            BasicConsole.WriteLine(((Framework.String)"transfer.packetSize=") + transfer.packetSize +
                                                       ", length=" + Length);
#endif
            ushort clampedLength = Math.Min(transfer.packetSize, length);
            length -= clampedLength;
#if HCI_TRACE || USB_TRACE
            BasicConsole.WriteLine(((Framework.String)"clampedLength=") + clampedLength);
            BasicConsole.DelayOutput(1);
#endif
            ushort remainingTransactions = (ushort)(length/transfer.packetSize);
#if HCI_TRACE || USB_TRACE
            BasicConsole.WriteLine("Division passed.");
            BasicConsole.DelayOutput(1);
#endif
            if (length%transfer.packetSize != 0)
            {
                remainingTransactions++;
            }

            USBTransaction transaction = new USBTransaction();
            transaction.type = USBTransactionType.IN;

            if (controlHandshake) // Handshake transaction of control transfers always have toggle set to 1
            {
                ((Endpoint)transfer.device.Endpoints[transfer.endpoint]).Toggle = true;
            }

#if HCI_TRACE
            BasicConsole.WriteLine("Call _INTransaction...");
            BasicConsole.DelayOutput(1);
#endif

            _INTransaction(transfer, transaction, ((Endpoint)transfer.device.Endpoints[transfer.endpoint]).Toggle,
                buffer, clampedLength);

#if HCI_TRACE
            BasicConsole.WriteLine("Done.");
            BasicConsole.DelayOutput(1);
#endif

            transfer.transactions.Add(transaction);

            ((Endpoint)transfer.device.Endpoints[transfer.endpoint]).Toggle =
                !((Endpoint)transfer.device.Endpoints[transfer.endpoint]).Toggle; // Switch toggle

            if (remainingTransactions > 0)
            {
                INTransaction(transfer, controlHandshake, (byte*)buffer + clampedLength, length);
            }
        }

        /// <summary>
        ///     Sets up an OUT transaction and adds it to the specified transfer.
        /// </summary>
        /// <param name="transfer">The transfer to which the transaction should be added.</param>
        /// <param name="controlHandshake">Whether the transaction is part of a control handshake or not.</param>
        /// <param name="buffer">The buffer of outgoing data.</param>
        /// <param name="length">The length of the buffer.</param>
        public void OUTTransaction(USBTransfer transfer, bool controlHandshake, void* buffer, ushort length)
        {
            ushort clampedLength = Math.Min(transfer.packetSize, length);
            length -= clampedLength;
            ushort remainingTransactions = (ushort)(length/transfer.packetSize);
            if (length%transfer.packetSize != 0)
                remainingTransactions++;

            USBTransaction transaction = new USBTransaction();
            transaction.type = USBTransactionType.OUT;

            if (controlHandshake) // Handshake transaction of control transfers always have toggle set to 1
            {
                ((Endpoint)transfer.device.Endpoints[transfer.endpoint]).Toggle = true;
            }

            _OUTTransaction(transfer, transaction, ((Endpoint)transfer.device.Endpoints[transfer.endpoint]).Toggle,
                buffer, clampedLength);

            transfer.transactions.Add(transaction);

            ((Endpoint)transfer.device.Endpoints[transfer.endpoint]).Toggle =
                !((Endpoint)transfer.device.Endpoints[transfer.endpoint]).Toggle; // Switch toggle

            if (remainingTransactions > 0)
            {
                OUTTransaction(transfer, controlHandshake, (byte*)buffer + clampedLength, length);
            }
        }

        /// <summary>
        ///     Issues the specified transfer to the physical device.
        /// </summary>
        /// <param name="transfer">The transfer to issue.</param>
        public void IssueTransfer(USBTransfer transfer)
        {
            _IssueTransfer(transfer);
        }

        /// <summary>
        ///     When overridden in a derived class, handles HC implementation specific transfer initialisation.
        /// </summary>
        /// <param name="transfer">The transfer to set up.</param>
        protected abstract void _SetupTransfer(USBTransfer transfer);

        /// <summary>
        ///     When overridden in a derived class, handles HC implementation specific SETUP transaction initialisation.
        /// </summary>
        /// <param name="transfer">The transfer to which the transaction should be added.</param>
        /// <param name="uTransaction">The USB Transaction to convert to an EHCI Transaction.</param>
        /// <param name="toggle">The transaction toggle state.</param>
        /// <param name="tokenBytes">The number of bytes to send.</param>
        /// <param name="type">The type of the USB Request.</param>
        /// <param name="req">The specific USB Request.</param>
        /// <param name="hiVal">The USB Request Hi-Val.</param>
        /// <param name="loVal">The USB Request Lo-Val.</param>
        /// <param name="index">The USB request index.</param>
        /// <param name="length">The length of the USB request.</param>
        protected abstract void _SETUPTransaction(USBTransfer transfer, USBTransaction uTransaction, bool toggle,
            ushort tokenBytes,
            byte type, byte req, byte hiVal, byte loVal, ushort index, ushort length);

        /// <summary>
        ///     When overridden in a derived class, handles HC implementation specific IN transaction initialisation.
        /// </summary>
        /// <param name="transfer">The transfer to which the transaction should be added.</param>
        /// <param name="uTransaction">The USB Transaction to convert to an EHCI transaction.</param>
        /// <param name="toggle">The transaction toggle state.</param>
        /// <param name="buffer">The buffer to store the incoming data in.</param>
        /// <param name="length">The length of the buffer.</param>
        protected abstract void _INTransaction(USBTransfer transfer, USBTransaction uTransaction, bool toggle,
            void* buffer, ushort length);

        /// <summary>
        ///     When overridden in a derived class, handles HC implementation specific OUT transaction initialisation.
        /// </summary>
        /// <param name="transfer">The transfer to which the transaction should be added.</param>
        /// <param name="uTransaction">The USB Transaction to convert to an EHCI transaction.</param>
        /// <param name="toggle">The transaction toggle state.</param>
        /// <param name="buffer">The buffer of outgoing data.</param>
        /// <param name="length">The length of the buffer.</param>
        protected abstract void _OUTTransaction(USBTransfer transfer, USBTransaction uTransaction, bool toggle,
            void* buffer, ushort length);

        /// <summary>
        ///     When overridden in a derived class, handles HC implementation specific method of issuing a transfer.
        /// </summary>
        /// <param name="transfer">The transfer to issue.</param>
        protected abstract void _IssueTransfer(USBTransfer transfer);

        internal abstract void IRQHandler();

        public abstract void ResetPort(byte port);

        /// <summary>
        ///     Updates the HC such as checking for port/device changes.
        /// </summary>
        public abstract void Update();

        /// <summary>
        ///     Sets up a USb device connected to the specified port.
        /// </summary>
        /// <param name="portNum">The port to which the device to set up is connected.</param>
        protected virtual void SetupUSBDevice(byte portNum)
        {
            HCPort port = GetPort(portNum);
            port.deviceInfo = USBManager.CreateDeviceInfo(this, port);
            USBManager.SetupDevice(port.deviceInfo, (byte)(portNum + 1));
        }

        /// <summary>
        ///     Gets (or creates) the HCPort instance for the specified port number.
        /// </summary>
        /// <param name="num">The port number of the HCPort instance to get.</param>
        /// <returns>The existing or new HCPort instance.</returns>
        public HCPort GetPort(byte num)
        {
            if (num < RootPortCount)
                return (HCPort)RootPorts[num];

            num -= RootPortCount;
            if (num < OtherPorts.Count)
            {
                return (HCPort)OtherPorts[num];
            }
            for (int i = OtherPorts.Count; i <= num; i++)
            {
                OtherPorts.Add(new HCPort
                {
                    connected = false,
                    deviceInfo = null,
                    portNum = (byte)(i + RootPortCount),
                    speed = USBPortSpeed.UNSET
                });
            }

            return (HCPort)OtherPorts[num];
        }
    }

    /// <summary>
    ///     The allowable USB port speeds.
    /// </summary>
    public enum USBPortSpeed
    {
        /// <summary>
        ///     Specifies no valid speed.
        /// </summary>
        UNSET = -1,
        //DO NOT CHANGE THESE VALUES!
        /// <summary>
        ///     Indiciates a low-speed port or device (USB 1.0/1.1).
        /// </summary>
        Low = 1,

        /// <summary>
        ///     Indiciates a full-speed port or device (USB 1.0/1.1).
        /// </summary>
        Full = 0,

        /// <summary>
        ///     Indiciates a high-speed port or device (USB 2.0).
        /// </summary>
        High = 2,

        /// <summary>
        ///     Indiciates a super-speed port or device (USB 3.0).
        /// </summary>
        SuperSpeed = 4
    }

    /// <summary>
    ///     Represents a port on a host controller.
    /// </summary>
    public class HCPort : Object
    {
        /// <summary>
        ///     Whether a device is attached to the port or not.
        /// </summary>
        public bool connected;

        /// <summary>
        ///     The USB device attached to the port, if any.
        /// </summary>
        public USBDevice device = null;

        /// <summary>
        ///     The device information about the device attached to the port, if any.
        /// </summary>
        public USBDeviceInfo deviceInfo;

        /// <summary>
        ///     The port number (index).
        /// </summary>
        public byte portNum;

        /// <summary>
        ///     The speed of the port. Default: UNSET.
        /// </summary>
        public USBPortSpeed speed = USBPortSpeed.UNSET;

        public void Reset()
        {
            deviceInfo.hc.ResetPort(portNum);
        }
    }

    /// <summary>
    ///     Represents a host-controller-level transaction.
    /// </summary>
    public abstract class HCTransaction : Object
    {
    }
}