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
        public FOS_System.String ExtendedMessage;

        /// <summary>
        /// Sets the message to "Argument exception."
        /// </summary>
        public NotSupportedException(FOS_System.String anExtendedMessage)
            : base("Not supported exception.")
        {
            ExtendedMessage = anExtendedMessage;
        }
    }
}
