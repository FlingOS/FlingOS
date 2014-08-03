using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Core
{
    public abstract class Shell : FOS_System.Object
    {
        protected Console console;
        protected bool terminating = false;
        public bool Terminating
        {
            get
            {
                return terminating;
            }
        }

        public Shell()
        {
            Console.InitDefault();
            console = Console.Default;
        }

        public abstract void Execute();
        
        [Compiler.NoDebug]
        protected void OutputCurrentExceptionInfo()
        {
            if (ExceptionMethods.CurrentException != null)
            {
                console.WarningColour();
                console.WriteLine(ExceptionMethods.CurrentException.Message);
                console.DefaultColour();
            }
            else
            {
                console.WriteLine("No current exception.");
            }
        }

        [Compiler.NoDebug]
        protected void OutputDivider()
        {
            console.WriteLine("--------------------------------------------------------");
        }

        public static Shell Default;
        public static void InitDefault()
        {
            if (Default == null)
            {
                Default = new Shells.MainShell();
            }
        }
    }
}
