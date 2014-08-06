using System;

namespace Kernel.Hardware.Devices
{
    /// <summary>
    /// Represents a timer device. Also contains static methods for handling the default timer.
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
        
        /// <summary>
        /// Enables the timer.
        /// </summary>
        public abstract void Enable();
        /// <summary>
        /// Disables the timer.
        /// </summary>
        public abstract void Disable();

        /// <summary>
        /// Blocks the caller for the specified number of milliseconds.
        /// </summary>
        /// <param name="ms">The number of milliseconds to block.</param>
        public abstract void Wait(uint ms);
        /// <summary>
        /// Blocks the calller for the specified nuymber of nanoseconds.
        /// </summary>
        /// <param name="ns">The number of nanoseconds to block.</param>
        public abstract void WaitNS(long ns);

        /// <summary>
        /// The default timer device for the core kernel.
        /// </summary>
        public static Timer Default;
        /// <summary>
        /// Initialises the default timer including enabling it.
        /// </summary>
        public static void InitDefault()
        {
            Timers.PIT.Init();
            Default = Timers.PIT.ThePIT;
        }
        /// <summary>
        /// Cleans up the default timer including disabling it.
        /// </summary>
        public static void CleanDefault()
        {
            if (Default != null)
            {
                Default.Disable();
            }
        }
    }
}
