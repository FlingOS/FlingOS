using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Isinst : IL.ILOps.Isinst
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            //Weirdly this is not a true/false returning op - it actually returns a null or object ref.
        } 

        /// <summary>
        /// See base class documentation.
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
            conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });

            // 1.1. Test if object ref is null:
            conversionState.Append(new ASMOps.Branch() { Src1 = "$t0", Src2 = "0", BranchType = ASMOps.BranchOp.BranchNotEqual, DestILPosition = currOpPosition, Extension = "False1", UnsignedTest = true });

            // 1.1.1 True: Push null and continue
            conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "0" });
            conversionState.Append(new ASMOps.Branch() { BranchType = ASMOps.BranchOp.Branch, DestILPosition = currOpPosition, Extension = "End" });

            // 1.1.2 False: Go to 2
            conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "False1" });

            // 2. Load object type
            //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "0($t0)", Dest = "$t1", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
            GlobalMethods.LoadData(conversionState, theOp, "$t0", "$t1", 0, 4);

            // 3. Test if object type == provided type:
            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            Type theType = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveType(metadataToken);
            Types.TypeInfo theTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theType);
            string TestTypeId = theTypeInfo.ID;
            conversionState.AddExternalLabel(TestTypeId);

            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = TestTypeId, Dest = "$t2", MoveType = ASMOps.Mov.MoveTypes.ImmediateToReg });

            conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "Label3" });

            conversionState.Append(new ASMOps.Branch() { Src1 = "$t1", Src2 = "$t2", BranchType = ASMOps.BranchOp.BranchNotEqual, DestILPosition = currOpPosition, Extension = "False2", UnsignedTest = true });

            //      3.1 True: Push object ref and continue
            conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });
            conversionState.Append(new ASMOps.Branch() { BranchType = ASMOps.BranchOp.Branch, DestILPosition = currOpPosition, Extension = "End" });

            //      3.2 False: 
            conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "False2" });

            //      3.2.1. Move to base type
            int baseTypeOffset = conversionState.GetTypeFieldOffset("TheBaseType");
            //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = baseTypeOffset + "($t1)", Dest = "$t1" });
            GlobalMethods.LoadData(conversionState, theOp, "$t1", "$t1", baseTypeOffset, 4);


            //      3.2.2. Test if base type null:
            //      3.2.2.2   False: Jump back to (3)
            conversionState.Append(new ASMOps.Branch() { Src1 = "$t1", Src2 = "0", BranchType = ASMOps.BranchOp.BranchNotEqual, DestILPosition = currOpPosition, Extension = "Label3", UnsignedTest = true });

            //      3.2.2.1   True: Push null and continue
            conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "0" });

            conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "End" });
        }
    }
}
