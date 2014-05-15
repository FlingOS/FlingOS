using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System
{
    /// <summary>
    /// An exception object.
    /// </summary>
    public class Exception : Object
    {
        /// <summary>
        /// The exception message.
        /// </summary>
        public string Message;

        /// <summary>
        /// Creates a new, empty exception.
        /// </summary>
        public Exception()
            : base()
        {
        }
        /// <summary>
        /// Creates a new exception with specified message.
        /// </summary>
        /// <param name="aMessage">The exception message.</param>
        public Exception(string aMessage)
            : base()
        {
            Message = aMessage;
        }
    }
}
