#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Kernel.Debug.Data;

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Stfld : ILOps.Stfld
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if the value to store is floating point or
        /// if the value is not 4 or 8 bytes in size.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            int metadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
            FieldInfo theField = aScannerState.CurrentILChunk.Method.Module.ResolveField(metadataToken);
            DB_Type objDBType = DebugDatabase.GetType(aScannerState.GetTypeID(theField.DeclaringType));

            int offset = aScannerState.GetFieldOffset(objDBType, theField.Name);

            int stackSize = Utils.GetNumBytesForType(theField.FieldType);
            int memSize = theField.FieldType.IsValueType ? Utils.GetSizeForType(theField.FieldType) : stackSize;

            StackItem value = aScannerState.CurrentStackFrame.Stack.Pop();
            StackItem objPointer = aScannerState.CurrentStackFrame.Stack.Pop();
            
            if (value.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Storing fields of type float not supported yet!");
            }

            //Get object pointer
            result.AppendLine(string.Format("mov ecx, [esp+{0}]", stackSize));
            //Pop and mov value
            for (int i = 0; i < memSize; i += 2)
            {
                if (memSize - i == 1)
                {
                    result.AppendLine("pop word ax");
                    result.AppendLine(string.Format("mov byte [ecx+{0}], al", offset + i));
                }
                else
                {
                    result.AppendLine("pop word ax");
                    result.AppendLine(string.Format("mov word [ecx+{0}], ax", offset + i));
                }
            }
            result.AppendLine(string.Format("add esp, {0}", ((stackSize - memSize) / 2) * 2)); //Rounds down
            result.AppendLine("add esp, 4");//Pop object pointer

            return result.ToString().Trim();
        }
    }
}
