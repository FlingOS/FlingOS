#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
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
using Drivers.Compiler.Attributes;
using Kernel.Framework.Processes.Synchronisation;
using Kernel.VirtualMemory.Implementations;

namespace Kernel.VirtualMemory
{
    [Flags]
    public enum UpdateUsedPagesFlags : byte
    {
        None = 0,
        Physical = 1,
        Virtual = 2,
        Both = 3
    }

    [Flags]
    public enum PageFlags : uint
    {
        None = 0,
        Present = 1,
        Writeable = 2,
        KernelOnly = 4
    }

    /// <summary>
    ///     The virtual memory manager for the kernel. Wraps the specific implementation to allow targetting different
    ///     architectures without
    ///     changing the entire kernel.
    /// </summary>
    public static unsafe class VirtualMemoryManager
    {
        /// <summary>
        ///     The specific virtual memory implementation to use.
        /// </summary>
        [Group(Name = "IsolatedKernel_VirtualMemory")] private static IVirtualMemoryImplementation impl;

        [Group(Name = "IsolatedKernel_VirtualMemory")] private static readonly SpinLock MapFreePagesLock =
            new SpinLock(-1);

        /// <summary>
        ///     Initialises the virtual memory manager.
        /// </summary>
        public static void Init(IVirtualMemoryImplementation anImpl)
        {
            impl = anImpl;

            // Map in the kernel pages.
            //   - Technically this has already been done in "VirtMemInit.x86_32.asm", however,
            //     from the C# code perspective it has no idea so the mapping done here is to
            //     get the high level code's view of the memory up to speed with the actual
            //     state
            impl.MapKernel();
        }

        public static bool IsWithinKernelFixedMemory(uint VAddr)
        {
            return impl.IsWithinKernelFixedMemory(VAddr);
        }

        public static void* MapFreePage(PageFlags flags, out void* physAddr)
        {
            return MapFreePages(flags, 1, out physAddr);
        }

        public static void* MapFreePages(PageFlags flags, int numPages,
            out void* physAddrsStart)
        {
            uint virtAddrsStart = 0xDEADBEEF;
            physAddrsStart = (void*)0xDEADBEEF;

            MapFreePagesLock.Enter();

            void* result = (void*)0xDEADBEEF;

            try
            {
                virtAddrsStart = impl.FindFreeVirtPageAddrs(numPages);

                if (virtAddrsStart == 0xDEADBEEF)
                {
                    BasicConsole.WriteLine("!!! PANIC !!!");
                    BasicConsole.WriteLine("VirtMemManager.MapFreePages using 0xDEADBEEF for virtual addresses!");
                    BasicConsole.WriteLine("!-!-!-!-!-!-!");
                }

                physAddrsStart = (void*)impl.FindFreePhysPageAddrs(numPages);

                if ((uint)physAddrsStart == 0xDEADBEEF)
                {
                    BasicConsole.WriteLine("!!! PANIC !!!");
                    BasicConsole.WriteLine("VirtMemManager.MapFreePages using 0xDEADBEEF for physical addresses!");
                    BasicConsole.WriteLine("!-!-!-!-!-!-!");
                }

                for (uint i = 0; i < numPages; i++)
                {
                    Map((uint)physAddrsStart + i*4096, virtAddrsStart + i*4096, 4096, flags);
                }

                result = (void*)virtAddrsStart;
            }
            finally
            {
                MapFreePagesLock.Exit();
            }

            return result;
        }

        public static void* MapFreePages(PageFlags flags, int numPages, uint virtAddrsStart,
            out void* physAddrsStart)
        {
            physAddrsStart = (void*)0xDEADBEEF;
            void* result = (void*)0xDEADBEEF;

            MapFreePagesLock.Enter();

            try
            {
                physAddrsStart = (void*)impl.FindFreePhysPageAddrs(numPages);

                if ((uint)physAddrsStart == 0xDEADBEEF)
                {
                    BasicConsole.WriteLine("!!! PANIC !!!");
                    BasicConsole.WriteLine("VirtMemManager.MapFreePages using 0xDEADBEEF for physical addresses!");
                    BasicConsole.WriteLine("!-!-!-!-!-!-!");
                }

                for (uint i = 0; i < numPages; i++)
                {
                    Map((uint)physAddrsStart + i*4096, virtAddrsStart + i*4096, 4096, flags);
                }

                result = (void*)virtAddrsStart;
            }
            finally
            {
                MapFreePagesLock.Exit();
            }

            return result;
        }

        public static void* MapFreePages(PageFlags flags, int numPages, uint virtAddrsStart,
            uint physAddrsStart)
        {
            MapFreePagesLock.Enter();

            void* result = (void*)0xDEADBEEF;

            try
            {
                for (uint i = 0; i < numPages; i++)
                {
                    Map(physAddrsStart + i*4096, virtAddrsStart + i*4096, 4096, flags);
                }

                result = (void*)virtAddrsStart;
            }
            finally
            {
                MapFreePagesLock.Exit();
            }

            return result;
        }

