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
    public class IOPort : Object
    {
        /// <summary>
        ///     The port number.
        /// </summary>
        public readonly ushort Port;

        /// <summary>
        ///     Initialises a new IO port with specified port number.
        /// </summary>
        /// <param name="aPort">The port number.</param>
        [NoDebug]
        public IOPort(ushort aPort)
        {
            Port = aPort;
        }

        /// <summary>
        ///     Initialises a new IO port with specified port number and
        ///     offset from that port number.
        /// </summary>
        /// <param name="aBase">The port base number.</param>
        /// <param name="anOffset">The offset from the base port number.</param>
        [NoDebug]
        public IOPort(ushort aBase, ushort anOffset)
        {
            Port = (ushort)(aBase + anOffset);
        }

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
        ///     Reads a byte from the port.
        /// </summary>
        /// <returns>The value read.</returns>
        [NoDebug]
        [NoGC]
        public byte Read_Byte()
        {
            return doRead_Byte(Port);
        }

        /// <summary>
        ///     Reads a UInt16 from the port.
        /// </summary>
        /// <returns>The value read.</returns>
        [NoDebug]
        [NoGC]
        public ushort Read_UInt16()
        {
            return doRead_UInt16(Port);
        }

        /// <summary>
        ///     Reads a UInt32 from the port.
        /// </summary>
        /// <returns>The value read.</returns>
        [NoDebug]
        [NoGC]
        public uint Read_UInt32()
        {
            return doRead_UInt32(Port);
        }

        /// <summary>
        ///     Reads a UInt64 from the port.
        /// </summary>
        /// <returns>The value read.</returns>
        [NoDebug]
        [NoGC]
        public ulong Read_UInt64()
        {
            return doRead_UInt64(Port);
        }

        /// <summary>
        ///     Reads a byte from the port at the specified offset from this port.
        /// </summary>
        /// <returns>The value read.</returns>
        [NoDebug]
        [NoGC]
        public byte Read_Byte(ushort offset)
        {
            return doRead_Byte((ushort)(Port + offset));
        }

        /// <summary>
        ///     Reads a UInt16 from the port at the specified offset from this port.
        /// </summary>
        /// <returns>The value read.</returns>
        [NoDebug]
        [NoGC]
        public ushort Read_UInt16(ushort offset)
        {
            return doRead_UInt16((ushort)(Port + offset));
        }

        /// <summary>
        ///     Reads a UInt32 from the port at the specified offset from this port.
        /// </summary>
        /// <returns>The value read.</returns>
        [NoDebug]
        [NoGC]
        public uint Read_UInt32(ushort offset)
        {
            return doRead_UInt32((ushort)(Port + offset));
        }

        /// <summary>
        ///     Reads a UInt64 from the port at the specified offset from this port.
        /// </summary>
        /// <returns>The value read.</returns>
        [NoDebug]
        [NoGC]
        public ulong Read_UInt64(ushort offset)
        {
            return doRead_UInt64((ushort)(Port + offset));
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

        /// <summary>
        ///     Writes a byte to the port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [NoDebug]
        [NoGC]
        public virtual void Write_Byte(byte aVal)
        {
            doWrite_Byte(Port, aVal);
        }

        /// <summary>
        ///     Writes a UInt16 to the port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [NoDebug]
        [NoGC]
        public void Write_UInt16(ushort aVal)
        {
            doWrite_UInt16(Port, aVal);
        }

        /// <summary>
        ///     Writes a UInt32 to the port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [NoDebug]
        [NoGC]
        public void Write_UInt32(uint aVal)
        {
            doWrite_UInt32(Port, aVal);
        }

        /// <summary>
        ///     Writes a UInt64 to the port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [NoDebug]
        [NoGC]
        public void Write_UInt64(ulong aVal)
        {
            doWrite_UInt64(Port, aVal);
        }

        /// <summary>
        ///     Writes a byte to the port at the specified offset from this port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [NoDebug]
        [NoGC]
        public virtual void Write_Byte(byte aVal, ushort offset)
        {
            doWrite_Byte((ushort)(Port + offset), aVal);
        }

        /// <summary>
        ///     Writes a UInt16 to the port at the specified offset from this port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [NoDebug]
        [NoGC]
        public virtual void Write_UInt16(ushort aVal, ushort offset)
        {
            doWrite_UInt16((ushort)(Port + offset), aVal);
        }

        /// <summary>
        ///     Writes a UInt32 to the port at the specified offset from this port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [NoDebug]
        [NoGC]
        public virtual void Write_UInt32(uint aVal, ushort offset)
        {
            doWrite_UInt32((ushort)(Port + offset), aVal);
        }

        /// <summary>
        ///     Writes a UInt64 to the port at the specified offset from this port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [NoDebug]
        [NoGC]
        public virtual void Write_UInt64(ulong aVal, ushort offset)
        {
            doWrite_UInt64((ushort)(Port + offset), aVal);
        }

        /// <summary>
        ///     Reads bytes into the specified byte array.
        /// </summary>
        /// <param name="aData">The byte array to read data into.</param>
        [NoDebug]
        [NoGC]
        public void Read_Bytes(byte[] aData)
        {
            ushort xValue;
            for (int i = 0; i < aData.Length/2; i++)
            {
                xValue = Read_UInt16();
                aData[i*2] = (byte)xValue;
                aData[i*2 + 1] = (byte)(xValue >> 8);
            }
        }

        /// <summary>
        ///     Reads UInt16s into the specified UInt16 array.
        /// </summary>
        /// <param name="aData">The UInt16 array to read data into.</param>
        [NoDebug]
        [NoGC]
        public void Read_UInt16s(ushort[] aData)
        {
            for (int i = 0; i < aData.Length; i++)
            {
                aData[i] = Read_UInt16();
            }
        }

        /// <summary>
        ///     Reads UInt32s into the specified UInt32 array.
        /// </summary>
        /// <param name="aData">The UInt32 array to read data into.</param>
        [NoDebug]
        [NoGC]
        public void Read_UInt32s(uint[] aData)
        {
            for (int i = 0; i < aData.Length; i++)
            {
                aData[i] = Read_UInt32();
            }
        }

        /// <summary>
        ///     Writes UInt16s from the specified byte array. Length of byte array must be multiple of 2.
        /// </summary>
        /// <param name="aData">The byte array to write data from.</param>
        [NoDebug]
        [NoGC]
        public void Write_UInt16s(byte[] aData)
        {
            for (int i = 0; i < aData.Length; i += 2)
            {
                Write_UInt16((ushort)(aData[i] | (aData[i + 1] << 8)));
            }
        }

        /// <summary>
        ///     Writes UInt16s from the specified UInt16 array.
        /// </summary>
        /// <param name="aData">The UInt16 array to write data from.</param>
        [NoDebug]
        [NoGC]
        public void Write_UInt16s(ushort[] aData)
        {
            for (int i = 0; i < aData.Length; i++)
            {
                Write_UInt16(aData[i]);
            }
        }
    }
}