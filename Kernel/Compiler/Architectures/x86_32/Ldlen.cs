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
    public class Ldlen : ILOps.Ldlen
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

            aScannerState.CurrentStackFrame.Stack.Pop();

            DB_Type arrayDBType = DebugDatabase.GetType(aScannerState.GetTypeID(aScannerState.ArrayConstructorMethod.DeclaringType));
            int lengthOffset = 0;
            #region Offset calculation
            {
                //Get the child links of the type (i.e. the fields of the type)
                List<DB_ComplexTypeLink> allChildLinks = arrayDBType.ChildTypes.OrderBy(x => x.ParentIndex).ToList();
                //Get the DB type information for the field we want to load
                DB_ComplexTypeLink theTypeLink = (from links in arrayDBType.ChildTypes
                                                  where links.FieldId == "length"
                                                  select links).First();
                //Get all the fields that come before the field we want to load
                //This is so we can calculate the offset (in memory, in bytes) from the start of the object
                allChildLinks = allChildLinks.Where(x => x.ParentIndex < theTypeLink.ParentIndex).ToList();
                //Calculate the offset
                //We use StackBytesSize since fields that are reference types are only stored as a pointer
                lengthOffset = allChildLinks.Sum(x => x.ChildType.StackBytesSize);
            }
            #endregion

            // 1. Check array reference is not null
            //      1.1. Move array ref into eax
            //      1.2. Compare eax (array ref) to 0
            //      1.3. If not zero, jump to continue execution further down
            //      1.4. Otherwise, call Exceptions.ThrowNullReferenceException
            // 2. Load array length

            string ContinueExecutionLabelBase = string.Format("{0}.IL_{1}_ContinueExecution",
                    aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                    anILOpInfo.Position);
            string ContinueExecutionLabel1 = ContinueExecutionLabelBase + "1";
            //      1.1. Move array ref into eax
            result.AppendLine("mov eax, [esp]");
            //      1.2. Compare eax (array ref) to 0
            result.AppendLine("cmp eax, 0");
            //      1.3. If not zero, jump to continue execution further down
            result.AppendLine("jnz " + ContinueExecutionLabel1);
            //      1.4. Otherwise, call Exceptions.ThrowNullReferenceException
            result.AppendLine(string.Format("call {0}", aScannerState.GetMethodID(aScannerState.ThrowNullReferenceExceptionMethod)));
            result.AppendLine(ContinueExecutionLabel1 + ":");

            //2. Load array length
            //  - Pop array ref
            result.AppendLine("pop dword ecx");
            //  - Load length from array ref
            result.AppendLine(string.Format("mov eax, [ecx+{0}]", lengthOffset));
            //  - Push array length
            result.AppendLine("push dword eax");

            aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = false,
                sizeOnStackInBytes = 4
            });

            return result.ToString().Trim();
        }
    }
}
