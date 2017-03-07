using Kernel.Framework.Processes;
using Kernel.Devices;
using Kernel.PCI;
using Drivers.Compiler.Attributes;
using Kernel.Framework;

namespace Kernel.Tasks.Driver
{
    public unsafe class AMDPCNetII : PCIDeviceNormal
    {
        private byte* IO_base;
        private byte* pBuffer_virt;
        private uint Buffer_phys;

        private static IOPort rap;
        private static IOPort rdp;
        private static IOPort bcr;

        private static byte* RX_DE_start;
        private static byte* TX_DE_start;
        private static byte* RX_start;
        private static byte* TX_start;

        private static uint RX_DE_phys;
        private static uint TX_DE_phys;
        private static uint RX_phys;
        private static uint TX_phys;

        private static byte RX_buffer_id = 0;
        private static byte TX_buffer_id = 0;

        private static byte DE_size = 16;
        private static ushort Buffer_size = 1548;
        private static byte RX_count = 32;
        private static byte TX_count = 8;

        // Controller Status Register (CSR0) Bits

        private static uint CSR0_ERR = 0x8000;
        private static uint CSR0_BABL = 0x4000;
        private static uint CSR0_CERR = 0x2000;
        private static uint CSR0_MISS = 0x1000;
        private static uint CSR0_MERR = 0x0800;
        private static uint CSR0_RINT = 0x0400;
        private static uint CSR0_TINT = 0x0200;
        private static uint CSR0_IDON = 0x0100;
        private static uint CSR0_INTR = 0x0080;
        private static uint CSR0_IENA = 0x0040;
        private static uint CSR0_RXON = 0x0020;
        private static uint CSR0_TXON = 0x0010;
        private static uint CSR0_TDMD = 0x0008;
        private static uint CSR0_STOP = 0x0004;
        private static uint CSR0_STRT = 0x0002;
        private static uint CSR0_INIT = 0x0001;

        byte[] eeprom_mac;

        private static bool InitDone = false;
        private static bool inInt = false;
        private static uint MissedInt = 0;

        private static int ReceivedSemaphoreId;

        [NoDebug]
        public AMDPCNetII(uint bus, uint slot, uint function)
            : base(bus, slot, function)
        {
            BasicConsole.WriteLine("AMDPCNetII > Constructor...");
        }

