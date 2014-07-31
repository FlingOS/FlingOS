using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.Keyboards
{
    public class PS2 : Devices.Keyboard
    {
        protected IO.IOPort DataPort = new IO.IOPort(0x60);
        protected int InterruptHandlerId;
        
        public PS2()
            : base()
        {
        }

        public override void Enable()
        {
            if (!enabled)
            {
                InterruptHandlerId = Interrupts.Interrupts.SetIRQHandler(1, InterruptHandler, this);
                DeviceManager.Devices.Add(this);
                enabled = true;
            }
        }
        public override void Disable()
        {
            if (enabled)
            {
                DeviceManager.Devices.Remove(this);
                Interrupts.Interrupts.RemoveIRQHandler(1, InterruptHandlerId);
                enabled = false;
            }
        }
        
        private static void InterruptHandler(object data)
        {
            ((PS2)data).InterruptHandler();
        }
        private void InterruptHandler()
        {
            byte scanCode = DataPort.Read_Byte();
            bool released = (scanCode & 0x80) == 0x80;
            if (released)
            {
                scanCode = (byte)(scanCode ^ 0x80);
            }
            HandleScancode(scanCode, released);
        }
        private void HandleScancode(byte scancode, bool released)
        {
            uint theScancode = scancode;
            if (mEscaped)
            {
                theScancode = (ushort)(theScancode << 8);
                mEscaped = false;
            }
            switch (theScancode)
            {
                case 0x36:
                case 0x2A:
                    {
                        mShiftState = !released;
                        break;
                    }
                case 0x1D:
                    {
                        mCtrlState = !released;
                        break;
                    }
                case 0x38:
                    {
                        mAltState = !released;
                        break;
                    }
                default:
                    {
                        if ((mCtrlState) && (mAltState) && (theScancode == 0x53))
                        {
                            //TODO: Remove this Ctrl+Alt+Delete hack
                            Console.WriteLine("Detected Ctrl-Alt-Delete! Disabling keyboard.");
                        }
                        if (mShiftState)
                        {
                            theScancode = theScancode << 16;
                        }
                        if (!released)
                        {
                            Enqueue(theScancode);
                        }
                        break;
                    }
            }
        }

        public static PS2 ThePS2 = null;
        public static void Init()
        {
            if (ThePS2 == null)
            {
                ThePS2 = new PS2();
            }
            ThePS2.Enable();
        }
        public static void Clean()
        {
            if(ThePS2 != null)
            {
                ThePS2.Disable();
            }
        }
    }
}
