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
using System.Reflection;
using System.Reflection.Emit;
using Drivers.Compiler.Types;
using FieldInfo = System.Reflection.FieldInfo;
using MethodInfo = Drivers.Compiler.Types.MethodInfo;

namespace Drivers.Compiler.IL
{
    /// <summary>
    ///     The IL Reader manages reading in both plugged and non-plugged methods from an IL library.
    /// </summary>
    public static class ILReader
    {
        /// <summary>
        ///     List of all the possible IL op codes (whether supported or not).
        /// </summary>
        public static OpCode[] AllOpCodes = new OpCode[ushort.MaxValue];

        /// <summary>
        ///     Initialises the IL reader.
        /// </summary>
        /// <remarks>
        ///     Calls <see cref="LoadILOpTypes" />.
        /// </remarks>
        static ILReader()
        {
            LoadILOpTypes();
        }

        /// <summary>
        ///     Loads all the possible IL ops (whether supported by the Drivers Compiler or not).
        /// </summary>
        private static void LoadILOpTypes()
        {
            //Get the list of ILOps from the fields in OpCodes (all the fields in OpCodes are the ILOps)
            foreach (
                FieldInfo aField in
                    typeof(OpCodes).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public))
            {
                //Get an instance of the op code
                var anOpCode = (OpCode)aField.GetValue(null);
                //Get the op code's identifying value and use it as the index in our array
                ushort index = (ushort)anOpCode.Value;
                //Set the op code in our array
                AllOpCodes[index] = anOpCode;
            }
        }

        /// <summary>
        ///     Reads the specified method.
        /// </summary>
        /// <param name="aMethodInfo">The method to read.</param>
        /// <returns>The new IL block for the method.</returns>
        public static ILBlock Read(MethodInfo aMethodInfo)
        {
            if (aMethodInfo.IsPlugged)
            {
                return ReadPlugged(aMethodInfo);
            }
            return ReadNonPlugged(aMethodInfo);
        }

        /// <summary>
        ///     Reads a plugged method.
        /// </summary>
        /// <param name="aMethodInfo">The method to read.</param>
        /// <returns>The new IL block for the method.</returns>
        public static ILBlock ReadPlugged(MethodInfo aMethodInfo)
        {
            string PlugPath = aMethodInfo.PlugAttribute.ASMFilePath;
            return new ILBlock
            {
                PlugPath = string.IsNullOrWhiteSpace(PlugPath) ? " " : PlugPath,
                TheMethodInfo = aMethodInfo
            };
        }

        /// <summary>
        ///     Reads a non-plugged method.
        /// </summary>
        /// <param name="aMethodInfo">The method to read.</param>
        /// <returns>The new IL block for the method.</returns>
        public static ILBlock ReadNonPlugged(MethodInfo aMethodInfo)
        {
            ILBlock result = new ILBlock
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
                    aMethodInfo.LocalInfos.Add(new VariableInfo
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
                        currOpCodeID = (ushort)(0xFE00 + ILBytes[ILBytesPos + 1]);
                        currOpBytesSize += 2;
                    }
                    else
                    {
                        currOpCodeID = ILBytes[ILBytesPos];
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
                            operandSize = (int)(count*4);
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

                    result.ILOps.Add(new ILOp
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
                            CatchBlock catchBlock = new CatchBlock
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
                            FinallyBlock finallyBlock = new FinallyBlock
                            {
                                Offset = aClause.HandlerOffset,
                                Length = aClause.HandlerLength
                            };
                            exBlock.FinallyBlocks.Add(finallyBlock);
                        }
                            break;
                        default:
                            Logger.LogError("IL0010", "", 0,
                                "Exception handling clause not supported! Type: " + aClause.Flags);
                            break;
                    }
                }
            }

            return result;
        }
    }
}