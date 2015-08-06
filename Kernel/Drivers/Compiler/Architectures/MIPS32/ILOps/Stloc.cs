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
    public class Stloc : IL.ILOps.Stloc
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            conversionState.CurrentStackFrame.Stack.Pop();
        }

        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if the value to store is floating point.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            UInt16 localIndex = 0;
            switch ((ILOp.OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Stloc:
                    localIndex = (UInt16)Utilities.ReadInt16(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Stloc_0:
                    localIndex = 0;
                    break;
                case OpCodes.Stloc_1:
                    localIndex = 1;
                    break;
                case OpCodes.Stloc_2:
                    localIndex = 2;
                    break;
                case OpCodes.Stloc_3:
                    localIndex = 3;
                    break;
                case OpCodes.Stloc_S:
                    localIndex = (UInt16)theOp.ValueBytes[0];
                    break;
            }

            int bytesOffset = 0;
            for (int i = 0; i < conversionState.Input.TheMethodInfo.LocalInfos.Count && i <= localIndex; i++)
            {
                bytesOffset += conversionState.Input.TheMethodInfo.LocalInfos.ElementAt(i).TheTypeInfo.SizeOnStackInBytes;
            }
            StackItem theItem = conversionState.CurrentStackFrame.Stack.Pop();
            if (theItem.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Float locals not supported yet!");
            }

            int locSize = conversionState.Input.TheMethodInfo.LocalInfos.ElementAt(localIndex).TheTypeInfo.SizeOnStackInBytes;
            if (locSize == 0)
            {
                conversionState.Append(new ASMOps.Comment("0 pop size (?!)"));
            }
            else if (locSize == 8)
            {
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "$t0", Dest = "-" + bytesOffset.ToString() + "($fp)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "$t0", Dest = "-" + (bytesOffset - 4).ToString() + "($fp)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
            }
            else if (locSize == 4)
            {
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest =  "$t0"});
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "$t0", Dest = "-" + bytesOffset.ToString() + "($fp)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });

            }
            else if (locSize == 2)
            {
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Halfword, Dest = "$t0" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Halfword, Src = "$t0", Dest = "-" + bytesOffset.ToString() + "($fp)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });

            }
            else if (locSize == 1)
            {
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t0", Dest = "-" + bytesOffset.ToString() + "($fp)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });

            }
            else
            {
                for (int i = 0; i < locSize; i++)
                {
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Byte, Dest = "$t0" });
                    conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t0", Dest = "-" + (bytesOffset - i).ToString() + "($fp)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
                }
            }
        }
    }
}
