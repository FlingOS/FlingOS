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
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.x86
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Stsfld : IL.ILOps.Stsfld
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if the value to store is floating point.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            FieldInfo theField = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveField(metadataToken);
            Types.FieldInfo theFieldInfo = conversionState.GetFieldInfo(theField.DeclaringType, theField.Name);

            string fieldId = theFieldInfo.ID;
            int size = conversionState.TheILLibrary.GetTypeInfo(theFieldInfo.FieldType).SizeOnStackInBytes;
            bool isFloat = Utilities.IsFloat(theField.FieldType);

            conversionState.AddExternalLabel(fieldId);

            StackItem value = conversionState.CurrentStackFrame.Stack.Pop();
            
            if (isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Storing static fields of type float not supported yet!");
            }

            if (size == 1)
            {
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EAX" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "AL", Dest = "[" + fieldId + "]" });
            }
            else if (size == 2)
            {
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EAX" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "AX", Dest = "[" + fieldId + "]" });
            }
            else if (size == 4)
            {
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EAX" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "EAX", Dest = "[" + fieldId + "]" });
            }
            else if (size == 8)
            {
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EAX" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "EAX", Dest = "[" + fieldId + "]" });
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EAX" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "EAX", Dest = "[" + fieldId + "+4]" });
            }
        }
    }
}
