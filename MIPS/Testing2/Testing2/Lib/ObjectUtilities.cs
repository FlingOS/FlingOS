using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing2.Utilities
{
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
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = @"ASM\ObjectUtilities\GetHandle")]
        [Drivers.Compiler.Attributes.NoGC]
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
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = @"ASM\ObjectUtilities\GetObject")]
        [Drivers.Compiler.Attributes.NoGC]
        public static unsafe object GetObject(void* anObjPtr)
        {
            return null;
        }
    }
}
