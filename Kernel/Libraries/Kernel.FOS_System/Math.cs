#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
using System;

namespace Kernel.FOS_System
{
    /// <summary>
    /// Provides constants and static methods for common mathematical functions and some operations not supported by 
    /// IL code.
    /// </summary>
    [Compiler.PluggedClass]
    public static class Math
    {
        /// <summary>
        /// Divides a UInt64 by a UInt32.
        /// </summary>
        /// <param name="dividend">The UInt64 to be divided.</param>
        /// <param name="divisor">The UInt32 to divide by.</param>
        /// <returns>The quotient of the division.</returns>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\Math\Divide")]
        public static ulong Divide(ulong dividend, uint divisor)
        {
            return 0;
        }

        /// <summary>
        /// Returns the lower of the two inputs.
        /// </summary>
        /// <param name="x">Input 1.</param>
        /// <param name="y">Input 2.</param>
        /// <returns>The lower of the two inputs.</returns>
        public static ushort Min(ushort x, ushort y)
        {
            return (x < y ? x : y);
        }
        /// <summary>
        /// Returns the higher of the two inputs.
        /// </summary>
        /// <param name="x">Input 1.</param>
        /// <param name="y">Input 2.</param>
        /// <returns>The higher of the two inputs.</returns>
        public static int Max(int x, int y)
        {
            return (x > y ? x : y);
        }
    }
}
