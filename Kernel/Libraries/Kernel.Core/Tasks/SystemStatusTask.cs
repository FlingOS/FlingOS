using System;
using Kernel.FOS_System.IO;
using Kernel.Hardware.Processes;

namespace Kernel.Core.Tasks
{
    public static class SystemStatusTask
    {
        private static Console MainConsole;
        private static Console StatusConsole;

        private static bool Terminating = false;

        public static void Main()
        {
            BasicConsole.Clear();
            Console.InitDefault();
            Console.Default.ScreenHeightInLines = 25 - 8;
            Console.Default.ScreenStartLine = 8;

            // Wait for other system startup to occur
            Thread.Sleep(1000);
            
            MainConsole = new Consoles.AdvancedConsole();
            MainConsole.ScreenHeightInLines = 7;
            MainConsole.LineLength = 60;
            MainConsole.ScreenStartLineOffset = 0;
            MainConsole.UpdateCursorPosition = false;

            StatusConsole = new Consoles.AdvancedConsole();
            StatusConsole.ScreenHeightInLines = 7;
            StatusConsole.LineLength = 19;
            StatusConsole.ScreenStartLineOffset = 61;
            StatusConsole.UpdateCursorPosition = false;

            MainConsole.Clear();
            StatusConsole.Clear();

            bool StatusLine1 = true;

            Hardware.DeviceManager.AddDeviceAddedListener(DeviceManager_DeviceAdded, null);

            while (!Terminating)
            {
                try
                {
                    ((Consoles.AdvancedConsole)StatusConsole).DrawBottomBorder();
                    ((Consoles.AdvancedConsole)StatusConsole).DrawLeftBorder();
                    ((Consoles.AdvancedConsole)MainConsole).DrawBottomBorder();
            
                    StatusConsole.Clear();
                    if (StatusLine1)
                    {
                        StatusConsole.WriteLine("State: 1");
                    }
                    else
                    {
                        StatusConsole.WriteLine("State: 2");
                    }
                    StatusLine1 = !StatusLine1;

                    StatusConsole.Write("Processes: ");
                    StatusConsole.WriteLine_AsDecimal(ProcessManager.Processes.Count);

                    int ThreadCount = 0;
                    for (int i = 0; i < ProcessManager.Processes.Count; i++)
                    {
                        ThreadCount += ((Process)ProcessManager.Processes[i]).Threads.Count;
                    }
                    StatusConsole.Write("Threads: ");
                    StatusConsole.WriteLine_AsDecimal(ThreadCount);

                    StatusConsole.Write("Devices: ");
                    StatusConsole.WriteLine_AsDecimal(Hardware.DeviceManager.Devices.Count);
                    StatusConsole.Write("File Sys:");
                    for(int i = 0; i < FileSystemManager.FileSystemMappings.Count; i++)
                    {
                        FileSystemMapping mapping = (FileSystemMapping)FileSystemManager.FileSystemMappings[i];
                        StatusConsole.Write(" ");
                        StatusConsole.Write(mapping.Prefix);
                    }
                    StatusConsole.WriteLine();
                    StatusConsole.Write("USB Devices: ");
                    StatusConsole.WriteLine_AsDecimal(Hardware.USB.USBManager.Devices.Count);
                }
                catch
                {
                    MainConsole.ErrorColour();
                    MainConsole.WriteLine("Error updating the status console:");
                    if (ExceptionMethods.CurrentException != null)
                    {
                        MainConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                    }
                    MainConsole.DefaultColour();
                }

                MainConsole.Update();
                StatusConsole.Update();

                Thread.Sleep(500);
            }
        }

        private static void DeviceManager_DeviceAdded(FOS_System.Object state, Hardware.Device aDevice)
        {
            MainConsole.WriteLine("Device added: " + aDevice._Type.Signature);
        }
    }
}
