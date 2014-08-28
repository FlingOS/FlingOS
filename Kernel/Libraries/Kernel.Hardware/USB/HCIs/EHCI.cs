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
    
#define EHCI_TRACE
#undef EHCI_TRACE

#if EHCI_TRACE
    #define EHCI_TESTS
    #undef EHCI_TESTS //Note: Also comment out the undef in EHCITesting.cs
#endif

using System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware.USB.Devices;
using Utils = Kernel.Utilities.ConstantsUtils;
using Kernel.Utilities;

namespace Kernel.Hardware.USB.HCIs
{
    #region Constants
    /// <summary>
    /// Interrupt threshold control values - determines the minimum time between interrupt being fired.
    /// </summary>
    public enum EHCI_InterruptThresholdControls : uint
    {
        /// <summary>
        /// 1 Microframes (= 0.125ms)
        /// </summary>
        x01 = 0x010000,
        /// <summary>
        /// 2 Microframes (= 0.25ms)
        /// </summary>
        x02 = 0x020000,
        /// <summary>
        /// 4 Microframes (= 0.5ms)
        /// </summary>
        x04 =  0x040000,
        /// <summary>
        /// 8 Microframes (= 1ms)
        /// </summary>
        x08 = 0x080000,
        /// <summary>
        /// 16 Microframes (= 2ms)
        /// </summary>
        x16 = 0x100000,
        /// <summary>
        /// 32 Microframes (= 4ms)
        /// </summary>
        x32 = 0x200000,
        /// <summary>
        /// 64 Microframes (= 8ms)
        /// </summary>
        x64 = 0x400000
    }
    /// <summary>
    /// Frame list size values.
    /// </summary>
    public enum EHCI_FrameListSizes : uint
    {
        /// <summary>
        /// List size of 1024.
        /// </summary>
        x1024 = 0x0,
        /// <summary>
        /// List size of 512.
        /// </summary>
        x0512 = 0x4,
        /// <summary>
        /// List size of 256.
        /// </summary>
        x0256 = 0x8
    }
    /// <summary>
    /// Constants used by the EHCI driver.
    /// </summary>
    public class EHCI_Consts
    {
        /* ------ USBCMD ------ */

        /// <summary>
        /// Mask for the Interrupt Threshold setting in the CMD operational register.
        /// </summary>
        /// <see cref="EHCI_InterruptThresholdControls"/>
        public static uint CMD_InterruptThresholdMask = 0x00FF0000;
        /// <summary>
        /// Mask for the Frame List Size setting in the CMD operational register.
        /// </summary>
        /// <see cref="EHCI_FrameListSizes"/>
        public static uint CMD_FrameListSizeMask = 0xC;
                
        /// <summary>
        /// Mask for the Park Mode setting in the CMD operational register.
        /// </summary>
        public static uint CMD_ParkModeMask = 0x800;
        /// <summary>
        /// Mask for the Park Count setting in the CMD operational register.
        /// </summary>
        public static uint CMD_ParkCountMask = 0x300;
        /// <summary>
        /// Mask for the Light Reset command in the CMD operational register.
        /// </summary>
        public static uint CMD_LightResetMask = Utils.BIT(7);
        /// <summary>
        /// Mask for the Async Interrupt Doorbell setting in the CMD operational register.
        /// </summary>
        public static uint CMD_AsyncInterruptDoorbellMask = Utils.BIT(6);
        /// <summary>
        /// Mask for the Async Schedule Enable command in the CMD operational register.
        /// </summary>
        public static uint CMD_AsyncScheduleEnableMask = Utils.BIT(5);
        /// <summary>
        /// Mask for the Periodic Schedule Enable command in the CMD operational register.
        /// </summary>
        public static uint CMD_PeriodicScheduleEnableMask = Utils.BIT(4);

        /// <summary>
        /// Mask for the Host Controller Reset command in the CMD operational register.
        /// </summary>
        public static uint CMD_HCResetMask = Utils.BIT(1);// reset
        /// <summary>
        /// Mask for the Run-Stop command bit in the CMD operational register.
        /// </summary>
        public static uint CMD_RunStopMask = Utils.BIT(0);// run/stop


        /* ------ USBSTS / USBINTR ------ */

        /* Only USBSTS */
        /// <summary>
        /// Mask for the Async Schedule Enabled flag.
        /// </summary>
        public static uint STS_AsyncEnabled = Utils.BIT(15);
        /// <summary>
        /// Mask for the Periodic Schedule Enabled flag.
        /// </summary>
        public static uint STS_PeriodicEnabled = Utils.BIT(14);
        /// <summary>
        /// Mask for the Reclamation flag.
        /// </summary>
        public static uint STS_ReclamationFlag = Utils.BIT(13);
        /// <summary>
        /// Mask for the Host Controller Halted flag.
        /// </summary>
        public static uint STS_HCHalted = Utils.BIT(12);

        /* USBSTS / USBINTR */
        /// <summary>
        /// Mask for the interrupt type flag indicating Async Interrupt occurred.
        /// </summary>
        public static uint STS_AsyncInterrupt = Utils.BIT(5);
        /// <summary>
        /// Mask for the interrupt type flag indicating a Host System Error occurred.
        /// </summary>
        public static uint STS_HostSystemError = Utils.BIT(4);
        /// <summary>
        /// Mask for the interrupt type flag indicating a Frame List Rollover occurred.
        /// </summary>
        public static uint STS_FrameListRollover = Utils.BIT(3);
        /// <summary>
        /// Mask for the interrupt type flag indicating a Port Change occurred.
        /// </summary>
        public static uint STS_PortChange = Utils.BIT(2);
        /// <summary>
        /// Mask for the interrupt type flag indicating a USB Error occurred.
        /// </summary>
        public static uint STS_USBErrorInterrupt = Utils.BIT(1);
        /// <summary>
        /// Mask for the interrupt type flag indicating a general USB Interrupt occurred.
        /// </summary>
        public static uint STS_USBInterrupt = Utils.BIT(0);


        /* ------ FRINDEX ------ */
        /// <summary>
        /// Frame index register mask.
        /// </summary>
        public static uint FRI_Mask = 0x00001FFF;


        /* ------ PERIODICLISTBASE ------ */
        /// <summary>
        /// Periodic list base alignment mask. 4KiB alignment.
        /// </summary>
        public static uint PLB_Alignment = 0x00000FFF; // 4 KiB


        /* ------ ASYNCLISTADDR ------ */
        /// <summary>
        /// Async list address alignment mask. 32 byte alignment.
        /// </summary>
        public static uint ALB_Alignment = 0x0000001F;  // 32 Byte


        /* ------ CONFIGFLAG ------ */
        /// <summary>
        /// Config flag mask.
        /// </summary>
        public static uint CF = Utils.BIT(0);


        /* ------ PORTSC[0-n] ------ */
        /// <summary>
        /// R/W. Port status mask to get/set whether the port is owned by the companion host controller.
        /// </summary>
        public static uint PSTS_CompanionHCOwned = Utils.BIT(13);
        /// <summary>
        /// R/W. Port status mask to power the port on/off. Valid if PPC == 1.
        /// </summary>
        public static uint PSTS_PowerOn = Utils.BIT(12);
        /// <summary>
        /// R/W. Port status mask to tell the port to reset.
        /// </summary>
        public static uint PSTS_PortReset = Utils.BIT(8);
        /// <summary>
        /// R/W. Port status mask to tell the port to suspend.
        /// </summary>
        public static uint PSTS_PortSuspend = Utils.BIT(7);
        /// <summary>
        /// R/W. Port status mask to tell the port to suspend.
        /// </summary>
        public static uint PSTS_PortResume = Utils.BIT(6);
        /// <summary>
        /// R/WC. Port status mask to read or clear the port overrcurrent changed status.
        /// </summary>
        public static uint PSTS_OverCurrentChange = Utils.BIT(5);
        /// <summary>
        /// R. Port status mask to read whether the port has gone into overcurrent or not.
        /// </summary>
        public static uint PSTS_OverCurrent = Utils.BIT(4);
        /// <summary>
        /// R/WC. Port status mask to read or clear the port changed status.
        /// </summary>
        public static uint PSTS_EnabledChange = Utils.BIT(3);
        /// <summary>
        /// R/W. Port status mask to get/set whether the port is enabled or not.
        /// </summary>
        public static uint PSTS_Enabled = Utils.BIT(2);
        /// <summary>
        /// R/WC. Port status mask to read or clear the port connected changed status.
        /// </summary>
        public static uint PSTS_ConnectedChange = Utils.BIT(1);
        /// <summary>
        /// R. Port status mask to read whether the port is connected to a device or not.
        /// </summary>
        public static uint PSTS_Connected = Utils.BIT(0);

        /// <summary>
        /// Mask for the number of ports.
        /// </summary>
        public static uint NumPorts = 0xF;// number of ports (Bits 3:0 set)
        /// <summary>
        /// Mask for the overall port routing bit.
        /// </summary>
        public static uint PortRoutingMask = Utils.BIT(7);// port routing to EHCI or cHC
        /// <summary>
        /// Constant to set the number of times that software should re-attempt to send a transfer 
        /// in the async schedule.
        /// </summary>
        public static uint NumAsyncListRetries = 3;
    }
    /// <summary>
    /// The types of queue head (under EHCI): IN, OUT or SETUP.
    /// </summary>
    public enum EHCI_qTDTypes : byte
    {
        /// <summary>
        /// Indicates an OUT transaction where data is sent from the controller to the device.
        /// </summary>
        OUT = 0,
        /// <summary>
        /// Indicates an IN transaction where data is sent from the device to the controller.
        /// </summary>
        IN = 1,
        /// <summary>
        /// Indicates a SETUP transaction.
        /// </summary>
        SETUP = 2
    }
    #endregion

