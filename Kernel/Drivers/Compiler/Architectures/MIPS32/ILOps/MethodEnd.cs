using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
    public class MethodEnd : IL.ILOps.MethodEnd
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            Type retType = (conversionState.Input.TheMethodInfo.IsConstructor ?
                typeof(void) : ((MethodInfo)conversionState.Input.TheMethodInfo.UnderlyingInfo).ReturnType);
            Types.TypeInfo retTypeInfo = conversionState.TheILLibrary.GetTypeInfo(retType);
            if (retTypeInfo.SizeOnStackInBytes != 0)
            {
                conversionState.CurrentStackFrame.Stack.Pop();
            }
        }

        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Store the return value
            //Get the return type
            Type retType = (conversionState.Input.TheMethodInfo.IsConstructor ?
                typeof(void) : ((MethodInfo)conversionState.Input.TheMethodInfo.UnderlyingInfo).ReturnType);
            Types.TypeInfo retTypeInfo = conversionState.TheILLibrary.GetTypeInfo(retType);
            //Get the size of the return type on stack
            int retSize = retTypeInfo.SizeOnStackInBytes;
            //If the size isn't 0 (i.e. isn't "void" which has no return value)
            if (retSize != 0)
            {
                //Pop the return value off our stack
                StackItem retItem = conversionState.CurrentStackFrame.Stack.Pop();

                //If it is float, well, we don't support it yet...
                if (retItem.isFloat)
                {
                    //SUPPORT - floats
                    throw new NotSupportedException("Floats return type not supported yet!");
                }
                //Otherwise, store the return value at [ebp+8]
                //[ebp+8] because that is last "argument"
                //      - read the calling convention spec
                else if (retSize == 4)
                {
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                    conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Dest = "8($fp)", Src = "$t0", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
                }
                else if (retSize == 8)
                {
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                    conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Dest = "8($fp)", Src = "$t0", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });

                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                    conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Dest = "12($fp)", Src = "$t0", MoveType = ASMOps.Mov.MoveTypes.SrcRegToDestMemory });
                }
                else
                {
                    throw new NotSupportedException("Return type size not supported / invalid!");
                }
            }

            //Once return value is off the stack, remove the locals
            //Deallocate stack space for locals
            //Only bother if there are any locals
            if (conversionState.Input.TheMethodInfo.LocalInfos.Count > 0)
            {
                //Get the total size of all locals
                int totalBytes = 0;
                foreach (Types.VariableInfo aLocal in conversionState.Input.TheMethodInfo.LocalInfos)
                {
                    totalBytes += aLocal.TheTypeInfo.SizeOnStackInBytes;
                }
                //Move esp past the locals
                conversionState.Append(new ASMOps.Add() { Src1 = "$sp", Src2 = totalBytes.ToString(), Dest = "$sp" });
            }

            //Restore ebp to previous method's ebp
            conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$fp" });
            //This pop also takes last value off the stack which
            //means top item is the return address
            //So ret command can now be correctly executed.
        }
    }
}
