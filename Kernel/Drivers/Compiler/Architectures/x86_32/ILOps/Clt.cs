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
    public class Clt : IL.ILOps.Clt
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if compare operands are floating point numbers or if either value is &lt; 4 bytes in length or
        /// operands are not of the same size.
        /// </exception>
        public virtual void Convert(ILConversionState conversionState, ILOp theOp)
        {
            

            //Pop in reverse order to push
            StackItem itemB = conversionState.CurrentStackFrame.Stack.Pop();
            StackItem itemA = conversionState.CurrentStackFrame.Stack.Pop();

            bool unsignedComparison = (OpCodes)theOp.opCode.Value == OpCodes.Clt_Un;
            int currOpPosition = conversionState.PositionOf(theOp);

            if (itemB.isFloat || itemA.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Add floats is unsupported!");
            }
            else if(itemA.sizeOnStackInBytes == 4 && itemB.sizeOnStackInBytes == 4)
            {
                //Pop item B
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EBX" });
                //Pop item A
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EAX" });
                //Compare A to B
                conversionState.Append(new ASMOps.Cmp() { Arg1 = "EAX", Arg2 = "EBX" });
                //If A >= B, jump to Else (not-true case)
                conversionState.Append(new ASMOps.Jmp() { JumpType = ASMOps.JmpOp.JumpGreaterThanEqual, DestILPosition = currOpPosition, Extension = "Else", UnsignedTest = unsignedComparison });
                //Otherwise, A < B, so push true (true=1)
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Dword, Src = "1" });
                //And then jump to the end of this IL op.
                conversionState.Append(new ASMOps.Jmp() { JumpType = ASMOps.JmpOp.Jump, DestILPosition = currOpPosition, Extension = "End" });
                //Insert the Else label.
                conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "Else" });
                //Else case - Push false (false=0)
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Dword, Src = "0" });
                
                //Push the result onto our stack
                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false
                });
            }
            else if (itemA.sizeOnStackInBytes == 8 && itemB.sizeOnStackInBytes == 8)
            {
                //Pop item B
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EBX" });
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "ECX" });
                //Pop item A
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EAX" });
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EDX" });
                //If A high bytes < B high bytes : True
                //If A high bytes = B high bytes : Check, if A low bytes < B low bytes : True
                //Else : False
                
                //Compare high bytes
                conversionState.Append(new ASMOps.Cmp() { Arg1 = "EDX", Arg2 = "ECX" });
                //A high bytes < B high bytes? Jump to true case.
                conversionState.Append(new ASMOps.Jmp() { JumpType = ASMOps.JmpOp.JumpLessThan, DestILPosition = currOpPosition, Extension = "True", UnsignedTest = unsignedComparison });
                //A high bytes > B high bytes? Jump to else case.
                conversionState.Append(new ASMOps.Jmp() { JumpType = ASMOps.JmpOp.JumpGreaterThan, DestILPosition = currOpPosition, Extension = "Else", UnsignedTest = unsignedComparison });
                //Otherwise, A high bytes = B high bytes
                //So compare low bytes
                conversionState.Append(new ASMOps.Cmp() { Arg1 = "EAX", Arg2 = "EBX" });
                //A low bytes >= B low bytes? Jump to else case.
                conversionState.Append(new ASMOps.Jmp() { JumpType = ASMOps.JmpOp.JumpGreaterThanEqual, DestILPosition = currOpPosition, Extension = "Else", UnsignedTest = unsignedComparison });
                //Otherwise A < B.

                //Insert True case label
                conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "True" });
                //True case - Push true (true=1)
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Dword, Src = "1" });
                //And then jump to the end of this IL op.
                conversionState.Append(new ASMOps.Jmp() { JumpType = ASMOps.JmpOp.Jump, DestILPosition = currOpPosition, Extension = "End" });
                //Insert Else case label
                conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "Else" });
                //Else case - Push false (false=0)
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Dword, Src = "0" });
                
                //Push the result onto our stack
                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    // Yes, this is supposed to be 4 - the value that just got pushed was a 
                    // true / false integer
                    sizeOnStackInBytes = 4,
                    isGCManaged = false
                });
            }
            else
            {
                throw new NotSupportedException("Unsupported number of bytes for compare less than!");
            }

            //Always append the end label
            conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "End" });
        }
    }
}
