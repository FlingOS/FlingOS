#if MIPS
using System; 

namespace FlingOops.MIPS.CI20
{
    /// <summary>
    /// <remarks>
    /// <para>
    /// See CompilerTests class remarks for notes on UART setup for MIPS.
    /// </para>
    /// </remarks>
    /// </summary>
    public static class Kernel
    {
        [Drivers.Compiler.Attributes.NoGC]
        public static void Start()
        {
            UART.Init();
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