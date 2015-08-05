using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
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
                case OpCodes.Ldc_I4_8:
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_S:
                    numBytes = 4;
                    break;
            }

            conversionState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                sizeOnStackInBytes = numBytes,
                isFloat = isFloat,
                isGCManaged = false
            });
        }

        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Stores the integer value to push onto the stack
            long iValue = 0;
            //Indicates whether we should be pushing a float or integer value
            bool isFloat = false;
            //The number of bytes to push (e.g. 4 for Int32, 8 for Int64)
            int numBytes = 0;

            //Load the constant and type of constant
            switch((OpCodes)theOp.opCode.Value)
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
                case OpCodes.Ldc_I4_8:
                    iValue = 8;
                    numBytes = 4;
                    break;
                case OpCodes.Ldc_I4_S:
                    iValue = (Int32)(sbyte)theOp.ValueBytes[0];
                    numBytes = 4;
                    break;
            }

            //Stores the bytes to be pushed onto the stack
            byte[] valueBytes = new byte[0];
            if (isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Float constants not supported yet!");
            }
            else
            {
                //Get the bytes for the value
                valueBytes = BitConverter.GetBytes(iValue);
            }

            //Start the push (0x indicates what follows is a hex number)
            string valueToPush = "0x";
            for (int i = numBytes - 1; i > -1; i--)
            {
                valueToPush += valueBytes[i].ToString("X2");
            }
            conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = valueToPush });

            //Push the constant onto our stack
            conversionState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                sizeOnStackInBytes = numBytes,
                isFloat = isFloat,
                isGCManaged = false
            });
        }
    }
}
