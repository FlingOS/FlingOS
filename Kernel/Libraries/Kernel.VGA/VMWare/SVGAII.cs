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
        
        private uint* FIFOMem;
        private byte* FBMem;
        private uint FIFOSize;
        private uint FBSize;
        private uint VRAMSize;

        //private uint DeviceVersionId;
        //private uint Capabilities;

        private uint Width;
        private uint Height;
        private uint BPP;
        private uint Pitch;

        private FIFOStruct FIFO;

        private PCIVirtualNormalDevice ThePCIDevice;

        public SVGAII(PCIVirtualNormalDevice pciDevice)
        {
            ThePCIDevice = pciDevice;
        }
    }
}
