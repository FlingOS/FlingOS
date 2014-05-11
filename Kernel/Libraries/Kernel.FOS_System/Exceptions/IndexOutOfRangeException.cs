using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.Exceptions
{
    /// <summary>
    /// Represents an index out of range exception.
    /// </summary>
    public class IndexOutOfRangeException : FOS_System.Exception
    {
        /// <summary>
        /// Sets the message to "Index out of range exception."
        /// </summary>
        public IndexOutOfRangeException()
        {
            Message = "Index out of range exception.";
        }
    }
}
