using Drivers.Compiler.Attributes;
using Kernel.Consoles;
using Kernel.FileSystems;
using Kernel.Framework.Processes;

namespace Kernel.Tasks.Driver
{
    public static class FileSystemsDriverTask
    {
        private static VirtualConsole console;

        private static uint GCThreadId;

        public static bool Terminating = false;

        [Group(Name = "IsolatedKernel")] public static bool Ready;

        public static void Main()
        {
            Ready = false;

            Helpers.ProcessInit("File Systems Driver", out GCThreadId);

            try
            {
                BasicConsole.WriteLine("File Systems Driver > Creating virtual console...");
                console = new VirtualConsole();

                BasicConsole.WriteLine("File Systems Driver > Connecting virtual console...");
                console.Connect();

                BasicConsole.WriteLine("File Systems Driver > Executing.");

                try
                {
                    BasicConsole.WriteLine("File Systems Driver > Initialising partition manager...");
                    PartitionManager.Init();

                    BasicConsole.WriteLine("File Systems Driver > Initialising storage manager...");
                    StorageManager.Init();

                    BasicConsole.WriteLine("File Systems Driver > Initialising file system manager...");
                    FileSystemManager.Init();

                    BasicConsole.WriteLine("File Systems Driver > Reporting ready.");
                    Ready = true;

                    int Delay = 10000;
                    int DelayMax = 60000*5;
                    while (!Terminating)
                    {
                        BasicConsole.WriteLine("File Systems Driver > Searching for file systems...");

                        StorageManager.InitStoragePipes();
                        StorageManager.InitControllers();

                        SystemCalls.SleepThread(Delay);
                        
                        if (Delay * 2 < DelayMax)
                        {
                            Delay = Delay * 2;
                        }
                        else
                        {
                            Delay = DelayMax;
                        }
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