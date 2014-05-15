using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.Exceptions
{
    /// <summary>
    /// Represents a page fault exception.
    /// Usually thrown by the hardware interrupt.
    /// </summary>
    public class PageFaultException : FOS_System.Exception
    {
        /// <summary>
        /// The error code passed with the exception.
        /// </summary>
        public uint errorCode;
        /// <summary>
        /// The (virtual) address that caused the exception.
        /// </summary>
        public uint address;

        /// <summary>
        /// Sets the message to "Page fault"
        /// </summary>
        public PageFaultException(uint anErrorCode, uint anAddress)
            : base("Page fault")
        {
            errorCode = anErrorCode;
            address = anAddress;
        }
    }
}
