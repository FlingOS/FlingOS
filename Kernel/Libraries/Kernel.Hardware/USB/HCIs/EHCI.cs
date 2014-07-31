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

using System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware.USB.Devices;
using Utils = Kernel.Utilities.ConstantsUtils;
using Kernel.Utilities;

namespace Kernel.Hardware.USB.HCIs
{
    #region Constants
    public class EHCI_Consts
    {
        /* ****** */
        /* USBCMD */
        /* ****** */

        public static uint CMD_INTERRUPT_THRESHOLD = 0x00FF0000;// valid values are:
        public static uint CMD_1_MICROFRAME = Utils.BIT(16);
        public static uint CMD_2_MICROFRAME = Utils.BIT(17);
        public static uint CMD_4_MICROFRAME = Utils.BIT(18);
        public static uint CMD_8_MICROFRAME = Utils.BIT(19);// 1ms
        public static uint CMD_16_MICROFRAME = Utils.BIT(20);
        public static uint CMD_32_MICROFRAME = Utils.BIT(21);
        public static uint CMD_64_MICROFRAME = Utils.BIT(22);

        public static uint CMD_PARK_MODE = 0x800;
        public static uint CMD_PARK_COUNT = 0x300;
        public static uint CMD_LIGHT_RESET = Utils.BIT(7);
        public static uint CMD_ASYNCH_INT_DOORBELL = Utils.BIT(6);
        public static uint CMD_ASYNCH_ENABLE = Utils.BIT(5);
        public static uint CMD_PERIODIC_ENABLE = Utils.BIT(4);

        public static uint CMD_FRAMELIST_SIZE = 0xC;// valid values are:
        public static uint CMD_FRAMELIST_1024 = 0x0;
        public static uint CMD_FRAMELIST_512 = 0x4;
        public static uint CMD_FRAMELIST_256 = 0x8;

        public static uint CMD_HCRESET = Utils.BIT(1);// reset
        public static uint CMD_RUN_STOP = Utils.BIT(0);// run/stop


        /* ************** */
        /* USBSTS USBINTR */
        /* ************** */

        // only USBSTS
        public static uint STS_ASYNC_ENABLED = Utils.BIT(15);
        public static uint STS_PERIODIC_ENABLED = Utils.BIT(14);
        public static uint STS_RECLAMATION = Utils.BIT(13);
        public static uint STS_HCHALTED = Utils.BIT(12);

        // USBSTS (Interrupts)
        public static uint STS_ASYNC_INT = Utils.BIT(5);
        public static uint STS_HOST_SYSTEM_ERROR = Utils.BIT(4);
        public static uint STS_FRAMELIST_ROLLOVER = Utils.BIT(3);
        public static uint STS_PORT_CHANGE = Utils.BIT(2);
        public static uint STS_USBERRINT = Utils.BIT(1);
        public static uint STS_USBINT = Utils.BIT(0);


        /* *********/
        /* FRINDEX */
        /* *********/

        public static uint FRI_MASK = 0x00001FFF;


        /* **************** */
        /* PERIODICLISTBASE */
        /* **************** */

        public static uint PLB_ALIGNMENT = 0x00000FFF; // 4 KiB


        /* ************* */
        /* ASYNCLISTADDR */
        /* ************* */

        public static uint ALB_ALIGNMENT = 0x0000001F;  // 32 Byte


        /* ********** */
        /* CONFIGFLAG */
        /* ********** */

        public static uint CF = Utils.BIT(0);


        /* *********** */
        /* PORTSC[...] */
        /* *********** */

        public static uint PSTS_COMPANION_HC_OWNED = Utils.BIT(13);// rw
        public static uint PSTS_POWERON = Utils.BIT(12);// rw valid, if PPC == 1
        public static uint PSTS_PORT_RESET = Utils.BIT(8);// rw
        public static uint PSTS_PORT_SUSPEND = Utils.BIT(7);// rw
        public static uint PSTS_PORT_RESUME = Utils.BIT(6);// rw
        public static uint PSTS_OVERCURRENT_CHANGE = Utils.BIT(5);// rwc
        public static uint PSTS_OVERCURRENT = Utils.BIT(4);// ro
        public static uint PSTS_ENABLED_CHANGE = Utils.BIT(3);// rwc
        public static uint PSTS_ENABLED = Utils.BIT(2);// rw
        public static uint PSTS_CONNECTED_CHANGE = Utils.BIT(1);// rwc
        public static uint PSTS_CONNECTED = Utils.BIT(0);// ro

        public static uint N_PORTS = 0xF;// number of ports (Utils.BITs 3:0)
        public static uint PORT_ROUTING_RULES = Utils.BIT(7);// port routing to EHCI or cHC
        public static uint NUMBER_OF_EHCI_ASYNCLIST_RETRIES = 3;
    }
    public class EHCI_QHConsts
    {
        public const byte OUT = 0;
        public const byte IN = 1;
        public const byte SETUP = 2;
    }
    #endregion

    /// <summary>
    /// Represents a USB Extended Host Controller Interface
    /// </summary>
    public unsafe class EHCI : HCI
    {
        //TODO - Reprogram bits of this to handle physical-to-virtual and reverse conversions where necessary

        /*
         * Based on the Intel EHCI Specification for USB 2.0
         *  http://www.intel.co.uk/content/dam/www/public/us/en/documents/technical-specifications/ehci-specification-for-usb.pdf
         */

        /// <summary>
        /// The base address of the USB HCI device in memory.
        /// </summary>
        protected byte* usbBaseAddress;

