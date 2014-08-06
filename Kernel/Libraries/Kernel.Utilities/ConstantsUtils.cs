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

namespace Kernel.Utilities
{
    /// <summary>
    /// Static utility methods for constant values.
    /// </summary>
    public static class ConstantsUtils
    {
        /// <summary>
        /// Creates a mask for the specified bit index.
        /// </summary>
        /// <param name="bitNum">The bit index to mask.</param>
        /// <returns>The mask.</returns>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static uint BIT(int bitNum)
        {
            return 1u << bitNum;
        }
    }
}
