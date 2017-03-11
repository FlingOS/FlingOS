using Kernel.Devices;
using Kernel.Devices.Serial;
using Kernel.Framework.Exceptions;
using Kernel.Framework.Processes;
using Kernel.PCI;

namespace Kernel.VGA.VMWare
{
    public unsafe class SVGAII : Devices.Device
    {
        public struct FIFOStruct
        {
            public uint ReservedSize;
            public byte UsingBounceBuffer;
            public fixed byte BounceBuffer[1024 * 1024];
            public uint NextFence;
        }

        public struct IRQStruct
        {
            public uint Pending;
            public uint Count;
        }

        private ushort IOBase;
        private uint* FIFOMem;
        private byte* FBMem;
        private uint FIFOSize;
        private uint FBSize;
        private uint VRAMSize;

        private SVGAII_Registers.ID DeviceVersionId;
        private uint Capabilities;

        private uint Width;
        private uint Height;
        private uint BPP;
        private uint Pitch;

        private FIFOStruct FIFO;
        private IRQStruct IRQ;

        private IOPort IndexPort;
        private IOPort ValuePort;
        private IOPort BIOSPort;
        private IOPort IRQStatusPort;
        
        private PCIVirtualNormalDevice ThePCIDevice;

        public SVGAII(PCIVirtualNormalDevice pciDevice)
        {
            ThePCIDevice = pciDevice;

            IOBase = (ushort)ThePCIDevice.BaseAddresses[0].BaseAddress();
            FIFOMem = (uint*)ThePCIDevice.BaseAddresses[1].BaseAddress();
            FBMem = ThePCIDevice.BaseAddresses[2].BaseAddress();

            IndexPort = new IOPort(IOBase, (ushort)SVGAII_Registers.PORTS.INDEX);
            ValuePort = new IOPort(IOBase, (ushort)SVGAII_Registers.PORTS.VALUE);
            BIOSPort = new IOPort(IOBase, (ushort)SVGAII_Registers.PORTS.BIOS);
            IRQStatusPort = new IOPort(IOBase, (ushort)SVGAII_Registers.PORTS.IRQ_STATUS);

            DeviceVersionId = SVGAII_Registers.ID._2;
            do
            {
                WriteReg((uint)SVGAII_Registers.Registers.ID, (uint)DeviceVersionId);
                if (ReadReg((uint)SVGAII_Registers.Registers.ID) == (uint)DeviceVersionId)
                {
                    break;
                }

                DeviceVersionId--;
            }
            while (DeviceVersionId >= SVGAII_Registers.ID._0);

            if (DeviceVersionId <= SVGAII_Registers.ID.INVALID)
            {
                BasicConsole.WriteLine("SVGA-II : Error negotiating SVGA device version.");
                ExceptionMethods.Throw(new ArgumentException("SVGA-II Device Version not negotiable."));
            }

            VRAMSize = ReadReg((uint)SVGAII_Registers.Registers.VRAM_SIZE);
            FBSize = ReadReg((uint)SVGAII_Registers.Registers.FB_SIZE);
            FIFOSize = ReadReg((uint)SVGAII_Registers.Registers.MEM_SIZE);

            if (FBSize < 0x100000)
            {
                BasicConsole.WriteLine("SVGA-II : Frame buffer size too small.");
                ExceptionMethods.Throw(new ArgumentException("SVGA-II frame buffer size too small."));
            }

            if (FIFOSize < 0x20000)
            {
                BasicConsole.WriteLine("SVGA-II : FIFO size too small.");
                ExceptionMethods.Throw(new ArgumentException("SVGA-II FIFO size too small."));
            }

            if (DeviceVersionId >= SVGAII_Registers.ID._1)
            {
                Capabilities = ReadReg((uint)SVGAII_Registers.Registers.CAPABILITIES);
            }

            if ((Capabilities & (uint)SVGAII_Registers.Capabilities.IRQMASK) != 0)
            {
                byte IRQNum = ThePCIDevice.InterruptLine;
                WriteReg((uint)SVGAII_Registers.Registers.IRQMASK, 0);
                IRQStatusPort.Write_UInt32(0xFF);

                ClearIRQ();

                if (SystemCalls.RegisterIRQHandler(IRQNum, IRQHandler) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("SVGA-II : Failed to register IRQ handler.");
                    ExceptionMethods.Throw(new ArgumentException("SVGA-II failed to register IRQ handler."));
                }
            }

            Enable();
        }

        public static int IRQHandler(uint irqNumber)
        {
            //TODO
            return 0;
        }

