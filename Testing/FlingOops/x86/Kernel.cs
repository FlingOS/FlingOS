using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlingOops.x86
{
    public static class Kernel
    {
        [Drivers.Compiler.Attributes.NoGC]
        public static void Start()
        {
        }

        [Drivers.Compiler.Attributes.NoGC]
        public static void End()
        {
            bool OK = true;
            while (OK)
            {
                ;
            }
        }
    }
}
