using System;
using Kernel.FOS_System;
using Kernel.Core.Processes;

namespace Kernel.Core.Tasks
{
    public static unsafe class DeviceManagerTask
    {
        public static bool Terminating = false;

        public static void Main()
        {
            BasicConsole.WriteLine("Device Manager started.");

            Hardware.Processes.ProcessManager.CurrentProcess.InitHeap();
            if (SystemCallMethods.StartThread(GCCleanupTask.Main) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Device Manager: GC thread failed to create!");
            }

            switch (SystemCallMethods.RegisterPipeOutpoint(Pipes.PipeClasses.Display, Pipes.PipeSubclasses.Display_Text_ASCII, 1))
            {
                case SystemCallResults.Unhandled:
                    BasicConsole.WriteLine("DM > RegisterPipeOutpoint: Unhandled!");
                    break;
                case SystemCallResults.Fail:
                    BasicConsole.WriteLine("DM > RegisterPipeOutpoint: Failed!");
                    break;
                case SystemCallResults.OK:
                    BasicConsole.WriteLine("DM > RegisterPipeOutpoint: Succeeded.");
                    break;
                default:
                    BasicConsole.WriteLine("DM > RegisterPipeOutpoint: Unexpected system call result!");
                    break;
            }

            int numOutpoints;
            switch (SystemCallMethods.GetNumPipeOutpoints(Pipes.PipeClasses.Display, Pipes.PipeSubclasses.Display_Text_ASCII, out numOutpoints))
            {
                case SystemCallResults.Unhandled:
                    BasicConsole.WriteLine("DM > GetNumPipeOutpoints: Unhandled!");
                    break;
                case SystemCallResults.Fail:
                    BasicConsole.WriteLine("DM > GetNumPipeOutpoints: Failed!");
                    break;
                case SystemCallResults.OK:
                    BasicConsole.WriteLine("DM > GetNumPipeOutpoints: Succeeded.");
                    break;
                default:
                    BasicConsole.WriteLine("DM > GetNumPipeOutpoints: Unexpected system call result!");
                    break;
            }
            BasicConsole.Write("DM > Num pipe outpoints of class: Display, subclass: Display_Text_ASCII = ");
            BasicConsole.WriteLine(numOutpoints);

            while (!Terminating)
            {
                //TODO
                testclass x = new testclass();

                SystemCallMethods.SleepThread(SystemCallMethods.IndefiniteSleepThread);
            }
        }
    }

    class testclass : FOS_System.Object
    {
    }
}
