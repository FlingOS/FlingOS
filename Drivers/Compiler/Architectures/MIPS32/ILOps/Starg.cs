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
using System.Reflection;
using Drivers.Compiler.Architectures.MIPS32.ASMOps;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
    /// <summary>
    ///     See base class documentation.
    /// </summary>
    public class Starg : IL.ILOps.Starg
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
        }

        /// <summary>
        ///     See base class documentation.
        ///     <para>To Do's:</para>
        ///     <list type="bullet">
        ///         <item>
        ///             <term>To do</term>
        ///             <description>Implement storing of float arguments.</description>
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotImplementedException">
        ///     Thrown when storing a float argument is required as it currently hasn't been
        ///     implemented.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///     Thrown when an invalid number of bytes is specified for the argument to store.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Get the index of the argument to load
            short index = 0;
            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Starg:
                    index = Utilities.ReadInt16(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Starg_S:
                    index = theOp.ValueBytes[0];
                    break;
            }

            //Used to store the number of bytes to subtract from $fp to get to the arg
            int BytesOffsetFromFp = 0;
            //Get all the params for the current method
            List<Type> allParams =
                conversionState.Input.TheMethodInfo.UnderlyingInfo.GetParameters().Select(x => x.ParameterType).ToList();
            if (!conversionState.Input.TheMethodInfo.IsStatic)
            {
                allParams.Insert(0, conversionState.Input.TheMethodInfo.UnderlyingInfo.DeclaringType);
            }
            //Check whether the arg we are going to load is float or not
            if (Utilities.IsFloat(allParams[index]))
            {
                //SUPPORT - floats
                throw new NotSupportedException("Float arguments not supported yet!");
            }
            //For all the parameters pushed on the stack after the param we want
            for (int i = allParams.Count - 1; i > -1 && i > index; i--)
            {
                //Add the param stack size to the $fp offset
                BytesOffsetFromFp += conversionState.TheILLibrary.GetTypeInfo(allParams[i]).SizeOnStackInBytes;
            }

            //Add 8 for return address and value of $fp (both pushed at start of method call)
            BytesOffsetFromFp += 8;

            //We must check the return value to see if it has a size on the stack
            //Get the return type
            Type retType = conversionState.Input.TheMethodInfo.IsConstructor
                ? typeof(void)
                : ((MethodInfo)conversionState.Input.TheMethodInfo.UnderlyingInfo).ReturnType;
            //Get the size of the return type
            int retSize = conversionState.TheILLibrary.GetTypeInfo(retType).SizeOnStackInBytes;
            //Add it to $fp offset
            BytesOffsetFromFp += retSize;

            //Pop the argument value from the stack
            int bytesForArg = conversionState.TheILLibrary.GetTypeInfo(allParams[index]).SizeOnStackInBytes;
            if (bytesForArg == 4)
            {
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "$t0",
                    Dest = BytesOffsetFromFp + "($fp)",
                    MoveType = Mov.MoveTypes.SrcRegToDestMemory
                });
            }
            else if (bytesForArg == 8)
            {
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "$t0",
                    Dest = BytesOffsetFromFp + "($fp)",
                    MoveType = Mov.MoveTypes.SrcRegToDestMemory
                });
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "$t0",
                    Dest = BytesOffsetFromFp + 4 + "($fp)",
                    MoveType = Mov.MoveTypes.SrcRegToDestMemory
                });
            }
            else
            {
                throw new ArgumentException("Cannot store arg! Don't understand byte size of the arg!");
            }

            //Pop the arg value from our stack
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
        }
    }
}