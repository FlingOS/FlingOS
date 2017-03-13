using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.VGA.VMWare
{
    public static class SVGAII_Mouse
    {
        public enum Commands : uint
        {
            READ_IO = 0x45414552,
            DISABLE = 0x000000f5,
            REQUEST_RELATIVE = 0x4c455252,
            REQUEST_ABSOLUTE = 0x53424152
        }

        public static string VERSION_ID_STR = "JUB4";
        public const uint VERSION_ID = 0x3442554a;

        public enum Ports : uint
        {
            Data = 96,
            Status = 100,
            IRQ = 12,
            Error = 0xffff0000
        }

        [Flags]
        public enum MoveType
        {
            RELATIVE = 1,
            ABSOLUTE = 0
        }

        [Flags]
        public enum Buttons
        {
            LEFT = 0x20,
            RIGHT = 0x10,
            MIDDLE = 0x08
        }
    }
}
