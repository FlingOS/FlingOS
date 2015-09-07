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
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.IO
{
    /// <summary>
    /// Represents an IO port.
    /// </summary>
    [Compiler.PluggedClass]
    public class IOPort : FOS_System.Object
    {
        /// <summary>
        /// The port number.
        /// </summary>
        public readonly UInt16 Port;
        
        /// <summary>
        /// Initialises a new IO port with specified port number.
        /// </summary>
        /// <param name="aPort">The port number.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        public IOPort(UInt16 aPort)
        {
            Port = aPort;
        }
        /// <summary>
        /// Initialises a new IO port with specified port number and 
        /// offset from that port number.
        /// </summary>
        /// <param name="aBase">The port base number.</param>
        /// <param name="anOffset">The offset from the base port number.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        public IOPort(UInt16 aBase, UInt16 anOffset)
        {
            Port = (UInt16)(aBase + anOffset);
        }

        /// <summary>
        /// Reads a byte from the specified port.
        /// </summary>
        /// <param name="port">The port to read.</param>
        /// <returns>The value read.</returns>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\IO\IOPort\Read")]
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = @"ASM\IO\IOPort\Read")]
        public static byte doRead_Byte(UInt16 port)
        {
            return 0;
        }
        /// <summary>
        /// Reads a UInt16 from the specified port.
        /// </summary>
        /// <param name="port">The port to read.</param>
        /// <returns>The value read.</returns>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static UInt16 doRead_UInt16(UInt16 port)
        {
            return 0;
        }
        /// <summary>
        /// Reads a UInt32 from the specified port.
        /// </summary>
        /// <param name="port">The port to read.</param>
        /// <returns>The value read.</returns>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static UInt32 doRead_UInt32(UInt16 port)
        {
            return 0;
        }
        /// <summary>
        /// Reads a UInt64 from the specified port.
        /// </summary>
        /// <param name="port">The port to read.</param>
        /// <returns>The value read.</returns>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static UInt64 doRead_UInt64(UInt16 port)
        {
            return 0;
        }

        /// <summary>
        /// Reads a byte from the port.
        /// </summary>
        /// <returns>The value read.</returns>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public byte Read_Byte()
        {
            return doRead_Byte(this.Port);
        }
        /// <summary>
        /// Reads a UInt16 from the port.
        /// </summary>
        /// <returns>The value read.</returns>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public UInt16 Read_UInt16()
        {
            return doRead_UInt16(this.Port);
        }
        /// <summary>
        /// Reads a UInt32 from the port.
        /// </summary>
        /// <returns>The value read.</returns>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public UInt32 Read_UInt32()
        {
            return doRead_UInt32(this.Port);
        }
        /// <summary>
        /// Reads a UInt64 from the port.
        /// </summary>
        /// <returns>The value read.</returns>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public UInt64 Read_UInt64()
        {
            return doRead_UInt64(this.Port);
        }

        /// <summary>
        /// Reads a byte from the port at the specified offset from this port.
        /// </summary>
        /// <returns>The value read.</returns>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public UInt16 Read_Byte(UInt16 offset)
        {
            return doRead_Byte((UInt16)(this.Port + offset));
        }
        /// <summary>
        /// Reads a UInt16 from the port at the specified offset from this port.
        /// </summary>
        /// <returns>The value read.</returns>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public UInt16 Read_UInt16(UInt16 offset)
        {
            return doRead_UInt16((UInt16)(this.Port + offset));
        }
        /// <summary>
        /// Reads a UInt32 from the port at the specified offset from this port.
        /// </summary>
        /// <returns>The value read.</returns>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public UInt32 Read_UInt32(UInt16 offset)
        {
            return doRead_UInt32((UInt16)(this.Port + offset));
        }
        /// <summary>
        /// Reads a UInt64 from the port at the specified offset from this port.
        /// </summary>
        /// <returns>The value read.</returns>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public UInt64 Read_UInt64(UInt16 offset)
        {
            return doRead_UInt64((UInt16)(this.Port + offset));
        }
        

        /// <summary>
        /// Writes a byte to the specified port.
        /// </summary>
        /// <param name="port">The port to write to.</param>
        /// <param name="aVal">The value to write.</param>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\IO\IOPort\Write")]
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = @"ASM\IO\IOPort\Write")]
        public static void doWrite_Byte(UInt16 port, byte aVal)
        {
        }
        /// <summary>
        /// Writes a UInt16 to the specified port.
        /// </summary>
        /// <param name="port">The port to write to.</param>
        /// <param name="aVal">The value to write.</param>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static void doWrite_UInt16(UInt16 port, UInt16 aVal)
        {
        }
        /// <summary>
        /// Writes a UInt32 to the specified port.
        /// </summary>
        /// <param name="port">The port to write to.</param>
        /// <param name="aVal">The value to write.</param>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static void doWrite_UInt32(UInt16 port, UInt32 aVal)
        {
        }
        /// <summary>
        /// Writes a UInt64 to the specified port.
        /// </summary>
        /// <param name="port">The port to write to.</param>
        /// <param name="aVal">The value to write.</param>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static void doWrite_UInt64(UInt16 port, UInt64 aVal)
        {
        }

        /// <summary>
        /// Writes a byte to the port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public virtual void Write_Byte(byte aVal)
        {
            doWrite_Byte(this.Port, aVal);
        }
        /// <summary>
        /// Writes a UInt16 to the port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public void Write_UInt16(UInt16 aVal)
        {
            doWrite_UInt16(this.Port, aVal);
        }
        /// <summary>
        /// Writes a UInt32 to the port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public void Write_UInt32(UInt32 aVal)
        {
            doWrite_UInt32(this.Port, aVal);
        }
        /// <summary>
        /// Writes a UInt64 to the port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public void Write_UInt64(UInt64 aVal)
        {
            doWrite_UInt64(this.Port, aVal);
        }

        /// <summary>
        /// Writes a byte to the port at the specified offset from this port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public virtual void Write_Byte(byte aVal, UInt16 offset)
        {
            doWrite_Byte((UInt16)(this.Port + offset), aVal);
        }
        /// <summary>
        /// Writes a UInt16 to the port at the specified offset from this port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public virtual void Write_UInt16(UInt16 aVal, UInt16 offset)
        {
            doWrite_UInt16((UInt16)(this.Port + offset), aVal);
        }
        /// <summary>
        /// Writes a UInt32 to the port at the specified offset from this port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public virtual void Write_UInt32(UInt32 aVal, UInt16 offset)
        {
            doWrite_UInt32((UInt16)(this.Port + offset), aVal);
        }
        /// <summary>
        /// Writes a UInt64 to the port at the specified offset from this port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public virtual void Write_UInt64(UInt64 aVal, UInt16 offset)
        {
            doWrite_UInt64((UInt16)(this.Port + offset), aVal);
        }

        /// <summary>
        /// Reads bytes into the specified byte array.
        /// </summary>
        /// <param name="aData">The byte array to read data into.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public void Read_Bytes(byte[] aData)
        {
            UInt16 xValue;
            for (int i = 0; i < aData.Length / 2; i++)
            {
                xValue = Read_UInt16();
                aData[i * 2] = (byte)xValue;
                aData[i * 2 + 1] = (byte)(xValue >> 8);
            }
        }
        /// <summary>
        /// Reads UInt16s into the specified UInt16 array.
        /// </summary>
        /// <param name="aData">The UInt16 array to read data into.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public void Read_UInt16s(UInt16[] aData)
        {
            for (int i = 0; i < aData.Length; i++)
            {
                aData[i] = Read_UInt16();
            }
        }
        /// <summary>
        /// Reads UInt32s into the specified UInt32 array.
        /// </summary>
        /// <param name="aData">The UInt32 array to read data into.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public void Read_UInt32s(UInt32[] aData)
        {
            for (int i = 0; i < aData.Length; i++)
            {
                aData[i] = Read_UInt32();
            }
        }

        /// <summary>
        /// Writes UInt16s from the specified byte array. Length of byte array must be multiple of 2.
        /// </summary>
        /// <param name="aData">The byte array to write data from.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public void Write_UInt16s(byte[] aData)
        {
            for (int i = 0; i < aData.Length; i += 2)
            {
                Write_UInt16((UInt16)(aData[i] | (aData[i+1] << 8)));
            }
        }
        /// <summary>
        /// Writes UInt16s from the specified UInt16 array.
        /// </summary>
        /// <param name="aData">The UInt16 array to write data from.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public void Write_UInt16s(UInt16[] aData)
        {
            for (int i = 0; i < aData.Length; i++)
            {
                Write_UInt16(aData[i]);
            }
        }
    }
}
