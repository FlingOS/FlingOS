using System;

namespace Kernel.Hardware.Devices
{
    /// <summary>
    /// Represents a timer device.
    /// </summary>
    /// <seealso cref="Kernel.Hardware.Timers.PIT"/>
    public abstract class Timer : Device
    {
        /// <summary>
        /// Whether the timer is enabled or not.
        /// </summary>
        protected bool enabled;
        /// <summary>
        /// Whether the timer is enabled or not.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return enabled;
            }
        }
        
        protected virtual void InterruptHandler()
        {
        }

        public abstract void Enable();
        public abstract void Disable();

        public abstract void Wait(uint ms);
        public abstract void WaitNS(long ns);

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