        public bool Init()
        {
            BasicConsole.WriteLine("AMDPCNetII > Initialization...");

            Claimed = true;

            SystemCallResults Result = SystemCalls.CreateSemaphore(-1, out ReceivedSemaphoreId);
            if (Result != SystemCallResults.OK)
            {
                ExceptionMethods.Throw(new Exception("Couldn't create the necessary semaphore!"));
            }

            if ((Command & PCIDevice.PCICommand.Master) == PCIDevice.PCICommand.Master)
                BasicConsole.WriteLine("AMDPCNetII > Bus mastering already enabled");

            Command = PCIDevice.PCICommand.IO | PCIDevice.PCICommand.Master | PCIDevice.PCICommand.Memory;

            IO_base = BaseAddresses[0].BaseAddress();

            BasicConsole.WriteLine("AMDPCNetII > allocate buffers memory");
            uint actualAddress;
            SystemCallResults mapFrameListPageResult = SystemCalls.RequestPages(16, out actualAddress);
            if (mapFrameListPageResult != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Error! AMDPCNetII cannot map page for Frame List.");
                ExceptionMethods.Throw(new Framework.Exception("AMDPCNetII cannot map page for buffers."));
            }
            pBuffer_virt = (byte*)actualAddress;
            Buffer_phys = (uint)GetPhysicalAddress(pBuffer_virt);

            BasicConsole.Write("AMDPCNetII > pBuffer_virt: ");
            BasicConsole.WriteLine((uint)pBuffer_virt);
            BasicConsole.Write("AMDPCNetII > buffer_phys: ");
            BasicConsole.WriteLine(Buffer_phys);

            rap = new IOPort((ushort)IO_base, 0x14);
            rdp = new IOPort((ushort)IO_base, 0x10);
            bcr = new IOPort((ushort)IO_base, 0x1C);

            // Set 32-Bit mode
            rdp.Write_UInt32(0x00);

            // SWStyle to 2
            write_bcr32(20, 0x2);

            // ASEL enable
            write_bcr32(2, read_bcr32(2) | 0x02);

            IOPort MAC1 = new IOPort((ushort)IO_base, 0x00);
            IOPort MAC2 = new IOPort((ushort)IO_base, 0x04);

            eeprom_mac = new byte[6];

            uint Mac1 = MAC1.Read_UInt32();
            uint Mac2 = MAC2.Read_UInt32();

            eeprom_mac[0] = (byte)(Mac1 & 0xFF);
            eeprom_mac[1] = (byte)((Mac1 & 0xFF00) >> 0x8);
            eeprom_mac[2] = (byte)((Mac1 & 0xFF0000) >> 0x10);
            eeprom_mac[3] = (byte)((Mac1 & 0xFF000000) >> 0x18);
            eeprom_mac[4] = (byte)(Mac2 & 0xFF);
            eeprom_mac[5] = (byte)((Mac2 & 0xFF00) >> 0x8);

            BasicConsole.Write("AMDPCNetII > EEPROM MAC : ");
            for (byte i = 0; i < 6; i++)
            {
                BasicConsole.Write(eeprom_mac[i]);
                BasicConsole.Write(":");
            }
            BasicConsole.WriteLine();

            RX_DE_start = pBuffer_virt + 28;
            TX_DE_start = RX_DE_start + RX_count * DE_size;
            RX_start = TX_DE_start + TX_count * DE_size;
            TX_start = RX_start + RX_count * Buffer_size;

            BasicConsole.Write("RX_DE_start : ");
            BasicConsole.WriteLine((uint)RX_DE_start);
            BasicConsole.Write("TX_DE_start : ");
            BasicConsole.WriteLine((uint)TX_DE_start);
            BasicConsole.Write("RX_start : ");
            BasicConsole.WriteLine((uint)RX_start);
            BasicConsole.Write("TX_Start : ");
            BasicConsole.WriteLine((uint)TX_start);

            RX_DE_phys = Buffer_phys + 28;
            TX_DE_phys = (uint)(RX_DE_phys + RX_count * DE_size);
            RX_phys = (uint)(TX_DE_phys + TX_count * DE_size);
            TX_phys = (uint)(RX_phys + RX_count * Buffer_size);

            BasicConsole.Write("RX_DE_phys : ");
            BasicConsole.WriteLine((uint)RX_DE_phys);
            BasicConsole.Write("TX_DE_phys : ");
            BasicConsole.WriteLine((uint)TX_DE_phys);
            BasicConsole.Write("RX_phys : ");
            BasicConsole.WriteLine((uint)RX_phys);
            BasicConsole.Write("TX_phys : ");
            BasicConsole.WriteLine((uint)TX_phys);

            for (int i = 0; i < RX_count; i++)
            {
                init_descriptor(i, false);
            }

            for (int i = 0; i < TX_count; i++)
            {
                init_descriptor(i, true);
            }

            BasicConsole.WriteLine("AMDPCNetII > initBlock configuration");

            (*(byte*)(pBuffer_virt + 0)) = 0;
            (*(byte*)(pBuffer_virt + 1)) = 0;
            (*(byte*)(pBuffer_virt + 2)) = 5 << 4;
            (*(byte*)(pBuffer_virt + 3)) = 3 << 4;
            (*(byte*)(pBuffer_virt + 4)) = eeprom_mac[0];
            (*(byte*)(pBuffer_virt + 5)) = eeprom_mac[1];
            (*(byte*)(pBuffer_virt + 6)) = eeprom_mac[2];
            (*(byte*)(pBuffer_virt + 7)) = eeprom_mac[3];
            (*(byte*)(pBuffer_virt + 8)) = eeprom_mac[4];
            (*(byte*)(pBuffer_virt + 9)) = eeprom_mac[5];
            (*(byte*)(pBuffer_virt + 10)) = 0; // Reserved
            (*(byte*)(pBuffer_virt + 11)) = 0; // Reserved
            (*(byte*)(pBuffer_virt + 12)) = 0;
            (*(byte*)(pBuffer_virt + 13)) = 0;
            (*(byte*)(pBuffer_virt + 14)) = 0;
            (*(byte*)(pBuffer_virt + 15)) = 0;
            (*(byte*)(pBuffer_virt + 16)) = 0;
            (*(byte*)(pBuffer_virt + 17)) = 0;
            (*(byte*)(pBuffer_virt + 18)) = 0;
            (*(byte*)(pBuffer_virt + 19)) = 0;
            (*(uint*)(pBuffer_virt + 20)) = RX_DE_phys;
            (*(uint*)(pBuffer_virt + 24)) = TX_DE_phys;

            write_csr32(1, 0xFFFF & Buffer_phys);
            write_csr32(2, Buffer_phys >> 16);

            SystemCalls.RegisterIRQHandler(InterruptLine, HandleIRQ);

            BasicConsole.WriteLine("AMDPCNetII > Init card...");
            // Init card
            write_csr32(0, CSR0_INIT | CSR0_IENA);
            SystemCalls.SleepThread(100);

            while (!InitDone)
            {
                BasicConsole.Write("AMDPCNetII > CSR0: ");
                BasicConsole.WriteLine(read_csr32(0));
                SystemCalls.SleepThread(1000);
            }

            BasicConsole.WriteLine("AMDPCNetII > Init done...");

            BasicConsole.WriteLine("AMDDPNetII > Dump buffer: ");
            int line = 0;
            for (int i = 0; i < 668; i += 4)
            {
                if (i == 28) BasicConsole.WriteLine("AMDDPNetII > End of InitBlock");
                if (i == 28 + 512) BasicConsole.WriteLine("AMDDPNetII > End of Rx Descriptor");
                BasicConsole.Write(line++);
                BasicConsole.Write(" : ");
                BasicConsole.Write(Buffer_phys + i);
                BasicConsole.Write(" ");
                BasicConsole.Write((*(uint*)(pBuffer_virt + i)));
                BasicConsole.WriteLine();
            }
            BasicConsole.WriteLine("AMDDPNetII > End of Tx Descriptor");
            BasicConsole.WriteLine();

            return true;
        }

