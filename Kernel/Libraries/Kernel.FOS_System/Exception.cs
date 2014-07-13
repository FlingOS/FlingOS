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

namespace Kernel.FOS_System
{
    /// <summary>
    /// An exception object.
    /// </summary>
    public class Exception : Object
    {
        /// <summary>
        /// The exception message.
        /// </summary>
        public FOS_System.String Message;

        /// <summary>
        /// Creates a new, empty exception.
        /// </summary>
        public Exception()
            : base()
        {
        }
        /// <summary>
        /// Creates a new exception with specified message.
        /// </summary>
        /// <param name="aMessage">The exception message.</param>
        public Exception(FOS_System.String aMessage)
            : base()
        {
            Message = aMessage;
        }
    }
}
