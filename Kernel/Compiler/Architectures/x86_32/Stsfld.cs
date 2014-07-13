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
    public class Stsfld : ILOps.Stsfld
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if the value to store is floating point.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            int metadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
            FieldInfo theField = aScannerState.CurrentILChunk.Method.Module.ResolveField(metadataToken);
            string fieldID = aScannerState.GetStaticFieldID(theField);

            int size = Utils.GetNumBytesForType(theField.FieldType);
            bool isFloat = Utils.IsFloat(theField.FieldType);

            StackItem value = aScannerState.CurrentStackFrame.Stack.Pop();
            
            if (isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Storing static fields of type float not supported yet!");
            }

            if (size == 1)
            {
                result.AppendLine("pop dword eax");
                result.AppendLine(string.Format("mov byte [{0}], al", fieldID));
            }
            else if (size == 2)
            {
                result.AppendLine("pop dword eax");
                result.AppendLine(string.Format("mov word [{0}], ax", fieldID));
            }
            else if (size == 4)
            {
                result.AppendLine("pop dword eax");
                result.AppendLine(string.Format("mov dword [{0}], eax", fieldID));
            }
            else if (size == 8)
            {
                result.AppendLine("pop dword eax");
                result.AppendLine(string.Format("mov byte [{0}], eax", fieldID));
                result.AppendLine("pop dword eax");
                result.AppendLine(string.Format("mov byte [{0}+4], eax", fieldID));
            }

            return result.ToString().Trim();
        }
    }
}
