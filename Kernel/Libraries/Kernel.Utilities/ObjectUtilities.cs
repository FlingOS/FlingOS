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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Utilities
{
    /// <summary>
    /// Utility methods for object manipulation.
    /// </summary>
    [Compiler.PluggedClass]
    public static class ObjectUtilities
    {
        /// <summary>
        /// Gets a handle for the specified object - basically, a round-about way of casting an object to a pointer.
        /// </summary>
        /// <remarks>
        /// All the plug does is to set the return value to the argument value!
        /// </remarks>
        /// <param name="anObj">The object to get a handle of.</param>
        /// <returns>The pointer to the object.</returns>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\ObjectUtilities\GetHandle")]
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static unsafe void* GetHandle(object anObj)
        {
            return null;
        }
        /// <summary>
        /// Gets an object for the specified pointer - basically, a round-about way of casting a pointer to an object.
        /// </summary>
        /// <remarks>
        /// All the plug does is to set the return value to the argument value!
        /// </remarks>
        /// <param name="anObjPtr">The pointer to get an object of.</param>
        /// <returns>The object the pointer points to.</returns>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\ObjectUtilities\GetObject")]
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static unsafe object GetObject(void* anObjPtr)
        {
            return null;
        }

    }
}
