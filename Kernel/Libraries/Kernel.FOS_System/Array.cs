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

namespace Kernel.FOS_System
{
    /// <summary>
    /// Represents the underlying type of any array within the Kernel.
    /// </summary>
    [Compiler.ArrayClass]
    public unsafe class Array : Object
    {
        /* If changing the fields in this class, remember to update the 
         * Kernel.GC.NewArr method implementation. */

        /// <summary>
        /// The length of the array. Can also use standard System.Array.Length e.g. new object[5].Length.
        /// </summary>
        public int length;
        /// <summary>
        /// The type of the elements within the array. Do NOT change this except during array setup 
        /// (i.e. in GC.NewArr method).
        /// </summary>
        public Type elemType;

        /// <summary>
        /// Implicitly converts a System.Array to an FOS_System.Array. The two are one and the same thing within the 
        /// kernel just Fos_System.Array allows access to actual fields.
        /// </summary>
        /// <param name="x">The System.Array to convert.</param>
        /// <returns>The FOS_System.Array (a reference to the exact same object).</returns>
        [Compiler.NoGC]
        [Compiler.NoDebug]
        public static implicit operator FOS_System.Array(object[] x)
        {
            return (FOS_System.Array)(object)x;
        }

        /// <summary>
        /// Copies the number of elements ("count") at sourceOffset from source to elements in dest at destOffset.
        /// </summary>
        /// <param name="source">The array to copy elements from.</param>
        /// <param name="sourceOffset">The offset in the source array to start copying at.</param>
        /// <param name="dest">The array to copy elements to.</param>
        /// <param name="destOffset">The offset in the destination array to start copying to.</param>
        /// <param name="count">The number of elements to copy.</param>
        [Compiler.NoGC]
        [Compiler.NoDebug]
        public static void Copy(byte[] source, int sourceOffset, byte[] dest, int destOffset, int count)
        {
            int srcIndex = sourceOffset;
            int destIndex = destOffset;
            for (int i = 0; i < count; i++, srcIndex++, destIndex++)
            {
                dest[destIndex] = source[srcIndex];
            }
        }
    }
}
