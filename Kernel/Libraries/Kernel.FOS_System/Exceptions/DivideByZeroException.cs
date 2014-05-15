using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.Exceptions
{
    /// <summary>
    /// Represents a divide by zero exception.
    /// Usually thrown by hardware interrupt 0 when code attempts to divide a number (always integer?) by 0.
    /// </summary>
    public class DivideByZeroException : FOS_System.Exception
    {
        /// <summary>
        /// Sets the message to "Attempt to divide by zero invalid."
        /// </summary>
        public DivideByZeroException()
            : base("Attempt to divide by zero invalid.")
        {
        }
    }
}