        //protected List QueueHeadReclaimList;

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
                return (USBSTS & EHCI_Consts.STS_HCHALTED) != 0;
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

        #region old stuff

        ///// <summary>
        ///// Initializes a new EHCI device.
        ///// </summary>
        ///// <param name="aPCIDevice">The PCI device that represents the EHCI device.</param>
        //public EHCI(PCI.PCIDeviceNormal aPCIDevice)
        //    : base(aPCIDevice)
        //{
        //    usbBaseAddress = pciDevice.BaseAddresses[0].BaseAddress();
        //    CapabilitiesRegAddr = usbBaseAddress;
        //    BasicConsole.WriteLine("CapabilitiesRegAddr: " + (FOS_System.String)(uint)CapabilitiesRegAddr);
            
        //    SBRN = pciDevice.ReadRegister8(0x60);
        //    BasicConsole.WriteLine("SBRN: " + (FOS_System.String)SBRN);
            
        //    CapabilitiesRegsLength = *CapabilitiesRegAddr;
        //    BasicConsole.WriteLine("CapabilitiesRegsLength: " + (FOS_System.String)CapabilitiesRegsLength);
        //    HCIVersion = *((UInt16*)(CapabilitiesRegAddr + 2));
        //    BasicConsole.WriteLine("HCIVersion: " + (FOS_System.String)HCIVersion);
        //    HCSParams = *((UInt32*)(CapabilitiesRegAddr + 4));
        //    BasicConsole.WriteLine("HCSParams: " + (FOS_System.String)HCSParams);
        //    HCCParams = *((UInt32*)(CapabilitiesRegAddr + 8));
        //    BasicConsole.WriteLine("HCCParams: " + (FOS_System.String)HCCParams);
        //    HCSPPortRouteDesc = *((UInt64*)(CapabilitiesRegAddr + 12));
        //    BasicConsole.WriteLine("HCSPPortRouteDesc: " + (FOS_System.String)HCSPPortRouteDesc);
            
        //    OpRegAddr = (uint*)(usbBaseAddress + CapabilitiesRegsLength);
        //    BasicConsole.WriteLine("OpRegAddr: " + (FOS_System.String)(uint)OpRegAddr);

        //    LoadExtendedCapabilities();
        //}

        //#region Test Methods

        ///// <summary>
        ///// Performs basic tests with the driver covering initialisation
        ///// and the async queue.
        ///// </summary>
        //public void Test()
        //{
        //    BasicConsole.WriteLine("Testing Init()...");
        //    Init();
        //    BasicConsole.WriteLine("Test passed.");
        //    BasicConsole.WriteLine("Testing EnableAsyncQueue()...");
        //    EnableAsyncQueue();
        //    BasicConsole.WriteLine("Test passed.");
        //    BasicConsole.WriteLine("Testing DisableAsyncQueue()...");
        //    DisableAsyncQueue();
        //    BasicConsole.WriteLine("Test passed.");
        //    BasicConsole.WriteLine("Tests complete.");
        //}

        //#endregion

        //#region Initialization / setup methods

        ///// <summary>
        ///// Initializes the EHCI device.
        ///// </summary>
        //public void Init()
        //{
        //    //Host Controller Initialisation
        //    //See section 4.1 of spec.

        //    //Program CTRLDSSEGMENT   - 64-Utils.BIT segment offset ofr 64-Utils.BIT hardware.
        //    //  - N/A
            
        //    //Write USBINTR           - Write to USBINTR to enable interrupts. 
        //    //  - We are not using any interrupts
            
        //    //Write PERIODICLIST BASE - Base address of the period frames linked-list
        //    //  - We won't be using periodic-based transfers
            
        //    //Write USBCMD            - Set interrupt threshold, frame list size and run/stop Utils.BIT
        //    //  - Interrupt threshold = 0, Frame List Size = 0, Run/Stop = 1
        //    USBCMD = (USBCMD | 0x1);
            
        //    //Write CONFIGFLAG        - Route all ports to EHCI
        //    // - Write 1 = Route all ports to EHCI
        //    CONFIGFLAG = CONFIGFLAG | 0x1;

        //    //From the spec:
        //    /*
        //     * "At this point, the host controller is up and running and the port registers will 
        //     * begin reporting device connects, etc."
        //     */
        //}
        
        //#endregion

        //#region Port Routing and Control methods

        ///*
        // * See section 4.2 of spec
        // */

        ///*
        // * See Init() method - all ports are routed to EHCI initially.
        // * For our simple implementation, we will not use re-routing 
        // * to companion controllers yet nor individual port power 
        // * management.
        // */

        //#endregion

        //#region Async Schedule methods

        ///*
        // * See sections 4.8 and 4.10 of spec.
        // */

        //#region Documentation - Read the comments in this region before proceeding

        ///*
        // * Key point of information (quoted from spec section 4.10, page 79):
        // * 
        // * "One queue head is used to manage the data stream for 
        // *  one endpoint."
        // *  
        // * and
        // * 
        // * "Each qTD [Queue Transfer Descriptor] represents one
        // *  or more bus transactions, which is defined in the 
        // *  context of this specification as a transfer."
        // */

