using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Core.Pipes
{
    public static class PipeConstants
    {
        public const int UnlimitedConnections = -1;
    }
    public enum PipeClasses : uint
    {
        Display = 0xDEADBEEF
    }
    public enum PipeSubclasses : uint
    {
        Display_Text_ASCII = 0x2BADB002
    }
}
