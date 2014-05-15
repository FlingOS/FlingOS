using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.IO
{
    [Compiler.PluggedClass]
    public class IOPort
    {
        public readonly UInt16 Port;

        protected internal IOPort(UInt16 aPort)
        {
            Port = aPort;
        }
        protected internal IOPort(UInt16 aBase, UInt16 anOffset)
        {
            Port = (UInt16)(aBase + anOffset);
        }

        [Compiler.PluggedMethod(ASMFilePath=@"ASM\IO\IOPort\Write")]
        public void Write(byte aByte)
        {

        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public void Write(UInt16 aByte)
        {

        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public void Write(UInt32 aByte)
        {

        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public void Write(UInt64 aByte)
        {

        }
    }
}
