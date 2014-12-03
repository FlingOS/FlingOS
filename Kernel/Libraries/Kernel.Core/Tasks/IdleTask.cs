using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Core.Tasks
{
    public static unsafe class IdleTask
    {
        public static void Main()
        {
            while (true)
            {
                Hardware.Devices.Timer.Default.Wait(1000);
            }
        }
    }
}
