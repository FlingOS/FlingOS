using Kernel.FOS_System;

namespace Kernel.Devices.Exceptions
{
    public class NoDiskException : Exception
    {
        public NoDiskException(String message)
            : base("No disk in drive. " + message)
        {
        }
    }
}