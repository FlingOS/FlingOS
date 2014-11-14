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

namespace Kernel.Core
{
    /// <summary>
    /// Represents a text-only shell (/command-line) interface.
    /// </summary>
    public abstract class Shell : FOS_System.Object
    {
        /// <summary>
        /// The console used for input/output.
        /// </summary>
        protected Console console;
        /// <summary>
        /// Whether the shell is in the process of closing (/terminating) or not.
        /// </summary>
        protected bool terminating = false;
        /// <summary>
        /// Whether the shell is in the process of closing (/terminating) or not.
        /// </summary>
        public bool Terminating
        {
            get
            {
                return terminating;
            }
        }

        /// <summary>
        /// Initialises a new shell instances that uses the default console.
        /// </summary>
        public Shell()
        {
            Console.InitDefault();
            console = Console.Default;
        }

        /// <summary>
        /// Executes the shell.
        /// </summary>
        public abstract void Execute();
        
        /// <summary>
        /// Outputs informations about the current exception, if any.
        /// </summary>
        [Compiler.NoDebug]
        protected void OutputCurrentExceptionInfo()
        {
            if (ExceptionMethods.CurrentException != null)
            {
                console.WarningColour();
                console.WriteLine(ExceptionMethods.CurrentException.Message);
                if (ExceptionMethods.CurrentException is FOS_System.Exceptions.PageFaultException)
                {
                    console.WriteLine(((FOS_System.String)"    - Address: ") + ((FOS_System.Exceptions.PageFaultException)ExceptionMethods.CurrentException).address);
                    console.WriteLine(((FOS_System.String)"    - Error code: ") + ((FOS_System.Exceptions.PageFaultException)ExceptionMethods.CurrentException).errorCode);
                }
                console.DefaultColour();
            }
            else
            {
                console.WriteLine("No current exception.");
            }
        }

        /// <summary>
        /// Outputs a divider line.
        /// </summary>
        [Compiler.NoDebug]
        protected void OutputDivider()
        {
            console.WriteLine("--------------------------------------------------------");
        }

        /// <summary>
        /// The default shell for the core kernel.
        /// </summary>
        public static Shell Default;
        /// <summary>
        /// Initialises the default shell.
        /// </summary>
        public static void InitDefault()
        {
            if (Default == null)
            {
                Default = new Shells.MainShell();
            }
        }
    }
}
