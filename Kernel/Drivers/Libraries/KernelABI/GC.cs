using System;

namespace KernelABI
{
    public static class GC
    {
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.IncrementRefCountMethod]
        public static void IncrementRefCount(Drivers.Framework.Object anObj)
        {
        }
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.DecrementRefCountMethod]
        public static void DecrementRefCount(Drivers.Framework.Object anObj)
        {
        }
    }
}
