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
using Kernel.Debug.Data;

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Ldfld : ILOps.Ldfld
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if field to load is a floating value or the field to load
        /// is not of size 4 or 8 bytes.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Get the field's token that is used to get FieldInfo from the assembly
            int metadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
            //Get the field info from the referencing assembly
            FieldInfo theField = aScannerState.CurrentILChunk.Method.Module.ResolveField(metadataToken);
            //Get the database type information about the object that contains the field
            DB_Type objDBType = DebugDatabase.GetType(aScannerState.GetTypeID(theField.DeclaringType));
            
            //Get the child links of the type (i.e. the fields of the type)
            List<DB_ComplexTypeLink> allChildLinks = objDBType.ChildTypes.OrderBy(x => x.ParentIndex).ToList();
            //Get the DB type information for the field we want to load
            DB_ComplexTypeLink theTypeLink = (from links in objDBType.ChildTypes
                                              where links.FieldId == theField.Name
                                              select links).First();
            //Get all the fields that come before the field we want to load
            //This is so we can calculate the offset (in memory, in bytes) from the start of the object
            allChildLinks = allChildLinks.Where(x => x.ParentIndex < theTypeLink.ParentIndex).ToList();
            //Calculate the offset
            //We use StackBytesSize since fields that are reference types are only stored as a pointer
            int offset = allChildLinks.Sum(x => x.ChildType.StackBytesSize);

            //Is the value to load a floating pointer number?
            bool valueisFloat = Utils.IsFloat(theField.FieldType);
            //Get the size of the value to load (in bytes, as it will appear on the stack)
            int valuesizeOnStackInBytes = Utils.GetNumBytesForType(theField.FieldType);

            //Pop the object pointer from our stack
            StackItem objPointer = aScannerState.CurrentStackFrame.Stack.Pop();
            
            //If the value to load is a float, erm...abort...
            if (valueisFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Loading fields of type float not supported yet!");
            }
            //Otherwise, if the value to load is a weird size, abort
            else if (!(valuesizeOnStackInBytes == 8 || valuesizeOnStackInBytes == 4))
            {
                throw new NotSupportedException("Loading fields of supplied size not supported yet!");
            }

            //Pop object pointer
            result.AppendLine("pop dword ecx");
            if ((OpCodes)anILOpInfo.opCode.Value == OpCodes.Ldflda)
            {
                result.AppendLine(string.Format("add ecx, {0}", offset));
                result.AppendLine("push dword ecx");

                aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = valueisFloat,
                    sizeOnStackInBytes = 4
                });
            }
            else
            {
                //Push value at pointer+offset
                result.AppendLine(string.Format("push dword [ecx+{0}]", offset));
                if (valuesizeOnStackInBytes == 8)
                {
                    result.AppendLine(string.Format("push dword [ecx+{0}]", offset + 4));
                }

                aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = valueisFloat,
                    sizeOnStackInBytes = valuesizeOnStackInBytes
                });
            }

            return result.ToString().Trim();
        }
    }
}