        ///*
        // * Interpretation of the spec:
        // * 
        // * A queue head defines what endpoint to send data to
        // * and points to a qTD. A qTD defines what data to send 
        // * and points to more qTDs in a linked list. Thus, one 
        // * queue head defines the transactions to be sent to a 
        // * given endpoint. 
        // * 
        // * A queue head also has a pointer to another queue head
        // * forming a linked list of queue heads. This linked list 
        // * is circular i.e. the last item points to the first
        // * item. Thus, the queue heads form a list of
        // * endpoints and the data to be sent to them which can be
        // * cycled through by the host controller (HC) during 
        // * (micro)frames to process the transactions.
        // */

        //#endregion

        ////Done
        ///// <summary>
        ///// Waits for the HCI async schedule status and enabled values to match.
        ///// </summary>
        //protected void WaitForAsyncQueueEnabledStatusMatch()
        //{
        //    //BasicConsole.WriteLine("USBCMD: " + (FOS_System.String)USBCMD);
        //    //BasicConsole.WriteLine("USBSTS: " + (FOS_System.String)USBSTS);

        //    //if (AsynchronousScheduleEnabled)
        //    //{
        //    //    BasicConsole.WriteLine("Schedule enabled: true");
        //    //}
        //    //else
        //    //{
        //    //    BasicConsole.WriteLine("Schedule enabled: false");
        //    //}
        //    //if (AsynchronousScheduleStatus)
        //    //{
        //    //    BasicConsole.WriteLine("Schedule status: true");
        //    //}
        //    //else
        //    //{
        //    //    BasicConsole.WriteLine("Schedule status: false");
        //    //}

        //    while (AsynchronousScheduleEnabled != AsynchronousScheduleStatus &&
        //           ASYNCLISTADDR != null)
        //    {
        //        ;
        //    }
        //}
        ///// <summary>
        ///// Enables the async queue and waits for the enable to be acknowledged.
        ///// </summary>
        //protected void EnableAsyncQueue()
        //{
        //    // - Wait for AsyncQueueEnabled and corresponding Status Utils.BIT to match
        //    WaitForAsyncQueueEnabledStatusMatch();
        //    // - Set AsyncQueueEnabled
        //    AsynchronousScheduleEnabled = true;
        //    // - Wait again
        //    WaitForAsyncQueueEnabledStatusMatch();
        //}
        ///// <summary>
        ///// Disables the async queue and waits for the disable to be acknowledged.
        ///// </summary>
        //protected void DisableAsyncQueue()
        //{
        //    // - Wait for AsyncQueueStatus to equal 0 i.e. schedule goes idle
        //    while (AsynchronousScheduleStatus)
        //    {
        //        ;
        //    }
        //    // - Set AsyncQueueEnabled to false
        //    AsynchronousScheduleEnabled = false;
        //}
        
        ///// <summary>
        ///// Adds a queue head to the async queue.
        ///// </summary>
        ///// <param name="theHead">The queue head to add.</param>
        ///// <remarks>
        ///// See EHCI spec section 4.8.1
        ///// </remarks>
        //protected void AddAsyncQueueHead(QueueHead theHead)
        //{
        //    // - Reclaim anything from the queue that we can
        //    // - Check if queue is empty:
        //    //      - If so, create new queue

        //    //Call Reclaim
        //    Reclaim();

        //    //Check if queue activated (/empty)
        //    if (ASYNCLISTADDR == null)
        //    {
        //        //Inactive (/empty) queue
        //        //Set as only item in queue
        //        //(Re-)enable (/activate) queue

        //        ASYNCLISTADDR = theHead.queueHead;
        //        EnableAsyncQueue();
        //    }
        //    else
        //    {
        //        //Active (/not empty) queue
        //        //Insert into the linked list maintaining queue coherency

        //        QueueHead currQueueHead = new QueueHead(ASYNCLISTADDR);
        //        theHead.HorizontalLinkPointer = currQueueHead.HorizontalLinkPointer;
        //        currQueueHead.HorizontalLinkPointer = theHead.queueHead;
        //    }
        //}
        ///// <summary>
        ///// Removes a queue head from the async queue.
        ///// </summary>
        ///// <param name="theHead">The queue head to remove.</param>
        ///// <remarks>
        ///// See EHCI spec section 4.8.2
        ///// </remarks>
        //protected void RemoveAsyncQueueHead(QueueHead theHead)
        //{
        //    //Check if queue is empty: If so, do nothing
        //    //Otherwise:
        //    //Deactivate all qTDs in the queue head
        //    //Wait for queue head to go inactive
        //    //Find prev qHead. 
        //    //If no prev: Last in the list so deactivate async list and remove
        //    //Otherwise:
        //    //Find next qHead
        //    //Check H-Utils.BIT. If set, find another to have H-Utils.BIT set
        //    //Unlink theHead maintaining queue coherency
        //    //Handshake with the host controller to confirm cache release of theHead

        //    //Check if queue is empty
        //    if (ASYNCLISTADDR == null)
        //    {
        //        //If so, do nothing
        //        return;
        //    }

        //    //Deactivate all qTDs in the queue head
        //    if (theHead.NextqTDPointer != null)
        //    {
        //        qTD aqTD = new qTD(theHead.NextqTDPointer);
        //        while (aqTD != null)
        //        {
        //            //Deactivate by setting status to 0
        //            aqTD.Status &= 0x7F;

        //            //Move to next qTD
        //            if (aqTD.NextqTDPointer != null)
        //            {
        //                aqTD.qtd = aqTD.NextqTDPointer;
        //            }
        //            else
        //            {
        //                aqTD = null;
        //            }
        //        }
        //    }

        //    //Wait for queue head to go inactive
        //    while (theHead.Active)
        //    {
        //        ;
        //    }

