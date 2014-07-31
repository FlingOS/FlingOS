using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.Keyboards
{
    public class PS2 : Devices.Keyboard
    {
        IO.IOPort Data0 = new IO.IOPort(0x60);

        public PS2()
        {
            Interrupts.Interrupts.SetIRQHandler(1, InterruptHandler, this);
            DeviceManager.Devices.Add(this);
        }

        private static void InterruptHandler(object data)
        {
            ((PS2)data).InterruptHandler();
        }
        private void InterruptHandler()
        {
            //Read the key
            char keyChar = (char)Data0.Read_Byte();
            //Output the key for now
            BasicConsole.Write(keyChar);
        }

        public static PS2 ThePS2 = null;
        public static void Init()
        {
            if (ThePS2 == null)
            {
                ThePS2 = new PS2();
            }
        }
    }
}
