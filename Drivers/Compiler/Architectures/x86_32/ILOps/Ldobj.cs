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
using Drivers.Compiler.Architectures.x86.ASMOps;
using Drivers.Compiler.IL;
using Drivers.Compiler.Types;

namespace Drivers.Compiler.Architectures.x86
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
            //Load the metadata token used to get the type info
            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            //Get the type info for the object to load
            Type theType = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveType(metadataToken);
            TypeInfo theTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theType);

            //Get the object size information
            int memSize = theTypeInfo.IsValueType ? theTypeInfo.SizeOnHeapInBytes : theTypeInfo.SizeOnStackInBytes;

            //Load the object onto the stack
            conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "ECX"});

            int irregularSize = memSize%4;
            if (irregularSize > 0)
            {
                conversionState.Append(new ASMOps.Xor {Src = "EAX", Dest = "EAX"});
                switch (irregularSize)
                {
                    case 1:
                        conversionState.Append(new Mov
                        {
                            Size = OperandSize.Byte,
                            Src = "[ECX+" + (memSize - 1) + "]",
                            Dest = "AL"
                        });
                        break;
                    case 2:
                        conversionState.Append(new Mov
                        {
                            Size = OperandSize.Word,
                            Src = "[ECX+" + (memSize - 2) + "]",
                            Dest = "AX"
                        });
                        break;
                    case 3:
                        conversionState.Append(new Mov
                        {
                            Size = OperandSize.Byte,
                            Src = "[ECX+" + (memSize - 1) + "]",
                            Dest = "AL"
                        });
                        conversionState.Append(new ASMOps.Shl {Dest = "EAX", Src = "16"});
                        conversionState.Append(new Mov
                        {
                            Size = OperandSize.Word,
                            Src = "[ECX+" + (memSize - 3) + "]",
                            Dest = "AX"
                        });
                        break;
                }
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});
            }

            for (int i = memSize - irregularSize - 4; i >= 0; i -= 4)
            {
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Dword,
                    Src = "[ECX+" + i + "]",
                    Dest = "EAX"
                });
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});
            }

            // Pop address
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            // Push value
            conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
            {
                isFloat = false,
                sizeOnStackInBytes = memSize,
                isGCManaged = false,
                isValue = theTypeInfo.IsValueType
            });
        }
    }
}