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
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.x86
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Ldind : IL.ILOps.Ldind
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Load indirect
            //Pop address
            //Push [address]

            StackItem addressItem = conversionState.CurrentStackFrame.Stack.Pop();
            int bytesToLoad = 0;

            switch ((OpCodes)theOp.opCode.Value)
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
            conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EBX" });

            if (bytesToLoad == 1)
            {
                conversionState.Append(new ASMOps.Xor() { Src = "EAX", Dest = "EAX" });
                GlobalMethods.InsertPageFaultDetection(conversionState, "ebx", 0, (OpCodes)theOp.opCode.Value);
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "[EBX]", Dest = "AL" });
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Dword, Src = "EAX" });
            }
            else if (bytesToLoad == 2)
            {
                conversionState.Append(new ASMOps.Xor() { Src = "EAX", Dest = "EAX" });
                GlobalMethods.InsertPageFaultDetection(conversionState, "ebx", 0, (OpCodes)theOp.opCode.Value);
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "[EBX]", Dest = "AX" });
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Dword, Src = "EAX" });
            }
            else if (bytesToLoad == 4)
            {
                GlobalMethods.InsertPageFaultDetection(conversionState, "ebx", 0, (OpCodes)theOp.opCode.Value);
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Dword, Src = "[EBX]" });
            }
            else if (bytesToLoad == 8)
            {
                GlobalMethods.InsertPageFaultDetection(conversionState, "ebx", 4, (OpCodes)theOp.opCode.Value);
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Dword, Src = "[EBX+4]" });
                GlobalMethods.InsertPageFaultDetection(conversionState, "ebx", 0, (OpCodes)theOp.opCode.Value);
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Dword, Src = "[EBX]" });
            }

            conversionState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                sizeOnStackInBytes = bytesToLoad == 8 ? 8 : 4,
                isFloat = false,
                isGCManaged = false
            });
        }
    }
}
