#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Starg : ILOps.Starg
    {
        /// <summary>
        /// See base class documentation.
        /// <para>To Do's:</para>
        /// <list type="bullet">
        /// <item>
        /// <term>To do</term>
        /// <description>Implement storing of float arguments.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotImplementedException">
        /// Thrown when storing a float argument is required as it currently hasn't been
        /// implemented.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when an invalid number of bytes is specified for the argument to store.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Get the index of the argument to load
            Int16 index = 0;
            switch ((OpCodes)anILOpInfo.opCode.Value)
            {
                case OpCodes.Starg:
                    index = Utils.ReadInt16(anILOpInfo.ValueBytes, 0);
                    break;
                case OpCodes.Starg_S:
                    index = (Int16)anILOpInfo.ValueBytes[0];
                    break;
            }

            //Used to store the number of bytes to subtract from EBP to get to the arg
            int BytesOffsetFromEBP = 0;
            //Get all the params for the current method
            List<Type> allParams = aScannerState.CurrentILChunk.Method.GetParameters().Select(x => x.ParameterType).ToList();
            if (!aScannerState.CurrentILChunk.Method.IsStatic)
            {
                allParams.Insert(0, aScannerState.CurrentILChunk.Method.DeclaringType);
            }
            //Check whether the arg we are going to load is float or not
            if (Utils.IsFloat(allParams[index]))
            {
                //SUPPORT - floats
                throw new NotSupportedException("Float arguments not supported yet!");
            }
            //For all the parameters pushed on the stack after the param we want
            for (int i = allParams.Count - 1; i > -1 && i > index; i--)
            {
                //Add the param stack size to the EBP offset
                BytesOffsetFromEBP += Utils.GetNumBytesForType(allParams[i]);
            }

            //Add 8 for return address and value of EBP (both pushed at start of method call)
            BytesOffsetFromEBP += 8;

            //We must check the return value to see if it has a size on the stack
            //Get the return type
            Type retType = (aScannerState.CurrentILChunk.Method.IsConstructor || aScannerState.CurrentILChunk.Method is ConstructorInfo ?
                    typeof(void) : ((MethodInfo)aScannerState.CurrentILChunk.Method).ReturnType);
            //Get the size of the return type
            int retSize = Utils.GetNumBytesForType(retType);
            //Add it to EBP offset
            BytesOffsetFromEBP += retSize;
        
            //Pop the argument value from the stack
            int bytesForArg = Utils.GetNumBytesForType(allParams[index]);
            if(bytesForArg == 4)
            {
                result.AppendLine(string.Format("pop dword [ebp+{0}]", BytesOffsetFromEBP));
            }
            else if (bytesForArg == 8)
            {
                result.AppendLine(string.Format("pop dword [ebp+{0}]", BytesOffsetFromEBP));
                result.AppendLine(string.Format("pop dword [ebp+{0}]", BytesOffsetFromEBP + 4));
            }
            else
            {
                throw new ArgumentException("Cannot store arg! Don't understand byte size of the arg!");
            }

            //Pop the arg value from our stack
            aScannerState.CurrentStackFrame.Stack.Pop();

            return result.ToString().Trim();
        }
    }
}
