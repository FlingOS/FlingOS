using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Kernel.Compiler
{
    /// <summary>
    /// Represents a chunk of IL code (e.g. a method) that will later be compiled to ASM.
    /// </summary>
    public class ILChunk
    {
        /// <summary>
        /// Whether the ILChunk is plugged or not.
        /// </summary>
        public bool Plugged
        {
            get;
            set;
        }
        /// <summary>
        /// The relative, non-architecture-specific ASM plug path.
        /// </summary>
        public string PlugASMFilePath
        {
            get;
            set;
        }

        /// <summary>
        /// The priority of the ILChunk - used in sequencing the final assembler code.
        /// </summary>
        public long SequencePriority
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the method used to create this ILChunk was 
        /// marked as the kernel's main entry point.
        /// </summary>
        public bool IsMainMethod = false;
        /// <summary>
        /// Indicates whether the method used to create this ILChunk was 
        /// marked as the kernel's halt method.
        /// </summary>
        public bool IsHaltMethod = false;
        /// <summary>
        /// Indicates whether the method used to create this ILChunk was 
        /// marked as the method to fill in to call all the 
        /// static constructors.
        /// </summary>
        public bool IsCallStaticConstructorsMethod = false;
        /// <summary>
        /// Indicates whether the method used to create this ILChunk was 
        /// marked as the method to call to add a new ExceptionHandlerInfo.
        /// </summary>
        public bool IsAddExceptionHandlerInfoMethod = false;
        /// <summary>
        /// Indicates whether the method used to create this ILChunk was 
        /// marked as the method to call to handle leaving an exception 
        /// handled block (i.e. a try or catch block not finally)
        /// </summary>
        public bool IsExceptionsHandleLeaveMethod = false;
        /// <summary>
        /// Indicates whether the method used to create this ILChunk was 
        /// marked as the method to call to handle an EndFinally Il op.
        /// </summary>
        public bool IsExceptionsHandleEndFinallyMethod = false;
        /// <summary>
        /// Indicates whether the method used to create this ILChunk was 
        /// marked as the method to call to handle an exception.
        /// </summary>
        public bool IsExceptionsHandleExceptionMethod = false;
        /// <summary>
        /// Indicates whether the method used to create this ILChunk was 
        /// marked as the method to call to throw an exception.
        /// </summary>
        public bool IsExceptionsThrowMethod = false;
        /// <summary>
        /// Indicates whether the method used to create this ILChunk was 
        /// marked as the method to call to create a new object.
        /// </summary>
        public bool IsNewObjMethod = false;
        /// <summary>
        /// Indicates whether the method used to create this ILChunk was 
        /// marked as the method to call to create a new array.
        /// </summary>
        public bool IsNewArrMethod = false;
        /// <summary>
        /// Indicates whether the method used to create this ILChunk was 
        /// marked as the method to call to increment a GC managed object's
        /// ref count.
        /// </summary>
        public bool IsIncrementRefCountMethod = false;
        /// <summary>
        /// Indicates whether the method used to create this ILChunk was 
        /// marked as the method to call to decrement a GC managed object's
        /// ref count.
        /// </summary>
        public bool IsDecrementRefCountMethod = false;
        /// <summary>
        /// Indicates whether the method used to create this ILChunk was 
        /// marked as the method to call to as the Array constructor.
        /// </summary>
        public bool IsArrayConstructorMethod = false;

        /// <summary>
        /// Whether to apply Garbage Collection stuff to the IL chunk or not.
        /// </summary>
        public bool ApplyGC = true;

        /// <summary>
        /// The method info that was used to create this ILChunk
        /// </summary>
        public MethodBase Method
        {
            get;
            set;
        }

        /// <summary>
        /// The exception handled blocks for the IL chunk.
        /// </summary>
        /// <remarks>
        /// For methods with GC applied, they will almost always have at least
        /// 1 exception handled block around the entire method so that args 
        /// / locals can be cleaned up.
        /// </remarks>
        public List<ExceptionHandledBlock> ExceptionHandledBlocks = new List<ExceptionHandledBlock>();

        /// <summary>
        /// The ILOpInfos for the IL chunk.
        /// </summary>
        public List<ILOpInfo> ILOpInfos = new List<ILOpInfo>();

        /// <summary>
        /// A list of the local variables in the method that the ILChunk represents. Only used by ILChunks which represent methods.
        /// </summary>
        public List<LocalVariable> LocalVariables = new List<LocalVariable>();

        /// <summary>
        /// Whether this IL chunk should or should not have debug ops
        /// emitted.
        /// </summary>
        public bool NoDebugOps = false;

        /// <summary>
        /// Gets the exception handled block that starts at exactly 
        /// the specified offset (if one exists).
        /// </summary>
        /// <param name="Offset">The start offset (in IL bytes) of the block to get.</param>
        /// <returns>The block or null if no block starts at the specified offset.</returns>
        public ExceptionHandledBlock GetExactExceptionHandledBlock(int Offset)
        {
            List<ExceptionHandledBlock> potBlocks = (from blocks in ExceptionHandledBlocks
                                                     where (blocks.Offset == Offset)
                                                     select blocks)
                                                     .ToList();
            if(potBlocks.Count > 0)
            {
                return potBlocks.First();
            }
            return null;
        }
        /// <summary>
        /// Gets the inner-most exception handled block that starts at or 
        /// contains the specified offset or has a handler that starts at or
        /// contains the specified offset.
        /// </summary>
        /// <param name="Offset">The offset to look for (in IL bytes).</param>
        /// <returns>The exception handled block or null if none found.</returns>
        public ExceptionHandledBlock GetExceptionHandledBlock(int Offset)
        {
            List<ExceptionHandledBlock> potBlocks = (from blocks in ExceptionHandledBlocks
                                                     where (
                                                     (blocks.Offset <= Offset &&
                                                      blocks.Offset + blocks.Length >= Offset)
                                                     ||  (from catchBlocks in blocks.CatchBlocks
                                                          where (
                                                            catchBlocks.Offset <= Offset &&
                                                            catchBlocks.Offset  + catchBlocks.Length >= Offset
                                                          )
                                                          select catchBlocks).Count() > 0
                                                     || (from finallyBlocks in blocks.FinallyBlocks
                                                         where (
                                                            finallyBlocks.Offset <= Offset &&
                                                            finallyBlocks.Offset + finallyBlocks.Length >= Offset
                                                         )
                                                         select finallyBlocks).Count() > 0
                                                     )
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
