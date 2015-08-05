using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
    public class MethodStart : IL.ILOps.MethodStart
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            base.PerformStackOperations(conversionState, theOp);
        }

        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Save return address
            conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$ra" });
            //Push the previous method's fp
            conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$fp" });
            //Set fp for this method
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "$sp", Dest = "$fp" });

            //Allocate stack space for locals
            //Only bother if there are any locals
            if (conversionState.Input.TheMethodInfo.LocalInfos.Count > 0)
            {
                int totalBytes = 0;
                foreach (Types.VariableInfo aLocal in conversionState.Input.TheMethodInfo.LocalInfos)
                {
                    totalBytes += aLocal.TheTypeInfo.SizeOnStackInBytes;
                }
                //We do not use "sub esp, X" (see below) because that leaves
                //junk memory - we need memory to be "initialised" to 0 
                //so that local variables are null unless properly initialised.
                //This prevents errors in the GC.
                for (int i = 0; i < totalBytes / 4; i++)
                {
                    conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$0" });
                }
                //result.AppendLine(string.Format("sub esp, {0}", totalBytes));
            }
        }
    }
}
