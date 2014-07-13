#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
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
        protected internal IOPort(UInt16 aPort)
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
        protected internal IOPort(UInt16 aBase, UInt16 anOffset)
        {
            Port = (UInt16)(aBase + anOffset);
        }

        /// <summary>
        /// Reads a byte from the specified port.
        /// </summary>
        /// <param name="port">The port to read.</param>
        /// <returns>The value read.</returns>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\IO\IOPort\Read")]
        private static byte doRead_Byte(UInt16 port)
        {
            return 0;
        }
        /// <summary>
        /// Reads a UInt16 from the specified port.
        /// </summary>
        /// <param name="port">The port to read.</param>
        /// <returns>The value read.</returns>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static UInt16 doRead_UInt16(UInt16 port)
        {
            return 0;
        }
        /// <summary>
        /// Reads a UInt32 from the specified port.
        /// </summary>
        /// <param name="port">The port to read.</param>
        /// <returns>The value read.</returns>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static UInt32 doRead_UInt32(UInt16 port)
        {
            return 0;
        }
        /// <summary>
        /// Reads a UInt64 from the specified port.
        /// </summary>
        /// <param name="port">The port to read.</param>
        /// <returns>The value read.</returns>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static UInt64 doRead_UInt64(UInt16 port)
        {
            return 0;
        }

        /// <summary>
        /// Reads a byte from the port.
        /// </summary>
        /// <returns>The value read.</returns>
        [Compiler.NoDebug]
        public byte Read_Byte()
        {
            return doRead_Byte(this.Port);
        }
        /// <summary>
        /// Reads a UInt16 from the port.
        /// </summary>
        /// <returns>The value read.</returns>
        [Compiler.NoDebug]
        public UInt16 Read_UInt16()
        {
            return doRead_UInt16(this.Port);
        }
        /// <summary>
        /// Reads a UInt32 from the port.
        /// </summary>
        /// <returns>The value read.</returns>
        [Compiler.NoDebug]
        public UInt32 Read_UInt32()
        {
            return doRead_UInt32(this.Port);
        }
        /// <summary>
        /// Reads a UInt64 from the port.
        /// </summary>
        /// <returns>The value read.</returns>
        [Compiler.NoDebug]
        public UInt64 Read_UInt64()
        {
            return doRead_UInt64(this.Port);
        }

        /// <summary>
        /// Writes a byte to the specified port.
        /// </summary>
        /// <param name="port">The port to write to.</param>
        /// <param name="aVal">The value to write.</param>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\IO\IOPort\Write")]
        private static void doWrite(UInt16 port, byte aVal)
        {
        }
        /// <summary>
        /// Writes a UInt16 to the specified port.
        /// </summary>
        /// <param name="port">The port to write to.</param>
        /// <param name="aVal">The value to write.</param>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static void doWrite(UInt16 port, UInt16 aVal)
        {
        }
        /// <summary>
        /// Writes a UInt32 to the specified port.
        /// </summary>
        /// <param name="port">The port to write to.</param>
        /// <param name="aVal">The value to write.</param>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static void doWrite(UInt16 port, UInt32 aVal)
        {
        }
        /// <summary>
        /// Writes a UInt64 to the specified port.
        /// </summary>
        /// <param name="port">The port to write to.</param>
        /// <param name="aVal">The value to write.</param>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static void doWrite(UInt16 port, UInt64 aVal)
        {
        }

        /// <summary>
        /// Writes a byte to the port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [Compiler.NoDebug]
        public virtual void Write(byte aVal)
        {
            doWrite(this.Port, aVal);
        }
        /// <summary>
        /// Writes a UInt16 to the port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [Compiler.NoDebug]
        public void Write(UInt16 aVal)
        {
            doWrite(this.Port, aVal);
        }
        /// <summary>
        /// Writes a UInt32 to the port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [Compiler.NoDebug]
        public void Write(UInt32 aVal)
        {
            doWrite(this.Port, aVal);
        }
        /// <summary>
        /// Writes a UInt64 to the port.
        /// </summary>
        /// <param name="aVal">The value to write.</param>
        [Compiler.NoDebug]
        public void Write(UInt64 aVal)
        {
            doWrite(this.Port, aVal);
        }

        /// <summary>
        /// Reads bytes into the specified byte array.
        /// </summary>
        /// <param name="aData">The byte array to read data into.</param>
        [Compiler.NoDebug]
        public void Read8(byte[] aData)
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
        public void Read16(UInt16[] aData)
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
        public void Read32(UInt32[] aData)
        {
            for (int i = 0; i < aData.Length; i++)
            {
                aData[i] = Read_UInt32();
            }
        }
    }
}
