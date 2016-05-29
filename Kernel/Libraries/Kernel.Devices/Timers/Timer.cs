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

using Drivers.Compiler.Attributes;
using Kernel.Framework;
using Kernel.Framework.Processes.Requests.Devices;

namespace Kernel.Devices.Timers
{
    public delegate void TimerHandler(IObject state);

    /// <summary>
    ///     Represents a timer device. Also contains static methods for handling the default timer.
    /// </summary>
    /// <seealso cref="Kernel.Devices.Timers.PIT" />
    public abstract class Timer : Device
    {
        /// <summary>
        ///     The default timer device for the core kernel.
        /// </summary>
        public static Timer Default;

        /// <summary>
        ///     Whether the timer is enabled or not.
        /// </summary>
        protected bool enabled;

        public Timer(DeviceGroup @group, DeviceSubClass subClass, String name, uint[] SomeInfo, bool IsClaimed)
            : base(@group, DeviceClass.Timer, subClass, name, SomeInfo, IsClaimed)
        {
        }

        /// <summary>
        ///     Whether the timer is enabled or not.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
        }

        /// <summary>
        ///     Enables the timer.
        /// </summary>
        public abstract void Enable();

        /// <summary>
        ///     Disables the timer.
        /// </summary>
        public abstract void Disable();

        /// <summary>
        ///     Blocks the caller for the specified number of milliseconds.
        /// </summary>
        /// <param name="ms">The number of milliseconds to block.</param>
        public abstract void Wait(uint ms);

        /// <summary>
        ///     Blocks the caller for the specified number of nanoseconds.
        /// </summary>
        /// <param name="ns">The number of nanoseconds to block.</param>
        public abstract void WaitNS(long ns);

        [NoDebug]
        public abstract int RegisterHandler(TimerHandler handler, long TimeoutNS, bool Recurring, IObject state);

        public abstract void UnregisterHandler(int handlerId);
    }
}