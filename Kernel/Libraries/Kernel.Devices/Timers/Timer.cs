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

        /// <summary>
        ///     Whether the timer is enabled or not.
        /// </summary>
        public bool Enabled => enabled;

        /// <summary>
        ///     Initialises a Timer instance with the specified device info.
        /// </summary>
        /// <param name="Group">The device group of the timer.</param>
        /// <param name="SubClass">The device sub class of the timer.</param>
        /// <param name="Name">The human-readable name of the timer.</param>
        /// <param name="SomeInfo">Device-specific information to store with the timer.</param>
        /// <param name="IsClaimed">Whether the timer has already been claimed by a driver or not.</param>
        protected Timer(DeviceGroup Group, DeviceSubClass SubClass, String Name, uint[] SomeInfo, bool IsClaimed)
            : base(Group, DeviceClass.Timer, SubClass, Name, SomeInfo, IsClaimed)
        {
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

        /// <summary>
        ///     Registers a handler for a timer event to occur after the specified timeout. The timeout can occur repeatedly. 
        /// </summary>
        /// <param name="Handler">The handler to invoke when the timer event occurs.</param>
        /// <param name="TimeoutNS">The time after which the timer event should occur.</param>
        /// <param name="Recurring">Whether the timer event should repeat after every Timeout period.</param>
        /// <param name="State">A state object to pass the handler when the timer event occurs.</param>
        /// <returns>The Id of the registered handler or -1 if the handler did not register.</returns>
        [NoDebug]
        public abstract int RegisterHandler(TimerHandler Handler, long TimeoutNS, bool Recurring, IObject State);

        /// <summary>
        ///     Removes the specified handler from the timer's list of handlers. The associated timer event will
        ///     not occur while the handler is not registered.
        /// </summary>
        /// <param name="HandlerId">The Id of the registered handler.</param>
        public abstract void UnregisterHandler(int HandlerId);
    }
}