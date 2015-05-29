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
    public class Shr : ILOps.Shr
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if either or both values to shift left are floating point values or
        /// if the values are 8 bytes in size.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if either or both values to multiply are not 4 or 8 bytes
        /// in size or if the values are of different size.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Pop in reverse order to push
            StackItem itemB = aScannerState.CurrentStackFrame.Stack.Pop();
            StackItem itemA = aScannerState.CurrentStackFrame.Stack.Pop();


            if (itemB.sizeOnStackInBytes < 4 ||
                itemA.sizeOnStackInBytes < 4)
            {
                throw new InvalidOperationException("Invalid stack operand sizes!");
            }
            else if (itemB.isFloat || itemA.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Shift right on floats is unsupported!");
            }
            else
            {
                string op = (OpCodes)anILOpInfo.opCode.Value == OpCodes.Shr_Un ? "shr" : "sar";

                if (itemA.sizeOnStackInBytes == 4 &&
                    itemB.sizeOnStackInBytes == 4)
                {
                    //Pop item B
                    result.AppendLine("pop dword ecx");
                    //Pop item A
                    result.AppendLine("pop dword eax");
                    result.AppendLine(op + " eax, cl");
                    result.AppendLine("push dword eax");

                    aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 4,
                        isGCManaged = false
                    });
                }
                else if ((itemA.sizeOnStackInBytes == 4 &&
                    itemB.sizeOnStackInBytes == 8))
                {
                    throw new InvalidOperationException("Invalid stack operand sizes! 4,8 not supported.");
                }
                else if ((itemA.sizeOnStackInBytes == 8 &&
                    itemB.sizeOnStackInBytes == 4))
                {
                    if ((OpCodes)anILOpInfo.opCode.Value == OpCodes.Shr_Un)
                    {
                        string shiftMoreThan32LabelName = string.Format("{0}.IL_{1}_Shift64",
                        aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                        anILOpInfo.Position);
                        string endLabelName = string.Format("{0}.IL_{1}_End",
                        aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                        anILOpInfo.Position);

                        //Pop item B
                        result.AppendLine("pop dword ecx");
                        //Pop item A (8 bytes)
                        result.AppendLine("pop dword eax");
                        result.AppendLine("pop dword edx");

                        //Check shift size
                        result.AppendLine("cmp ecx, 32");
                        result.AppendLine("jae " + shiftMoreThan32LabelName);

                        //Shr (< 32)
                        result.AppendLine("shrd eax, edx, cl");
                        result.AppendLine("shr edx, cl");
                        result.AppendLine("jmp " + endLabelName);

                        //Shr (>= 32)
                        result.AppendLine(shiftMoreThan32LabelName + ":");
                        result.AppendLine("mov eax, edx");
                        result.AppendLine("mov edx, 0");
                        result.AppendLine("sub ecx, 32");
                        result.AppendLine("shr eax, cl");

                        //Push result
                        result.AppendLine(endLabelName + ":");
                        result.AppendLine("push dword edx");
                        result.AppendLine("push dword eax");

                        aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                        {
                            isFloat = false,
                            sizeOnStackInBytes = 8,
                            isGCManaged = false
                        });
                    }
                    else
                    {
                        string shiftMoreThan32LabelName = string.Format("{0}.IL_{1}_Shift64",
                        aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                        anILOpInfo.Position);
                        string endLabelName = string.Format("{0}.IL_{1}_End",
                        aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                        anILOpInfo.Position);

                        //Pop item B
                        result.AppendLine("pop dword ecx");
                        //Pop item A (8 bytes)
                        result.AppendLine("pop dword eax");
                        result.AppendLine("pop dword edx");

                        //Check shift size
                        result.AppendLine("cmp ecx, 32");
                        result.AppendLine("jae " + shiftMoreThan32LabelName);

                        //Shr (< 32)
                        result.AppendLine("shrd eax, edx, cl");
                        result.AppendLine("sar edx, cl");
                        result.AppendLine("jmp " + endLabelName);

                        //Shr (>= 32)
                        result.AppendLine(shiftMoreThan32LabelName + ":");
                        result.AppendLine("mov eax, edx");
                        // (Preserve sign bit)
                        result.AppendLine("sar edx, 32");
                        result.AppendLine("sub ecx, 32");
                        result.AppendLine("sar eax, cl");

                        //Push result
                        result.AppendLine(endLabelName + ":");
                        result.AppendLine("push dword edx");
                        result.AppendLine("push dword eax");

                        aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                        {
                            isFloat = false,
                            sizeOnStackInBytes = 8,
                            isGCManaged = false
                        });
                    }
                }
                else if (itemA.sizeOnStackInBytes == 8 &&
                    itemB.sizeOnStackInBytes == 8)
                {
                    //Note: Shifting by more than 64 bits is pointless since the value will be annihilated entirely.
                    //          "64" fits well within the low 32-bits
                    //      So for this op, we do the same as the 8-4 byte version but discard the top four bytes
                    //          of the second operand
                    //      Except we must check the high bytes for non-zero value. If they are non-zero, we simply
                    //          push a result of zero.

                    if ((OpCodes)anILOpInfo.opCode.Value == OpCodes.Shr_Un)
                    {
                        string zeroLabelName = string.Format("{0}.IL_{1}_Zero",
                        aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                        anILOpInfo.Position);
                        string shiftMoreThan32LabelName = string.Format("{0}.IL_{1}_Shift64",
                        aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                        anILOpInfo.Position);
                        string end1LabelName = string.Format("{0}.IL_{1}_End1",
                        aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                        anILOpInfo.Position);
                        string end2LabelName = string.Format("{0}.IL_{1}_End2",
                        aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                        anILOpInfo.Position);

                        //Pop item B
                        result.AppendLine("pop dword ecx");
                        result.AppendLine("pop dword ebx");
                        //Pop item A (8 bytes)
                        result.AppendLine("pop dword eax");
                        result.AppendLine("pop dword edx");
                        //Check high 4 bytes of second param
                        result.AppendLine("cmp ebx, 0");
                        result.AppendLine("jz " + zeroLabelName);
                        result.AppendLine("push dword 0");
                        result.AppendLine("push dword 0");
                        result.AppendLine("jmp " + end2LabelName);
                        result.AppendLine(zeroLabelName + ":");

                        //Check shift size
                        result.AppendLine("cmp ecx, 32");
                        result.AppendLine("jae " + shiftMoreThan32LabelName);

                        //Shr (< 32)
                        result.AppendLine("shrd eax, edx, cl");
                        result.AppendLine("shr edx, cl");
                        result.AppendLine("jmp " + end1LabelName);

                        //Shr (>= 32)
                        result.AppendLine(shiftMoreThan32LabelName + ":");
                        result.AppendLine("mov eax, edx");
                        result.AppendLine("mov edx, 0");
                        result.AppendLine("sub ecx, 32");
                        result.AppendLine("shr eax, cl");

                        //Push result
                        result.AppendLine(end1LabelName + ":");
                        result.AppendLine("push dword edx");
                        result.AppendLine("push dword eax");
                        result.AppendLine(end2LabelName + ":");

                        aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                        {
                            isFloat = false,
                            sizeOnStackInBytes = 8,
                            isGCManaged = false
                        });
                    }
                    else
                    {
                        string zeroLabelName = string.Format("{0}.IL_{1}_Zero",
                        aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                        anILOpInfo.Position);
                        string shiftMoreThan32LabelName = string.Format("{0}.IL_{1}_Shift64",
                        aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                        anILOpInfo.Position);
                        string end1LabelName = string.Format("{0}.IL_{1}_End1",
                        aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                        anILOpInfo.Position);
                        string end2LabelName = string.Format("{0}.IL_{1}_End2",
                        aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                        anILOpInfo.Position);

                        //Pop item B
                        result.AppendLine("pop dword ecx");
                        result.AppendLine("pop dword ebx");
                        //Pop item A (8 bytes)
                        result.AppendLine("pop dword eax");
                        result.AppendLine("pop dword edx");
                        //Check high 4 bytes of second param
                        result.AppendLine("cmp ebx, 0");
                        result.AppendLine("jz " + zeroLabelName);
                        // (Preserve sign bit)
                        result.AppendLine("sar edx, 32");
                        result.AppendLine("push dword edx");
                        result.AppendLine("push dword edx");
                        result.AppendLine("jmp " + end2LabelName);
                        result.AppendLine(zeroLabelName + ":");

                        //Check shift size
                        result.AppendLine("cmp ecx, 32");
                        result.AppendLine("jae " + shiftMoreThan32LabelName);

                        //Shr (< 32)
                        result.AppendLine("shrd eax, edx, cl");
                        result.AppendLine("sar edx, cl");
                        result.AppendLine("jmp " + end1LabelName);

                        //Shr (>= 32)
                        result.AppendLine(shiftMoreThan32LabelName + ":");
                        result.AppendLine("mov eax, edx");
                        // (Preserve sign bit)
                        result.AppendLine("sar edx, 32");
                        result.AppendLine("sub ecx, 32");
                        result.AppendLine("sar eax, cl");

                        //Push result
                        result.AppendLine(end1LabelName + ":");
                        result.AppendLine("push dword edx");
                        result.AppendLine("push dword eax");
                        result.AppendLine(end2LabelName + ":");

                        aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                        {
                            isFloat = false,
                            sizeOnStackInBytes = 8,
                            isGCManaged = false
                        });
                    }
                }
            }


            return result.ToString().Trim();
        }
    }
}
