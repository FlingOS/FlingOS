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
using Kernel.Hardware.USB.Devices;
using Utils = Kernel.Utilities.ConstantsUtils;

namespace Kernel.Hardware.USB.HCIs
{
//    public static class UHCI_Consts
//    {
//        public static byte UHCIPORTMAX = 8;  // max number of UHCI device ports

//        public static byte UHCI_USBCMD = 0x00;
//        public static byte UHCI_USBSTS = 0x02;
//        public static byte UHCI_USBINTR = 0x04;
//        public static byte UHCI_FRNUM = 0x06;
//        public static byte UHCI_FRBASEADD = 0x08;
//        public static byte UHCI_SOFMOD = 0x0C;
//        public static byte UHCI_PORTSC1 = 0x10;
//        public static byte UHCI_PORTSC2 = 0x12;


//        /* ****** */
//        /* USBCMD */
//        /* ****** */

//        public static ushort UHCI_CMD_MAXP = (ushort)Utils.BIT(7);
//        public static ushort UHCI_CMD_CF = (ushort)Utils.BIT(6);
//        public static ushort UHCI_CMD_SWDBG = (ushort)Utils.BIT(5);
//        public static ushort UHCI_CMD_FGR = (ushort)Utils.BIT(4);
//        public static ushort UHCI_CMD_EGSM = (ushort)Utils.BIT(3);
//        public static ushort UHCI_CMD_GRESET = (ushort)Utils.BIT(2);
//        public static ushort UHCI_CMD_HCRESET = (ushort)Utils.BIT(1);
//        public static ushort UHCI_CMD_RS = (ushort)Utils.BIT(0);


//        /* ******* */
//        /* USBSTS  */
//        /* ******* */

//        public static uint UHCI_STS_HCHALTED = Utils.BIT(5);
//        public static uint UHCI_STS_HC_PROCESS_ERROR = Utils.BIT(4);
//        public static uint UHCI_STS_HOST_SYSTEM_ERROR = Utils.BIT(3);
//        public static uint UHCI_STS_RESUME_DETECT = Utils.BIT(2);
//        public static uint UHCI_STS_USB_ERROR = Utils.BIT(1);
//        public static uint UHCI_STS_USBINT = Utils.BIT(0);
//        public static ushort UHCI_STS_MASK = 0x3F;

//        /* ******* */
//        /* USBINTR */
//        /* ******* */

//        public static uint UHCI_INT_SHORT_PACKET_ENABLE = Utils.BIT(3);
//        public static uint UHCI_INT_IOC_ENABLE = Utils.BIT(2);
//        public static uint UHCI_INT_RESUME_ENABLE = Utils.BIT(1);
//        public static uint UHCI_INT_TIMEOUT_ENABLE = Utils.BIT(0);
//        public static uint UHCI_INT_MASK = 0xF;

//        /* ******* */
//        /* PORTSC  */
//        /* ******* */

//        public static ushort UHCI_SUSPEND = (ushort)Utils.BIT(12);
//        public static ushort UHCI_PORT_RESET = (ushort)Utils.BIT(9);
//        public static ushort UHCI_PORT_LOWSPEED_DEVICE = (ushort)Utils.BIT(8);
//        public static ushort UHCI_PORT_VALID = (ushort)Utils.BIT(7); // reserved as readonly; always read as 1

//        public static ushort UHCI_PORT_RESUME_DETECT = (ushort)Utils.BIT(6);
//        public static ushort UHCI_PORT_ENABLE_CHANGE = (ushort)Utils.BIT(3);
//        public static ushort UHCI_PORT_ENABLE = (ushort)Utils.BIT(2);
//        public static ushort UHCI_PORT_CS_CHANGE = (ushort)Utils.BIT(1);
//        public static ushort UHCI_PORT_CS = (ushort)Utils.BIT(0);

//        /* *************** */
//        /* LEGACY SUPPORT  */
//        /* *************** */

//        // Register in PCI (ushort)
//        public static byte UHCI_PCI_LEGACY_SUPPORT = 0xC0;

//        // Interrupt carried out as a PCI interrupt
//        public static ushort UHCI_PCI_LEGACY_SUPPORT_PIRQ = 0x2000;

//        // RO Bits
//        public static ushort UHCI_PCI_LEGACY_SUPPORT_NO_CHG = 0x5040;

//        // Status bits that are cleared by setting to 1
//        public static ushort UHCI_PCI_LEGACY_SUPPORT_STATUS = 0x8F00;


//        /* ***** */
//        /* QH TD */
//        /* ***** */

//        public static uint BIT_T = Utils.BIT(0);
//        public static uint BIT_QH = Utils.BIT(1);
//        public static uint BIT_Vf = Utils.BIT(2);

//        /*
//        Packet Identification (PID). This field contains the Packet ID to be used for this transaction. Only
//        the IN (69h), OUT (E1h), and SETUP (2Dh) tokens are allowed. Any other value in this field causes
//        a consistency check failure resulting in an immediate halt of the Host Controller. Bits [3:0] are
//        complements of bits [7:4].
//        */
//        public static uint UHCI_TD_SETUP = 0x2D;  // 00101101
//        public static uint UHCI_TD_IN = 0x69;  // 11100001
//        public static uint UHCI_TD_OUT = 0xE1;  // 11100001
//    }
//    public unsafe class UHCI : HCI
//    {
//        /// <summary>
//        /// The base address of the USB HCI device in memory.
//        /// </summary>
//        protected byte* usbBaseAddress;

//        protected uint memSize;
//        protected bool enabledPortFlag;


//        /// <summary>
//        /// USB command operational port.
//        /// </summary>
//        protected IO.IOPort USBCMD;
//        /// <summary>
//        /// USB interrupt operational port.
//        /// </summary>
//        protected IO.IOPort USBINTR;
//        /// <summary>
//        /// USB status operational port.
//        /// </summary>
//        protected IO.IOPort USBSTS;
//        /// <summary>
//        /// SOFMOD operational port.
//        /// </summary>
//        protected IO.IOPort SOFMOD;
//        /// <summary>
//        /// FRBASEADD operational port.
//        /// </summary>
//        protected IO.IOPort FRBASEADD;
//        /// <summary>
//        /// FRNUM operational port.
//        /// </summary>
//        protected IO.IOPort FRNUM;
//        /// <summary>
//        /// USB port SC 1 operational port.
//        /// </summary>
//        protected IO.IOPort PORTSC1;
//        /// <summary>
//        /// USB port SC 2 operational port.
//        /// </summary>
//        protected IO.IOPort PORTSC2;

//        frPtr_Struct* framelistAddrVirt; // virtual adress of frame list
//        UHCI_QueueHead qhPointerVirt;     // virtual adress of QH
//        bool run;

//        public UHCI(PCI.PCIDeviceNormal aPCIDevice)
//            : base(aPCIDevice)
//        {
//#if UHCI_TRACE
//            DBGMSG(">>> UHCI Constructor <<<");
//#endif

//            PCI.PCIBaseAddress pciBAR = pciDevice.BaseAddresses[4];
//            usbBaseAddress = (byte*)((uint)pciBAR.BaseAddress() & 0xFFFFFFF0);
//#if UHCI_TRACE
//            DBGMSG("usbBaseAddress: " + (FOS_System.String)(uint)usbBaseAddress);
//#endif

//            memSize = pciBAR.Size();
//#if UHCI_TRACE
//            DBGMSG("memSize: " + (FOS_System.String)memSize);
//#endif

//            //Hmm...is cast to UInt16 valid here? Could we potentially have the port mapped higher than 
//            //      UINt16 max value??
//            USBCMD = new IO.IOPort((UInt16)usbBaseAddress, UHCI_Consts.UHCI_USBCMD);
//            USBINTR = new IO.IOPort((UInt16)usbBaseAddress, UHCI_Consts.UHCI_USBINTR);
//            USBSTS = new IO.IOPort((UInt16)usbBaseAddress, UHCI_Consts.UHCI_USBSTS);
//            SOFMOD = new IO.IOPort((UInt16)usbBaseAddress, UHCI_Consts.UHCI_SOFMOD);
//            FRBASEADD = new IO.IOPort((UInt16)usbBaseAddress, UHCI_Consts.UHCI_FRBASEADD);
//            FRNUM = new IO.IOPort((UInt16)usbBaseAddress, UHCI_Consts.UHCI_FRNUM);
//            PORTSC1 = new IO.IOPort((UInt16)usbBaseAddress, UHCI_Consts.UHCI_PORTSC1);
//            PORTSC2 = new IO.IOPort((UInt16)usbBaseAddress, UHCI_Consts.UHCI_PORTSC2);

//#if UHCI_TRACE
//            DBGMSG("USBCMD: " + (FOS_System.String)USBCMD.Port);
//            DBGMSG("USBINTR: " + (FOS_System.String)USBINTR.Port);
//            DBGMSG("USBSTS: " + (FOS_System.String)USBSTS.Port);
//            DBGMSG("SOFMOD: " + (FOS_System.String)SOFMOD.Port);
//            DBGMSG("FRBASEADD: " + (FOS_System.String)FRBASEADD.Port);
//            DBGMSG("FRNUM: " + (FOS_System.String)(uint)FRNUM.Port);
//            DBGMSG("PORTSC1: " + (FOS_System.String)(uint)PORTSC1.Port);
//            DBGMSG("PORTSC2: " + (FOS_System.String)(uint)PORTSC2.Port);
//#endif
//            Start();
//        }

//        public override void Update()
//        {
//        }

//        protected void Start()
//        {
//#if UHCI_TRACE
//            DBGMSG(">>> startUHCI <<<");
//#endif

//            InitHC();
//        }

//        protected void InitHC()
//        {
//#if UHCI_TRACE
//            DBGMSG(">>> initUHCIHostController <<<");
//            DBGMSG("Initialize UHCI Host Controller:");
//#endif

//            pciDevice.Command = pciDevice.Command | PCI.PCIDevice.PCICommand.IO | PCI.PCIDevice.PCICommand.Master;

//            Interrupts.Interrupts.AddIRQHandler(pciDevice.InterruptLine, UHCI.InterruptHandler, this);

//            ResetHC();
//        }

//        protected void ResetHC()
//        {
//#if UHCI_TRACE
//            DBGMSG(">>> uhci_resetHostController <<<");
//#endif

//            // http://www.lowlevel.eu/wiki/Universal_Host_Controller_Interface#Informationen_vom_PCI-Treiber_holen

//            ushort legacySupport = pciDevice.ReadRegister16(UHCI_Consts.UHCI_PCI_LEGACY_SUPPORT);
//#if UHCI_TRACE
//            DBGMSG(((FOS_System.String)"legacySupport : ") + legacySupport);
//#endif

//            USBCMD.Write_UInt16(UHCI_Consts.UHCI_CMD_GRESET);
//            sleepMilliSeconds(100); // at least 50 msec
//            USBCMD.Write_UInt16((ushort)0u);

//            // get number of valid root ports
//            RootPortCount = (byte)((memSize - UHCI_Consts.UHCI_PORTSC1) / 2); // each port has a two byte PORTSC register

//            for (byte i = 2; i < RootPortCount; i++)
//            {
//                if (((ReadPort_SC1_16(i) & UHCI_Consts.UHCI_PORT_VALID) == 0) || // reserved bit 7 is already read as 1
//                     (ReadPort_SC1_16(i) == 0xFFFF))
//                {
//                    RootPortCount = i;
//                    break;
//                }
//            }

//            if (RootPortCount > UHCI_Consts.UHCIPORTMAX)
//            {
//                RootPortCount = UHCI_Consts.UHCIPORTMAX;
//            }
            
//            ushort uhci_usbcmd = USBCMD.Read_UInt16();
//            ushort uhci_usbintr = USBINTR.Read_UInt16();
//#if UHCI_TRACE
//            DBGMSG(((FOS_System.String)"UHCI root ports: ") + RootPortCount);
//            DBGMSG(((FOS_System.String)"USBCMD: ") + uhci_usbcmd);
//            DBGMSG(((FOS_System.String)"USBINTR: ") + uhci_usbintr);
//#endif

//            if (((legacySupport & ~(UHCI_Consts.UHCI_PCI_LEGACY_SUPPORT_STATUS |
//                                   UHCI_Consts.UHCI_PCI_LEGACY_SUPPORT_NO_CHG |
//                                   UHCI_Consts.UHCI_PCI_LEGACY_SUPPORT_PIRQ)) != 0) ||
//                 ((uhci_usbcmd & UHCI_Consts.UHCI_CMD_RS) != 0) ||
//                 ((uhci_usbcmd & UHCI_Consts.UHCI_CMD_CF) != 0) ||
//                 ((uhci_usbcmd & UHCI_Consts.UHCI_CMD_EGSM) == 0) ||
//                 ((uhci_usbintr & UHCI_Consts.UHCI_INT_MASK) != 0))
//            {
//#if UHCI_TRACE
//                DBGMSG("Resetting HC...");
//#endif

//                USBSTS.Write_UInt16(UHCI_Consts.UHCI_STS_MASK);
//                sleepMilliSeconds(1);                             // wait one frame
//                pciDevice.WriteRegister16(UHCI_Consts.UHCI_PCI_LEGACY_SUPPORT, UHCI_Consts.UHCI_PCI_LEGACY_SUPPORT_STATUS);
//                USBCMD.Write_UInt16(UHCI_Consts.UHCI_CMD_HCRESET);

//                byte timeout = 50;
//                bool Reset = (USBCMD.Read_UInt16() & UHCI_Consts.UHCI_CMD_HCRESET) != 0;
//                while (Reset)
//                {
//                    if (timeout == 0)
//                    {
//#if UHCI_TRACE
//                        DBGMSG("USBCMD_HCRESET timed out!");
//#endif
//                        break;
//                    }
//                    sleepMilliSeconds(10);
//                    timeout--;
//                    Reset = (USBCMD.Read_UInt16() & UHCI_Consts.UHCI_CMD_HCRESET) != 0;
//                }
//#if UHCI_TRACE
//                DBGMSG(((FOS_System.String)"Reset : ") + Reset);
//#endif

//                USBINTR.Write_UInt16((ushort)0u); // switch off all interrupts
//                USBCMD.Write_UInt16((ushort)0u); // switch off the host controller

//                for (byte i = 0; i < RootPortCount; i++) // switch off the valid root ports
//                {
//                    WritePort_SC1_16(i, (ushort)0u);
//                }
//#if UHCI_TRACE
//                DBGMSG("HC reset.");
//#endif
//            }
      
//            framelistAddrVirt = (frPtr_Struct*)FOS_System.Heap.Alloc((uint)sizeof(frPtr_Struct));
//            framelistAddrVirt->frPtr = (uint**)FOS_System.Heap.Alloc((uint)(sizeof(uint) * 1024), 1024);

//            UHCI_QueueHead qh = new UHCI_QueueHead();
//            qh.queueHead->next = (UHCI_QueueHead_Struct*)UHCI_Consts.BIT_T;
//            qh.queueHead->transfer = (uint*)UHCI_Consts.BIT_T;
//            qh.queueHead->q_first = null;
//            qh.queueHead->q_last = null;
//            qhPointerVirt = qh;

//            for (ushort i = 0; i < 1024; i++)
//            {
//                framelistAddrVirt->frPtr[i] = (uint*)((uint)qh.queueHead | UHCI_Consts.BIT_QH);
//            }

//            // define each millisecond one frame, provide physical address of frame list, and start at frame 0
//            SOFMOD.Write_Byte((byte)0x40); // SOF cycle time: 12000. For a 12 MHz SOF counter clock input, this produces a 1 ms Frame period.    

//            FRBASEADD.Write_UInt32((uint)(framelistAddrVirt->frPtr));
//            FRNUM.Write_UInt16((ushort)0u);

//            // set PIRQ
//            pciDevice.WriteRegister16(UHCI_Consts.UHCI_PCI_LEGACY_SUPPORT, UHCI_Consts.UHCI_PCI_LEGACY_SUPPORT_PIRQ);

//            // start hostcontroller and mark it configured with a 64-byte max packet
//            USBCMD.Write_UInt16((ushort)(UHCI_Consts.UHCI_CMD_RS | UHCI_Consts.UHCI_CMD_CF | UHCI_Consts.UHCI_CMD_MAXP));
//            USBINTR.Write_UInt16((ushort)(UHCI_Consts.UHCI_INT_MASK)); // switch on all interrupts

//            for (byte i = 0; i < RootPortCount; i++) // reset the CSC of the valid root ports
//            {
//                WritePort_SC1_16(i, (ushort)(UHCI_Consts.UHCI_PORT_CS_CHANGE));
//            }

//            USBCMD.Write_UInt16((ushort)(UHCI_Consts.UHCI_CMD_RS | UHCI_Consts.UHCI_CMD_CF | UHCI_Consts.UHCI_CMD_MAXP | UHCI_Consts.UHCI_CMD_FGR));
//            sleepMilliSeconds(20);
//            USBCMD.Write_UInt16((ushort)(UHCI_Consts.UHCI_CMD_RS | UHCI_Consts.UHCI_CMD_CF | UHCI_Consts.UHCI_CMD_MAXP));
//            sleepMilliSeconds(100);

//#if UHCI_TRACE
//            DBGMSG(((FOS_System.String)"Root-Ports   port1: ") + ReadPort_SC1_16(0) +
//                                                  "  port2: " + ReadPort_SC2_16(0));
//#endif

//            run = (USBCMD.Read_UInt16() & UHCI_Consts.UHCI_CMD_RS) != 0;

//            if ((USBSTS.Read_UInt16() & UHCI_Consts.UHCI_STS_HCHALTED) == 0)
//            {
//#if UHCI_TRACE
//                DBGMSG(((FOS_System.String)"RunStop bit: ") + run);
//#endif
//                //TODO: uhci_enablePorts(u); // attaches the ports
//            }
//#if UHCI_TRACE
//            else
//            {
//                DBGMSG("Fatal Error: UHCI - HCHalted. Ports will not be enabled.");
//                DBGMSG(((FOS_System.String)"RunStop Bit: ") + run + " Frame Number: " + FRNUM.Read_UInt16());
//            }
//#endif
//        }

//        //// ports
        
//        protected void EnablePorts()
//        {
//#if UHCI_TRACE
//            DBGMSG(">>> uhci_enablePorts <<<");
//#endif

//            for (byte j = 0; j < RootPortCount; j++)
//            {
//                ResetPort(j);
//                RootPorts.Add(new HCPort()
//                    {
//                        portNum = j
//                    });
//            }
            
//            enabledPortFlag = true;

//            for (byte j = 0; j < RootPortCount; j++)
//            {
//                ushort val = ReadPort_SC1_16(j);
//#if UHCI_TRACE
//                DBGMSG(((FOS_System.String)"port ") + (j + 1) + ": " + val);
//#endif
//                AnalysePortStatus(j, val);
//            }
//        }

//        protected void ResetPort(byte port)
//        {
//            WritePort_SC1_16(port, UHCI_Consts.UHCI_PORT_RESET);
//            sleepMilliSeconds(50); // do not delete this wait
//            WritePort_SC1_16(port, (ushort)(ReadPort_SC1_16(port) & ~UHCI_Consts.UHCI_PORT_RESET)); // clear reset bit

//            // wait and check, whether reset bit is really zero
//            uint timeout = 20;
//            while ((ReadPort_SC1_16(port) & UHCI_Consts.UHCI_PORT_RESET) != 0)
//            {
//                sleepMilliSeconds(20);
//                timeout--;
//                if (timeout == 0)
//                {
//#if UHCI_TRACE
//                    DBGMSG(((FOS_System.String)"Timeour Error: Port ") + (port + 1) + " did not reset! ");
//                    DBGMSG(((FOS_System.String)"Port Status: ") + ReadPort_SC1_16(port));
//                    BasicConsole.DelayOutput(5);
//#endif
//                    break;
//                }
//            }

//            sleepMilliSeconds(10);

//            // Enable

//            WritePort_SC1_16(port, (ushort)(UHCI_Consts.UHCI_PORT_CS_CHANGE | // clear Change-Bits Connected and Enabled
//                                            UHCI_Consts.UHCI_PORT_ENABLE_CHANGE | // clear as per above
//                                            UHCI_Consts.UHCI_PORT_ENABLE));       // set Enable-Bit
//            sleepMilliSeconds(10);

//#if UHCI_TRACE
//            DBGMSG(((FOS_System.String)"Port Status: ") + ReadPort_SC1_16(port));
//            BasicConsole.DelayOutput(5);
//#endif
//        }

//        protected static void InterruptHandler(FOS_System.Object data)
//        {
//            ((UHCI)data).InterruptHandler();
//        }
//        protected void InterruptHandler()
//        {
//            ushort val = USBSTS.Read_UInt16();

//            if (val == 0) // Interrupt came from another UHCI device
//            {
//#if UHCI_TRACE
//                DBGMSG("Interrupt came from another UHCI device!");
//#endif
//                return;
//            }

//            if ((val & UHCI_Consts.UHCI_STS_USBINT) == 0)
//            {
//#if UHCI_TRACE
//                DBGMSG("USB UHCI Interrupt");
//#endif
//            }

//            if ((val & UHCI_Consts.UHCI_STS_USBINT) != 0)
//            {
//#if UHCI_TRACE
//                DBGMSG(((FOS_System.String)"Frame: ") + FRNUM.Read_UInt16() + " - USB transaction completed");
//#endif
//                USBSTS.Write_UInt16((ushort)UHCI_Consts.UHCI_STS_USBINT); // reset interrupt
//            }

//            if ((val & UHCI_Consts.UHCI_STS_RESUME_DETECT) != 0)
//            {
//#if UHCI_TRACE
//                DBGMSG("Resume Detect");
//#endif
//                USBSTS.Write_UInt16((ushort)UHCI_Consts.UHCI_STS_RESUME_DETECT); // reset interrupt
//            }

//            if ((val & UHCI_Consts.UHCI_STS_HCHALTED) != 0)
//            {
//#if UHCI_TRACE
//                DBGMSG("Host Controller Halted");
//#endif
//                USBSTS.Write_UInt16((ushort)UHCI_Consts.UHCI_STS_HCHALTED); // reset interrupt
//            }

//            if ((val & UHCI_Consts.UHCI_STS_HC_PROCESS_ERROR) != 0)
//            {
//#if UHCI_TRACE
//                DBGMSG("Host Controller Process Error");
//#endif
//                USBSTS.Write_UInt16((ushort)UHCI_Consts.UHCI_STS_HC_PROCESS_ERROR); // reset interrupt
//            }

//            if ((val & UHCI_Consts.UHCI_STS_USB_ERROR) != 0)
//            {
//#if UHCI_TRACE
//                DBGMSG("USB Error");
//#endif
//                USBSTS.Write_UInt16((ushort)UHCI_Consts.UHCI_STS_USB_ERROR); // reset interrupt
//            }

//            if ((val & UHCI_Consts.UHCI_STS_HOST_SYSTEM_ERROR) != 0)
//            {
//#if UHCI_TRACE
//                DBGMSG("Host System Error");
//#endif
//                USBSTS.Write_UInt16((ushort)UHCI_Consts.UHCI_STS_HOST_SYSTEM_ERROR); // reset interrupt
//                //pci_analyzeHostSystemError(u->PCIdevice);
//            }
//        }

//        protected static void ShowPortState(ushort val)
//        {
//#if UHCI_TRACE
//            if ((val & UHCI_Consts.UHCI_PORT_RESET) != 0) { DBGMSG(" - RESET"); }

//            if ((val & UHCI_Consts.UHCI_SUSPEND) != 0) { DBGMSG(" - SUSPEND"); }
//            if ((val & UHCI_Consts.UHCI_PORT_RESUME_DETECT) != 0) { DBGMSG(" - RESUME DETECT"); }

//            if ((val & UHCI_Consts.UHCI_PORT_LOWSPEED_DEVICE) != 0) { DBGMSG(" - LOWSPEED DEVICE"); }
//            else { DBGMSG(" - FULLSPEED DEVICE"); }
//            if ((val & Utils.BIT(5)) != 0) { DBGMSG(" - Line State: D-"); }
//            if ((val & Utils.BIT(4)) != 0) { DBGMSG(" - Line State: D+"); }

//            if ((val & UHCI_Consts.UHCI_PORT_ENABLE_CHANGE) != 0) { DBGMSG(" - ENABLE CHANGE"); }
//            if ((val & UHCI_Consts.UHCI_PORT_ENABLE) != 0) { DBGMSG(" - ENABLED"); }

//            if ((val & UHCI_Consts.UHCI_PORT_CS_CHANGE) != 0) { DBGMSG(" - DEVICE CHANGE"); }
//            if ((val & UHCI_Consts.UHCI_PORT_CS) != 0) { DBGMSG(" - DEVICE ATTACHED"); }
//            else { DBGMSG(" - NO DEVICE ATTACHED"); }
//#endif
//        }

//        protected void AnalysePortStatus(byte j, ushort val)
//        {
//            HCPort port = GetPort(j);
//            if ((val & UHCI_Consts.UHCI_PORT_LOWSPEED_DEVICE) != 0)
//            {
//#if UHCI_TRACE
//                DBGMSG("Low-speed device");
//#endif
//                port.speed = USBPortSpeed.Low;
//            }
//            else
//            {
//#if UHCI_TRACE
//                DBGMSG("Full-speed device");
//#endif
//                port.speed = USBPortSpeed.Full;
//            }

//            if (((val & UHCI_Consts.UHCI_PORT_CS) != 0) && !port.connected)
//            {
//#if UHCI_TRACE
//                DBGMSG(" - attached.");
//#endif
//                port.connected = true;
//                ResetPort(j);      // reset on attached
//#if UHCI_TRACE
//                BasicConsole.DelayOutput(2);
//#endif

//                SetupUSBDevice(j);
//            }
//            else if (port.connected)
//            {
//#if UHCI_TRACE
//                DBGMSG(" - removed.");
//#endif
//                port.connected = false;

//                if (port.deviceInfo != null)
//                {
//                    port.deviceInfo.FreePort();
//                }
//            }
//#if UHCI_TRACE
//            ShowPortState(val);
//#endif
//        }

//        //void uhci_pollDisk(port_t* dev)
//        //{
//        //    uhci_t* u = (uhci_t*)((hc_port_t*)dev->data)->hc;

//        //    for (byte j=0; j < u->hc.rootPortCount; j++)
//        //    {
//        //        ushort val = inportw(u->bar + UHCI_PORTSC1 + 2*j);

//        //        if (val & UHCI_PORT_CS_CHANGE)
//        //        {
//        //            DBGMSG("UHCI %u: Port %u changed: ", u->num, j+1);
//        //            outportw(u->bar + UHCI_PORTSC1 + 2*j, UHCI_PORT_CS_CHANGE);
//        //            uhci_analysePortStatus(u, j, val);
//        //        }
//        //    }
//        //}


//        //*******************************************************************************************************/
//        //*                                                                                                      *
//        //*                                            Transactions                                              *
//        //*                                                                                                      *
//        //*******************************************************************************************************/

//        //typedef struct
//        //{
//        //    uhciTD_t*   TD;
//        //    void*       TDBuffer;
//        //    void*       inBuffer;
//        //    size_t      inLength;
//        //} uhci_transaction_t;

//        //static bool isTransactionSuccessful(uhci_transaction_t* uT);
//        //static void uhci_showStatusbyteTD(uhciTD_t* TD);

//        //void uhci_setupTransfer(usb_transfer_t* transfer)
//        //{
//        //    uhci_t* u = (uhci_t*)((hc_port_t*)transfer->device->port->data)->hc;
//        //    transfer->data = u->qhPointerVirt; // QH
//        //}

//        //void uhci_setupTransaction(usb_transfer_t* transfer, usb_transaction_t* usbTransaction, bool toggle, uint tokenBytes, byte type, byte req, byte hiVal, byte loVal, ushort i, ushort length)
//        //{
//        //    uhci_transaction_t* uT = usbTransaction->data = malloc(sizeof(uhci_transaction_t), 0, "uhci_transaction_t");
//        //    uT->inBuffer = 0;
//        //    uT->inLength = 0;

//        //    uhci_t* u = (uhci_t*)((hc_port_t*)transfer->device->port->data)->hc;

//        //    uT->TD = uhci_createTD_SETUP(u, transfer->data, 1, toggle, tokenBytes, type, req, hiVal, loVal, i, length, &uT->TDBuffer, transfer->device->num, transfer->endpoint, transfer->packetSize);

//        //#if UHCI_TRACE
//        //    usb_request_t* request = (usb_request_t*)uT->TDBuffer;
//        //    DBGMSG("type: %u req: %u valHi: %u valLo: %u i: %u len: %u", request->type, request->request, request->valueHi, request->valueLo, request->index, request->length);
//        //#endif

//        //    /// TEST
//        //    textColor(LIGHT_GRAY);
//        //    DBGMSG("uhci_setup - \ttoggle: %u \tlength: %u \tdev: %u \tendp: %u", toggle, length, transfer->device->num, transfer->endpoint);
//        //    textColor(TEXT);
//        //    /// TEST

//        //    if (transfer->transactions.tail)
//        //    {
//        //        uhci_transaction_t* uhciLastTransaction = ((usb_transaction_t*)transfer->transactions.tail->data)->data;
//        //        uhciLastTransaction->TD->next = (paging_getPhysAddr(uT->TD) & 0xFFFFFFF0) | BIT_Vf; // build TD queue
//        //        uhciLastTransaction->TD->q_next = uT->TD;
//        //    }
//        //}

//        //void uhci_inTransaction(usb_transfer_t* transfer, usb_transaction_t* usbTransaction, bool toggle, void* buffer, size_t length)
//        //{
//        //    uhci_transaction_t* uT = usbTransaction->data = malloc(sizeof(uhci_transaction_t), 0, "uhci_transaction_t");
//        //    uT->inBuffer = buffer;
//        //    uT->inLength = length;

//        //    uhci_t* u = (uhci_t*)((hc_port_t*)transfer->device->port->data)->hc;

//        //    uT->TD = uhci_createTD_IO(u, transfer->data, 1, UHCI_TD_IN, toggle, length, transfer->device->num, transfer->endpoint, transfer->packetSize);
//        //    uT->TDBuffer = uT->TD->virtBuffer;

//        //    /// TEST
//        //    textColor(LIGHT_BLUE);
//        //    DBGMSG("uhci_in - \ttoggle: %u \tlength: %u \tdev: %u \tendp: %u", toggle, length, transfer->device->num, transfer->endpoint);
//        //    textColor(TEXT);
//        //    /// TEST

//        //    if (transfer->transactions.tail)
//        //    {
//        //        uhci_transaction_t* uhciLastTransaction = ((usb_transaction_t*)transfer->transactions.tail->data)->data;
//        //        uhciLastTransaction->TD->next = (paging_getPhysAddr(uT->TD) & 0xFFFFFFF0) | BIT_Vf; // build TD queue
//        //        uhciLastTransaction->TD->q_next = uT->TD;
//        //    }
//        //}

//        //void uhci_outTransaction(usb_transfer_t* transfer, usb_transaction_t* usbTransaction, bool toggle, void* buffer, size_t length)
//        //{
//        //    uhci_transaction_t* uT = usbTransaction->data = malloc(sizeof(uhci_transaction_t), 0, "uhci_transaction_t");
//        //    uT->inBuffer = 0;
//        //    uT->inLength = 0;

//        //    uhci_t* u = (uhci_t*)((hc_port_t*)transfer->device->port->data)->hc;

//        //    uT->TD = uhci_createTD_IO(u, transfer->data, 1, UHCI_TD_OUT, toggle, length, transfer->device->num, transfer->endpoint, transfer->packetSize);
//        //    uT->TDBuffer = uT->TD->virtBuffer;

//        //    if (buffer != 0 && length != 0)
//        //    {
//        //        memcpy(uT->TDBuffer, buffer, length);
//        //    }

//        //    /// TEST
//        //    textColor(LIGHT_GREEN);
//        //    DBGMSG("uhci_out - \ttoggle: %u \tlength: %u \tdev: %u \tendp: %u", toggle, length, transfer->device->num, transfer->endpoint);
//        //    textColor(TEXT);
//        //    /// TEST

//        //    if (transfer->transactions.tail)
//        //    {
//        //        uhci_transaction_t* uhciLastTransaction = ((usb_transaction_t*)transfer->transactions.tail->data)->data;
//        //        uhciLastTransaction->TD->next = (paging_getPhysAddr(uT->TD) & 0xFFFFFFF0) | BIT_Vf; // build TD queue
//        //        uhciLastTransaction->TD->q_next = uT->TD;
//        //    }
//        //}

//        //void uhci_issueTransfer(usb_transfer_t* transfer)
//        //{
//        //    uhci_t* u = (uhci_t*)((hc_port_t*)transfer->device->port->data)->hc; // HC
//        //    uhci_transaction_t* firstTransaction = ((usb_transaction_t*)transfer->transactions.head->data)->data;
//        //    uhci_createQH(u, transfer->data, (uintptr_t)transfer->data, firstTransaction->TD);

//        //    for (byte i = 0; i < NUMBER_OF_UHCI_RETRIES && !transfer->success; i++)
//        //    {
//        //#if UHCI_TRACE
//        //        DBGMSG("transfer try = %u", i);
//        //#endif

//        //        transfer->success = true;

//        //        // start scheduler
//        //        outportl(u->bar + UHCI_FRBASEADD, paging_getPhysAddr((void*)u->framelistAddrVirt->frPtr));
//        //        outportw(u->bar + UHCI_FRNUM, 0);
//        //        outportw(u->bar + UHCI_USBCMD, inportw(u->bar + UHCI_USBCMD) | UHCI_CMD_RS);

//        //        // run transactions
//        //        for (dlelement_t* elem = transfer->transactions.head; elem != 0; elem = elem->next)
//        //        {
//        //#if UHCI_TRACE
//        //            ushort num = inportw(u->bar + UHCI_FRNUM);
//        //            DBGMSG("FRBADDR: %X  frame pointer: %X frame number: %u", inportl(u->bar + UHCI_FRBASEADD), u->framelistAddrVirt->frPtr[num], num);
//        //#endif

//        //            delay(50000); // pause after transaction
//        //        }
//        //        delay(50000); // pause after transfer

//        //        // stop scheduler
//        //        outportw(u->bar + UHCI_USBCMD, inportw(u->bar + UHCI_USBCMD) & ~UHCI_CMD_RS);

//        //        // check conditions and save data
//        //        for (dlelement_t* elem = transfer->transactions.head; elem != 0; elem = elem->next)
//        //        {
//        //            uhci_transaction_t* uT = ((usb_transaction_t*)elem->data)->data;
//        //            uhci_showStatusbyteTD(uT->TD);
//        //            transfer->success = transfer->success && isTransactionSuccessful(uT); // executed w/o error

//        //            if (uT->inBuffer != 0 && uT->inLength != 0)
//        //            {
//        //                memcpy(uT->inBuffer, uT->TDBuffer, uT->inLength);
//        //            }
//        //        }

//        //#if UHCI_TRACE
//        //        DBGMSG("QH: %X  QH->transfer: %X", paging_getPhysAddr(transfer->data), ((uhciQH_t*)transfer->data)->transfer);

//        //        for (dlelement_t* elem = transfer->transactions.head; elem != 0; elem = elem->next)
//        //        {
//        //            uhci_transaction_t* uT = ((usb_transaction_t*)elem->data)->data;
//        //            DBGMSG("TD: %X next: %X", paging_getPhysAddr(uT->TD), uT->TD->next);
//        //        }
//        //#endif

//        //        if (transfer->success)
//        //        {
//        //#if UHCI_TRACE
//        //            textColor(SUCCESS);
//        //            DBGMSG("Transfer successful.");
//        //            textColor(TEXT);
//        //#endif
//        //        }
//        //        else
//        //        {
//        //            textColor(ERROR);
//        //            DBGMSG("Transfer failed.");
//        //            textColor(TEXT);
//        //        }
//        //    }
//        //}


//        //*******************************************************************************************************/
//        //*                                                                                                      *
//        //*                                            uhci QH TD functions                                      *
//        //*                                                                                                      *
//        //*******************************************************************************************************/

//        //static uhciTD_t* uhci_allocTD(uintptr_t next)
//        //{
//        //    uhciTD_t* td = (uhciTD_t*)malloc(sizeof(uhciTD_t), 16, "uhciTD"); // 16 byte alignment
//        //    memset(td, 0, sizeof(uhciTD_t));

//        //    if (next != BIT(0))
//        //    {
//        //        td->next   = (paging_getPhysAddr((void*)next) & 0xFFFFFFF0) | BIT_Vf;
//        //        td->q_next = (void*)next;
//        //    }
//        //    else
//        //    {
//        //        td->next = BIT_T;
//        //    }

//        //    td->active             = 1;  // to be executed
//        //    td->intOnComplete      = 1;  // We want an interrupt after complete transfer
//        //    td->PacketID           = UHCI_TD_SETUP;
//        //    td->maxLength          = 0x3F; // 64 byte // uhci, rev. 1.1, page 24

//        //    return td;
//        //}

//        //static void* uhci_allocTDbuffer(uhciTD_t* td)
//        //{
//        //    td->virtBuffer = malloc(1024, 0, "uhciTD-buffer");
//        //    memset(td->virtBuffer, 0, 1024);
//        //    td->buffer = paging_getPhysAddr(td->virtBuffer);

//        //    return td->virtBuffer;
//        //}


//        //uhciTD_t* uhci_createTD_SETUP(uhci_t* u, uhciQH_t* uQH, uintptr_t next, bool toggle, uint tokenBytes, byte type, byte req, byte hiVal, byte loVal, ushort i, ushort length, void** buffer, uint device, uint endpoint, uint packetSize)
//        //{
//        //    uhciTD_t* td = uhci_allocTD(next);

//        //    td->PacketID      = UHCI_TD_SETUP;

//        //    td->dataToggle    = toggle; // Should be toggled every list entry

//        //    td->deviceAddress = device;
//        //    td->endpoint      = endpoint;
//        //    td->maxLength     = tokenBytes-1;

//        //    usb_request_t* request = *buffer = td->virtBuffer = uhci_allocTDbuffer(td);
//        //    request->type    = type;
//        //    request->request = req;
//        //    request->valueHi = hiVal;
//        //    request->valueLo = loVal;
//        //    request->index   = i;
//        //    request->length  = length;

//        //    uQH->q_last = td;
//        //    return (td);
//        //}

//        //uhciTD_t* uhci_createTD_IO(uhci_t* u, uhciQH_t* uQH, uintptr_t next, byte direction, bool toggle, uint tokenBytes, uint device, uint endpoint, uint packetSize)
//        //{
//        //    uhciTD_t* td = uhci_allocTD(next);

//        //    td->PacketID      = direction;

//        //    if (tokenBytes)
//        //    {
//        //        td->maxLength = (tokenBytes-1) & 0x7FF;
//        //    }
//        //    else
//        //    {
//        //        td->maxLength = 0x7FF;
//        //    }

//        //    td->dataToggle    = toggle; // Should be toggled every list entry

//        //    td->deviceAddress = device;
//        //    td->endpoint      = endpoint;

//        //    td->virtBuffer    = uhci_allocTDbuffer(td);
//        //    td->buffer        = paging_getPhysAddr(td->virtBuffer);

//        //    uQH->q_last = td;
//        //    return (td);
//        //}

//        //void uhci_createQH(uhci_t* u, uhciQH_t* head, uint horizPtr, uhciTD_t* firstTD)
//        //{
//        //    head->next = BIT_T; // (paging_getPhysAddr((void*)horizPtr) & 0xFFFFFFF0) | BIT_QH;

//        //    if (firstTD == 0)
//        //    {
//        //        head->transfer = BIT_T;
//        //    }

//        //    else
//        //    {
//        //        head->transfer = (paging_getPhysAddr(firstTD) & 0xFFFFFFF0);
//        //        head->q_first  = firstTD;
//        //    }
//        //}


//        //////////////////////
//        //// analysis tools //
//        //////////////////////

//        //void uhci_showStatusbyteTD(uhciTD_t* TD)
//        //{
//        //    textColor(ERROR);
//        //    if (TD->bitstuffError)     DBGMSG("Bitstuff Error");          // receive data stream contained a sequence of more than 6 ones in a row
//        //    if (TD->crc_timeoutError)  DBGMSG("No Response from Device"); // no response from the device (CRC or timeout)
//        //    if (TD->nakReceived)       DBGMSG("NAK received");            // NAK handshake
//        //    if (TD->babbleDetected)    DBGMSG("Babble detected");         // Babble (fatal error), e.g. more data from the device than MAXP
//        //    if (TD->dataBufferError)   DBGMSG("Data Buffer Error");       // HC cannot keep up with the data  (overrun) or cannot supply data fast enough (underrun)
//        //    if (TD->stall)             DBGMSG("Stalled");                 // can be caused by babble, error counter (0) or STALL from device

//        //    textColor(GRAY);
//        //    if (TD->active)            DBGMSG("active");                  // 1: HC will execute   0: set by HC after excution (HC will not excute next time)

//        //#if UHCI_TRACE
//        //    textColor(IMPORTANT);
//        //    if (TD->intOnComplete)     DBGMSG("interrupt on complete");   // 1: HC issues interrupt on completion of the frame in which the TD is executed
//        //    if (TD->isochrSelect)      DBGMSG("isochronous TD");          // 1: Isochronous TD
//        //    if (TD->lowSpeedDevice)    DBGMSG("Lowspeed Device");         // 1: LS   0: FS
//        //    if (TD->shortPacketDetect) DBGMSG("ShortPacketDetect");       // 1: enable   0: disable
//        //#endif

//        //    textColor(TEXT);
//        //}

//        //bool isTransactionSuccessful(uhci_transaction_t* uT)
//        //{
//        //    return
//        //    (
//        //        // no error
//        //        (uT->TD->bitstuffError  == 0) && (uT->TD->crc_timeoutError == 0) && (uT->TD->nakReceived == 0) &&
//        //        (uT->TD->babbleDetected == 0) && (uT->TD->dataBufferError  == 0) && (uT->TD->stall       == 0) &&
//        //        // executed
//        //        (uT->TD->active == 0)
//        //    );
//        //}


//        protected override void _INTransaction(USBTransfer transfer, USBTransaction uTransaction, bool toggle, void* buffer, ushort length)
//        {
//            ExceptionMethods.Throw(new FOS_System.Exceptions.NotSupportedException("UHCI method not implemented."));
//        }
//        protected override void _IssueTransfer(USBTransfer transfer)
//        {
//            ExceptionMethods.Throw(new FOS_System.Exceptions.NotSupportedException("UHCI method not implemented."));
//        }
//        protected override void _OUTTransaction(USBTransfer transfer, USBTransaction uTransaction, bool toggle, void* buffer, ushort length)
//        {
//            ExceptionMethods.Throw(new FOS_System.Exceptions.NotSupportedException("UHCI method not implemented."));
//        }
//        protected override void _SETUPTransaction(USBTransfer transfer, USBTransaction uTransaction, bool toggle, ushort tokenBytes, byte type, byte req, byte hiVal, byte loVal, ushort index, ushort length)
//        {
//            ExceptionMethods.Throw(new FOS_System.Exceptions.NotSupportedException("UHCI method not implemented."));
//        }
//        protected override void _SetupTransfer(USBTransfer transfer)
//        {
//            ExceptionMethods.Throw(new FOS_System.Exceptions.NotSupportedException("UHCI method not implemented."));
//        }
        
//#if UHCI_TRACE
//        private static void DBGMSG(FOS_System.String msg)
//        {
//            BasicConsole.WriteLine(msg);
//        }
//#endif

//        protected void sleepMilliSeconds(uint length)
//        {
//            for (uint i = 0; i < length; i++)
//            {
//                for (uint j = 0; j < 100000; j++)
//                    ;
//            }
//        }

//        protected void WritePort_SC1_16(byte port, ushort value)
//        {
//            IO.IOPort.doWrite_UInt16((ushort)(usbBaseAddress + UHCI_Consts.UHCI_PORTSC1 + port * 2), value);
//        }
//        protected ushort ReadPort_SC1_16(byte port)
//        {
//            return IO.IOPort.doRead_UInt16((ushort)(usbBaseAddress + UHCI_Consts.UHCI_PORTSC1 + port * 2));
//        }
//        protected void WritePort_SC2_16(byte port, ushort value)
//        {
//            IO.IOPort.doWrite_UInt16((ushort)(usbBaseAddress + UHCI_Consts.UHCI_PORTSC2 + port * 2), value);
//        }
//        protected ushort ReadPort_SC2_16(byte port)
//        {
//            return IO.IOPort.doRead_UInt16((ushort)(usbBaseAddress + UHCI_Consts.UHCI_PORTSC2 + port * 2));
//        }
//    }

//    // Transfer Descriptors (TD) are always aligned on 16-byte boundaries.
//    // All transfer descriptors have the same basic, 32-byte structure.
//    // The last 4 DWORDs are for software use.
//    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
//    public struct UHCI_qTD_Struct
//    {
//        public uint u0;
//        public uint u1;
//        public uint u2;
//        public uint u3;
//        public uint u4;
//        public uint u5;
//        public uint u6;
//        public uint u7;
//    }
//    public unsafe class UHCI_qTD
//    {
//        //TODO - Bit shift the values!!!

//        public UHCI_qTD_Struct* qtd;

//        public uint next
//        {
//            get
//            {
//                return qtd->u0;
//            }
//            set
//            {
//                qtd->u0 = value;
//            }
//        }

//        //    // TD CONTROL AND STATUS  (DWORD 1)
//        public uint actualLength
//        {
//            get
//            {
//                return (qtd->u1 & 0x000007FFu);
//            }
//            set
//            {
//                qtd->u1 = (qtd->u1 & 0xFFFFF800u) | (value & 0x000007FFu);
//            }
//        }
//        public uint bitstuffError
//        {
//            get
//            {
//                return (qtd->u1 & 0x00020000u);
//            }
//            set
//            {
//                qtd->u1 = (qtd->u1 & 0xFFFDFFFFu) | (value & 0x00020000u);
//            }
//        }
//        public uint crc_timeoutError
//        {
//            get
//            {
//                return (qtd->u1 & 0x00040000u);
//            }
//            set
//            {
//                qtd->u1 = (qtd->u1 & 0xFFFBFFFFu) | (value & 0x00040000u);
//            }
//        }
//        public uint nakReceived
//        {
//            get
//            {
//                return (qtd->u1 & 0x00080000u);
//            }
//            set
//            {
//                qtd->u1 = (qtd->u1 & 0xFFF7FFFFu) | (value & 0x00080000u);
//            }
//        }
//        public uint babbleDetected
//        {
//            get
//            {
//                return (qtd->u1 & 0x00100000u);
//            }
//            set
//            {
//                qtd->u1 = (qtd->u1 & 0xFFEFFFFFu) | (value & 0x00100000u);
//            }
//        }
//        public uint dataBufferError
//        {
//            get
//            {
//                return (qtd->u1 & 0x00200000u);
//            }
//            set
//            {
//                qtd->u1 = (qtd->u1 & 0xFFDFFFFFu) | (value & 0x00200000u);
//            }
//        }
//        public uint stall
//        {
//            get
//            {
//                return (qtd->u1 & 0x00400000u);
//            }
//            set
//            {
//                qtd->u1 = (qtd->u1 & 0xFFBFFFFFu) | (value & 0x00400000u);
//            }
//        }
//        public uint active
//        {
//            get
//            {
//                return (qtd->u1 & 0x00800000u);
//            }
//            set
//            {
//                qtd->u1 = (qtd->u1 & 0xFF7FFFFFu) | (value & 0x00800000u);
//            }
//        }
//        public uint intOnComplete
//        {
//            get
//            {
//                return (qtd->u1 & 0x01000000u);
//            }
//            set
//            {
//                qtd->u1 = (qtd->u1 & 0xFEFFFFFFu) | (value & 0x01000000u);
//            }
//        }
//        public uint isochrSelect
//        {
//            get
//            {
//                return (qtd->u1 & 0x02000000u);
//            }
//            set
//            {
//                qtd->u1 = (qtd->u1 & 0xFDFFFFFFu) | (value & 0x02000000u);
//            }
//        }
//        public uint lowSpeedDevice
//        {
//            get
//            {
//                return (qtd->u1 & 0x04000000u);
//            }
//            set
//            {
//                qtd->u1 = (qtd->u1 & 0xFBFFFFFFu) | (value & 0x04000000u);
//            }
//        }
//        public uint errorCounter
//        {
//            get
//            {
//                return (qtd->u1 & 0x18000000u);
//            }
//            set
//            {
//                qtd->u1 = (qtd->u1 & 0xE7FFFFFFu) | (value & 0x18000000u);
//            }
//        }
//        public uint shortPacketDetect
//        {
//            get
//            {
//                return (qtd->u1 & 0x20000000u);
//            }
//            set
//            {
//                qtd->u1 = (qtd->u1 & 0xDFFFFFFFu) | (value & 0x20000000u);
//            }
//        }

//        //    // TD TOKEN  (DWORD 2)
//        public uint PacketID
//        {
//            get
//            {
//                return (qtd->u2 & 0x0000007Fu);
//            }
//            set
//            {
//                qtd->u2 = (qtd->u2 & 0xFFFFFF80u) | (value & 0x0000007Fu);
//            }
//        }
//        public uint deviceAddress
//        {
//            get
//            {
//                return (qtd->u2 & 0x00007F00u);
//            }
//            set
//            {
//                qtd->u2 = (qtd->u2 & 0xFFFF80FFu) | (value & 0x00007F00u);
//            }
//        }
//        public uint endpoint
//        {
//            get
//            {
//                return (qtd->u2 & 0x00078000u);
//            }
//            set
//            {
//                qtd->u2 = (qtd->u2 & 0xFFF87FFFu) | (value & 0x00078000u);
//            }
//        }
//        public uint dataToggle
//        {
//            get
//            {
//                return (qtd->u2 & 0x00080000u);
//            }
//            set
//            {
//                qtd->u2 = (qtd->u2 & 0xFFF7FFFFu) | (value & 0x00080000u);
//            }
//        }
//        public uint maxLength
//        {
//            get
//            {
//                return (qtd->u1 & 0xFFE00000u);
//            }
//            set
//            {
//                qtd->u1 = (qtd->u1 & 0x001FFFFFu) | (value & 0xFFE00000u);
//            }
//        }


//        //    // TD BUFFER POINTER (DWORD 3)
//        public uint* buffer
//        {
//            get
//            {
//                return (uint*)qtd->u3;
//            }
//            set
//            {
//                qtd->u3 = (uint)value;
//            }
//        }

//        public UHCI_qTD_Struct* q_next
//        {
//            get
//            {
//                return (UHCI_qTD_Struct*)qtd->u4;
//            }
//            set
//            {
//                qtd->u4 = (uint)value;
//            }
//        }
//        public void* virtBuffer
//        {
//            get
//            {
//                return (void*)qtd->u5;
//            }
//            set
//            {
//                qtd->u5 = (uint)value;
//            }
//        }

//        /// <summary>
//        /// Initializes a new qTD with new data structure.
//        /// </summary>
//        public UHCI_qTD()
//        {
//            qtd = (UHCI_qTD_Struct*)FOS_System.Heap.Alloc((uint)sizeof(UHCI_qTD_Struct), 16);
//        }
//        /// <summary>
//        /// Initializes a qTD with specified underlying data structure.
//        /// </summary>
//        /// <param name="aqTD">The existing underlying data structure.</param>
//        public UHCI_qTD(UHCI_qTD_Struct* aqTD)
//        {
//            qtd = aqTD;
//        }

//        /// <summary>
//        /// Frees the underlying memory structure.
//        /// </summary>
//        [Compiler.NoGC]
//        public void Free()
//        {
//            FOS_System.Heap.Free(qtd);
//            qtd = null;
//        }
//    }
//    //public struct UHCI_qTD
//    //{
//    //    // pointer to another TD or QH
//    //    // inclusive control bits  (DWORD 0)
//    //    uint next;

//    //    // TD CONTROL AND STATUS  (DWORD 1)
//    //    uint actualLength      : 11; //  0-10
//    //    uint reserved1         :  5; // 11-15
//    //    uint reserved2         :  1; //    16
//    //    uint bitstuffError     :  1; //    17
//    //    uint crc_timeoutError  :  1; //    18
//    //    uint nakReceived       :  1; //    19
//    //    uint babbleDetected    :  1; //    20
//    //    uint dataBufferError   :  1; //    21
//    //    uint stall             :  1; //    22
//    //    uint active            :  1; //    23
//    //    uint intOnComplete     :  1; //    24
//    //    uint isochrSelect      :  1; //    25
//    //    uint lowSpeedDevice    :  1; //    26
//    //    uint errorCounter      :  2; // 27-28
//    //    uint shortPacketDetect :  1; //    29
//    //    uint reserved3         :  2; // 30-31

//    //    // TD TOKEN  (DWORD 2)
//    //    uint PacketID          :  8; //  0-7
//    //    uint deviceAddress     :  7; //  8-14
//    //    uint endpoint          :  4; // 15-18
//    //    uint dataToggle        :  1; //    19
//    //    uint reserved4         :  1; //    20
//    //    uint maxLength         : 11; // 21-31

//    //    // TD BUFFER POINTER (DWORD 3)
//    //    uintptr_t        buffer;

//    //    // RESERVED FOR SOFTWARE (DWORDS 4-7)
//    //    struct uhci_td*  q_next;
//    //    void*            virtBuffer;
//    //    uint         dWord6; // ?
//    //    uint         dWord7; // ?
//    //} __attribute__((packed)) uhciTD_t;

//    // Queue Heads support the requirements of Control, Bulk, and Interrupt transfers
//    // and must be aligned on a 16-byte boundary
//    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
//    public unsafe struct UHCI_QueueHead_Struct
//    {
//        public UHCI_QueueHead_Struct* next;
//        public uint* transfer;
//        public UHCI_qTD_Struct* q_first;
//        public UHCI_qTD_Struct* q_last;
//    }
//    public unsafe class UHCI_QueueHead
//    {
//        public UHCI_QueueHead_Struct* queueHead;

//        /// <summary>
//        /// Initializes a new queue head with empty underlying memory structure.
//        /// </summary>
//        public UHCI_QueueHead()
//        {
//            queueHead = (UHCI_QueueHead_Struct*)FOS_System.Heap.Alloc((uint)sizeof(UHCI_QueueHead_Struct), 16);
//        }
//        /// <summary>
//        /// Initializes a new queue head with specified underlying memory structure.
//        /// </summary>
//        /// <param name="aQueueHead">The existing underlying queue head.</param>
//        public UHCI_QueueHead(UHCI_QueueHead_Struct* aQueueHead)
//        {
//            queueHead = aQueueHead;
//        }

//        /// <summary>
//        /// Frees the underlying memory structure.
//        /// </summary>
//        [Compiler.NoGC]
//        public void Free()
//        {
//            FOS_System.Heap.Free(queueHead);
//            queueHead = null;
//        }
//    }
//    //typedef struct
//    //{
//    //    // QUEUE HEAD LINK POINTER
//    //    // inclusive control bits (DWORD 0)
//    //    uintptr_t   next;

//    //    // QUEUE ELEMENT LINK POINTER
//    //    // inclusive control bits (DWORD 1)
//    //    uintptr_t   transfer;

//    //    // TDs
//    //    uhciTD_t* q_first;
//    //    uhciTD_t* q_last;
//    //} __attribute__((packed)) uhciQH_t;

//    public unsafe struct frPtr_Struct
//    {
//        public uint** frPtr;
//    }
//    //typedef struct
//    //{
//    //    uintptr_t frPtr[1024];
//    //} frPtr_t;


//    //typedef struct
//    //{
//    //    hc_t           hc;                // Generic HC data
//    //    pciDev_t*      PCIdevice;         // PCI device
//    //    ushort       bar;               // start of I/O space (base address register)
//    //    frPtr_t*       framelistAddrVirt; // virtual adress of frame list
//    //    uhciQH_t*      qhPointerVirt;     // virtual adress of QH
//    //    size_t         memSize;           // memory size of IO space
//    //    bool           enabledPortFlag;   // root ports enabled
//    //    bool           run;               // hc running (RS bit)
//    //    byte        num;               // Number of the UHCI
//    //} uhci_t;
}
