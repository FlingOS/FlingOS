#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
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

        public static uint FindFreePhysPage()
        {
            return impl.FindFreePhysPageAddr();
        }
        public static uint FindFreeVirtPage()
        {
            return impl.FindFreeVirtPageAddr();
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
        public static void Map(void* pAddr, void* vAddr, uint size, VirtMemImpl.PageFlags flags, UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
            Map((uint)pAddr, (uint)vAddr, size, flags, UpdateUsedPages);
        }
        /// <summary>
        /// Maps the specified amount of memory.
        /// </summary>
        /// <param name="pAddr">The physical address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="vAddr">The virtual address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="size">The amount of memory (in bytes) to map (must be a multiple of 4KiB)</param>
        public static void Map(uint pAddr, void* vAddr, uint size, VirtMemImpl.PageFlags flags, UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
            Map(pAddr, (uint)vAddr, size, flags, UpdateUsedPages);
        }
        /// <summary>
        /// Maps the specified amount of memory.
        /// </summary>
        /// <param name="pAddr">The physical address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="vAddr">The virtual address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="size">The amount of memory (in bytes) to map (must be a multiple of 4KiB)</param>
        public static void Map(void* pAddr, uint vAddr, uint size, VirtMemImpl.PageFlags flags, UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
            Map((uint)pAddr, vAddr, size, flags, UpdateUsedPages);
        }
        /// <summary>
        /// Maps the specified amount of memory.
        /// </summary>
        /// <param name="pAddr">The physical address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="vAddr">The virtual address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="size">The amount of memory (in bytes) to map (must be a multiple of 4KiB)</param>
        public static void Map(uint pAddr, uint vAddr, uint size, VirtMemImpl.PageFlags flags, UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
            flags |= VirtMemImpl.PageFlags.Present | VirtMemImpl.PageFlags.Writeable;
            while (size > 0)
            {
                impl.Map(pAddr, vAddr, flags, UpdateUsedPages);
                size -= 4096;
                pAddr += 4096;
                vAddr += 4096;
            }
        }

        public static void Unmap(void* vAddr, UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
            Unmap((uint)vAddr, UpdateUsedPages);
        }
        public static void Unmap(uint vAddr, UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
            impl.Unmap(vAddr, UpdateUsedPages);
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
