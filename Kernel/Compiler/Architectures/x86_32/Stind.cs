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

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Stind : ILOps.Stind
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
        /// <exception cref="System.NotImplementedException">
        /// Thrown if the op is 'StIndRef'.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Pop value
            //Pop address
            //Mov [address], value

            StackItem valueItem = aScannerState.CurrentStackFrame.Stack.Pop();
            StackItem addressItem = aScannerState.CurrentStackFrame.Stack.Pop();
            int bytesToStore = 0;
            bool isFloat = false;

            switch ((OpCodes)anILOpInfo.opCode.Value)
            {
                case OpCodes.Stind_I:
                    bytesToStore = 4;
                    break;
                case OpCodes.Stind_I1:
                    bytesToStore = 1;
                    break;
                case OpCodes.Stind_I2:
                    bytesToStore = 2;
                    break;
                case OpCodes.Stind_I4:
                    bytesToStore = 4;
                    break;
                case OpCodes.Stind_I8:
                    bytesToStore = 8;
                    break;
                case OpCodes.Stind_R4:
                    bytesToStore = 4;
                    isFloat = true;
                    break;
                case OpCodes.Stind_R8:
                    bytesToStore = 8;
                    isFloat = true;
                    break;
                case OpCodes.Stind_Ref:
                    bytesToStore = 4;
                    break;
            }

            if(isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Floats not supported yet!");
            }

            if (bytesToStore == 8)
            {
                //Pop value low bits
                result.AppendLine("pop dword eax");
                //Pop value high bits
                result.AppendLine("pop dword edx");

                //Pop address
                result.AppendLine("pop dword ebx");

                //Mov [address], value
                GlobalMethods.InsertPageFaultDetection(result, aScannerState, "ebx", 4, (OpCodes)anILOpInfo.opCode.Value);
                result.AppendLine("mov dword [ebx+4], edx");
                GlobalMethods.InsertPageFaultDetection(result, aScannerState, "ebx", 0, (OpCodes)anILOpInfo.opCode.Value);
                result.AppendLine("mov dword [ebx], eax");
            }
            else if (bytesToStore == 4)
            {
                //Pop value
                result.AppendLine("pop dword eax");

                //Pop address
                result.AppendLine("pop dword ebx");

                //Mov [address], value
                GlobalMethods.InsertPageFaultDetection(result, aScannerState, "ebx", 0, (OpCodes)anILOpInfo.opCode.Value);
                result.AppendLine("mov dword [ebx], eax");
            }
            else if (bytesToStore == 2)
            {
                //Pop value
                result.AppendLine("pop dword eax");

                //Pop address
                result.AppendLine("pop dword ebx");

                //Mov [address], value
                GlobalMethods.InsertPageFaultDetection(result, aScannerState, "ebx", 0, (OpCodes)anILOpInfo.opCode.Value);
                result.AppendLine("mov word [ebx], ax");
            }
            else if (bytesToStore == 1)
            {
                //Pop value
                result.AppendLine("pop dword eax");

                //Pop address
                result.AppendLine("pop dword ebx");

                //Mov [address], value
                GlobalMethods.InsertPageFaultDetection(result, aScannerState, "ebx", 0, (OpCodes)anILOpInfo.opCode.Value);
                result.AppendLine("mov byte [ebx], al");
            }

            return result.ToString().Trim();
        }
    }
}
