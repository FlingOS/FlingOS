using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlingOops
{
    public static class Kernel
    {
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = "ASM\\Kernel")]
        [Drivers.Compiler.Attributes.SequencePriority(Priority = long.MinValue)]
        public static void Boot()
        {
        }

        [Drivers.Compiler.Attributes.MainMethod]
        [Drivers.Compiler.Attributes.NoGC]
        public static void Main()
        {
#if MIPS
            FlingOops.MIPS.CI20.Kernel.Start();
#elif x86
            FlingOops.x86.Kernel.Start();
#endif

            BasicConsole.Init();
            BasicConsole.WriteLine("Kernel executing...");

            CompilerTests.RunTests();

#if MIPS
            FlingOops.MIPS.CI20.Kernel.End();
#elif x86
            FlingOops.x86.Kernel.End();
#endif
        }

        [Drivers.Compiler.Attributes.CallStaticConstructorsMethod]
        public static void CallStaticConstructors()
        {
        }
    }
}
