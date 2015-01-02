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
    public class MethodStart : ILOps.MethodStart
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

            //Push the previous method's ebp
            result.AppendLine("push dword ebp");
            //Set ebp for this method
            //See calling convention spec - this allows easy access of
            //args and locals within the method without having to track
            //temporary values (which would be a nightmare with the
            //exception handling implmentation that the kernel uses!)
            result.AppendLine("mov dword ebp, esp");

            //Allocate stack space for locals
            //Only bother if there are any locals
            if (aScannerState.CurrentILChunk.LocalVariables.Count > 0)
            {
                int totalBytes = 0;
                foreach (StackItem aLocal in aScannerState.CurrentILChunk.LocalVariables)
                {
                    totalBytes += aLocal.sizeOnStackInBytes;
                }
                //We do not use "sub esp, X" (see below) because that leaves
                //junk memory - we need memory to be "initialised" to 0 
                //so that local variables are null unless properly initialised.
                //This prevents errors in the GC.
                for (int i = 0; i < totalBytes / 4; i++)
                {
                    result.AppendLine("push dword 0");
                }
                //result.AppendLine(string.Format("sub esp, {0}", totalBytes));
            }

            return result.ToString().Trim();
        }
    }
}
