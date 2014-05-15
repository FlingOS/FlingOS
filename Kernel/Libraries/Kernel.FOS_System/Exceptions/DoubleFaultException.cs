using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.Exceptions
{
    /// <summary>
    /// Represents a double fault exception.
    /// Usually thrown by the hardware interrupt.
    /// </summary>
    public class DoubleFaultException : FOS_System.Exception
    {
        /// <summary>
        /// Sets the message to "Double fault exception."
        /// </summary>
        public DoubleFaultException()
            : base("Double fault exception.")
        {
        }
    }
}
