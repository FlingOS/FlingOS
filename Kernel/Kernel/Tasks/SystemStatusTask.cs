#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //
#endregion
    
using System;
using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.IO;
using Kernel.Hardware.Processes;

namespace Kernel.Tasks
{
    public static class SystemStatusTask
    {
        internal static Console MainConsole;
        internal static Console StatusConsole;

        private static bool Terminating = false;

        public static void Main()
        {
            BasicConsole.Clear();
            Console.InitDefault();

            // Wait for other system startup to occur
            Processes.SystemCalls.SleepThread(1000);
            
            MainConsole = new Consoles.AdvancedConsole();
            MainConsole.ScreenHeight = 7;
            MainConsole.LineLength = 55;
            MainConsole.ScreenStartLineOffset = 0;
            MainConsole.UpdateScreenCursor = false;

            StatusConsole = new Consoles.AdvancedConsole();
            StatusConsole.ScreenHeight = 7;
            StatusConsole.LineLength = 24;
            StatusConsole.ScreenStartLineOffset = 56;
            StatusConsole.UpdateScreenCursor = false;

            MainConsole.Clear();
            StatusConsole.Clear();

            bool StatusLine1 = true;

            Hardware.DeviceManager.AddDeviceAddedListener(DeviceManager_DeviceAdded, null);

            Processes.SystemCalls.SleepThread(500);
            Console.Default.ScreenHeight = 25 - 8;
            Console.Default.ScreenStartLine = 8;

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
                    int SleptThreads = 0;
                    int IndefiniteSleptThreads = 0;
                    for (int i = 0; i < ProcessManager.Processes.Count; i++)
                    {
                        List threads = ((Process)ProcessManager.Processes[i]).Threads;
                        ThreadCount += threads.Count;
                        for (int j = 0; j < threads.Count; j++)
                        {
                            Thread thread = (Thread)threads[j];
                            if (thread.TimeToSleep == Thread.IndefiniteSleep)
                            {
                                IndefiniteSleptThreads++;
                                SleptThreads++;
                            }
                            else if (thread.TimeToSleep > 0)
                            {
                                SleptThreads++;
                            }
                        }
                    }
                    StatusConsole.Write("Threads: ");
                    StatusConsole.Write_AsDecimal(ThreadCount);
                    StatusConsole.Write(" / ");
                    StatusConsole.Write_AsDecimal(SleptThreads);
                    StatusConsole.Write(" / ");
                    StatusConsole.WriteLine_AsDecimal(IndefiniteSleptThreads);

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

                    unsafe
                    {
                        StatusConsole.Write("Heap: ");
                        //StatusConsole.Write_AsDecimal(Heap.GetTotalUsedMem());
                        //StatusConsole.Write(" / ");
                        //StatusConsole.Write_AsDecimal(Heap.GetTotalMem());
                        uint totalMem = Heap.GetTotalMem();
                        StatusConsole.Write_AsDecimal(Heap.GetTotalUsedMem() / (totalMem / 100));
                        StatusConsole.Write("% / ");
                        StatusConsole.Write_AsDecimal(totalMem / 1024);
                        StatusConsole.Write(" KiB");
                    }
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

                Processes.SystemCalls.SleepThread(500);
            }
        }

        private static void DeviceManager_DeviceAdded(FOS_System.Object state, Hardware.Device aDevice)
        {
            MainConsole.WriteLine("Device added: " + aDevice._Type.Signature);
        }
    }
}
