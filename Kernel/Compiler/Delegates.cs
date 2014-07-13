#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
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
