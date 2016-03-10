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
                        //TODO: Use GetTime system call console.WriteLine(rtc.GetDateTime().ToString());
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
