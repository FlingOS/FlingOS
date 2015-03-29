using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public static class ASMUtilities
    {
        public static string GetOpSizeStr(OperandSize Size)
        {
            return System.Enum.GetName(typeof(OperandSize), Size);
        }
    }
}
