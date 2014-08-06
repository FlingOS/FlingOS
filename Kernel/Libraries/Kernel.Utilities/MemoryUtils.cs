#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
using System;

namespace Kernel.Utilities
{
    /// <summary>
    /// Static utility methods for memory manipulation.
    /// </summary>
    [Compiler.PluggedClass]
    public static unsafe class MemoryUtils
    {
        /// <summary>
        /// Copies the specified amount of memory from the source to the dest.
        /// </summary>
        /// <param name="dest">The destination memory.</param>
        /// <param name="src">The source memory.</param>
        /// <param name="length">The amount of memory to copy.</param>
        [Compiler.NoGC]
        [Compiler.NoDebug]
        public static void MemCpy_32(byte* dest, byte* src, uint length)
        {
            for (uint i = 0; i < length; i++)
            {
                dest[i] = src[i];
            }
        }
        /// <summary>
        /// Copies the specified amount of memory from the source to the dest.
        /// </summary>
        /// <param name="dest">The destination memory.</param>
        /// <param name="src">The source memory.</param>
        /// <param name="length">The amount of memory to copy.</param>
        [Compiler.NoGC]
        [Compiler.NoDebug]
        public static void MemCpy(byte* dest, byte* src, ulong length)
        {
            for(ulong i = 0; i < length; i++)
            {
                dest[i] = src[i];
            }
        }

        /// <summary>
        /// Zeroes-out the specified memory.
        /// </summary>
        /// <param name="ptr">Pointer to the start of the memory to set to zero.</param>
        /// <param name="size">The length of memory to set to zeroes.</param>
        /// <returns>The original pointer.</returns>
        [Compiler.NoGC]
        [Compiler.NoDebug]
        public static void* ZeroMem(void* ptr, uint size)
        {
            byte* bPtr = (byte*)ptr;
            byte* bEndPtr = ((byte*)ptr) + size;
            while (bPtr < bEndPtr)
            {
                *bPtr++ = 0;
            }
            return ptr;
        }

        /// <summary>
        /// Gets a field from a byte in memory.
        /// </summary>
        /// <param name="addr">The pointer to the memory to get the field from.</param>
        /// <param name="byteNum">The index of the byte to use.</param>
        /// <param name="shift">
        /// The amount to shift the byte right. This is the index of the left-most bit of field (little-endian,
        /// hi-to-lo notation).
        /// </param>
        /// <param name="len">The length of the field in bits.</param>
        /// <returns>The field value.</returns>
        [Compiler.NoGC]
        [Compiler.NoDebug]
        public static byte GetField(byte* addr, byte byteNum, byte shift, byte len) 
        {
            return (byte)((addr[byteNum] >> (shift)) & ((1 << len) - 1));
        }

        /// <summary>
        /// Converts a value from host to network byte order.
        /// </summary>
        /// <param name="aUInt32">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [Compiler.PluggedMethod(ASMFilePath=@"ASM\MemoryUtils")]
        public static uint htonl(uint aUInt32)
        {
            return 0;
        }
    }
}
