using System;

namespace Kernel.FOS_System
{
    /// <summary>
    /// Provides constants and static methods for common mathematical functions and some operations not supported by 
    /// IL code.
    /// </summary>
    [Compiler.PluggedClass]
    public static class Math
    {
        /// <summary>
        /// Divides a UInt64 by a UInt32.
        /// </summary>
        /// <param name="dividend">The UInt64 to be divided.</param>
        /// <param name="divisor">The UInt32 to divide.</param>
        /// <returns>The quotient of the division.</returns>
        [Compiler.PluggedMethod(ASMFilePath=@"ASM\Math\Divide")]
        public static ulong Divide(ulong dividend, uint divisor)
        {
            return 0;
        }
    }
}
