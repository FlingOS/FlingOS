using System;

namespace Kernel.VGA.VMWare
{
    public static class SVGAII_Registers
    {
        /// <summary>
        /// VMWare SVGA-II PCI Vendor ID value
        /// </summary>
        public const ushort PCI_VENDOR_ID = 0x15AD;

        /// <summary>
        /// VMWare SVGA-II PCI Device ID value
        /// </summary>
        public const ushort PCI_DEVICE_ID = 0x0405;
        
        [Flags]
        public enum REG_ENABLE : byte
        {
            DISABLE = 0x0,
            ENABLE = 0x1,
            HIDE = 0x2,
            ENABLE_HIDE = ENABLE | HIDE
        }

        public const uint FB_MAX_TRACEABLE_SIZE = 0x1000000;

        public const int MAX_PSEDUOCOLOUR_DEPTH = 8;
        public const int MAX_PSEDUOCOLOURS = 1 << MAX_PSEDUOCOLOUR_DEPTH;
        public const int NUM_PALETTE_REGS = 3 * MAX_PSEDUOCOLOURS;

        public const uint MAGIC = 0x900000;

        public enum VERSION : byte
        {
            _0 = 0,
            _1 = 1,
            _2 = 2
        }

        public enum ID : int
        {
            _0 = unchecked((int)(MAGIC << 8) | VERSION._0),
            _1 = unchecked((int)(MAGIC << 8) | VERSION._1),
            _2 = unchecked((int)(MAGIC << 8) | VERSION._2),
            INVALID = -1
        }

        public enum PORTS : ushort
        {
            INDEX = 0x0,
            VALUE = 0x1,
            BIOS = 0x2,
            IRQ_STATUS = 0x8
        }

        [Flags]
        public enum IRQ_FLAGS : byte
        {
            ANY_FENCE = 0x1,
            PROGRESS = 0x2,
            FENCE_GOAL = 0x4
        }

        public enum Registers : uint
        {
            ID = 0,
            ENABLE = 1,
            WIDTH = 2,
            HEIGHT = 3,
            MAX_WIDTH = 4,
            MAX_HEIGHT = 5,
            DEPTH = 6,
            BITS_PER_PIXEL = 7,
            PSEUDOCOLOR = 8,
            RED_MASK = 9,
            GREEN_MASK = 10,
            BLUE_MASK = 11,
            BYTES_PER_LINE = 12,
            FB_START = 13,
            FB_OFFSET = 14,
            VRAM_SIZE = 15,
            FB_SIZE = 16,

            CAPABILITIES = 17,
            MEM_START = 18,
            MEM_SIZE = 19,
            CONFIG_DONE = 20,
            SYNC = 21,
            BUSY = 22,
            GUEST_ID = 23,
            CURSOR_ID = 24,
            CURSOR_X = 25,
            CURSOR_Y = 26,
            CURSOR_ON = 27,
            HOST_BITS_PER_PIXEL = 28,
            SCRATCH_SIZE = 29,
            MEM_REGS = 30,
            NUM_DISPLAYS = 31,
            PITCHLOCK = 32,
            IRQMASK = 33,

            NUM_GUEST_DISPLAYS = 34,
            DISPLAY_ID = 35,
            DISPLAY_IS_PRIMARY = 36,
            DISPLAY_POSITION_X = 37,
            DISPLAY_POSITION_Y = 38,
            DISPLAY_WIDTH = 39,
            DISPLAY_HEIGHT = 40,

            GMR_ID = 41,
            GMR_DESCRIPTOR = 42,
            GMR_MAX_IDS = 43,
            GMR_MAX_DESCRIPTOR_LENGTH = 44,

            TRACES = 45,
            GMRS_MAX_PAGES = 46,
            MEMORY_SIZE = 47,
            TOP = 48,

            PALETTE_BASE = 1024,

            SCRATCH_BASE = PALETTE_BASE + NUM_PALETTE_REGS
        }

        public enum GuestMemoryRegion : uint
        {
            NULL = 0xFFFFFFFF,
            FRAMEBUFFER = 0xFFFFFFFE
        }

