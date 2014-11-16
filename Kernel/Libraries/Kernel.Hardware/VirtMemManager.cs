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
using Kernel.Hardware.VirtMem;

namespace Kernel.Hardware
{
    /// <summary>
    /// The virtual memory manager for the kernel. Wraps the specific implementation to allow targetting different architectures without
    /// changing the entire kernel.
    /// </summary>
    public static unsafe class VirtMemManager
    {
        /// <summary>
        /// The specific virtual memory implementation to use.
        /// </summary>
        private static VirtMemImpl impl;

        /// <summary>
        /// Initialises the virtual memory manager.
        /// </summary>
        public static void Init()
        {
            impl = new x86();

            // Map in the kernel pages.
            //   - Technically this has already been done in "VirtMemInit.x86_32.asm", however,
            //     from the C# code perspective it has no idea so the mapping done here is to
            //     get the high level code's view of the memory up to speed with the actual
            //     state
            impl.MapKernel();
        }

        /// <summary>
        /// Maps the specified amount of memory.
        /// </summary>
        /// <param name="pAddr">The physical address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="vAddr">The virtual address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="size">The amount of memory (in bytes) to map (must be a multiple of 4KiB)</param>
        public static void Map(void* pAddr, void* vAddr, uint size)
        {
            Map((uint)pAddr, (uint)vAddr, size);
        }
        /// <summary>
        /// Maps the specified amount of memory.
        /// </summary>
        /// <param name="pAddr">The physical address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="vAddr">The virtual address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="size">The amount of memory (in bytes) to map (must be a multiple of 4KiB)</param>
        public static void Map(uint pAddr, void* vAddr, uint size)
        {
            Map(pAddr, (uint)vAddr, size);
        }
        /// <summary>
        /// Maps the specified amount of memory.
        /// </summary>
        /// <param name="pAddr">The physical address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="vAddr">The virtual address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="size">The amount of memory (in bytes) to map (must be a multiple of 4KiB)</param>
        public static void Map(void* pAddr, uint vAddr, uint size)
        {
            Map((uint)pAddr, vAddr, size);
        }
        /// <summary>
        /// Maps the specified amount of memory.
        /// </summary>
        /// <param name="pAddr">The physical address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="vAddr">The virtual address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="size">The amount of memory (in bytes) to map (must be a multiple of 4KiB)</param>
        public static void Map(uint pAddr, uint vAddr, uint size)
        {
            while (size > 0)
            {
                impl.Map(pAddr, vAddr);
                size -= 4096;
                pAddr += 4096;
                vAddr += 4096;
            }
        }

        /// <summary>
        /// Gets the physical address for the specified virtual address.
        /// </summary>
        /// <param name="vAddr">The virtual address to get the physical address of.</param>
        /// <returns>The physical address.</returns>
        /// <remarks>
        /// This has an undefined return value and behaviour if the virtual address is not mapped.
        /// </remarks>
        public static void* GetPhysicalAddress(void* vAddr)
        {
            return (void*)GetPhysicalAddress((uint)vAddr);
        }
        /// <summary>
        /// Gets the physical address for the specified virtual address.
        /// </summary>
        /// <param name="vAddr">The virtual address to get the physical address of.</param>
        /// <returns>The physical address.</returns>
        /// <remarks>
        /// This has an undefined return value and behaviour if the virtual address is not mapped.
        /// </remarks>
        public static uint GetPhysicalAddress(uint vAddr)
        {
            return impl.GetPhysicalAddress(vAddr);
        }

        /// <summary>
        /// Tests the virtual memory system.
        /// </summary>
        public static void Test()
        {
            impl.Test();
        }
        /// <summary>
        /// Prints out information about the free physical and virtual pages.
        /// </summary>
        public static void PrintFreePages()
        {
            impl.PrintFreePages();
        }
    }
}
