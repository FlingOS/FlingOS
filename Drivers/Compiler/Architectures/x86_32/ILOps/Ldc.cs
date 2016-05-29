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
    public class Ldc : IL.ILOps.Ldc
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            bool isFloat = false;
            int numBytes = 0;

            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Ldc_I4:
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_0:
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_1:
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_2:
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_3:
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_4:
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_5:
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_6:
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_7:
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_8:
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_M1:
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_S:
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I8:
                    numBytes = 8;
                    break;
                case OpCodes.Ldc_R4:
                    numBytes = 4;
                    isFloat = true;
                    break;
                case OpCodes.Ldc_R8:
                    numBytes = 8;
                    isFloat = true;
                    break;
            }

            conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
            {
                sizeOnStackInBytes = numBytes,
                isFloat = isFloat,
                isGCManaged = false,
                isValue = true
            });
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     Thrown if constant is a floating point number.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Stores the integer value to push onto the stack
            long iValue = 0;
            //Stores the float value to push onto the stack
            double fValue = 0;
            //Indicates whether we should be pushing a float or integer value
            bool isFloat = false;
            //The number of bytes to push (e.g. 4 for Int32, 8 for Int64)
            int numBytes = 0;

            //Load the constant and type of constant
            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Ldc_I4:
                    iValue = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_0:
                    iValue = 0;
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_1:
                    iValue = 1;
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_2:
                    iValue = 2;
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_3:
                    iValue = 3;
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_4:
                    iValue = 4;
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_5:
                    iValue = 5;
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_6:
                    iValue = 6;
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_7:
                    iValue = 7;
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_8:
                    iValue = 8;
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_M1:
                    iValue = -1;
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_S:
                    iValue = (sbyte)theOp.ValueBytes[0];
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I8:
                    iValue = Utilities.ReadInt64(theOp.ValueBytes, 0);
                    numBytes = 8;
                    break;
                case OpCodes.Ldc_R4:
                    fValue = Utilities.ReadFloat32(theOp.ValueBytes, 0);
                    numBytes = 4;
                    isFloat = true;
                    break;
                case OpCodes.Ldc_R8:
                    fValue = Utilities.ReadFloat64(theOp.ValueBytes, 0);
                    numBytes = 8;
                    isFloat = true;
                    break;
            }

            //Stores the bytes to be pushed onto the stack
            byte[] valueBytes = new byte[0];
            if (isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Float constants not supported yet!");
            }
            //Get the bytes for the value
            valueBytes = BitConverter.GetBytes(iValue);

            //If pushing Int64:
            if (numBytes == 8)
            {
                //Push the high-bits as a dword

                //Start the push (0x indicates what follows is a hex number)
                string valueToPush = "0x";
                //High bits
                //Process bits in reverse order i.e. highest bit first
                for (int i = numBytes - 1; i > 3; i--)
                {
                    valueToPush += valueBytes[i].ToString("X2");
                }
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = valueToPush});

                //Then push the low-bits as a dword
                //See above
                valueToPush = "0x";
                //Low bits
                for (int i = numBytes - 4 - 1; i > -1; i--)
                {
                    valueToPush += valueBytes[i].ToString("X2");
                }
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = valueToPush});
            }
            else
            {
                //See above

                string valueToPush = "0x";
                for (int i = numBytes - 1; i > -1; i--)
                {
                    valueToPush += valueBytes[i].ToString("X2");
                }
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = valueToPush});
            }

            //Push the constant onto our stack
            conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
            {
                sizeOnStackInBytes = numBytes,
                isFloat = isFloat,
                isGCManaged = false,
                isValue = true
            });
        }
    }
}