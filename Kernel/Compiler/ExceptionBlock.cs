using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler
{
    /// <summary>
    /// Describes a block of IL code where exceptions are handled.
    /// This block sets the start offset and length of the try-block
    /// and lists the catch and finally blocks for the try block.
    /// </summary>
    public class ExceptionHandledBlock
    {
        /// <summary>
        /// The offset (in IL bytes) from the start of the method.
        /// </summary>
        public int Offset;
        /// <summary>
        /// The length (in IL bytes) of the try-block.
        /// </summary>
        public int Length;

        /// <summary>
        /// The catch-blocks for the try-block.
        /// </summary>
        public List<CatchBlock> CatchBlocks = new List<CatchBlock>();
        /// <summary>
        /// The finally-blocks for the try-block.
        /// </summary>
        public List<FinallyBlock> FinallyBlocks = new List<FinallyBlock>();
    }
    /// <summary>
    /// Describes an IL catch block for a try-block.
    /// </summary>
    public class CatchBlock
    {
        /// <summary>
        /// The offset (in IL bytes) from the start of the method to 
        /// the first IL op of the catch handler.
        /// </summary>
        public int Offset;
        /// <summary>
        /// The length (in IL bytes) of the catch handler.
        /// </summary>
        public int Length;

        /// <summary>
        /// The type of exception to catch - this is not actually implemented
        /// / used yet!
        /// </summary>
        public Type FilterType;
    }
    /// <summary>
    /// Describes an IL finally block for a try-block.
    /// </summary>
    public class FinallyBlock
    {
        /// <summary>
        /// The offset (in IL bytes) from the start of the method to 
        /// the first IL op of the finally handler.
        /// </summary>
        public int Offset;
        /// <summary>
        /// The length (in IL bytes) of the finally handler.
        /// </summary>
        public int Length;
    }
}
