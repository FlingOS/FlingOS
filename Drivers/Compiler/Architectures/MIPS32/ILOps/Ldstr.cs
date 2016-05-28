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
    public class Ldstr : IL.ILOps.Ldstr
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
            {
                sizeOnStackInBytes = 4,
                isFloat = false,
                isGCManaged = true,
                isValue = false
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
            //Load a string literal (- fixed string i.e. one programmed as "a string in code")

            //Get the string metadata token used to get the string from the assembly
            int StringMetadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            //Get the value of the string to load
            string theString =
                conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveString(StringMetadataToken);
            //Add the string literal and get its ID
            string theStringID = conversionState.TheILLibrary.AddStringLiteral(theString);
            conversionState.AddExternalLabel(theStringID);

            //Push the address of the string (i.e. address of ID - ASM label)
            conversionState.Append(new La {Dest = "$t4", Label = theStringID});
            conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t4"});

            conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
            {
                sizeOnStackInBytes = 4,
                isFloat = false,
                isGCManaged = true,
                isValue = false
            });
        }
    }
}