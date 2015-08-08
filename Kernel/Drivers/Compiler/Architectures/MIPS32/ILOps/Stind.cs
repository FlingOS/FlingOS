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
    public class Stind : IL.ILOps.Stind
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            conversionState.CurrentStackFrame.Stack.Pop();
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
        /// <exception cref="System.NotImplementedException">
        /// Thrown if the op is 'StIndRef'.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Pop value
            //Pop address
            //Mov [address], value

            StackItem valueItem = conversionState.CurrentStackFrame.Stack.Pop();
            StackItem addressItem = conversionState.CurrentStackFrame.Stack.Pop();
            int bytesToStore = 0;
            bool isFloat = false;

            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Stind_I:
                    bytesToStore = 4;
                    break;
                case OpCodes.Stind_I1:
                    bytesToStore = 1;
                    break;
                case OpCodes.Stind_I2:
                    bytesToStore = 2;
                    break;
                case OpCodes.Stind_I4:
                    bytesToStore = 4;
                    break;
                case OpCodes.Stind_I8:
                    bytesToStore = 8;
                    break;
                case OpCodes.Stind_R4:
                    bytesToStore = 4;
                    isFloat = true;
                    break;
                case OpCodes.Stind_R8:
                    bytesToStore = 8;
                    isFloat = true;
                    break;
                case OpCodes.Stind_Ref:
                    bytesToStore = 4;
                    break;
            }

            if (isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Floats not supported yet!");
            }

            if (bytesToStore == 8)
            {
                //Pop value low bits
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                //Pop value high bits
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t3" });

                //Pop address
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t1" });

                //Mov [address], value
                // Assume half word misaligned
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t0", Dest = "0($t1)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
                conversionState.Append(new ASMOps.Srl() { Src = "$t0", Dest = "$t0", Bits = 8 });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t0", Dest = "1($t1)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
                conversionState.Append(new ASMOps.Srl() { Src = "$t0", Dest = "$t0", Bits = 8 });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t0", Dest = "2($t1)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
                conversionState.Append(new ASMOps.Srl() { Src = "$t0", Dest = "$t0", Bits = 8 });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t0", Dest = "3($t1)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });

                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t3", Dest = "4($t1)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
                conversionState.Append(new ASMOps.Srl() { Src = "$t3", Dest = "$t3", Bits = 8 });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t3", Dest = "5($t1)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
                conversionState.Append(new ASMOps.Srl() { Src = "$t3", Dest = "$t3", Bits = 8 });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t3", Dest = "6($t1)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
                conversionState.Append(new ASMOps.Srl() { Src = "$t3", Dest = "$t3", Bits = 8 });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t3", Dest = "7($t1)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
            }
            else if (bytesToStore == 4)
            {
                //Pop value
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });

                //Pop address
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t1" });

                //Mov [address], value
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t0", Dest = "0($t1)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
                conversionState.Append(new ASMOps.Srl() { Src = "$t0", Dest = "$t0", Bits = 8 });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t0", Dest = "1($t1)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
                conversionState.Append(new ASMOps.Srl() { Src = "$t0", Dest = "$t0", Bits = 8 });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t0", Dest = "2($t1)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
                conversionState.Append(new ASMOps.Srl() { Src = "$t0", Dest = "$t0", Bits = 8 });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t0", Dest = "3($t1)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
            }
            else if (bytesToStore == 2)
            {
                //Pop value
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });

                //Pop address
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t1" });

                //Mov [address], value
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t0", Dest = "0($t1)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
                conversionState.Append(new ASMOps.Srl() { Src = "$t0", Dest = "$t0", Bits = 8 });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t0", Dest = "1($t1)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
            }
            else if (bytesToStore == 1)
            {
                //Pop value
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });

                //Pop address
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t1" });

                //Mov [address], value
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t0", Dest = "0($t1)", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
            }
        }
    }
}
