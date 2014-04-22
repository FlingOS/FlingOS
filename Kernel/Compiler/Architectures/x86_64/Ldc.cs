using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.Architectures.x86_64
{
    public class Ldc : ILOps.Ldc
    {
        static string[][] RegistersToUse = new string[][]
        {
            new string[4] { "cl", "dl", "r8l", "r9l"},
            new string[4] { "cx", "dx", "r8w", "r9w"},
            new string[4] { "ecx", "edx", "r8d", "r9d"},
            new string[4] { "rcx", "rdx", "r8", "r9"},
        };

        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            long iValue = 0;
            double fValue = 0f;
            bool isFloat = false;
            int bytes = 0;

            switch ((ILOps.ILOp.OpCodes)anILOpInfo.opCode.Value)
            {
                case ILOps.ILOp.OpCodes.Ldc_I4:
                    iValue = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
                    bytes = 4;
                    break;
                case ILOps.ILOp.OpCodes.Ldc_I4_0:
                    iValue = 0;
                    bytes = 4;
                    break;
                case ILOps.ILOp.OpCodes.Ldc_I4_1:
                    iValue = 1;
                    bytes = 4;
                    break;
                case ILOps.ILOp.OpCodes.Ldc_I4_2:
                    iValue = 2;
                    bytes = 4;
                    break;
                case ILOps.ILOp.OpCodes.Ldc_I4_3:
                    iValue = 3;
                    bytes = 4;
                    break;
                case ILOps.ILOp.OpCodes.Ldc_I4_4:
                    iValue = 4;
                    bytes = 4;
                    break;
                case ILOps.ILOp.OpCodes.Ldc_I4_5:
                    iValue = 5;
                    bytes = 4;
                    break;
                case ILOps.ILOp.OpCodes.Ldc_I4_6:
                    iValue = 6;
                    bytes = 4;
                    break;
                case ILOps.ILOp.OpCodes.Ldc_I4_7:
                    iValue = 7;
                    bytes = 4;
                    break;
                case ILOps.ILOp.OpCodes.Ldc_I4_8:
                    iValue = 8;
                    bytes = 4;
                    break;
                case ILOps.ILOp.OpCodes.Ldc_I4_M1:
                    iValue = -1;
                    bytes = 4;
                    break;
                case ILOps.ILOp.OpCodes.Ldc_I4_S:
                    iValue = (long)anILOpInfo.ValueBytes[0];
                    bytes = 1;
                    break;
                case ILOps.ILOp.OpCodes.Ldc_I8:
                    iValue = Utils.ReadInt64(anILOpInfo.ValueBytes, 0);
                    bytes = 8;
                    break;
                case ILOps.ILOp.OpCodes.Ldc_R4:
                    fValue = Utils.ReadFloat32(anILOpInfo.ValueBytes, 0);
                    bytes = 4;
                    break;
                case ILOps.ILOp.OpCodes.Ldc_R8:
                    fValue = Utils.ReadFloat64(anILOpInfo.ValueBytes, 0);
                    bytes = 8;
                    break;
            }

            if (isFloat)
            {
                throw new NotSupportedException("Pushing floating points not supported yet!");
            }
            else
            {
                //Push to:
                /*
                 * RCX
                 * RDX
                 * R8
                 * R9
                 * Stack
                 */

                StackFrame currFrame = aScannerState.StackFrames.Peek();
                if (currFrame.ActualStackSize < 4)
                {
                    //Stack is less than 4 long so we should push to RCX/etc.

                    //If this is 1 byte, we store using 8-bit format e.g. cl, dl, r8l, r9l
                    //If this is 2 bytes, we store using 16-bit format e.g. cx, dx, r8w, r9w
                    //If this is 4 bytes, we store using 32-bit format e.g. ecx, edx, r8d, r9d
                    //If this is 8 bytes, we store using 64-bit format e.g. rcx, rdx, r8, r9

                    string register = RegistersToUse[(int)Math.Log(bytes, 2)][currFrame.ActualStackSize];
                    switch(register)
                    {
                        //We need to store the arguments already in RCX/RDX/R8/R9
                        //But also check they haven't already been stored
                        //The caller is responsible for allocating stack space for storing RCX/etc.

                        case "cl":
                        case "cx":
                        case "ecx":
                        case "rcx":
                            //Store rcx
                            if (currFrame.NeedToStoreRCX)
                            {
                                result.AppendLine("mov [ebp+8], rcx");
                                currFrame.RCXStored = true;
                            }
                            break;

                        case "dl":
                        case "dx":
                        case "edx":
                        case "rdx":
                            //Store rdx
                            if (currFrame.NeedToStoreRDX)
                            {
                                result.AppendLine("mov [ebp+16], rdx");
                                currFrame.RDXStored = true;
                            }
                            break;

                        case "r8l":
                        case "r8w":
                        case "r8d":
                        case "r8":
                            //Store r8
                            if (currFrame.NeedToStoreR8)
                            {
                                result.AppendLine("mov [ebp+24], r8");
                                currFrame.R8Stored = true;
                            }
                            break;

                        case "r9l":
                        case "r9w":
                        case "r9d":
                        case "r9":
                            //Store r9
                            if (currFrame.NeedToStoreR9)
                            {
                                result.AppendLine("mov [ebp+32], r9");
                                currFrame.R9Stored = true;
                            }
                            break;
                    }
                    result.AppendLine(string.Format("mov {0}, {1}d", register, iValue.ToString()));

                    currFrame.Stack.Push(new StackItem()
                    {
                        sizeOnStackInBytes = bytes,
                        register = register
                    });
                }
                else
                {
                    //Stack is 4 or more long already so registers are filled so just do a normal push

                    result.AppendLine("push " + iValue.ToString());

                    currFrame.Stack.Push(new StackItem()
                    {
                        sizeOnStackInBytes = bytes,
                        register = null
                    });
                }
            }

            return result.ToString().Trim();
        }
    }
}
