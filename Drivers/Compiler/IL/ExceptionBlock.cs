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
using System.Collections.Generic;

namespace Drivers.Compiler.IL
{
    /// <summary>
    ///     Describes a block of IL code where exceptions are handled.
    ///     This block sets the start offset and length of the try-block
    ///     and lists the catch and finally blocks for the try block.
    /// </summary>
    public class ExceptionHandledBlock
    {
        /// <summary>
        ///     The catch-blocks for the try-block.
        /// </summary>
        public List<CatchBlock> CatchBlocks = new List<CatchBlock>();

        /// <summary>
        ///     The finally-blocks for the try-block.
        /// </summary>
        public List<FinallyBlock> FinallyBlocks = new List<FinallyBlock>();

        /// <summary>
        ///     The length (in IL bytes) of the try-block.
        /// </summary>
        public int Length;

        /// <summary>
        ///     The offset (in IL bytes) from the start of the method.
        /// </summary>
        public int Offset;
    }

    /// <summary>
    ///     Describes an IL catch block for a try-block.
    /// </summary>
    public class CatchBlock
    {
        /// <summary>
        ///     The type of exception to catch - this is not actually implemented
        ///     / used yet!
        /// </summary>
        public Type FilterType;

        /// <summary>
        ///     The length (in IL bytes) of the catch handler.
        /// </summary>
        public int Length;

        /// <summary>
        ///     The offset (in IL bytes) from the start of the method to
        ///     the first IL op of the catch handler.
        /// </summary>
        public int Offset;
    }

    /// <summary>
    ///     Describes an IL finally block for a try-block.
    /// </summary>
    public class FinallyBlock
    {
        /// <summary>
        ///     The length (in IL bytes) of the finally handler.
        /// </summary>
        public int Length;

        /// <summary>
        ///     The offset (in IL bytes) from the start of the method to
        ///     the first IL op of the finally handler.
        /// </summary>
        public int Offset;
    }
}