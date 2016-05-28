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

using Drivers.Compiler.IL;
using Drivers.Compiler.Types;

namespace Drivers.Compiler.Architectures.MIPS32
{
    /// <summary>
    ///     See base class documentation.
    /// </summary>
    public class Pop : IL.ILOps.Pop
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            StackItem theItem = conversionState.CurrentStackFrame.GetStack(theOp).Pop();

            if (theItem.isNewGCObject)
            {
                //Decrement ref count

                //Get the ID of method to call as it will be labeled in the output ASM.
                MethodInfo anInfo = conversionState.GetDecrementRefCountMethodInfo();
                string methodID = anInfo.ID;
                conversionState.AddExternalLabel(anInfo.ID);
                //Append the actual call
                conversionState.Append(new ASMOps.Call {Target = methodID});
            }

            conversionState.Append(new ASMOps.Add
            {
                Src1 = "$sp",
                Src2 = theItem.sizeOnStackInBytes.ToString(),
                Dest = "$sp"
            });
        }
    }
}