        [Flags]
        public enum Capabilities : uint
        {
            NONE = 0x00000000,
            RECT_COPY = 0x00000002,
            CURSOR = 0x00000020,
            CURSOR_BYPASS = 0x00000040,
            CURSOR_BYPASS_2 = 0x00000080,
            EMULATION_8BIT = 0x00000100,
            ALPHA_CURSOR = 0x00000200,
            _3D = 0x00004000,
            EXTENDED_FIFO = 0x00008000,
            MULTIMON = 0x00010000,
            PITCHLOCK = 0x00020000,
            IRQMASK = 0x00040000,
            DISPLAY_TOPOLOGY = 0x00080000,
            GMR = 0x00100000,
            TRACES = 0x00200000,
            GMR2 = 0x00400000,
            SCREEN_OBJECT_2 = 0x00800000
        }

        public enum FIFO : int
        {

            MIN = 0,
            MAX,
            NEXT_CMD,
            STOP,

            CAPABILITIES = 4,
            FLAGS,
            FENCE,

            HWVERSION_3D,

            PITCHLOCK,


            CURSOR_ON,
            CURSOR_X,
            CURSOR_Y,
            CURSOR_COUNT,
            CURSOR_LAST_UPDATED,

            RESERVED,

            CURSOR_SCREEN_ID,

            DEAD,

            HWVERSION_REVISED_3D,

            CAPS_3D = 32,
            CAPS_LAST_3D = 32 + 255,

            GUEST_3D_HWVERSION,
            FENCE_GOAL,
            BUSY,

            NUM_REGS
        }

        public const uint FIFO_EXTENDED_MANDATORY_REGS = (uint)FIFO.CAPS_LAST_3D + 1;

        public enum FIFO_Capabilities : uint
        {
            NONE = 0,
            FENCE = 1 << 0,
            ACCELFRONT = 1 << 1,
            PITCHLOCK = 1 << 2,
            VIDEO = 1 << 3,
            CURSOR_BYPASS_3 = 1 << 4,
            ESCAPE = 1 << 5,
            RESERVE = 1 << 6,
            SCREEN_OBJECT = 1 << 7,
            GMR2 = 1 << 8,
            HWVERSION_REVISED_3D = GMR2,
            SCREEN_OBJECT_2 = 1 << 9,
            DEAD = 1 << 10
        }

        public enum FIFO_Flags : uint
        {
            NONE = 0x0,
            ACCELFRONT = 0x1,
            RESERVED = 0x80000000
        }

        public const uint FIFO_RESERVED_UNKNOWN = 0xFFFFFFFF;

        public const uint NUM_OVERLAY_UNITS = 32;

        public const ushort VIDEO_FLAG_COLOURKEY = 0x0001;

        public enum VideoOverlayRegisterOffsets
        {
            ENABLED = 0,
            FLAGS,
            DATA_OFFSET,
            FORMAT,
            COLORKEY,
            SIZE,
            WIDTH,
            HEIGHT,
            SRC_X,
            SRC_Y,
            SRC_WIDTH,
            SRC_HEIGHT,
            DST_X,
            DST_Y,
            DST_WIDTH,
            DST_HEIGHT,
            PITCH_1,
            PITCH_2,
            PITCH_3,
            DATA_GMRID,
            DST_SCREEN_ID,
            NUM_REGS
        }

        [Flags]
        public enum Screen : byte
        {
            MUST_BE_SET = 0x1,
            HAS_ROOT = MUST_BE_SET,
            IS_PRIMARY = 0x2,
            FULLSCREEN_HINT = 0x4,
            DEACTIVATE = 0x8,
            BLANKING = 0x10
        }

        public enum FIFO_Command : uint
        {
            INVALID_CMD = 0,
            UPDATE = 1,
            RECT_COPY = 3,
            DEFINE_CURSOR = 19,
            DEFINE_ALPHA_CURSOR = 22,
            UPDATE_VERBOSE = 25,
            FRONT_ROP_FILL = 29,
            FENCE = 30,
            ESCAPE = 33,
            DEFINE_SCREEN = 34,
            DESTROY_SCREEN = 35,
            DEFINE_GMRFB = 36,
            BLIT_GMRFB_TO_SCREEN = 37,
            BLIT_SCREEN_TO_GMRFB = 38,
            ANNOTATION_FILL = 39,
            ANNOTATION_COPY = 40,
            DEFINE_GMR2 = 41,
            REMAP_GMR2 = 42,
            MAX
        }

        public const uint FIFO_Command_MAX_DATASIZE = 256 * 1024;
        public const uint FIFO_Command_MAX_ARGS = 64;

        public const byte ROP_COPY = 0x03;