        public void Start()
        {
            BasicConsole.WriteLine("AMDPCNetII > Starting...");

            write_csr32(0, CSR0_IENA | CSR0_STRT);

            byte[] ARP = new byte[64];

            ARP[0] = 0xFF;  // Broadcast
            ARP[1] = 0xFF;
            ARP[2] = 0xFF;
            ARP[3] = 0xFF;
            ARP[4] = 0xFF;
            ARP[5] = 0xFF;

            ARP[6] = eeprom_mac[0]; // MyMAC
            ARP[7] = eeprom_mac[1];
            ARP[8] = eeprom_mac[2];
            ARP[9] = eeprom_mac[3];
            ARP[10] = eeprom_mac[4];
            ARP[11] = eeprom_mac[5];

            ARP[12] = 0x08; // Type ARP
            ARP[13] = 0x06;

            ARP[14] = 0x00; // Hardware Type 1
            ARP[15] = 0x01;

            ARP[16] = 0x08; // IPv4
            ARP[17] = 0x00;

            ARP[18] = 0x06; // Hardware size 6

            ARP[19] = 0x04; // Protocole size 4

            ARP[20] = 0x00; // Request
            ARP[21] = 0x01;

            ARP[22] = eeprom_mac[0]; // Sender MAC address
            ARP[23] = eeprom_mac[1];
            ARP[24] = eeprom_mac[2];
            ARP[25] = eeprom_mac[3];
            ARP[26] = eeprom_mac[4];
            ARP[27] = eeprom_mac[5];

            ARP[28] = 192; // Sender IP address
            ARP[29] = 168;
            ARP[30] = 0;
            ARP[31] = 49;

            ARP[32] = 0; // Target MAC address
            ARP[33] = 0;
            ARP[34] = 0;
            ARP[35] = 0;
            ARP[36] = 0;
            ARP[37] = 0;

            ARP[38] = 192; // Target IP address
            ARP[39] = 168;
            ARP[40] = 0;
            ARP[41] = 254;

            SendPacket(ARP);

            bool Terminated = false;
            while (!Terminated) // Don't use while (true), it crash OS
            {
                BasicConsole.Write("AMDPCNetII > CSR0: ");
                BasicConsole.WriteLine(read_csr32(0));
                BasicConsole.Write("AMDPPCNetII > MissedInt: ");
                BasicConsole.WriteLine(MissedInt);
                SystemCalls.WaitSemaphore(ReceivedSemaphoreId);
            }
        }

