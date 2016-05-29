#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //

#endregion

using Drivers.Compiler.Architectures.MIPS32.ASMOps;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
    /// <summary>
    ///     See base class documentation.
    /// </summary>
    public class Ldind : IL.ILOps.Ldind
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            StackItem addressItem = conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            int bytesToLoad = 0;

            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Ldind_U1:
                case OpCodes.Ldind_I1:
                    bytesToLoad = 1;
                    break;
                case OpCodes.Ldind_U2:
                case OpCodes.Ldind_I2:
                    bytesToLoad = 2;
                    break;
                case OpCodes.Ldind_U4:
                case OpCodes.Ldind_I4:
                case OpCodes.Ldind_I:
                    bytesToLoad = 4;
                    break;
                case OpCodes.Ldind_I8:
                    bytesToLoad = 8;
                    break;
                case OpCodes.Ldind_Ref:
                    bytesToLoad = Options.AddressSizeInBytes;
                    break;
            }

            conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
            {
                sizeOnStackInBytes = bytesToLoad == 8 ? 8 : 4,
                isFloat = false,
                isGCManaged = false,
                isValue = (OpCodes)theOp.opCode.Value != OpCodes.Ldind_Ref
            });
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Load indirect
            //Pop address
            //Push [address]

            StackItem addressItem = conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            int bytesToLoad = 0;

            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Ldind_U1:
                case OpCodes.Ldind_I1:
                    bytesToLoad = 1;
                    break;
                case OpCodes.Ldind_U2:
                case OpCodes.Ldind_I2:
                    bytesToLoad = 2;
                    break;
                case OpCodes.Ldind_U4:
                case OpCodes.Ldind_I4:
                case OpCodes.Ldind_I:
                    bytesToLoad = 4;
                    break;
                case OpCodes.Ldind_I8:
                    bytesToLoad = 8;
                    break;
                case OpCodes.Ldind_Ref:
                    bytesToLoad = Options.AddressSizeInBytes;
                    break;
            }

            int currOpPosition = conversionState.PositionOf(theOp);

            //Pop address
            conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t1"});

            if (bytesToLoad != 1)
            {
                // Alignment tests
                conversionState.Append(new ASMOps.And {Src1 = "$t1", Src2 = "3", Dest = "$t5"});
                conversionState.Append(new Branch
                {
                    BranchType = BranchOp.BranchEqual,
                    Src1 = "$t5",
                    Src2 = "1",
                    DestILPosition = currOpPosition,
                    Extension = "ByteAligned"
                });
                conversionState.Append(new Branch
                {
                    BranchType = BranchOp.BranchEqual,
                    Src1 = "$t5",
                    Src2 = "2",
                    DestILPosition = currOpPosition,
                    Extension = "HalfwordAligned"
                });
                conversionState.Append(new Branch
                {
                    BranchType = BranchOp.BranchEqual,
                    Src1 = "$t5",
                    Src2 = "3",
                    DestILPosition = currOpPosition,
                    Extension = "ByteAligned"
                });
                conversionState.Append(new Branch
                {
                    BranchType = BranchOp.Branch,
                    DestILPosition = currOpPosition,
                    Extension = "WordAligned"
                });
            }

            if ((OpCodes)theOp.opCode.Value == OpCodes.Ldind_Ref)
            {
                conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "ByteAligned"});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Byte,
                    Src = "3($t1)",
                    Dest = "$t0",
                    MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                });
                conversionState.Append(new Sll {Src = "$t0", Dest = "$t0", Bits = 24});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Byte,
                    Src = "2($t1)",
                    Dest = "$t4",
                    MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                });
                conversionState.Append(new Sll {Src = "$t4", Dest = "$t4", Bits = 16});
                conversionState.Append(new ASMOps.Or {Src1 = "$t4", Src2 = "$t0", Dest = "$t0"});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Byte,
                    Src = "1($t1)",
                    Dest = "$t4",
                    MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                });
                conversionState.Append(new Sll {Src = "$t4", Dest = "$t4", Bits = 8});
                conversionState.Append(new ASMOps.Or {Src1 = "$t4", Src2 = "$t0", Dest = "$t0"});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Byte,
                    Src = "0($t1)",
                    Dest = "$t4",
                    MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                });
                conversionState.Append(new ASMOps.Or {Src1 = "$t4", Src2 = "$t0", Dest = "$t0"});
                conversionState.Append(new Branch
                {
                    BranchType = BranchOp.Branch,
                    DestILPosition = currOpPosition,
                    Extension = "End"
                });

                conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "HalfwordAligned"});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Halfword,
                    Src = "2($t1)",
                    Dest = "$t0",
                    MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                });
                conversionState.Append(new Sll {Src = "$t0", Dest = "$t0", Bits = 16});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Halfword,
                    Src = "0($t1)",
                    Dest = "$t4",
                    MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                });
                conversionState.Append(new ASMOps.Or {Src1 = "$t4", Src2 = "$t0", Dest = "$t0"});
                conversionState.Append(new Branch
                {
                    BranchType = BranchOp.Branch,
                    DestILPosition = currOpPosition,
                    Extension = "End"
                });

                conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "WordAligned"});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "0($t1)",
                    Dest = "$t0",
                    MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                });
            }
            else
            {
                if (bytesToLoad == 1)
                {
                    conversionState.Append(new ASMOps.Xor {Src1 = "$t0", Src2 = "$t0", Dest = "$t0"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Byte,
                        Src = "0($t1)",
                        Dest = "$t0",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                }
                else if (bytesToLoad == 2)
                {
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "ByteAligned"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Byte,
                        Src = "1($t1)",
                        Dest = "$t0",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new Sll {Src = "$t0", Dest = "$t0", Bits = 8});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Byte,
                        Src = "0($t1)",
                        Dest = "$t4",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new ASMOps.Or {Src1 = "$t4", Src2 = "$t0", Dest = "$t0"});
                    conversionState.Append(new Branch
                    {
                        BranchType = BranchOp.Branch,
                        DestILPosition = currOpPosition,
                        Extension = "End"
                    });

                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "HalfwordAligned"});
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "WordAligned"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Halfword,
                        Src = "0($t1)",
                        Dest = "$t0",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                }
                else if (bytesToLoad == 4)
                {
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "ByteAligned"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Byte,
                        Src = "3($t1)",
                        Dest = "$t0",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new Sll {Src = "$t0", Dest = "$t0", Bits = 24});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Byte,
                        Src = "2($t1)",
                        Dest = "$t4",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new Sll {Src = "$t4", Dest = "$t4", Bits = 16});
                    conversionState.Append(new ASMOps.Or {Src1 = "$t4", Src2 = "$t0", Dest = "$t0"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Byte,
                        Src = "1($t1)",
                        Dest = "$t4",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new Sll {Src = "$t4", Dest = "$t4", Bits = 8});
                    conversionState.Append(new ASMOps.Or {Src1 = "$t4", Src2 = "$t0", Dest = "$t0"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Byte,
                        Src = "0($t1)",
                        Dest = "$t4",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new ASMOps.Or {Src1 = "$t4", Src2 = "$t0", Dest = "$t0"});
                    conversionState.Append(new Branch
                    {
                        BranchType = BranchOp.Branch,
                        DestILPosition = currOpPosition,
                        Extension = "End"
                    });

                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "HalfwordAligned"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Halfword,
                        Src = "2($t1)",
                        Dest = "$t0",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new Sll {Src = "$t0", Dest = "$t0", Bits = 16});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Halfword,
                        Src = "0($t1)",
                        Dest = "$t4",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new ASMOps.Or {Src1 = "$t4", Src2 = "$t0", Dest = "$t0"});
                    conversionState.Append(new Branch
                    {
                        BranchType = BranchOp.Branch,
                        DestILPosition = currOpPosition,
                        Extension = "End"
                    });

                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "WordAligned"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "0($t1)",
                        Dest = "$t0",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                }
                else if (bytesToLoad == 8)
                {
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "ByteAligned"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Byte,
                        Src = "7($t1)",
                        Dest = "$t0",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new Sll {Src = "$t0", Dest = "$t0", Bits = 24});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Byte,
                        Src = "6($t1)",
                        Dest = "$t4",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new Sll {Src = "$t4", Dest = "$t4", Bits = 16});
                    conversionState.Append(new ASMOps.Or {Src1 = "$t4", Src2 = "$t0", Dest = "$t0"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Byte,
                        Src = "4($t1)",
                        Dest = "$t4",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new Sll {Src = "$t4", Dest = "$t4", Bits = 8});
                    conversionState.Append(new ASMOps.Or {Src1 = "$t4", Src2 = "$t0", Dest = "$t0"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Byte,
                        Src = "5($t1)",
                        Dest = "$t4",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new ASMOps.Or {Src1 = "$t4", Src2 = "$t0", Dest = "$t0"});

                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});

                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Byte,
                        Src = "3($t1)",
                        Dest = "$t0",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new Sll {Src = "$t0", Dest = "$t0", Bits = 24});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Byte,
                        Src = "2($t1)",
                        Dest = "$t4",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new Sll {Src = "$t4", Dest = "$t4", Bits = 16});
                    conversionState.Append(new ASMOps.Or {Src1 = "$t4", Src2 = "$t0", Dest = "$t0"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Byte,
                        Src = "1($t1)",
                        Dest = "$t4",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new Sll {Src = "$t4", Dest = "$t4", Bits = 8});
                    conversionState.Append(new ASMOps.Or {Src1 = "$t4", Src2 = "$t0", Dest = "$t0"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Byte,
                        Src = "0($t1)",
                        Dest = "$t4",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new ASMOps.Or {Src1 = "$t4", Src2 = "$t0", Dest = "$t0"});
                    conversionState.Append(new Branch
                    {
                        BranchType = BranchOp.Branch,
                        DestILPosition = currOpPosition,
                        Extension = "End"
                    });


                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "HalfwordAligned"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Halfword,
                        Src = "6($t1)",
                        Dest = "$t0",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new Sll {Src = "$t0", Dest = "$t0", Bits = 16});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Halfword,
                        Src = "4($t1)",
                        Dest = "$t4",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new ASMOps.Or {Src1 = "$t4", Src2 = "$t0", Dest = "$t0"});

                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});

                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Halfword,
                        Src = "2($t1)",
                        Dest = "$t0",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new Sll {Src = "$t0", Dest = "$t0", Bits = 16});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Halfword,
                        Src = "0($t1)",
                        Dest = "$t4",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new ASMOps.Or {Src1 = "$t4", Src2 = "$t0", Dest = "$t0"});
                    conversionState.Append(new Branch
                    {
                        BranchType = BranchOp.Branch,
                        DestILPosition = currOpPosition,
                        Extension = "End"
                    });


                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "WordAligned"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "4($t1)",
                        Dest = "$t0",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "0($t1)",
                        Dest = "$t0",
                        MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                    });
                }
            }

            conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "End"});
            conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});

            conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
            {
                sizeOnStackInBytes = bytesToLoad == 8 ? 8 : 4,
                isFloat = false,
                isGCManaged = false,
                isValue = (OpCodes)theOp.opCode.Value != OpCodes.Ldind_Ref
            });
        }
    }
}