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

namespace Drivers.Compiler.Architectures.x86
{
    /// <summary>
    ///     See base class documentation.
    /// </summary>
    public class Stind : IL.ILOps.Stind
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
        ///     Thrown if the value to store is floating point.
        /// </exception>
        /// <exception cref="System.NotImplementedException">
        ///     Thrown if the op is 'StIndRef'.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Pop value
            //Pop address
            //Mov [address], value

            StackItem valueItem = conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            StackItem addressItem = conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            int bytesToStore = 0;
            bool isFloat = false;

            switch ((OpCodes)theOp.opCode.Value)
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

            if (isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Floats not supported yet!");
            }

            if (bytesToStore == 8)
            {
                //Pop value low bits
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});
                //Pop value high bits
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EDX"});

                //Pop address
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EBX"});

                //Mov [address], value
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "EDX", Dest = "[EBX+4]"});
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "EAX", Dest = "[EBX]"});
            }
            else if (bytesToStore == 4)
            {
                //Pop value
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});

                //Pop address
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EBX"});

                //Mov [address], value
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "EAX", Dest = "[EBX]"});
            }
            else if (bytesToStore == 2)
            {
                //Pop value
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});

                //Pop address
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EBX"});

                //Mov [address], value
                conversionState.Append(new Mov {Size = OperandSize.Word, Src = "AX", Dest = "[EBX]"});
            }
            else if (bytesToStore == 1)
            {
                //Pop value
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});

                //Pop address
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EBX"});

                //Mov [address], value
                conversionState.Append(new Mov {Size = OperandSize.Byte, Src = "AL", Dest = "[EBX]"});
            }
        }
    }
}