        public static void* MapFreePhysicalPages(PageFlags flags, int numPages,
            uint physAddrsStart)
        {
            uint virtAddrsStart = 0xDEADBEEF;

            MapFreePagesLock.Enter();

            void* result = (void*)0xDEADBEEF;

            try
            {
                virtAddrsStart = impl.FindFreeVirtPageAddrs(numPages);

                if (virtAddrsStart == 0xDEADBEEF)
                {
                    BasicConsole.WriteLine("!!! PANIC !!!");
                    BasicConsole.WriteLine("VirtMemManager.MapFreePages using 0xDEADBEEF for virtual addresses!");
                    BasicConsole.WriteLine("!-!-!-!-!-!-!");
                }

                for (uint i = 0; i < numPages; i++)
                {
                    Map(physAddrsStart + i*4096, virtAddrsStart + i*4096, 4096, flags);
                }

                result = (void*)virtAddrsStart;
            }
            finally
            {
                MapFreePagesLock.Exit();
            }

            return result;
        }

        public static void* MapFreePageForKernel(PageFlags flags, out void* physAddr)
        {
            return MapFreePagesForKernel(flags, 1, out physAddr);
        }

        public static void* MapFreePagesForKernel(PageFlags flags, int numPages,
            out void* physAddrsStart)
        {
            uint virtAddrsStart = 0xDEADBEEF;
            physAddrsStart = (void*)0xDEADBEEF;

            MapFreePagesLock.Enter();

            void* result = (void*)0xDEADBEEF;

            try
            {
                virtAddrsStart = impl.FindFreeVirtPageAddrsForKernel(numPages);

                if (virtAddrsStart == 0xDEADBEEF)
                {
                    BasicConsole.WriteLine("!!! PANIC !!!");
                    BasicConsole.WriteLine(
                        "VirtMemManager.MapFreePagesForKernel using 0xDEADBEEF for virtual addresses!");
                    BasicConsole.WriteLine("!-!-!-!-!-!-!");
                }

                physAddrsStart = (void*)impl.FindFreePhysPageAddrs(numPages);

                if ((uint)physAddrsStart == 0xDEADBEEF)
                {
                    BasicConsole.WriteLine("!!! PANIC !!!");
                    BasicConsole.WriteLine("VirtMemManager.MapFreePages using 0xDEADBEEF for physical addresses!");
                    BasicConsole.WriteLine("!-!-!-!-!-!-!");
                }

                for (uint i = 0; i < numPages; i++)
                {
                    Map((uint)physAddrsStart + i*4096, virtAddrsStart + i*4096, 4096, flags);
                }

                result = (void*)virtAddrsStart;
            }
            finally
            {
                MapFreePagesLock.Exit();
            }

            return result;
        }

        public static void* MapFreePhysicalPagesForKernel(PageFlags flags, int numPages,
            uint physAddrsStart)
        {
            uint virtAddrsStart = 0xDEADBEEF;

            MapFreePagesLock.Enter();

            void* result = (void*)0xDEADBEEF;

            try
            {
                virtAddrsStart = impl.FindFreeVirtPageAddrsForKernel(numPages);

                if (virtAddrsStart == 0xDEADBEEF)
                {
                    BasicConsole.WriteLine("!!! PANIC !!!");
                    BasicConsole.WriteLine(
                        "VirtMemManager.MapFreePhysicalPagesForKernel using 0xDEADBEEF for virtual addresses!");
                    BasicConsole.WriteLine("!-!-!-!-!-!-!");
                }

                for (uint i = 0; i < numPages; i++)
                {
                    Map(physAddrsStart + i*4096, virtAddrsStart + i*4096, 4096, flags);
                }

                result = (void*)virtAddrsStart;
            }
            finally
            {
                MapFreePagesLock.Exit();
            }

            return result;
        }

        /// <summary>
        ///     Maps the specified amount of memory.
        /// </summary>
        /// <param name="pAddr">The physical address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="vAddr">The virtual address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="size">The amount of memory (in bytes) to map (must be a multiple of 4KiB)</param>
        /// <param name="flags">The flags to apply to the allocated pages.</param>
        /// <param name="UpdateUsedPages">Which, if any, of the physical and virtual used pages lists to update.</param>
        public static void Map(void* pAddr, void* vAddr, uint size, PageFlags flags,
            UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
            Map((uint)pAddr, (uint)vAddr, size, flags, UpdateUsedPages);
        }

        /// <summary>
        ///     Maps the specified amount of memory.
        /// </summary>
        /// <param name="pAddr">The physical address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="vAddr">The virtual address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="size">The amount of memory (in bytes) to map (must be a multiple of 4KiB)</param>
        /// <param name="flags">The flags to apply to the allocated pages.</param>
        /// <param name="UpdateUsedPages">Which, if any, of the physical and virtual used pages lists to update.</param>
        public static void Map(uint pAddr, void* vAddr, uint size, PageFlags flags,
            UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
            Map(pAddr, (uint)vAddr, size, flags, UpdateUsedPages);
        }

