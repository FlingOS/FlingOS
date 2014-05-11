using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System
{
    public class Array : Object
    {
        /* If changing the fields in this class, remember to update the 
         * Kernel.GC.NewArr method implementation. */

        public int length;
        public Type elemType;

        [Compiler.ArrayConstructorMethod]
        public Array(int aLength, Type anElemType)
        {
            length = aLength;
            elemType = anElemType;
        }
    }
}
