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
using System.Reflection;
using System.Reflection.Emit;

namespace Drivers.Compiler.IL
{
    public static class ILReader
    {
        public static OpCode[] AllOpCodes = new OpCode[ushort.MaxValue];
        static ILReader()
        {
            LoadILOpTypes();
        }
        private static void LoadILOpTypes()
        {
            //Get the list of ILOps from the fields in OpCodes (all the fields in OpCodes are the ILOps)
            foreach (FieldInfo aField in typeof(OpCodes).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public))
            {
                //Get an instance of the op code
                var anOpCode = (OpCode)aField.GetValue(null);
                //Get the op code's identifying value and use it as the index in our array
                ushort index = (ushort)anOpCode.Value;
                //Set the op code in our array
                AllOpCodes[index] = anOpCode;
            }
        }


        public static ILBlock Read(Types.MethodInfo aMethodInfo)
        {
            if (aMethodInfo.IsPlugged)
            {
                return ReadPlugged(aMethodInfo);
            }
            else
            {
                return ReadNonPlugged(aMethodInfo);
            }
        }

        public static ILBlock ReadPlugged(Types.MethodInfo aMethodInfo)
        {
            string PlugPath = aMethodInfo.PlugAttribute.ASMFilePath;
            return new ILBlock()
            {
                PlugPath = string.IsNullOrWhiteSpace(PlugPath) ? " " : PlugPath,
                TheMethodInfo = aMethodInfo
            };
        }
        public static ILBlock ReadNonPlugged(Types.MethodInfo aMethodInfo)
        {
            ILBlock result = new ILBlock()
            {
                TheMethodInfo = aMethodInfo
            };

            MethodBody methodBody = aMethodInfo.MethodBody;
            //Method body for something like [DelegateType].Invoke() is null. 
            //  So just return an empty method if that is this case.
            if (methodBody != null)
            {
                foreach (LocalVariableInfo aLocal in methodBody.LocalVariables)
                {
                    aMethodInfo.LocalInfos.Add(new Types.VariableInfo()
                    {
                        UnderlyingType = aLocal.LocalType,
                        Position = aLocal.LocalIndex
                    });
                }

                byte[] ILBytes = methodBody.GetILAsByteArray();
                int ILBytesPos = 0;

                while (ILBytesPos < ILBytes.Length)
                {
                    OpCode currOpCode;
                    ushort currOpCodeID = 0;
                    int currOpBytesSize = 0;

                    if (ILBytes[ILBytesPos] == 0xFE)
                    {
                        currOpCodeID = (ushort)(0xFE00 + (short)ILBytes[ILBytesPos + 1]);
                        currOpBytesSize += 2;
                    }
                    else
                    {
                        currOpCodeID = (ushort)ILBytes[ILBytesPos];
                        currOpBytesSize++;
                    }
                    currOpCode = AllOpCodes[currOpCodeID];

                    int operandSize = 0;
                    switch (currOpCode.OperandType)
                    {
                        #region Operands

                        case OperandType.InlineBrTarget:
                            operandSize = 4;
                            break;
                        case OperandType.InlineField:
                            operandSize = 4;
                            break;
                        case OperandType.InlineI:
                            operandSize = 4;
                            break;
                        case OperandType.InlineI8:
                            operandSize = 8;
                            break;
                        case OperandType.InlineMethod:
                            operandSize = 4;
                            break;
                        case OperandType.InlineNone:
                            //No operands = no op size
                            break;
                        case OperandType.InlineR:
                            operandSize = 8;
                            break;
                        case OperandType.InlineSig:
                            operandSize = 4;
                            break;
                        case OperandType.InlineString:
                            operandSize = 4;
                            break;
                        case OperandType.InlineSwitch:
                            {
                                uint count = Utilities.ReadUInt32(ILBytes, ILBytesPos + currOpBytesSize);
                                currOpBytesSize += 4;
                                operandSize = (int)(count * 4);
                            }
                            break;
                        case OperandType.InlineTok:
                            operandSize = 4;
                            break;
                        case OperandType.InlineType:
                            operandSize = 4;
                            break;
                        case OperandType.InlineVar:
                            operandSize = 2;
                            break;
                        case OperandType.ShortInlineBrTarget:
                            operandSize = 1;
                            break;
                        case OperandType.ShortInlineI:
                            operandSize = 1;
                            break;
                        case OperandType.ShortInlineR:
                            operandSize = 4;
                            break;
                        case OperandType.ShortInlineVar:
                            operandSize = 1;
                            break;
                        default:
                            throw new Exception("Unrecognised operand type!");

                        #endregion
                    }

                    MethodBase methodToCall = null;
                    byte[] valueBytes = new byte[operandSize];
                    if (operandSize > 0)
                    {
                        Array.Copy(ILBytes, ILBytesPos + currOpBytesSize, valueBytes, 0, operandSize);

                        if (currOpCode.Equals(OpCodes.Call) ||
                            currOpCode.Equals(OpCodes.Calli) ||
                            currOpCode.Equals(OpCodes.Callvirt) ||
                            currOpCode.Equals(OpCodes.Ldftn) ||
                            currOpCode.Equals(OpCodes.Newobj))
                        {
                            int MethodMetadataToken = Utilities.ReadInt32(valueBytes, 0);
                            methodToCall = aMethodInfo.UnderlyingInfo.Module.ResolveMethod(MethodMetadataToken);
                        }
                    }

                    result.ILOps.Add(new ILOp()
                    {
                        opCode = currOpCode,
                        Offset = ILBytesPos,
                        BytesSize = currOpBytesSize + operandSize,
                        ValueBytes = valueBytes,
                        MethodToCall = methodToCall
                    });

                    ILBytesPos += currOpBytesSize + operandSize;
                }

                foreach (ExceptionHandlingClause aClause in methodBody.ExceptionHandlingClauses)
                {
                    ExceptionHandledBlock exBlock = result.GetExactExceptionHandledBlock(aClause.TryOffset);
                    if (exBlock == null)
                    {
                        exBlock = new ExceptionHandledBlock();
                        exBlock.Offset = aClause.TryOffset;
                        exBlock.Length = aClause.TryLength;
                        result.ExceptionHandledBlocks.Add(exBlock);
                    }

                    switch (aClause.Flags)
                    {
                        case ExceptionHandlingClauseOptions.Fault:
                        case ExceptionHandlingClauseOptions.Clause:
                            {
                                CatchBlock catchBlock = new CatchBlock()
                                {
                                    Offset = aClause.HandlerOffset,
                                    Length = aClause.HandlerLength,
                                    //Though not used, we may as well set it anyway
                                    FilterType = aClause.CatchType
                                };
                                exBlock.CatchBlocks.Add(catchBlock);
                            }
                            break;
                        case ExceptionHandlingClauseOptions.Finally:
                            {
                                FinallyBlock finallyBlock = new FinallyBlock()
                                {
                                    Offset = aClause.HandlerOffset,
                                    Length = aClause.HandlerLength
                                };
                                exBlock.FinallyBlocks.Add(finallyBlock);
                            }
                            break;
                        default:
                            Logger.LogError("IL0010", "", 0,
                                "Exception handling clause not supported! Type: " + aClause.Flags.ToString());
                            break;
                    }
                }
            }

            return result;
        }
    }
}
