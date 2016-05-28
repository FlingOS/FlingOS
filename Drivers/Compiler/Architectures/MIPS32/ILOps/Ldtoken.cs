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

namespace Drivers.Compiler.Architectures.MIPS32
{
    /// <summary>
    ///     See base class documentation.
    /// </summary>
    public class Ldtoken : IL.ILOps.Ldtoken
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            try
            {
                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false,
                    isValue = false
                });
            }
            catch
            {
                throw new NotSupportedException(
                    "The metadata token specifies a fieldref or methodref which isn't supported yet!");
            }
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     Thrown when the metadata token is not for method metadata.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Load token i.e. typeref
            //It should also support methodref and fieldrefs

            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            try
            {
                Type theType = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveType(metadataToken);
                if (theType == null)
                {
                    throw new NullReferenceException();
                }
                string typeTableId = conversionState.TheILLibrary.GetTypeInfo(theType).ID;
                conversionState.AddExternalLabel(typeTableId);

                conversionState.Append(new La {Dest = "$t4", Label = typeTableId});
                conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t4"});

                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false,
                    isValue = false
                });
            }
            catch
            {
                throw new NotSupportedException(
                    "The metadata token specifies a fieldref or methodref which isn't supported yet!");
            }
        }
    }
}