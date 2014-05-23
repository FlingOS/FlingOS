using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.Exceptions
{
    /// <summary>
    /// Represents an argument exception.
    /// </summary>
    public class ArgumentException : FOS_System.Exception
    {
        public FOS_System.String ExtendedMessage;

        /// <summary>
        /// Sets the message to "Argument exception."
        /// </summary>
        public ArgumentException(FOS_System.String anExtendedMessage)
            : base("Argument exception.")
        {
            ExtendedMessage = anExtendedMessage;
        }
    }
}
