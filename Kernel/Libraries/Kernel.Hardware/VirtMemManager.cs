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

        public static void* MapFreePage(VirtMemImpl.PageFlags flags)
        {
            uint physAddr = impl.FindFreePhysPageAddr();
            uint virtAddr = impl.FindFreeVirtPageAddr();
            //BasicConsole.WriteLine(((FOS_System.String)"Mapping free page. physAddr=") + physAddr + ", virtAddr=" + virtAddr);
            Map(physAddr, virtAddr, 4096, flags);
            return (void*)virtAddr;
        }

        /// <summary>
        /// Maps the specified amount of memory.
        /// </summary>
        /// <param name="pAddr">The physical address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="vAddr">The virtual address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="size">The amount of memory (in bytes) to map (must be a multiple of 4KiB)</param>
        public static void Map(void* pAddr, void* vAddr, uint size, VirtMemImpl.PageFlags flags)
        {
            Map((uint)pAddr, (uint)vAddr, size, flags);
        }
        /// <summary>
        /// Maps the specified amount of memory.
        /// </summary>
        /// <param name="pAddr">The physical address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="vAddr">The virtual address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="size">The amount of memory (in bytes) to map (must be a multiple of 4KiB)</param>
        public static void Map(uint pAddr, void* vAddr, uint size, VirtMemImpl.PageFlags flags)
        {
            Map(pAddr, (uint)vAddr, size, flags);
        }
        /// <summary>
        /// Maps the specified amount of memory.
        /// </summary>
        /// <param name="pAddr">The physical address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="vAddr">The virtual address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="size">The amount of memory (in bytes) to map (must be a multiple of 4KiB)</param>
        public static void Map(void* pAddr, uint vAddr, uint size, VirtMemImpl.PageFlags flags)
        {
            Map((uint)pAddr, vAddr, size, flags);
        }
        /// <summary>
        /// Maps the specified amount of memory.
        /// </summary>
        /// <param name="pAddr">The physical address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="vAddr">The virtual address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="size">The amount of memory (in bytes) to map (must be a multiple of 4KiB)</param>
        public static void Map(uint pAddr, uint vAddr, uint size, VirtMemImpl.PageFlags flags)
        {
            flags |= VirtMemImpl.PageFlags.Present | VirtMemImpl.PageFlags.Writeable;
            while (size > 0)
            {
                impl.Map(pAddr, vAddr, flags);
                size -= 4096;
                pAddr += 4096;
                vAddr += 4096;
            }
        }

        public static void Unmap(void* vAddr)
        {
            Unmap((uint)vAddr);
        }
        public static void Unmap(uint vAddr)
        {
            impl.Unmap(vAddr);
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
            BasicConsole.WriteLine("Starting virt mem test...");

            try
            {
                impl.Test();

                byte* ptr = (byte*)MapFreePage(VirtMemImpl.PageFlags.KernelOnly);
                for (int i = 0; i < 4096; i++, ptr++)
                {
                    *ptr = 5;

                    if (*ptr != 5)
                    {
                        BasicConsole.WriteLine("Failed to set mem!");
                    }
                }
            }
            catch
            {
                BasicConsole.WriteLine("Exception. Failed test.");
                BasicConsole.WriteLine(ExceptionMethods.CurrentException._Type.Signature);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
            }

            BasicConsole.DelayOutput(5);
        }
        /// <summary>
        /// Prints out information about the free physical and virtual pages.
        /// </summary>
        public static void PrintUsedPages()
        {
            impl.PrintUsedPages();
        }
    }
}
