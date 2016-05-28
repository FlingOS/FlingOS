using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware;
using Kernel.Hardware.ATA;
using Kernel.Hardware.Devices;
using Kernel.Hardware.PCI;
using Kernel.USB;
using Kernel.Hardware.IO.Serial;
using Kernel.FOS_System.Processes;
using Kernel.FOS_System.Processes.Requests.Processes;

namespace Kernel.Shells
{
    public class DeviceShell : Shell
    {
        public DeviceShell()
            : base()
        {
        }
        public DeviceShell(Console AConsole, Keyboard AKeyboard)
            : base(AConsole, AKeyboard)
        {
        }
        
        public override void Execute()
        {
            try
            {                
                if (Tasks.Helpers.StartBuiltInProcess("DM", "ATA Driver", Tasks.Driver.ATADriverTask.Main, false))
                {
                    BasicConsole.WriteLine("DM > Couldn't start the ATA Driver!");
                }
                else
                {
                    BasicConsole.WriteLine("DM > ATA Driver started.");
                }

                if (Tasks.Helpers.StartBuiltInProcess("DM", "PCI Driver", Tasks.Driver.PCIDriverTask.Main, false))
                {
                    BasicConsole.WriteLine("DM > Couldn't start the PCI Driver!");
                }
                else
                {
                    BasicConsole.WriteLine("DM > PCI Driver started.");
                }

                if (Tasks.Helpers.StartBuiltInProcess("DM", "USB Driver", Tasks.Driver.USBDriverTask.Main, false))
                {
                    BasicConsole.WriteLine("DM > Couldn't start the USB Driver!");
                }
                else
                {
                    BasicConsole.WriteLine("DM > USB Driver started.");
                }

                while (!terminating)
                {
                    try
                    {
                        console.WriteLine();
                        //TODO: Use GetTime system call console.WriteLine(rtc.GetDateTime().ToString());
                        console.WriteLine("--------------------------");
                        KeyboardKey k = keyboard.ReadKey();
                        switch (k)
                        {
                            case KeyboardKey.A:
                                OutputDeviceList(DeviceManager.GetDeviceList());
                                break;
                            default:
                                console.WriteLine("Use the following keys:\nA: All");
                                break;
                        }
                    }
                    catch
                    {
                        console.WriteLine("Error in Device Shell!");
                        OutputExceptionInfo(ExceptionMethods.CurrentException);
                        SystemCalls.SleepThread(1000);
                    }
                }
            }
            catch
            {
                OutputExceptionInfo(ExceptionMethods.CurrentException);
                //Pause to give us the chance to read the output. 
                //  We do not know what the code outside this shell may do.
                SystemCalls.SleepThread(1000);
            }
            console.WriteLine("Device shell exited.");
        }

        private void OutputDeviceList(List DeviceList)
        {
            if (DeviceList == null)
            {
                console.WriteLine("Failed to get device list!");
            }
            else
            {
                if (DeviceList.Count == 0)
                {
                    console.WriteLine("No devices in list.");
                }
                else
                {
                    for (int i = 0; i < DeviceList.Count; i++)
                    {
                        Device ADevice = (Device)DeviceList[i];
                        console.WriteLine("Id: " + (String)ADevice.Id);
                        console.Write("Group: ");
                        console.WriteLine_AsDecimal((int)ADevice.Group);
                        console.Write("Class: ");
                        console.WriteLine_AsDecimal((int)ADevice.Class);
                        console.Write("Subclass: ");
                        console.WriteLine_AsDecimal((int)ADevice.SubClass);
                        console.Write("Name: ");
                        console.WriteLine(ADevice.Name);
                        console.Write("Info: ");
                        bool PrintedOne = false;
                        for (int j = 0; j < ADevice.Info.Length; j++)
                        {
                            if (ADevice.Info[j] != 0xFFFFFFFF)
                            {
                                if (PrintedOne)
                                {
                                    console.Write(", ");
                                }
                                console.Write(ADevice.Info[j]);
                                PrintedOne = true;
                            }
                        }
                        console.WriteLine();
                        console.Write("Claimed: ");
                        console.WriteLine(ADevice.Claimed);
                        console.Write("Owner Id: ");
                        console.WriteLine(ADevice.OwnerProcessId);
                        console.WriteLine("------------------------");
                    }
                }
            }
        }
    }
}
