using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.Exceptions
{
    public class NoDiskException : FOS_System.Exception
    {
        public NoDiskException(FOS_System.String message)
            : base("No disk in drive. " + message)
        {
        }
    }
}
