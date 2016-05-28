#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
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

using Drivers.Compiler.Architectures.x86.ASMOps;
using Drivers.Compiler.IL;
using Drivers.Compiler.Types;

namespace Drivers.Compiler.Architectures.x86
{
    /// <summary>
    ///     See base class documentation.
    /// </summary>
    public class MethodStart : IL.ILOps.MethodStart
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            base.PerformStackOperations(conversionState, theOp);
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Push the previous method's ebp
            conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EBP"});
            //Set ebp for this method
            //See calling convention spec - this allows easy access of
            //args and locals within the method without having to track
            //temporary values (which would be a nightmare with the
            //exception handling implementation that the kernel uses!)
            conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "ESP", Dest = "EBP"});

            //Allocate stack space for locals
            //Only bother if there are any locals
            if (conversionState.Input.TheMethodInfo.LocalInfos.Count > 0)
            {
                int totalBytes = 0;
                foreach (VariableInfo aLocal in conversionState.Input.TheMethodInfo.LocalInfos)
                {
                    totalBytes += aLocal.TheTypeInfo.SizeOnStackInBytes;
                }
                //We do not use "sub esp, X" (see below) because that leaves
                //junk memory - we need memory to be "initialised" to 0 
                //so that local variables are null unless properly initialised.
                //This prevents errors in the GC.
                for (int i = 0; i < totalBytes/4; i++)
                {
                    conversionState.Append(new Push {Size = OperandSize.Dword, Src = "0"});
                }
                //result.AppendLine(string.Format("sub esp, {0}", totalBytes));
            }
        }
    }
}