        private void* GetPhysicalAddress(void* vAddr)
        {
            return GetPhysicalAddress((uint)vAddr);
        }

        private void* GetPhysicalAddress(uint vAddr)
        {
            uint address = 0xFFFFFFFF;

            SystemCallResults result = SystemCalls.GetPhysicalAddress(vAddr, out address);
            if (result != SystemCallResults.OK)
            {
                ExceptionMethods.Throw(new Framework.Exception("AMDPCNetII > cannot get physical address."));
            }

            return (void*)address;
        }

        private static uint read_csr32(uint csr_no)
        {
            rap.Write_UInt32(csr_no);
            return rdp.Read_UInt32();
        }

        private static void write_csr32(uint csr_no, uint value)
        {
            rap.Write_UInt32(csr_no);
            rdp.Write_UInt32(value);
        }

        private static uint read_bcr32(uint bcr_no)
        {
            rap.Write_UInt32(bcr_no);
            return bcr.Read_UInt32();
        }

        private static void write_bcr32(uint bcr_no, uint value)
        {
            rap.Write_UInt32(bcr_no);
            bcr.Write_UInt32(value);
        }

        private static bool driver_owns(byte* de_table, int index)
        {
            return ((*(byte*)(de_table + (index * DE_size) + 7) & 0x80) == 0);
        }

        private static byte next_tx_index(byte current_tx_index)
        {
            byte b = (byte)(current_tx_index + 1);
            if (b == TX_count)
                return 0;
            return b;
        }

        private static byte next_rx_index(byte current_rx_index)
        {
            byte b = (byte)(current_rx_index + 1);
            if (b == RX_count)
                return 0;
            return b;
        }

        private void init_descriptor(int index, bool is_tx)
        {
            byte* DE_table = is_tx ? TX_DE_start : RX_DE_start;

            uint buf_addr = is_tx ? TX_phys : RX_phys;
            (*(uint*)(DE_table + (index * DE_size))) = (uint)(buf_addr + index * Buffer_size);

            uint bcnt = (uint)(~Buffer_size);
            bcnt &= 0x0FFF;
            bcnt |= 0xF000;
            (*(uint*)(DE_table + (index * 16 + 4))) = bcnt;
            (*(uint*)(DE_table + (index * 16 + 8))) = 0x0; // Zeroed
            (*(uint*)(DE_table + (index * 16 + 12))) = 0x0; // Zeroed

            if (!is_tx)
                (*(byte*)(DE_table + (index * DE_size + 7))) = 0x80;
        }

