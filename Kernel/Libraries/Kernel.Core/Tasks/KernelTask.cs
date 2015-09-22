using System;
using Kernel.FOS_System;
using Kernel.Core.Processes;
using Kernel.Hardware.Processes;

namespace Kernel.Core.Tasks
{
    public unsafe static class KernelTask
    {
        public static bool Terminate = false;

        public static void Main()
        {
            BasicConsole.WriteLine("Kernel task! ");
            BasicConsole.WriteLine(" > Executing normally...");

            try
            {
                BasicConsole.WriteLine(" > Initialising system calls...");
                Core.Processes.SystemCalls.Init();

                ProcessManager.CurrentProcess.SyscallHandler = SyscallHandler;
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.Sleep);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.RegisterSyscallHandler);
                ProcessManager.CurrentProcess.SyscallsToHandle.Set((int)SystemCallNumbers.DeregisterSyscallHandler);

                BasicConsole.WriteLine(" > Starting GC Cleanup thread...");
                ProcessManager.CurrentProcess.CreateThread(Core.Tasks.GCCleanupTask.Main);

                BasicConsole.WriteLine(" > Starting Idle thread...");
                ProcessManager.CurrentProcess.CreateThread(Core.Tasks.IdleTask.Main);

                BasicConsole.WriteLine(" > Starting Device Manager...");
                ProcessManager.RegisterProcess(
                    ProcessManager.CreateProcess(DeviceManagerTask.Main, "Device Manager", false), 
                    Scheduler.Priority.Normal);

                BasicConsole.WriteLine(" > Starting Window Manager...");
                ProcessManager.RegisterProcess(
                    ProcessManager.CreateProcess(WindowManagerTask.Main, "Window Manager", false), 
                    Scheduler.Priority.Normal);

                //TODO: Main task for commands etc

                BasicConsole.PrimaryOutputEnabled = false;

                while (!Terminate)
                {
                    SystemCallMethods.Sleep(100);

                    if (!SystemCallMethods.Ping())
                    {
                        BasicConsole.WriteLine("Ping failed.");
                    }
                }

                //BasicConsole.WriteLine(" > Starting Non-critical interrupts task...");
                //ProcessManager.CurrentProcess.CreateThread(Hardware.Tasks.NonCriticalInterruptsTask.Main);

                //BasicConsole.WriteLine(" > Starting System Status task...");
                //ProcessManager.CurrentProcess.CreateThread(Core.Tasks.SystemStatusTask.Main);

                //BasicConsole.WriteLine(" > Starting Device Manager task...");
                //ProcessManager.CurrentProcess.CreateThread(Hardware.Tasks.DeviceManagerTask.Main);

                //BasicConsole.WriteLine(" > Starting Play Notes task...");
                //ProcessManager.CurrentProcess.CreateThread(Hardware.Tasks.PlayNotesTask.Main);

                //Hardware.Devices.Keyboard.InitDefault();
                //Core.Console.InitDefault();
                //Core.Shell.InitDefault();

                //BasicConsole.PrimaryOutputEnabled = false;
                //Core.Shell.Default.Execute();
                //BasicConsole.PrimaryOutputEnabled = true;

                //if (!Core.Shell.Default.Terminating)
                //{
                //    Core.Console.Default.WarningColour();
                //    Core.Console.Default.WriteLine("Abnormal shell shutdown!");
                //    Core.Console.Default.DefaultColour();
                //}
                //else
                //{
                //    Core.Console.Default.Clear();
                //}
            }
            catch
            {
                BasicConsole.PrimaryOutputEnabled = true;
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            BasicConsole.WriteLine();
            BasicConsole.WriteLine("---------------------");
            BasicConsole.WriteLine();
            BasicConsole.WriteLine("End of kernel task.");

            ExceptionMethods.HaltReason = "Managed main thread ended.";

            //TODO: Exit syscall
        }

        public static int SyscallHandler(uint syscallNumber, uint param1, uint param2, uint param3, 
            ref uint Return2, ref uint Return3, ref uint Return4,
            uint callerProcesId, uint callerThreadId)
        {
            return SystemCalls.HandleSystemCallForKernel(syscallNumber, 
                param1, param2, param3, 
                ref Return2, ref Return3, ref Return4,
                callerProcesId, callerThreadId);
        }
    }
}