        private void Enable()
        {
            FIFOMem[(int)SVGAII_Registers.FIFO.MIN] = (int)SVGAII_Registers.FIFO.NUM_REGS * sizeof(uint);
            FIFOMem[(int)SVGAII_Registers.FIFO.MAX] = FIFOSize;
            FIFOMem[(int)SVGAII_Registers.FIFO.NEXT_CMD] = FIFOMem[(int)SVGAII_Registers.FIFO.MIN];
            FIFOMem[(int)SVGAII_Registers.FIFO.STOP] = FIFOMem[(int)SVGAII_Registers.FIFO.MIN];

            if (HasFIFOCapability((uint)SVGAII_Registers.Capabilities.EXTENDED_FIFO) &&
                IsFIFORegValid((int)SVGAII_Registers.FIFO.GUEST_3D_HWVERSION))
            {
                BasicConsole.WriteLine("SVGA-II : Could support 3D hardware acceleration if desired.");
            }

            WriteReg((uint)SVGAII_Registers.Registers.ENABLE, 1);
            WriteReg((uint)SVGAII_Registers.Registers.CONFIG_DONE, 1);

            if ((Capabilities & (uint)SVGAII_Registers.Capabilities.IRQMASK) != 0)
            {
                WriteReg((uint)SVGAII_Registers.Registers.IRQMASK, (uint)SVGAII_Registers.IRQ_FLAGS.ANY_FENCE);

                ClearIRQ();

                InsertFence();

                WriteReg((uint)SVGAII_Registers.Registers.SYNC, 1);
                while (ReadReg((uint)SVGAII_Registers.Registers.BUSY) != 0) ;

                WriteReg((uint)SVGAII_Registers.Registers.IRQMASK, 0);

                if ((IRQ.Pending & (uint)SVGAII_Registers.IRQ_FLAGS.ANY_FENCE) == 0)
                {
                    BasicConsole.WriteLine("SVGA-II : IRQ is present but not working.");
                    ExceptionMethods.Throw(new NotSupportedException("SVGA-II : IRQ is present but not working."));
                }

                WaitForIRQ();
            }
        }

        private void Disable()
        {
            WriteReg((uint)SVGAII_Registers.Registers.ENABLE, 0);
        }

        private void SetMode(uint width, uint height, uint bpp)
        {
            Width = width;
            Height = height;
            BPP = bpp;

            WriteReg((uint)SVGAII_Registers.Registers.WIDTH, width);
            WriteReg((uint)SVGAII_Registers.Registers.HEIGHT, height);
            WriteReg((uint)SVGAII_Registers.Registers.BITS_PER_PIXEL, bpp);
            WriteReg((uint)SVGAII_Registers.Registers.ENABLE, 1);

            Pitch = ReadReg((uint)SVGAII_Registers.Registers.BYTES_PER_LINE);
        }

        public uint ReadReg(uint index)
        {
            IndexPort.Write_UInt32(index);
            return ValuePort.Read_UInt32();
        }

        public void WriteReg(uint index, uint value)
        {
            IndexPort.Write_UInt32(index);
            ValuePort.Write_UInt32(value);
        }

        private void ClearIRQ()
        {
            //TODO
        }
        
        private uint WaitForIRQ()
        {
            //TODO
            return 0;
        }

        private bool IsFIFORegValid(int reg)
        {
            return FIFOMem[(uint)SVGAII_Registers.FIFO.MIN] > (reg << 2);
        }

        public bool HasFIFOCapability(uint capability)
        {
            return (FIFOMem[(uint)SVGAII_Registers.FIFO.CAPABILITIES] & capability) != 0;
        }

        public void* FIFOReserve(uint bytes)
        {
            //TODO
            return null;
        }

        public void* FIFOReserveCommand(uint type, uint bytes)
        {
            //TODO
            return null;
        }

        public void* FIFOReserveEscape(uint nsid, uint bytes)
        {
            //TODO
            return null;
        }

        public void FIFOCommit(uint bytes)
        {
            //TODO
        }

        public void FIFOCommitAll()
        {
            //TODO
        }

        private uint InsertFence()
        {
            //TODO
            return 0;
        }

        private void SyncToFence(uint fence)
        {
            //TODO
        }

        private bool HasFencePassed(uint fence)
        {
            //TODO
            return false;
        }

        private void RingDoorbell()
        {
            //TODO
        }

        public void* AllocGMR(uint size, SVGAII_Registers.GuestPointer* Pointer)
        {
            //TODO
            return null;
        }

        private void Update(uint x, uint y, uint width, uint height)
        {
            //TODO
        }

        private void BeginDefineCursor(SVGAII_Registers.FIFO_CommandDefineCursor* CursorInfo,
            void** ANDMask, void** XORMask)
        {
            //TODO
        }

        private void BeginDefineAlphaCursor(SVGAII_Registers.FIFO_CommandDefineAlphaCursor* CursorInfo,
            void** data)
        {
            //TODO
        }

        private void MoveCursor(uint visible, uint x, uint y, uint screenId)
        {
            //TODO
        }

        private void BeginVideoSetRegs(uint streamId, uint numItems, SVGAII_Escape.VideoSetRegs** setRegs)
        {
            //TODO
        }

        private void VideoSetAllRegs(uint streamId, SVGAII_Registers.OverlayUnit* regs,
            uint maxReg)
        {
            //TODO
        }

        private void VideoSetReg(uint streamId, uint registerId, uint value)
        {
            //TODO
        }

        private void VideoFlush(uint streamId)
        {
            //TODO
        }
    }
}
