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
    public class Ldobj : IL.ILOps.Ldobj
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            Type theType = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveType(metadataToken);
            TypeInfo theTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theType);
            int size = theTypeInfo.SizeOnStackInBytes;

            conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
            {
                isFloat = false,
                sizeOnStackInBytes = size,
                isGCManaged = false,
                isValue = theTypeInfo.IsValueType
            });
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     Thrown when loading a static float field.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Load static field

            //Load the metadata token used to get the type info
            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            //Get the type info for the object to load
            Type theType = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveType(metadataToken);
            TypeInfo theTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theType);

            //Get the object size information
            int size = theTypeInfo.SizeOnStackInBytes;

            //Load the object onto the stack
            conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t2"});
            for (int i = size - 4; i >= 0; i -= 4)
            {
                //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = i.ToString() + "($t2)", Dest = "$t0" });
                GlobalMethods.LoadData(conversionState, theOp, "$t2", "$t0", i, 4);
                conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});
            }
            int extra = size%4;
            for (int i = extra - 1; i >= 0; i--)
            {
                //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = i.ToString() + "($t2)", Dest = "$t0" });
                GlobalMethods.LoadData(conversionState, theOp, "$t2", "$t0", i, 1);
                conversionState.Append(new Push {Size = OperandSize.Byte, Src = "$t0"});
            }

            conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
            {
                isFloat = false,
                sizeOnStackInBytes = size,
                isGCManaged = false,
                isValue = theTypeInfo.IsValueType
            });
        }
    }
}