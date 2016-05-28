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
    public class Stobj : IL.ILOps.Stobj
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
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
            int size = theTypeInfo.IsValueType ? theTypeInfo.SizeOnHeapInBytes : theTypeInfo.SizeOnStackInBytes;

            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();

            //Load the object onto the stack
            conversionState.Append(new Mov
            {
                Size = OperandSize.Dword,
                Dest = "ECX",
                Src = "[ESP+" + theTypeInfo.SizeOnStackInBytes + "]"
            });
            if (size == 1)
            {
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});
                conversionState.Append(new Mov {Size = OperandSize.Byte, Src = "AL", Dest = "[ECX]"});
            }
            else if (size == 2)
            {
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});
                conversionState.Append(new Mov {Size = OperandSize.Word, Src = "AX", Dest = "[ECX]"});
            }
            else if (size >= 4)
            {
                for (int i = 0; i < size; i += 4)
                {
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});

                    switch (size - i)
                    {
                        case 1:
                            conversionState.Append(new Mov
                            {
                                Size = OperandSize.Byte,
                                Src = "AL",
                                Dest = "[ECX+" + i + "]"
                            });
                            break;
                        case 2:
                            conversionState.Append(new Mov
                            {
                                Size = OperandSize.Word,
                                Src = "AX",
                                Dest = "[ECX+" + i + "]"
                            });
                            break;
                        case 3:
                            conversionState.Append(new Mov
                            {
                                Size = OperandSize.Byte,
                                Src = "AL",
                                Dest = "[ECX+" + i + "]"
                            });
                            conversionState.Append(new ASMOps.Shr {Src = "16", Dest = "EAX"});
                            conversionState.Append(new Mov
                            {
                                Size = OperandSize.Word,
                                Src = "AX",
                                Dest = "[ECX+" + (i + 1) + "]"
                            });
                            break;
                        default:
                            conversionState.Append(new Mov
                            {
                                Size = OperandSize.Dword,
                                Src = "EAX",
                                Dest = "[ECX+" + i + "]"
                            });
                            break;
                    }
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("Storing object with unsupported size! Size: " + size);
            }

            conversionState.Append(new ASMOps.Add {Dest = "ESP", Src = "4"});
        }
    }
}