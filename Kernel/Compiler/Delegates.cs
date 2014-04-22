using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler
{
    /// <summary>
    /// A delegate for a method that can output an error message.
    /// </summary>
    /// <param name="ex">The exception to output.</param>
    public delegate void OutputErrorDelegate(Exception ex);
    /// <summary>
    /// A delegate for a method that can output an standard message.
    /// </summary>
    /// <param name="message">The message to output.</param>
    public delegate void OutputMessageDelegate(string message);
    /// <summary>
    /// A delegate for a method that can output an warning message.
    /// </summary>
    /// <param name="ex">The warning to output.</param>
    public delegate void OutputWarningDelegate(Exception ex);
}
