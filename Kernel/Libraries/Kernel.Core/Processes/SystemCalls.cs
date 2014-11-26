﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Core.Processes
{
    public static class SystemCalls
    {
        private static int Int48HandlerId = 0;
        public static void Init()
        {
            if (Int48HandlerId == 0)
            {
                // We want to ignore process state so that we handle the interrupt in the context of
                //  the calling process.
                Int48HandlerId = Hardware.Interrupts.Interrupts.AddISRHandler(48, Int48, null, true);
            }
        }

        private static void Int48(FOS_System.Object state)
        {
            //Console.Default.WriteLine("Sys call by " + Hardware.Processes.ProcessManager.CurrentProcess.Name);
            BasicConsole.WriteLine("Sys call:");
            BasicConsole.WriteLine(" Process: " + Hardware.Processes.ProcessManager.CurrentProcess.Name);
            //Hardware.Processes.Thread.EnterSleepPrint = true;
            //Hardware.Processes.Thread.EnterSleep(1000);
            //Hardware.Processes.Thread.EnterSleepPrint = false;
            //BasicConsole.WriteLine("Exiting sys call.");
        }
    }
}