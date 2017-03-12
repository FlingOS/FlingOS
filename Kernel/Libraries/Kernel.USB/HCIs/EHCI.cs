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

//#define EHCI_TRACE

#if EHCI_TRACE
    //#define EHCI_TESTS //Note: Also uncomment the undef in EHCITesting.cs
#endif

using System.Runtime.InteropServices;
using Drivers.Compiler.Attributes;
using Kernel.Framework;
using Kernel.Framework.Processes;
using Kernel.PCI;
using Kernel.Utilities;
using Utils = Kernel.Utilities.ConstantsUtils;

namespace Kernel.USB.HCIs
{

    #region Constants

    /// <summary>
    ///     Interrupt threshold control values - determines the minimum time between interrupt being fired.
    /// </summary>
    public enum EHCI_InterruptThresholdControls : uint
    {
        /// <summary>
        ///     1 Microframes (= 0.125ms)
        /// </summary>
        x01 = 0x010000,

        /// <summary>
        ///     2 Microframes (= 0.25ms)
        /// </summary>
        x02 = 0x020000,

        /// <summary>
        ///     4 Microframes (= 0.5ms)
        /// </summary>
        x04 = 0x040000,

        /// <summary>
        ///     8 Microframes (= 1ms)
        /// </summary>
        x08 = 0x080000,

        /// <summary>
        ///     16 Microframes (= 2ms)
        /// </summary>
        x16 = 0x100000,

        /// <summary>
        ///     32 Microframes (= 4ms)
        /// </summary>
        x32 = 0x200000,

        /// <summary>
        ///     64 Microframes (= 8ms)
        /// </summary>
        x64 = 0x400000
    }

    /// <summary>
    ///     Frame list size values.
    /// </summary>
    public enum EHCI_FrameListSizes : uint
    {
        /// <summary>
        ///     List size of 1024.
        /// </summary>
        x1024 = 0x0,

        /// <summary>
        ///     List size of 512.
        /// </summary>
        x0512 = 0x4,

        /// <summary>
        ///     List size of 256.
        /// </summary>
        x0256 = 0x8
    }

    /// <summary>
    ///     Constants used by the EHCI driver.
    /// </summary>
    public class EHCI_Consts
    {
        /* ------ USBCMD ------ */

        /// <summary>
        ///     Mask for the Interrupt Threshold setting in the CMD operational register.
        /// </summary>
        /// <see cref="EHCI_InterruptThresholdControls" />
        public static uint CMD_InterruptThresholdMask = 0x00FF0000;

        /// <summary>
        ///     Mask for the Frame List Size setting in the CMD operational register.
        /// </summary>
        /// <see cref="EHCI_FrameListSizes" />
        public static uint CMD_FrameListSizeMask = 0xC;

        /// <summary>
        ///     Mask for the Park Mode setting in the CMD operational register.
        /// </summary>
        public static uint CMD_ParkModeMask = 0x800;

        /// <summary>
        ///     Mask for the Park Count setting in the CMD operational register.
        /// </summary>
        public static uint CMD_ParkCountMask = 0x300;

        /// <summary>
        ///     Mask for the Light Reset command in the CMD operational register.
        /// </summary>
        public static uint CMD_LightResetMask = Utils.BIT(7);

        /// <summary>
        ///     Mask for the Async Interrupt Doorbell setting in the CMD operational register.
        /// </summary>
        public static uint CMD_AsyncInterruptDoorbellMask = Utils.BIT(6);

        /// <summary>
        ///     Mask for the Async Schedule Enable command in the CMD operational register.
        /// </summary>
        public static uint CMD_AsyncScheduleEnableMask = Utils.BIT(5);

        /// <summary>
        ///     Mask for the Periodic Schedule Enable command in the CMD operational register.
        /// </summary>
        public static uint CMD_PeriodicScheduleEnableMask = Utils.BIT(4);

        /// <summary>
        ///     Mask for the Host Controller Reset command in the CMD operational register.
        /// </summary>
        public static uint CMD_HCResetMask = Utils.BIT(1); // reset

        /// <summary>
        ///     Mask for the Run-Stop command bit in the CMD operational register.
        /// </summary>
        public static uint CMD_RunStopMask = Utils.BIT(0); // run/stop


        /* ------ USBSTS / USBINTR ------ */

        /* Only USBSTS */

        /// <summary>
        ///     A mask for all the possible interrupt bits in the USBSTS register.
        /// </summary>
        public static uint STS_AllInterrupts = 0xF03F;

        /// <summary>
        ///     Mask for the Async Schedule Enabled flag.
        /// </summary>
        public static uint STS_AsyncEnabled = Utils.BIT(15);

        /// <summary>
        ///     Mask for the Periodic Schedule Enabled flag.
        /// </summary>
        public static uint STS_PeriodicEnabled = Utils.BIT(14);

        /// <summary>
        ///     Mask for the Reclamation flag.
        /// </summary>
        public static uint STS_ReclamationFlag = Utils.BIT(13);

        /// <summary>
        ///     Mask for the Host Controller Halted flag.
        /// </summary>
        public static uint STS_HCHalted = Utils.BIT(12);

        /* USBSTS / USBINTR */

        /// <summary>
        ///     Mask for the interrupt type flag indicating Async Interrupt occurred.
        /// </summary>
        public static uint STS_AsyncInterrupt = Utils.BIT(5);

        /// <summary>
        ///     Mask for the interrupt type flag indicating a Host System Error occurred.
        /// </summary>
        public static uint STS_HostSystemError = Utils.BIT(4);

        /// <summary>
        ///     Mask for the interrupt type flag indicating a Frame List Rollover occurred.
        /// </summary>
        public static uint STS_FrameListRollover = Utils.BIT(3);

        /// <summary>
        ///     Mask for the interrupt type flag indicating a Port Change occurred.
        /// </summary>
        public static uint STS_PortChange = Utils.BIT(2);

        /// <summary>
        ///     Mask for the interrupt type flag indicating a USB Error occurred.
        /// </summary>
        public static uint STS_USBErrorInterrupt = Utils.BIT(1);

        /// <summary>
        ///     Mask for the interrupt type flag indicating a general USB Interrupt occurred.
        /// </summary>
        public static uint STS_USBInterrupt = Utils.BIT(0);

        /* ------ FRINDEX ------ */

        /// <summary>
        ///     Frame index register mask.
        /// </summary>
        public static uint FRI_Mask = 0x00001FFF;


        /* ------ PERIODICLISTBASE ------ */

        /// <summary>
        ///     Periodic list base alignment mask. 4KiB alignment.
        /// </summary>
        public static uint PLB_Alignment = 0x00000FFF; // 4 KiB


        /* ------ ASYNCLISTADDR ------ */

        /// <summary>
        ///     Async list address alignment mask. 32 byte alignment.
        /// </summary>
        public static uint ALB_Alignment = 0x0000001F; // 32 Byte


        /* ------ CONFIGFLAG ------ */

        /// <summary>
        ///     Config flag mask.
        /// </summary>
        public static uint CF = Utils.BIT(0);


        /* ------ PORTSC[0-n] ------ */

        /// <summary>
        ///     R/W. Port status mask to get/set whether the port is owned by the companion host controller.
        /// </summary>
        public static uint PSTS_CompanionHCOwned = Utils.BIT(13);

        /// <summary>
        ///     R/W. Port status mask to power the port on/off. Valid if PPC == 1.
        /// </summary>
        public static uint PSTS_PowerOn = Utils.BIT(12);

        /// <summary>
        ///     R/W. Port status mask to tell the port to reset.
        /// </summary>
        public static uint PSTS_PortReset = Utils.BIT(8);

        /// <summary>
        ///     R/W. Port status mask to tell the port to suspend.
        /// </summary>
        public static uint PSTS_PortSuspend = Utils.BIT(7);

        /// <summary>
        ///     R/W. Port status mask to tell the port to suspend.
        /// </summary>
        public static uint PSTS_PortResume = Utils.BIT(6);

        /// <summary>
        ///     R/WC. Port status mask to read or clear the port overrcurrent changed status.
        /// </summary>
        public static uint PSTS_OverCurrentChange = Utils.BIT(5);

        /// <summary>
        ///     R. Port status mask to read whether the port has gone into overcurrent or not.
        /// </summary>
        public static uint PSTS_OverCurrent = Utils.BIT(4);

        /// <summary>
        ///     R/WC. Port status mask to read or clear the port changed status.
        /// </summary>
        public static uint PSTS_EnabledChange = Utils.BIT(3);

        /// <summary>
        ///     R/W. Port status mask to get/set whether the port is enabled or not.
        /// </summary>
        public static uint PSTS_Enabled = Utils.BIT(2);

        /// <summary>
        ///     R/WC. Port status mask to read or clear the port connected changed status.
        /// </summary>
        public static uint PSTS_ConnectedChange = Utils.BIT(1);

        /// <summary>
        ///     R. Port status mask to read whether the port is connected to a device or not.
        /// </summary>
        public static uint PSTS_Connected = Utils.BIT(0);

        /// <summary>
        ///     Mask for the number of ports.
        /// </summary>
        public static uint NumPorts = 0xF; // number of ports (Bits 3:0 set)

        /// <summary>
        ///     Mask for the overall port routing bit.
        /// </summary>
        public static uint PortRoutingMask = Utils.BIT(7); // port routing to EHCI or cHC

        /// <summary>
        ///     Constant to set the number of times that software should re-attempt to send a transfer
        ///     in the async schedule.
        /// </summary>
        public static uint NumAsyncListRetries = 1; //Min val. = 1
    }

    /// <summary>
    ///     The types of queue head (under EHCI): IN, OUT or SETUP.
    /// </summary>
    public enum EHCI_qTDTypes : byte
    {
        /// <summary>
        ///     Indicates an OUT transaction where data is sent from the controller to the device.
        /// </summary>
        OUT = 0,

        /// <summary>
        ///     Indicates an IN transaction where data is sent from the device to the controller.
        /// </summary>
        IN = 1,

        /// <summary>
        ///     Indicates a SETUP transaction.
        /// </summary>
        SETUP = 2
    }

    #endregion

    /// <summary>
    ///     Represents a USB Extended Host Controller Interface
    /// </summary>
    public unsafe class EHCI : HCI
    {
        //TODO: Periodic schedule support
        //TODO: Isochronous transfer support

        /*
         * Based on the Intel EHCI Specification for USB 2.0
         *  http://www.intel.co.uk/content/dam/www/public/us/en/documents/technical-specifications/ehci-specification-for-usb.pdf
         */

        /// <summary>
        ///     The base address of the USB HCI device in memory.
        /// </summary>
        protected byte* usbBaseAddress;

        #region PCI Registers

        /*
         * See section 2.1 of spec.
         */

        /// <summary>
        ///     SBRN PCI memory-mapped register.
        /// </summary>
        protected byte SBRN;

        /// <summary>
        ///     FLADJ PCI memory-mapped register.
        /// </summary>
        protected byte FLADJ
        {
            get { return pciDevice.ReadRegister8(0x61); }
            set { pciDevice.WriteRegister8(0x61, value); }
        }

        /// <summary>
        ///     Port wakeup capability PCI memory-mapped register.
        /// </summary>
        protected byte PortWakeCap
        {
            get { return pciDevice.ReadRegister8(0x62); }
        }

        #endregion

        #region Capability Registers

        /*
         *  See section 2.2 of spec.
         */

        /// <summary>
        ///     The base address of the capabilities regsiters.
        /// </summary>
        protected byte* CapabilitiesRegAddr;

        /// <summary>
        ///     The length of the capabilities registers. Used to calculate
        ///     offset to operational registers.
        /// </summary>
        protected byte CapabilitiesRegsLength;

        /// <summary>
        ///     HCI Version number.
        /// </summary>
        protected ushort HCIVersion;

        /// <summary>
        ///     HCS params
        /// </summary>
        protected uint HCSParams;

        /// <summary>
        ///     HCC params
        /// </summary>
        protected uint HCCParams;

        #region From HCS Params

        /*
         *  See section 2.2.3 of spec.
         */

        #endregion

        #region From HCC Params

        /*
         *  See section 2.2.4 of spec.
         */

        /// <summary>
        ///     EECP from HCC params
        /// </summary>
        protected byte EECP
        {
            get { return (byte)(HCCParams >> 8); }
        }

        #endregion

        #endregion

        #region Operation Registers

        /*
         * See section 2.3 of spec.
         */

        /// <summary>
        ///     Base address of the operational registers.
        /// </summary>
        protected uint* OpRegAddr;

        #region Core Well

        /// <summary>
        ///     USB command operational memory-mapped register.
        /// </summary>
        protected uint USBCMD
        {
            get { return *OpRegAddr; }
            set { *OpRegAddr = value; }
        }

        /// <summary>
        ///     USB status operational memory-mapped register.
        /// </summary>
        protected uint USBSTS
        {
            get { return *(OpRegAddr + 1); }
            set { *(OpRegAddr + 1) = value; }
        }

        /// <summary>
        ///     USB interrupts operational memory-mapped register.
        /// </summary>
        protected uint USBINTR
        {
            get { return *(OpRegAddr + 2); }
            set { *(OpRegAddr + 2) = value; }
        }

        /// <summary>
        ///     USB frame index operational memory-mapped register.
        /// </summary>
        protected uint FRINDEX
        {
            get { return *(OpRegAddr + 3); }
            set { *(OpRegAddr + 3) = value; }
        }

