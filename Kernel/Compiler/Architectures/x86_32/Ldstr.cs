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

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Ldstr : ILOps.Ldstr
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Load a string literal (- fixed string i.e. one programmed as "a string in code")

            //Get the string metadata token used to get the string from the assembly
            int StringMetadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
            //Get the value of the string to load
            string theString = aScannerState.CurrentILChunk.Method.Module.ResolveString(StringMetadataToken);
            //Add the string literal and get its ID
            string theStringID = aScannerState.AddStringLiteral(theString, anILOpInfo);

            //Push the address of the string (i.e. address of ID - ASM label)
            result.AppendLine(string.Format("push {0}", theStringID));

            aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                sizeOnStackInBytes = 4,
                isFloat = false,
                isGCManaged = true
            });

            return result.ToString().Trim();
        }
    }
}