        public enum RemapGMR2Flags : byte
        {
            PPN32 = 0,
            VIA_GMR = 0x1,
            PPN64 = 0x2,
            SINGLE_PPN = 0x4
        }

        public struct GuestMemoryDescriptor
        {
            public uint PPN;
            public uint NumPages;
        }

        public struct GuestPointer
        {
            public uint GMRId;
            public uint Offset;
        }

        public struct GMRImageFormat
        {
            public byte BitsPerPixel;
            public byte ColourDepth;
            public ushort Reserved;
        }

        public struct GuestImage
        {
            public GuestPointer Ptr;
            public uint Pitch;
        }

        public struct ColourBGRX
        {
            public byte B;
            public byte G;
            public byte R;
            public byte X;
        }

        public struct SignedRectangle
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public struct SignedPoint
        {
            public int X;
            public int Y;
        }

        public unsafe struct OverlayUnit
        {
            public uint enabled;
            public uint flags;
            public uint dataOffset;
            public uint format;
            public uint colorKey;
            public uint size;
            public uint width;
            public uint height;
            public uint srcX;
            public uint srcY;
            public uint srcWidth;
            public uint srcHeight;
            public int dstX;
            public int dstY;
            public uint dstWidth;
            public uint dstHeight;
            public fixed uint pitches [3];
            public uint dataGMRId;
            public uint dstScreenId;
        }

        public struct UnsignedDimensions
        {
            public uint Width;
            public uint Height;
        }

        public struct ScreenObject
        {
            public uint StructSize;
            public uint Id;
            public uint Flags;
            public UnsignedDimensions Size;
            public SignedPoint Root;
            public GuestImage BackingStore;
            public uint CloneCount;
        }

        public struct FIFO_CommandUpdate
        {
            public uint X;
            public uint Y;
            public uint Width;
            public uint Height;
        }

        public struct FIFO_CommandRectangleCopy
        {
            public uint SrcX;
            public uint SrcY;
            public uint DstX;
            public uint DstY;
            public uint Width;
            public uint Height;
        }

        public struct FIFO_CommandDefineCursor
        {
            public uint Id;
            public uint HotspotX;
            public uint HotspotY;
            public uint Width;
            public uint Height;
            public uint ANDMaskDepth;
            public uint XORMaskDepth;
        }

        public struct FIFO_CommandDefineAlphaCursor
        {
            public uint Id;
            public uint HotspotX;
            public uint HotspotY;
            public uint Width;
            public uint Height;
        }

        public struct FIFO_CommandUpdateVerbose
        {
            public uint X;
            public uint Y;
            public uint Width;
            public uint Height;
            public uint Reason;
        }

        public struct FIFO_CommandFrontROPFill
        {
            public uint Colour;
            public uint X;
            public uint Y;
            public uint Width;
            public uint Height;
            public uint ROP;
        }

        public struct FIFO_CommandFence
        {
            public uint Fence;
        }

        public struct FIFO_CommandEscape
        {
            public uint NSId;
            public uint Size;
        }

        public struct FIFO_CommandDefineScreen
        {
            public ScreenObject Screen;
        }

        public struct FIFO_CommandDestroyScreen
        {
            public uint ScreenId;
        }

        public struct FIFO_CommandDefineGMRFB
        {
            public GuestPointer Pointer;
            public uint BytesPerLine;
            public GMRImageFormat Format;
        }

        public struct FIFO_CommandBlitGMRFBToScreen
        {
            public SignedPoint SrcOrigin;
            public SignedRectangle DstRectangle;
            public uint DstScreenId;
        }

        public struct FIFO_CommandBlitScreenToGMRFB
        {
            public SignedPoint DstOrigin;
            public SignedRectangle SrcRectangle;
            public uint SrcScreenId;
        }

        public struct FIFO_CommandAnnotationFill
        {
            public ColourBGRX Colour;
        }

        public struct FIFO_CommandAnnotationCopy
        {
            public SignedPoint SrcOrigin;
            public uint SrcScreenId;
        }

        public struct FIFO_CommandDefineGMR2
        {
            public uint GMRId;
            public uint NumPages;
        }

        public struct FIFO_CommandRemapGMR2
        {
            public uint GMRId;
            public RemapGMR2Flags Flags;
            public uint OffsetPages;
            public uint NumPages;
        }

        public struct FIFO_CommandWithId
        {
            public uint id;
            public uint fence;
        }
    }
}
