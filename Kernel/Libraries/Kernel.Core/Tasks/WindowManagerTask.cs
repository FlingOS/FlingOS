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
            int loops = 0;

            ScreenOutput = new Consoles.AdvancedConsole();

            SystemCallMethods.RegisterSyscallHandler(SystemCallNumbers.Semaphore, SyscallHandler);

            while (true)
            {
                ScreenOutput.Write("Window Manager Task (");
                ScreenOutput.Write_AsDecimal(loops);
                ScreenOutput.WriteLine(")");

                ScreenOutput.Write(" > Pings : ");
                ScreenOutput.WriteLine_AsDecimal(Pings);

                SystemCallMethods.Sleep(1000);

                loops++;
            }
        }

        public static int SyscallHandler(uint syscallNumber, uint param1, uint param2, uint param3,
            ref uint Return2, ref uint Return3, ref uint Return4,
            uint callerProcesId, uint callerThreadId)
        {
            switch((SystemCallNumbers)syscallNumber)
            {
                case SystemCallNumbers.Semaphore:
                    Pings++;
                    return 0;
            }
            return -1;
        }
    }
}
