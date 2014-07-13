#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.DMA
{
    /// <summary>
    /// Provides direct access memory block access.
    /// </summary>
    public unsafe class DMABlock
    {
        /// <summary>
        /// The memory block base pointer.
        /// </summary>
        public byte* Base;
        /// <summary>
        /// The size of the memory block.
        /// </summary>
        public uint Size;

        /// <summary>
        /// Initialises a new direct access memory block .
        /// </summary>
        /// <param name="aBase">The block base pointer.</param>
        /// <param name="aSize">The size of the block.</param>
        public DMABlock(byte* aBase, uint aSize)
        {
            Base = aBase;
            Size = aSize;
        }

        /// <summary>
        /// Gets the UInt32 at the specified byte-specific index.
        /// </summary>
        /// <param name="byteIndex">The byte-specific index of the first byte of the UInt32.</param>
        /// <returns>The UInt32 starting at the specified byte.</returns>
        public UInt32 this[UInt32 byteIndex]
        {
            get
            {
                if (byteIndex >= Size)
                {
                    Kernel.ExceptionMethods.Throw(new FOS_System.Exception("DMABlock: Memory access violation! byteIndex >= Size"));
                }
                return *(UInt32*)(Base + byteIndex);
            }
            set
            {
                if (byteIndex >= Size)
                {
                    Kernel.ExceptionMethods.Throw(new FOS_System.Exception("DMABlock: Memory access violation! byteIndex >= Size"));
                }
                *(UInt32*)(Base + byteIndex) = value;
            }
        }
    }
}
