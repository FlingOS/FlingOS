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
    public class Ldsfld : ILOps.Ldsfld
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when loading a static float field.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Load static field

            //Load the metadata token used to get the field info
            int metadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
            //Get the field info for the field to load
            FieldInfo theField = aScannerState.CurrentILChunk.Method.Module.ResolveField(metadataToken);
            //Get the ID (i.e. ASM label) of the field to load
            string fieldID = aScannerState.GetStaticFieldID(theField);

            //Load the field or field address
            switch ((OpCodes)anILOpInfo.opCode.Value)
            {
                case OpCodes.Ldsfld:
                    {
                        int size = Utils.GetNumBytesForType(theField.FieldType);
                        bool isFloat = Utils.IsFloat(theField.FieldType);
                        
                        if (isFloat)
                        {
                            //SUPPORT - floats
                            throw new NotSupportedException("Loading static fields of type float not supported yet!");
                        }

                        if(size == 1)
                        {
                            result.AppendLine("xor eax, eax");
                            result.AppendLine(string.Format("mov byte al, [{0}]", fieldID));
                            result.AppendLine("push dword eax");
                        }
                        else if(size == 2)
                        {
                            result.AppendLine("xor eax, eax");
                            result.AppendLine(string.Format("mov word ax, [{0}]", fieldID));
                            result.AppendLine("push dword eax");
                        }
                        else if(size == 4)
                        {
                            result.AppendLine(string.Format("mov dword eax, [{0}]", fieldID));
                            result.AppendLine("push dword eax");
                        }
                        else if (size == 8)
                        {
                            result.AppendLine(string.Format("mov dword eax, [{0}+4]", fieldID));
                            result.AppendLine("push dword eax");
                            result.AppendLine(string.Format("mov dword eax, [{0}]", fieldID));
                            result.AppendLine("push dword eax");
                        }

                        aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                        {
                            isFloat = isFloat,
                            sizeOnStackInBytes = (size == 8 ? 8 : 4)
                        });
                    }
                    break;
                case OpCodes.Ldsflda:
                    //Load the address of the field i.e. address of the ASM label
                    result.AppendLine(string.Format("push dword {0}", fieldID));

                    aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 4
                    });
                    break;
            }

            return result.ToString().Trim();
        }
    }
}
