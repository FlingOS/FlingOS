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

using System.Runtime.InteropServices;

namespace Kernel.USB
{
    /// <summary>
    ///     The USB Request structure used for sending USB requests under the USB protocol.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct USBRequest
    {
        /// <summary>
        ///     The USB request type.
        /// </summary>
        public byte type;

        /// <summary>
        ///     The specific USB request.
        /// </summary>
        public byte request;

        /// <summary>
        ///     The USB request lo-val.
        /// </summary>
        public byte valueLo;

        /// <summary>
        ///     The USB request hi-val.
        /// </summary>
        public byte valueHi;

        /// <summary>
        ///     The request index.
        /// </summary>
        public ushort index;

        /// <summary>
        ///     The length of the request.
        /// </summary>
        public ushort length;
    }
}