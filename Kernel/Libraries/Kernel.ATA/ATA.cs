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

using Kernel.Devices;
using Kernel.Framework;
using Kernel.Framework.Processes.Requests.Devices;

namespace Kernel.ATA
{
    /// <summary>
    ///     Represents any ATA device. This class provides the most
    ///     basic subset of information that every ATA device will
    ///     have.
    /// </summary>
    public abstract class ATA : DiskDevice
    {
        /// <summary>
        ///     The ATA bus positions.
        /// </summary>
        public enum BusPositions
        {
            /// <summary>
            ///     Master device.
            /// </summary>
            Master,

            /// <summary>
            ///     Slave device.
            /// </summary>
            Slave
        }

        /// <summary>
        ///     The ATA controller identifiers.
        /// </summary>
        public enum ControllerIds
        {
            /// <summary>
            ///     Primary ATA controller.
            /// </summary>
            Primary,

            /// <summary>
            ///     Secondary ATA controller.
            /// </summary>
            Secondary
        }

        /// <summary>
        ///     The device's bus position.
        /// </summary>
        public BusPositions BusPosition;

        /// <summary>
        ///     The device's controller ID.
        /// </summary>
        public ControllerIds ControllerId;

        /// <summary>
        ///     Initialises a new ATA device with block size 512.
        /// </summary>
        /// <param name="Name">The human-readable name of the device.</param>
        internal ATA(String Name)
            : base(DeviceGroup.Storage, DeviceClass.Storage, DeviceSubClass.ATA, Name, new uint[2], true)
        {
            blockSize = 512;
        }
    }
}