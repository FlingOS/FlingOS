using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Core.Tasks
{
    public static class GCCleanupTask
    {
        public static void Main()
        {
            while (true)
            {
                try
                {
                    Hardware.Processes.Scheduler.Disable();
                    FOS_System.GC.Cleanup();
                }
                catch
                {
                }

                Hardware.Processes.Scheduler.Enable();
                Hardware.Processes.Thread.Sleep(300);
            }
        }
    }
}
