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

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Ldind : ILOps.Ldind
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Load indirect
            //Pop address
            //Push [address]

            StackItem addressItem = aScannerState.CurrentStackFrame.Stack.Pop();
            int bytesToLoad = 0;

            switch ((OpCodes)anILOpInfo.opCode.Value)
            {
                case OpCodes.Ldind_U1:
                case OpCodes.Ldind_I1:
                    bytesToLoad = 1;
                    break;
                case OpCodes.Ldind_U2:
                case OpCodes.Ldind_I2:
                    bytesToLoad = 2;
                    break;
                case OpCodes.Ldind_U4:
                case OpCodes.Ldind_I4:
                case OpCodes.Ldind_I:
                    bytesToLoad = 4;
                    break;
                case OpCodes.Ldind_I8:
                    bytesToLoad = 8;
                    break;
            }

            //Pop address
            result.AppendLine("pop dword ebx");

            if (bytesToLoad == 1)
            {
                result.AppendLine("xor eax, eax");
                GlobalMethods.InsertPageFaultDetection(result, aScannerState, "ebx", 0, (OpCodes)anILOpInfo.opCode.Value);
                result.AppendLine("mov byte al, [ebx]");
                result.AppendLine("push dword eax");
            }
            else if (bytesToLoad == 2)
            {
                result.AppendLine("xor eax, eax");
                GlobalMethods.InsertPageFaultDetection(result, aScannerState, "ebx", 0, (OpCodes)anILOpInfo.opCode.Value);
                result.AppendLine("mov word ax, [ebx]");
                result.AppendLine("push dword eax");
            }
            else if (bytesToLoad == 4)
            {
                GlobalMethods.InsertPageFaultDetection(result, aScannerState, "ebx", 0, (OpCodes)anILOpInfo.opCode.Value);
                result.AppendLine("push dword [ebx]");
            }
            else if (bytesToLoad == 8)
            {
                GlobalMethods.InsertPageFaultDetection(result, aScannerState, "ebx", 4, (OpCodes)anILOpInfo.opCode.Value);
                result.AppendLine("push dword [ebx+4]");
                GlobalMethods.InsertPageFaultDetection(result, aScannerState, "ebx", 0, (OpCodes)anILOpInfo.opCode.Value);
                result.AppendLine("push dword [ebx]");
            }

            aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                sizeOnStackInBytes = bytesToLoad == 8 ? 8 : 4,
                isFloat = false,
                isGCManaged = false
            });

            return result.ToString().Trim();
        }
    }
}
