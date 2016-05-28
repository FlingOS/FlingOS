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

using System.Collections.Generic;
using System.Linq;
using Drivers.Compiler.Types;

namespace Drivers.Compiler.IL
{
    /// <summary>
    ///     Represents a block of IL ops.
    /// </summary>
    /// <remarks>
    ///     An IL block always originates from a C# method, even if the block is plugged.
    ///     An IL block is mutually equivalent to a C# method except that the ILOps list
    ///     may be empty if the method is plugged.
    /// </remarks>
    public class ILBlock
    {
        /// <summary>
        ///     Contains the information about all the exception handler blocks for this IL block.
        /// </summary>
        public List<ExceptionHandledBlock> ExceptionHandledBlocks = new List<ExceptionHandledBlock>();

        /// <summary>
        ///     The list of IL ops in the block.
        /// </summary>
        /// <remarks>
        ///     This should be ignored if the block is plugged.
        /// </remarks>
        public List<ILOp> ILOps = new List<ILOp>();

        /// <summary>
        ///     The path to the ASM plug file, if any.
        /// </summary>
        /// <remarks>
        ///     This should be null if the block is not plugged. An empty string is a valid
        ///     plug file path and simply causes the compiler to ignore to block.
        /// </remarks>
        public string PlugPath = null;

        /// <summary>
        ///     The method info from which the block originated.
        /// </summary>
        /// <remarks>
        ///     This is always set, even for plugged blocks. The compiler has no cases of
        ///     entirely compiler-generated IL blocks. It should remain this way.
        /// </remarks>
        public MethodInfo TheMethodInfo;

        /// <summary>
        ///     Whether the block is plugged or not.
        /// </summary>
        /// <remarks>
        ///     Just returns whether the PlugPath is null or not. Note: empty string means
        ///     the block is plugged but should be ignored! The block will thus produce
        ///     no output.
        /// </remarks>
        /// <value>Gets whether the plug path is not equal to null.</value>
        public bool Plugged
        {
            get { return PlugPath != null; }
        }

        /// <summary>
        ///     Gets the position (index) of the specified IL op.
        /// </summary>
        /// <param name="anOp">The op to get the position of.</param>
        /// <returns>The position.</returns>
        public int PositionOf(ILOp anOp)
        {
            return ILOps.IndexOf(anOp);
        }

        /// <summary>
        ///     Gets the IL op that has the specified IL offset. An IL offset is the offset, in bytes, from
        ///     the start of the method.
        /// </summary>
        /// <remarks>
        ///     Not all IL ops have an IL offset. Some IL ops are injected by the compiler so have no "offset"
        ///     as they were not part of the original byte code. The IL offset is used by IL code for reference
        ///     in branch instructions and exception blocks. IL offsets should be translated into IL Positions
        ///     <see cref="PositionOf" />. All IL ops have a position.
        /// </remarks>
        /// <param name="offset">The IL offset of the op to get.</param>
        /// <returns>The IL op or null if no IL op with the specified offset was found.</returns>
        public ILOp At(int offset)
        {
            List<ILOp> potOps = (from ops in ILOps
                where ops.Offset == offset
                select ops).ToList();
            if (potOps.Count > 0)
            {
                return potOps.OrderBy(x => ILOps.IndexOf(x)).First();
            }
            return null;
        }

        /// <summary>
        ///     Gets the ExceptionHandledBlock that starts exactly at the specified offset.
        /// </summary>
        /// <remarks>
        ///     To obtain the ExceptionHandledBlock that contains/covers a specified offset,
        ///     <see cref="GetExceptionHandledBlock" />.
        /// </remarks>
        /// <param name="Offset">The IL offset of the start of the block to get.</param>
        /// <returns>The block or null if no exact match was found.</returns>
        public ExceptionHandledBlock GetExactExceptionHandledBlock(int Offset)
        {
            List<ExceptionHandledBlock> potBlocks = (from blocks in ExceptionHandledBlocks
                where blocks.Offset == Offset
                select blocks)
                .ToList();
            if (potBlocks.Count > 0)
            {
                return potBlocks.First();
            }
            return null;
        }

        /// <summary>
        ///     Gets the ExceptionHandledBlock which contains/covers the specified Il offset.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         In the case of multiple blocks covering the same offset, the method returns the inner-most
        ///         (with regards to scope).
        ///     </para>
        ///     <para>
        ///         To obtain the ExceptionHandledBlock that starts exactly at a specified offset,
        ///         <see cref="GetExactExceptionHandledBlock" />.
        ///     </para>
        /// </remarks>
        /// <param name="Offset">The IL offset of the IL op that is covered by the ExceptionHandledBlock to be retrieved.</param>
        /// <returns>The block or null if the IL op is not covered by any ExceptionHandledBlock.</returns>
        public ExceptionHandledBlock GetExceptionHandledBlock(int Offset)
        {
            List<ExceptionHandledBlock> potBlocks = (from blocks in ExceptionHandledBlocks
                where (blocks.Offset <= Offset &&
                       blocks.Offset + blocks.Length >= Offset)
                      || (from catchBlocks in blocks.CatchBlocks
                          where catchBlocks.Offset <= Offset &&
                                catchBlocks.Offset + catchBlocks.Length >= Offset
                          select catchBlocks).Count() > 0
                      || (from finallyBlocks in blocks.FinallyBlocks
                          where finallyBlocks.Offset <= Offset &&
                                finallyBlocks.Offset + finallyBlocks.Length >= Offset
                          select finallyBlocks).Count() > 0
                select blocks).OrderByDescending(x => x.Offset)
                .ToList();
            if (potBlocks.Count > 0)
            {
                return potBlocks.First();
            }
            return null;
        }
    }
}