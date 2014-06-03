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
