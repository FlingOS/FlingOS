#if x86
using System;

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
#endif
