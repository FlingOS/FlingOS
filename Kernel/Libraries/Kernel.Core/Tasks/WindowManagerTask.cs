using System;
using Kernel.FOS_System;
using Kernel.Core.Processes;

namespace Kernel.Core.Tasks
{
    public static unsafe class WindowManagerTask
    {
        private static Console ScreenOutput;
        private static int Pings = 0;

        public static void Main()
        {
            BasicConsole.WriteLine("Window Manager: Started.");

            int loops = 0;

            ScreenOutput = new Consoles.AdvancedConsole();

            SystemCallMethods.RegisterSyscallHandler(SystemCallNumbers.Semaphore, SyscallHandler);

            while (true)
            {
                try
                {
                    ScreenOutput.Clear();
                    ScreenOutput.Write("Window Manager Task (");
                    ScreenOutput.Write_AsDecimal(loops);
                    ScreenOutput.WriteLine(")");

                    ScreenOutput.Write("WM > Pings : ");
                    ScreenOutput.WriteLine_AsDecimal(Pings);

                    ScreenOutput.Write("WM Heap: ");
                    uint totalMem = Heap.GetTotalMem();
                    ScreenOutput.Write_AsDecimal(Heap.GetTotalUsedMem() / (totalMem / 100));
                    ScreenOutput.Write("% / ");
                    ScreenOutput.Write_AsDecimal(totalMem / 1024);
                    ScreenOutput.WriteLine(" KiB");

                    SystemCallMethods.Sleep(100);

                    loops++;
                }
                catch
                {
                    BasicConsole.WriteLine("Exception running window manager.");
                }
            }
        }

        public static int SyscallHandler(uint syscallNumber, uint param1, uint param2, uint param3,
            ref uint Return2, ref uint Return3, ref uint Return4,
            uint callerProcesId, uint callerThreadId)
        {
            SystemCallResults result = SystemCallResults.Unhandled;

            switch((SystemCallNumbers)syscallNumber)
            {
                case SystemCallNumbers.Semaphore:
                    Pings++;
                    result = SystemCallResults.OK;
                    break;
            }

            return (int)result;
        }
    }
}
