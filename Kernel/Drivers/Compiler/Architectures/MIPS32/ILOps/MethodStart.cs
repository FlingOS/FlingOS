using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32.ILOps
{
    public class MethodStart : IL.ILOps.MethodStart
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            base.PerformStackOperations(conversionState, theOp);
        }

        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            base.Convert(conversionState, theOp);
        }
    }
}
