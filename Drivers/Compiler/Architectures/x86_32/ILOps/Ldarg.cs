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
using Drivers.Compiler.Types;

namespace Drivers.Compiler.Architectures.x86
{
    /// <summary>
    ///     See base class documentation.
    /// </summary>
    public class Ldarg : IL.ILOps.Ldarg
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            short index = 0;
            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Ldarg:
                    index = Utilities.ReadInt16(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Ldarg_0:
                    index = 0;
                    break;
                case OpCodes.Ldarg_1:
                    index = 1;
                    break;
                case OpCodes.Ldarg_2:
                    index = 2;
                    break;
                case OpCodes.Ldarg_3:
                    index = 3;
                    break;
                case OpCodes.Ldarg_S:
                    index = theOp.ValueBytes[0];
                    break;
                case OpCodes.Ldarga:
                    index = Utilities.ReadInt16(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Ldarga_S:
                    index = theOp.ValueBytes[0];
                    break;
            }

            if ((OpCodes)theOp.opCode.Value == OpCodes.Ldarga ||
                (OpCodes)theOp.opCode.Value == OpCodes.Ldarga_S)
            {
                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    sizeOnStackInBytes = 4,
                    isFloat = false,
                    isGCManaged = false,
                    isValue = false
                });
            }
            else
            {
                VariableInfo argInfo = conversionState.Input.TheMethodInfo.ArgumentInfos[index];
                TypeInfo paramTypeInfo = argInfo.TheTypeInfo;
                int bytesForArg = paramTypeInfo.SizeOnStackInBytes;
                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    sizeOnStackInBytes = bytesForArg,
                    isFloat = false,
                    isGCManaged = paramTypeInfo.IsGCManaged,
                    isValue = paramTypeInfo.IsValueType
                });
            }
        }

        /// <summary>
        ///     See base class documentation.
        ///     <para>To Do's:</para>
        ///     <list type="bullet">
        ///         <item>
        ///             <term>To do</term>
        ///             <description>Implement loading of float arguments.</description>
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     Thrown when loading a float argument is required as it currently hasn't been
        ///     implemented.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///     Thrown when an invalid number of bytes is specified for the argument to load.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Get the index of the argument to load
            short index = 0;
            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Ldarg:
                    index = Utilities.ReadInt16(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Ldarg_0:
                    index = 0;
                    break;
                case OpCodes.Ldarg_1:
                    index = 1;
                    break;
                case OpCodes.Ldarg_2:
                    index = 2;
                    break;
                case OpCodes.Ldarg_3:
                    index = 3;
                    break;
                case OpCodes.Ldarg_S:
                    index = theOp.ValueBytes[0];
                    break;
                case OpCodes.Ldarga:
                    index = Utilities.ReadInt16(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Ldarga_S:
                    index = theOp.ValueBytes[0];
                    break;
            }

            VariableInfo argInfo = conversionState.Input.TheMethodInfo.ArgumentInfos[index];
            if (Utilities.IsFloat(argInfo.TheTypeInfo.UnderlyingType))
            {
                //SUPPORT - floats
                throw new NotSupportedException("Float arguments not supported yet!");
            }

            //Used to store the number of bytes to add to EBP to get to the arg
            int BytesOffsetFromEBP = argInfo.Offset;

            if ((OpCodes)theOp.opCode.Value == OpCodes.Ldarga ||
                (OpCodes)theOp.opCode.Value == OpCodes.Ldarga_S)
            {
                //Push the address of the argument onto the stack

                conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "EBP", Dest = "ECX"});
                conversionState.Append(new ASMOps.Add {Src = BytesOffsetFromEBP.ToString(), Dest = "ECX"});
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "ECX"});

                //Push the address onto our stack
                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    sizeOnStackInBytes = 4,
                    isFloat = false,
                    isGCManaged = false,
                    isValue = false
                });
            }
            else
            {
                //Push the argument onto the stack
                TypeInfo paramTypeInfo = argInfo.TheTypeInfo;
                int bytesForArg = paramTypeInfo.SizeOnStackInBytes;

                if (bytesForArg%4 != 0)
                {
                    throw new ArgumentException("Cannot load arg! Don't understand byte size of the arg! Size:" +
                                                bytesForArg);
                }

                while (bytesForArg > 0)
                {
                    bytesForArg -= 4;

                    conversionState.Append(new Push
                    {
                        Size = OperandSize.Dword,
                        Src = "[EBP+" + (BytesOffsetFromEBP + bytesForArg) + "]"
                    });
                }

                //Push the arg onto our stack
                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    sizeOnStackInBytes = paramTypeInfo.SizeOnStackInBytes,
                    isFloat = false,
                    isGCManaged = paramTypeInfo.IsGCManaged,
                    isValue = paramTypeInfo.IsValueType
                });
            }
        }
    }
}