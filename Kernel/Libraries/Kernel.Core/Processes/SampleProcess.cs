using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Core.Processes
{
    [Compiler.PluggedClass]
    public static unsafe class SampleProcess
    {
        public static void Main()
        {
            Console.Default.WriteLine("Boo!");
        }

        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static void* GetMainMethodPtr()
        {
            return null;
        }
    }
}
