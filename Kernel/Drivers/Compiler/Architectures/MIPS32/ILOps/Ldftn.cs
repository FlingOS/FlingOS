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
    public class Ldftn : IL.ILOps.Ldftn
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            conversionState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = false,
                sizeOnStackInBytes = 4,
                isGCManaged = false
            });
        }

        public override void Preprocess(ILPreprocessState preprocessState, ILOp theOp)
        {
            Types.MethodInfo methodInfo = preprocessState.TheILLibrary.GetMethodInfo(theOp.MethodToCall);

            if (theOp.LoadAtILOffset != int.MaxValue)
            {
                int offset = theOp.LoadAtILOffset;
                ILOp theLoadedOp = preprocessState.TheILLibrary.GetILBlock(methodInfo).At(offset);
                theLoadedOp.LabelRequired = true;
            }
        }

        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Get the ID (i.e. ASM label) of the method to load a pointer to
            Types.MethodInfo methodInfo = conversionState.TheILLibrary.GetMethodInfo(theOp.MethodToCall);
            string methodID = methodInfo.ID;
            bool addExternalLabel = methodID != conversionState.Input.TheMethodInfo.ID;

            //If we want to load the pointer at a specified IL op number:
            if (theOp.LoadAtILOffset != int.MaxValue)
            {
                //Append the IL sub-label to the ID
                ILBlock anILBlock = conversionState.TheILLibrary.GetILBlock(methodInfo);
                methodID = ASM.ASMBlock.GenerateLabel(methodID, anILBlock.PositionOf(anILBlock.At(theOp.LoadAtILOffset)));

                //Note: This is used by try/catch/finally blocks for pushing pointers 
                //      to catch/finally handlers and filters
            }

            if (addExternalLabel)
            {
                conversionState.AddExternalLabel(methodID);
            }

            //Push the pointer to the function
            conversionState.Append(new ASMOps.La() { Dest = "$t4", Label = methodID });
            conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t4" });

            conversionState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = false,
                sizeOnStackInBytes = 4,
                isGCManaged = false
            });
        }
    }
}
