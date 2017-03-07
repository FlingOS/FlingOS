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
using Kernel.Framework.Collections;
using Kernel.Framework.Processes.Requests.Devices;
using Kernel.Framework.Exceptions;
using Kernel.Framework.Processes;
using Kernel.Multiprocessing;

namespace Kernel.Devices
{
    /// <summary>
    ///     Represents an IO port.
    /// </summary>
    public class IOPort : Device
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
            : this(aPort, 0)
        {
        }

        /// <summary>
        ///     Initialises a new IO port with specified port number and
        ///     offset from that port number.
        /// </summary>
        /// <param name="aBase">The port base number.</param>
        /// <param name="anOffset">The offset from the base port number.</param>
        [NoDebug]
        public IOPort(ushort aBase, ushort anOffset)
            : base(DeviceGroup.System, DeviceClass.IO, DeviceSubClass.Port, "IOPort", new uint[] { (uint)(aBase + anOffset) }, false)
        {
            Port = (ushort)(aBase + anOffset);
            Id = (ulong)(Port + 1);

            //BasicConsole.WriteLine((String)"IOPort: Creating port: " + Port);
            if (ProcessManager.CurrentProcess != null)
            {
                // Note: Assumptions: Serial ports were registered first, serial ports have id == portnum+1
                if (!DeviceManager.ClaimDevice(this))
                {
                    ExceptionMethods.Throw(new NotSupportedException((String)"Port already claimed! Port: " + Port));
                }
            }
        }

        
        /// <summary>
        ///     Reads a byte from the port.
        /// </summary>
        /// <returns>The value read.</returns>
        [NoDebug]
        [NoGC]
        public byte Read_Byte()
        {
            return IO.IOPort.doRead_Byte(Port);
        }

        /// <summary>
        ///     Reads a UInt16 from the port.
        /// </summary>
        /// <returns>The value read.</returns>
        [NoDebug]
        [NoGC]
        public ushort Read_UInt16()
        {
            return IO.IOPort.doRead_UInt16(Port);
        }

        /// <summary>
        ///     Reads a UInt32 from the port.
        /// </summary>
        /// <returns>The value read.</returns>
        [NoDebug]
        [NoGC]
        public uint Read_UInt32()
        {
            return IO.IOPort.doRead_UInt32(Port);
        }

        /// <summary>
        ///     Reads a UInt64 from the port.
        /// </summary>
        /// <returns>The value read.</returns>
        [NoDebug]
        [NoGC]
        public ulong Read_UInt64()
        {
            return IO.IOPort.doRead_UInt64(Port);
        }

        /// <summary>
        ///     Reads a byte from the port at the specified offset from this port.
        /// </summary>
        /// <returns>The value read.</returns>
        [NoDebug]
        [NoGC]
        public byte Read_Byte(ushort offset)
        {
            return IO.IOPort.doRead_Byte((ushort)(Port + offset));
        }

        /// <summary>
        ///     Reads a UInt16 from the port at the specified offset from this port.
        /// </summary>
        /// <returns>The value read.</returns>
        [NoDebug]
        [NoGC]
        public ushort Read_UInt16(ushort offset)
        {
            return IO.IOPort.doRead_UInt16((ushort)(Port + offset));
        }

        /// <summary>
        ///     Reads a UInt32 from the port at the specified offset from this port.
        /// </summary>
        /// <returns>The value read.</returns>
        [NoDebug]
        [NoGC]
        public uint Read_UInt32(ushort offset)
        {
            return IO.IOPort.doRead_UInt32((ushort)(Port + offset));
        }

        /// <summary>
        ///     Reads a UInt64 from the port at the specified offset from this port.
        /// </summary>
        /// <returns>The value read.</returns>
        [NoDebug]
        [NoGC]
        public ulong Read_UInt64(ushort offset)
        {
            return IO.IOPort.doRead_UInt64((ushort)(Port + offset));
        }


        
        /// <summary>
        ///     Writes a byte to the port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [NoDebug]
        [NoGC]
        public virtual void Write_Byte(byte aVal)
        {
            IO.IOPort.doWrite_Byte(Port, aVal);
        }

        /// <summary>
        ///     Writes a UInt16 to the port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [NoDebug]
        [NoGC]
        public void Write_UInt16(ushort aVal)
        {
            IO.IOPort.doWrite_UInt16(Port, aVal);
        }

        /// <summary>
        ///     Writes a UInt32 to the port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [NoDebug]
        [NoGC]
        public void Write_UInt32(uint aVal)
        {
            IO.IOPort.doWrite_UInt32(Port, aVal);
        }

        /// <summary>
        ///     Writes a UInt64 to the port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [NoDebug]
        [NoGC]
        public void Write_UInt64(ulong aVal)
        {
            IO.IOPort.doWrite_UInt64(Port, aVal);
        }

        /// <summary>
        ///     Writes a byte to the port at the specified offset from this port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [NoDebug]
        [NoGC]
        public virtual void Write_Byte(byte aVal, ushort offset)
        {
            IO.IOPort.doWrite_Byte((ushort)(Port + offset), aVal);
        }

        /// <summary>
        ///     Writes a UInt16 to the port at the specified offset from this port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [NoDebug]
        [NoGC]
        public virtual void Write_UInt16(ushort aVal, ushort offset)
        {
            IO.IOPort.doWrite_UInt16((ushort)(Port + offset), aVal);
        }

        /// <summary>
        ///     Writes a UInt32 to the port at the specified offset from this port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [NoDebug]
        [NoGC]
        public virtual void Write_UInt32(uint aVal, ushort offset)
        {
            IO.IOPort.doWrite_UInt32((ushort)(Port + offset), aVal);
        }

        /// <summary>
        ///     Writes a UInt64 to the port at the specified offset from this port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [NoDebug]
        [NoGC]
        public virtual void Write_UInt64(ulong aVal, ushort offset)
        {
            IO.IOPort.doWrite_UInt64((ushort)(Port + offset), aVal);
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