        /// <summary>
        ///     USB control DS segment operational memory-mapped register.
        /// </summary>
        protected uint CTRLDSSEGMENT
        {
            get { return *(OpRegAddr + 4); }
            set { *(OpRegAddr + 4) = value; }
        }

        /// <summary>
        ///     USB periodic list base operational memory-mapped register.
        /// </summary>
        protected uint PERIODICLISTBASE
        {
            get { return *(OpRegAddr + 5); }
            set { *(OpRegAddr + 5) = value; }
        }

        /// <summary>
        ///     USB async list address operational memory-mapped register.
        /// </summary>
        protected EHCI_QueueHead_Struct* ASYNCLISTADDR
        {
            get { return (EHCI_QueueHead_Struct*)*(OpRegAddr + 6); }
            set { *(OpRegAddr + 6) = (uint)value; }
        }

        /// <summary>
        ///     Whether the host controller has been halted or not.
        /// </summary>
        protected bool HCHalted
        {
            get { return (USBSTS & EHCI_Consts.STS_HCHalted) != 0; }
        }

        /// <summary>
        ///     Whether the asynchronous schedule is enabled or not.
        /// </summary>
        protected bool AsynchronousScheduleEnabled
        {
            /*
             * See section 2.3.1 of spec.
             */
            get { return (USBCMD & 0x20) > 0; }
            set
            {
                if (value)
                {
                    USBCMD = USBCMD | 0x20;
                }
                else
                {
                    USBCMD = USBCMD & 0xFFFFFFDF;
                }
            }
        }

        /// <summary>
        ///     Whether the HCI thinks the asynchronous schedule is enabled or not.
        /// </summary>
        protected bool AsynchronousScheduleStatus
        {
            get { return (USBSTS & 0x80) > 0; }
        }

        /// <summary>
        ///     Used as a doorbell by software to tell the host controller to issue an interrupt the next time it advances
        ///     asynchronous schedule. Used when a queue head is removed from the async queue.
        /// </summary>
        protected bool InterruptOnAsyncAdvanceDoorbell
        {
            /*
             * See sections 2.3.1 and 4.8.2 of spec.
             */
            get { return (USBCMD & 0x40) > 0; }
            set
            {
                if (value)
                {
                    USBCMD = USBCMD | 0x40;
                }
                else
                {
                    USBCMD = USBCMD & 0xFFFFFFBF;
                }
            }
        }

        /// <summary>
        ///     Indicates the interrupt has/would have occurred.
        /// </summary>
        protected bool InterruptOnAsyncAdvance
        {
            /*
             * See sections 2.3.1 and 4.8.2 of spec.
             */
            get { return (USBSTS & 0x20) > 0; }
            set
            {
                if (value)
                {
                    USBSTS = USBSTS | 0x20;
                }
                else
                {
                    USBSTS = USBSTS & 0xFFFFFFDF;
                }
            }
        }

        #endregion

        #region Aux Well

        /// <summary>
        ///     USB configuration flags operational memory-mapped register.
        /// </summary>
        protected uint CONFIGFLAG
        {
            get { return *(OpRegAddr + 16); }
            set { *(OpRegAddr + 16) = value; }
        }

        /// <summary>
        ///     USB port SC operational memory-mapped register.
        /// </summary>
        protected uint* PORTSC
        {
            get { return OpRegAddr + 17; }
            set { *(OpRegAddr + 17) = (uint)value; }
        }

        #endregion

        #endregion

        private bool anyPortsChanged;

        /// <summary>
        ///     Whether any ports have changed since the last port check.
        /// </summary>
        protected bool AnyPortsChanged
        {
            get { return anyPortsChanged; }
            set
            {
                anyPortsChanged = value;

                if (value)
                {
                    USBManager.NotifyDevicesNeedUpdate();
                }
            }
        }

        /// <summary>
        ///     Whether the ports have been enabled or not.
        /// </summary>
        protected bool EnabledPorts;

        /// <summary>
        ///     A countdown of the number of async transaction complete interrupts that have occurred since the last
        ///     reload. Used for detecting the end of an async transfer (queue head completetion).
        /// </summary>
        protected int USBIntCount;

        /// <summary>
        ///     Pointer to the idle queue head. Required by the spec and this should always remain as the head of the
        ///     async queue while the async queue is enabled.
        /// </summary>
        protected EHCI_QueueHead_Struct* IdleQueueHead = null;

        /// <summary>
        ///     Pointer to the tail queue head - the queue head at the end of the linked list. Optimisation - this allows
        ///     us to append to the async queue without having to traverse the whole list first. This should be set to
        ///     the idle queue head pointer when the list is "empty".
        /// </summary>
        protected EHCI_QueueHead_Struct* TailQueueHead = null;

        protected int AsyncDoorbellIntCount;

        private readonly int IRQHandlerID = 0;

        private int hostSystemErrors;

        /// <summary>
        ///     Whether the EHCI has hit a host system error or not.
        /// </summary>
        public int HostSystemErrors
        {
            get { return hostSystemErrors; }
            protected set
            {
                hostSystemErrors = value;
                if (value != 0)
                {
                    Status = HCIStatus.Dead;
                    USBManager.NotifyDevicesNeedUpdate();
                }
            }
        }

        /// <summary>
        ///     Whether the EHCI has hit an unrecoverable error or not.
        /// </summary>
        public bool IrrecoverableError
        {
            get { return HostSystemErrors >= 1; }
        }

        /// <summary>
        ///     Initialises a new EHCI device using the specified PCI device. Includes starting the host controller.
        /// </summary>
        /// <param name="aPCIDevice">The PCI device that represents the physical EHCI device.</param>
        public EHCI(PCIVirtualNormalDevice aPCIDevice)
            : base(aPCIDevice, "EHCI USB Controller")
        {
#if EHCI_TESTS
            try
            {
                EHCITesting.Test_PointerManipulation();
                EHCITesting.Test_StructValueSetting();
                EHCITesting.Test_QueueHeadWrapper();
                EHCITesting.Test_qTDWrapper();
            }
            catch
            {
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }
#endif

            // The USB Base Address is the physical address of the memory mapped registers
            //  used to control the host controller. It is a Memory Space BAR.
            // The BAR to use is BAR0. 
            // Section 2.1.3 of the Intel EHCI Spec
            usbBaseAddress = pciDevice.BaseAddresses[0].BaseAddress();

#if EHCI_TRACE
            BasicConsole.WriteLine(((Framework.String)"EHCI: usbBaseAddress=") + (uint)usbBaseAddress);
#endif

            // Map in the required memory
            bool isUSBPageAlreadyMapped = false;
            SystemCallResults checkUSBPageResult =
                SystemCalls.IsPhysicalAddressMapped((uint)usbBaseAddress & 0xFFFFF000, out isUSBPageAlreadyMapped);
            if (checkUSBPageResult != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Error! EHCI cannot check USB Base Address.");
                ExceptionMethods.Throw(new Exception("EHCI cannot check USB Base Address."));
            }

            if (!isUSBPageAlreadyMapped)
            {
                uint actualAddress = 0xFFFFFFFF;
                SystemCallResults mapUSBPageResult = SystemCalls.RequestPhysicalPages(
                    (uint)usbBaseAddress & 0xFFFFF000, 1, out actualAddress);
                if (mapUSBPageResult != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Error! EHCI cannot map USB Base Address.");
                    ExceptionMethods.Throw(new Exception("EHCI cannot map USB Base Address."));
                }
                usbBaseAddress = (byte*)actualAddress;
            }
            else
            {
                uint actualAddress = 0xFFFFFFFF;
                SystemCallResults getUSBPageResult = SystemCalls.GetVirtualAddress((uint)usbBaseAddress & 0xFFFFF000,
                    out actualAddress);
                if (getUSBPageResult != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Error! EHCI cannot get USB Base Address.");
                    ExceptionMethods.Throw(new Exception("EHCI cannot get USB Base Address."));
                }
                usbBaseAddress = (byte*)actualAddress;
            }

            // Caps registers start at the beginning of the memory mapped IO registers.
            // Section 2 of the Intel EHCI Spec
            CapabilitiesRegAddr = usbBaseAddress;
#if EHCI_TRACE
            DBGMSG("CapabilitiesRegAddr: " + (Framework.String)(uint)CapabilitiesRegAddr);
#endif
            // Read the Serial Bus Release Number
            //  This is an 8-bit register where 0xXY means Revision X.Y
            //  e.g. 0x20 means Revision 2.0
            // Section 2.1.4 of the Intel EHCI Spec
            SBRN = pciDevice.ReadRegister8(0x60);
#if EHCI_TRACE
            DBGMSG("SBRN: " + (Framework.String)SBRN);
#endif

            // The first register of the Capabilities Registers tells you the length
            //  of the registers (in bytes). This provides the offset from the start of
            //  the capabilities registers to the start of the Operational registers.
            CapabilitiesRegsLength = *CapabilitiesRegAddr;
            // BCD encoding of the HC Interface version supported by this host controller.
            //  Most Significant Byte is major version
            //  Least Significant Byte is minor version
            //  e.g. 0x20 = Version 2.0
            HCIVersion = *(ushort*)(CapabilitiesRegAddr + 2);
            // Host Controller Structural Params
            //  Section 2.2.3 of the Intel EHCI Spec
            //  This register contains various bit fields providing specific information 
            //  about the HC hardware / firmware physical design (e.g. number of root ports)
            HCSParams = *(uint*)(CapabilitiesRegAddr + 4);
            // Host Controller Capability Params
            //  Section 2.2.4 of the Intel EHCI Spec
            //  This register contains various bit fields providing specific information 
            //  about the HC hardware / firmware capabilities (e.g. 64-bit addressing capaiblity)
            HCCParams = *(uint*)(CapabilitiesRegAddr + 8);

            // Operational registers address. Calculated as stated above from:
            //      USB Base Address + Length of Capabilities registers.
            //  Section 2.3 of the Intel EHCI Spec
            OpRegAddr = (uint*)(usbBaseAddress + CapabilitiesRegsLength);

#if EHCI_TRACE
            DBGMSG("CapabilitiesRegsLength: " + (Framework.String)CapabilitiesRegsLength);
            DBGMSG("HCIVersion: " + (Framework.String)HCIVersion);
            DBGMSG("HCSParams: " + (Framework.String)HCSParams);
            DBGMSG("HCCParams: " + (Framework.String)HCCParams);
            //DBGMSG("HCSPPortRouteDesc: " + (Framework.String)HCSPPortRouteDesc);
            DBGMSG("OpRegAddr: " + (Framework.String)(uint)OpRegAddr);
#endif
            // Number of root ports 
            //  Section 2.2.3 of Intel EHCI Spec
            RootPortCount = (byte)(HCSParams & 0x000F);
        }

        /// <summary>
        ///     Updates the host controller (runs a port check if any ports have changed since the last port check).
        /// </summary>
        public override void Update()
        {
            // Don't attempt any updates or communication with the firmware
            //  if the HC has utterly crashed...
            if (IrrecoverableError)
            {
//#if EHCI_TRACE
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine("EHCI controller has hit an irrecoverable error!");
                BasicConsole.DelayOutput(10);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
//#endif
            }
            else if (HostSystemErrors == 1)
            {
//#if EHCI_TRACE
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine("EHCI Host System error occurred!");
                BasicConsole.DelayOutput(10);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
//#endif

                // Attempt to restart the HC
                Start();
            }
            // Otherwise, if any ports have changed status (e.g. device connected / disconnected):
            else if (AnyPortsChanged)
            {
                //Update all ports
                PortCheck();
            }
        }

        /// <summary>
        ///     Starts the host controller including all necessary initialisation, port resets and port enabling.
        ///     Also detects any devices already connected to the controller.
        /// </summary>
        internal override void Start()
        {
            int hcErrors = HostSystemErrors;

            //Initialise the host controller
            InitHC();
            //Reset the host controller (to a known, stable, startup state)
            ResetHC();
            //Start the host controller
            StartHC();

            Status = HCIStatus.Active;

            //Initialise the Async Schedule of the host controller
            InitializeAsyncSchedule();

            //In future, this could also contain initialisation of the periodic
            //  schedule and perhaps more complex host controller features such as
            //  the more advanced power modes / controls.

            // If at this point the host controller is still in the "halted" state then it has
            //  crashed irrecoverably...
            if (!HCHalted)
            {
                //Otherwise, the HC is ready to be used.

                // So we attempt to enable and initialise all the ports on the HC.
                EnablePorts();

                if (HostSystemErrors == hcErrors)
                {
                    HostSystemErrors = 0;
                }
#if EHCI_TRACE
                DBGMSG("USB ports enabled.");
#endif
            }
            else
            {
                HostSystemErrors++;
                ExceptionMethods.Throw(new Exception("EHCI halted even after start requested! Cannot start EHCI driver!"));
            }
        }

        /// <summary>
        ///     Initialises the host controller.
        /// </summary>
        protected void InitHC()
        {
#if EHCI_TRACE
            DBGMSG("InitHC() : Start");
#endif

            // Set the PCI command signal that the HC should:
            //  1) Enable the memory-mapped IO registers (Capabilities and Operational registers)
            //  2) Set the HC as the master HC for all ports.
            pciDevice.Command = pciDevice.Command | PCIDevice.PCICommand.Memory | PCIDevice.PCICommand.Master;

#if EHCI_TRACE
            DBGMSG("Hooking IRQ...");
#endif
            if (IRQHandlerID == 0)
            {
                // Setup the interrupt handler (IRQ number = PCIDevice.InterruptLine)
#if EHCI_TRACE
                DBGMSG(((Framework.String)"EHCI Interrupt line: ") + pciDevice.InterruptLine);
#endif
                SystemCalls.RegisterIRQHandler(pciDevice.InterruptLine);
            }
#if EHCI_TRACE
            DBGMSG("Hooked IRQ.");

            DBGMSG("InitHC() : End");
#endif
        }

