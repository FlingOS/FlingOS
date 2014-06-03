using System;

namespace Kernel.FOS_System.IO
{
    /// <summary>
    /// Represents an IO exception.
    /// </summary>
    public class IOException : FOS_System.Exception
    {
        /// <summary>
        /// Initializes a new IO exception.
        /// </summary>
        /// <param name="aMessage">The IO exception message.</param>
        public IOException(string aMessage)
            : base(aMessage)
        {
        }
    }
}
