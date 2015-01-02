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
    public class Stsfld : ILOps.Stsfld
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if the value to store is floating point.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            int metadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
            FieldInfo theField = aScannerState.CurrentILChunk.Method.Module.ResolveField(metadataToken);
            string fieldID = aScannerState.GetStaticFieldID(theField);

            int size = Utils.GetNumBytesForType(theField.FieldType);
            bool isFloat = Utils.IsFloat(theField.FieldType);

            StackItem value = aScannerState.CurrentStackFrame.Stack.Pop();
            
            if (isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Storing static fields of type float not supported yet!");
            }

            if (size == 1)
            {
                result.AppendLine("pop dword eax");
                result.AppendLine(string.Format("mov byte [{0}], al", fieldID));
            }
            else if (size == 2)
            {
                result.AppendLine("pop dword eax");
                result.AppendLine(string.Format("mov word [{0}], ax", fieldID));
            }
            else if (size == 4)
            {
                result.AppendLine("pop dword eax");
                result.AppendLine(string.Format("mov dword [{0}], eax", fieldID));
            }
            else if (size == 8)
            {
                result.AppendLine("pop dword eax");
                result.AppendLine(string.Format("mov byte [{0}], eax", fieldID));
                result.AppendLine("pop dword eax");
                result.AppendLine(string.Format("mov byte [{0}+4], eax", fieldID));
            }

            return result.ToString().Trim();
        }
    }
}
