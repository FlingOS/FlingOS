using Drivers.Compiler.Attributes;
using Kernel.Devices;
using Kernel.Devices.Serial;
using Kernel.Framework;
using Kernel.Framework.Exceptions;
using Kernel.Framework.Processes;
using Kernel.PCI;
using Kernel.Utilities;

namespace Kernel.VGA.VMWare
{
    public unsafe class SVGAII : Devices.Device
    {
        public struct FIFOStruct
        {
            public const int BounceBufferSize = 1024 * 1024;

            public uint ReservedSize;
            public bool UsingBounceBuffer;
            public byte* BounceBuffer;
            public uint NextFence;
        }

        public struct IRQStruct
        {
            public uint Pending;
            public uint Count;
        }

        private bool Terminating = false;

        private ushort IOBase;
        private uint* FIFOMem;
        private byte* FBMem;
        private uint FIFOSize;
        private uint FBSize;
        private uint VRAMSize;

        private SVGAII_Registers.ID DeviceVersionId;
        public uint Capabilities;

        private uint Width;
        private uint Height;
        private uint BPP;
        private uint Pitch;

        private FIFOStruct FIFO;
        private static IRQStruct IRQ;

        private static IOPort IndexPort;
        private static IOPort ValuePort;
        private static IOPort BIOSPort;
        private static IOPort IRQStatusPort;
        
        private PCIDeviceNormal ThePCIDevice;

        private static int IRQSemaphoreId = -1;

        public SVGAII(PCIDeviceNormal pciDevice)
        {
            ThePCIDevice = pciDevice;

            IOBase = (ushort)ThePCIDevice.BaseAddresses[0].BaseAddress();
            
            if (IndexPort == null)
            {
                IndexPort = new IOPort(IOBase, (ushort)SVGAII_Registers.PORTS.INDEX);
            }
            if (ValuePort == null)
            {
                ValuePort = new IOPort(IOBase, (ushort)SVGAII_Registers.PORTS.VALUE);
            }
            if (BIOSPort == null)
            {
                BIOSPort = new IOPort(IOBase, (ushort)SVGAII_Registers.PORTS.BIOS);
            }
            if (IRQStatusPort == null)
            {
                IRQStatusPort = new IOPort(IOBase, (ushort)SVGAII_Registers.PORTS.IRQ_STATUS);
            }

            FIFO.BounceBuffer = (byte*)AllocPages(FIFOStruct.BounceBufferSize / 4096);
            
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

            BasicConsole.WriteLine((String)"SVGA-II : Device Version Id: " + (uint)DeviceVersionId);
            if ((int)DeviceVersionId < (int)SVGAII_Registers.ID._0)
            {
                Error("Error negotiating SVGA device version.");
            }

            FBMem = ThePCIDevice.BaseAddresses[1].BaseAddress();
            FIFOMem = (uint*)ThePCIDevice.BaseAddresses[2].BaseAddress();
            
            VRAMSize = ReadReg((uint)SVGAII_Registers.Registers.VRAM_SIZE);
            FBSize = ReadReg((uint)SVGAII_Registers.Registers.FB_SIZE);
            FIFOSize = ReadReg((uint)SVGAII_Registers.Registers.MEM_SIZE);

            if (FBSize < 0x100000)
            {
                Error("Frame buffer size too small.");
            }

            if (FIFOSize < 0x20000)
            {
                Error("FIFO size too small.");
            }

            uint temp;
            if (SystemCalls.RequestPages((uint)FIFOMem & 0xFFFFF000, (uint)FIFOMem & 0xFFFFF000, (FIFOSize + 4095) / 4096, out temp) != SystemCallResults.OK)
            {
                Error("Couldn't allocate FIFOMem page.");
            }
            FIFOMem = (uint*)((byte*)temp + ((uint)FIFOMem & 0x00000FFF));

            if (SystemCalls.RequestPages((uint)FBMem & 0xFFFFF000, (uint)FBMem & 0xFFFFF000, (FBSize + 4095) / 4096, out temp) != SystemCallResults.OK)
            {
                Error("Couldn't allocate FBMem page.");
            }
            FBMem = (byte*)temp + ((uint)FBMem & 0x00000FFF);
            
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

                BasicConsole.WriteLine((String)"SVGA-II : IRQNum=" + IRQNum);
                BasicConsole.WriteLine((String)"SVGA-II : Registering IRQ handler...");

                if (SystemCalls.RegisterIRQHandler(IRQNum, IRQHandler) != SystemCallResults.OK)
                {
                    Error("Failed to register IRQ handler.");
                }
            }