        //    //Find prev qHead.
        //    QueueHead prevHead = new QueueHead(ASYNCLISTADDR);
        //    while (prevHead.queueHead != null)
        //    {
        //        if (prevHead.HorizontalLinkPointer == theHead.queueHead)
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            prevHead.queueHead = prevHead.HorizontalLinkPointer;
        //        }
        //    }

        //    //If no prev: Last in the list so deactivate async list and remove
        //    if (prevHead.queueHead == null)
        //    {
        //        DisableAsyncQueue();
        //        ASYNCLISTADDR = null;
        //    }
        //    else
        //    {
        //        //Otherwise:
        //        //Find next qHead
        //        QueueHead nextHead = new QueueHead(theHead.HorizontalLinkPointer);

        //        //Check H-Utils.BIT. If set, find another to have H-Utils.BIT set
        //        if (theHead.HeadOfReclamationList)
        //        {
        //            nextHead.HeadOfReclamationList = true;
        //            theHead.HeadOfReclamationList = false;
        //        }

        //        //Unlink theHead maintaining queue coherency
        //        prevHead.HorizontalLinkPointer = theHead.HorizontalLinkPointer;
        //        theHead.HorizontalLinkPointer = nextHead.queueHead;
        //    }

        //    //Handshake with the host controller to confirm cache release of theHead
        //    InterruptOnAsyncAdvanceDoorbell = true;
        //    //TO DO - Use actual interrupts not this ignorant/hacky solution
        //    while(!InterruptOnAsyncAdvance)
        //    {
        //        ;
        //    }
        //    //TO DO : Use reclaimed queue heads
        //    //TO DO : Use reclaimed qTDs
        //    //QueueHeadReclaimList.Add(theHead);

        //    //TO DO : Free qTDs

        //    theHead.Free();
        //}

        ///// <summary>
        ///// Creates a new queue head for the async queue.
        ///// </summary>
        ///// <param name="IsControlEndpoint">Whether the endpoint is a control endpoint or not.</param>
        ///// <param name="MaxPacketLength">The maximum packet length to use with the endpoint.</param>
        ///// <param name="EndpointSpeed">The endpoint speed.</param>
        ///// <param name="EndpointNumber">The endpoint number.</param>
        ///// <param name="DeviceAddress">The address of the device to which the endpoint belongs.</param>
        ///// <returns>The new (or recycled) queue head.</returns>
        //protected QueueHead CreateQueueHead(bool IsControlEndpoint, ushort MaxPacketLength, 
        //                                    byte EndpointSpeed,     byte EndpointNumber,
        //                                    byte DeviceAddress)
        //{
        //    //Check if recycle list not empty:
        //    //  - If so, reinitialise a reclaimed queue head
        //    //Otherwise, create a new queue head and initialise it (to valid empty values?)

        //    //TO DO : Use reclaimed queue heads

        //    QueueHead newHead = new QueueHead();
        //    newHead.ControlEndpointFlag = IsControlEndpoint;
        //    newHead.MaximumPacketLength = MaxPacketLength;
        //    newHead.EndpointSpeed = EndpointSpeed;
        //    newHead.EndpointNumber = EndpointNumber;
        //    newHead.DeviceAddress = DeviceAddress;
            
        //    return newHead;
        //}

        ///// <summary>
        ///// Creates a new queue transfer descriptor.
        ///// </summary>
        ///// <param name="TotalBytesToTransfer">The total number of bytes to transfer from the buffers. Max: 0x5000 (5 pages)</param>
        ///// <param name="ErrorCounter">
        ///// 2-Utils.BIT counter. Valid values: 3, 2, 1 or 0 (except for low or full speed devices in which case 0 is not valid)
        ///// </param>
        ///// <param name="PIDCode">0=OUT Token, 1=IN Token, 2=SETUP Token, 3=(Reserved)</param>
        ///// <param name="Buffer0">4K page-aligned pointer to first buffer of memory.</param>
        ///// <param name="Buffer1">4K page-aligned pointer to second buffer of memory.</param>
        ///// <param name="Buffer2">4K page-aligned pointer to third buffer of memory.</param>
        ///// <param name="Buffer3">4K page-aligned pointer to fourth buffer of memory.</param>
        ///// <param name="Buffer4">4K page-aligned pointer to fifth buffer of memory.</param>
        ///// <param name="NextqTDPtr">32-byte aligned pointer to the next qTD in the list or null ptr (value of 0)</param>
        ///// <returns>The new (or recycled) qTD.</returns>
        //protected qTD CreateqTD(char TotalBytesToTransfer,
        //                        byte ErrorCounter,  byte PIDCode,
        //                        byte* Buffer0,      byte* Buffer1,
        //                        byte* Buffer2,      byte* Buffer3,
        //                        byte* Buffer4,      qTD_Struct* NextqTDPtr)
        //{
        //    //Check if recycle list not empty:
        //    //  - If so, reinitialise a reclaimed qTD
        //    //Otherwise, create a new qTD and initialise it (to valid empty values?)

        //    //TO DO : Use reclaimed qTDs

        //    qTD newqTD = new qTD();
        //    newqTD.ErrorCounter = ErrorCounter;
        //    newqTD.NextqTDPointer = NextqTDPtr;
        //    newqTD.NextqTDPointerTerminate = (uint)NextqTDPtr == 0u;
        //    newqTD.PIDCode = PIDCode;
        //    newqTD.TotalBytesToTransfer = TotalBytesToTransfer;
        //    newqTD.Buffer0 = Buffer0;
        //    newqTD.Buffer1 = Buffer1;
        //    newqTD.Buffer2 = Buffer2;
        //    newqTD.Buffer3 = Buffer3;
        //    newqTD.Buffer4 = Buffer4;

