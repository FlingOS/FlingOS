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
                bool reenable = Hardware.Processes.Scheduler.Enabled;
                try
                {
                    if (reenable)
                    {
                        Hardware.Processes.Scheduler.Disable();
                    }
                    FOS_System.GC.Cleanup();
                }
                catch
                {
                }

                if (reenable)
                {
                    Hardware.Processes.Scheduler.Enable();
                }
                Hardware.Processes.Thread.Sleep(1000);
            }
        }
    }
}
