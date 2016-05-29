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
    public class Starg : IL.ILOps.Starg
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
        }

        /// <summary>
        ///     See base class documentation.
        ///     <para>To Do's:</para>
        ///     <list type="bullet">
        ///         <item>
        ///             <term>To do</term>
        ///             <description>Implement storing of float arguments.</description>
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotImplementedException">
        ///     Thrown when storing a float argument is required as it currently hasn't been
        ///     implemented.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///     Thrown when an invalid number of bytes is specified for the argument to store.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Get the index of the argument to load
            short index = 0;
            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Starg:
                    index = Utilities.ReadInt16(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Starg_S:
                    index = theOp.ValueBytes[0];
                    break;
            }

            VariableInfo argInfo = conversionState.Input.TheMethodInfo.ArgumentInfos[index];
            //Used to store the number of bytes to add to EBP to get to the arg
            int BytesOffsetFromEBP = argInfo.Offset;

            //Pop the argument value from the stack
            int bytesForArg = argInfo.TheTypeInfo.SizeOnStackInBytes;
            for (int i = 0; i < bytesForArg; i += 4)
            {
                conversionState.Append(new ASMOps.Pop
                {
                    Size = OperandSize.Dword,
                    Dest = "[EBP+" + (BytesOffsetFromEBP + i) + "]"
                });
            }

            //Pop the arg value from our stack
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
        }
    }
}