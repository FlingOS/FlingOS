#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Ldsfld : ILOps.Ldsfld
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when loading a static float field.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Load static field

            //Load the metadata token used to get the field info
            int metadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
            //Get the field info for the field to load
            FieldInfo theField = aScannerState.CurrentILChunk.Method.Module.ResolveField(metadataToken);
            //Get the ID (i.e. ASM label) of the field to load
            string fieldID = aScannerState.GetStaticFieldID(theField);

            //Load the field or field address
            switch ((OpCodes)anILOpInfo.opCode.Value)
            {
                case OpCodes.Ldsfld:
                    {
                        int size = Utils.GetNumBytesForType(theField.FieldType);
                        bool isFloat = Utils.IsFloat(theField.FieldType);
                        
                        if (isFloat)
                        {
                            //SUPPORT - floats
                            throw new NotSupportedException("Loading static fields of type float not supported yet!");
                        }

                        if(size == 1)
                        {
                            result.AppendLine("xor eax, eax");
                            result.AppendLine(string.Format("mov byte al, [{0}]", fieldID));
                            result.AppendLine("push dword eax");
                        }
                        else if(size == 2)
                        {
                            result.AppendLine("xor eax, eax");
                            result.AppendLine(string.Format("mov word ax, [{0}]", fieldID));
                            result.AppendLine("push dword eax");
                        }
                        else if(size == 4)
                        {
                            result.AppendLine(string.Format("mov dword eax, [{0}]", fieldID));
                            result.AppendLine("push dword eax");
                        }
                        else if (size == 8)
                        {
                            result.AppendLine(string.Format("mov dword eax, [{0}+4]", fieldID));
                            result.AppendLine("push dword eax");
                            result.AppendLine(string.Format("mov dword eax, [{0}]", fieldID));
                            result.AppendLine("push dword eax");
                        }

                        aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                        {
                            isFloat = isFloat,
                            sizeOnStackInBytes = (size == 8 ? 8 : 4),
                            isGCManaged = Utils.IsGCManaged(theField.FieldType)
                        });
                    }
                    break;
                case OpCodes.Ldsflda:
                    //Load the address of the field i.e. address of the ASM label
                    result.AppendLine(string.Format("push dword {0}", fieldID));

                    aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 4,
                        isGCManaged = false
                    });
                    break;
            }

            return result.ToString().Trim();
        }
    }
}