            if (IRQSemaphoreId == -1)
            {
                if (SystemCalls.CreateSemaphore(-1, out IRQSemaphoreId) != SystemCallResults.OK)
                {
                    Error("Failed to create semaphore for IRQ detection.");
                }
            }

            Enable();
        }

        private uint AllocPages(int count)
        {
            uint page;
            if (SystemCalls.RequestPages((uint)count, out page) != SystemCallResults.OK)
            {
                Error("Couldn't allocate page.");
            }
            return page;
        }

        public void Error(String message)
        {
            message = "SVGA-II : " + message;
            BasicConsole.WriteLine(message);
            ExceptionMethods.Throw(new Exception(message));
        }

        public static int IRQHandler(uint irqNumber)
        {
            //BasicConsole.WriteLine("SVGA-II : IRQ");

            uint flags = IRQStatusPort.Read_UInt32();
            if (flags == 0)
            {
                return InterruptUtils.ConstructInterruptResult(SystemCallResults.OK, 0);
            }

            IRQStatusPort.Write_UInt32(flags);
            IRQ.Count++;
            MemoryUtils.AtomicOR(ref IRQ.Pending, flags);

            return InterruptUtils.ConstructInterruptResult(SystemCallResults.RequestAction_SignalSemaphore,
                (uint)IRQSemaphoreId);
        }

        private void Enable()
        {
            FIFOMem[(int)SVGAII_Registers.FIFO.MIN] = (int)SVGAII_Registers.FIFO.NUM_REGS * sizeof(uint);
            FIFOMem[(int)SVGAII_Registers.FIFO.MAX] = FIFOSize;
            FIFOMem[(int)SVGAII_Registers.FIFO.NEXT_CMD] = FIFOMem[(int)SVGAII_Registers.FIFO.MIN];
            FIFOMem[(int)SVGAII_Registers.FIFO.STOP] = FIFOMem[(int)SVGAII_Registers.FIFO.MIN];

            WriteReg((uint)SVGAII_Registers.Registers.ENABLE, 1);

            if (HasFIFOCapability((uint)SVGAII_Registers.Capabilities.EXTENDED_FIFO) &&
                IsFIFORegValid(SVGAII_Registers.FIFO.GUEST_3D_HWVERSION))
            {
                BasicConsole.WriteLine("SVGA-II : Could support 3D hardware acceleration if desired.");
            }

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
                    Error((String)"IRQ is present but not working. IRQ.Pending=" + IRQ.Pending);
                }

