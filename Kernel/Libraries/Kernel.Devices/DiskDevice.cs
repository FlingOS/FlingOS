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

using Kernel.Framework;
using Kernel.Framework.Processes.Requests.Devices;

namespace Kernel.Devices
{
    /// <summary>
    ///     Represents a disk device.
    /// </summary>
    public abstract class DiskDevice : BlockDevice
    {
        public DiskDevice(DeviceGroup Group, DeviceClass Class, DeviceSubClass SubClass, String Name,
            uint[] SomeInfo, bool IsClaimed)
            : base(Group, Class, SubClass, Name, SomeInfo, IsClaimed)
        {
        }

        /// <summary>
        ///     Cleans the software and hardware caches (if any) by writing necessary data
        ///     to disk before wiping the caches.
        /// </summary>
        public abstract void CleanCaches();
    }
}