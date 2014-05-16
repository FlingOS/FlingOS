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


        protected internal IOPort(UInt16 aPort)
        {
            Port = aPort;
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
        
        public byte Read_Byte()
        {
            return doRead_Byte(this.Port);
        }
        public UInt16 Read_UInt16()
        {
            return doRead_UInt16(this.Port);
        }
        public UInt32 Read_UInt32()
        {
            return doRead_UInt32(this.Port);
        }
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
        
        public virtual void Write(byte aVal)
        {
            doWrite(this.Port, aVal);
        }
        public void Write(UInt16 aVal)
        {
            doWrite(this.Port, aVal);
        }
        public void Write(UInt32 aVal)
        {
            doWrite(this.Port, aVal);
        }
        public void Write(UInt64 aVal)
        {
            doWrite(this.Port, aVal);
        }
    }

    public class IOPortRead : IOPort
    {
        public IOPortRead()
            : base(5)
        {
        }

        public override void Write(byte aVal)
        {
            Kernel.ExceptionMethods.Throw(new FOS_System.Exception("Not implemented exception!"));
            base.Write(aVal);
        }
    }
}
