#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
using System;

namespace Kernel.Hardware.Devices
{
    public delegate void TimerHandler(FOS_System.Object state);

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
        /// Blocks the caller for the specified number of nanoseconds.
        /// </summary>
        /// <param name="ns">The number of nanoseconds to block.</param>
        public abstract void WaitNS(long ns);

        public abstract int RegisterHandler(TimerHandler handler, long TimeoutNS, bool Recurring, FOS_System.Object state);
        public abstract void UnregisterHandler(int handlerId);

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
