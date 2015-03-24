#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Ldobj : ILOps.Ldobj
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

            //Load the metadata token used to get the type info
            int metadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
            //Get the type info for the object to load
            Type theType = aScannerState.CurrentILChunk.Method.Module.ResolveType(metadataToken);

            //Get the object size information
            int size = Utils.GetNumBytesForType(theType);

            //Load the object onto the stack
            result.AppendLine("pop dword ecx");
            for (int i = size - 4; i >= 0; i -= 4)
            {
                GlobalMethods.InsertPageFaultDetection(result, aScannerState, "ecx", i, (OpCodes)anILOpInfo.opCode.Value);
                result.AppendLine(string.Format("mov dword eax, [ecx+{0}]", i));
                result.AppendLine("push dword eax");
            }
            int extra = size % 4;
            for (int i = extra - 1; i >= 0; i--)
            {
                GlobalMethods.InsertPageFaultDetection(result, aScannerState, "ecx", i, (OpCodes)anILOpInfo.opCode.Value);
                result.AppendLine(string.Format("mov byte al, [ecx+{0}]", i));
                result.AppendLine("push byte al");
            }

            aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = false,
                sizeOnStackInBytes = size,
                isGCManaged = false
            });

            return result.ToString().Trim();
        }
    }
}