        /// <summary>
        ///     Starts the host controller.
        /// </summary>
        protected void StartHC()
        {
#if EHCI_TRACE
            DBGMSG("StartHC() : Start");
#endif

            // Deactive legacy support if it is available
            DeactivateLegacySupport();

            //Check if 64-bit addressing is enabled
            //  HCCParams, bit 0 set indicates enabled
            //  Section 2.2.4 of Intel EHCI Spec
            if ((HCCParams & 0x1) == 0x1)
            {
                // Set the Control Data Structure Segment Register
                //  Section 2.3.5 of Intel EHCI Spec
                // If 64-bit addressing is enabled, we currently want to make sure
                //  all addresses are treated as normal 32-bit addresses so we must 
                //  zero-out this register.
                CTRLDSSEGMENT = 0u;
            }
            // Clear the USBSTS register
            //  This is a latch register. To clear all the bits, you write 1s to all of them.
            //      Some bits are reserved and must be set to 0 when writing to the register.
            //  Section 2.3.2 of Intel EHCI Spec
            USBSTS = EHCI_Consts.STS_AllInterrupts;
            // Enable interrupts.
            EnableInterrupts();

            //If the HC is halted, start it!
            // Also:
            //          "Software must not write a one to [the Run/Stop] field unless the host 
            //           controller is in the Halted state (i.e. HCHalted in the USBSTS register 
            //           is a one). Doing so will yield undefined results."
            //  (Intel EHCI Spec, Section 2.3.1, Table 2-9, Run/Stop
            if (HCHalted)
            {
                // Set run-stop bit to signal the start command.
                //  Section 2.3.1 of Intel EHCI Spec
                USBCMD |= EHCI_Consts.CMD_RunStopMask;
            }

            // Set the Interrupt Threshold Control (min. time between interrupts) to:
            //      8 Microframes (1ms).
            //  Section 2.3.1 of Intel EHCI Spec
            //          "Software modifications to this bit while HCHalted bit is 
            //           equal to zero results in undefined behavior."
            //  (Intel EHCI Spec, Section 2.3.1, Table 2-9, Interrupt Threshold Control
            USBCMD |= (uint)EHCI_InterruptThresholdControls.x08;

            //Set port routing to route all ports to the HC as opposed to companion HCs
            //  Section 2.3.8 of Intel EHCI Spec
            CONFIGFLAG = EHCI_Consts.CF;

            // We just told the any Companion HCs (CHCs) to transfer control to the Enhanced HC (EHC)
            //  This takes a moment so we want to wait for the HC to stabalise and for any new 
            //  interrupts to occur (if they are going to).
            SystemCalls.SleepThread(100);

#if EHCI_TRACE
            DBGMSG("StartHC() : End");
#endif
        }

        /// <summary>
        ///     Enables all the necessary interrupts for the EHCI driver. Does not add the Interrupts Handler.
        /// </summary>
        /// <remarks>
        ///     This method handles writing to the USBINTR register to enable the interrupts whic the EHCI
        ///     driver is designed to handle.
        /// </remarks>
        private void EnableInterrupts()
        {
            // Enable all the interrupts we wish to handle.
            //  In future, you would probably have to enable the periodic schedule interrupt(s) here
            //  too. 
            // Section 2.3.3 of Intel EHCI Spec
            USBINTR = EHCI_Consts.STS_AsyncInterrupt | EHCI_Consts.STS_HostSystemError | EHCI_Consts.STS_PortChange |
                      EHCI_Consts.STS_USBInterrupt | EHCI_Consts.STS_USBErrorInterrupt;
        }

        /// <summary>
        ///     Resets the host controller and consequently all ports.
        /// </summary>
        protected void ResetHC()
        {
#if EHCI_TRACE
            DBGMSG("ResetHC() : Start");
#endif

            // Clear the run/stop bit to halt the HC.
            // Section 2.3.1 of Intel EHCI Spec
            USBCMD &= ~EHCI_Consts.CMD_RunStopMask;

            // Wait for halt to complete. Intel EHCI Spec (Hardware) says:
            //  "The Host Controller must halt within 16 micro-frames after software clears the Run bit"
            //  (Intel EHCI Spec, Section 2.3.1, Table 2-9, Run/stop)
            // So the following loop will only ever run once or at most twice since 16 microframes is
            //  2ms.
            while (!HCHalted)
            {
                //Sleep for 1ms (8 micro frames)
                SystemCalls.SleepThread(1);
            }

            // Set the reset bit to signal the reset command
            //  Section 2.3.1 of Intel EHCI Spec
            //  Note: "PCI Configuration registers are not affected by this reset. All operational 
            //         registers, including port registers and port state machines are set to their 
            //         initial values. Port ownership reverts to the companion host controller(s), 
            //         with the side effects described in Section 4.2. Software must reinitialize 
            //         the host controller as described in Section 4.1 in order to return the host 
            //         controller to an operational state. 

            //          This bit is set to zero by the Host Controller when the reset process is 
            //          complete. Software cannot terminate the reset process early by writing a 
            //          zero to this register. 
            //
            //          Software should not set this bit to a one when the HCHalted bit in the 
            //          USBSTS register is a zero. Attempting to reset an actively running host 
            //          controller will result in undefined behavior."
            //  (Intel EHCI Spec, Section 2.3.1, Table 2-9, Host Controller Reset)
            USBCMD |= EHCI_Consts.CMD_HCResetMask;

            int timeout = 30;
            // Wait while the reset-bit is still set
            while ((USBCMD & EHCI_Consts.CMD_HCResetMask) != 0)
            {
                SystemCalls.SleepThread(10);

                timeout--;
                if (timeout == 0)
                {
                    ExceptionMethods.Throw(new Exception("EHCI HC Reset timed out!"));
#if EHCI_TRACE
                    DBGMSG("EHCI.Reset(): Timeout! USBCMD Reset bit not cleared!");
#endif
                    break;
                }
            }

#if EHCI_TRACE
            DBGMSG("ResetHC() : End");
#endif
        }

        /// <summary>
        ///     Deactivates legacy support mode if it is available.
        /// </summary>
        protected void DeactivateLegacySupport()
        {
            byte eecp = EECP;

#if EHCI_TRACE
            DBGMSG(((Framework.String)"DeactivateLegacySupport: eecp = ") + eecp);
#endif
            /*
            cf. EHCI 1.0 spec, 2.2.4 HCCPARAMS - Capability Parameters, Bit 15:8 (BYTE2)
            EHCI Extended Capabilities Pointer (EECP). Default = Implementation Dependent.
            This optional field indicates the existence of a capabilities list.
            A value of 00h indicates no extended capabilities are implemented.
            A non-zero value in this register indicates the offset in PCI configuration space
            of the first EHCI extended capability. The pointer value must be 40h or greater
            if implemented to maintain the consistency of the PCI header defined for this class of device.
            */
            // cf. http://wiki.osdev.org/PCI#PCI_Device_Structure

            //   eecp     // RO - This field identifies the extended capability.
            //      01h identifies the capability as Legacy Support.
            if (eecp >= 0x40)
            {
                byte eecp_id = 0;

                while (eecp != 0) // 00h indicates end of the ext. cap. list.
                {
#if EHCI_TRACE
                    DBGMSG(((Framework.String)"eecp = ") + eecp);
#endif
                    eecp_id = pciDevice.ReadRegister8(eecp);
#if EHCI_TRACE
                    DBGMSG(((Framework.String)"eecp_id = ") + eecp_id);
#endif
                    if (eecp_id == 1)
                    {
                        break;
                    }
                    eecp = pciDevice.ReadRegister8((byte)(eecp + 1));
                }
                byte BIOSownedSemaphore = (byte)(eecp + 2);
                // R/W - only Bit 16 (Bit 23:17 Reserved, must be set to zero)
                byte OSownedSemaphore = (byte)(eecp + 3);
                // R/W - only Bit 24 (Bit 31:25 Reserved, must be set to zero)
                byte USBLEGCTLSTS = (byte)(eecp + 4);
                // USB Legacy Support Control/Status (DWORD, cf. EHCI 1.0 spec, 2.1.8)

                // Legacy-Support-EC found? BIOS-Semaphore set?
                if (eecp_id == 1 && (pciDevice.ReadRegister8(BIOSownedSemaphore) & 0x01) != 0)
                {
#if EHCI_TRACE
                    DBGMSG("set OS-Semaphore.");
#endif
                    pciDevice.WriteRegister8(OSownedSemaphore, 0x01);

                    int timeout = 250;
                    // Wait for BIOS-Semaphore to clear
                    while ((pciDevice.ReadRegister8(BIOSownedSemaphore) & 0x01) != 0 && (timeout > 0))
                    {
                        timeout--;
                        SystemCalls.SleepThread(10);
                    }
                    // If the bit is clear, i.e. we didn't time-out 
                    // Note: This is a safer check than checking if "timeout == 0"
                    if ((pciDevice.ReadRegister8(BIOSownedSemaphore) & 0x01) == 0)
                    {
#if EHCI_TRACE
                        DBGMSG("BIOS-Semaphore being cleared.");
#endif
                        timeout = 250;
                        //Wait for OS Owned Semaphore to set.
                        while ((pciDevice.ReadRegister8(OSownedSemaphore) & 0x01) == 0 && (timeout > 0))
                        {
                            timeout--;
                            SystemCalls.SleepThread(10);
                        }
                    }
#if EHCI_TRACE
                    if ((pciDevice.ReadRegister8(OSownedSemaphore) & 0x01) != 0)
                    {
                        DBGMSG("OS-Semaphore being set.");
                    }
                    DBGMSG(((Framework.String)"Check: BIOSownedSemaphore: ") + pciDevice.ReadRegister8(BIOSownedSemaphore) + 
                                                     " OSownedSemaphore: " + pciDevice.ReadRegister8(OSownedSemaphore));
#endif

                    // USB SMI Enable R/W. 0=Default.
                    // The OS tries to set SMI to disabled in case that BIOS bit stays at one.
                    pciDevice.WriteRegister32(USBLEGCTLSTS, 0x0); // USB SMI disabled
                }
#if EHCI_TRACE
                else
                {
                    DBGMSG("BIOS did not own the EHCI. No action needed.");
                }
            }
            else
            {
                DBGMSG("No valid eecp found.");
#endif
            }

#if EHCI_TRACE
            BasicConsole.DelayOutput(2);
#endif
        }

        /// <summary>
        ///     Enables all ports and checks their line states.
        /// </summary>
        protected void EnablePorts()
        {
#if EHCI_TRACE
            DBGMSG("EnablePorts() : Start");
#endif

            // Enabling ports in this implementation just involves
            //  setting up virtual objects to represent each port.
            //  It then checks each port's status.
            // A more advanced implementation may also need to set 
            //  port power states and other similar features.
#if EHCI_TRACE
            DBGMSG("Enabling ports...");
#endif
            // We must add an entry to our list of ports for each root port
            //  This is because when we access a port (within the list) later,
            //  the code presumes the entry has been added.
            RootPorts.Empty();
            for (byte i = 0; i < RootPortCount; i++)
            {
                RootPorts.Add(new HCPort
                {
                    portNum = i
                });
            }
#if EHCI_TRACE
            DBGMSG("Added root ports.");
#endif
            // Store that we have now enabled ports.
            EnabledPorts = true;
#if EHCI_TRACE
            DBGMSG("Checking line statuses...");
#endif
            // Check the status of each root port.
            for (byte i = 0; i < RootPortCount; i++)
            {
                CheckPortLineStatus(i);
            }
#if EHCI_TRACE
            DBGMSG("Checked port line statuses.");

            DBGMSG("EnablePorts() : End");
#endif
        }