        //    return null;
        //}

        ///// <summary>
        ///// Adds a qTD to a queue head.
        ///// </summary>
        ///// <param name="theqTD">The qTD to add.</param>
        ///// <param name="theQueueHead">The queue head to add to.</param>
        //protected void AddqTDToQueueHead(qTD theqTD, QueueHead theQueueHead)
        //{
        //    //TO DO : Don't add to active queue head
        //    if (theQueueHead.CurrentqTDPointer == null)
        //    {
        //        theQueueHead.NextqTDPointer = theqTD.qtd;
        //        theQueueHead.NextqTDPointerTerminate = false;
        //    }
        //    else
        //    {
        //        qTD currqTD = new qTD(theQueueHead.CurrentqTDPointer);
        //        while (!currqTD.NextqTDPointerTerminate)
        //        {
        //            currqTD.qtd = currqTD.NextqTDPointer;
        //        }
        //        theqTD.NextqTDPointerTerminate = true;
        //        currqTD.NextqTDPointer = theqTD.qtd;
        //        currqTD.NextqTDPointerTerminate = false;
        //    }
        //}

        ////To be done

        ////TO DO - Methods for re-cycling (/re-using) queue heads
        ////       and qTDs to reduce memory allocation load / 
        ////       improve performance
        ///// <summary>
        ///// Recycles a queue head for re-use. Recycling reduces heap memory operations.
        ///// </summary>
        ///// <param name="theQueueHead">The queue head to recycle.</param>
        //protected void RecycleQueueHead(QueueHead theQueueHead)
        //{
        //}
        ///// <summary>
        ///// Recycles a qTD for re-use. Recycling reduces heap memory operations.
        ///// </summary>
        ///// <param name="theqTD">The qTD to recycle.</param>
        //protected void RecycleqTD(qTD theqTD)
        //{
        //}

        ////TO DO - Methods for reclaiming QueueHeads and qTDs
        ///// <summary>
        ///// Reclaims queue heads and qTDs from the async transfer queue.
        ///// </summary>
        //protected void Reclaim()
        //{
        //}
        
        //#endregion
        
        //#region Control Transfer methods

        ///*
        // * See section 4.10 of spec.
        // *      and read the USB spec
        // */

        //#endregion

        //#region Bulk Transfer methods

        ///*
        // * See section 4.10 of spec.
        // *      and read the USB spec
        // */

        //#endregion

        //#region Interrupts

        ///*
        // * See section 4.15 of spec
        // */

        //#endregion

        #endregion

        protected bool AnyPortsChanged = false;
        protected bool EnabledPortFlag = false;
        protected int USBIntCount = 0;
        protected EHCI_QueueHead_Struct* IdleQueueHead = null;
        protected EHCI_QueueHead_Struct* TailQueueHead = null;

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

        public override void Update()
        {
            if (AnyPortsChanged)
            {
                PortCheck();
            }
        }

