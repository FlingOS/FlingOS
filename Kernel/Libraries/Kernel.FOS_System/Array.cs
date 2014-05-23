using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System
{
    public unsafe class Array : Object
    {
        /* If changing the fields in this class, remember to update the 
         * Kernel.GC.NewArr method implementation. */

        public int length;
        public Type elemType;

        [Compiler.ArrayConstructorMethod]
        [Compiler.NoDebug]
        public Array(int aLength, Type anElemType)
        {
            length = aLength;
            elemType = anElemType;
        }

        [Compiler.NoGC]
        [Compiler.NoDebug]
        public static implicit operator FOS_System.Array(object[] x)
        {
            return (FOS_System.Array)(object)x;
        }

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
