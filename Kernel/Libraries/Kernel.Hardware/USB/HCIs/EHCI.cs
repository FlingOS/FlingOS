#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
using System;
using Kernel.FOS_System.Collections;

namespace Kernel.Hardware.USB.HCIs
{
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

        protected List QueueHeadReclaimList;

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
        /// <summary>
        /// The EHCI USB Legacy Support extended capabilities information if it exists.
        /// </summary>
        protected EHCI_USBLegacySupportExtendedCapability USBLegacySupportExtendedCapability;
        
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

        //TODO - Add HCS fields

        #endregion

        #region From HCC Params

        /*
         *  See section 2.2.4 of spec.
         */

        //TODO - Add remaining HCC fields
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
                return *(OpRegAddr + 2);
            }
            set
            {
                *(OpRegAddr + 2) = value;
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
        protected QueueHead_Struct* ASYNCLISTADDR
        {
            get
            {
                return (QueueHead_Struct*)*(OpRegAddr + 6);
            }
            set
            {
                *(OpRegAddr + 6) = (uint)value;
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
        protected uint PORTSC
        {
            get
            {
                return *(OpRegAddr + 17);
            }
            set
            {
                *(OpRegAddr + 17) = value;
            }
        }
        
        #endregion

        #endregion
        
        /// <summary>
        /// Initializes a new EHCI device.
        /// </summary>
        /// <param name="aPCIDevice">The PCI device that represents the EHCI device.</param>
        public EHCI(PCI.PCIDeviceNormal aPCIDevice)
            : base(aPCIDevice)
        {
            usbBaseAddress = pciDevice.BaseAddresses[0].BaseAddress();
            CapabilitiesRegAddr = usbBaseAddress;
            BasicConsole.WriteLine("CapabilitiesRegAddr: " + (FOS_System.String)(uint)CapabilitiesRegAddr);
            
            SBRN = pciDevice.ReadRegister8(0x60);
            BasicConsole.WriteLine("SBRN: " + (FOS_System.String)SBRN);
            
            CapabilitiesRegsLength = *CapabilitiesRegAddr;
            BasicConsole.WriteLine("CapabilitiesRegsLength: " + (FOS_System.String)CapabilitiesRegsLength);
            HCIVersion = *((UInt16*)(CapabilitiesRegAddr + 2));
            BasicConsole.WriteLine("HCIVersion: " + (FOS_System.String)HCIVersion);
            HCSParams = *((UInt32*)(CapabilitiesRegAddr + 4));
            BasicConsole.WriteLine("HCSParams: " + (FOS_System.String)HCSParams);
            HCCParams = *((UInt32*)(CapabilitiesRegAddr + 8));
            BasicConsole.WriteLine("HCCParams: " + (FOS_System.String)HCCParams);
            HCSPPortRouteDesc = *((UInt64*)(CapabilitiesRegAddr + 12));
            BasicConsole.WriteLine("HCSPPortRouteDesc: " + (FOS_System.String)HCSPPortRouteDesc);
            
            OpRegAddr = (uint*)(usbBaseAddress + CapabilitiesRegsLength);
            BasicConsole.WriteLine("OpRegAddr: " + (FOS_System.String)(uint)OpRegAddr);

            LoadExtendedCapabilities();
        }

        #region Test Methods

        /// <summary>
        /// Performs basic tests with the driver covering initialisation
        /// and the async queue.
        /// </summary>
        public void Test()
        {
            BasicConsole.WriteLine("Testing Init()...");
            Init();
            BasicConsole.WriteLine("Test passed.");
            BasicConsole.WriteLine("Testing EnableAsyncQueue()...");
            EnableAsyncQueue();
            BasicConsole.WriteLine("Test passed.");
            BasicConsole.WriteLine("Testing DisableAsyncQueue()...");
            DisableAsyncQueue();
            BasicConsole.WriteLine("Test passed.");
            BasicConsole.WriteLine("Tests complete.");
        }

        #endregion

        #region Initialization / setup methods

        /// <summary>
        /// Initializes the EHCI device.
        /// </summary>
        public void Init()
        {
            //Host Controller Initialisation
            //See section 4.1 of spec.

            //Program CTRLDSSEGMENT   - 64-bit segment offset ofr 64-bit hardware.
            //  - N/A
            
            //Write USBINTR           - Write to USBINTR to enable interrupts. 
            //  - We are not using any interrupts
            
            //Write PERIODICLIST BASE - Base address of the period frames linked-list
            //  - We won't be using periodic-based transfers
            
            //Write USBCMD            - Set interrupt threshold, frame list size and run/stop bit
            //  - Interrupt threshold = 0, Frame List Size = 0, Run/Stop = 1
            USBCMD = (USBCMD | 0x1);
            
            //Write CONFIGFLAG        - Route all ports to EHCI
            // - Write 1 = Route all ports to EHCI
            CONFIGFLAG = CONFIGFLAG | 0x1;

            //From the spec:
            /*
             * "At this point, the host controller is up and running and the port registers will 
             * begin reporting device connects, etc."
             */
        }
        
        #endregion

        #region Port Routing and Control methods

        /*
         * See section 4.2 of spec
         */

        /*
         * See Init() method - all ports are routed to EHCI initially.
         * For our simple implementation, we will not use re-routing 
         * to companion controllers yet nor individual port power 
         * management.
         */

        #endregion

        #region Async Schedule methods

        /*
         * See sections 4.8 and 4.10 of spec.
         */

        #region Documentation - Read the comments in this region before proceeding

        /*
         * Key point of information (quoted from spec section 4.10, page 79):
         * 
         * "One queue head is used to manage the data stream for 
         *  one endpoint."
         *  
         * and
         * 
         * "Each qTD [Queue Transfer Descriptor] represents one
         *  or more bus transactions, which is defined in the 
         *  context of this specification as a transfer."
         */

        /*
         * Interpretation of the spec:
         * 
         * A queue head defines what endpoint to send data to
         * and points to a qTD. A qTD defines what data to send 
         * and points to more qTDs in a linked list. Thus, one 
         * queue head defines the transactions to be sent to a 
         * given endpoint. 
         * 
         * A queue head also has a pointer to another queue head
         * forming a linked list of queue heads. This linked list 
         * is circular i.e. the last item points to the first
         * item. Thus, the queue heads form a list of
         * endpoints and the data to be sent to them which can be
         * cycled through by the host controller (HC) during 
         * (micro)frames to process the transactions.
         */

        #endregion

        //Done
        /// <summary>
        /// Waits for the HCI async schedule status and enabled values to match.
        /// </summary>
        protected void WaitForAsyncQueueEnabledStatusMatch()
        {
            //BasicConsole.WriteLine("USBCMD: " + (FOS_System.String)USBCMD);
            //BasicConsole.WriteLine("USBSTS: " + (FOS_System.String)USBSTS);

            //if (AsynchronousScheduleEnabled)
            //{
            //    BasicConsole.WriteLine("Schedule enabled: true");
            //}
            //else
            //{
            //    BasicConsole.WriteLine("Schedule enabled: false");
            //}
            //if (AsynchronousScheduleStatus)
            //{
            //    BasicConsole.WriteLine("Schedule status: true");
            //}
            //else
            //{
            //    BasicConsole.WriteLine("Schedule status: false");
            //}

            while (AsynchronousScheduleEnabled != AsynchronousScheduleStatus &&
                   ASYNCLISTADDR != null)
            {
                ;
            }
        }
        /// <summary>
        /// Enables the async queue and waits for the enable to be acknowledged.
        /// </summary>
        protected void EnableAsyncQueue()
        {
            // - Wait for AsyncQueueEnabled and corresponding Status bit to match
            WaitForAsyncQueueEnabledStatusMatch();
            // - Set AsyncQueueEnabled
            AsynchronousScheduleEnabled = true;
            // - Wait again
            WaitForAsyncQueueEnabledStatusMatch();
        }
        /// <summary>
        /// Disables the async queue and waits for the disable to be acknowledged.
        /// </summary>
        protected void DisableAsyncQueue()
        {
            // - Wait for AsyncQueueStatus to equal 0 i.e. schedule goes idle
            while (AsynchronousScheduleStatus)
            {
                ;
            }
            // - Set AsyncQueueEnabled to false
            AsynchronousScheduleEnabled = false;
        }
        
        /// <summary>
        /// Adds a queue head to the async queue.
        /// </summary>
        /// <param name="theHead">The queue head to add.</param>
        /// <remarks>
        /// See EHCI spec section 4.8.1
        /// </remarks>
        protected void AddAsyncQueueHead(QueueHead theHead)
        {
            // - Reclaim anything from the queue that we can
            // - Check if queue is empty:
            //      - If so, create new queue

            //Call Reclaim
            Reclaim();

            //Check if queue activated (/empty)
            if (ASYNCLISTADDR == null)
            {
                //Inactive (/empty) queue
                //Set as only item in queue
                //(Re-)enable (/activate) queue

                ASYNCLISTADDR = theHead.queueHead;
                EnableAsyncQueue();
            }
            else
            {
                //Active (/not empty) queue
                //Insert into the linked list maintaining queue coherency

                QueueHead currQueueHead = new QueueHead(ASYNCLISTADDR);
                theHead.HorizontalLinkPointer = currQueueHead.HorizontalLinkPointer;
                currQueueHead.HorizontalLinkPointer = theHead.queueHead;
            }
        }
        /// <summary>
        /// Removes a queue head from the async queue.
        /// </summary>
        /// <param name="theHead">The queue head to remove.</param>
        /// <remarks>
        /// See EHCI spec section 4.8.2
        /// </remarks>
        protected void RemoveAsyncQueueHead(QueueHead theHead)
        {
            //Check if queue is empty: If so, do nothing
            //Otherwise:
            //Deactivate all qTDs in the queue head
            //Wait for queue head to go inactive
            //Find prev qHead. 
            //If no prev: Last in the list so deactivate async list and remove
            //Otherwise:
            //Find next qHead
            //Check H-bit. If set, find another to have H-bit set
            //Unlink theHead maintaining queue coherency
            //Handshake with the host controller to confirm cache release of theHead

            //Check if queue is empty
            if (ASYNCLISTADDR == null)
            {
                //If so, do nothing
                return;
            }

            //Deactivate all qTDs in the queue head
            if (theHead.NextqTDPointer != null)
            {
                qTD aqTD = new qTD(theHead.NextqTDPointer);
                while (aqTD != null)
                {
                    //Deactivate by setting status to 0
                    aqTD.Status &= 0x7F;

                    //Move to next qTD
                    if (aqTD.NextqTDPointer != null)
                    {
                        aqTD.qtd = aqTD.NextqTDPointer;
                    }
                    else
                    {
                        aqTD = null;
                    }
                }
            }

            //Wait for queue head to go inactive
            while (theHead.Active)
            {
                ;
            }

            //Find prev qHead.
            QueueHead prevHead = new QueueHead(ASYNCLISTADDR);
            while (prevHead.queueHead != null)
            {
                if (prevHead.HorizontalLinkPointer == theHead.queueHead)
                {
                    break;
                }
                else
                {
                    prevHead.queueHead = prevHead.HorizontalLinkPointer;
                }
            }

            //If no prev: Last in the list so deactivate async list and remove
            if (prevHead.queueHead == null)
            {
                DisableAsyncQueue();
                ASYNCLISTADDR = null;
            }
            else
            {
                //Otherwise:
                //Find next qHead
                QueueHead nextHead = new QueueHead(theHead.HorizontalLinkPointer);

                //Check H-bit. If set, find another to have H-bit set
                if (theHead.HeadOfReclamationList)
                {
                    nextHead.HeadOfReclamationList = true;
                    theHead.HeadOfReclamationList = false;
                }

                //Unlink theHead maintaining queue coherency
                prevHead.HorizontalLinkPointer = theHead.HorizontalLinkPointer;
                theHead.HorizontalLinkPointer = nextHead.queueHead;
            }

            //Handshake with the host controller to confirm cache release of theHead
            InterruptOnAsyncAdvanceDoorbell = true;
            //TODO - Use actual interrupts not this ignorant/hacky solution
            while(!InterruptOnAsyncAdvance)
            {
                ;
            }
            //TODO : Use reclaimed queue heads
            //QueueHeadReclaimList.Add(theHead);
            theHead.Free();
        }

        /// <summary>
        /// Creates a new queue head for the async queue.
        /// </summary>
        /// <param name="IsControlEndpoint">Whether the endpoint is a control endpoint or not.</param>
        /// <param name="MaxPacketLength">The maximum packet length to use with the endpoint.</param>
        /// <param name="EndpointSpeed">The endpoint speed.</param>
        /// <param name="EndpointNumber">The endpoint number.</param>
        /// <param name="DeviceAddress">The address of the device to which the endpoint belongs.</param>
        /// <returns>The new (or recycled) queue head.</returns>
        protected QueueHead CreateQueueHead(bool IsControlEndpoint, ushort MaxPacketLength, 
                                            byte EndpointSpeed,     byte EndpointNumber,
                                            byte DeviceAddress)
        {
            //Check if recycle list not empty:
            //  - If so, reinitialise a reclaimed queue head
            //Otherwise, create a new queue head and initialise it (to valid empty values?)

            //TODO : Use reclaimed queue heads

            QueueHead newHead = new QueueHead();
            newHead.ControlEndpointFlag = IsControlEndpoint;
            newHead.MaximumPacketLength = MaxPacketLength;
            newHead.EndpointSpeed = EndpointSpeed;
            newHead.EndpointNumber = EndpointNumber;
            newHead.DeviceAddress = DeviceAddress;
            
            return newHead;
        }

        /// <summary>
        /// Creates a new queue transfer descriptor.
        /// </summary>
        /// <param name="TotalBytesToTransfer">The total number of bytes to transfer from the buffers. Max: 0x5000 (5 pages)</param>
        /// <param name="ErrorCounter">
        /// 2-bit counter. Valid values: 3, 2, 1 or 0 (except for low or full speed devices in which case 0 is not valid)
        /// </param>
        /// <param name="PIDCode">0=OUT Token, 1=IN Token, 2=SETUP Token, 3=(Reserved)</param>
        /// <param name="Buffer0">4K page-aligned pointer to first buffer of memory.</param>
        /// <param name="Buffer1">4K page-aligned pointer to second buffer of memory.</param>
        /// <param name="Buffer2">4K page-aligned pointer to third buffer of memory.</param>
        /// <param name="Buffer3">4K page-aligned pointer to fourth buffer of memory.</param>
        /// <param name="Buffer4">4K page-aligned pointer to fifth buffer of memory.</param>
        /// <param name="NextqTDPtr">32-byte aligned pointer to the next qTD in the list or null ptr (value of 0)</param>
        /// <returns>The new (or recycled) qTD.</returns>
        protected qTD CreateqTD(char TotalBytesToTransfer,
                                byte ErrorCounter,  byte PIDCode,
                                byte* Buffer0,      byte* Buffer1,
                                byte* Buffer2,      byte* Buffer3,
                                byte* Buffer4,      qTD_Struct* NextqTDPtr)
        {
            //Check if recycle list not empty:
            //  - If so, reinitialise a reclaimed qTD
            //Otherwise, create a new qTD and initialise it (to valid empty values?)

            //TODO: Use reclaimed qTDs

            qTD newqTD = new qTD();
            newqTD.ErrorCounter = ErrorCounter;
            newqTD.NextqTDPointer = NextqTDPtr;
            newqTD.NextqTDPointerTerminate = (uint)NextqTDPtr == 0u;
            newqTD.PIDCode = PIDCode;
            newqTD.TotalBytesToTransfer = TotalBytesToTransfer;
            newqTD.Buffer0 = Buffer0;
            newqTD.Buffer1 = Buffer1;
            newqTD.Buffer2 = Buffer2;
            newqTD.Buffer3 = Buffer3;
            newqTD.Buffer4 = Buffer4;

            return null;
        }

        //To be done

        /// <summary>
        /// Adds a qTD to a queue head.
        /// </summary>
        /// <param name="theqTD">The qTD to add.</param>
        /// <param name="theQueueHead">The queue head to add to.</param>
        protected void AddqTDToQueueHead(qTD theqTD, QueueHead theQueueHead)
        {
        }
        /// <summary>
        /// Removes a qTD from a queue head.
        /// </summary>
        /// <param name="theqTD">The qTD to remove.</param>
        /// <param name="theQueueHead">The queue head to remove from.</param>
        protected void RemoveqTDFromQueueHead(qTD theqTD, QueueHead theQueueHead)
        {
            //TODO: Free buffer memory
        }

        //TODO - Methods for re-cycling (/re-using) queue heads
        //       and qTDs to reduce memory allocation load / 
        //       improve performance
        /// <summary>
        /// Recycles a queue head for re-use. Recycling reduces heap memory operations.
        /// </summary>
        /// <param name="theQueueHead">The queue head to recycle.</param>
        protected void RecycleQueueHead(QueueHead theQueueHead)
        {
        }
        /// <summary>
        /// Recycles a qTD for re-use. Recycling reduces heap memory operations.
        /// </summary>
        /// <param name="theqTD">The qTD to recycle.</param>
        protected void RecycleqTD(qTD theqTD)
        {
        }

        //TODO - Methods for reclaiming QueueHeads and qTDs
        /// <summary>
        /// Reclaims queue heads and qTDs from the async transfer queue.
        /// </summary>
        protected void Reclaim()
        {
        }
        
        #endregion
        
        #region Control Transfer methods

        /*
         * See section 4.10 of spec.
         *      and read the USB spec
         */

        #endregion

        #region Bulk Transfer methods

        /*
         * See section 4.10 of spec.
         *      and read the USB spec
         */

        #endregion

        #region Interrupts

        /*
         * See section 4.15 of spec
         */

        #endregion

        /// <summary>
        /// Loads the extended capabilities PCI registers.
        /// </summary>
        private void LoadExtendedCapabilities()
        {
            byte aEECP = EECP;
            if (aEECP > 0)
            {
                byte capID = *(usbBaseAddress + aEECP);
                if (capID == 0x1)
                {
                    /* USB Legacy support extended capability */
                    USBLegacySupportExtendedCapability = new EHCI_USBLegacySupportExtendedCapability(usbBaseAddress, aEECP);

                    /* Claim EHCI for OS */
                    USBLegacySupportExtendedCapability.HCOSOwnedSemaphore = true;
                }
            }
        }
    }

    /// <summary>
    /// Represents a Queue Head structure's memory layout.
    /// This structure can be passed to the HCI.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct QueueHead_Struct
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
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct qTD_Struct
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
    public unsafe class qTD : FOS_System.Object
    {
        /*
         * See section 3.5 of spec.
         */

        /// <summary>
        /// The qTD data/memory structure that can be passed to the HCI.
        /// </summary>
        public qTD_Struct* qtd;

        /// <summary>
        /// Pointer to the next qTD in the linked list.
        /// </summary>
        public qTD_Struct* NextqTDPointer
        {
            [Compiler.NoGC]
            get
            {
                return (qTD_Struct*)(qtd->u1 & 0xFFFFFFF0u);
            }
            [Compiler.NoGC]
            set
            {
                qtd->u1 = (qtd->u1 & 0x0000000Fu) | ((uint)value & 0xFFFFFFF0u);
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
                return (byte)((qtd->u3 & 0x00000030u) >> 8);
            }
            [Compiler.NoGC]
            set
            {
                qtd->u3 = (qtd->u3 & 0xFFFFFFCFu) | (uint)(value << 8); 
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
                qtd->u3 = (qtd->u3 & 0xFFFFF3FFu) | (uint)(value << 10);
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
                qtd->u3 = (qtd->u3 & 0xFFFF8FFF) | (uint)(value << 12);
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
                    qtd->u1 = qtd->u3 | 0x00008000u;
                }
                else
                {
                    qtd->u1 = qtd->u3 & 0xFFFF7FFFu;
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
                qtd->u3 = (qtd->u3 & 0x8000FFFF) | (uint)(value << 16);
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
        /// Buffer 0 pointer.
        /// </summary>
        public byte* Buffer0
        {
            [Compiler.NoGC]
            get
            {
                return (byte*)(qtd->u4 & 0xFFFFF000);
            }
            [Compiler.NoGC]
            set
            {
                qtd->u4 = (uint)value & 0xFFFFF000;
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
        public qTD()
        {
            qtd = (qTD_Struct*)FOS_System.Heap.Alloc((uint)sizeof(qTD_Struct));
        }
        /// <summary>
        /// Initializes a qTD with specified underlying data structure.
        /// </summary>
        /// <param name="aqTD">The existing underlying data structure.</param>
        public qTD(qTD_Struct* aqTD)
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
    public unsafe class QueueHead : FOS_System.Object
    {
        /*
         * See section 3.6 of spec.
         */

        /// <summary>
        /// The queue head data/memory structure that can be passed to the HCI.
        /// </summary>
        public QueueHead_Struct* queueHead;

        /// <summary>
        /// Horizontal link pointer - points to the next queue head in the list.
        /// </summary>
        public QueueHead_Struct* HorizontalLinkPointer
        {
            [Compiler.NoGC]
            get
            {
                return (QueueHead_Struct*)(queueHead->u1 & 0xFFFFFFF0u);
            }
            [Compiler.NoGC]
            set
            {
                queueHead->u1 = (uint)HorizontalLinkPointer & 0xFFFFFFF0u;
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
                return (byte)((queueHead->u1 >> 1) & 0x0000000Fu);
            }
            [Compiler.NoGC]
            set
            {
                queueHead->u1 = (queueHead->u1 & 0xFFFFFFF0u) | (uint)((value & 0x0000000Fu) << 1);
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
                queueHead->u2 = (queueHead->u2 & 0xFFFFF0FFu) | (uint)(value << 8);
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
                queueHead->u2 = (queueHead->u2 & 0xFFFFCFFFu) | (uint)(value << 12);
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
                queueHead->u2 = (queueHead->u2 & 0xF800FFFFu) | ((uint)(value << 16) & 0x07FF0000u);
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
                queueHead->u2 = (queueHead->u2 & 0x0FFFFFFFu) | (uint)(value << 28);
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
                queueHead->u3 = (queueHead->u3 & 0xFFFF00FFu) | (uint)(value << 8);
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
                queueHead->u3 = (queueHead->u3 & 0xFF80FFFFu) | (uint)(value << 16);
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
                return (byte)((queueHead->u3 & 0x007F0000u) >> 23);
            }
            [Compiler.NoGC]
            set
            {
                queueHead->u3 = (queueHead->u3 & 0xC07FFFFFu) | (uint)(value << 23);
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
                queueHead->u3 = (queueHead->u3 & 0x3FFFFFFFu) | (uint)(value << 30);
            }
        }
        /// <summary>
        /// Current qTD pointer.
        /// </summary>
        public qTD_Struct* CurrentqTDPointer
        {
            [Compiler.NoGC]
            get
            {
                return (qTD_Struct*)(queueHead->u4 & 0xFFFFFFF0u);
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
        public qTD_Struct* NextqTDPointer
        {
            [Compiler.NoGC]
            get
            {
                return (qTD_Struct*)(queueHead->u5 & 0xFFFFFFF0u);
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
        public QueueHead()
        {
            queueHead = (QueueHead_Struct*)FOS_System.Heap.Alloc((uint)sizeof(QueueHead_Struct));
        }
        /// <summary>
        /// Initializes a new queue head with specified underlying memory structure.
        /// </summary>
        /// <param name="aQueueHead">The existing underlying queue head.</param>
        public QueueHead(QueueHead_Struct* aQueueHead)
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

    /// <summary>
    /// Represents EHCI Extended Capabilities register information.
    /// </summary>
    public unsafe class EHCI_ExtendedCapability : FOS_System.Object
    {
        /// <summary>
        /// USB PCI base address.
        /// </summary>
        internal byte* usbBaseAddress;
        /// <summary>
        /// Capabilities registers offset from base address.
        /// </summary>
        internal byte capOffset;
        /// <summary>
        /// Capability ID.
        /// </summary>
        public byte CapabilityID
        {
            [Compiler.NoGC]
            get
            {
                return *(usbBaseAddress + capOffset);
            }
        }
        /// <summary>
        /// Next EHCI extended capability offset.
        /// </summary>
        public byte NextEHCIExtendedCapabilityOffset
        {
            [Compiler.NoGC]
            get
            {
                return *(usbBaseAddress + capOffset + 8);
            }
        }

        /// <summary>
        /// Initializes new extended capabilities information.
        /// </summary>
        /// <param name="aUSBBaseAddress">The USB base address.</param>
        /// <param name="aCapOffset">The capabilities registers offset form base address.</param>
        public EHCI_ExtendedCapability(byte* aUSBBaseAddress, byte aCapOffset)
        {
            usbBaseAddress = aUSBBaseAddress;
            capOffset = aCapOffset;
        }
    }
    /// <summary>
    /// Represents EHCI USB Legacy Support Extended Capabilities register information.
    /// </summary>
    public unsafe class EHCI_USBLegacySupportExtendedCapability : EHCI_ExtendedCapability
    {
        /*
         * See section 2.1.7 of spec.
         */

        /// <summary>
        /// HC OS Owned semaphore - whether the OS controls the EHCI.
        /// </summary>
        public bool HCOSOwnedSemaphore
        {
            [Compiler.NoGC]
            get
            {
                return (*(usbBaseAddress + capOffset + 24) & 0x01) > 0;
            }
            [Compiler.NoGC]
            set
            {
                if (value)
                {
                    *(usbBaseAddress + capOffset + 24) = (byte)(*(usbBaseAddress + capOffset + 24) | 0x01u);
                }
                else
                {
                    *(usbBaseAddress + capOffset + 24) = (byte)(*(usbBaseAddress + capOffset + 24) & 0xFEu);
                }
            }
        }
        /// <summary>
        /// HC BIOS Owned semaphore - whether the BIOS controls the EHCI.
        /// </summary>
        public bool HCBIOSOwnedSemaphore
        {
            [Compiler.NoGC]
            get
            {
                return (*(usbBaseAddress + capOffset + 16) & 0x01) > 0;
            }
            [Compiler.NoGC]
            set
            {
                if (value)
                {
                    *(usbBaseAddress + capOffset + 16) = (byte)(*(usbBaseAddress + capOffset + 16) | 0x01u);
                }
                else
                {
                    *(usbBaseAddress + capOffset + 16) = (byte)(*(usbBaseAddress + capOffset + 16) & 0xFEu);
                }
            }
        }

        /// <summary>
        /// Control status capabilities.
        /// </summary>
        public EHCI_USBLegacySupportControlStatusCapability ControlStatusCap;

        /// <summary>
        /// Initializes new EHCI USB Legacy Support Extended Capabilities register information.
        /// </summary>
        /// <param name="aUSBBaseAddress">The USB base address.</param>
        /// <param name="aCapOffset">The capabilities registers offset from the base address.</param>
        public EHCI_USBLegacySupportExtendedCapability(byte* aUSBBaseAddress, byte aCapOffset)
            : base(aUSBBaseAddress, aCapOffset)
        {
            ControlStatusCap = new EHCI_USBLegacySupportControlStatusCapability(usbBaseAddress, (byte)(capOffset + 4u));
        }
    }
    /// <summary>
    /// Represents EHCI USB Legacy Support Control/Status Capabilities register information.
    /// </summary>
    public unsafe class EHCI_USBLegacySupportControlStatusCapability : EHCI_ExtendedCapability
    {
        /*
         * See section 2.1.8 of spec.
         */

        //TODO - Add the necessary fields to this class

        /// <summary>
        /// Initializes new EHCI USB Legacy Support Control/Status Capabilities register information.
        /// </summary>
        /// <param name="aUSBBaseAddress">The USB base address.</param>
        /// <param name="aCapOffset">The capabilities registers offset from the base address.</param>
        public EHCI_USBLegacySupportControlStatusCapability(byte* aUSBBaseAddress, byte aCapOffset)
            : base(aUSBBaseAddress, aCapOffset)
        {
        }
    }
}
