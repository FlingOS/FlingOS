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
using Kernel.Framework.Exceptions;
using Kernel.Framework.Processes.Requests.Devices;

namespace Kernel.ATA
{
    /// <summary>
    ///     Represents a SATAPI driver. Nothing is implemented yet so this is just a stub.
    /// </summary>
    public class SATAPI : DiskDevice
    {
        /// <summary>
        ///     Initialises a new stub SATAPI driver.
        /// </summary>
        public SATAPI()
            : base(DeviceGroup.Storage, DeviceClass.Storage, DeviceSubClass.ATA, "SATAPI Disk", new uint[0], true)
        {
        }

        /// <summary>
        ///     This is only a stub driver at the moment so this method throws a not supported exception.
        /// </summary>
        /// <param name="BlockNo">Unused</param>
        /// <param name="BlockCount">Unused</param>
        /// <param name="Data">Unused</param>
        public override void ReadBlock(ulong BlockNo, uint BlockCount, byte[] Data)
            => ExceptionMethods.Throw(new NotSupportedException("SATAPI driver has not been implemented yet."));

        /// <summary>
        ///     This is only a stub driver at the moment so this method throws a not supported exception.
        /// </summary>
        /// <param name="BlockNo">Unused</param>
        /// <param name="BlockCount">Unused</param>
        /// <param name="Data">Unused</param>
        public override void WriteBlock(ulong BlockNo, uint BlockCount, byte[] Data)
            => ExceptionMethods.Throw(new NotSupportedException("SATAPI driver has not been implemented yet."));

        /// <summary>
        ///     This is only a stub driver at the moment so this method does nothing.
        /// </summary>
        /// <remarks>
        ///     The choice to do nothing rather than throw an exception is deliberate. Management code
        ///     higher up may iterate all known disk devices to clean all caches before performing a
        ///     shut down. The management code shouldn't have to worry about devices which don't clean
        ///     anything because there is nothing to clean!
        /// </remarks>
        public override void CleanCaches()
        {
        }
    }
}