using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.Exceptions
{
    /// <summary>
    /// Represents a null reference exception.
    /// </summary>
    public class NullReferenceException : FOS_System.Exception
    {
        /// <summary>
        /// Sets the message to "Null reference exception."
        /// </summary>
        public NullReferenceException()
        {
            Message = "Null reference exception.";
        }
    }
}
