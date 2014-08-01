using System;

namespace Kernel.Hardware.Devices
{
    public abstract class Timer : Device
    {
        protected bool enabled;
        public bool Enabled
        {
            get
            {
                return enabled;
            }
        }

        public abstract ulong CurrentTime
        {
            get;
            protected set;
        }

        protected virtual void InterruptHandler()
        {
        }

        public abstract void Enable();
        public abstract void Disable();

        public abstract void Wait(uint ms);
        public abstract void WaitNS(ulong ns);

        public static Timer Default;
        public static void InitDefault()
        {
            Timers.PIT.Init();
            Default = Timers.PIT.ThePIT;
        }
        public static void CleanDefault()
        {
            if (Default != null)
            {
                Default.Disable();
            }
        }
    }
}
