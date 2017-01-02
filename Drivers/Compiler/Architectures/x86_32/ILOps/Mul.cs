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
    public class Mul : IL.ILOps.Mul
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            //Pop in reverse order to push
            StackItem itemB = conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            StackItem itemA = conversionState.CurrentStackFrame.GetStack(theOp).Pop();

            if (itemA.sizeOnStackInBytes == 4 &&
                itemB.sizeOnStackInBytes == 4)
            {
                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false,
                    isValue = itemA.isValue && itemB.isValue
                });
            }
            else if (itemA.sizeOnStackInBytes == 8 &&
                     itemB.sizeOnStackInBytes == 8)
            {
                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    isFloat = false,
                    isNewGCObject = false,
                    sizeOnStackInBytes = 8,
                    isGCManaged = false,
                    isValue = itemA.isValue && itemB.isValue
                });
            }
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     Thrown if either or both values to multiply are floating point values or
        ///     if the values are 8 bytes in size.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     Thrown if either or both values to multiply are not 4 or 8 bytes
        ///     in size or if the values are of different size.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Pop in reverse order to push
            StackItem itemB = conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            StackItem itemA = conversionState.CurrentStackFrame.GetStack(theOp).Pop();


            if (itemB.sizeOnStackInBytes < 4 ||
                itemA.sizeOnStackInBytes < 4)
            {
                throw new InvalidOperationException("Invalid stack operand sizes!");
            }
            if (itemB.isFloat || itemA.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Multiply floats is unsupported!");
            }
            if (itemA.sizeOnStackInBytes == 4 &&
                itemB.sizeOnStackInBytes == 4)
            {
                //Pop item B
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EBX"});
                //Pop item A
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});
                //Sign extend A to EAX:EDX
                conversionState.Append(new Cdq());
                //Do the multiplication
                conversionState.Append(new ASMOps.Mul {Arg = "EBX", Signed = true});
                //Result stored in eax
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});

                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false,
                    isValue = itemA.isValue && itemB.isValue
                });
            }
            else if ((itemA.sizeOnStackInBytes == 8 &&
                      itemB.sizeOnStackInBytes == 4) ||
                     (itemA.sizeOnStackInBytes == 4 &&
                      itemB.sizeOnStackInBytes == 8))
            {
                throw new InvalidOperationException("Invalid stack operand sizes! They should be 32-32 or 64-64.");
            }
            else if (itemA.sizeOnStackInBytes == 8 &&
                     itemB.sizeOnStackInBytes == 8)
            {
                /*TODO: This long multiplication really doesn't work in practice for signed values... 
                     *          because we can't tell the computer to treat one value as signed and 
                     *          another as unsigned in a single multiply. We would need to store the
                     *          sign bit of the high parts (AH and BH) then make them unsigned.
                     *          Then do each part of the multiplication unsigned, then apply sign-combination
                     *          rules to each sub-part to make relevant sub-parts signed, then sum all the 
                     *          sub-parts.
                     */

                Logger.LogWarning(Errors.ILCompiler_ScanILOpCustomWarning_ErrorCode, "", 0,
                    string.Format(Errors.ErrorMessages[Errors.ILCompiler_ScanILOpCustomWarning_ErrorCode],
                        "All 64-bit multiplication is treated as unsigned. Ensure you didn't intend signed 64-bit multiplication. Signed 64-bit multiplication is not supported yet."));

                //A = item A, B = item B
                //L = low bits, H = high bits
                // => A = AL + AH, B = BL + BH

                // A * B = (AL + AH) * (BL + BH)
                //       = (AL * BL) + (AL * BH) + (AH * BL) (Ignore: + (AH * BH))

                // AH = [ESP+12]
                // AL = [ESP+8]
                // BH = [ESP+4]
                // BL = [ESP+0]

                // mov eax, 0        - Zero out registers
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "0", Dest = "EAX"});
                // mov ebx, 0
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "0", Dest = "EBX"});
                // mov ecx, 0
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "0", Dest = "ECX"});
                // mov edx, 0
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "0", Dest = "EDX"});

                // mov eax, [ESP+0] - Load BL
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "[ESP+0]", Dest = "EAX"});
                // mov ebx, [ESP+8] - Load AL
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "[ESP+8]", Dest = "EBX"});
                // mul ebx           - BL * AL, result in eax:edx
                conversionState.Append(new ASMOps.Mul {Arg = "EBX"});
                // push edx          - Push result keeping high bits
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EDX"});
                // push eax
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});

                //                   - Add 8 to offsets for result(s)

                // mov eax, 0        - Zero out registers
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "0", Dest = "EAX"});
                // mov edx, 0
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "0", Dest = "EDX"});
                // mov eax [ESP+4+8] - Load BH
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "[ESP+12]", Dest = "EAX"});
                // mul ebx           - BH * AL, result in eax:edx
                conversionState.Append(new ASMOps.Mul {Arg = "EBX"});
                // push eax          - Push result truncating high bits
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});

                //                   - Add 12 to offsets for result(s)

                // mov eax, 0        - Zero out registers
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "0", Dest = "EAX"});
                // mov edx, 0
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "0", Dest = "EDX"});
                // mov eax, [ESP+0+12] - Load BL
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "[ESP+12]", Dest = "EAX"});
                // mov ebx, [ESP+12+12] - Load AH
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "[ESP+24]", Dest = "EBX"});
                // mul ebx             - BL * AH, result in eax:edx
                conversionState.Append(new ASMOps.Mul {Arg = "EBX"});
                // push eax            - Push result truncating high bits
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});

                //                     - Add 16 to offsets for result(s)

                // AL * BL = [ESP+8] , 64 bits
                // AL * BH = [ESP+4] , 32 bits - high bits
                // AH * BL = [ESP+0] , 32 bits - high bits

                // mov eax, [ESP+8]  - Load AL * BL
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "[ESP+8]", Dest = "EAX"});
                // mov edx, [ESP+12]
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "[ESP+12]", Dest = "EDX"});
                // mov ebx, 0
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "0", Dest = "EBX"});
                // mov ecx, [ESP+4]   - Load AL * BH
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "[ESP+4]", Dest = "ECX"});
                // add edx, ecx       - Add (AL * BL) + (AL * BH), result in eax:edx
                conversionState.Append(new ASMOps.Add {Src = "ECX", Dest = "EDX"});
                // mov ecx, [ESP+0]   - Load AH * BL
                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "[ESP+0]", Dest = "ECX"});
                // add edx, ecx       - Add ((AL * BL) + (AL * BH)) + (AH * BL), result in eax:edx
                conversionState.Append(new ASMOps.Add {Src = "ECX", Dest = "EDX"});

                // add esp, 16+16     - Remove temp results and input values from stack
                conversionState.Append(new ASMOps.Add {Src = "32", Dest = "ESP"});

                // push edx           - Push final result
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EDX"});
                // push eax
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});

                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    isFloat = false,
                    isNewGCObject = false,
                    sizeOnStackInBytes = 8,
                    isGCManaged = false,
                    isValue = itemA.isValue && itemB.isValue
                });
            }
        }
    }
}