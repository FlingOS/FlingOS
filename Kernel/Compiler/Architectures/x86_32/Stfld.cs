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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Kernel.Debug.Data;

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Stfld : ILOps.Stfld
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if the value to store is floating point or
        /// if the value is not 4 or 8 bytes in size.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            int metadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
            FieldInfo theField = aScannerState.CurrentILChunk.Method.Module.ResolveField(metadataToken);
            DB_Type objDBType = DebugDatabase.GetType(aScannerState.GetTypeID(theField.DeclaringType));

            int offset = aScannerState.GetFieldOffset(objDBType, theField.Name);

            int stackSize = Utils.GetNumBytesForType(theField.FieldType);
            int memSize = theField.FieldType.IsValueType ? Utils.GetSizeForType(theField.FieldType) : stackSize;

            StackItem value = aScannerState.CurrentStackFrame.Stack.Pop();
            StackItem objPointer = aScannerState.CurrentStackFrame.Stack.Pop();
            
            if (value.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Storing fields of type float not supported yet!");
            }

            //Get object pointer
            GlobalMethods.InsertPageFaultDetection(result, aScannerState, "esp", stackSize, (OpCodes)anILOpInfo.opCode.Value);
            result.AppendLine(string.Format("mov ecx, [esp+{0}]", stackSize));
            //Pop and mov value
            for (int i = 0; i < memSize; i += 2)
            {
                if (memSize - i == 1)
                {
                    result.AppendLine("pop word ax");
                    GlobalMethods.InsertPageFaultDetection(result, aScannerState, "ecx", offset + i, (OpCodes)anILOpInfo.opCode.Value);
                    result.AppendLine(string.Format("mov byte [ecx+{0}], al", offset + i));
                }
                else
                {
                    result.AppendLine("pop word ax");
                    GlobalMethods.InsertPageFaultDetection(result, aScannerState, "ecx", offset + i, (OpCodes)anILOpInfo.opCode.Value);
                    result.AppendLine(string.Format("mov word [ecx+{0}], ax", offset + i));
                }
            }
            result.AppendLine(string.Format("add esp, {0}", ((stackSize - memSize) / 2) * 2)); //Rounds down
            result.AppendLine("add esp, 4");//Pop object pointer

            return result.ToString().Trim();
        }
    }
}