                WaitForIRQ();
            }
        }

        private void Disable()
        {
            WriteReg((uint)SVGAII_Registers.Registers.ENABLE, 0);
        }

        public void SetMode(uint width, uint height, uint bpp)
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

        private uint ClearIRQ()
        {
            uint flags = 0;
            MemoryUtils.AtomicExchange(ref IRQ.Pending, ref flags);
            return flags;
        }
        
        private uint WaitForIRQ()
        {
            uint flags = 0;
            do
            {
                MemoryUtils.AtomicExchange(ref IRQ.Pending, ref flags);

                if (flags == 0)
                {
                    if (SystemCalls.WaitSemaphore(IRQSemaphoreId) != SystemCallResults.OK)
                    {
                        Error("Failed to wait for IRQ semaphore.");
                    }
                }

            } while (flags == 0);
            
            //BasicConsole.WriteLine("SVGA-II : Finished waiting for IRQ.");

            return flags;
        }

        private bool IsFIFORegValid(SVGAII_Registers.FIFO reg)
        {
            return FIFOMem[(uint)SVGAII_Registers.FIFO.MIN] > ((int)reg << 2);
        }

        public bool HasFIFOCapability(uint capability)
        {
            //BasicConsole.WriteLine((String)"FIFO Capabilities=" + FIFOMem[(uint)SVGAII_Registers.FIFO.CAPABILITIES]);
            return (FIFOMem[(uint)SVGAII_Registers.FIFO.CAPABILITIES] & capability) != 0;
        }

        public void* FIFOReserve(uint bytes)
        {
            uint* fifo = FIFOMem;

            uint max = fifo[(int)SVGAII_Registers.FIFO.MAX];
            uint min = fifo[(int)SVGAII_Registers.FIFO.MIN];
            uint nextCmd = fifo[(int)SVGAII_Registers.FIFO.NEXT_CMD];
            bool reservable = HasFIFOCapability((uint)SVGAII_Registers.FIFO_Capabilities.RESERVE);

            if (bytes > FIFOStruct.BounceBufferSize ||
                bytes > (max - min))
            {
                Error("Command too large.");
            }

            if (bytes % sizeof(uint) != 0)
            {
                Error("Command length not 32-bit aligned.");
            }

            if (FIFO.ReservedSize != 0)
            {
                Error("Reserve should be called after commit.");
            }

            FIFO.ReservedSize = bytes;

            while (!Terminating)
            {
                uint stop = fifo[(int)SVGAII_Registers.FIFO.STOP];
                bool reserveInPlace = false;
                bool needBounce = false;

                if (nextCmd >= stop)
                {
                    if (nextCmd + bytes < max ||
                        (nextCmd + bytes == max && stop > min))
                    {
                        reserveInPlace = true;
                    }
                    else if ((max - nextCmd) + (stop - min) <= bytes)
                    {
                        FIFOFull();
                    }
                    else
                    {
                        needBounce = true;
                    }
                }
                else
                {
                    if (nextCmd + bytes < stop)
                    {
                        reserveInPlace = true;
                    }
                    else
                    {
                        FIFOFull();
                    }
                }

                if (reserveInPlace)
                {
                    if (reservable || bytes <= sizeof(uint))
                    {
                        FIFO.UsingBounceBuffer = false;
                        if (reservable)
                        {
                            fifo[(int)SVGAII_Registers.FIFO.RESERVED] = bytes;
                        }
                        return nextCmd + (byte*)fifo;
                    }
                    else
                    {
                        needBounce = true;
                    }
                }

                if (needBounce)
                {
                    FIFO.UsingBounceBuffer = true;
                    return FIFO.BounceBuffer;
                }
            }

            return null;
        }

        public void* FIFOReserveCommand(uint type, uint bytes)
        {
            uint* cmd = (uint*)FIFOReserve(bytes + sizeof(uint));
            cmd[0] = type;
            return cmd + 1;
        }

        public void* FIFOReserveEscape(uint nsid, uint bytes)
        {
            uint paddedBytes = (bytes + 3) & ~3U;
            SVGAII_Escape.Header* header = (SVGAII_Escape.Header*)FIFOReserve(
                paddedBytes + (uint)sizeof(SVGAII_Escape.Header));

            header->cmd = (uint)SVGAII_Registers.FIFO_Command.ESCAPE;
            header->nsid = nsid;
            header->size = bytes;
            
            return header + 1;
        }

        [NoGC]
        public void FIFOCommit(uint bytes)
        {
            uint* fifo = FIFOMem;

            uint max = fifo[(uint)SVGAII_Registers.FIFO.MAX];
            uint min = fifo[(uint)SVGAII_Registers.FIFO.MIN];
            uint nextCmd = fifo[(uint)SVGAII_Registers.FIFO.NEXT_CMD];
            bool reservable = HasFIFOCapability((uint)SVGAII_Registers.FIFO_Capabilities.RESERVE);

            if (FIFO.ReservedSize == 0)
            {
                Error("Commit should be called before Reserve.");
            }

            FIFO.ReservedSize = 0;

            if (FIFO.UsingBounceBuffer)
            {
                byte* buffer = FIFO.BounceBuffer;

                if (reservable)
                {
                    uint chunkSize = Math.Min(bytes, max - nextCmd);
                    fifo[(int)SVGAII_Registers.FIFO.RESERVED] = bytes;
                    MemoryUtils.MemCpy(nextCmd + (byte*)fifo, buffer, chunkSize);
                    MemoryUtils.MemCpy(min + (byte*)fifo, buffer + chunkSize, bytes - chunkSize);
                }
                else
                {
                    uint* dword = (uint*)buffer;

                    while (bytes > 0)
                    {
                        fifo[nextCmd / (uint)sizeof(uint*)] = *dword++;
                        nextCmd += (uint)sizeof(uint*);
                        if (nextCmd == max)
                        {
                            nextCmd = min;
                        }
                        fifo[(uint)SVGAII_Registers.FIFO.NEXT_CMD] = nextCmd;
                        bytes -= (uint)sizeof(uint*);
                    }
                }
            }

            if (!FIFO.UsingBounceBuffer || reservable)
            {
                nextCmd += bytes;
                if (nextCmd >= max)
                {
                    nextCmd -= max - min;
                }
                fifo[(uint)SVGAII_Registers.FIFO.NEXT_CMD] = nextCmd;
            }

            if (reservable)
            {
                fifo[(uint)SVGAII_Registers.FIFO.RESERVED] = 0;
            }
        }

        public void FIFOCommitAll()
        {
            FIFOCommit(FIFO.ReservedSize);
        }

        private void FIFOFull()
        {
            if (IsFIFORegValid(SVGAII_Registers.FIFO.FENCE_GOAL) &&
                (Capabilities & (uint)SVGAII_Registers.Capabilities.IRQMASK) != 0)
            {
                WriteReg((uint)SVGAII_Registers.Registers.IRQMASK, (uint)SVGAII_Registers.IRQ_FLAGS.PROGRESS);
                ClearIRQ();
                RingDoorbell();
                WaitForIRQ();
                WriteReg((uint)SVGAII_Registers.Registers.IRQMASK, 0);
            }
            else
            {
                WriteReg((uint)SVGAII_Registers.Registers.SYNC, 1);
                ReadReg((uint)SVGAII_Registers.Registers.BUSY);
            }
        }

        public uint InsertFence()
        {
            if (!HasFIFOCapability((uint)SVGAII_Registers.FIFO_Capabilities.FENCE))
            {
                BasicConsole.WriteLine("SVGA-II : No Fence capability.");
                return 1;
            }

            if (FIFO.NextFence == 0)
            {
                FIFO.NextFence = 1;
            }
            uint fence = FIFO.NextFence++;
            
            SVGAII_Registers.FIFO_CommandWithId* cmd = (SVGAII_Registers.FIFO_CommandWithId*)
                FIFOReserve((uint)sizeof(SVGAII_Registers.FIFO_CommandWithId));

            cmd->id = (uint)SVGAII_Registers.FIFO_Command.FENCE;
            cmd->fence = fence;
            
            FIFOCommitAll();

            return fence;
        }

        public void SyncToFence(uint fence)
        {
            if (fence == 0)
            {
                return;
            }

            if (!HasFIFOCapability((uint)SVGAII_Registers.FIFO_Capabilities.FENCE))
            {
                WriteReg((uint)SVGAII_Registers.Registers.SYNC, 1);
                while (ReadReg((uint)SVGAII_Registers.Registers.BUSY) != 0) ;
                return;
            }

            if (HasFencePassed(fence))
            {
                return;
            }

            if (IsFIFORegValid(SVGAII_Registers.FIFO.FENCE_GOAL) &&
                (Capabilities & (uint)SVGAII_Registers.Capabilities.IRQMASK) != 0)
            {
                FIFOMem[(uint)SVGAII_Registers.FIFO.FENCE_GOAL] = fence;
                WriteReg((uint)SVGAII_Registers.Registers.IRQMASK, (uint)SVGAII_Registers.IRQ_FLAGS.FENCE_GOAL);

                ClearIRQ();

                if (!HasFencePassed(fence))
                {
                    RingDoorbell();

                    if (!HasFencePassed(fence))
                    {
                        WaitForIRQ();
                    }
                }

                WriteReg((uint)SVGAII_Registers.Registers.IRQMASK, 0);
            }
            else
            {
                bool busy = true;
                WriteReg((uint)SVGAII_Registers.Registers.SYNC, 1);
                while (!HasFencePassed(fence) && busy)
                {
                    busy = ReadReg((uint)SVGAII_Registers.Registers.BUSY) != 0;
                }
            }

            if (HasFencePassed(fence))
            {
                Error("SyncToFence failed!");
            }
        }

        private bool HasFencePassed(uint fence)
        {
            if (fence == 0)
            {
                return true;
            }


            if (HasFIFOCapability((uint)SVGAII_Registers.FIFO_Capabilities.FENCE))
            {
                return false;
            }

            return (int)(FIFOMem[(uint)SVGAII_Registers.FIFO.FENCE]) >= 0;
        }

        private void RingDoorbell()
        {
            if (IsFIFORegValid(SVGAII_Registers.FIFO.BUSY) &&
                FIFOMem[(uint)SVGAII_Registers.FIFO.BUSY] == 0)
            {
                FIFOMem[(uint)SVGAII_Registers.FIFO.BUSY] = 1;
                WriteReg((uint)SVGAII_Registers.Registers.SYNC, 1);
            }
        }

        private static SVGAII_Registers.GuestPointer NextPointer = new SVGAII_Registers.GuestPointer()
        {
            GMRId = (uint)SVGAII_Registers.GuestMemoryRegion.FRAMEBUFFER,
            Offset = 0
        };

        public void* AllocGMR(uint size, SVGAII_Registers.GuestPointer* Pointer)
        {
            *Pointer = NextPointer;
            NextPointer.Offset += size;
            return FBMem + Pointer->Offset;
        }

        private void Update(uint x, uint y, uint width, uint height)
        {
            SVGAII_Registers.FIFO_CommandUpdate* cmd = (SVGAII_Registers.FIFO_CommandUpdate*)
                FIFOReserveCommand(
                    (uint)SVGAII_Registers.FIFO_Command.UPDATE, 
                    (uint)sizeof(SVGAII_Registers.FIFO_CommandUpdate));
            cmd->X = x;
            cmd->Y = y;
            cmd->Width = width;
            cmd->Height = height;
            FIFOCommitAll();
        }

        private void BeginDefineCursor(SVGAII_Registers.FIFO_CommandDefineCursor* CursorInfo,
            void** ANDMask, void** XORMask)
        {
            uint andPitch = ((CursorInfo->ANDMaskDepth * CursorInfo->Width + 31) >> 5) << 2;
            uint andSize = andPitch * CursorInfo->Height;
            uint xorPitch = ((CursorInfo->XORMaskDepth * CursorInfo->Width + 31) >> 5) << 2;
            uint xorSize = xorPitch * CursorInfo->Height;

            SVGAII_Registers.FIFO_CommandDefineCursor* cmd =
                (SVGAII_Registers.FIFO_CommandDefineCursor*)
                    FIFOReserveCommand(
                        (uint)SVGAII_Registers.FIFO_Command.DEFINE_CURSOR,
                        (uint)sizeof(SVGAII_Registers.FIFO_CommandDefineCursor) + andSize + xorSize);
            *cmd = *CursorInfo;
            *ANDMask = (void*)(cmd + 1);
            *XORMask = (void*)(andSize + (byte*)*ANDMask);
        }

        private void BeginDefineAlphaCursor(SVGAII_Registers.FIFO_CommandDefineAlphaCursor* CursorInfo,
            void** data)
        {
            uint imageSize = CursorInfo->Width * CursorInfo->Height * sizeof(uint);
            SVGAII_Registers.FIFO_CommandDefineAlphaCursor* cmd =
                (SVGAII_Registers.FIFO_CommandDefineAlphaCursor*)
                    FIFOReserveCommand(
                        (uint)SVGAII_Registers.FIFO_Command.DEFINE_ALPHA_CURSOR,
                        (uint)sizeof(SVGAII_Registers.FIFO_CommandDefineAlphaCursor) + imageSize);
            *cmd = *CursorInfo;
            *data = (void*)(cmd + 1);
        }

        private void MoveCursor(uint visible, uint x, uint y, uint screenId)
        {
            if (HasFIFOCapability((uint)SVGAII_Registers.FIFO_Capabilities.SCREEN_OBJECT))
            {
                FIFOMem[(uint)SVGAII_Registers.FIFO.CURSOR_SCREEN_ID] = screenId;
            }

            if (HasFIFOCapability((uint)SVGAII_Registers.FIFO_Capabilities.CURSOR_BYPASS_3))
            {
                FIFOMem[(uint)SVGAII_Registers.FIFO.CURSOR_ON] = visible;
                FIFOMem[(uint)SVGAII_Registers.FIFO.CURSOR_X] = x;
                FIFOMem[(uint)SVGAII_Registers.FIFO.CURSOR_Y] = y;
                FIFOMem[(uint)SVGAII_Registers.FIFO.CURSOR_COUNT]++;
            }
        }

        private void BeginVideoSetRegs(uint streamId, uint numItems, SVGAII_Escape.VideoSetRegs** setRegs)
        {
            uint cmdSize = (uint)sizeof(SVGAII_Escape.VideoSetRegs)
                           - (uint)sizeof(SVGAII_Escape.VideoSetRegsItem*) +
                           (numItems * (uint)sizeof(SVGAII_Escape.VideoSetRegsItem));
            SVGAII_Escape.VideoSetRegs* cmd =
                (SVGAII_Escape.VideoSetRegs*)
                    FIFOReserveEscape((uint)SVGAII_Escape.NSID_VMWARE, cmdSize);

            cmd->CommandType = (uint)SVGAII_Escape.VMWARE_VIDEO_SET_REGS;
            cmd->StreamId = streamId;

            *setRegs = cmd;
        }

        private void VideoSetAllRegs(uint streamId, SVGAII_Registers.OverlayUnit* regs,
            uint maxReg)
        {
            uint* regArray = (uint*)regs;
            uint numRegs = maxReg + 1;
            SVGAII_Escape.VideoSetRegs* setRegs;

            BeginVideoSetRegs(streamId, numRegs, &setRegs);

            for (uint i = 0; i < numRegs; i++)
            {
                (&setRegs->Item0)[i].RegisterId = i;
                (&setRegs->Item0)[i].Value = regArray[i];
            }

            FIFOCommitAll();
        }

        private void VideoSetReg(uint streamId, uint registerId, uint value)
        {
            SVGAII_Escape.VideoSetRegs* setRegs;
            BeginVideoSetRegs(streamId, 1, &setRegs);
            (&setRegs->Item0)[0].RegisterId = registerId;
            (&setRegs->Item0)[0].Value = value;
            FIFOCommitAll();
        }

        private void VideoFlush(uint streamId)
        {
            SVGAII_Escape.VideoFlush* cmd =
                (SVGAII_Escape.VideoFlush*)
                    FIFOReserveEscape(
                        SVGAII_Escape.NSID_VMWARE,
                        (uint)sizeof(SVGAII_Escape.VideoFlush));
            cmd->CommandType = SVGAII_Escape.VMWARE_VIDEO_FLUSH;
            cmd->StreamId = streamId;
            FIFOCommitAll();
        }

    }
}
