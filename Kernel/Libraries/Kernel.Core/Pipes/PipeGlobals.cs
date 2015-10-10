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
        Standard
    }
    public enum PipeSubclasses : uint
    {
        Standard_Out,
        Standard_In
    }
}
