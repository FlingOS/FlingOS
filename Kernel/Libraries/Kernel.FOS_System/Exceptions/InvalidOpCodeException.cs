using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.Exceptions
{
    /// <summary>
    /// Represents an invalid op-code exception.
    /// Usually thrown by the hardware interrupt.
    /// </summary>
    public class InvalidOpCodeException : FOS_System.Exception
    {
        /// <summary>
        /// Sets the message to "Attempted to execute an invalid op code."
        /// </summary>
        public InvalidOpCodeException()
        {
            Message = "Attempted to execute an invalid op code.";
        }
    }
}