        /// <summary>
        ///     Maps the specified amount of memory.
        /// </summary>
        /// <param name="pAddr">The physical address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="vAddr">The virtual address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="size">The amount of memory (in bytes) to map (must be a multiple of 4KiB)</param>
        /// <param name="flags">The flags to apply to the allocated pages.</param>
        /// <param name="UpdateUsedPages">Which, if any, of the physical and virtual used pages lists to update.</param>
        public static void Map(void* pAddr, uint vAddr, uint size, PageFlags flags,
            UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
            Map((uint)pAddr, vAddr, size, flags, UpdateUsedPages);
        }

        /// <summary>
        ///     Maps the specified amount of memory.
        /// </summary>
        /// <param name="pAddr">The physical address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="vAddr">The virtual address to start mapping at (must be 4KiB aligned).</param>
        /// <param name="size">The amount of memory (in bytes) to map (must be a multiple of 4KiB)</param>
        /// <param name="flags">The flags to apply to the allocated pages.</param>
        /// <param name="UpdateUsedPages">Which, if any, of the physical and virtual used pages lists to update.</param>
        public static void Map(uint pAddr, uint vAddr, uint size, PageFlags flags,
            UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.4.5.1");
            //}

            flags |= PageFlags.Present | PageFlags.Writeable;
            while (size > 0)
            {
                //if (Processes.Scheduler.OutputMessages)
                //{
                //    BasicConsole.WriteLine("Debug Point 9.1.4.5.2");
                //}

                impl.Map(pAddr, vAddr, flags, UpdateUsedPages);

                //if (Processes.Scheduler.OutputMessages)
                //{
                //    BasicConsole.WriteLine("Debug Point 9.1.4.5.3");
                //}

                size -= 4096;
                pAddr += 4096;
                vAddr += 4096;
            }
        }

        /// <summary>
        ///     Unmaps the specified page of virtual memory.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Unmaps means it sets the address to 0 and marks the page as not present.
        ///     </para>
        ///     <para>
        ///         It is common to call this with just UpdateUsedPages set to Virtual, since then the virtual page becomes
        ///         available for use
        ///         but the physical page remains reserved (though unmapped).
        ///     </para>
        /// </remarks>
        /// <param name="vAddr">The virtual address of the page to unmap.</param>
        /// <param name="UpdateUsedPages">Which, if any, of the physical and virtual used pages lists to update.</param>
        public static void Unmap(void* vAddr, UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
            Unmap((uint)vAddr, UpdateUsedPages);
        }

        /// <summary>
        ///     Unmaps the specified page of virtual memory.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Unmaps means it sets the address to 0 and marks the page as not present.
        ///     </para>
        ///     <para>
        ///         It is common to call this with just UpdateUsedPages set to Virtual, since then the virtual page becomes
        ///         available for use
        ///         but the physical page remains reserved (though unmapped).
        ///     </para>
        /// </remarks>
        /// <param name="vAddr">The virtual address of the page to unmap.</param>
        /// <param name="UpdateUsedPages">Which, if any, of the physical and virtual used pages lists to update.</param>
        public static void Unmap(uint vAddr, UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.2-5.1");
            //}

            impl.Unmap(vAddr, UpdateUsedPages);

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.2-5.2");
            //}
        }

        public static bool IsVirtualMapped(void* vAddr)
        {
            return IsVirtualMapped((uint)vAddr);
        }

        public static bool IsVirtualMapped(uint vAddr)
        {
            return impl.IsVirtualMapped(vAddr);
        }

        public static bool AreAnyVirtualMapped(uint vAddrStart, uint count)
        {
            uint vAddrEnd = vAddrStart + count*4096;
            for (uint addr = vAddrStart; addr < vAddrEnd; addr += 4096)
            {
                if (IsVirtualMapped(addr))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool AreAnyPhysicalMapped(uint pAddrStart, uint count)
        {
            return impl.AreAnyPhysicalMapped(pAddrStart, pAddrStart + count*4096);
        }

        public static void MapKernelProcessToMemoryLayout(MemoryLayout TheLayout)
        {
            impl.MapKernelProcessToMemoryLayout(TheLayout);
        }

        public static uint[] GetBuiltInProcessVAddrs()
        {
            return impl.GetBuiltInProcessVAddrs();
        }
        
        /// <summary>
        ///     Gets the physical address for the specified virtual address.
        /// </summary>
        /// <param name="vAddr">The virtual address to get the physical address of.</param>
        /// <returns>The physical address.</returns>
        /// <remarks>
        ///     This has an undefined return value and behaviour if the virtual address is not mapped.
        /// </remarks>
        public static uint GetPhysicalAddress(uint vAddr)
        {
            return impl.GetPhysicalAddress(vAddr);
        }

        /// <summary>
        ///     Tests the virtual memory system.
        /// </summary>
        public static void Test()
        {
            BasicConsole.WriteLine("Starting virt mem test...");

            try
            {
                impl.Test();

                void* unusedPAddr;
                byte* ptr = (byte*)MapFreePage(PageFlags.KernelOnly, out unusedPAddr);
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
        ///     Prints out information about the free physical and virtual pages.
        /// </summary>
        public static void PrintUsedPages()
        {
            impl.PrintUsedPages();
        }
    }
}