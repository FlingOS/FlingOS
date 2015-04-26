using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using KernelABI;
using Kernel.FOS_System;

namespace Drivers.Framework
{
    public static class TestClass
    {
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        static TestClass()
        {
        }

        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.MainMethod]
        public static void Test()
        {
            int x = 0xF;
            int y = 0xF;
            int z = Kernel.FOS_System.Math.Min(x, y);
            //KernelABI.SystemCalls.Call(SystemCalls.Calls.Sleep, 1000, 0, 0);
        }

        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.CallStaticConstructorsMethod]
        public static void CallStaticConstructors()
        {
        }

        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.HaltMethod]
        public static void Halt()
        {
        }
    }
}
