using Kernel.FOS_System;

namespace Kernel.Hardware.Exceptions
{
    public class NoDiskException : Exception
    {
        public NoDiskException(String message)
            : base("No disk in drive. " + message)
        {
        }
    }
}