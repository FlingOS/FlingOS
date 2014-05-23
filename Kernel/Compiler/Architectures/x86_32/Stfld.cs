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
            
            List<DB_ComplexTypeLink> allChildLinks = objDBType.ChildTypes.OrderBy(x => x.ParentIndex).ToList();
            DB_ComplexTypeLink theTypeLink = (from links in objDBType.ChildTypes
                                              where links.FieldId == theField.Name
                                              select links).First();
            allChildLinks = allChildLinks.Where(x => x.ParentIndex < theTypeLink.ParentIndex).ToList();
            int offset = allChildLinks.Sum(x => x.ChildType.StackBytesSize);

            int size = Utils.GetNumBytesForType(theField.FieldType);
            
            StackItem value = aScannerState.CurrentStackFrame.Stack.Pop();
            StackItem objPointer = aScannerState.CurrentStackFrame.Stack.Pop();
            
            if (value.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Storing fields of type float not supported yet!");
            }
            else if (!(value.sizeOnStackInBytes == 8 || value.sizeOnStackInBytes == 4))
            {
                throw new NotSupportedException("Storing fields of supplied size not supported yet!");
            }

            //Pop value
            if(value.sizeOnStackInBytes == 4)
            {
                result.AppendLine("pop dword eax");
            }
            else if(value.sizeOnStackInBytes == 8)
            {
                result.AppendLine("pop dword edx");
                result.AppendLine("pop dword eax");
            }
            //Pop object pointer
            result.AppendLine("pop dword ecx");
            //Move value into pointer+offset
            result.AppendLine(string.Format("mov dword [ecx+{0}], eax", offset));
            if(value.sizeOnStackInBytes == 8)
            {
                result.AppendLine(string.Format("mov dword [ecx+{0}], edx", offset + 4));
            }

            return result.ToString().Trim();
        }
    }
}