    /// <summary>
    /// Represents a USB Extended Host Controller Interface
    /// </summary>
    public unsafe class EHCI : HCI
    {
        //TODO - Perhaps should avoid using identity mapping for registers etc? 

        /*
         * Based on the Intel EHCI Specification for USB 2.0
         *  http://www.intel.co.uk/content/dam/www/public/us/en/documents/technical-specifications/ehci-specification-for-usb.pdf
         */

        /// <summary>
        /// The base address of the USB HCI device in memory.
        /// </summary>
        protected byte* usbBaseAddress;

        #region PCI Registers
        
        /*
         * See section 2.1 of spec.
         */

        /// <summary>
        /// SBRN PCI memory-mapped register.
        /// </summary>
        protected byte SBRN;
        /// <summary>
        /// FLADJ PCI memory-mapped register.
        /// </summary>
        protected byte FLADJ
        {
            get
            {
                return pciDevice.ReadRegister8(0x61);
            }
            set
            {
                pciDevice.WriteRegister8(0x61, value);
            }
        }
        /// <summary>
        /// Port wakeup capability PCI memory-mapped register.
        /// </summary>
        protected byte PortWakeCap
        {
            get
            {
                return pciDevice.ReadRegister8(0x62);
            }
        }
        
        #endregion

        #region Capability Registers
        
        /*
         *  See section 2.2 of spec.
         */

        /// <summary>
        /// The base address of the capabilities regsiters.
        /// </summary>
        protected byte* CapabilitiesRegAddr;
        /// <summary>
        /// The length of the capabilities registers. Used to calculate 
        /// offset to operational registers.
        /// </summary>
        protected byte CapabilitiesRegsLength;
        /// <summary>
        /// HCI Version number.
        /// </summary>
        protected UInt16 HCIVersion;
        /// <summary>
        /// HCS params
        /// </summary>
        protected uint HCSParams;
        /// <summary>
        /// HCC params
        /// </summary>
        protected uint HCCParams;
        /// <summary>
        /// HCSP port route desc
        /// </summary>
        protected UInt64 HCSPPortRouteDesc;

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
        /// EECP from HCC params
        /// </summary>
        protected byte EECP
        {
            get
            {
                return (byte)(HCCParams >> 8);
            }
        }

        #endregion

        #endregion

        #region Operation Registers

        /*
         * See section 2.3 of spec.
         */

        /// <summary>
        /// Base address of the operational registers.
        /// </summary>
        protected uint* OpRegAddr;

        #region Core Well

        /// <summary>
        /// USB command operational memory-mapped register.
        /// </summary>
        protected uint USBCMD
        {
            get
            {
                return *OpRegAddr;
            }
            set
            {
                *OpRegAddr = value;
            }
        }
        /// <summary>
        /// USB status operational memory-mapped register.
        /// </summary>
        protected uint USBSTS
        {
            get
            {
                return *(OpRegAddr + 1);
            }
            set
            {
                *(OpRegAddr + 1) = value;
            }
        }
        /// <summary>
        /// USB interrupts operational memory-mapped register.
        /// </summary>
        protected uint USBINTR
        {
            get
            {
                return (*(OpRegAddr + 2));
            }
            set
            {
                *(OpRegAddr + 2) = (uint)value;
            }
        }
        /// <summary>
        /// USB frame index operational memory-mapped register.
        /// </summary>
        protected uint FRINDEX
        {
            get
            {
                return *(OpRegAddr + 3);
            }
            set
            {
                *(OpRegAddr + 3) = value;
            }
        }
        /// <summary>
        /// USB control DS segment operational memory-mapped register.
        /// </summary>
        protected uint CTRLDSSEGMENT
        {
            get
            {
                return *(OpRegAddr + 4);
            }
            set
            {
                *(OpRegAddr + 4) = value;
            }
        }
        /// <summary>
        /// USB periodic list base operational memory-mapped register.
        /// </summary>
        protected uint PERIODICLISTBASE
        {
            get
            {
                return *(OpRegAddr + 5);
            }
            set
            {
                *(OpRegAddr + 5) = value;
            }
        }
        /// <summary>
        /// USB async list address operational memory-mapped register.
        /// </summary>
        protected EHCI_QueueHead_Struct* ASYNCLISTADDR
        {
            get
            {
                return (EHCI_QueueHead_Struct*)*(OpRegAddr + 6);
            }
            set
            {
                *(OpRegAddr + 6) = (uint)value;
            }
        }

        /// <summary>
        /// Whether the host controller has been halted or not.
        /// </summary>
        protected bool HCHalted
        {
            get
            {
                return (USBSTS & EHCI_Consts.STS_HCHalted) != 0;
            }
        }