        protected void Start()
        {
            InitHC();
            ResetHC();
            StartHC();
            InitializeAsyncScheduler();

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
        protected void InitHC()
        {
            pciDevice.Command = pciDevice.Command | PCI.PCIDevice.PCICommand.Memory | PCI.PCIDevice.PCICommand.Master;

#if EHCI_TRACE
            DBGMSG("Hooking IRQ...");
#endif
            //Setup InterruptHandler (IRQ number = PCIDevice.InterruptLine)
            Interrupts.Interrupts.SetIRQHandler(pciDevice.InterruptLine, EHCI.InterruptHandler, this);
#if EHCI_TRACE
            DBGMSG("Hooked IRQ.");
#endif
        }
        protected void StartHC()
        {
            DeactivateLegacySupport();
            CTRLDSSEGMENT = 0u;
            USBSTS = 0u; //Will this ever have any effect? According to spec, only writing bits set to 1 will have an effect??
            USBINTR = EHCI_Consts.STS_ASYNC_INT | EHCI_Consts.STS_HOST_SYSTEM_ERROR | EHCI_Consts.STS_PORT_CHANGE | 
                      EHCI_Consts.STS_USBINT | EHCI_Consts.STS_USBERRINT;
            if (HCHalted)
            {
                USBCMD |= EHCI_Consts.CMD_RUN_STOP; //Set run-stop bit
            }

            //This can only be set when HCHalted != 0  !!!
            USBCMD |= EHCI_Consts.CMD_8_MICROFRAME; //InterruptThresholdControl = 8 Microframes (1ms).
            
            CONFIGFLAG = EHCI_Consts.CF; //Set port routing to route all ports to EHCI

            //Is this delay necessary? If so, why?
            for (int i = 1000000; i > 0; i--)
            {
                ;
            }
        }
        protected void ResetHC()
        {
            USBCMD &= ~EHCI_Consts.CMD_RUN_STOP; //Clear run-stop bit

            //Wait for halt
            while (!HCHalted)
            {
                //Sleep for a bit
                for (int i = 0; i < 100000; i++)
                    ;
            }

            USBCMD |= EHCI_Consts.CMD_HCRESET; //Set reset bit

            int timeout = 30;
            while ((USBCMD & EHCI_Consts.CMD_HCRESET) != 0) // Reset-bit still set to 1
            {
                for (int i = 0; i < 100000; i++)
                    ;

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
                        for(int i = 0; i < 100000; i++)
                            ;
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
                            for(int i = 0; i < 100000; i++)
                                ;
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
            EnabledPortFlag = true;
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
        protected void ResetPort(byte portNum)
        {
            PORTSC[portNum] |= EHCI_Consts.PSTS_POWERON;
            PORTSC[portNum] &= ~EHCI_Consts.PSTS_ENABLED;
            USBSTS |= ~EHCI_Consts.STS_PORT_CHANGE;
            if (HCHalted)
            {
                ExceptionMethods.Throw(new FOS_System.Exception("EHCI.ResetPort(): HCHalted not zero!"));
            }
            USBINTR = 0;
            PORTSC[portNum] |= EHCI_Consts.PSTS_PORT_RESET;

            //~200ms
            for (int i = 0; i < 2000000; i++)
                ;

            PORTSC[portNum] &= ~EHCI_Consts.PSTS_PORT_RESET;


            // wait and check, whether really zero
            uint timeout = 50;
            while ((PORTSC[portNum] & EHCI_Consts.PSTS_PORT_RESET) != 0)
            {
                // ~1ms
                for (int i = 0; i < 10000; i++)
                    ;

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
            
            USBINTR = EHCI_Consts.STS_ASYNC_INT | EHCI_Consts.STS_HOST_SYSTEM_ERROR | EHCI_Consts.STS_PORT_CHANGE |
                      EHCI_Consts.STS_USBINT | EHCI_Consts.STS_USBERRINT;

            //~20ms
            for (int i = 0; i < 200000; i++)
                ;
        }
        protected static void InterruptHandler(FOS_System.Object data)
        {
            ((EHCI)data).InterruptHandler();
        }
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

#if EHCI_TRACE
            if((val & EHCI_Consts.STS_USBERRINT) != 0u)
            {
                USBIntCount--;

                DBGMSG("USB Error Interrupt!");
            }
#endif

            if ((val & EHCI_Consts.STS_PORT_CHANGE) != 0u)
            {
                if (EnabledPortFlag && pciDevice != null)
                {
                    AnyPortsChanged = true;
                }
            }


            if ((val & EHCI_Consts.STS_HOST_SYSTEM_ERROR) != 0u)
            {
                Start();
            }

            if ((val & EHCI_Consts.STS_USBINT) != 0u)
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
        protected void PortCheck()
        {
            AnyPortsChanged = false;
            for (byte j = 0; j < RootPortCount; j++)
            {
                if ((PORTSC[j] & EHCI_Consts.PSTS_CONNECTED_CHANGE) != 0)
                {
                    PORTSC[j] |= EHCI_Consts.PSTS_CONNECTED_CHANGE; // reset interrupt
                    if ((PORTSC[j] & EHCI_Consts.PSTS_CONNECTED) != 0)
                    {
                        CheckPortLineStatus(j);
                    }
                    else
                    {
                        PORTSC[j] &= ~EHCI_Consts.PSTS_COMPANION_HC_OWNED; // port is given back to the EHCI

                        if (((HCPort)RootPorts[j]).deviceInfo != null)
                        {
                            ((HCPort)RootPorts[j]).deviceInfo.FreePort();
                        }

                    }
                }
            }
            AnyPortsChanged = false;
        }
        protected void CheckPortLineStatus(byte portNum)
        {
            if ((PORTSC[portNum] & EHCI_Consts.PSTS_CONNECTED) == 0)
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
                    PORTSC[portNum] |= EHCI_Consts.PSTS_COMPANION_HC_OWNED; // release it to the cHC
                    break;
                case 0: // SE0
                case 2: // J-state
                case 3: // undefined
                    DetectDevice(portNum);
                    break;
            }
        }
        protected void DetectDevice(byte portNum)
        {
#if EHCI_TRACE
            DBGMSG("Detecting device...");
#endif
            ResetPort(portNum);
#if EHCI_TRACE
            DBGMSG("Reset port.");
#endif
            if (EnabledPortFlag && ((PORTSC[portNum] & EHCI_Consts.PSTS_POWERON) != 0)) // power on
            {
#if EHCI_TRACE
                DBGMSG("Device powered on.");
#endif
                if ((PORTSC[portNum] & EHCI_Consts.PSTS_ENABLED) != 0) // High speed
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
                    PORTSC[portNum] |= EHCI_Consts.PSTS_COMPANION_HC_OWNED; // release it to the cHC
                }
            }
#if EHCI_TRACE
            DBGMSG("End DetectDevice()");
#endif
        }
        protected override void _SetupTransfer(USBTransfer transfer)
        {
            transfer.data = (EHCI_QueueHead_Struct*)FOS_System.Heap.AllocZeroed((uint)sizeof(EHCI_QueueHead_Struct), 32);
        }
        protected override void _SETUPTransaction(USBTransfer transfer, USBTransaction uTransaction, bool toggle, ushort tokenBytes,
                                           byte type, byte req, byte hiVal, byte loVal, ushort index, ushort length)
        {
            EHCITransaction eTransaction = new EHCITransaction();
            uTransaction.data = eTransaction;
            eTransaction.inBuffer = null;
            eTransaction.inLength = 0u;
            fixed(void** bufferPtr = &(eTransaction.qTDBuffer))
            {
                eTransaction.qTD = CreateQTD_SETUP((EHCI_qTD_Struct*)1u, toggle, tokenBytes, type, req, hiVal, loVal, index, length, bufferPtr).qtd;
            }
            if (transfer.transactions.Count > 0)
            {
                EHCITransaction eLastTransaction = (EHCITransaction)((USBTransaction)(transfer.transactions[transfer.transactions.Count - 1])).data;
                EHCI_qTD lastQTD = new EHCI_qTD(eLastTransaction.qTD);
                lastQTD.NextqTDPointer = eTransaction.qTD;
                lastQTD.NextqTDPointerTerminate = false;
            }
        }
        protected override void _INTransaction(USBTransfer transfer, USBTransaction uTransaction, bool toggle, void* buffer, ushort length)
        {
            EHCITransaction eTransaction = new EHCITransaction();
            uTransaction.data = eTransaction;
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
                EHCITransaction eLastTransaction = (EHCITransaction)((USBTransaction)(transfer.transactions[transfer.transactions.Count - 1])).data;
                EHCI_qTD lastQTD = new EHCI_qTD(eLastTransaction.qTD);
                lastQTD.NextqTDPointer = eTransaction.qTD;
                lastQTD.NextqTDPointerTerminate = false;
            }
        }
        protected override void _OUTTransaction(USBTransfer transfer, USBTransaction uTransaction, bool toggle, void* buffer, ushort length)
        {
            EHCITransaction eTransaction = new EHCITransaction();
            uTransaction.data = eTransaction;
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
                EHCITransaction eLastTransaction = (EHCITransaction)((USBTransaction)(transfer.transactions[transfer.transactions.Count - 1])).data;
                EHCI_qTD lastQTD = new EHCI_qTD(eLastTransaction.qTD);
                lastQTD.NextqTDPointer = eTransaction.qTD;
                lastQTD.NextqTDPointerTerminate = false;
            }
        }
        protected override void _IssueTransfer(USBTransfer transfer)
        {
            EHCITransaction lastTransaction = (EHCITransaction)((USBTransaction)transfer.transactions[transfer.transactions.Count - 1]).data;
            EHCI_qTD lastQTD = new EHCI_qTD(lastTransaction.qTD);
            lastQTD.InterruptOnComplete = true;

#if EHCI_TRACE
            //Test walking the transaction tree
            bool treeOK = true;
            for (int k = 0; k < transfer.transactions.Count - 1; k++)
            {
                EHCITransaction transaction1 = (EHCITransaction)((USBTransaction)transfer.transactions[k]).data;
                EHCITransaction transaction2 = (EHCITransaction)((USBTransaction)transfer.transactions[k + 1]).data;
                EHCI_qTD qtd1 = new EHCI_qTD(transaction1.qTD);
                treeOK = treeOK && (qtd1.NextqTDPointer == transaction2.qTD) && !qtd1.NextqTDPointerTerminate;
            }
            {
                treeOK = treeOK && lastQTD.NextqTDPointerTerminate;
            }
            DBGMSG(((FOS_System.String)"Transfer transactions tree OK: ") + treeOK);
            BasicConsole.DelayOutput(10);
#endif            

            EHCITransaction firstTransaction = (EHCITransaction)((USBTransaction)(transfer.transactions[0])).data;
            CreateQH((EHCI_QueueHead_Struct*)transfer.data, (uint)transfer.data, firstTransaction.qTD, false, transfer.device.num, transfer.endpoint, transfer.packetSize);
            
            for (byte i = 0; i < EHCI_Consts.NUMBER_OF_EHCI_ASYNCLIST_RETRIES && !transfer.success; i++)
            {
#if EHCI_TRACE
                transfer.success = true;
                for (int k = 0; k < transfer.transactions.Count; k++)
                {
                    EHCITransaction transaction = (EHCITransaction)((USBTransaction)transfer.transactions[k]).data;
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

                if (transfer.type == USBTransferType.USB_CONTROL)
                {
                    AddToAsyncScheduler(transfer, 0);
                }
                else
                {
                    AddToAsyncScheduler(transfer, (byte)(1 + transfer.packetSize / 200));
                }

                transfer.success = true;
                for (int k = 0; k < transfer.transactions.Count; k++)
                {
                    EHCITransaction transaction = (EHCITransaction)((USBTransaction)transfer.transactions[k]).data;
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

            FOS_System.Heap.Free(transfer.data);
            for (int k = 0; k < transfer.transactions.Count; k++)
            {
                EHCITransaction transaction = (EHCITransaction)((USBTransaction)transfer.transactions[k]).data;

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

        protected void InitializeAsyncScheduler()
        {
            if (IdleQueueHead == null)
            {
                IdleQueueHead = TailQueueHead = new EHCI_QueueHead().queueHead;
            }
            CreateQH(IdleQueueHead, (uint)IdleQueueHead, null, true, 0, 0, 0);
            ASYNCLISTADDR = IdleQueueHead;
            EnableAsyncScheduler();
        }
        protected void EnableAsyncScheduler()
        {
            USBCMD |= EHCI_Consts.CMD_ASYNCH_ENABLE;

            uint timeout = 7;
            while ((USBSTS & EHCI_Consts.STS_ASYNC_ENABLED) == 0) // wait until it is really on
            {
                timeout--;
                if (timeout>0)
                {
                    //~10ms
                    for (int i = 0; i < 100000; i++)
                        ;
                }
                else
                {
                    ExceptionMethods.Throw(new FOS_System.Exception("EHCI.EnableAsyncScheduler(): Timeout Error - STS_ASYNC_ENABLED not set!"));
                    break;
                }
            }
        }

        protected EHCI_qTD CreateQTD_SETUP(EHCI_qTD_Struct* next, bool toggle, ushort tokenBytes, byte type, byte req,
                                                 byte hiVal, byte loVal, ushort index, ushort length, void** buffer)
        {
            EHCI_qTD td = allocQTD(next);

            td.PIDCode = EHCI_QHConsts.SETUP;      // SETUP = 2
            td.TotalBytesToTransfer = tokenBytes; // dependent on transfer
            td.DataToggle = toggle;     // Should be toggled every list entry

                                                                     //PAGESIZE
            //Transaction Buffer0
            USBRequest* request = (USBRequest*)(*buffer = allocQTDbuffer(td, (uint)sizeof(USBRequest)));
            request->type = type;
            request->request = req;
            request->valueHi = hiVal;
            request->valueLo = loVal;
            request->index = index;
            request->length = length;
            
            return td;
        }
        protected static EHCI_qTD allocQTD(EHCI_qTD_Struct* next)
        {
            EHCI_qTD newQTD = new EHCI_qTD();

            if ((uint)next != 0x1)
            {
                newQTD.NextqTDPointerTerminate = false;
                newQTD.NextqTDPointer = next;
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
        protected static void* allocQTDbuffer(EHCI_qTD td, uint actualSizeToAlloc)
        {
            td.Buffer0 = (byte*)FOS_System.Heap.AllocZeroed(0x1000u, 0x1000u);
            td.CurrentPage = 0;
            td.CurrentOffset = 0;
            return td.Buffer0;
        }
        protected EHCI_qTD CreateQTD_IO(EHCI_qTD_Struct* next, byte direction, bool toggle, ushort tokenBytes, void** buffer, uint bufferSize)
        {
            EHCI_qTD td = allocQTD(next);

            td.PIDCode = direction;
            td.TotalBytesToTransfer = tokenBytes; // dependent on transfer
            td.DataToggle = toggle;     // Should be toggled every list entry

            *buffer = allocQTDbuffer(td, bufferSize);

            return td;
        }

        protected void CreateQH(EHCI_QueueHead_Struct* headPtr, uint horizPtr, EHCI_qTD_Struct* firstQTD, bool H, byte device, 
                                   byte endpoint, ushort packetSize)
        {
            EHCI_QueueHead head = new EHCI_QueueHead(headPtr);
            // bit 31:5 Horizontal Link Pointer
            head.HorizontalLinkPointer = (EHCI_QueueHead_Struct*)(horizPtr);
            head.Type = 0x1;      // type:  00b iTD,   01b QH,   10b siTD,   11b FSTN
            head.Terminate = false; // T-Bit: is set to zero
            head.DeviceAddress = device;         // The device address
            head.InactiveOnNextTransaction = false;
            head.EndpointNumber = endpoint;       // endpoint 0 contains Device infos such as name
            head.EndpointSpeed = 2;              // 00b = full speed; 01b = low speed; 10b = high speed
            head.DataToggleControl = true;              // get the Data Toggle bit out of the included qTD
            head.HeadOfReclamationList = H;              // mark a queue head as being the head of the reclaim list
            head.MaximumPacketLength = packetSize;     // 64 byte for a control transfer to a high speed device
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
                head.NextqTDPointer = firstQTD;
                head.NextqTDPointerTerminate = false;
            }
        }
        protected void AddToAsyncScheduler(USBTransfer transfer, byte velocity)
        {
            USBIntCount = 1;
            
            if ((USBSTS & EHCI_Consts.STS_ASYNC_ENABLED) == 0)
            {
                EnableAsyncScheduler(); // Start async scheduler, when it is not running
            }
            //USBCMD |= EHCI_Consts.CMD_ASYNCH_INT_DOORBELL; // Activate Doorbell: We would like to receive an asynchronous schedule interrupt

            EHCI_QueueHead oldTailQH = new EHCI_QueueHead(TailQueueHead); // save old tail QH
            TailQueueHead = (EHCI_QueueHead_Struct*)transfer.data; // new QH will now be end of Queue

            EHCI_QueueHead idleQH = new EHCI_QueueHead(IdleQueueHead);
            EHCI_QueueHead tailQH = new EHCI_QueueHead(TailQueueHead);
            tailQH.HorizontalLinkPointer = IdleQueueHead; // Create ring. Link new QH with idleQH (always head of Queue)
            tailQH.Type = 1;
            oldTailQH.HorizontalLinkPointer = TailQueueHead; // Insert qh to Queue as element behind old queue head
            oldTailQH.Type = 1;

            //int timeout = 10 * velocity + 25; // Wait up to 250+100*velocity milliseconds for USBasyncIntFlag to be set
            while (USBIntCount > 0 /*&& timeout > 0*/)
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

            idleQH.HorizontalLinkPointer = IdleQueueHead; // Restore link of idleQH to idleQH (endless loop)
            idleQH.Type = 0x1;
            TailQueueHead = IdleQueueHead; // qh done. idleQH is end of Queue again (ring structure of asynchronous schedule)
        }

#if EHCI_TRACE
        internal static void DBGMSG(FOS_System.String msg)
        {
            BasicConsole.WriteLine(msg);
        }
#endif

#if EHCI_TRACE
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
    public unsafe class EHCITransaction : HCTransaction
    {
        public EHCI_qTD_Struct* qTD;
        public void* qTDBuffer;
        public void* inBuffer;
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