        /// <summary>
        ///     Resets the specified port.
        /// </summary>
        /// <param name="portNum">The port to reset.</param>
        public override void ResetPort(byte portNum)
        {
            // Enable power for the port
            //  Technically we only need to do this if PPC (Section 2.2.3 of Intel EHCI Spec) is 1.
            //  However, it is faster to just set the bit regardless since it has no side effects
            //  if PPC is 0.
            //      "The function of this bit depends on the value of the Port Power Control (PPC) 
            //       field in the HCSPARAMS register."
            //  (Intel EHCI Spec, Section 2.3.9, Table 2-16, Port Power)
            PORTSC[portNum] |= EHCI_Consts.PSTS_PowerOn;

            // Disable the port
            //  Section 2.3.9 of Intel EHCI Spec
            //  As per requirements in Port Reset:
            //      "Note: When software writes this bit to a one, it must also write a zero 
            //             to the Port Enable bit."
            //  (Intel EHCI Spec, Section 2.3.9, Table 2-16, Port Reset)
            PORTSC[portNum] &= ~EHCI_Consts.PSTS_Enabled;

            // Clear the Port Change interrupt.
            //  "...clear the connect change..."
            //  (Intel EHCI Spec, Section 4.2.2, Bullet Point 2)
            //  Section 2.3.2 of Intel EHCI Spec
            USBSTS |= EHCI_Consts.STS_PortChange;

            // If at this point the HC is halted, we have a fatal condition.
            //  As per requirements in Port Reset:
            //      "The HCHalted bit in the USBSTS register should be a zero before software 
            //       attempts to use this bit. The host controller may hold Port Reset asserted 
            //       to a one when the HCHalted bit is a one."
            //  (Intel EHCI Spec, Section 2.3.9, Table 2-16, Port Reset)
            if (HCHalted)
            {
                ExceptionMethods.Throw(new Exception("EHCI could not reset the port as the host controller is halted."));
            }
            // Clear the Interrupts Enable register. This prevents any further Port Change interrupts 
            //  during the reset sequence.
            //  Section 2.3.3 of Intel EHCI Spec
            USBINTR = 0;

            // Set the Port Reset bit to signal the HC to start the reset port sequence.
            //  Section 2.3.9 of Intel EHCI Spec
            PORTSC[portNum] |= EHCI_Consts.PSTS_PortReset;

            // Wait long enough for the Port Reset to complete.
            //      "Software must keep this bit at a one long enough to ensure the reset 
            //       sequence, as specified in the USB Specification Revision 2.0, completes."
            //Waits ~200ms which is ample amounts of time
            SystemCalls.SleepThread(200);

            // Terminate the reset port sequence.
            //      " Software writes a zero to this bit to terminate the bus reset sequence."
            //  (Intel EHCI Spec, Section 2.3.9, Table 2-16, Port Reset)
            PORTSC[portNum] &= ~EHCI_Consts.PSTS_PortReset;

            // Wait for the sequence to actually end.
            //  "Note that when software writes a zero to this bit there may be a delay before the bit 
            //   status changes to a zero. The bit status will not read as a zero until after the reset 
            //   has completed."
            //  (Intel EHCI Spec, Section 2.3.9, Table 2-16, Port Reset)

            // Timeout set to 5ms (Wait method is only approximate!)
            // "A host controller must terminate the reset and stabilize the state of the port within 2 
            //  milliseconds of software transitioning this bit from a one to a zero."
            //  (Intel EHCI Spec, Section 2.3.9, Table 2-16, Port Reset)
            uint timeout = 5;
            while ((PORTSC[portNum] & EHCI_Consts.PSTS_PortReset) != 0)
            {
                // ~1ms
                SystemCalls.SleepThread(1);

                timeout--;
                if (timeout == 0)
                {
                    ExceptionMethods.Throw(new Exception("EHCI port not reset and stabalised within 2ms!"));

                    break;
                }
            }

            // Reenable interrupts.
            EnableInterrupts();

            // A brief wait for stabilisation / interrupts to occur.
            //  (This is not grounded in any spec but seems logical to me. It may be possible
            //   to remove this wait but it's only short and occurs infrequently so no harm in
            //   having it.)
            SystemCalls.SleepThread(20);
        }

        /// <summary>
        ///     The static wrapper for the interrupt handler.
        /// </summary>
        /// <param name="data">The EHCI state object.</param>
        /// <summary>
        ///     The interrupt handler for all EHCI interrupts.
        /// </summary>
        internal override void IRQHandler()
        {
#if EHCI_TRACE
            DBGMSG("EHCI Interrupt Handler called");
#endif

            //Check interrupt came from this EHCI
            uint val = USBSTS;
            if (val == 0)
            {
#if EHCI_TRACE
                DBGMSG("Interrupt ignored.");
#endif
                return;
            }
            // Reset interrupt occured flag by writing the value back to the register
            //  Section 2.3.2 of Intel EHCI Spec
            USBSTS = val;

            // If the interrupt signals a USB error:
            if ((val & EHCI_Consts.STS_USBErrorInterrupt) != 0u)
            {
                // "The Host Controller sets this bit to 1 when completion of a USB transaction results 
                //  in an error condition (e.g., error counter underflow). If the TD on which the error 
                //  interrupt occurred also had its IOC bit set, both this bit and USBINT bit are set."
                //  (Intel EHCI Spec, Section 2.3.2, Table 2-10, USB Error Interrupt)

                // The latter part of that quote is important. Even if the transaction hit an error
                //  condition, we will still detect the Interrupt On Complete condition further down.
                //  This prevents us hitting an infinite while-loop in the AddToAsyncSchedule method.
#if EHCI_TRACE
                DBGMSG("USB Error Interrupt!");
#endif
            }

            // If the interrupt signals a port has changed:
            if ((val & EHCI_Consts.STS_PortChange) != 0u)
            {
                // And only if we have enabled ports:
                if (EnabledPorts)
                {
                    // Mark that a port has changed
                    AnyPortsChanged = true;
                }
            }

            // If the interrupt signals the HC has hit an error:
            if ((val & EHCI_Consts.STS_HostSystemError) != 0u)
            {
                // We will attempt to restart the HC. If, however, the error is occurring
                //  during the restart, we will just get another error interrupt as we try to
                //  restart. If we don't have the check condition below, we hit an infinite loop
                //  of "Start HC -> (Host Error) -> Interrupt -> Start HC -> ..."

                // If we haven't tried to reset the HC once already...
                if (!IrrecoverableError)
                {
                    // Store that we have already hit HC error once before.
                    HostSystemErrors++;
                    // HC Restart will occur the next time Update is called.
                }
            }
            else
            {
                HostSystemErrors = 0;
            }

            // If the interrupt signals the completion of the last transaction of 
            //  an Async Queue Head (i.e. transfer)
            if ((val & EHCI_Consts.STS_USBInterrupt) != 0u)
            {
                // And we were expecting a transfer to complete i.e. we were expecting this interrupt
                //  to occur.
                //  Note: Without this condition, a spurious interrupt could cause us to decrement 
                //        unsigned 0, which would result in 0xFFFFFFFF and cause an infinite loop in 
                //        AddToAsyncSchedule.
                if (USBIntCount != 0)
                {
                    // Decrement our expected interrupt count.
                    USBIntCount--;
                }
#if EHCI_TRACE
    //DBGMSG(((Framework.String)"EHCI: USB Interrupt occurred! USBIntCount: ") + USBIntCount);
                DBGMSG("EHCI: USB Interrupt occurred!");
#endif
            }

            if ((val & EHCI_Consts.STS_AsyncInterrupt) != 0u)
            {
                // And we were expecting an Async Doorbell to occur i.e. we were expecting this interrupt
                //  to occur.
                //  Note: Without this condition, a spurious interrupt could cause us to decrement 
                //        unsigned 0, which would result in 0xFFFFFFFF and cause an infinite loop in _IssueTransfer.
                if (AsyncDoorbellIntCount != 0)
                {
                    // Decrement our expected interrupt count.
                    AsyncDoorbellIntCount--;
                }
#if EHCI_TRACE
                DBGMSG("EHCI: Async Doorbell interrupt occurred!");
#endif
            }

#if EHCI_TRACE
            BasicConsole.DelayOutput(5);
#endif
        }

