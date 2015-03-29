using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KernelABI;

namespace Drivers.Framework
{
    public class TestClass
    {
        public static void Test()
        {
            KernelABI.SystemCalls.Call(SystemCalls.Calls.Sleep, 1000, 0, 0);
        }
    }
}
