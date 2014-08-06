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
    
#define HCI_TRACE
#undef HCI_TRACE

using System;
using Kernel.FOS_System.Collections;

namespace Kernel.Hardware.USB.HCIs
{
    /// <summary>
    /// Represents a generic USB Host Controller Interface.
    /// </summary>
    public unsafe abstract class HCI : Device
    {
        protected List RootPorts = new List(4);
        protected List OtherPorts = new List(4);
        protected byte RootPortCount = 0;

        /// <summary>
        /// The underlying PCI device for the host controller.
        /// </summary>
        protected PCI.PCIDeviceNormal pciDevice;

        /// <summary>
        /// Initializes a new generic host controller interface using the specified PCI device.
        /// </summary>
        /// <param name="aPCIDevice">The PCI device that represents the HCI device.</param>
        public HCI(PCI.PCIDeviceNormal aPCIDevice)
            : base()
        {
            pciDevice = aPCIDevice;

            for (byte i = 0; i < RootPortCount; i++)
            {
                RootPorts.Add(new HCPort()
                {
                    portNum = i
                });
            }
        }

        public void SetupTransfer(Devices.USBDeviceInfo usbDevice, USBTransfer transfer, USBTransferType type, byte endpoint, 
                                  ushort maxLength)
        {
            transfer.device = usbDevice;
            transfer.endpoint = endpoint;
            transfer.type = type;
            transfer.packetSize = FOS_System.Math.Min(maxLength, ((Endpoint)usbDevice.Endpoints[endpoint]).mps);
            transfer.success = false;
            transfer.transactions = new List(3);

            _SetupTransfer(transfer);
        }
        public void SETUPTransaction(USBTransfer transfer, ushort tokenBytes, byte type, byte req, byte hiVal, byte loVal, 
                                     ushort index, ushort length)
        {
            USBTransaction transaction = new USBTransaction();
            transaction.type = USBTransactionType.SETUP;

            _SETUPTransaction(transfer, transaction, false, tokenBytes, type, req, hiVal, loVal, index, length);

            transfer.transactions.Add(transaction);

            ((Endpoint)transfer.device.Endpoints[transfer.endpoint]).toggle = true;
        }
        public void INTransaction(USBTransfer transfer, bool controlHandshake, void* buffer, ushort length)
        {
            ushort clampedLength = FOS_System.Math.Min(transfer.packetSize, length);
            length -= clampedLength;
#if HCI_TRACE || USB_TRACE
            BasicConsole.WriteLine(((FOS_System.String)"transfer.packetSize=") + transfer.packetSize + 
                                                       ", length=" + length);
            BasicConsole.DelayOutput(1);
#endif
            ushort remainingTransactions = (ushort)(length / transfer.packetSize);
#if HCI_TRACE || USB_TRACE
            BasicConsole.WriteLine("Division passed.");
            BasicConsole.DelayOutput(1);
#endif
            if (length % transfer.packetSize != 0)
            {
                remainingTransactions++;
            }

            USBTransaction transaction = new USBTransaction();
            transaction.type = USBTransactionType.IN;

            if (controlHandshake) // Handshake transaction of control transfers have always set toggle to 1
            {
                ((Endpoint)transfer.device.Endpoints[transfer.endpoint]).toggle = true;
            }

#if HCI_TRACE
            BasicConsole.WriteLine("Call _INTransaction...");
            BasicConsole.DelayOutput(1);
#endif

            _INTransaction(transfer, transaction, ((Endpoint)transfer.device.Endpoints[transfer.endpoint]).toggle, buffer, clampedLength);

#if HCI_TRACE
            BasicConsole.WriteLine("Done.");
            BasicConsole.DelayOutput(1);
#endif

            transfer.transactions.Add(transaction);

            ((Endpoint)transfer.device.Endpoints[transfer.endpoint]).toggle = !((Endpoint)transfer.device.Endpoints[transfer.endpoint]).toggle; // Switch toggle

            if (remainingTransactions > 0)
            {
                INTransaction(transfer, ((Endpoint)transfer.device.Endpoints[transfer.endpoint]).toggle, ((byte*)buffer + clampedLength), length);
            }
        }
        public void OUTTransaction(USBTransfer transfer, bool controlHandshake, void* buffer, ushort length)
        {
            ushort clampedLength = FOS_System.Math.Min(transfer.packetSize, length);
            length -= clampedLength;
            ushort remainingTransactions = (ushort)(length / transfer.packetSize);
            if (length % transfer.packetSize != 0)
                remainingTransactions++;

            USBTransaction transaction = new USBTransaction();
            transaction.type = USBTransactionType.OUT;

            if (controlHandshake) // Handshake transaction of control transfers have always set toggle to 1
            {
                ((Endpoint)transfer.device.Endpoints[transfer.endpoint]).toggle = true;
            }

            _OUTTransaction(transfer, transaction, ((Endpoint)transfer.device.Endpoints[transfer.endpoint]).toggle, buffer, clampedLength);

            transfer.transactions.Add(transaction);

            ((Endpoint)transfer.device.Endpoints[transfer.endpoint]).toggle = !((Endpoint)transfer.device.Endpoints[transfer.endpoint]).toggle; // Switch toggle

            if (remainingTransactions > 0)
            {
                OUTTransaction(transfer, ((Endpoint)transfer.device.Endpoints[transfer.endpoint]).toggle, ((byte*)buffer + clampedLength), length);
            }
        }
        public void IssueTransfer(USBTransfer transfer)
        {
            _IssueTransfer(transfer);
        }

        protected abstract void _SetupTransfer(USBTransfer transfer);
        protected abstract void _SETUPTransaction(USBTransfer transfer, USBTransaction uTransaction, bool toggle, ushort tokenBytes,
                                           byte type, byte req, byte hiVal, byte loVal, ushort index, ushort length);
        protected abstract void _INTransaction(USBTransfer transfer, USBTransaction uTransaction, bool toggle, void* buffer, ushort length);
        protected abstract void _OUTTransaction(USBTransfer transfer, USBTransaction uTransaction, bool toggle, void* buffer, ushort length);
        protected abstract void _IssueTransfer(USBTransfer transfer);

        public abstract void Update();

        protected virtual void SetupUSBDevice(byte portNum)
        {
            HCPort port = GetPort(portNum);
            port.deviceInfo = USBManager.CreateDeviceInfo(this, port);
            USBManager.SetupDevice(port.deviceInfo, (byte)(portNum + 1));
        }
        public HCPort GetPort(byte num)
        {
            if (num < RootPortCount)
                return (HCPort)RootPorts[num];

            num -= RootPortCount;
            if (num < OtherPorts.Count)
            {
                return (HCPort)OtherPorts[num];
            }
            else
            {
                for (int i = OtherPorts.Count; i <= num; i++)
                {
                    OtherPorts.Add(new HCPort()
                    {
                        connected = false,
                        deviceInfo = null,
                        portNum = (byte)(i + RootPortCount),
                        speed = USBPortSpeed.UNSET
                    });
                }
            }

            return (HCPort)OtherPorts[num];
        }
    }

    public enum USBPortSpeed
    {
        UNSET = -1,
        //DO NOT CHANGE THESE VALUES!
        Low = 1,
        Full = 0,
        High = 2,
        SuperSpeed = 4
    }
    public class HCPort : FOS_System.Object
    {
        public Devices.USBDevice device = null;
        public Devices.USBDeviceInfo deviceInfo = null;
        public bool connected = false;
        public byte portNum = 0;
        public USBPortSpeed speed = USBPortSpeed.UNSET;
    }

    public abstract class HCTransaction : FOS_System.Object
    {
    }
}
