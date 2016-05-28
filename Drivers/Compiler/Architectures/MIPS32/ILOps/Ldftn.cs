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
using Drivers.Compiler.ASM;
using Drivers.Compiler.IL;
using Drivers.Compiler.Types;

namespace Drivers.Compiler.Architectures.MIPS32
{
    /// <summary>
    ///     See base class documentation.
    /// </summary>
    public class Ldftn : IL.ILOps.Ldftn
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
            {
                isFloat = false,
                sizeOnStackInBytes = 4,
                isGCManaged = false,
                isValue = true
            });
        }

        public override void Preprocess(ILPreprocessState preprocessState, ILOp theOp)
        {
            MethodInfo methodInfo = preprocessState.TheILLibrary.GetMethodInfo(theOp.MethodToCall);

            if (theOp.LoadAtILOpAfterOp != null)
            {
                ILBlock anILBlock = preprocessState.TheILLibrary.GetILBlock(methodInfo);
                int index = anILBlock.ILOps.IndexOf(theOp.LoadAtILOpAfterOp);
                if (index == -1)
                {
                    throw new NullReferenceException("LoadAtILOpAfterOp not found in IL block!");
                }

                index++;
                anILBlock.ILOps[index].LabelRequired = true;
            }
            else if (theOp.LoadAtILOffset != int.MaxValue)
            {
                int offset = theOp.LoadAtILOffset;
                ILOp theLoadedOp = preprocessState.TheILLibrary.GetILBlock(methodInfo).At(offset);
                theLoadedOp.LabelRequired = true;
            }
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Get the ID (i.e. ASM label) of the method to load a pointer to
            MethodInfo methodInfo = conversionState.TheILLibrary.GetMethodInfo(theOp.MethodToCall);
            string methodID = methodInfo.ID;
            bool addExternalLabel = methodID != conversionState.Input.TheMethodInfo.ID;

            //If we want to load the pointer at a specified IL op number:
            if (theOp.LoadAtILOpAfterOp != null)
            {
                ILBlock anILBlock = conversionState.TheILLibrary.GetILBlock(methodInfo);
                int index = anILBlock.ILOps.IndexOf(theOp.LoadAtILOpAfterOp);
                index++;

                methodID = ASMBlock.GenerateLabel(methodID, anILBlock.PositionOf(anILBlock.ILOps[index]));
            }
            else if (theOp.LoadAtILOffset != int.MaxValue)
            {
                //Append the IL sub-label to the ID
                ILBlock anILBlock = conversionState.TheILLibrary.GetILBlock(methodInfo);
                methodID = ASMBlock.GenerateLabel(methodID, anILBlock.PositionOf(anILBlock.At(theOp.LoadAtILOffset)));

                //Note: This is used by try/catch/finally blocks for pushing pointers 
                //      to catch/finally handlers and filters
            }

            if (addExternalLabel)
            {
                conversionState.AddExternalLabel(methodID);
            }

            //Push the pointer to the function
            conversionState.Append(new La {Dest = "$t4", Label = methodID});
            conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t4"});

            conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
            {
                isFloat = false,
                sizeOnStackInBytes = 4,
                isGCManaged = false,
                isValue = true
            });
        }
    }
}