        private static int HandleIRQ(uint IRQNum)
        {
            bool signalSemaphore = false;

            if (!inInt)
            {
                inInt = true;
                BasicConsole.WriteLine("AMCPCNetII > Interrupt");

                if ((read_csr32(0) & (CSR0_IDON)) == CSR0_IDON) // Initilization done
                {
                    InitDone = true;
                    write_csr32(0, read_csr32(0) | (CSR0_IDON));
                }
                else
                {
                    if ((read_csr32(0) & (CSR0_TINT)) == CSR0_TINT) // Frame sent
                    {
                        BasicConsole.WriteLine("AMDPCNetII > Frame sent");
                        write_csr32(0, read_csr32(0) | (CSR0_TINT));
                    }
                    else
                    {
                        if ((read_csr32(0) & (CSR0_RINT)) == CSR0_RINT) // Frame received
                        {
                            BasicConsole.WriteLine("AMCPCNetII > Frame received");

                            write_csr32(0, CSR0_BABL | CSR0_CERR | CSR0_MISS | CSR0_MERR | CSR0_IDON | CSR0_IENA);

                            while (driver_owns(RX_DE_start, RX_buffer_id))
                            {
                                ushort len = (*(ushort*)(RX_DE_start + (RX_buffer_id * DE_size) + 4));

                                //NetworkPacket packet = new NetworkPacket();
                                //packet.data = new byte[len];

                                //for (uint i = 0; i < packet.data.Length; i++)
                                //    packet.data[i] = (*(byte*)(pcnet_rx_de_start[pcnet_rx_buffer_id * pcnet_de_size] + i));

                                (*(byte*)(RX_DE_start + (RX_buffer_id * DE_size + 7))) = 0x80;

                                RX_buffer_id = next_rx_index(RX_buffer_id);
                            }
                            write_csr32(0, read_csr32(0) | (CSR0_RINT));

                            signalSemaphore = true;
                        }
                        else
                        {
                            while (((read_csr32(0) & (CSR0_ERR | CSR0_RINT | CSR0_TINT)) != 0x0))
                            {
                                // Acknowledge all of the current interrupt sources
                                write_csr32(0, (read_csr32(0) & ~(CSR0_IENA | CSR0_TDMD | CSR0_STOP | CSR0_STRT | CSR0_INIT)));
                            }
                        }
                    }
                }
                inInt = false;
            }
            else
                MissedInt++;

            BasicConsole.WriteLine("AMDPCNetII > Leave Interrupt");

            if (signalSemaphore)
            {
                return InterruptUtils.ConstructInterruptResult(SystemCallResults.RequestAction_SignalSemaphore, (uint)ReceivedSemaphoreId);
            }
            else
            {
                return InterruptUtils.ConstructInterruptResult(SystemCallResults.OK, 0);
            }
        }

        private static bool SendPacket(byte[] data)
        {
            if (!driver_owns(TX_DE_start, TX_buffer_id))
            {
                BasicConsole.WriteLine("AMDPCNetII > No transmit descriptors available ...");
                return false;
            }
            if (data.Length > Buffer_size)
            {
                BasicConsole.WriteLine("AMDPCNetII > Packet too big ...");
                return false;
            }

            BasicConsole.WriteLine("AMDPCNetII > Dump send packet");
            BasicConsole.Write("AMDPCNetII > TX_buffer_id: ");
            BasicConsole.WriteLine(TX_buffer_id);
            BasicConsole.Write("AMDPCNetII > data.Length: ");
            BasicConsole.WriteLine(data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                (*(byte*)(TX_start + TX_buffer_id * Buffer_size + i)) = data[i];

                BasicConsole.Write((uint)(TX_start + TX_buffer_id * Buffer_size + i));
                BasicConsole.Write(" ");
                BasicConsole.WriteLine(data[i]);
            }
            (*(byte*)(TX_DE_start + TX_buffer_id * DE_size + 7)) |= 0x3; // STP & ENP Bits, frame fit into buffer

            ushort bcnt = (ushort)(~data.Length);
            bcnt &= 0x0FFF;
            bcnt |= 0xF000;
            (*(ushort*)(TX_DE_start + TX_buffer_id * DE_size + 4)) = bcnt;

            (*(byte*)(TX_DE_start + TX_buffer_id * DE_size + 7)) |= 0x80;

            write_csr32(0, read_csr32(0) | (1 << 3));

            TX_buffer_id = next_tx_index(TX_buffer_id);
            return true;
        }


    }
}