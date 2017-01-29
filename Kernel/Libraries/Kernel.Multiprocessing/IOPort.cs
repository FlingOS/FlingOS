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

namespace Kernel.IO
{
    /// <summary>
    ///     Represents an IO port.
    /// </summary>
    public static class IOPort
    {
        /// <summary>
        ///     Reads a byte from the specified port.
        /// </summary>
        /// <param name="port">The port to read.</param>
        /// <returns>The value read.</returns>
        [PluggedMethod(ASMFilePath = @"ASM\IO\IOPort\Read")]
        public static byte doRead_Byte(ushort port)
        {
            return 0;
        }

        /// <summary>
        ///     Reads a UInt16 from the specified port.
        /// </summary>
        /// <param name="port">The port to read.</param>
        /// <returns>The value read.</returns>
        [PluggedMethod(ASMFilePath = null)]
        public static ushort doRead_UInt16(ushort port)
        {
            return 0;
        }

        /// <summary>
        ///     Reads a UInt32 from the specified port.
        /// </summary>
        /// <param name="port">The port to read.</param>
        /// <returns>The value read.</returns>
        [PluggedMethod(ASMFilePath = null)]
        public static uint doRead_UInt32(ushort port)
        {
            return 0;
        }

        /// <summary>
        ///     Reads a UInt64 from the specified port.
        /// </summary>
        /// <param name="port">The port to read.</param>
        /// <returns>The value read.</returns>
        [PluggedMethod(ASMFilePath = null)]
        public static ulong doRead_UInt64(ushort port)
        {
            return 0;
        }
        
        /// <summary>
        ///     Writes a byte to the specified port.
        /// </summary>
        /// <param name="port">The port to write to.</param>
        /// <param name="aVal">The value to write.</param>
        [PluggedMethod(ASMFilePath = @"ASM\IO\IOPort\Write")]
        public static void doWrite_Byte(ushort port, byte aVal)
        {
        }

        /// <summary>
        ///     Writes a UInt16 to the specified port.
        /// </summary>
        /// <param name="port">The port to write to.</param>
        /// <param name="aVal">The value to write.</param>
        [PluggedMethod(ASMFilePath = null)]
        public static void doWrite_UInt16(ushort port, ushort aVal)
        {
        }

        /// <summary>
        ///     Writes a UInt32 to the specified port.
        /// </summary>
        /// <param name="port">The port to write to.</param>
        /// <param name="aVal">The value to write.</param>
        [PluggedMethod(ASMFilePath = null)]
        public static void doWrite_UInt32(ushort port, uint aVal)
        {
        }

        /// <summary>
        ///     Writes a UInt64 to the specified port.
        /// </summary>
        /// <param name="port">The port to write to.</param>
        /// <param name="aVal">The value to write.</param>
        [PluggedMethod(ASMFilePath = null)]
        public static void doWrite_UInt64(ushort port, ulong aVal)
        {
        }
        
    }
}