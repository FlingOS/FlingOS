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
        /// <summary>
        /// Sets the message to "Argument exception".
        /// </summary>
        /// <param name="anExtendedMessage">
        /// The extended message to append to the main message. Should specify which argument caused the exception.
        /// </param>
        public ArgumentException(FOS_System.String anExtendedMessage)
            : base("Argument exception. " + anExtendedMessage)
        {
        }
    }
}
