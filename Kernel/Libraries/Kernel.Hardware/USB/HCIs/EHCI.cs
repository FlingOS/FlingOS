using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.USB.HCIs
{
    public unsafe class EHCI : HCI
    {
        /*
         * Based on the Intel EHCI Specification for USB 2.0
         *  http://www.intel.co.uk/content/dam/www/public/us/en/documents/technical-specifications/ehci-specification-for-usb.pdf
         */

        protected byte* usbBaseAddress;

        #region PCI Registers
        
        /*
         * See section 2.1 of spec.
         */

        protected byte SBRN;
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
        protected byte PortWakeCap
        {
            get
            {
                return pciDevice.ReadRegister8(0x62);
            }
        }
        protected EHCI_USBLegacySupportExtendedCapability USBLegacySupportExtendedCapability;
        
        #endregion

        #region Capability Registers
        
        /*
         *  See section 2.2 of spec.
         */

        protected byte* CapabilitiesRegAddr;
        protected byte CapabilitiesRegsLength;
        protected UInt16 HCIVersion;
        protected uint HCSParams;
        protected uint HCCParams;
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

        protected uint* OpRegAddr;

        #region Core Well

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
        protected uint ASYNCLISTADDR
        {
            get
            {
                return *(OpRegAddr + 6);
            }
            set
            {
                *(OpRegAddr + 6) = value;
            }
        }

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
        protected bool AsynchronousScheduleStatus
        {
            get
            {
                return (USBSTS & 0x80) > 0;
            }
        }

        #endregion

        #region Aux Well

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

        public EHCI(PCI.PCIDeviceNormal aPCIDevice)
            : base(aPCIDevice)
        {
            usbBaseAddress = pciDevice.BaseAddresses[4].BaseAddress();
            CapabilitiesRegAddr = usbBaseAddress;

            SBRN = pciDevice.ReadRegister8(0x60);

            CapabilitiesRegsLength = *CapabilitiesRegAddr;
            HCIVersion = *((UInt16*)(CapabilitiesRegAddr + 2));
            HCSParams = *((UInt32*)(CapabilitiesRegAddr + 4));
            HCCParams = *((UInt32*)(CapabilitiesRegAddr + 8));
            HCSPPortRouteDesc = *((UInt64*)(CapabilitiesRegAddr + 12));

            OpRegAddr = (uint*)(usbBaseAddress + CapabilitiesRegsLength);

            LoadExtendedCapabilities();
        }

        #region Initialisation / setup methods

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
         * to companion controllers yet nor inidividual port power 
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
        protected void WaitForAsyncQueueEnabledStatusMatch()
        {
            while (AsynchronousScheduleEnabled != AsynchronousScheduleStatus)
            {
                ;
            }
        }
        protected void EnableAsyncQueue()
        {
            // - Wait for AsyncQueueEnabled and corresponding Status bit to match
            WaitForAsyncQueueEnabledStatusMatch();
            // - Set AsyncQueueEnabled
            AsynchronousScheduleEnabled = true;
            // - Wait again
            WaitForAsyncQueueEnabledStatusMatch();
        }
        protected void DisableAsyncQueue()
        {
            // - Wait for AsyncQueueEnabled and corresponding Status bit to match
            WaitForAsyncQueueEnabledStatusMatch();
            // - Set AsyncQueueEnabled
            AsynchronousScheduleEnabled = false;
            // - Wait again
            WaitForAsyncQueueEnabledStatusMatch();
        }
        
        //To be done
        //TODO - Methods for adding / removing QueueHeads from the 
        //       HC's list
        protected void AddAsyncQueueHead(QueueHead theHead)
        {
            // - Reclaim anything from the queue that we can
            // - Check if queue is empty:
            //      - If so, create new queue
        }
        protected void RemoveAsyncQueueHead(QueueHead theHead)
        {
        }

        //TODO - Methods for creating QueueHeads for an endpoint
        protected QueueHead CreateQueueHead(/*Params?*/)
        {
            return null;
        }

        //TODO - Methods for creating lists of qTDs for a 
        //       queue head and for adding/removing them from/to 
        //       a queue head.
        protected qTD CreateqTD(/*Params?*/)
        {
            return null;
        }
        protected void AddqTDToQueueHead(qTD theqTD, QueueHead theQueueHead)
        {
        }
        protected void RemoveqTDFromQueueHead(qTD theqTD, QueueHead theQueueHead)
        {
        }

        //TODO - Methods for re-cycling (/re-using) queue heads
        //       and qTDs to reduce memory allocation load / 
        //       improve performance
        protected void RecycleQueueHead(QueueHead theQueueHead)
        {
        }
        protected void RecycleqTD(qTD theqTD)
        {
        }

        //TODO - Methods for reclaiming QueueHeads and qTDs
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

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct QueueHead_Struct
    {
        /*
         * See section 3.6 of spec.
         */

        public uint u1;
        public uint u2;
        public uint u3;
        public uint u4;
        public uint u5;
        public uint u6;
        public uint u7;
        public uint u8;
        public uint u9;
        public uint u10;
        public uint u11;
        public uint u12;
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct qTD_Struct
    {
        /*
         * See section 3.5 of spec.
         */

        public uint u1;
        public uint u2;
        public uint u3;
        public uint u4;
        public uint u5;
        public uint u6;
        public uint u7;
        public uint u8;
    }
    public unsafe class qTD : FOS_System.Object
    {
        /*
         * See section 3.5 of spec.
         */

        public qTD_Struct* qtd;

        public qTD_Struct* NextqTDPointer
        {
            get
            {
                return (qTD_Struct*)(qtd->u1 & 0xFFFFFFF0u);
            }
            set
            {
                qtd->u1 = (qtd->u1 & 0x0000000Fu) | ((uint)value & 0xFFFFFFF0u);
            }
        }
        public bool NextqTDPointerTerminate
        {
            get
            {
                return (qtd->u1 & 0x00000001u) > 0;
            }
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
        public byte Status
        {
            get
            {
                return (byte)(qtd->u3);
            }
            set
            {
                qtd->u3 = (qtd->u3 & 0xFFFFFF00u) | value;
            }
        }
        public byte PIDCode
        {
            get
            {
                return (byte)((qtd->u3 & 0x00000030u) >> 8);
            }
            set
            {
                qtd->u3 = (qtd->u3 & 0xFFFFFFCFu) | (uint)(value << 8); 
            }
        }
        public byte ErrorCounter
        {
            get
            {
                return (byte)((qtd->u3 & 0x00000C00u) >> 10);
            }
            set
            {
                qtd->u3 = (qtd->u3 & 0xFFFFF3FFu) | (uint)(value << 10);
            }
        }
        public byte CurrentPage
        {
            get
            {
                return (byte)((qtd->u3 & 0x00007000) >> 12);
            }
            set
            {
                qtd->u3 = (qtd->u3 & 0xFFFF8FFF) | (uint)(value << 12);
            }
        }
        public bool InterruptOnComplete
        {
            get
            {
                return (qtd->u3 & 0x00008000u) > 0;
            }
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
        public UInt16 TotalBytesToTransfer
        {
            get
            {
                return (UInt16)((qtd->u3 >> 16) & 0x00007FFF);
            }
            set
            {
                qtd->u3 = (qtd->u3 & 0x8000FFFF) | (uint)(value << 16);
            }
        }
        public bool DataToggle
        {
            get
            {
                return (qtd->u3 & 0x80000000u) > 0;
            }
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
        
        public qTD()
        {
            qtd = (qTD_Struct*)FOS_System.Heap.Alloc((uint)sizeof(qTD_Struct));
        }
        public qTD(qTD_Struct* aqTD)
        {
            qtd = aqTD;
        }

        public void Free()
        {
            FOS_System.Heap.Free(qtd);
        }
    }
    public unsafe class QueueHead : FOS_System.Object
    {
        /*
         * See section 3.6 of spec.
         */

        public QueueHead_Struct* queueHead;

        public QueueHead_Struct* HorizontalLinkPointer
        {
            get
            {
                return (QueueHead_Struct*)(queueHead->u1 & 0xFFFFFFF0u);
            }
            set
            {
                queueHead->u1 = (uint)HorizontalLinkPointer & 0xFFFFFFF0u;
            }
        }
        public byte Type
        {
            get
            {
                return (byte)((queueHead->u1 >> 1) & 0x0000000Fu);
            }
            set
            {
                queueHead->u1 = (queueHead->u1 & 0xFFFFFFF0u) | (uint)((value & 0x0000000Fu) << 1);
            }
        }
        public byte DeviceAddress
        {
            get
            {
                return (byte)(queueHead->u2 & 0x0000007Fu);
            }
            set
            {
                queueHead->u2 = (queueHead->u2 & 0xFFFFFF80u) | (value & 0x0000007Fu);
            }
        }
        public bool InactiveOnNextTransaction
        {
            get
            {
                return (queueHead->u2 & 0x00000080u) > 0;
            }
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
        public byte EndpointNumber
        {
            get
            {
                return (byte)((queueHead->u2 & 0x00000F00u) >> 8);
            }
            set
            {
                queueHead->u2 = (queueHead->u2 & 0xFFFFF0FFu) | (uint)(value << 8);
            }
        }
        public byte EndpointSpeed
        {
            get
            {
                return (byte)((queueHead->u2 & 0x00003000u) >> 12);
            }
            set
            {
                queueHead->u2 = (queueHead->u2 & 0xFFFFCFFFu) | (uint)(value << 12);
            }
        }
        public bool DataToggleControl
        {
            get
            {
                return (queueHead->u2 & 0x00004000u) > 0;
            }
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
        public bool HeadOfReclamationList
        {
            get
            {
                return (queueHead->u2 & 0x00008000u) > 0;
            }
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
        public UInt16 MaximumPacketLength
        {
            get
            {
                return (UInt16)((queueHead->u2 & 0x07FF0000u) >> 16);
            }
            set
            {
                queueHead->u2 = (queueHead->u2 & 0xF800FFFFu) | ((uint)(value << 16) & 0x07FF0000u);
            }
        }
        public bool ControlEndpointFlag
        {
            get
            {
                return (queueHead->u2 & 0x08000000u) > 0;
            }
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
        public byte NakCountReload
        {
            get
            {
                return (byte)((queueHead->u2 & 0xF0000000u) >> 28);
            }
            set
            {
                queueHead->u2 = (queueHead->u2 & 0x0FFFFFFFu) | (uint)(value << 28);
            }
        }
        public byte InterruptScheduleMask
        {
            get
            {
                return (byte)(queueHead->u3);
            }
            set
            {
                queueHead->u3 = (queueHead->u3 & 0xFFFFFF00u) | value;
            }
        }
        public byte SplitCompletionMask
        {
            get
            {
                return (byte)(queueHead->u3 >> 8);
            }
            set
            {
                queueHead->u3 = (queueHead->u3 & 0xFFFF00FFu) | (uint)(value << 8);
            }
        }
        public byte HubAddr
        {
            get
            {
                return (byte)((queueHead->u3 & 0x007F0000u) >> 16);
            }
            set
            {
                queueHead->u3 = (queueHead->u3 & 0xFF80FFFFu) | (uint)(value << 16);
            }
        }
        public byte PortNumber
        {
            get
            {
                return (byte)((queueHead->u3 & 0x007F0000u) >> 23);
            }
            set
            {
                queueHead->u3 = (queueHead->u3 & 0xC07FFFFFu) | (uint)(value << 23);
            }
        }
        public byte HighBandwidthPipeMultiplier
        {
            get
            {
                return (byte)((queueHead->u3 & 0xC0000000u) >> 30);
            }
            set
            {
                queueHead->u3 = (queueHead->u3 & 0x3FFFFFFFu) | (uint)(value << 30);
            }
        }
        public qTD_Struct* CurrentqTDPointer
        {
            get
            {
                return (qTD_Struct*)(queueHead->u4 & 0xFFFFFFF0u);
            }
            set
            {
                queueHead->u4 = (queueHead->u4 & 0x0000000Fu) | ((uint)value & 0xFFFFFFF0u);
            }
        }
        public qTD_Struct* NextqTDPointer
        {
            get
            {
                return (qTD_Struct*)(queueHead->u5 & 0xFFFFFFF0u);
            }
            set
            {
                queueHead->u5 = (queueHead->u5 & 0x0000000Fu) | ((uint)value & 0xFFFFFFF0u);
            }
        }
        public bool NextqTDPointerTerminate
        {
            get
            {
                return (queueHead->u5 & 0x00000001u) > 0;
            }
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

        public QueueHead()
        {
            queueHead = (QueueHead_Struct*)FOS_System.Heap.Alloc((uint)sizeof(QueueHead_Struct));
        }
        public QueueHead(QueueHead_Struct* aQueueHead)
        {
            queueHead = aQueueHead;
        }

        public void Free()
        {
            FOS_System.Heap.Free(queueHead);
        }
    }

    public unsafe class EHCI_ExtendedCapability : FOS_System.Object
    {
        internal byte* usbBaseAddress;
        internal byte capOffset;
        public byte CapabilityID
        {
            get
            {
                return *(usbBaseAddress + capOffset);
            }
        }
        public byte NextEHCIExtendedCapabilityOffset
        {
            get
            {
                return *(usbBaseAddress + capOffset + 8);
            }
        }

        public EHCI_ExtendedCapability(byte* aUSBBaseAddress, byte aCapOffset)
        {
            usbBaseAddress = aUSBBaseAddress;
            capOffset = aCapOffset;
        }
    }
    public unsafe class EHCI_USBLegacySupportExtendedCapability : EHCI_ExtendedCapability
    {
        /*
         * See section 2.1.7 of spec.
         */

        public bool HCOSOwnedSemaphore
        {
            get
            {
                return (*(usbBaseAddress + capOffset + 24) & 0x01) > 0;
            }
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
        public bool HCBIOSOwnedSemaphore
        {
            get
            {
                return (*(usbBaseAddress + capOffset + 16) & 0x01) > 0;
            }
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

        public EHCI_USBLegacySupportControlStatusCapability ControlStatusCap;

        public EHCI_USBLegacySupportExtendedCapability(byte* aUSBBaseAddress, byte aCapOffset)
            : base(aUSBBaseAddress, aCapOffset)
        {
            ControlStatusCap = new EHCI_USBLegacySupportControlStatusCapability(usbBaseAddress, (byte)(capOffset + 4u));
        }
    }
    public unsafe class EHCI_USBLegacySupportControlStatusCapability : EHCI_ExtendedCapability
    {
        /*
         * See section 2.1.8 of spec.
         */

        //TODO - Add the necessary fields to this class

        public EHCI_USBLegacySupportControlStatusCapability(byte* aUSBBaseAddress, byte aCapOffset)
            : base(aUSBBaseAddress, aCapOffset)
        {
        }
    }
}