        /// <summary>
        /// Whether the asynchronous schedule is enabled or not.
        /// </summary>
        protected bool AsynchronousScheduleEnabled
        {
            /*
             * See section 2.3.1 of spec.
             */

            get
            {
                return (USBCMD & 0x20) > 0;
            }
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
        /// Whether the HCI thinks the asynchronous schedule is enabled or not.
        /// </summary>
        protected bool AsynchronousScheduleStatus
        {
            get
            {
                return (USBSTS & 0x80) > 0;
            }
        }

        /// <summary>
        /// Used as a doorbell by software to tell the host controller to issue an interrupt the next time it advances 
        /// asynchronous schedule. Used when a queue head is removed from the async queue.
        /// </summary>
        protected bool InterruptOnAsyncAdvanceDoorbell
        {
            /*
             * See sections 2.3.1 and 4.8.2 of spec.
             */

            get
            {
                return (USBCMD & 0x40) > 0;
            }
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
        /// Indicates the interrupt has/would have occurred.
        /// </summary>
        protected bool InterruptOnAsyncAdvance
        {
            /*
             * See sections 2.3.1 and 4.8.2 of spec.
             */

            get
            {
                return (USBSTS & 0x20) > 0;
            }
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
        /// USB configuration flags operational memory-mapped register.
        /// </summary>
        protected uint CONFIGFLAG
        {
            get
            {
                return *(OpRegAddr + 16);
            }
            set
            {
                *(OpRegAddr + 16) = value;
            }
        }
        /// <summary>
        /// USB port SC operational memory-mapped register.
        /// </summary>
        protected uint* PORTSC
        {
            get
            {
                return (uint*)(OpRegAddr + 17);
            }
            set
            {
                *(OpRegAddr + 17) = (uint)value;
            }
        }
        
        #endregion

        #endregion

        /// <summary>
        /// Whether any ports have changed since the last port check.
        /// </summary>
        protected bool AnyPortsChanged = false;
        /// <summary>
        /// Whether the ports have been enabled or not.
        /// </summary>
        protected bool EnabledPortsFlag = false;
        /// <summary>
        /// A countdown of the number of async transaction complete interrupts that have occurred since the last
        /// reload. Used for detecting the end of an async transfer (queue head completetion).
        /// </summary>
        protected int USBIntCount = 0;
        /// <summary>
        /// Pointer to the idle queue head. Required by the spec and this should always remain as the head of the 
        /// async queue while the async queue is enabled.
        /// </summary>
        protected EHCI_QueueHead_Struct* IdleQueueHead = null;
        /// <summary>
        /// Pointer to the tail queue head - the queue head at the end of the linked list. Optimisation - this allows 
        /// us to append to the async queue without having to traverse the whole list first. This should be set to
        /// the idle queue head pointer when the list is "empty".
        /// </summary>
        protected EHCI_QueueHead_Struct* TailQueueHead = null;

        /// <summary>
        /// Whether the EHCI has hit a host system error or not.
        /// </summary>
        public bool HostSystemError = false;
        /// <summary>
        /// Whether the EHCI has hit an unrecoverable error or not.
        /// </summary>
        public bool IrrecoverableError = false;

        /// <summary>
        /// Initialises a new EHCI device using the specified PCI device. Includes starting the host controller.
        /// </summary>
        /// <param name="aPCIDevice">The PCI device that represents the physical EHCI device.</param>
        public EHCI(PCI.PCIDeviceNormal aPCIDevice)
            : base(aPCIDevice)
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

            usbBaseAddress = (byte*)((uint)pciDevice.BaseAddresses[0].BaseAddress() & 0xFFFFFF00);

            //Map in the required memory - we will use identity mapping for the PCI / USB registers for now
            VirtMemManager.Map((uint)usbBaseAddress & 0xFFFFF000, (uint)usbBaseAddress & 0xFFFFF000, 4096);

            CapabilitiesRegAddr = usbBaseAddress;
#if EHCI_TRACE
            DBGMSG("CapabilitiesRegAddr: " + (FOS_System.String)(uint)CapabilitiesRegAddr);
#endif
            SBRN = pciDevice.ReadRegister8(0x60);
#if EHCI_TRACE
            DBGMSG("SBRN: " + (FOS_System.String)SBRN);
#endif

            CapabilitiesRegsLength = *CapabilitiesRegAddr;
            HCIVersion = *((UInt16*)(CapabilitiesRegAddr + 2));
            HCSParams = *((UInt32*)(CapabilitiesRegAddr + 4));
            HCCParams = *((UInt32*)(CapabilitiesRegAddr + 8));
            HCSPPortRouteDesc = *((UInt64*)(CapabilitiesRegAddr + 12));
            
            OpRegAddr = (uint*)(usbBaseAddress + CapabilitiesRegsLength);
            
#if EHCI_TRACE
            DBGMSG("CapabilitiesRegsLength: " + (FOS_System.String)CapabilitiesRegsLength);
            DBGMSG("HCIVersion: " + (FOS_System.String)HCIVersion);
            DBGMSG("HCSParams: " + (FOS_System.String)HCSParams);
            DBGMSG("HCCParams: " + (FOS_System.String)HCCParams);
            DBGMSG("HCSPPortRouteDesc: " + (FOS_System.String)HCSPPortRouteDesc);
            DBGMSG("OpRegAddr: " + (FOS_System.String)(uint)OpRegAddr);
#endif

            RootPortCount = (byte)(HCSParams & 0x000F);

            Start();
        }

        /// <summary>
        /// Updates the host controller (runs a port check if any ports have changed since the last port check).
        /// </summary>
        public override void Update()
        {
            if(IrrecoverableError)
            {
#if EHCI_TRACE
                BasicConsole.WriteLine("EHCI controller has hit an irrecoverable error!");
                BasicConsole.DelayOutput(10);
#endif
            }
            else if (AnyPortsChanged)
            {
                PortCheck();
            }
        }

        /// <summary>
        /// Starts the host controller including all necessary initialisation, port resets and port enabling. 
        /// Also detects any devices already connected to the controller.
        /// </summary>
        protected void Start()
        {
            InitHC();
            ResetHC();
            StartHC();
            InitializeAsyncSchedule();

            if (!HCHalted)
            {
                EnablePorts();
#if EHCI_TRACE
                DBGMSG("USB ports enabled.");
#endif
            }
            else
            {
                ExceptionMethods.Throw(new FOS_System.Exception("EHCI.Start(): Host controller halted! Cannot start EHCI driver!"));
            }
        }
        /// <summary>
        /// Initialises the host controller.
        /// </summary>
        protected void InitHC()
        {
            pciDevice.Command = pciDevice.Command | PCI.PCIDevice.PCICommand.Memory | PCI.PCIDevice.PCICommand.Master;

#if EHCI_TRACE
            DBGMSG("Hooking IRQ...");
#endif
            //Setup InterruptHandler (IRQ number = PCIDevice.InterruptLine)
            Interrupts.Interrupts.AddIRQHandler(pciDevice.InterruptLine, EHCI.InterruptHandler, this);
#if EHCI_TRACE
            DBGMSG("Hooked IRQ.");
#endif
        }
        /// <summary>
        /// Starts the host controller.
        /// </summary>
        protected void StartHC()
        {
            DeactivateLegacySupport();
            CTRLDSSEGMENT = 0u;
            USBSTS = 0u; //Will this ever have any effect? According to spec, only writing bits set to 1 will have an effect??
            USBINTR = EHCI_Consts.STS_AsyncInterrupt | EHCI_Consts.STS_HostSystemError | EHCI_Consts.STS_PortChange | 
                      EHCI_Consts.STS_USBInterrupt | EHCI_Consts.STS_USBErrorInterrupt;
            if (HCHalted)
            {
                USBCMD |= EHCI_Consts.CMD_RunStopMask; //Set run-stop bit
            }

            //This can only be set when HCHalted != 0  !!!
            USBCMD |= (uint)EHCI_InterruptThresholdControls.x08; //InterruptThresholdControl = 8 Microframes (1ms).
            
            CONFIGFLAG = EHCI_Consts.CF; //Set port routing to route all ports to EHCI

            //Is this delay necessary? If so, why?
            Hardware.Devices.Timer.Default.Wait(100);
        }
        /// <summary>
        /// Resets the host controller and consequently all ports.
        /// </summary>
        protected void ResetHC()
        {
            USBCMD &= ~EHCI_Consts.CMD_RunStopMask; //Clear run-stop bit

            //Wait for halt
            while (!HCHalted)
            {
                //Sleep for a bit
                Hardware.Devices.Timer.Default.Wait(10);
            }

            USBCMD |= EHCI_Consts.CMD_HCResetMask; //Set reset bit

            int timeout = 30;
            while ((USBCMD & EHCI_Consts.CMD_HCResetMask) != 0) // Reset-bit still set to 1
            {
                Hardware.Devices.Timer.Default.Wait(10);

                timeout--;
                if (timeout==0)
                {
                    //ExceptionMethods.Throw(new FOS_System.Exception("EHCI.Reset(): Timeout! USBCMD Reset bit not cleared!"));
#if EHCI_TRACE
                    DBGMSG("EHCI.Reset(): Timeout! USBCMD Reset bit not cleared!");
#endif
                    break;
                }
            }
        }
        /// <summary>
        /// Deactivates legacy support mode if it is available.
        /// </summary>
        protected void DeactivateLegacySupport()
        {
            byte eecp = EECP;

#if EHCI_TRACE
            DBGMSG(((FOS_System.String)"DeactivateLegacySupport: eecp = ") + eecp);
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
                    DBGMSG(((FOS_System.String)"eecp = ") + eecp);
#endif
                    eecp_id = pciDevice.ReadRegister8(eecp);
#if EHCI_TRACE
                    DBGMSG(((FOS_System.String)"eecp_id = ") + eecp_id);
#endif
                    if (eecp_id == 1)
                    {
                        break;
                    }
                    eecp = pciDevice.ReadRegister8((byte)(eecp + 1));
                }
                byte BIOSownedSemaphore = (byte)(eecp + 2); // R/W - only Bit 16 (Bit 23:17 Reserved, must be set to zero)
                byte OSownedSemaphore   = (byte)(eecp + 3); // R/W - only Bit 24 (Bit 31:25 Reserved, must be set to zero)
                byte USBLEGCTLSTS       = (byte)(eecp + 4); // USB Legacy Support Control/Status (DWORD, cf. EHCI 1.0 spec, 2.1.8)

                // Legacy-Support-EC found? BIOS-Semaphore set?
                if (eecp_id == 1 && (pciDevice.ReadRegister8(BIOSownedSemaphore) & 0x01) != 0)
                {
#if EHCI_TRACE
                    DBGMSG("set OS-Semaphore.");
#endif
                    pciDevice.WriteRegister8(OSownedSemaphore, 0x01);

                    int timeout = 250;
                    // Wait for BIOS-Semaphore being not set
                    while ((pciDevice.ReadRegister8(BIOSownedSemaphore) & 0x01) != 0 && (timeout > 0))
                    {
                        timeout--;
                        Hardware.Devices.Timer.Default.Wait(10);
                    }
                    if ((pciDevice.ReadRegister8(BIOSownedSemaphore) & 0x01) == 0) // not set
                    {
#if EHCI_TRACE
                        DBGMSG("BIOS-Semaphore being cleared.");
#endif
                        timeout = 250;
                        while ((pciDevice.ReadRegister8(OSownedSemaphore) & 0x01) == 0 && (timeout > 0))
                        {
                            timeout--;
                            Hardware.Devices.Timer.Default.Wait(10);
                        }
                    }
#if EHCI_TRACE
                    if ((pciDevice.ReadRegister8(OSownedSemaphore) & 0x01) != 0)
                    {
                        DBGMSG("OS-Semaphore being set.");
                    }
                    DBGMSG(((FOS_System.String)"Check: BIOSownedSemaphore: ") + pciDevice.ReadRegister8(BIOSownedSemaphore) + 
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
        /// Enables all ports and checks their line states.
        /// </summary>
        protected void EnablePorts()
        {
#if EHCI_TRACE
            DBGMSG("Enabling ports...");
#endif
            for (byte i = 0; i < RootPortCount; i++)
            {
                RootPorts.Add(new HCPort()
                    {
                        portNum = i
                    });
            }
#if EHCI_TRACE
            DBGMSG("Added root ports.");
#endif
            EnabledPortsFlag = true;
#if EHCI_TRACE
            DBGMSG("Checking line statuses...");
#endif
            for (byte i = 0; i < RootPortCount; i++)
            {
                CheckPortLineStatus(i);
            }
#if EHCI_TRACE
            DBGMSG("Checked port line statuses.");
#endif
        }
        /// <summary>
        /// Resets the specified port.
        /// </summary>
        /// <param name="portNum">The port to reset.</param>
        protected void ResetPort(byte portNum)
        {
            PORTSC[portNum] |= EHCI_Consts.PSTS_PowerOn;
            PORTSC[portNum] &= ~EHCI_Consts.PSTS_Enabled;
            USBSTS |= ~EHCI_Consts.STS_PortChange;
            if (HCHalted)
            {
                ExceptionMethods.Throw(new FOS_System.Exception("EHCI.ResetPort(): HCHalted not zero!"));
            }
            USBINTR = 0;
            PORTSC[portNum] |= EHCI_Consts.PSTS_PortReset;

            //~200ms
            Hardware.Devices.Timer.Default.Wait(200);

            PORTSC[portNum] &= ~EHCI_Consts.PSTS_PortReset;


            // wait and check, whether really zero
            uint timeout = 50;
            while ((PORTSC[portNum] & EHCI_Consts.PSTS_PortReset) != 0)
            {
                // ~1ms
                Hardware.Devices.Timer.Default.Wait(1);

                timeout--;
                if (timeout == 0)
                {
                    //ExceptionMethods.Throw(new FOS_System.Exception("EHCI.ResetPort(): Port not reset!"));

#if EHCI_TRACE
                    DBGMSG("EHCI.ResetPort(): Port not reset!");
#endif
                    break;
                }
            }
            
            USBINTR = EHCI_Consts.STS_AsyncInterrupt | EHCI_Consts.STS_HostSystemError | EHCI_Consts.STS_PortChange |
                      EHCI_Consts.STS_USBInterrupt | EHCI_Consts.STS_USBErrorInterrupt;

            //~20ms
            Hardware.Devices.Timer.Default.Wait(20);
        }
        /// <summary>
        /// The static wrapper for the interrupt handler.
        /// </summary>
        /// <param name="data">The EHCI state object.</param>
        protected static void InterruptHandler(FOS_System.Object data)
        {
            ((EHCI)data).InterruptHandler();
        }
        /// <summary>
        /// The interrupt handler for all EHCI interrupts.
        /// </summary>
        protected void InterruptHandler()
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
            USBSTS = val; //Reset interrupt

            if((val & EHCI_Consts.STS_USBErrorInterrupt) != 0u)
            {
                USBIntCount--;

#if EHCI_TRACE
                DBGMSG("USB Error Interrupt!");
#endif
            }

            if ((val & EHCI_Consts.STS_PortChange) != 0u)
            {
                if (EnabledPortsFlag && pciDevice != null)
                {
                    AnyPortsChanged = true;
                }
            }


            if ((val & EHCI_Consts.STS_HostSystemError) != 0u)
            {
                //If we don't do this, we get stuck in an infinite-loop
                //  of "Start HC -> (Host Error) -> Interrupt -> Start HC -> ..."
                if (HostSystemError)
                {
                    IrrecoverableError = true;

#if EHCI_TRACE
                    BasicConsole.WriteLine("EHCI controller has hit an irrecoverable error!");
#endif
                }
                
                if (!IrrecoverableError)
                {
                    HostSystemError = true;
                    //Attempt restart
                    Start();
                }
            }
            else
            {
                HostSystemError = false;
            }

            if ((val & EHCI_Consts.STS_USBInterrupt) != 0u)
            {
                if (USBIntCount != 0)
                {
                    USBIntCount--;
                }
#if EHCI_TRACE
                DBGMSG(((FOS_System.String)"EHCI: USB Interrupt occurred! USBIntCount: ") + USBIntCount);
#endif
            }

#if EHCI_TRACE
            BasicConsole.DelayOutput(5);
#endif
        }
        /// <summary>
        /// Checks all ports for any changes.
        /// </summary>
        protected void PortCheck()
        {
            AnyPortsChanged = false;
            for (byte j = 0; j < RootPortCount; j++)
            {
                if ((PORTSC[j] & EHCI_Consts.PSTS_ConnectedChange) != 0)
                {
                    PORTSC[j] |= EHCI_Consts.PSTS_ConnectedChange; // reset interrupt
                    if ((PORTSC[j] & EHCI_Consts.PSTS_Connected) != 0)
                    {
                        CheckPortLineStatus(j);
                    }
                    else
                    {
                        PORTSC[j] &= ~EHCI_Consts.PSTS_CompanionHCOwned; // port is given back to the EHCI

                        if (((HCPort)RootPorts[j]).deviceInfo != null)
                        {
                            ((HCPort)RootPorts[j]).deviceInfo.FreePort();
                        }

                    }
                }
            }
            AnyPortsChanged = false;
        }
        /// <summary>
        /// Checks the specified port's line state. Calls Detect device or releases the port to the companion
        /// host controller if the connected device is a low/full speed device.
        /// </summary>
        /// <param name="portNum">The port to check.</param>
        protected void CheckPortLineStatus(byte portNum)
        {
            if ((PORTSC[portNum] & EHCI_Consts.PSTS_Connected) == 0)
            {
#if EHCI_TRACE
                DBGMSG("Port not connected.");
#endif
                return;
            }

            byte lineStatus = (byte)((PORTSC[portNum] >> 10) & 3); // bits 11:10

            switch (lineStatus)
            {
                case 1: // K-state, release ownership of port, because a low speed device is attached
#if EHCI_TRACE
                    DBGMSG("Low-speed device attached. Releasing port.");
#endif
                    PORTSC[portNum] |= EHCI_Consts.PSTS_CompanionHCOwned; // release it to the cHC
                    break;
                case 0: // SE0
                case 2: // J-state
                case 3: // undefined
                    DetectDevice(portNum);
                    break;
            }
        }
        /// <summary>
        /// Attempts to detect a device connected to the specified port.
        /// If one is detected, it creates the device through the USBManager.
        /// </summary>
        /// <param name="portNum">The port number to try and detect a device on.</param>
        protected void DetectDevice(byte portNum)
        {
#if EHCI_TRACE
            DBGMSG("Detecting device...");
#endif
            ResetPort(portNum);
#if EHCI_TRACE
            DBGMSG("Reset port.");
#endif
            if (EnabledPortsFlag && ((PORTSC[portNum] & EHCI_Consts.PSTS_PowerOn) != 0)) // power on
            {
#if EHCI_TRACE
                DBGMSG("Device powered on.");
#endif
                if ((PORTSC[portNum] & EHCI_Consts.PSTS_Enabled) != 0) // High speed
                {
#if EHCI_TRACE
                    DBGMSG("Setting up USB device.");
#endif
                    SetupUSBDevice(portNum);
                }
                else // Full speed
                {
#if EHCI_TRACE
                    DBGMSG("Full-speed device attached. Releasing port.");
                    BasicConsole.DelayOutput(2);
#endif
                    PORTSC[portNum] |= EHCI_Consts.PSTS_CompanionHCOwned; // release it to the cHC
                }
            }
#if EHCI_TRACE
            DBGMSG("End DetectDevice()");
#endif
        }
        /// <summary>
        /// Sets up a USB transfer for sending via the EHCI.
        /// </summary>
        /// <param name="transfer">The transfer to set up.</param>
        protected override void _SetupTransfer(USBTransfer transfer)
        {
            transfer.underlyingTransferData = (EHCI_QueueHead_Struct*)FOS_System.Heap.AllocZeroed((uint)sizeof(EHCI_QueueHead_Struct), 32);
        }
        /// <summary>
        /// Sets up a SETUP transaction and adds it to the specified transfer.
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
        protected override void _SETUPTransaction(USBTransfer transfer, USBTransaction uTransaction, bool toggle, ushort tokenBytes,
                                           byte type, byte req, byte hiVal, byte loVal, ushort index, ushort length)
        {
            EHCITransaction eTransaction = new EHCITransaction();
            uTransaction.underlyingTz = eTransaction;
            eTransaction.inBuffer = null;
            eTransaction.inLength = 0u;
            fixed(void** bufferPtr = &(eTransaction.qTDBuffer))
            {
                eTransaction.qTD = CreateQTD_SETUP((EHCI_qTD_Struct*)1u, toggle, tokenBytes, type, req, hiVal, loVal, index, length, bufferPtr).qtd;
            }
            if (transfer.transactions.Count > 0)
            {
                EHCITransaction eLastTransaction = (EHCITransaction)((USBTransaction)(transfer.transactions[transfer.transactions.Count - 1])).underlyingTz;
                EHCI_qTD lastQTD = new EHCI_qTD(eLastTransaction.qTD);
                lastQTD.NextqTDPointer = (EHCI_qTD_Struct*)VirtMemManager.GetPhysicalAddress(eTransaction.qTD);
                lastQTD.NextqTDPointerTerminate = false;
            }
        }
        /// <summary>
        /// Sets up an IN transaction and adds it to the specified transfer.
        /// </summary>
        /// <param name="transfer">The transfer to which the transaction should be added.</param>
        /// <param name="uTransaction">The USB Transaction to convert to an EHCI transaction.</param>
        /// <param name="toggle">The transaction toggle state.</param>
        /// <param name="buffer">The buffer to store the incoming data in.</param>
        /// <param name="length">The length of the buffer.</param>
        protected override void _INTransaction(USBTransfer transfer, USBTransaction uTransaction, bool toggle, void* buffer, ushort length)
        {
            EHCITransaction eTransaction = new EHCITransaction();
            uTransaction.underlyingTz = eTransaction;
            eTransaction.inBuffer = buffer;
#if EHCI_TRACE
            DBGMSG(((FOS_System.String)"IN Transaction : buffer=") + (uint)buffer);
#endif
            eTransaction.inLength = length;
            fixed (void** bufferPtr = &(eTransaction.qTDBuffer))
            {
#if EHCI_TRACE
                DBGMSG(((FOS_System.String)"IN Transaction : Before CreateQTD : bufferPtr=&qTDBuffer=") + (uint)bufferPtr + 
                                           ", *bufferPtr=" + (uint)(*bufferPtr));
#endif
                EHCI_qTD qtd = CreateQTD_IO((EHCI_qTD_Struct*)1u, 1, toggle, length, bufferPtr, length);
                eTransaction.qTD = qtd.qtd;
#if EHCI_TRACE
                DBGMSG(((FOS_System.String)"IN Transaction : After CreateQTD : bufferPtr=&qTDBuffer=") + (uint)bufferPtr +
                                           ", *bufferPtr=" + (uint)(*bufferPtr) + ", Buffer0=" + (uint)qtd.Buffer0);
                for (int i = 0; i < length; i++)
                {
                    ((byte*)eTransaction.qTDBuffer)[i] = 0xDE;
                    ((byte*)buffer)[i] = 0xBF;
                }
                for (int i = length; i < 0x1000; i++)
                {
                    ((byte*)eTransaction.qTDBuffer)[i] = 0x56;
                }
#endif
            }
            if (transfer.transactions.Count > 0)
            {
                EHCITransaction eLastTransaction = (EHCITransaction)((USBTransaction)(transfer.transactions[transfer.transactions.Count - 1])).underlyingTz;
                EHCI_qTD lastQTD = new EHCI_qTD(eLastTransaction.qTD);
                lastQTD.NextqTDPointer = (EHCI_qTD_Struct*)VirtMemManager.GetPhysicalAddress(eTransaction.qTD);
                lastQTD.NextqTDPointerTerminate = false;
            }
        }
        /// <summary>
        /// Sets up an IN transaction and adds it to the specified transfer.
        /// </summary>
        /// <param name="transfer">The transfer to which the transaction should be added.</param>
        /// <param name="uTransaction">The USB Transaction to convert to an EHCI transaction.</param>
        /// <param name="toggle">The transaction toggle state.</param>
        /// <param name="buffer">The buffer of outgoing data.</param>
        /// <param name="length">The length of the buffer.</param>
        protected override void _OUTTransaction(USBTransfer transfer, USBTransaction uTransaction, bool toggle, void* buffer, ushort length)
        {
            EHCITransaction eTransaction = new EHCITransaction();
            uTransaction.underlyingTz = eTransaction;
            eTransaction.inBuffer = null;
            eTransaction.inLength = 0u;
            fixed (void** bufferPtr = &(eTransaction.qTDBuffer))
            {
                eTransaction.qTD = CreateQTD_IO((EHCI_qTD_Struct*)1u, 0, toggle, length, bufferPtr, length).qtd;
                if (buffer != null && length != 0)
                {
                    Utilities.MemoryUtils.MemCpy_32((byte*)eTransaction.qTDBuffer, (byte*)buffer, length);
                }
            }
            if (transfer.transactions.Count > 0)
            {
                EHCITransaction eLastTransaction = (EHCITransaction)((USBTransaction)(transfer.transactions[transfer.transactions.Count - 1])).underlyingTz;
                EHCI_qTD lastQTD = new EHCI_qTD(eLastTransaction.qTD);
                lastQTD.NextqTDPointer = (EHCI_qTD_Struct*)VirtMemManager.GetPhysicalAddress(eTransaction.qTD);
                lastQTD.NextqTDPointerTerminate = false;
            }
        }
        /// <summary>
        /// Issues the specified transfer to the physical device via the async schedule.
        /// </summary>
        /// <param name="transfer">The transfer to issue.</param>
        protected override void _IssueTransfer(USBTransfer transfer)
        {
            EHCITransaction lastTransaction = (EHCITransaction)((USBTransaction)transfer.transactions[transfer.transactions.Count - 1]).underlyingTz;
            EHCI_qTD lastQTD = new EHCI_qTD(lastTransaction.qTD);
            lastQTD.InterruptOnComplete = true;

#if EHCI_TRACE
            //Test walking the transaction tree
            bool treeOK = true;
            for (int k = 0; k < transfer.transactions.Count - 1; k++)
            {
                EHCITransaction transaction1 = (EHCITransaction)((USBTransaction)transfer.transactions[k]).underlyingTz;
                EHCITransaction transaction2 = (EHCITransaction)((USBTransaction)transfer.transactions[k + 1]).underlyingTz;
                EHCI_qTD qtd1 = new EHCI_qTD(transaction1.qTD);
                treeOK = treeOK && (qtd1.NextqTDPointer == transaction2.qTD) && !qtd1.NextqTDPointerTerminate;
            }
            {
                treeOK = treeOK && lastQTD.NextqTDPointerTerminate;
            }
            DBGMSG(((FOS_System.String)"Transfer transactions tree OK: ") + treeOK);
            BasicConsole.DelayOutput(10);
#endif            

            EHCITransaction firstTransaction = (EHCITransaction)((USBTransaction)(transfer.transactions[0])).underlyingTz;
            InitQH((EHCI_QueueHead_Struct*)transfer.underlyingTransferData, (uint)transfer.underlyingTransferData, firstTransaction.qTD, false, transfer.device.address, transfer.endpoint, transfer.packetSize);
            
            for (byte i = 0; i < EHCI_Consts.NumAsyncListRetries && !transfer.success; i++)
            {
#if EHCI_TRACE
                transfer.success = true;
                for (int k = 0; k < transfer.transactions.Count; k++)
                {
                    EHCITransaction transaction = (EHCITransaction)((USBTransaction)transfer.transactions[k]).underlyingTz;
                    byte status = new EHCI_qTD(transaction.qTD).Status;
                    transfer.success = transfer.success && (status == 0 || status == Utils.BIT(0));

                    DBGMSG(((FOS_System.String)"PRE Issue: Transaction ") + k + " status: " + status);
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
                AddToAsyncSchedule(transfer);
                if (IrrecoverableError)
                {
                    transfer.success = false;

#if EHCI_TRACE
                    DBGMSG("EHCI: Irrecoverable error! No retry.");
                    BasicConsole.DelayOutput(2);
#endif
                }
                else
                {
                    transfer.success = true;
                    for (int k = 0; k < transfer.transactions.Count; k++)
                    {
                        EHCITransaction transaction = (EHCITransaction)((USBTransaction)transfer.transactions[k]).underlyingTz;
                        byte status = new EHCI_qTD(transaction.qTD).Status;
                        transfer.success = transfer.success && (status == 0 || status == Utils.BIT(0));

#if EHCI_TRACE
                        DBGMSG(((FOS_System.String)"POST Issue: Transaction ") + k + " status: " + status);
#endif
                    }

#if EHCI_TRACE
                    if (!transfer.success)
                    {
                        DBGMSG(((FOS_System.String)"EHCI: Retry transfer: ") + (i + 1));
                        BasicConsole.DelayOutput(2);
                    }
#endif
                }
            }

            FOS_System.Heap.Free(transfer.underlyingTransferData);
            for (int k = 0; k < transfer.transactions.Count; k++)
            {
                EHCITransaction transaction = (EHCITransaction)((USBTransaction)transfer.transactions[k]).underlyingTz;

                if (transaction.inBuffer != null && transaction.inLength != 0)
                {
#if EHCI_TRACE
                    DBGMSG(((FOS_System.String)"Doing MemCpy of in data... inBuffer=") + (uint)transaction.inBuffer + 
                                               ", qTDBuffer=" + (uint)transaction.qTDBuffer + 
                                               ", inLength=" + transaction.inLength + ", Data to copy: ");
#endif
                    
                    Utilities.MemoryUtils.MemCpy_32((byte*)transaction.inBuffer, (byte*)transaction.qTDBuffer, transaction.inLength);

#if EHCI_TRACE
                    for (int i = 0; i < transaction.inLength; i++)
                    {
                        DBGMSG(((FOS_System.String)"i=") + i + ", qTDBuffer[i]=" + ((byte*)transaction.qTDBuffer)[i] + ", inBuffer[i]=" + ((byte*)transaction.inBuffer)[i]);
                    }
#endif
#if EHCI_TRACE
                    DBGMSG("Done.");
                    BasicConsole.DelayOutput(2);
#endif
                }
                FOS_System.Heap.Free(transaction.qTDBuffer);
                FOS_System.Heap.Free(transaction.qTD);
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
        /// Initialises the async schedule.
        /// </summary>
        protected void InitializeAsyncSchedule()
        {
            if (IdleQueueHead == null)
            {
                IdleQueueHead = TailQueueHead = new EHCI_QueueHead().queueHead;
            }
            InitQH(IdleQueueHead, (uint)IdleQueueHead, null, true, 0, 0, 0);
            ASYNCLISTADDR = (EHCI_QueueHead_Struct*)VirtMemManager.GetPhysicalAddress(IdleQueueHead);
            EnableAsyncSchedule();
        }
        /// <summary>
        /// Enables the async schedule.
        /// </summary>
        protected void EnableAsyncSchedule()
        {
            USBCMD |= EHCI_Consts.CMD_AsyncScheduleEnableMask;

            uint timeout = 7;
            while ((USBSTS & EHCI_Consts.STS_AsyncEnabled) == 0) // wait until it is really on
            {
                timeout--;
                if (timeout>0)
                {
                    //~10ms
                    Hardware.Devices.Timer.Default.Wait(10);
                }
                else
                {
                    ExceptionMethods.Throw(new FOS_System.Exception("EHCI.EnableAsyncScheduler(): Timeout Error - STS_ASYNC_ENABLED not set!"));
                    break;
                }
            }
        }
        /// <summary>
        /// Adds a transfer for the async schedule.
        /// </summary>
        /// <param name="transfer">The transfer to add.</param>
        protected void AddToAsyncSchedule(USBTransfer transfer)
        {
            USBIntCount = 1;

            if ((USBSTS & EHCI_Consts.STS_AsyncEnabled) == 0)
            {
                EnableAsyncSchedule(); // Start async scheduler, when it is not running
            }
            //USBCMD |= EHCI_Consts.CMD_ASYNCH_INT_DOORBELL; // Activate Doorbell: We would like to receive an asynchronous schedule interrupt

            EHCI_QueueHead oldTailQH = new EHCI_QueueHead(TailQueueHead); // save old tail QH
            TailQueueHead = (EHCI_QueueHead_Struct*)transfer.underlyingTransferData; // new QH will now be end of Queue

            EHCI_QueueHead idleQH = new EHCI_QueueHead(IdleQueueHead);
            EHCI_QueueHead tailQH = new EHCI_QueueHead(TailQueueHead);
            // Create ring. Link new QH with idleQH (always head of Queue)
            tailQH.HorizontalLinkPointer = (EHCI_QueueHead_Struct*)VirtMemManager.GetPhysicalAddress(IdleQueueHead);
            tailQH.Type = 1;
            // Insert qh to Queue as element behind old queue head
            oldTailQH.HorizontalLinkPointer = (EHCI_QueueHead_Struct*)VirtMemManager.GetPhysicalAddress(TailQueueHead);
            oldTailQH.Type = 1;

            //int timeout = 10 * velocity + 25; // Wait up to 250+100*velocity milliseconds for USBasyncIntFlag to be set
            while (USBIntCount > 0 /*&& timeout > 0*/ && !IrrecoverableError)
            {
                //timeout--;
                //~100ms
                //for (int i = 0; i < 1000000; i++)
                //    ;
            }

            //            if (timeout == 0)
            //            {
            //#if EHCI_TRACE
            //                DBGMSG(((FOS_System.String)"EHCI.AddToAsyncScheduler(): Num interrupts not 0! not set! USBIntCount: ") + USBIntCount);
            //#endif
            //            }

            // Restore link of idleQH to idleQH (endless loop)
            idleQH.HorizontalLinkPointer = (EHCI_QueueHead_Struct*)VirtMemManager.GetPhysicalAddress(IdleQueueHead);
            idleQH.Type = 0x1;
            TailQueueHead = IdleQueueHead; // qh done. idleQH is end of Queue again (ring structure of asynchronous schedule)
        }

        /// <summary>
        /// Initialises a queue head - memory must already be allocated.
        /// </summary>
        /// <param name="headPtr">A pointer to the queue head structure to initialise.</param>
        /// <param name="horizPtr">
        /// The physical address of the next queue head in the list (or the first queue head since the 
        /// async queue is a circular buffer)
        /// </param>
        /// <param name="firstQTD">A pointer to the first qTD of the queue head.</param>
        /// <param name="H">The Head of Reclamation list flag.</param>
        /// <param name="deviceAddr">The address of the USB device to which this queue head belongs.</param>
        /// <param name="endpoint">The endpoint number of the USB device to which this queue head belongs.</param>
        /// <param name="maxPacketSize">The maximum packet size to use when transferring.</param>
        protected void InitQH(EHCI_QueueHead_Struct* headPtr, uint horizPtr, EHCI_qTD_Struct* firstQTD, bool H, byte deviceAddr,
                                   byte endpoint, ushort maxPacketSize)
        {
            EHCI_QueueHead head = new EHCI_QueueHead(headPtr);
            // bit 31:5 Horizontal Link Pointer
            head.HorizontalLinkPointer = (EHCI_QueueHead_Struct*)VirtMemManager.GetPhysicalAddress(horizPtr);
            head.Type = 0x1;      // type:  00b iTD,   01b QH,   10b siTD,   11b FSTN
            head.Terminate = false; // T-Bit: is set to zero
            head.DeviceAddress = deviceAddr;         // The device address
            head.InactiveOnNextTransaction = false;
            head.EndpointNumber = endpoint;       // endpoint 0 contains Device infos such as name
            head.EndpointSpeed = 2;              // 00b = full speed; 01b = low speed; 10b = high speed
            head.DataToggleControl = true;              // get the Data Toggle bit out of the included qTD
            head.HeadOfReclamationList = H;              // mark a queue head as being the head of the reclaim list
            head.MaximumPacketLength = maxPacketSize;     // 64 byte for a control transfer to a high speed device
            head.ControlEndpointFlag = false;              // only used if endpoint is a control endpoint and not high speed
            head.NakCountReload = 0;              // this value is used by EHCI to reload the Nak Counter field. 0=ignores NAK counter.
            head.InterruptScheduleMask = 0;              // not used for async schedule
            head.SplitCompletionMask = 0;              // unused if (not low/full speed and in periodic schedule)
            head.HubAddr = 0;              // unused if high speed (Split transfer)
            head.PortNumber = 0;              // unused if high speed (Split transfer)
            head.HighBandwidthPipeMultiplier = 1;              // 1-3 transaction per micro-frame, 0 means undefined results
            if (firstQTD == null)
            {
                head.NextqTDPointer = null;
                head.NextqTDPointerTerminate = true;
            }
            else
            {
                head.NextqTDPointer = (EHCI_qTD_Struct*)VirtMemManager.GetPhysicalAddress(firstQTD);
                head.NextqTDPointerTerminate = false;
            }
        }
        /// <summary>
        /// Creates and initialises a new qTD as a SETUP qTD.
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
        /// <param name="buffer">OUT. A pointer to the qTD data buffer.</param>
        /// <returns>The new qTD.</returns>
        protected EHCI_qTD CreateQTD_SETUP(EHCI_qTD_Struct* next, bool toggle, ushort tokenBytes, byte type, byte req,
                                                 byte hiVal, byte loVal, ushort index, ushort length, void** buffer)
        {
            EHCI_qTD td = AllocAndInitQTD(next);

            td.PIDCode = (byte)EHCI_qTDTypes.SETUP;      // SETUP = 2
            td.TotalBytesToTransfer = tokenBytes; // dependent on transfer
            td.DataToggle = toggle;     // Should be toggled every list entry

                                                                     //PAGESIZE
            //Transaction Buffer0
            USBRequest* request = (USBRequest*)(*buffer = AllocQTDbuffer(td));
            request->type = type;
            request->request = req;
            request->valueHi = hiVal;
            request->valueLo = loVal;
            request->index = index;
            request->length = length;
            
            return td;
        }
        /// <summary>
        /// Creates a new qTD and initialises it as an IN or OUT qTD.
        /// </summary>
        /// <param name="next">A pointer to the next qTD in the linked list or 1 to specify no pointer.</param>
        /// <param name="direction">The direction of the qTD (in or out)</param>
        /// <param name="toggle">The toggle state for the new qTD.</param>
        /// <param name="tokenBytes">The number of bytes to transfer.</param>
        /// <param name="buffer">OUT. A pointer to the qTD data buffer.</param>
        /// <param name="bufferSize">The size of the qTD data buffer.</param>
        /// <returns>The new qTD.</returns>
        protected EHCI_qTD CreateQTD_IO(EHCI_qTD_Struct* next, byte direction, bool toggle, ushort tokenBytes, void** buffer, uint bufferSize)
        {
            EHCI_qTD td = AllocAndInitQTD(next);

            td.PIDCode = direction;
            td.TotalBytesToTransfer = tokenBytes; // dependent on transfer
            td.DataToggle = toggle;     // Should be toggled every list entry

            *buffer = AllocQTDbuffer(td);

            return td;
        }
        /// <summary>
        /// Allocates memory for a new qTD and does intialisation common to all qTD types.
        /// </summary>
        /// <param name="next">A pointer to the next qTD in the linked list or 1 to specify no pointer.</param>
        /// <returns>The new qTD.</returns>
        protected static EHCI_qTD AllocAndInitQTD(EHCI_qTD_Struct* next)
        {
            EHCI_qTD newQTD = new EHCI_qTD();

            if ((uint)next != 0x1)
            {
                newQTD.NextqTDPointerTerminate = false;
                newQTD.NextqTDPointer = (EHCI_qTD_Struct*)VirtMemManager.GetPhysicalAddress(next);
            }
            else
            {
                newQTD.NextqTDPointerTerminate = true;
            }

            newQTD.AlternateNextqTDPointerTerminate = true;  // No alternate next, so T-Bit is set to 1
            newQTD.Status = 0x80; // This will be filled by the Host Controller. Active bit set
            newQTD.ErrorCounter = 0x0;  // Written by the Host Controller.
            newQTD.CurrentPage = 0x0;  // Written by the Host Controller.
            newQTD.InterruptOnComplete = false; //Set only for the last transaction of a transfer

            return newQTD;
        }
        /// <summary>
        /// Allocates a correctly page-aligned buffer for use as a qTD data buffer. Sets it as Buffer0 of the qTD.
        /// </summary>
        /// <param name="td">The qTD to add the buffer to.</param>
        /// <returns>A pointer to the new buffer.</returns>
        protected static void* AllocQTDbuffer(EHCI_qTD td)
        {
            byte* result = (byte*)FOS_System.Heap.AllocZeroed(0x1000u, 0x1000u);
            td.Buffer0 = (byte*)VirtMemManager.GetPhysicalAddress(result);
            td.CurrentPage = 0;
            td.CurrentOffset = 0;
            return result;
        }
        
#if EHCI_TRACE
        internal static void DBGMSG(FOS_System.String msg)
        {
            BasicConsole.WriteLine(msg);
        }
#endif

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
            FOS_System.String testName = "Queue Transfer Descrip";
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
                EHCITesting.DBGERR(testName, ((FOS_System.String)"Test failed! Errors: ") + EHCITesting.errors + " Warnings: " + EHCITesting.warnings);
            }
            else
            {
                if (EHCITesting.warnings > 0)
                {
                    EHCITesting.DBGWRN(testName, ((FOS_System.String)"Test passed with warnings: ") + EHCITesting.warnings);
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
    /// Represents a transaction made through an EHCI.
    /// </summary>
    public unsafe class EHCITransaction : HCTransaction
    {
        /// <summary>
        /// A pointer to the actual qTD of the transaction.
        /// </summary>
        public EHCI_qTD_Struct* qTD;
        /// <summary>
        /// A pointer to the main qTD buffer of the transaction.
        /// </summary>
        public void* qTDBuffer;
        /// <summary>
        /// A pointer to the input buffer.
        /// </summary>
        public void* inBuffer;
        /// <summary>
        /// The length of the input buffer.
        /// </summary>
        public uint inLength;
    }

    /// <summary>
    /// Represents a Queue Head structure's memory layout.
    /// This structure can be passed to the HCI.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct EHCI_QueueHead_Struct
    {
        /*
         * See section 3.6 of spec.
         */

        /// <summary>
        /// UInt32 1 in the structure.
        /// </summary>
        public uint u1;
        /// <summary>
        /// UInt32 2 in the structure.
        /// </summary>
        public uint u2;
        /// <summary>
        /// UInt32 3 in the structure.
        /// </summary>
        public uint u3;
        /// <summary>
        /// UInt32 4 in the structure.
        /// </summary>
        public uint u4;
        /// <summary>
        /// UInt32 5 in the structure.
        /// </summary>
        public uint u5;
        /// <summary>
        /// UInt32 6 in the structure.
        /// </summary>
        public uint u6;
        /// <summary>
        /// UInt32 7 in the structure.
        /// </summary>
        public uint u7;
        /// <summary>
        /// UInt32 8 in the structure.
        /// </summary>
        public uint u8;
        /// <summary>
        /// UInt32 9 in the structure.
        /// </summary>
        public uint u9;
        /// <summary>
        /// UInt32 10 in the structure.
        /// </summary>
        public uint u10;
        /// <summary>
        /// UInt32 11 in the structure.
        /// </summary>
        public uint u11;
        /// <summary>
        /// UInt32 12 in the structure.
        /// </summary>
        public uint u12;
    }
    /// <summary>
    /// Represents a Queue Transfer Descriptor structure's memory layout.
    /// This structure can be passed to the HCI.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct EHCI_qTD_Struct
    {
        /*
         * See section 3.5 of spec.
         */

        /// <summary>
        /// UInt32 1 in the structure.
        /// </summary>
        public uint u1;
        /// <summary>
        /// UInt32 2 in the structure.
        /// </summary>
        public uint u2;
        /// <summary>
        /// UInt32 3 in the structure.
        /// </summary>
        public uint u3;
        /// <summary>
        /// UInt32 4 in the structure.
        /// </summary>
        public uint u4;
        /// <summary>
        /// UInt32 5 in the structure.
        /// </summary>
        public uint u5;
        /// <summary>
        /// UInt32 6 in the structure.
        /// </summary>
        public uint u6;
        /// <summary>
        /// UInt32 7 in the structure.
        /// </summary>
        public uint u7;
        /// <summary>
        /// UInt32 8 in the structure.
        /// </summary>
        public uint u8;
    }
    /// <summary>
    /// Represents a qTD. The underlying memory structure can be passed to the HCI. 
    /// This class provides methods / properties for manipulating qTD values.
    /// </summary>
    public unsafe class EHCI_qTD : FOS_System.Object
    {
        /*
         * See section 3.5 of spec.
         */

        /// <summary>
        /// The qTD data/memory structure that can be passed to the HCI.
        /// </summary>
        public EHCI_qTD_Struct* qtd;

        /// <summary>
        /// Pointer to the next qTD in the linked list.
        /// </summary>
        public EHCI_qTD_Struct* NextqTDPointer
        {
            [Compiler.NoGC]
            get
            {
                return (EHCI_qTD_Struct*)(qtd->u1 & 0xFFFFFFE0u);
            }
            [Compiler.NoGC]
            set
            {
                qtd->u1 = (qtd->u1 & 0x0000001Fu) | ((uint)value & 0xFFFFFFE0u);
            }
        }
        /// <summary>
        /// Whether the next qTD pointer indicates the end of the linked list.
        /// </summary>
        public bool NextqTDPointerTerminate
        {
            [Compiler.NoGC]
            get
            {
                return (qtd->u1 & 0x00000001u) > 0;
            }
            [Compiler.NoGC]
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
        /// Whether the alternate next qTD pointer indicates the end of the linked list.
        /// </summary>
        public bool AlternateNextqTDPointerTerminate
        {
            [Compiler.NoGC]
            get
            {
                return (qtd->u2 & 0x00000001u) > 0;
            }
            [Compiler.NoGC]
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
        /// The status.
        /// </summary>
        public byte Status
        {
            [Compiler.NoGC]
            get
            {
                return (byte)(qtd->u3);
            }
            [Compiler.NoGC]
            set
            {
                qtd->u3 = (qtd->u3 & 0xFFFFFF00u) | value;
            }
        }
        /// <summary>
        /// The PID code.
        /// </summary>
        public byte PIDCode
        {
            [Compiler.NoGC]
            get
            {
                return (byte)((qtd->u3 & 0x000000300u) >> 8);
            }
            [Compiler.NoGC]
            set
            {
                qtd->u3 = (qtd->u3 & 0xFFFFFCFFu) | ((uint)value << 8); 
            }
        }
        /// <summary>
        /// The error counter.
        /// </summary>
        public byte ErrorCounter
        {
            [Compiler.NoGC]
            get
            {
                return (byte)((qtd->u3 & 0x00000C00u) >> 10);
            }
            [Compiler.NoGC]
            set
            {
                qtd->u3 = (qtd->u3 & 0xFFFFF3FFu) | ((uint)value << 10);
            }
        }
        /// <summary>
        /// The current page number.
        /// </summary>
        public byte CurrentPage
        {
            [Compiler.NoGC]
            get
            {
                return (byte)((qtd->u3 & 0x00007000) >> 12);
            }
            [Compiler.NoGC]
            set
            {
                qtd->u3 = (qtd->u3 & 0xFFFF8FFF) | ((uint)value << 12);
            }
        }
        /// <summary>
        /// Whether to trigger an interrupt when the transfer is complete.
        /// </summary>
        public bool InterruptOnComplete
        {
            [Compiler.NoGC]
            get
            {
                return (qtd->u3 & 0x00008000u) > 0;
            }
            [Compiler.NoGC]
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
        /// The total number of bytes to transfer.
        /// </summary>
        public UInt16 TotalBytesToTransfer
        {
            [Compiler.NoGC]
            get
            {
                return (UInt16)((qtd->u3 >> 16) & 0x00007FFF);
            }
            [Compiler.NoGC]
            set
            {
                qtd->u3 = (qtd->u3 & 0x8000FFFF) | ((uint)value << 16);
            }
        }
        /// <summary>
        /// The data toggle status.
        /// </summary>
        public bool DataToggle
        {
            [Compiler.NoGC]
            get
            {
                return (qtd->u3 & 0x80000000u) > 0;
            }
            [Compiler.NoGC]
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
        /// Current offset in C_Page buffer.
        /// </summary>
        public ushort CurrentOffset
        {
            [Compiler.NoGC]
            get
            {
                return (ushort)(qtd->u4 & 0x00000FFFu);
            }
            [Compiler.NoGC]
            set
            {
                qtd->u4 = (qtd->u4 & 0xFFFFF000u) | ((uint)value & 0x00000FFFu);
            }
        }

        /// <summary>
        /// Buffer 0 pointer.
        /// </summary>
        public byte* Buffer0
        {
            [Compiler.NoGC]
            get
            {
                return (byte*)(qtd->u4 & 0xFFFFF000u);
            }
            [Compiler.NoGC]
            set
            {
                qtd->u4 = (qtd->u4 & 0x00000FFFu) | ((uint)value & 0xFFFFF000u);
            }
        }
        /// <summary>
        /// Buffer 1 pointer.
        /// </summary>
        public byte* Buffer1
        {
            [Compiler.NoGC]
            get
            {
                return (byte*)(qtd->u5 & 0xFFFFF000);
            }
            [Compiler.NoGC]
            set
            {
                qtd->u5 = (uint)value & 0xFFFFF000;
            }
        }
        /// <summary>
        /// Buffer 2 pointer.
        /// </summary>
        public byte* Buffer2
        {
            [Compiler.NoGC]
            get
            {
                return (byte*)(qtd->u6 & 0xFFFFF000);
            }
            [Compiler.NoGC]
            set
            {
                qtd->u6 = (uint)value & 0xFFFFF000;
            }
        }
        /// <summary>
        /// Buffer 3 pointer.
        /// </summary>
        public byte* Buffer3
        {
            [Compiler.NoGC]
            get
            {
                return (byte*)(qtd->u7 & 0xFFFFF000);
            }
            [Compiler.NoGC]
            set
            {
                qtd->u7 = (uint)value & 0xFFFFF000;
            }
        }
        /// <summary>
        /// Buffer 4 pointer.
        /// </summary>
        public byte* Buffer4
        {
            [Compiler.NoGC]
            get
            {
                return (byte*)(qtd->u8 & 0xFFFFF000);
            }
            [Compiler.NoGC]
            set
            {
                qtd->u8 = (uint)value & 0xFFFFF000;
            }
        }
        
        /// <summary>
        /// Initializes a new qTD with new data structure.
        /// </summary>
        public EHCI_qTD()
        {
            qtd = (EHCI_qTD_Struct*)FOS_System.Heap.AllocZeroed((uint)sizeof(EHCI_qTD_Struct), 32);
        }
        /// <summary>
        /// Initializes a qTD with specified underlying data structure.
        /// </summary>
        /// <param name="aqTD">The existing underlying data structure.</param>
        public EHCI_qTD(EHCI_qTD_Struct* aqTD)
        {
            qtd = aqTD;
        }

        /// <summary>
        /// Frees the underlying memory structure.
        /// </summary>
        [Compiler.NoGC]
        public void Free()
        {
            FOS_System.Heap.Free(qtd);
            qtd = null;
        }
    }
    /// <summary>
    /// Represents a queue head. The underlying memory structure can be passed to the HCI. 
    /// This class provides methods / properties for manipulating queue head values.
    /// </summary>
    public unsafe class EHCI_QueueHead : FOS_System.Object
    {
        /*
         * See section 3.6 of spec.
         */

        /// <summary>
        /// The queue head data/memory structure that can be passed to the HCI.
        /// </summary>
        public EHCI_QueueHead_Struct* queueHead;

        /// <summary>
        /// Whether the horizontal link pointer terminates (is valid) or not.
        /// </summary>
        public bool Terminate
        {
            [Compiler.NoGC]
            get
            {
                return (queueHead->u1 & 0x00000001u) != 0;
            }
            [Compiler.NoGC]
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
        /// Queue head type.
        /// </summary>
        public byte Type
        {
            [Compiler.NoGC]
            get
            {
                return (byte)((queueHead->u1 >> 1) & 0x00000003u);
            }
            [Compiler.NoGC]
            set
            {
                queueHead->u1 = (queueHead->u1 & 0xFFFFFFF9u) | (uint)((value & 0x00000003u) << 1);
            }
        }
        /// <summary>
        /// Horizontal link pointer - points to the next queue head in the list.
        /// </summary>
        public EHCI_QueueHead_Struct* HorizontalLinkPointer
        {
            [Compiler.NoGC]
            get
            {
                return (EHCI_QueueHead_Struct*)(queueHead->u1 & 0xFFFFFFE0u);
            }
            [Compiler.NoGC]
            set
            {
                queueHead->u1 = ((uint)value & 0xFFFFFFE0u) | (queueHead->u1 & 0x0000001Fu);
            }
        }
        /// <summary>
        /// Target USB device address.
        /// </summary>
        public byte DeviceAddress
        {
            [Compiler.NoGC]
            get
            {
                return (byte)(queueHead->u2 & 0x0000007Fu);
            }
            [Compiler.NoGC]
            set
            {
                queueHead->u2 = (queueHead->u2 & 0xFFFFFF80u) | (value & 0x0000007Fu);
            }
        }
        /// <summary>
        /// Inactive on next transaction.
        /// </summary>
        public bool InactiveOnNextTransaction
        {
            [Compiler.NoGC]
            get
            {
                return (queueHead->u2 & 0x00000080u) > 0;
            }
            [Compiler.NoGC]
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
        /// Target USB endpoint number.
        /// </summary>
        public byte EndpointNumber
        {
            [Compiler.NoGC]
            get
            {
                return (byte)((queueHead->u2 & 0x00000F00u) >> 8);
            }
            [Compiler.NoGC]
            set
            {
                queueHead->u2 = (queueHead->u2 & 0xFFFFF0FFu) | (((uint)value) << 8);
            }
        }
        /// <summary>
        /// Target USB endpoint speed.
        /// </summary>
        public byte EndpointSpeed
        {
            [Compiler.NoGC]
            get
            {
                return (byte)((queueHead->u2 & 0x00003000u) >> 12);
            }
            [Compiler.NoGC]
            set
            {
                queueHead->u2 = (queueHead->u2 & 0xFFFFCFFFu) | (((uint)value) << 12);
            }
        }
        /// <summary>
        /// Data toggle control.
        /// </summary>
        public bool DataToggleControl
        {
            [Compiler.NoGC]
            get
            {
                return (queueHead->u2 & 0x00004000u) > 0;
            }
            [Compiler.NoGC]
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
        /// Whether this queue head is the first in the reclamation list.
        /// </summary>
        public bool HeadOfReclamationList
        {
            [Compiler.NoGC]
            get
            {
                return (queueHead->u2 & 0x00008000u) > 0;
            }
            [Compiler.NoGC]
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
        /// Target endpoint's maximum packet length.
        /// </summary>
        public UInt16 MaximumPacketLength
        {
            [Compiler.NoGC]
            get
            {
                return (UInt16)((queueHead->u2 & 0x07FF0000u) >> 16);
            }
            [Compiler.NoGC]
            set
            {
                queueHead->u2 = (queueHead->u2 & 0xF800FFFFu) | (((uint)value << 16) & 0x07FF0000u);
            }
        }
        /// <summary>
        /// Control endpoint flag.
        /// </summary>
        public bool ControlEndpointFlag
        {
            [Compiler.NoGC]
            get
            {
                return (queueHead->u2 & 0x08000000u) > 0;
            }
            [Compiler.NoGC]
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
        /// Nak count reload number.
        /// </summary>
        public byte NakCountReload
        {
            [Compiler.NoGC]
            get
            {
                return (byte)((queueHead->u2 & 0xF0000000u) >> 28);
            }
            [Compiler.NoGC]
            set
            {
                queueHead->u2 = (queueHead->u2 & 0x0FFFFFFFu) | ((uint)value << 28);
            }
        }
        /// <summary>
        /// Interrupt schedule mask.
        /// </summary>
        public byte InterruptScheduleMask
        {
            [Compiler.NoGC]
            get
            {
                return (byte)(queueHead->u3);
            }
            [Compiler.NoGC]
            set
            {
                queueHead->u3 = (queueHead->u3 & 0xFFFFFF00u) | value;
            }
        }
        /// <summary>
        /// Split completion mask.
        /// </summary>
        public byte SplitCompletionMask
        {
            [Compiler.NoGC]
            get
            {
                return (byte)(queueHead->u3 >> 8);
            }
            [Compiler.NoGC]
            set
            {
                queueHead->u3 = (queueHead->u3 & 0xFFFF00FFu) | ((uint)value << 8);
            }
        }
        /// <summary>
        /// Hub address.
        /// </summary>
        public byte HubAddr
        {
            [Compiler.NoGC]
            get
            {
                return (byte)((queueHead->u3 & 0x007F0000u) >> 16);
            }
            [Compiler.NoGC]
            set
            {
                queueHead->u3 = (queueHead->u3 & 0xFF80FFFFu) | ((uint)value << 16);
            }
        }
        /// <summary>
        /// Port number.
        /// </summary>
        public byte PortNumber
        {
            [Compiler.NoGC]
            get
            {
                return (byte)((queueHead->u3 & 0x3f800000u) >> 23);
            }
            [Compiler.NoGC]
            set
            {
                queueHead->u3 = (queueHead->u3 & 0xC07FFFFFu) | ((uint)value << 23);
            }
        }
        /// <summary>
        /// High bandwidth pipe multiplier.
        /// </summary>
        public byte HighBandwidthPipeMultiplier
        {
            [Compiler.NoGC]
            get
            {
                return (byte)((queueHead->u3 & 0xC0000000u) >> 30);
            }
            [Compiler.NoGC]
            set
            {
                queueHead->u3 = (queueHead->u3 & 0x3FFFFFFFu) | ((uint)value << 30);
            }
        }
        /// <summary>
        /// Current qTD pointer.
        /// </summary>
        public EHCI_qTD_Struct* CurrentqTDPointer
        {
            [Compiler.NoGC]
            get
            {
                return (EHCI_qTD_Struct*)(queueHead->u4 & 0xFFFFFFF0u);
            }
            [Compiler.NoGC]
            set
            {
                queueHead->u4 = (queueHead->u4 & 0x0000000Fu) | ((uint)value & 0xFFFFFFF0u);
            }
        }
        /// <summary>
        /// Next qTD pointer.
        /// </summary>
        public EHCI_qTD_Struct* NextqTDPointer
        {
            [Compiler.NoGC]
            get
            {
                return (EHCI_qTD_Struct*)(queueHead->u5 & 0xFFFFFFF0u);
            }
            [Compiler.NoGC]
            set
            {
                queueHead->u5 = (queueHead->u5 & 0x0000000Fu) | ((uint)value & 0xFFFFFFF0u);
            }
        }
        /// <summary>
        /// Whether the next qTD pointer indicates end of the qTD list or not.
        /// </summary>
        public bool NextqTDPointerTerminate
        {
            [Compiler.NoGC]
            get
            {
                return (queueHead->u5 & 0x00000001u) > 0;
            }
            [Compiler.NoGC]
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
        /// Whether the queue head is active or not.
        /// </summary>
        public bool Active
        {
            [Compiler.NoGC]
            get
            {
                return (queueHead->u7 & 0x00000080u) > 0;
            }
            [Compiler.NoGC]
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
        /// Initializes a new queue head with empty underlying memory structure.
        /// </summary>
        public EHCI_QueueHead()
        {
            queueHead = (EHCI_QueueHead_Struct*)FOS_System.Heap.AllocZeroed((uint)sizeof(EHCI_QueueHead_Struct), 32);
        }
        /// <summary>
        /// Initializes a new queue head with specified underlying memory structure.
        /// </summary>
        /// <param name="aQueueHead">The existing underlying queue head.</param>
        public EHCI_QueueHead(EHCI_QueueHead_Struct* aQueueHead)
        {
            queueHead = aQueueHead;
        }

        /// <summary>
        /// Frees the underlying memory structure.
        /// </summary>
        [Compiler.NoGC]
        public void Free()
        {
            FOS_System.Heap.Free(queueHead);
            queueHead = null;
        }
    }
}
