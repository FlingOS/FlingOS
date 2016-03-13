using Kernel.FOS_System;
using Kernel.FOS_System.Processes;
using Kernel.FOS_System.IO;


namespace Kernel.Tasks.Driver
{
    public static class FileSystemsDriverTask
    {
        private static Consoles.VirtualConsole console;

        private static uint GCThreadId;

        public static bool Terminating = false;

        public static void Main()
        {
            Helpers.ProcessInit("File Systems Driver", out GCThreadId);

            try
            {
                BasicConsole.WriteLine("File Systems Driver > Creating virtual console...");
                console = new Consoles.VirtualConsole();

                BasicConsole.WriteLine("File Systems Driver > Connecting virtual console...");
                console.Connect();

                BasicConsole.WriteLine("File Systems Driver > Executing.");

                try
                {
                    BasicConsole.WriteLine("File Systems Driver > Initialising file systems...");
                    FileSystemManager.Init();

                    while (!Terminating)
                    {
                        BasicConsole.WriteLine("File Systems Driver > Searching for storage...");
                        FileSystemManager.CheckForStoragePipes();

                        SystemCalls.SleepThread(10000);
                    }
                }
                catch
                {
                    BasicConsole.WriteLine("File Systems Driver > Error executing!");
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                }

                BasicConsole.WriteLine("File Systems Driver > Execution complete.");
            }
            catch
            {
                BasicConsole.WriteLine("File Systems Driver > Error initialising!");
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
            }

            BasicConsole.WriteLine("File Systems Driver > Exiting...");
        }
    }
}
