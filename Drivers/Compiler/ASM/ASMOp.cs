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

namespace Drivers.Compiler.ASM
{
    public enum OpCodes
    {
        Any,
        Comment,
        ExternalLabel,
        GlobalLabel,
        Label,
        Header,
        StringLiteral,
        TypeTable,
        MethodTable,
        FieldTable,
        StaticField
    }

    /// <summary>
    ///     Represents an ASM op which will be translated into assembly language (i.e. assembly text).
    /// </summary>
    /// <remarks>
    ///     Specific ASM op implementations are found in a target architecture library.
    /// </remarks>
    public abstract class ASMOp
    {
        /// <summary>
        ///     The IL Position of the IL op which generated this ASM op. Default: -1 i.e. no position.
        /// </summary>
        /// <remarks>
        ///     Not all ASM ops will originate from an IL op. Also, a position of -1 is invalid so ASM ops
        ///     with IL Position -1 will not have a related label.
        /// </remarks>
        internal int ILLabelPosition = -1;

        /// <summary>
        ///     Whether the ASM op needs to be preceded by a label for the IL op to which it relates. This
        ///     is used, for example, when an IL branch instruction causes jump ASM operations to be
        ///     inserted. The jump ASM ops set RequiresILLabel on the first ASM op of the target IL op as
        ///     the reference point to jump to.
        /// </summary>
        internal bool RequiresILLabel = false;

        /// <summary>
        ///     Converts the ASM op into assembly code.
        /// </summary>
        /// <param name="TheBlock">The ASM block to which the ASM op belongs.</param>
        /// <returns>The assembly code text.</returns>
        public abstract string Convert(ASMBlock TheBlock);
    }
}