using System;
using Kernel.FOS_System;
using Kernel.Core.Processes;

namespace Kernel.Core.Tasks
{
    public static unsafe class WindowManagerTask
    {
        private static Console ScreenOutput;
        private static int Pings = 0;
        private static int TestThread_Loops = 0;
        private static int TestThread2_Loops = 0;
        private static bool Terminating = false;

        public static void Main()
        {
            BasicConsole.WriteLine("Window Manager: Started.");

            Hardware.Processes.ProcessManager.CurrentProcess.InitHeap();
            if (SystemCallMethods.CreateThread(GCCleanupTask.Main) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Window Manager: GC thread failed to create!");
            }

            int loops = 0;
            ScreenOutput = new Consoles.AdvancedConsole();

            SystemCallMethods.RegisterSyscallHandler(SystemCallNumbers.Semaphore, SyscallHandler);

            if (SystemCallMethods.CreateThread(TestThread) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Window Manager: Test thread failed to create!");
            }
            if (SystemCallMethods.CreateThread(TestThread2) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Window Manager: Test thread 2 failed to create!");
            }

            while (!Terminating)
            {
                try
                {
                    ScreenOutput.Clear();
                    ScreenOutput.Write("Window Manager Task (");
                    ScreenOutput.Write_AsDecimal(loops);
                    ScreenOutput.WriteLine(")");

                    ScreenOutput.Write("WM > Pings : ");
                    ScreenOutput.WriteLine_AsDecimal(Pings);

                    ScreenOutput.Write("WM > Test Thread loops : ");
                    ScreenOutput.WriteLine_AsDecimal(Pings);

                    ScreenOutput.Write("WM > Test Thread 2 loops : ");
                    ScreenOutput.WriteLine_AsDecimal(Pings);

                    ScreenOutput.WriteLine();

                    ScreenOutput.Write("WM > Heap: ");
                    uint totalMem = Heap.GetTotalMem();
                    ScreenOutput.Write_AsDecimal(Heap.GetTotalUsedMem() / (totalMem / 100));
                    ScreenOutput.Write("% / ");
                    ScreenOutput.Write_AsDecimal(totalMem / 1024);
                    ScreenOutput.WriteLine(" KiB");

                    ScreenOutput.WriteLine();

                    ScreenOutput.Write("WM > Number of objects: ");
                    ScreenOutput.WriteLine_AsDecimal(FOS_System.GC.NumObjs);
                    ScreenOutput.Write("WM > Number of strings: ");
                    ScreenOutput.WriteLine_AsDecimal(FOS_System.GC.NumStrings);

                    SystemCallMethods.Sleep(500);

                    loops++;
                }
                catch
                {
                    BasicConsole.WriteLine("Exception running window manager.");
                }
            }
        }

        public static void TestThread()
        {
            while (!Terminating)
            {
                try
                {
                    //BasicConsole.WriteLine("WM > Test Thread");
                    TestThread_Loops++;
                    SystemCallMethods.Sleep(100);
                }
                catch
                {
                    BasicConsole.WriteLine("Error in test thread!");
                }
            }
        }

        public static void TestThread2()
        {
            while (!Terminating)
            {
                try
                {
                    //BasicConsole.WriteLine("WM > Test Thread 2");
                    TestThread2_Loops++;
                    SystemCallMethods.Sleep(100);
                }
                catch
                {
                    BasicConsole.WriteLine("Error in test thread 2!");
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
