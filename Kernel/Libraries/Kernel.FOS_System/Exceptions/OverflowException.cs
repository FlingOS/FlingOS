using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.Exceptions
{
    /// <summary>
    /// Represents an overflow exception.
    /// Usually thrown by the hardware interrupt.
    /// </summary>
    public class OverflowException : FOS_System.Exception
    {
        /// <summary>
        /// Sets the message to "Overflow exception."
        /// </summary>
        public OverflowException()
        {
            Message = "Overflow exception.";
        }
    }
}
