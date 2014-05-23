using System;

namespace Kernel.FOS_System.IO
{
    public class IOException : FOS_System.Exception
    {
        public IOException(string aMessage)
            : base(aMessage)
        {
        }
    }
}
