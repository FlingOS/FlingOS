using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.IO
{
    [Compiler.PluggedClass]
    public class IOPort : FOS_System.Object
    {
        public readonly UInt16 Port;


        [Compiler.NoDebug]
        protected internal IOPort(UInt16 aPort)
        {
            Port = aPort;
        }
        [Compiler.NoDebug]
        protected internal IOPort(UInt16 aBase, UInt16 anOffset)
        {
            Port = (UInt16)(aBase + anOffset);
        }


        [Compiler.PluggedMethod(ASMFilePath = @"ASM\IO\IOPort\Read")]
        private static byte doRead_Byte(UInt16 port)
        {
            return 0;
        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static UInt16 doRead_UInt16(UInt16 port)
        {
            return 0;
        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static UInt32 doRead_UInt32(UInt16 port)
        {
            return 0;
        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static UInt64 doRead_UInt64(UInt16 port)
        {
            return 0;
        }

        [Compiler.NoDebug]
        public byte Read_Byte()
        {
            return doRead_Byte(this.Port);
        }
        [Compiler.NoDebug]
        public UInt16 Read_UInt16()
        {
            return doRead_UInt16(this.Port);
        }
        [Compiler.NoDebug]
        public UInt32 Read_UInt32()
        {
            return doRead_UInt32(this.Port);
        }
        [Compiler.NoDebug]
        public UInt64 Read_UInt64()
        {
            return doRead_UInt64(this.Port);
        }
        

        [Compiler.PluggedMethod(ASMFilePath = @"ASM\IO\IOPort\Write")]
        private static void doWrite(UInt16 port, byte aVal)
        {
        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static void doWrite(UInt16 port, UInt16 aVal)
        {
        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static void doWrite(UInt16 port, UInt32 aVal)
        {
        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static void doWrite(UInt16 port, UInt64 aVal)
        {
        }

        [Compiler.NoDebug]
        public virtual void Write(byte aVal)
        {
            doWrite(this.Port, aVal);
        }
        [Compiler.NoDebug]
        public void Write(UInt16 aVal)
        {
            doWrite(this.Port, aVal);
        }
        [Compiler.NoDebug]
        public void Write(UInt32 aVal)
        {
            doWrite(this.Port, aVal);
        }
        [Compiler.NoDebug]
        public void Write(UInt64 aVal)
        {
            doWrite(this.Port, aVal);
        }

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
        [Compiler.NoDebug]
        public void Read16(UInt16[] aData)
        {
            for (int i = 0; i < aData.Length; i++)
            {
                aData[i] = Read_UInt16();
            }
        }
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
