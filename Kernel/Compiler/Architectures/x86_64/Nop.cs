using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.Architectures.x86_64
{
    public class Nop : ILOps.Nop
    {
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            return "nop";
        }
    }
}
