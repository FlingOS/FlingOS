using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.Exceptions
{
    /// <summary>
    /// Represents a stack exception.
    /// Usually thrown by the hardware interrupt.
    /// </summary>
    public class StackException : FOS_System.Exception
    {
        /// <summary>
        /// Sets the message to "Stack error."
        /// </summary>
        public StackException()
            : base("Stack error.")
        {
        }
    }
}
