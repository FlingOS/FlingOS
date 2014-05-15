using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.Exceptions
{
    /// <summary>
    /// Represents a array type mismatch exception.
    /// </summary>
    public class ArrayTypeMismatchException : FOS_System.Exception
    {
        /// <summary>
        /// Sets the message to "Array type mismatch exception."
        /// </summary>
        public ArrayTypeMismatchException()
            : base("Array type mismatch exception.")
        {
        }
    }
}
