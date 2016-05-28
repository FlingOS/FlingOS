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

using System;
using Drivers.Compiler.Types;

namespace Drivers.Compiler.IL
{
    /// <summary>
    ///     Represents the current state of the IL Preprocessor during compilation of a single, non-plugged IL block.
    /// </summary>
    /// <remarks>
    ///     This is essentially a stripped-down version of the ILConversionState.
    /// </remarks>
    public class ILPreprocessState
    {
        /// <summary>
        ///     The output ASM block being produced from the Input IL block.
        /// </summary>
        public StackFrame CurrentStackFrame = new StackFrame();

        /// <summary>
        ///     The IL block being compiled.
        /// </summary>
        public ILBlock Input;

        /// <summary>
        ///     The IL library being compiled.
        /// </summary>
        public ILLibrary TheILLibrary;

        /// <summary>
        ///     Gets the position (index) of the specified IL op.
        /// </summary>
        /// <param name="anOp">The op to get the position of.</param>
        /// <returns>The position.</returns>
        public int PositionOf(ILOp anOp)
        {
            return Input.PositionOf(anOp);
        }

        /// <summary>
        ///     Gets the field info by name for the specified field of the specified type.
        /// </summary>
        /// <param name="aType">The type to which the field belongs.</param>
        /// <param name="FieldName">The name of the field to get.</param>
        /// <returns>The field information.</returns>
        public FieldInfo GetFieldInfo(Type aType, string FieldName)
        {
            TypeInfo aTypeInfo = TheILLibrary.GetTypeInfo(aType);
            return TheILLibrary.GetFieldInfo(aTypeInfo, FieldName);
        }
    }
}