using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Core.Tasks
{
    [Compiler.PluggedClass]
    public static unsafe class SampleTask
    {
        public static void Main()
        {
            Console.Default.WriteLine("Boo!");
        }
    }
}
