using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.Exceptions
{
    /// <summary>
    /// Represents a not supported exception.
    /// </summary>
    public class NotSupportedException : FOS_System.Exception
    {
        /// <summary>
        /// Sets the message to "Not supported exception."
        /// </summary>
        /// <param name="anExtendedMessage">The extended message to append to the main message.</param>
        public NotSupportedException(FOS_System.String anExtendedMessage)
            : base("Not supported exception. " + anExtendedMessage)
        {
        }
    }
}
