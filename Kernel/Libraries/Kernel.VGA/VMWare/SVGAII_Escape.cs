namespace Kernel.VGA.VMWare
{
    public static class SVGAII_Escape
    {
        public const uint NSID_VMWARE = 0x00000000;
        public const uint NSID_DEVEL  = 0xFFFFFFFF;

        public const uint VMWARE_MAJOR_MASK = 0xFFFF0000;

        public const uint VMWARE_HINT = 0x00030000;
        public const uint VMWARE_HINT_FULLSCREEN = 0x00030001;

        public const uint VMWARE_VIDEO = 0x00020000;
        public const uint VMWARE_VIDEO_SET_REGS = 0x00020001;
        public const uint VMWARE_VIDEO_FLUSH = 0x00020002;

        public struct HintFullscreen
        {
            public uint Command;
            public uint Fullscreen;
            SVGAII_Registers.SignedPoint MonitorPosition;
        }

        public struct VideoSetRegsItem
        {
            public uint RegisterId;
            public uint Value;
        }

        public struct VideoSetRegs
        {
            public uint CommandType;
            public uint StreamId;

            public VideoSetRegsItem Item0;
        }

        public struct VideoFlush
        {
            public uint CommandType;
            public uint StreamId;
        }

        public struct FIFO_EscapeCommandVideoBase
        {
            public uint Command;
            public uint Overlay;
        }

        public struct FIFO_EscapeCommandVideoFlush
        {
            FIFO_EscapeCommandVideoBase VideoCommand;
        }

        public struct FIFO_EscapeCommandVideoSetRegs
        {
            FIFO_EscapeCommandVideoBase VideoCommand;

            public VideoSetRegsItem Item0;
        }

        public unsafe struct FIFO_EscapeCommandVideoSetAllRegs
        {
            FIFO_EscapeCommandVideoBase VideoCommand;
            
            public VideoSetRegsItem Item0;
        }
    }
}
