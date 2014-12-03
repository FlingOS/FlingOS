using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KernelABI
{
    public static class SystemCalls
    {
        public enum Calls : uint
        {
            Sleep = 1
        }

        public static uint Call(Calls callNumber,
            uint Param1,
            uint Param2,
            uint Param3)
        {
            return 0;
        }
        public static void Sleep(uint ms)
        {
            Call(Calls.Sleep, ms, 0, 0, 0);
        }
    }
}
