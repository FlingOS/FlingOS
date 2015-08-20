using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Switch : IL.ILOps.Switch
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            conversionState.CurrentStackFrame.Stack.Pop();
        }

        public override void Preprocess(ILPreprocessState preprocessState, ILOp theOp)
        {
            for (int i = 0; i < theOp.ValueBytes.Length / 4; i++)
            {
                int branchOffset = theOp.NextOffset + Utilities.ReadInt32(theOp.ValueBytes, i * 4);
                ILOp opToGoTo = preprocessState.Input.At(branchOffset);
                opToGoTo.LabelRequired = true;
            }
        }

        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if divide operands are floating point numbers or if attempting to divide 64-bit numbers.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if either operand is &lt; 4 bytes long.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            StackItem testItem = conversionState.CurrentStackFrame.Stack.Pop();

            if (testItem.isFloat)
            {
                //TODO - Support floats
                throw new NotSupportedException("Switch for floats no supported!");
            }
            else if (testItem.sizeOnStackInBytes != 4)
            {
                //TODO - Support other sizes
                throw new NotSupportedException("Switch for non-int32s not supported!");
            }

            conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
            for (int i = 0; i < theOp.ValueBytes.Length / 4; i++)
            {
                int branchOffset = theOp.NextOffset + Utilities.ReadInt32(theOp.ValueBytes, i * 4);
                ILOp opToGoTo = conversionState.Input.At(branchOffset);
                int branchPos = conversionState.PositionOf(opToGoTo);

                conversionState.Append(new ASMOps.Branch() { BranchType = ASMOps.BranchOp.BranchEqual, Src2 = i.ToString(), Src1 = "$t0", DestILPosition = branchPos });
            }
        }
    }
}
