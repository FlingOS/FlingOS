#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
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
