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

using System;
using Drivers.Compiler.Architectures.MIPS32.ASMOps;
using Drivers.Compiler.IL;
using Drivers.Compiler.Types;

namespace Drivers.Compiler.Architectures.MIPS32
{
    /// <summary>
    ///     See base class documentation.
    /// </summary>
    public class Isinst : IL.ILOps.Isinst
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            //Weirdly this is not a true/false returning op - it actually returns a null or object ref.
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            int currOpPosition = conversionState.PositionOf(theOp);

            // Test if the object provided inherits from the specified class
            // 1. Pop object ref
            // 1.1. Test if object ref is null:
            // 1.1.1 True: Push null and continue
            // 1.1.2 False: Go to 2
            // 2. Load object type
            // 3. Test if object type == provided type:
            //      3.1 True: Push object ref and continue
            //      3.2 False: 
            //      3.2.1. Move to base type
            //      3.2.2. Test if base type null:
            //      3.2.2.1   True: Push null and continue
            //      3.2.2.2   False: Jump back to (3)

            // 1. Pop object ref
            conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});

            // 1.1. Test if object ref is null:
            conversionState.Append(new Branch
            {
                Src1 = "$t0",
                Src2 = "0",
                BranchType = BranchOp.BranchNotEqual,
                DestILPosition = currOpPosition,
                Extension = "False1",
                UnsignedTest = true
            });

            // 1.1.1 True: Push null and continue
            conversionState.Append(new Push {Size = OperandSize.Word, Src = "$zero"});
            conversionState.Append(new Branch
            {
                BranchType = BranchOp.Branch,
                DestILPosition = currOpPosition,
                Extension = "End"
            });

            // 1.1.2 False: Go to 2
            conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "False1"});

            // 2. Load object type
            //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "0($t0)", Dest = "$t1", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
            GlobalMethods.LoadData(conversionState, theOp, "$t0", "$t1", 0, 4);

            // 3. Test if object type == provided type:
            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            Type theType = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveType(metadataToken);
            TypeInfo theTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theType);
            string TestTypeId = theTypeInfo.ID;
            conversionState.AddExternalLabel(TestTypeId);

            conversionState.Append(new La {Label = TestTypeId, Dest = "$t2"});

            conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "Label3"});

            conversionState.Append(new Branch
            {
                Src1 = "$t1",
                Src2 = "$t2",
                BranchType = BranchOp.BranchNotEqual,
                DestILPosition = currOpPosition,
                Extension = "False2",
                UnsignedTest = true
            });

            //      3.1 True: Push object ref and continue
            conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});
            conversionState.Append(new Branch
            {
                BranchType = BranchOp.Branch,
                DestILPosition = currOpPosition,
                Extension = "End"
            });

            //      3.2 False: 
            conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "False2"});

            //      3.2.1. Move to base type
            int baseTypeOffset = conversionState.GetTypeFieldOffset("TheBaseType");
            //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = baseTypeOffset + "($t1)", Dest = "$t1" });
            GlobalMethods.LoadData(conversionState, theOp, "$t1", "$t1", baseTypeOffset, 4);


            //      3.2.2. Test if base type null:
            //      3.2.2.2   False: Jump back to (3)
            conversionState.Append(new Branch
            {
                Src1 = "$t1",
                Src2 = "0",
                BranchType = BranchOp.BranchNotEqual,
                DestILPosition = currOpPosition,
                Extension = "Label3",
                UnsignedTest = true
            });

            //      3.2.2.1   True: Push null and continue
            conversionState.Append(new Push {Size = OperandSize.Word, Src = "$zero"});

            conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "End"});
        }
    }
}