        /// <summary>
        ///     Checks all ports for any changes.
        /// </summary>
        protected void PortCheck()
        {
            // We are handling port changes now so mark that there are no port changes.
            AnyPortsChanged = false;

            // Go through each root port:
            for (byte j = 0; j < RootPortCount; j++)
            {
                // If its connected status has changed:
                //  Section 2.3.9 of Intel EHCI Spec
                if ((PORTSC[j] & EHCI_Consts.PSTS_ConnectedChange) != 0)
                {
                    // Reset the connected change status
                    //  "Software sets this bit to 0 by writing a 1 to it."
                    //  (Intel EHCI Spec, Section 2.3.9, Table 2-16, Connect Status Change)
                    PORTSC[j] |= EHCI_Consts.PSTS_ConnectedChange;

                    // If a device is connected:
                    //  Section 2.3.9 of Intel EHCI Spec
                    if ((PORTSC[j] & EHCI_Consts.PSTS_Connected) != 0)
                    {
                        CheckPortLineStatus(j);
                    }
                    else
                    {
                        //Otherwise, no device is connected. So we have two possibilities:
                        //  1) A device has been disconnected.
                        //  2) The port has been given back to the EHCI from a companion
                        //      controller.

                        // Clear the Port Owner bit so EHCI owns the port
                        //    "A one in this bit means that a companion host controller owns and 
                        //     controls the port. See Section 4.2 for operational details."
                        //  (Intel EHCI Spec, Section 2.3.9, Table 2-16, Port Owner)
                        PORTSC[j] &= ~EHCI_Consts.PSTS_CompanionHCOwned;

                        // Free the port if a device had been connected as the device is no longer 
                        //  connected.
                        if (((HCPort)RootPorts[j]).deviceInfo != null)
                        {
                            ((HCPort)RootPorts[j]).deviceInfo.FreePort();
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Checks the specified port's line state. Calls Detect device or releases the port to the companion
        ///     host controller if the connected device is a low/full speed device.
        /// </summary>
        /// <param name="portNum">The port to check.</param>
        protected void CheckPortLineStatus(byte portNum)
        {
#if EHCI_TRACE
            DBGMSG("CheckPortLineStatus() : Start");
#endif

            // Make sure we only check ports for which a device is actually connected.
            //  Section 2.3.9 of Intel EHCI Spec
            if ((PORTSC[portNum] & EHCI_Consts.PSTS_Connected) == 0)
            {
#if EHCI_TRACE
                DBGMSG("Port not connected.");
#endif
                return;
            }

            // Get the Line Status of the port. 
            //  Section 2.3.9 of Intel EHCI Spec
            //      "These bits reflect the current logical levels of the D+ (bit 11) and D- 
            //       (bit 10) signal lines. These bits are used for detection of low-speed 
            //       USB devices prior to the port reset and enable sequence. This field is 
            //       valid only when the port enable bit is zero and the current connect 
            //       status bit is set to a one."
            //  (Intel EHCI Spec, Section 2.3.9, Table 2-16, Line Status)
            //  This condition is met because Check Port Line Status calls Detect Device
            //   which is the only caller of ResetPort. There are two cases:
            //      - After an HC reset, all ports are disabled. So condition is met.
            //      - After new device connection, the specific port is disabled, so condition is met.
            byte lineStatus = (byte)((PORTSC[portNum] >> 10) & 3); // bits 11:10

            // Switch the various possible line states.
            switch (lineStatus)
            {
                case 1: // K-state
#if EHCI_TRACE
                    DBGMSG("Low-speed device attached. Releasing port.");
#endif
                    // In this case, a low-speed device has been attached so we must release it to the 
                    //  companion host controller.
                    //  (Intel EHCI Spec, Section 2.3.9, Table 2-16, Line Status)
                    //  (Intel EHCI Spec, Section 2.3.9, Table 2-16, Port Owner)
                    PORTSC[portNum] |= EHCI_Consts.PSTS_CompanionHCOwned;
                    break;
                case 0: // SE0
                case 2: // J-state
                case 3: // undefined
                    // In all these cases, we perform EHCI port reset and device detection
                    //  (Intel EHCI Spec, Section 2.3.9, Table 2-16, Line Status)
                    DetectDevice(portNum);
                    break;
            }

#if EHCI_TRACE
            DBGMSG("CheckPortLineStatus() : End");
#endif
        }

        /// <summary>
        ///     Attempts to detect a device connected to the specified port.
        ///     If one is detected, it creates the device through the USBManager.
        /// </summary>
        /// <param name="portNum">The port number to try and detect a device on.</param>
        protected void DetectDevice(byte portNum)
        {
#if EHCI_TRACE
            DBGMSG("Detecting device...");
#endif
            // Reset the port
            //  The following is handled by ResetPort:
            //  "...[issue] a request to clear the connect change, followed by a request to 
            //   reset and enable the port."
            //  (Intel EHCI Spec, Section 4.2.2, Bullet Point 2)
            ResetPort(portNum);
#if EHCI_TRACE
            DBGMSG("Reset port.");
#endif
            // If we have enabled ports and the port is powered on:
            //  Section 2.3.9 of Intel EHCI Spec
            if (EnabledPorts && ((PORTSC[portNum] & EHCI_Consts.PSTS_PowerOn) != 0))
            {
#if EHCI_TRACE
                DBGMSG("Device powered on.");
#endif
                //      "  The EHCI Driver checks the PortEnable bit in the PORTSC register. If set to 
                //         a one, the connected device is a high-speed device and EHCI Driver (root hub 
                //         emulator) issues a change report to the hub driver and the hub driver 
                //         continues to enumerate the attached device. 
                //       
                //       • At the time the EHCI Driver receives the port reset and enable request the 
                //         LineStatus bits might indicate a low-speed device. Additionally, when the port 
                //         reset process is complete, the PortEnable field may indicate that a full-speed 
                //         device is attached. In either case the EHCI driver sets the PortOwner bit in 
                //         the PORTSC register to a one to release port ownership to a companion host 
                //         controller."
                //  (Intel EHCI Spec, Section 4.2.2, Bullet Points 2 and 3)

                // If the port is enabled, it is a high-speed device
                //  Section 2.3.9 of Intel EHCI Spec
                if ((PORTSC[portNum] & EHCI_Consts.PSTS_Enabled) != 0)
                {
#if EHCI_TRACE
                    DBGMSG("Setting up USB device.");
#endif
                    // So we atempt to set up the USB device attached to the port
                    SetupUSBDevice(portNum);
                }
                else
                {
#if EHCI_TRACE
                    DBGMSG("Full-speed device attached. Releasing port.");
                    BasicConsole.DelayOutput(2);
#endif
                    // Otherwise, the device is a full-speed device and we must release it to the 
                    //  companion host controller
                    // Note: Low-speed devices will have been handled in Check Port Line Status.
                    // Note: In either case (low-speed or full-speed) the correct response is to
                    //       hand control to the companion host controller.

                    PORTSC[portNum] |= EHCI_Consts.PSTS_CompanionHCOwned;
                }
            }
#if EHCI_TRACE
            DBGMSG("End DetectDevice()");
#endif
        }

        /// <summary>
        ///     Sets up a USB transfer for sending via the EHCI.
        /// </summary>
        /// <param name="transfer">The transfer to set up.</param>
        protected override void _SetupTransfer(USBTransfer transfer)
        {
            // Allocate memory for the transfer strcture. This is a queue head strcture.
            //  It gets appended to the end of the Async Queue in AddToAsyncSchedule.
            // This memory must be allocated on a 32-byte boundary as per EHCI spec.
            //      "The memory structure referenced by this physical memory pointer is 
            //       assumed to be 32-byte (cache line) aligned."
            //  Section 2.3.7 of Intel EHCI Spec
            // Note: This sets the virtual address. This allows it to be accessed and freed by the 
            //       driver. However, when passing the address to the HC, it must be converted to
            //       a physical address.
            transfer.underlyingTransferData =
                (EHCI_QueueHead_Struct*)
                    Heap.AllocZeroedAPB((uint)sizeof(EHCI_QueueHead_Struct), 32, "EHCI : _SetupTransfer");
        }

        /// <summary>
        ///     Sets up a SETUP transaction and adds it to the specified transfer.
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
        protected override void _SETUPTransaction(USBTransfer transfer, USBTransaction uTransaction, bool toggle,
            ushort tokenBytes,
            byte type, byte req, byte hiVal, byte loVal, ushort index, ushort length)
        {
            // Create an EHCI-specific object to describe the transaction
            EHCITransaction eTransaction = new EHCITransaction();
            // Store the underlying HC-specific transaction info
            uTransaction.underlyingTz = eTransaction;
            // SETUP transaction so there is no input buffer
            eTransaction.inBuffer = null;
            eTransaction.inLength = 0u;
            // Create and initialise the SETUP queue transfer descriptor
            eTransaction.qTD = CreateQTD_SETUP(null, toggle, tokenBytes, type, req, hiVal, loVal, index, length);

            // If the number of existing transactions is greater than 0
            //  i.e. some transactions have already been added.
            if (transfer.transactions.Count > 0)
            {
                // Get the previous (i.e. last) transaction then the underlying transaction from it
                EHCITransaction eLastTransaction =
                    (EHCITransaction)
                        ((USBTransaction)transfer.transactions[transfer.transactions.Count - 1]).underlyingTz;
                // Create a wrapper for the last transaction (qTD)
                EHCI_qTD lastQTD = eLastTransaction.qTD;
                // Set the Next Transaction (qTD) Pointer on the previous qTD to point to the qTD
                //  we just created. 
                // Note: The NextqTDPointer must be the physical address of qTD data.
                lastQTD.NextqTDPointer = (EHCI_qTD_Struct*)GetPhysicalAddress(eTransaction.qTD.qtd);
                // Mark the previous qTD's Next Transaction Pointer as valid.
                lastQTD.NextqTDPointerTerminate = false;
            }
        }

        /// <summary>
        ///     Sets up an IN transaction and adds it to the specified transfer.
        /// </summary>
        /// <param name="transfer">The transfer to which the transaction should be added.</param>
        /// <param name="uTransaction">The USB Transaction to convert to an EHCI transaction.</param>
        /// <param name="toggle">The transaction toggle state.</param>
        /// <param name="buffer">The buffer to store the incoming data in.</param>
        /// <param name="length">The length of the buffer.</param>
        protected override void _INTransaction(USBTransfer transfer, USBTransaction uTransaction, bool toggle,
            void* buffer, ushort length)
        {
            // Create an EHCI-specific object to describe the transaction
            EHCITransaction eTransaction = new EHCITransaction();
            // Store the underlying HC-specific transaction info
            uTransaction.underlyingTz = eTransaction;
            // IN transaction so use the supplied input data buffer
            eTransaction.inBuffer = buffer;
            eTransaction.inLength = length;

#if EHCI_TRACE
            DBGMSG(((Framework.String)"IN Transaction : buffer=") + (uint)buffer);

            DBGMSG(((Framework.String)"IN Transaction : Before CreateQTD : bufferPtr=&qTDBuffer=") + (uint)buffer);
#endif

            // Create and initialise the IN queue transfer descriptor
            eTransaction.qTD = CreateQTD_IO(null, 1, toggle, length, length);

#if EHCI_TRACE
            DBGMSG(((Framework.String)"IN Transaction : After CreateQTD : bufferPtr=&qTDBuffer=") + (uint)buffer + ", Buffer0=" + (uint)eTransaction.qTD.Buffer0);
#endif
            // If the number of existing transactions is greater than 0
            //  i.e. some transactions have already been added. 
            if (transfer.transactions.Count > 0)
            {
                // Get the previous (i.e. last) transaction then the underlying transaction from it
                EHCITransaction eLastTransaction =
                    (EHCITransaction)
                        ((USBTransaction)transfer.transactions[transfer.transactions.Count - 1]).underlyingTz;
                // Create a wrapper for the last transaction (qTD)
                EHCI_qTD lastQTD = eLastTransaction.qTD;
                // Set the Next Transaction (qTD) Pointer on the previous qTD to point to the qTD
                //  we just created. 
                // Note: The NextqTDPointer must be the physical address of qTD data.
                lastQTD.NextqTDPointer = (EHCI_qTD_Struct*)GetPhysicalAddress(eTransaction.qTD.qtd);
                // Mark the previous qTD's Next Transaction Pointer as valid.
                lastQTD.NextqTDPointerTerminate = false;
            }
        }

        /// <summary>
        ///     Sets up an IN transaction and adds it to the specified transfer.
        /// </summary>
        /// <param name="transfer">The transfer to which the transaction should be added.</param>
        /// <param name="uTransaction">The USB Transaction to convert to an EHCI transaction.</param>
        /// <param name="toggle">The transaction toggle state.</param>
        /// <param name="buffer">The buffer of outgoing data.</param>
        /// <param name="length">The length of the buffer.</param>
        protected override void _OUTTransaction(USBTransfer transfer, USBTransaction uTransaction, bool toggle,
            void* buffer, ushort length)
        {
            // Create an EHCI-specific object to describe the transaction
            EHCITransaction eTransaction = new EHCITransaction();
            // Store the underlying HC-specific transaction info
            uTransaction.underlyingTz = eTransaction;
            // OUT transaction so there is no input buffer
            eTransaction.inBuffer = null;
            eTransaction.inLength = 0u;

            // Create and initialise the OUT queue transfer descriptor
            EHCI_qTD theQTD = CreateQTD_IO(null, 0, toggle, length, length);
            // Set the qTD structure in the transaction description object
            eTransaction.qTD = theQTD;
            // If there is an output buffer and it has > 0 length:
            if (buffer != null && length != 0)
            {
                // Copy the data from the output buffer to the transaction's output buffer
                // The transaction's output buffer has been allocated so it as aligned correctly
                //  where as there is no guarantee the output buffer passed to us has been so we
                //  must copy the data across.
                MemoryUtils.MemCpy(theQTD.Buffer0VirtAddr, (byte*)buffer, length);

#if EHCI_TRACE
                BasicConsole.WriteLine("EHCI: OUTTransaction - Buffer0:");
                BasicConsole.DumpMemory(theQTD.Buffer0VirtAddr, length);
#endif
            }

            // If the number of existing transactions is greater than 0
            //  i.e. some transactions have already been added. 
            if (transfer.transactions.Count > 0)
            {
                // Get the previous (i.e. last) transaction then the underlying transaction from it
                EHCITransaction eLastTransaction =
                    (EHCITransaction)
                        ((USBTransaction)transfer.transactions[transfer.transactions.Count - 1]).underlyingTz;
                // Create a wrapper for the last transaction (qTD)
                EHCI_qTD lastQTD = eLastTransaction.qTD;
                // Set the Next Transaction (qTD) Pointer on the previous qTD to point to the qTD
                //  we just created. 
                // Note: The NextqTDPointer must be the physical address of qTD data.
                lastQTD.NextqTDPointer = (EHCI_qTD_Struct*)GetPhysicalAddress(eTransaction.qTD.qtd);
                // Mark the previous qTD's Next Transaction Pointer as valid.
                lastQTD.NextqTDPointerTerminate = false;
            }
        }

        /// <summary>
        ///     Issues the specified transfer to the physical device via the async schedule.
        /// </summary>
        /// <param name="transfer">The transfer to issue.</param>
        protected override void _IssueTransfer(USBTransfer transfer)
        {
            if (HostSystemErrors > 0 || IrrecoverableError)
            {
                ExceptionMethods.Throw(
                    new Exception(
                        "Cannot issue transfer through EHCI because the host controller has encountered a host system error."));
            }
            else if (HCHalted)
            {
                ExceptionMethods.Throw(
                    new Exception("Cannot issue transfer through EHCI because the host controller is currently halted."));
            }

            // Please note: The word "completed" is not synonymous with "succeeded". 
            //              "Completed" means the hardware, firmware or software finished
            //              processing something.
            //              "Succeeded" means the hardware, firmware or software finished
            //              processing something and there were no errors during processing.

            // Get the last qTD of the transfer
            EHCITransaction lastTransaction =
                (EHCITransaction)((USBTransaction)transfer.transactions[transfer.transactions.Count - 1]).underlyingTz;
            EHCI_qTD lastQTD = lastTransaction.qTD;
            // Enable the Interrupt on Complete. This allows us to detect the end of the entire transfer 
            //  when the USB Interrupt occurs.
            lastQTD.InterruptOnComplete = true;

#if EHCI_TRACE
    //Test walking the transaction tree
            bool treeOK = true;
            for (int k = 0; k < transfer.transactions.Count - 1; k++)
            {
                EHCITransaction transaction1 = (EHCITransaction)((USBTransaction)transfer.transactions[k]).underlyingTz;
                EHCITransaction transaction2 = (EHCITransaction)((USBTransaction)transfer.transactions[k + 1]).underlyingTz;
                EHCI_qTD qtd1 = transaction1.qTD;
                treeOK = treeOK && (qtd1.NextqTDPointer == GetPhysicalAddress(transaction2.qTD.qtd)) && !qtd1.NextqTDPointerTerminate;
                if (!treeOK)
                {
                    BasicConsole.Write(((Framework.String)"Incorrect tansfer index: ") + k);
                    if (qtd1.NextqTDPointer != GetPhysicalAddress(transaction2.qTD.qtd))
                    {
                        BasicConsole.WriteLine(((Framework.String)"    > Pointers incorrect! QTD1.NextPtr=") + (uint)qtd1.NextqTDPointer + ", &QTD2=" + (uint)GetPhysicalAddress(transaction2.qTD.qtd));
                    }
                    else if (qtd1.NextqTDPointerTerminate)
                    {
                        BasicConsole.WriteLine("    > QTD1.NextTerminate incorrect!");
                    }
                    else
                    {
                        BasicConsole.WriteLine("    > Previous transaction was incorrect.");
                    }
                }
            }
            {
                treeOK = treeOK && lastQTD.NextqTDPointerTerminate;
                if (!treeOK)
                {
                    BasicConsole.WriteLine("Incorrect tansfer index: last");
                    if (!lastQTD.NextqTDPointerTerminate)
                    {
                        BasicConsole.WriteLine("    > LastQTD.NextTerminate incorrect!");
                    }
                    else
                    {
                        BasicConsole.WriteLine("    > Previous transaction was incorrect.");
                    }
                }
            }
            DBGMSG(((Framework.String)"Transfer transactions tree OK: ") + treeOK);
            BasicConsole.DelayOutput(10);
#endif
            // Get the first qTD of the transfer. This is passed to InitQH to tell it the start of the linked
            //  list of transactions.
            EHCITransaction firstTransaction =
                (EHCITransaction)((USBTransaction)transfer.transactions[0]).underlyingTz;
            // Init the Queue Head for this transfer
            InitQH((EHCI_QueueHead_Struct*)transfer.underlyingTransferData,
                (EHCI_QueueHead_Struct*)transfer.underlyingTransferData,
                firstTransaction.qTD.qtd,
                false,
                transfer.device.address,
                transfer.endpoint,
                transfer.packetSize);

            // Attempt to issue the transfer until it either succeeds or we reach our 
            //  maximum number of retries or an irrecoverable host system error occurs. 
            //  The maxmimum number of retries is an entirely arbitary, internal number.
            for (byte i = 0; i < EHCI_Consts.NumAsyncListRetries && !transfer.success && !IrrecoverableError; i++)
            {
#if EHCI_TRACE
                transfer.success = true;
                for (int k = 0; k < transfer.transactions.Count; k++)
                {
                    EHCITransaction transaction = (EHCITransaction)((USBTransaction)transfer.transactions[k]).underlyingTz;
                    byte status = transaction.qTD.Status;
                    transfer.success = transfer.success && (status == 0 || status == Utils.BIT(0));

                    DBGMSG(((Framework.String)"PRE Issue: Transaction ") + k + " status: " + status);
                }
                if (!transfer.success)
                {
                    DBGMSG("EHCI: PRE Issue - Transfer detected as failed.");
                    BasicConsole.DelayOutput(1);
                }
                else
                {
                    DBGMSG("EHCI: PRE Issue - Transfer OK.");
                }
#endif
                // Add it to the async schedule. This will cause the HC to attempt to send the transactions.
                //  This is a blocking method that waits until the HC signals the last transaction of the 
                //  transfer is complete. This does not mean all or the last transaction completed succesfully.
                AddToAsyncSchedule(transfer);
                // If during the transfer we hit an irrecoverable host system error:
                if (IrrecoverableError)
                {
                    // Mark the transfer as failed
                    transfer.success = false;
#if EHCI_TRACE
                    DBGMSG("EHCI: Irrecoverable error! No retry.");
                    BasicConsole.DelayOutput(2);
#endif
                }
                else
                {
                    // Assume the transfer succeeded to start with
                    transfer.success = true;
                    // Then check each transaction to see if it succeeded.
                    for (int k = 0; k < transfer.transactions.Count; k++)
                    {
                        // Get the transaction to check
                        EHCITransaction transaction =
                            (EHCITransaction)((USBTransaction)transfer.transactions[k]).underlyingTz;
                        // Get the transaction's status
                        byte status = transaction.qTD.Status;
                        // If the status == 0, it indicates success
                        // If bit 0 of the status is set and the other buts are 0, 
                        //  then since this must be a High Speed endpoint, it is just the
                        //  Ping State.
                        // It is worth noting that the Ping State bit is only valid if the 
                        //  endpoint is an OUT endpoint. However, the specification also suggests
                        //  the this bit is only an error bit if the device is a low- or full-speed
                        //  device. This means the value for IN endpoints on High Speed devices is 
                        //  somewhat undefined and I am lead to assume the value must always be 0.
                        transfer.success = transfer.success && (status == 0 || status == Utils.BIT(0));

#if EHCI_TRACE
                        DBGMSG(((Framework.String)"POST Issue: Transaction ") + k + " status: " + status);
#endif
                    }

#if EHCI_TRACE
                    if (!transfer.success)
                    {
                        DBGMSG(((Framework.String)"EHCI: Retry transfer: ") + (i + 1));
                        BasicConsole.DelayOutput(2);

                        // Reset the status bits so the transactions are active again
                        for (int k = 0; k < transfer.transactions.Count; k++)
                        {
                            EHCITransaction transaction = (EHCITransaction)((USBTransaction)transfer.transactions[k]).underlyingTz;
                            byte status = transaction.qTD.Status;
                            if (!(status == 0 || status == Utils.BIT(0)))
                            {
                                transaction.qTD.Status = 0x80;
                            }
                        }
                    }
#endif
                }
            }

            // After the transfer has completed, we can free the underlying queue head memory.
            //        At this point we use the Async Doorbell to confirm the
            //        HC has finished using the queue head and that all caches of 
            //        pointers to the queue head have been released. 

            AsyncDoorbellIntCount = 1;
            USBCMD |= EHCI_Consts.CMD_AsyncInterruptDoorbellMask;
            while (AsyncDoorbellIntCount > 0)
            {
                SystemCalls.SleepThread(5);
            }

            Heap.Free(transfer.underlyingTransferData);
            // Loop through each transaction in the transfer
            for (int k = 0; k < transfer.transactions.Count; k++)
            {
                // Get the current transaction
                EHCITransaction transaction = (EHCITransaction)((USBTransaction)transfer.transactions[k]).underlyingTz;
                // Create a wrapper for the underlying qTD of the transaction
                EHCI_qTD theQTD = transaction.qTD;

                // If the transaction has an input buffer, we must copy the input data from the qTD
                //  to the input buffer. 
                // Note: The reason the qTD has a different buffer to the input buffer is because when
                //       the input buffer was provided, there was no guarantee it had been properly 
                //       allocated (i.e. size and alignment may have been incorrect)
                if (transaction.inBuffer != null && transaction.inLength != 0)
                {
#if EHCI_TRACE
                    DBGMSG(((Framework.String)"Doing MemCpy of in data... inBuffer=") + (uint)transaction.inBuffer + 
                                               ", qTDBuffer=" + (uint)transaction.qTD.qtd + 
                                               ", inLength=" + transaction.inLength + ", Data to copy: ");
#endif
                    // Copy the memory
                    MemoryUtils.MemCpy((byte*)transaction.inBuffer, theQTD.Buffer0VirtAddr, transaction.inLength);

#if EHCI_TRACE
    //for (int i = 0; i < transaction.inLength; i++)
    //{
    //    DBGMSG(((Framework.String)"i=") + i + ", qTDBuffer[i]=" + ((byte*)transaction.qTD.qtd)[i] + ", inBuffer[i]=" + ((byte*)transaction.inBuffer)[i]);
    //}
#endif
#if EHCI_TRACE
                    DBGMSG("Done.");
                    BasicConsole.DelayOutput(2);
#endif
                }
                // Free the qTD buffer(s)
                Heap.Free(theQTD.Buffer0VirtAddr);
                // Free the qTD
                theQTD.Free();
            }

#if EHCI_TRACE
            if (transfer.success)
            {
                DBGMSG("EHCI: Transfer succeeded.");
            }
            else
            {
                DBGMSG("EHCI:IssueTransfer(): Transfer failed.");
            }
            BasicConsole.DelayOutput(2);
#endif
        }

        /// <summary>
        ///     Initialises the async schedule.
        /// </summary>
        protected void InitializeAsyncSchedule()
        {
#if EHCI_TRACE
            DBGMSG("InitializeAsyncSchedule() : Start");
#endif

            // If there is no idle queue head:
            if (IdleQueueHead == null)
            {
                // Create one and set it as both the idle and tail queue heads.
                IdleQueueHead = TailQueueHead = new EHCI_QueueHead().queueHead;

                // The Idle Queue Head always remains in the queue and it does nothing.
                // It is also always the head of the reclamation list. (Note: The 
                // reclamation list is not made use of by this driver implementation).

                // The spec makes no explicit mention of needing an Idle Queue Head.
                // However, the need is clearly implicitly there. The spec says that 
                // while the async queue is enabled, it will use the ASYNCLISTADDR to
                // try and traverse the async queue. This clearly suggests then, that 
                // the ASYNCLISTADDR must be valid for the entire time that the async 
                // schedule is enabled. For this to be possible without continually 
                // sending actual transfers, we must have the idle transfer (queue head).

                // Section 4.8 of Intel EHCI Spec
            }
            // Initialise the idle queue head as a circular list of 1 item - i.e. idle queue head
            //  points to itself. And as the head of the reclamation list.
            InitQH(IdleQueueHead, IdleQueueHead, null, true, 0, 0, 0);
            // Set the Async List Address to point to the idle queue head.
            // Note: Physical address of queue head is required.
            // Section 2.3.7 of Intel EHCI Spec
            ASYNCLISTADDR = (EHCI_QueueHead_Struct*)GetPhysicalAddress(IdleQueueHead);
            // Enable the async schedule.
            EnableAsyncSchedule();

#if EHCI_TRACE
            DBGMSG("InitializeAsyncSchedule() : End");
#endif
        }

        /// <summary>
        ///     Enables the async schedule.
        /// </summary>
        protected void EnableAsyncSchedule()
        {
#if EHCI_TRACE
            DBGMSG("EHCI: Enabling Async Schedule...");
#endif
            // Set the command to enable the async schedule
            //  Section 2.3.1 of Intel EHCI Spec
            USBCMD |= EHCI_Consts.CMD_AsyncScheduleEnableMask;

            uint timeout = 7;
            // Wait until the schedule is actually enabled
            // Section 2.3.2 of Intel EHCI Spec
            //      "The Host Controller is not required to immediately disable or enable the 
            //       Asynchronous Schedule when software transitions the Asynchronous Schedule 
            //       Enable bit in the USBCMD register"
            //  (Intel EHCI Spec, Section 2.3.2, Table 2-10, Asynchronous Schedule Status)
            while ((USBSTS & EHCI_Consts.STS_AsyncEnabled) == 0)
            {
#if EHCI_TRACE
                DBGMSG("EHCI: Waiting for enabled...");
#endif
                timeout--;
                if (timeout > 0)
                {
                    //~10ms
                    SystemCalls.SleepThread(10);
                }
                else
                {
                    ExceptionMethods.Throw(
                        new Exception("Async schedule enable timed out! The schedule was not enabled."));
                    break;
                }
            }
#if EHCI_TRACE
            DBGMSG("EHCI: Async Schedule enabled.");
#endif
        }

        /// <summary>
        ///     Adds a transfer for the async schedule.
        /// </summary>
        /// <param name="transfer">The transfer to add.</param>
        protected void AddToAsyncSchedule(USBTransfer transfer)
        {
#if EHCI_TRACE
            DBGMSG("EHCI: Add to Async Schedule");
#endif

            // Set the expected number of USB Interrupts to 1
            //  1 because we expect one and only one for the last transaction of the transfer
            //  which we flagged to Interrupt On Complete in IssueTransfer.
            USBIntCount = 1;

            // If the schedule is disabled:
            // Section 2.3.2 of Intel EHCI Spec
            if ((USBSTS & EHCI_Consts.STS_AsyncEnabled) == 0)
            {
                // Enable / start the async schedule
                EnableAsyncSchedule();
            }

#if EHCI_TRACE
            DBGMSG("EHCI: Updating queue");
#endif

            // Save the old tail queue head (which may not be the idle queue head) (save in a wrapper)
            EHCI_QueueHead oldTailQH = new EHCI_QueueHead(TailQueueHead);
            // The new queue head will now be end of the queue
            TailQueueHead = (EHCI_QueueHead_Struct*)transfer.underlyingTransferData;

            // Create wrappers for the idle and tail queue heads.
            EHCI_QueueHead idleQH = new EHCI_QueueHead(IdleQueueHead);
            EHCI_QueueHead tailQH = new EHCI_QueueHead(TailQueueHead);
            // Create the ring. Link the new queue head with idleQH (which is always the head of the queue)
            tailQH.HorizontalLinkPointer = (EHCI_QueueHead_Struct*)GetPhysicalAddress(IdleQueueHead);
            // Insert the queue head into the queue as an element behind old queue head
            oldTailQH.HorizontalLinkPointer = (EHCI_QueueHead_Struct*)GetPhysicalAddress(TailQueueHead);

#if EHCI_TRACE
            DBGMSG("EHCI: About to wait for transaction complete...");
#endif

            int timeout = 100;
            while (USBIntCount > 0 && (HostSystemErrors == 0) && !IrrecoverableError && --timeout > 0)
            {
#if EHCI_TRACE
                DBGMSG("EHCI: Waiting for transaction complete...");
#endif

                SystemCalls.SleepThread(50);

#if EHCI_TRACE
                if (timeout % 10 == 0)
                {
                    BasicConsole.WriteLine("Waiting for transfer to complete...");
                }
#endif
            }

#if EHCI_TRACE
            DBGMSG("EHCI: Transaction completed.");
#endif

#if EHCI_TRACE
            if (timeout == 0)
            {
                BasicConsole.WriteLine("Transfer timed out.");
            }
#endif

            // Restore the link of the old tail queue head to the idle queue head
            oldTailQH.HorizontalLinkPointer = (EHCI_QueueHead_Struct*)GetPhysicalAddress(IdleQueueHead);
            // Queue head done. 
            // Because nothing else touches the async queue and this method is a synchronous method, 
            //  the idle queue head will now always be the end of the queue again.
            TailQueueHead = oldTailQH.queueHead;

#if EHCI_TRACE
            DBGMSG("EHCI: Returning from Add to Async Schedule");
#endif
        }

        /// <summary>
        ///     Initialises a queue head - memory must already be allocated.
        /// </summary>
        /// <param name="headPtr">A pointer to the queue head structure to initialise.</param>
        /// <param name="horizPtr">
        ///     The virtual address of the next queue head in the list (or the first queue head since the
        ///     async queue is a circular buffer). This is translated into the physical address internally.
        /// </param>
        /// <param name="firstQTD">A pointer to the first qTD of the queue head.</param>
        /// <param name="H">The Head of Reclamation list flag.</param>
        /// <param name="deviceAddr">The address of the USB device to which this queue head belongs.</param>
        /// <param name="endpoint">The endpoint number of the USB device to which this queue head belongs.</param>
        /// <param name="maxPacketSize">The maximum packet size to use when transferring.</param>
        protected void InitQH(EHCI_QueueHead_Struct* headPtr, EHCI_QueueHead_Struct* horizPtr, EHCI_qTD_Struct* firstQTD,
            bool H, byte deviceAddr,
            byte endpoint, ushort maxPacketSize)
        {
            EHCI_QueueHead head = new EHCI_QueueHead(headPtr);
            head.HorizontalLinkPointer = (EHCI_QueueHead_Struct*)GetPhysicalAddress(horizPtr);
            head.Type = 0x1; // Types:  00b iTD,   01b QH,   10b siTD,   11b FSTN
            head.Terminate = false;
            head.DeviceAddress = deviceAddr; // The device address
            head.InactiveOnNextTransaction = false;
            head.EndpointNumber = endpoint; // endpoint 0 contains Device infos such as name
            head.EndpointSpeed = 2; // 00b = full speed; 01b = low speed; 10b = high speed
            head.DataToggleControl = true; // get the Data Toggle bit out of the included qTD
            head.HeadOfReclamationList = H; // mark a queue head as being the head of the reclaim list
            head.MaximumPacketLength = maxPacketSize; // 64 byte for a control transfer to a high speed device
            head.ControlEndpointFlag = false; // only used if endpoint is a control endpoint and not high speed
            head.NakCountReload = 0;
            // this value is used by EHCI to reload the Nak Counter field. 0=ignores NAK counter.
            head.InterruptScheduleMask = 0; // not used for async schedule
            head.SplitCompletionMask = 0; // unused if (not low/full speed and in periodic schedule)
            head.HubAddr = 0; // unused if high speed (Split transfer)
            head.PortNumber = 0; // unused if high speed (Split transfer)
            head.HighBandwidthPipeMultiplier = 1; // 1-3 transaction per micro-frame, 0 means undefined results
            if (firstQTD == null)
            {
                head.NextqTDPointer = null;
                head.NextqTDPointerTerminate = true;
            }
            else
            {
                head.NextqTDPointer = (EHCI_qTD_Struct*)GetPhysicalAddress(firstQTD);
                head.NextqTDPointerTerminate = false;
            }
        }

        /// <summary>
        ///     Creates and initialises a new qTD as a SETUP qTD.
        /// </summary>
        /// <param name="next">A pointer to the next qTD in the linked list or 1 to specify no pointer.</param>
        /// <param name="toggle">The toggle state for the new qTD.</param>
        /// <param name="tokenBytes">The number of bytes to transfer.</param>
        /// <param name="type">The USB Request type.</param>
        /// <param name="req">The specific USB Request.</param>
        /// <param name="hiVal">The USB Request Hi-Val.</param>
        /// <param name="loVal">The USB Request Lo-Val.</param>
        /// <param name="index">The index of the USB Request.</param>
        /// <param name="length">The length of the USB Request.</param>
        /// <returns>The new qTD.</returns>
        protected EHCI_qTD CreateQTD_SETUP(EHCI_qTD_Struct* next, bool toggle, ushort tokenBytes, byte type, byte req,
            byte hiVal, byte loVal, ushort index, ushort length)
        {
            EHCI_qTD td = AllocAndInitQTD(next);

            td.PIDCode = (byte)EHCI_qTDTypes.SETUP; // SETUP = 2
            td.TotalBytesToTransfer = tokenBytes; // dependent on transfer
            td.DataToggle = toggle; // Should be toggled every list entry

            USBRequest* request = (USBRequest*)AllocQTDbuffer(td);
            request->type = type;
            request->request = req;
            request->valueHi = hiVal;
            request->valueLo = loVal;
            request->index = index;
            request->length = length;

            return td;
        }

        /// <summary>
        ///     Creates a new qTD and initialises it as an IN or OUT qTD.
        /// </summary>
        /// <param name="next">A pointer to the next qTD in the linked list or 1 to specify no pointer.</param>
        /// <param name="direction">The direction of the qTD (in or out)</param>
        /// <param name="toggle">The toggle state for the new qTD.</param>
        /// <param name="tokenBytes">The number of bytes to transfer.</param>
        /// <param name="bufferSize">The size of the qTD data buffer.</param>
        /// <returns>The new qTD.</returns>
        protected EHCI_qTD CreateQTD_IO(EHCI_qTD_Struct* next, byte direction, bool toggle, ushort tokenBytes,
            uint bufferSize)
        {
            EHCI_qTD td = AllocAndInitQTD(next);

            td.PIDCode = direction;
            td.TotalBytesToTransfer = tokenBytes; // dependent on transfer
            td.DataToggle = toggle; // Should be toggled every list entry

            AllocQTDbuffer(td);

            return td;
        }

        /// <summary>
        ///     Allocates memory for a new qTD and does initialisation common to all qTD types.
        /// </summary>
        /// <param name="next">A pointer to the next qTD in the linked list or 1 to specify no pointer.</param>
        /// <returns>The new qTD.</returns>
        protected static EHCI_qTD AllocAndInitQTD(EHCI_qTD_Struct* next)
        {
            EHCI_qTD newQTD = new EHCI_qTD();

            if (next != null)
            {
                newQTD.NextqTDPointerTerminate = false;
                newQTD.NextqTDPointer = (EHCI_qTD_Struct*)GetPhysicalAddress(next);
            }
            else
            {
                newQTD.NextqTDPointerTerminate = true;
            }

            newQTD.AlternateNextqTDPointerTerminate = true; // No alternate next, so T-Bit is set to 1
            newQTD.Status = 0x80; // This will be filled by the Host Controller. Active bit set
            newQTD.ErrorCounter = 0x0; // Written by the Host Controller.
            newQTD.CurrentPage = 0x0; // Written by the Host Controller.
            newQTD.InterruptOnComplete = false; //Set only for the last transaction of a transfer

            return newQTD;
        }

        /// <summary>
        ///     Allocates a correctly page-aligned buffer for use as a qTD data buffer. Sets it as Buffer0 of the qTD.
        /// </summary>
        /// <param name="td">The qTD to add the buffer to.</param>
        /// <returns>A pointer to the new buffer.</returns>
        protected static void* AllocQTDbuffer(EHCI_qTD td)
        {
            byte* result = (byte*)Heap.AllocZeroedAPB(0x1000u, 0x1000u, "EHCI : AllocQTDBuffer");
            td.Buffer0 = (byte*)GetPhysicalAddress(result);
            td.CurrentPage = 0;
            td.CurrentOffset = 0;
            td.Buffer0VirtAddr = result;
            return result;
        }

#if EHCI_TRACE
        internal static void DBGMSG(Framework.String msg)
        {
            BasicConsole.WriteLine(msg);
        }
#endif

        private static void* GetPhysicalAddress(void* vAddr)
        {
            return GetPhysicalAddress((uint)vAddr);
        }

        private static void* GetPhysicalAddress(uint vAddr)
        {
            uint address = 0xFFFFFFFF;
#if EHCI_TRACE
            DBGMSG("Getting physical address of: " + ((Framework.String)vAddr));
#endif
            SystemCallResults result = SystemCalls.GetPhysicalAddress(vAddr, out address);
            if (result != SystemCallResults.OK)
            {
#if EHCI_TRACE
                DBGMSG("Error! EHCI cannot get physical address.");
#endif
                ExceptionMethods.Throw(new Exception("EHCI cannot get physical address."));
            }
#if EHCI_TRACE
            else
            {
                DBGMSG("Physical address is: " + ((Framework.String)address));
            }
#endif
            return (void*)address;
        }

#if EHCI_TESTS
#region EHCI Method Tests

        //SetupTransfer
        //SETUPTransaction
        //INTransaction
        //OUTTransaction
        //IssueTransfer
        //CreateQTD_SETUP
        //CreateQTD_IO
        //Create_QH
        public void Test_Create_QH()
        {
            Framework.String testName = "Queue Transfer Descrip";
            EHCITesting.DBGMSG(testName, "START");

            EHCITesting.errors = 0;
            EHCITesting.warnings = 0;

            EHCI_QueueHead qh = new EHCI_QueueHead();
            EHCI_QueueHead_Struct* pQH = qh.queueHead;
            try
            {
                CreateQH(pQH, 0xDEADBEEFu, (EHCI_qTD_Struct*)0x12345678u, true, 0xFE, 0xED, 0x1234);

                //Confirm values (other tests check that these get/set properties work properly)

            }
            catch
            {
                EHCITesting.errors++;
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }
            finally
            {
                qh.Free();
            }

            if (EHCITesting.errors > 0)
            {
                EHCITesting.DBGERR(testName, ((Framework.String)"Test failed! Errors: ") + EHCITesting.errors + " Warnings: " + EHCITesting.warnings);
            }
            else
            {
                if (EHCITesting.warnings > 0)
                {
                    EHCITesting.DBGWRN(testName, ((Framework.String)"Test passed with warnings: ") + EHCITesting.warnings);
                }
                else
                {
                    EHCITesting.DBGMSG(testName, "Test passed.");
                }
            }

            EHCITesting.DBGMSG(testName, "END");

            BasicConsole.DelayOutput(4);
        }

#endregion
#endif
    }

    /// <summary>
    ///     Represents a transaction made through an EHCI.
    /// </summary>
    public unsafe class EHCITransaction : HCTransaction
    {
        /// <summary>
        ///     A pointer to the input buffer.
        /// </summary>
        public void* inBuffer;

        /// <summary>
        ///     The length of the input buffer.
        /// </summary>
        public uint inLength;

        /// <summary>
        ///     A pointer to the actual qTD of the transaction.
        /// </summary>
        public EHCI_qTD qTD;
    }

    /// <summary>
    ///     Represents a Queue Head structure's memory layout.
    ///     This structure can be passed to the HCI.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EHCI_QueueHead_Struct
    {
        /*
         * See section 3.6 of spec.
         */

        /// <summary>
        ///     UInt32 1 in the structure.
        /// </summary>
        public uint u1;

        /// <summary>
        ///     UInt32 2 in the structure.
        /// </summary>
        public uint u2;

        /// <summary>
        ///     UInt32 3 in the structure.
        /// </summary>
        public uint u3;

        /// <summary>
        ///     UInt32 4 in the structure.
        /// </summary>
        public uint u4;

        /// <summary>
        ///     UInt32 5 in the structure.
        /// </summary>
        public uint u5;

        /// <summary>
        ///     UInt32 6 in the structure.
        /// </summary>
        public uint u6;

        /// <summary>
        ///     UInt32 7 in the structure.
        /// </summary>
        public uint u7;

        /// <summary>
        ///     UInt32 8 in the structure.
        /// </summary>
        public uint u8;

        /// <summary>
        ///     UInt32 9 in the structure.
        /// </summary>
        public uint u9;

        /// <summary>
        ///     UInt32 10 in the structure.
        /// </summary>
        public uint u10;

        /// <summary>
        ///     UInt32 11 in the structure.
        /// </summary>
        public uint u11;

        /// <summary>
        ///     UInt32 12 in the structure.
        /// </summary>
        public uint u12;
    }

    /// <summary>
    ///     Represents a Queue Transfer Descriptor structure's memory layout.
    ///     This structure can be passed to the HCI.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EHCI_qTD_Struct
    {
        /*
         * See section 3.5 of spec.
         */

        /// <summary>
        ///     UInt32 1 in the structure.
        /// </summary>
        public uint u1;

        /// <summary>
        ///     UInt32 2 in the structure.
        /// </summary>
        public uint u2;

        /// <summary>
        ///     UInt32 3 in the structure.
        /// </summary>
        public uint u3;

        /// <summary>
        ///     UInt32 4 in the structure.
        /// </summary>
        public uint u4;

        /// <summary>
        ///     UInt32 5 in the structure.
        /// </summary>
        public uint u5;

        /// <summary>
        ///     UInt32 6 in the structure.
        /// </summary>
        public uint u6;

        /// <summary>
        ///     UInt32 7 in the structure.
        /// </summary>
        public uint u7;

        /// <summary>
        ///     UInt32 8 in the structure.
        /// </summary>
        public uint u8;
    }

    /// <summary>
    ///     Represents a qTD. The underlying memory structure can be passed to the HCI.
    ///     This class provides methods / properties for manipulating qTD values.
    /// </summary>
    public unsafe class EHCI_qTD : Object
    {
        public byte* Buffer0VirtAddr;
        /*
         * See section 3.5 of spec.
         */

        /// <summary>
        ///     The qTD data/memory structure that can be passed to the HCI.
        /// </summary>
        public EHCI_qTD_Struct* qtd;

        /// <summary>
        ///     Pointer to the next qTD in the linked list.
        /// </summary>
        public EHCI_qTD_Struct* NextqTDPointer
        {
            [NoGC] get { return (EHCI_qTD_Struct*)(qtd->u1 & 0xFFFFFFE0u); }
            [NoGC] set { qtd->u1 = (qtd->u1 & 0x0000001Fu) | ((uint)value & 0xFFFFFFE0u); }
        }

        /// <summary>
        ///     Whether the next qTD pointer indicates the end of the linked list.
        /// </summary>
        public bool NextqTDPointerTerminate
        {
            [NoGC] get { return (qtd->u1 & 0x00000001u) > 0; }
            [NoGC]
            set
            {
                if (value)
                {
                    qtd->u1 = qtd->u1 | 0x00000001u;
                }
                else
                {
                    qtd->u1 = qtd->u1 & 0xFFFFFFFEu;
                }
            }
        }

        /// <summary>
        ///     Whether the alternate next qTD pointer indicates the end of the linked list.
        /// </summary>
        public bool AlternateNextqTDPointerTerminate
        {
            [NoGC] get { return (qtd->u2 & 0x00000001u) > 0; }
            [NoGC]
            set
            {
                if (value)
                {
                    qtd->u2 = qtd->u2 | 0x00000001u;
                }
                else
                {
                    qtd->u2 = qtd->u2 & 0xFFFFFFFEu;
                }
            }
        }

        /// <summary>
        ///     The status.
        /// </summary>
        public byte Status
        {
            [NoGC] get { return (byte)qtd->u3; }
            [NoGC] set { qtd->u3 = (qtd->u3 & 0xFFFFFF00u) | value; }
        }

        /// <summary>
        ///     The PID code.
        /// </summary>
        public byte PIDCode
        {
            [NoGC] get { return (byte)((qtd->u3 & 0x000000300u) >> 8); }
            [NoGC] set { qtd->u3 = (qtd->u3 & 0xFFFFFCFFu) | ((uint)value << 8); }
        }

        /// <summary>
        ///     The error counter.
        /// </summary>
        public byte ErrorCounter
        {
            [NoGC] get { return (byte)((qtd->u3 & 0x00000C00u) >> 10); }
            [NoGC] set { qtd->u3 = (qtd->u3 & 0xFFFFF3FFu) | ((uint)value << 10); }
        }

        /// <summary>
        ///     The current page number.
        /// </summary>
        public byte CurrentPage
        {
            [NoGC] get { return (byte)((qtd->u3 & 0x00007000) >> 12); }
            [NoGC] set { qtd->u3 = (qtd->u3 & 0xFFFF8FFF) | ((uint)value << 12); }
        }

        /// <summary>
        ///     Whether to trigger an interrupt when the transfer is complete.
        /// </summary>
        public bool InterruptOnComplete
        {
            [NoGC] get { return (qtd->u3 & 0x00008000u) > 0; }
            [NoGC]
            set
            {
                if (value)
                {
                    qtd->u3 = qtd->u3 | 0x00008000u;
                }
                else
                {
                    qtd->u3 = qtd->u3 & 0xFFFF7FFFu;
                }
            }
        }

        /// <summary>
        ///     The total number of bytes to transfer.
        /// </summary>
        public ushort TotalBytesToTransfer
        {
            [NoGC] get { return (ushort)((qtd->u3 >> 16) & 0x00007FFF); }
            [NoGC] set { qtd->u3 = (qtd->u3 & 0x8000FFFF) | ((uint)value << 16); }
        }

        /// <summary>
        ///     The data toggle status.
        /// </summary>
        public bool DataToggle
        {
            [NoGC] get { return (qtd->u3 & 0x80000000u) > 0; }
            [NoGC]
            set
            {
                if (value)
                {
                    qtd->u3 = qtd->u3 | 0x80000000u;
                }
                else
                {
                    qtd->u3 = qtd->u3 & 0x7FFFFFFFu;
                }
            }
        }

        /// <summary>
        ///     Current offset in C_Page buffer.
        /// </summary>
        public ushort CurrentOffset
        {
            [NoGC] get { return (ushort)(qtd->u4 & 0x00000FFFu); }
            [NoGC] set { qtd->u4 = (qtd->u4 & 0xFFFFF000u) | (value & 0x00000FFFu); }
        }

        /// <summary>
        ///     Buffer 0 pointer.
        /// </summary>
        public byte* Buffer0
        {
            [NoGC] get { return (byte*)(qtd->u4 & 0xFFFFF000u); }
            [NoGC] set { qtd->u4 = (qtd->u4 & 0x00000FFFu) | ((uint)value & 0xFFFFF000u); }
        }

        /// <summary>
        ///     Buffer 1 pointer.
        /// </summary>
        public byte* Buffer1
        {
            [NoGC] get { return (byte*)(qtd->u5 & 0xFFFFF000); }
            [NoGC] set { qtd->u5 = (uint)value & 0xFFFFF000; }
        }

        /// <summary>
        ///     Buffer 2 pointer.
        /// </summary>
        public byte* Buffer2
        {
            [NoGC] get { return (byte*)(qtd->u6 & 0xFFFFF000); }
            [NoGC] set { qtd->u6 = (uint)value & 0xFFFFF000; }
        }

        /// <summary>
        ///     Buffer 3 pointer.
        /// </summary>
        public byte* Buffer3
        {
            [NoGC] get { return (byte*)(qtd->u7 & 0xFFFFF000); }
            [NoGC] set { qtd->u7 = (uint)value & 0xFFFFF000; }
        }

        /// <summary>
        ///     Buffer 4 pointer.
        /// </summary>
        public byte* Buffer4
        {
            [NoGC] get { return (byte*)(qtd->u8 & 0xFFFFF000); }
            [NoGC] set { qtd->u8 = (uint)value & 0xFFFFF000; }
        }

        /// <summary>
        ///     Initializes a new qTD with new data structure.
        /// </summary>
        public EHCI_qTD()
        {
            qtd = (EHCI_qTD_Struct*)Heap.AllocZeroedAPB((uint)sizeof(EHCI_qTD_Struct), 32, "EHCI : EHCI_qtd()");
        }

        /// <summary>
        ///     Initializes a qTD with specified underlying data structure.
        /// </summary>
        /// <param name="aqTD">The existing underlying data structure.</param>
        public EHCI_qTD(EHCI_qTD_Struct* aqTD)
        {
            qtd = aqTD;
        }

        /// <summary>
        ///     Frees the underlying memory structure.
        /// </summary>
        [NoGC]
        public void Free()
        {
            Heap.Free(qtd);
            qtd = null;
        }
    }

    /// <summary>
    ///     Represents a queue head. The underlying memory structure can be passed to the HCI.
    ///     This class provides methods / properties for manipulating queue head values.
    /// </summary>
    public unsafe class EHCI_QueueHead : Object
    {
        /*
         * See section 3.6 of spec.
         */

        /// <summary>
        ///     The queue head data/memory structure that can be passed to the HCI.
        /// </summary>
        public EHCI_QueueHead_Struct* queueHead;

        /// <summary>
        ///     Whether the horizontal link pointer terminates (is valid) or not.
        /// </summary>
        public bool Terminate
        {
            [NoGC] get { return (queueHead->u1 & 0x00000001u) != 0; }
            [NoGC]
            set
            {
                if (value)
                {
                    queueHead->u1 = queueHead->u1 | 0x00000001u;
                }
                else
                {
                    queueHead->u1 = queueHead->u1 & 0xFFFFFFFEu;
                }
            }
        }

        /// <summary>
        ///     Queue head type.
        /// </summary>
        public byte Type
        {
            [NoGC] get { return (byte)((queueHead->u1 >> 1) & 0x00000003u); }
            [NoGC] set { queueHead->u1 = (queueHead->u1 & 0xFFFFFFF9u) | (value & 0x00000003u) << 1; }
        }

        /// <summary>
        ///     Horizontal link pointer - points to the next queue head in the list.
        /// </summary>
        public EHCI_QueueHead_Struct* HorizontalLinkPointer
        {
            [NoGC] get { return (EHCI_QueueHead_Struct*)(queueHead->u1 & 0xFFFFFFE0u); }
            [NoGC] set { queueHead->u1 = ((uint)value & 0xFFFFFFE0u) | (queueHead->u1 & 0x0000001Fu); }
        }

        /// <summary>
        ///     Target USB device address.
        /// </summary>
        public byte DeviceAddress
        {
            [NoGC] get { return (byte)(queueHead->u2 & 0x0000007Fu); }
            [NoGC] set { queueHead->u2 = (queueHead->u2 & 0xFFFFFF80u) | (value & 0x0000007Fu); }
        }

        /// <summary>
        ///     Inactive on next transaction.
        /// </summary>
        public bool InactiveOnNextTransaction
        {
            [NoGC] get { return (queueHead->u2 & 0x00000080u) > 0; }
            [NoGC]
            set
            {
                if (value)
                {
                    queueHead->u2 = queueHead->u2 | 0x00000080u;
                }
                else
                {
                    queueHead->u2 = queueHead->u2 & 0xFFFFFF7Fu;
                }
            }
        }

        /// <summary>
        ///     Target USB endpoint number.
        /// </summary>
        public byte EndpointNumber
        {
            [NoGC] get { return (byte)((queueHead->u2 & 0x00000F00u) >> 8); }
            [NoGC] set { queueHead->u2 = (queueHead->u2 & 0xFFFFF0FFu) | ((uint)value << 8); }
        }

        /// <summary>
        ///     Target USB endpoint speed.
        /// </summary>
        public byte EndpointSpeed
        {
            [NoGC] get { return (byte)((queueHead->u2 & 0x00003000u) >> 12); }
            [NoGC] set { queueHead->u2 = (queueHead->u2 & 0xFFFFCFFFu) | ((uint)value << 12); }
        }

        /// <summary>
        ///     Data toggle control.
        /// </summary>
        public bool DataToggleControl
        {
            [NoGC] get { return (queueHead->u2 & 0x00004000u) > 0; }
            [NoGC]
            set
            {
                if (value)
                {
                    queueHead->u2 = queueHead->u2 | 0x00004000u;
                }
                else
                {
                    queueHead->u2 = queueHead->u2 & 0xFFFFBFFFu;
                }
            }
        }

        /// <summary>
        ///     Whether this queue head is the first in the reclamation list.
        /// </summary>
        public bool HeadOfReclamationList
        {
            [NoGC] get { return (queueHead->u2 & 0x00008000u) > 0; }
            [NoGC]
            set
            {
                if (value)
                {
                    queueHead->u2 = queueHead->u2 | 0x00008000u;
                }
                else
                {
                    queueHead->u2 = queueHead->u2 & 0xFFFF7FFFu;
                }
            }
        }

        /// <summary>
        ///     Target endpoint's maximum packet length.
        /// </summary>
        public ushort MaximumPacketLength
        {
            [NoGC] get { return (ushort)((queueHead->u2 & 0x07FF0000u) >> 16); }
            [NoGC] set { queueHead->u2 = (queueHead->u2 & 0xF800FFFFu) | (((uint)value << 16) & 0x07FF0000u); }
        }

        /// <summary>
        ///     Control endpoint flag.
        /// </summary>
        public bool ControlEndpointFlag
        {
            [NoGC] get { return (queueHead->u2 & 0x08000000u) > 0; }
            [NoGC]
            set
            {
                if (value)
                {
                    queueHead->u2 = queueHead->u2 | 0x08000000u;
                }
                else
                {
                    queueHead->u2 = queueHead->u2 & 0xF7FFFFFFu;
                }
            }
        }

        /// <summary>
        ///     Nak count reload number.
        /// </summary>
        public byte NakCountReload
        {
            [NoGC] get { return (byte)((queueHead->u2 & 0xF0000000u) >> 28); }
            [NoGC] set { queueHead->u2 = (queueHead->u2 & 0x0FFFFFFFu) | ((uint)value << 28); }
        }

        /// <summary>
        ///     Interrupt schedule mask.
        /// </summary>
        public byte InterruptScheduleMask
        {
            [NoGC] get { return (byte)queueHead->u3; }
            [NoGC] set { queueHead->u3 = (queueHead->u3 & 0xFFFFFF00u) | value; }
        }

        /// <summary>
        ///     Split completion mask.
        /// </summary>
        public byte SplitCompletionMask
        {
            [NoGC] get { return (byte)(queueHead->u3 >> 8); }
            [NoGC] set { queueHead->u3 = (queueHead->u3 & 0xFFFF00FFu) | ((uint)value << 8); }
        }

        /// <summary>
        ///     Hub address.
        /// </summary>
        public byte HubAddr
        {
            [NoGC] get { return (byte)((queueHead->u3 & 0x007F0000u) >> 16); }
            [NoGC] set { queueHead->u3 = (queueHead->u3 & 0xFF80FFFFu) | ((uint)value << 16); }
        }

        /// <summary>
        ///     Port number.
        /// </summary>
        public byte PortNumber
        {
            [NoGC] get { return (byte)((queueHead->u3 & 0x3f800000u) >> 23); }
            [NoGC] set { queueHead->u3 = (queueHead->u3 & 0xC07FFFFFu) | ((uint)value << 23); }
        }

        /// <summary>
        ///     High bandwidth pipe multiplier.
        /// </summary>
        public byte HighBandwidthPipeMultiplier
        {
            [NoGC] get { return (byte)((queueHead->u3 & 0xC0000000u) >> 30); }
            [NoGC] set { queueHead->u3 = (queueHead->u3 & 0x3FFFFFFFu) | ((uint)value << 30); }
        }

        /// <summary>
        ///     Current qTD pointer.
        /// </summary>
        public EHCI_qTD_Struct* CurrentqTDPointer
        {
            [NoGC] get { return (EHCI_qTD_Struct*)(queueHead->u4 & 0xFFFFFFF0u); }
            [NoGC] set { queueHead->u4 = (queueHead->u4 & 0x0000000Fu) | ((uint)value & 0xFFFFFFF0u); }
        }

        /// <summary>
        ///     Next qTD pointer.
        /// </summary>
        public EHCI_qTD_Struct* NextqTDPointer
        {
            [NoGC] get { return (EHCI_qTD_Struct*)(queueHead->u5 & 0xFFFFFFF0u); }
            [NoGC] set { queueHead->u5 = (queueHead->u5 & 0x0000000Fu) | ((uint)value & 0xFFFFFFF0u); }
        }

        /// <summary>
        ///     Whether the next qTD pointer indicates end of the qTD list or not.
        /// </summary>
        public bool NextqTDPointerTerminate
        {
            [NoGC] get { return (queueHead->u5 & 0x00000001u) > 0; }
            [NoGC]
            set
            {
                if (value)
                {
                    queueHead->u5 = queueHead->u5 | 0x00000001u;
                }
                else
                {
                    queueHead->u5 = queueHead->u5 & 0xFFFFFFFEu;
                }
            }
        }

        /// <summary>
        ///     Whether the queue head is active or not.
        /// </summary>
        public bool Active
        {
            [NoGC] get { return (queueHead->u7 & 0x00000080u) > 0; }
            [NoGC]
            set
            {
                if (value)
                {
                    queueHead->u7 = queueHead->u7 | 0x00000080u;
                }
                else
                {
                    queueHead->u7 = queueHead->u7 & 0xFFFFFF7Fu;
                }
            }
        }

        /// <summary>
        ///     Initializes a new queue head with empty underlying memory structure.
        /// </summary>
        public EHCI_QueueHead()
        {
            queueHead =
                (EHCI_QueueHead_Struct*)
                    Heap.AllocZeroedAPB((uint)sizeof(EHCI_QueueHead_Struct), 32, "EHCI : EHCI_QueueHead()");
        }

        /// <summary>
        ///     Initializes a new queue head with specified underlying memory structure.
        /// </summary>
        /// <param name="aQueueHead">The existing underlying queue head.</param>
        public EHCI_QueueHead(EHCI_QueueHead_Struct* aQueueHead)
        {
            queueHead = aQueueHead;
        }

        /// <summary>
        ///     Frees the underlying memory structure.
        /// </summary>
        [NoGC]
        public void Free()
        {
            Heap.Free(queueHead);
            queueHead = null;
        }
    }
}