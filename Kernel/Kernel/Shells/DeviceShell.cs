using Kernel.Hardware;
using Kernel.Hardware.ATA;
using Kernel.Hardware.Devices;
using Kernel.Hardware.PCI;
using Kernel.Hardware.USB;
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
                //TODO: These need transferring from KernelTask memory (somehow) before we can add them to device manager
                //      Really they shouldn't be initialised by Kernel Task nor used by Kernel/Debugger Tasks directly.
                //      But they are needed for from-startup/first-instance/fail-proof debugging.
                //DeviceManager.AddDevice(Serial.COM1);
                //DeviceManager.AddDevice(Serial.COM2);
                //DeviceManager.AddDevice(Serial.COM3);
                
                Hardware.Timers.RTC rtc = (Hardware.Timers.RTC)DeviceManager.FindDevice((FOS_System.Type)typeof(Hardware.Timers.RTC));
                if (rtc == null)
                {
                    BasicConsole.WriteLine("RTC is null!");
                }

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
                    BasicConsole.WriteLine("DM > ATA Driver started.");
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
                        console.WriteLine(rtc.GetDateTime().ToString());
                        console.WriteLine("--------------------------");
                        KeyboardKey k = keyboard.ReadKey();
                        switch (k)
                        {
                            //TODO: Get devices system call
                            default:
                                console.WriteLine("No input options available.");
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
    }
}
