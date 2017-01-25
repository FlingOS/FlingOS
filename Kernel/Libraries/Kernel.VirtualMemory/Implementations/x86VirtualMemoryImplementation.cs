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

#if x86

//#define PAGING_TRACE

using System;
using Drivers.Compiler.Attributes;
using Kernel.Framework.Collections;
using Object = Kernel.Framework.Object;
using OutOfMemoryException = Kernel.Framework.Exceptions.OutOfMemoryException;
using String = Kernel.Framework.String;

namespace Kernel.VirtualMemory.Implementations
{
    /// <summary>
    ///     Provides methods for setting up paged virtual memory.
    /// </summary>
    public unsafe class X86VirtualMemoryImplementation : Object, IVirtualMemoryImplementation
    {
        /// <summary>
        ///     x86-specific Page Table Entry (bit) flags
        /// </summary>
        [Flags]
        private enum PTEFlags : uint
        {
            /// <summary>
            ///     No flags.
            /// </summary>
            None = 0x0,

            /// <summary>
            ///     Mark the page as present
            /// </summary>
            Present = 0x1,

            /// <summary>
            ///     Mark the page as writeable
            /// </summary>
            Writeable = 0x2,

            /// <summary>
            ///     Mark the page as user-mode accessible
            /// </summary>
            UserAllowed = 0x4,

            /// <summary>
            ///     Switches caching from write-back to write-through caching mode.
            /// </summary>
            PageLevelWriteThrough = 0x8,

            /// <summary>
            ///     Disables page level caching.
            /// </summary>
            PageLevelCacheDisable = 0x10,

            /// <summary>
            ///     Specifies page table entries refer to 4MiB instead of 4KiB pages.
            /// </summary>
            /// <remarks>
            ///     Attribute only set in page directory entries. 0 for page table entries.
            /// </remarks>
            Size_FourMiBPages = 0x80,

            /// <summary>
            ///     Whether the page table entry is global or not. Prevents TLB flush for the associated page when CR3 is changed.
            /// </summary>
            /// <remarks>
            ///     <para>
            ///         Requires Global Pages enabled bit in CR4 to be set.
            ///     </para>
            ///     <para>
            ///         Ignored for page directory entries.
            ///     </para>
            /// </remarks>
            Global = 0x100
        }

        /// <summary>
        ///     Bitmap of all the virtual pages that have ever been mapped.
        /// </summary>
        private readonly Bitmap AllUsedVirtPages = new Bitmap(1048576);

        //1024 * 1024 = 1048576
        /// <summary>
        ///     Bitmap of all the free (unmapped) physical pages of memory.
        /// </summary>
        private readonly Bitmap UsedPhysPages = new Bitmap(1048576);

        /// <summary>
        ///     Bitmap of all the free (unmapped) virtual pages of memory.
        /// </summary>
        private readonly Bitmap UsedVirtPages = new Bitmap(1048576);

        //TODO: This doesn't take into account pages that are removed from all memory layouts so may no longer be in use anywhere.

        private uint[] BuiltInProcessVAddrs;

        /// <summary>
        ///     Initialises the new x86 object.
        /// </summary>
        [NoDebug]
        public X86VirtualMemoryImplementation()
        {
        }

        /// <summary>
        ///     Tests the virtual memory system.
        /// </summary>
        public void Test()
        {
            BasicConsole.WriteLine("Testing paging...");

            try
            {
                uint pAddr = 0x40000000;
                uint vAddr1 = 0x40000000;
                uint vAddr2 = 0x60000000;
                uint* vPtr1 = (uint*)vAddr1;
                uint* vPtr2 = (uint*)vAddr2;

                BasicConsole.WriteLine("Set up addresses.");
                BasicConsole.WriteLine((String)"pAddr=" + pAddr);
                BasicConsole.WriteLine((String)"vAddr1=" + vAddr1);
                BasicConsole.WriteLine((String)"vAddr2=" + vAddr2);
                BasicConsole.WriteLine((String)"vPtr1=" + (uint)vPtr1);
                BasicConsole.WriteLine((String)"vPtr2=" + (uint)vPtr2);

                Map(pAddr, vAddr1);
                BasicConsole.WriteLine("Mapped virtual address 1.");

                Map(pAddr, vAddr2);
                BasicConsole.WriteLine("Mapped virtual address 2.");

                *vPtr1 = 5;
                BasicConsole.WriteLine("Set value");
                if (*vPtr1 != 5)
                {
                    BasicConsole.WriteLine("Failed test 1.");
                }
                else
                {
                    BasicConsole.WriteLine("Passed test 1.");
                }

                if (*vPtr2 != 5)
                {
                    BasicConsole.WriteLine("Failed test 2.");
                }
                else
                {
                    BasicConsole.WriteLine("Passed test 2.");
                }
            }
            catch
            {
                BasicConsole.WriteLine("Exception. Test failed.");
            }

            BasicConsole.WriteLine("Test completed.");
        }

        /// <summary>
        ///     Prints out information about the free physical and virtual pages.
        /// </summary>
        public void PrintUsedPages()
        {
            BasicConsole.WriteLine("Used physical pages: " + (String)UsedPhysPages.Count);
            BasicConsole.WriteLine("Used virtual pages : " + (String)UsedVirtPages.Count);
            BasicConsole.DelayOutput(5);
        }

        /// <summary>
        ///     Finds the specified number of contiguous, free, physical pages.
        /// </summary>
        /// <remarks>
        ///     While in most applications the pages needn't be contiguous, some areas require it (e.g. physical pages for USB
        ///     transactions).
        ///     A future optimisation would be to create a separate function which enforces contiguous pages and makes this one
        ///     non-contiguous
        ///     by default.
        /// </remarks>
        /// <param name="num">The number of physical pages to find.</param>
        /// <returns>The address of the first page.</returns>
        [NoDebug]
        public uint FindFreePhysPageAddrs(int num)
        {
            int result = UsedPhysPages.FindContiguousClearEntries(num);
            if (result == -1)
            {
                BasicConsole.WriteLine("Error finding free physical pages!");
                BasicConsole.DelayOutput(10);

                ExceptionMethods.Throw(new OutOfMemoryException("Could not find any more free physical pages."));
            }

            return (uint)(result*4096);
        }

        /// <summary>
        ///     Finds the specified number of contiguous, free, virtual pages.
        /// </summary>
        /// <remarks>
        ///     Unlike physical pages, virtual pages requested in blocks do normally need to be contiguous (since the contiguous to
        ///     non-contiguous
        ///     mapping is part of the point of virtual memory). As a result, it would not be worth optimising calls to this
        ///     function as most of the
        ///     time they will need contiguous pages.
        /// </remarks>
        /// <param name="num">The number of virtual pages to find.</param>
        /// <returns>The address of the first page.</returns>
        [NoDebug]
        public uint FindFreeVirtPageAddrs(int num)
        {
            int result = UsedVirtPages.FindContiguousClearEntries(num);
            if (result == -1)
            {
                BasicConsole.WriteLine("Error finding free virtual pages!");
                BasicConsole.DelayOutput(10);

                ExceptionMethods.Throw(new OutOfMemoryException("Could not find any more free virtual pages."));
            }

            return (uint)(result*4096);
        }

        /// <summary>
        ///     Finds the specified number of contiguous, free, virtual pages that are suitable for use by the kernel.
        /// </summary>
        /// <param name="num">The number of virtual pages to find.</param>
        /// <returns>The address of the first page.</returns>
        [NoDebug]
        public uint FindFreeVirtPageAddrsForKernel(int num)
        {
            int result = AllUsedVirtPages.FindContiguousClearEntries(num);
            if (result == -1)
            {
                BasicConsole.WriteLine("Error finding free virtual pages for the kernel!");
                BasicConsole.DelayOutput(10);

                ExceptionMethods.Throw(
                    new OutOfMemoryException("Could not find any more free virtual pages for the kernel."));
            }

            return (uint)(result*4096);
        }

        //public static bool Unmap_Print = false;

        /// <summary>
        ///     Maps the specified virtual address to the specified physical address.
        /// </summary>
        /// <remarks>
        ///     Uses the flags Present, KernelOnly and Writeable as defaults.
        /// </remarks>
        /// <param name="pAddr">The physical address to map to.</param>
        /// <param name="vAddr">The virtual address to map.</param>
        /// <param name="UpdateUsedPages">Which, if any, of the physical and virtual used pages lists to update.</param>
        [NoDebug]
        public void Map(uint pAddr, uint vAddr, UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
            Map(pAddr, vAddr, PageFlags.Present | PageFlags.KernelOnly | PageFlags.Writeable, UpdateUsedPages);
        }

        /// <summary>
        ///     Maps the specified virtual address to the specified physical address.
        /// </summary>
        /// <param name="pAddr">The physical address to map to.</param>
        /// <param name="vAddr">The virtual address to map.</param>
        /// <param name="flags">The flags to apply to the allocated pages.</param>
        /// <param name="UpdateUsedPages">Which, if any, of the physical and virtual used pages lists to update.</param>
        [NoDebug]
        public void Map(uint pAddr, uint vAddr, PageFlags flags,
            UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
            PTEFlags pteFlags = PTEFlags.None;
            if ((flags & PageFlags.Present) != 0)
            {
                pteFlags |= PTEFlags.Present;
            }
            if ((flags & PageFlags.KernelOnly) == 0)
            {
                pteFlags |= PTEFlags.UserAllowed;
            }
            if ((flags & PageFlags.Writeable) != 0)
            {
                pteFlags |= PTEFlags.Writeable;
            }
            Map(pAddr, vAddr, pteFlags, UpdateUsedPages);
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
        [NoDebug]
        public void Unmap(uint vAddr, UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
            //if (Unmap_Print)
            //{
            //    BasicConsole.WriteLine("Getting physical addr...");
            //}

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.2-5.1.1");
            //}

            uint pAddr = GetPhysicalAddress(vAddr);

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.2-5.1.2");
            //}

            //if (Unmap_Print)
            //{
            //    BasicConsole.WriteLine("Calculating virt addr parts...");
            //}
            uint virtPDIdx = vAddr >> 22;
            uint virtPTIdx = (vAddr >> 12) & 0x03FF;

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.2-5.1.3");
            //}

            //if (Unmap_Print)
            //{
            //    BasicConsole.WriteLine("Calculating phys addr parts...");
            //}
            uint physPDIdx = pAddr >> 22;
            uint physPTIdx = (pAddr >> 12) & 0x03FF;

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.2-5.1.4");
            //}

            //if (Unmap_Print)
            //{
            //    BasicConsole.WriteLine("Checking flags & stuff...");
            //}
            if ((UpdateUsedPages & UpdateUsedPagesFlags.Physical) != 0)
            {
                UsedPhysPages.Clear((int)(physPDIdx*1024 + physPTIdx));
            }
            if ((UpdateUsedPages & UpdateUsedPagesFlags.Virtual) != 0)
            {
                UsedVirtPages.Clear((int)(virtPDIdx*1024 + virtPTIdx));
            }


            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.2-5.1.5");
            //}

            //if (Unmap_Print)
            //{
            //    BasicConsole.WriteLine("Getting page table...");
            //}
            uint* virtPTPtr = GetFixedPage(virtPDIdx);

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.2-5.1.6");
            //}

            //if (Unmap_Print)
            //{
            //    BasicConsole.WriteLine("Setting page entry...");
            //}

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.2-5.1.7");
            //}

            SetPageEntry(virtPTPtr, virtPTIdx, 0, PTEFlags.None);

            //if (Unmap_Print)
            //{
            //    BasicConsole.WriteLine("Invalidating page table entry...");
            //}

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.2-5.1.8");
            //}

            InvalidatePTE(vAddr);

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.2-5.1.9");
            //}
        }

        /// <summary>
        ///     Gets the physical address for the specified virtual address.
        /// </summary>
        /// <param name="vAddr">The virtual address to get the physical address of.</param>
        /// <returns>The physical address.</returns>
        /// <remarks>
        ///     This has an undefined return value and behaviour if the virtual address is not mapped.
        /// </remarks>
        [NoDebug]
        public uint GetPhysicalAddress(uint vAddr)
        {
            //Calculate page directory and page table indices
            uint pdIdx = vAddr >> 22;
            uint ptIdx = (vAddr >> 12) & 0x3FF;
            //Get a pointer to the pre-allocated page table
            uint* ptPtr = GetFixedPage(pdIdx);

            //Get the physical address using the page table entry phys address
            //  plus the offset from the virtual address which will be the same 
            //  as the offset from the phys address (page-aligned addresses and 
            //  all that).
            return (ptPtr[ptIdx] & 0xFFFFF000) + (vAddr & 0xFFF);
        }

        public bool IsVirtualMapped(uint vAddr)
        {
            uint virtPDIdx = vAddr >> 22;
            uint virtPTIdx = (vAddr >> 12) & 0x03FF;

            return UsedVirtPages.IsSet((int)(virtPDIdx*1024 + virtPTIdx));
        }

        public bool AreAnyPhysicalMapped(uint pAddrStart, uint pAddrEnd)
        {
            uint physPDIdx = pAddrStart >> 22;
            uint physPTIdx = (pAddrStart >> 12) & 0x03FF;

            uint physEndPDIdx = pAddrEnd >> 22;
            uint physEndPTIdx = (pAddrEnd >> 12) & 0x03FF;

            int entry = (int)(physPDIdx*1024 + physPTIdx);
            int endEntry = (int)(physEndPDIdx*1024 + physEndPTIdx);

            for (; entry < endEntry; entry++)
            {
                if (UsedPhysPages.IsSet(entry))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Maps in the main kernel memory.
        /// </summary>
        [NoDebug]
        public void MapKernel()
        {
#if PAGING_TRACE
            BasicConsole.Write("Mapping 1st 1MiB...");
#endif

            uint VirtToPhysOffset = GetKernelVirtToPhysOffset();

            //Identity and virtual map the first 1MiB
            uint physAddr = 0;
            uint virtAddr = VirtToPhysOffset;
            for (; physAddr < 0x100000; physAddr += 4096, virtAddr += 4096)
            {
                Map(physAddr, physAddr, PageFlags.Present | PageFlags.Writeable);
                Map(physAddr, virtAddr, PageFlags.Present | PageFlags.Writeable);
            }

            Unmap(0, UpdateUsedPagesFlags.None);

#if PAGING_TRACE
            BasicConsole.WriteLine("Done.");
            BasicConsole.WriteLine("Mapping kernel...");
#endif

            //Map in the main kernel memory

            //Map all the required pages in between these two pointers.
            uint KernelMemStartPtr = (uint)GetKernelMemStartPtr();
            uint KernelMemEndPtr = (uint)GetKernelMemEndPtr();

#if PAGING_TRACE
            BasicConsole.WriteLine("Start pointer : " + (Framework.String)KernelMemStartPtr);
            BasicConsole.WriteLine("End pointer : " + (Framework.String)KernelMemEndPtr);
#endif

            // Round the start pointer down to nearest page
            KernelMemStartPtr = KernelMemStartPtr/4096*4096;

            // Round the end pointer up to nearest page
            KernelMemEndPtr = (KernelMemEndPtr/4096 + 1)*4096;

//#if PAGING_TRACE
            BasicConsole.WriteLine("VM: Start pointer : " + (String)KernelMemStartPtr);
            BasicConsole.WriteLine("VM: End pointer : " + (String)KernelMemEndPtr);
//#endif

            physAddr = KernelMemStartPtr - VirtToPhysOffset;

#if PAGING_TRACE
            BasicConsole.WriteLine("Phys addr : " + (Framework.String)physAddr);
            BasicConsole.DelayOutput(5);
#endif

            for (; KernelMemStartPtr <= KernelMemEndPtr; KernelMemStartPtr += 4096, physAddr += 4096)
            {
                Map(physAddr, KernelMemStartPtr, PageFlags.Present | PageFlags.Writeable | PageFlags.KernelOnly);
            }

#if PAGING_TRACE
            BasicConsole.WriteLine("Done.");
            BasicConsole.DelayOutput(5);
#endif
        }

        public void MapKernelProcessToMemoryLayout(MemoryLayout TheLayout)
        {
            uint cPtr = (uint)GetBSS_StartPtr();
            uint endPtr = (uint)GetBSS_EndPtr();
            uint cPhysPtr = GetPhysicalAddress(cPtr);

            for (; cPtr < endPtr; cPtr += 4096, cPhysPtr += 4096)
            {
                TheLayout.AddDataPage(cPhysPtr, cPtr);
            }
        }
        
        public bool IsWithinKernelFixedMemory(uint VAddr)
        {
            return VAddr >= (uint)GetKernelMemStartPtr() && VAddr < (uint)GetKernelMemEndPtr();
        }

        public uint[] GetBuiltInProcessVAddrs()
        {
            if (BuiltInProcessVAddrs == null)
            {
                uint startPtr = (uint)GetStaticFields_StartPtr();
                uint endPtr = (uint)GetStaticFields_EndPtr();
                uint isolatedKPtr = (uint)GetIsolatedKernelPtr();
                uint isolatedKH_MPPtr = (uint)GetIsolatedKernel_Hardware_MultiprocessingPtr();
                uint isolatedKH_DPtr = (uint)GetIsolatedKernel_Hardware_DevicesPtr();
                uint isolatedKVMPtr = (uint)GetIsolatedKernel_VirtualMemoryPtr();
                uint isolatedKFSPtr = (uint)GetIsolatedKernel_FrameworkPtr();
                uint isolatedKPPtr = (uint)GetIsolatedKernel_PipesPtr();

                uint startPhysPtr = GetPhysicalAddress(startPtr);

                int count = 0;

                uint cPtr = startPtr;
                uint cPhysPtr = startPhysPtr;
                for (; cPtr < endPtr; cPtr += 4096, cPhysPtr += 4096)
                {
                    if (cPtr != isolatedKPtr && cPtr != isolatedKH_MPPtr && cPtr != isolatedKH_DPtr &&
                        cPtr != isolatedKVMPtr && cPtr != isolatedKFSPtr && cPtr != isolatedKPPtr)
                    {
                        count++;
                    }
                }

                BuiltInProcessVAddrs = new uint[count];

                cPtr = startPtr;
                cPhysPtr = startPhysPtr;
                for (int i = 0; cPtr < endPtr; cPtr += 4096, cPhysPtr += 4096)
                {
                    if (cPtr != isolatedKPtr && cPtr != isolatedKH_MPPtr && cPtr != isolatedKH_DPtr &&
                        cPtr != isolatedKVMPtr && cPtr != isolatedKFSPtr && cPtr != isolatedKPPtr)
                    {
                        BuiltInProcessVAddrs[i++] = cPtr;
                    }
                }
            }

            return BuiltInProcessVAddrs;
        }

        /// <summary>
        ///     Maps the specified virtual address to the specified physical address.
        /// </summary>
        /// <param name="pAddr">The physical address to map to.</param>
        /// <param name="vAddr">The virtual address to map.</param>
        /// <param name="flags">The flags to apply to the allocated pages.</param>
        /// <param name="UpdateUsedPages">Which, if any, of the physical and virtual used pages lists to update.</param>
        [NoDebug]
        private void Map(uint pAddr, uint vAddr, PTEFlags flags,
            UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
#if PAGING_TRACE
            BasicConsole.WriteLine("Mapping addresses...");
#endif
            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.4.5.2.1");
            //}

            //Calculate page directory and page table indices
            uint virtPDIdx = vAddr >> 22;
            uint virtPTIdx = (vAddr >> 12) & 0x03FF;

            uint physPDIdx = pAddr >> 22;
            uint physPTIdx = (pAddr >> 12) & 0x03FF;

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.4.5.2.2");
            //}

#if PAGING_TRACE
            BasicConsole.WriteLine(((Framework.String)"pAddr=") + pAddr);
            BasicConsole.WriteLine(((Framework.String)"vAddr=") + vAddr);
            BasicConsole.WriteLine(((Framework.String)"physPDIdx=") + physPDIdx);
            BasicConsole.WriteLine(((Framework.String)"physPTIdx=") + physPTIdx);
            BasicConsole.WriteLine(((Framework.String)"virtPDIdx=") + virtPDIdx);
            BasicConsole.WriteLine(((Framework.String)"virtPTIdx=") + virtPTIdx);
#endif
            if ((UpdateUsedPages & UpdateUsedPagesFlags.Physical) != 0)
            {
                UsedPhysPages.Set((int)(physPDIdx*1024 + physPTIdx));
            }
            int virtIndex = (int)(virtPDIdx*1024 + virtPTIdx);
            AllUsedVirtPages.Set(virtIndex);
            if ((UpdateUsedPages & UpdateUsedPagesFlags.Virtual) != 0)
            {
                UsedVirtPages.Set(virtIndex);
            }

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.4.5.2.3");
            //}

            //Get a pointer to the pre-allocated page table
            uint* virtPTPtr = GetFixedPage(virtPDIdx);
#if PAGING_TRACE
            BasicConsole.WriteLine(((Framework.String)"ptPtr=") + (uint)virtPTPtr);

            if (Processes.Scheduler.OutputMessages)
            {
                if (vAddr == 0x00106000)
                {
                    BasicConsole.Write("Remapping 0x00106000 from ");
                    Framework.String valStr = "0x        ";
                    ExceptionMethods.FillString(GetPhysicalAddress(vAddr), 9, valStr);
                    BasicConsole.Write(valStr);
                    BasicConsole.Write(" to ");
                    ExceptionMethods.FillString(pAddr, 9, valStr);
                    BasicConsole.WriteLine(valStr);
                    BasicConsole.Write("ESP: ");
                    ExceptionMethods.FillString((uint)ExceptionMethods.StackPointer, 9, valStr);
                    BasicConsole.WriteLine(valStr);
                }
            }
#endif

            //Set the page table entry
            SetPageEntry(virtPTPtr, virtPTIdx, pAddr, flags);

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.4.5.2.5");
            //}

            //Set directory table entry
            SetDirectoryEntry(virtPDIdx, (uint*)GetPhysicalAddress((uint)virtPTPtr));

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.4.5.2.6");
            //}

            //Invalidate the page table entry so that mapping isn't CPU cached.
            InvalidatePTE(vAddr);

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.4.5.2.7");
            //}
        }

        /// <summary>
        ///     Gets the specified built-in, pre-allocated page table.
        ///     These page tables are not on the heap, they are part of the kernel pre-allocated memory.
        /// </summary>
        /// <param name="pageNum">The page number (directory index) of the page table to get.</param>
        /// <returns>A uint pointer to the page table.</returns>
        [NoDebug]
        private uint* GetFixedPage(uint pageNum)
        {
            return GetFirstPageTablePtr() + 1024*pageNum;
        }

        /// <summary>
        ///     Sets the specified page table entry.
        /// </summary>
        /// <param name="pageTablePtr">The page table to set the value in.</param>
        /// <param name="entry">The entry index (page table index) of the entry to set.</param>
        /// <param name="addr">The physical address to map the entry to.</param>
        [NoDebug]
        private void SetPageEntry(uint* pageTablePtr, uint entry, uint addr, PTEFlags flags)
        {
#if PAGING_TRACE
            BasicConsole.WriteLine("Setting page entry...");
            BasicConsole.WriteLine(((Framework.String)"pageTablePtr=") + (uint)pageTablePtr);
            BasicConsole.WriteLine(((Framework.String)"entry=") + entry);
            BasicConsole.WriteLine(((Framework.String)"addr=") + addr);
#endif

            pageTablePtr[entry] = addr | (uint)flags;

#if PAGING_TRACE
            if(pageTablePtr[entry] != (addr | 3))
            {
                BasicConsole.WriteLine("Set page entry verification failed.");
            }
#endif
        }

        /// <summary>
        ///     Sets the specified page directory entry.
        /// </summary>
        /// <param name="pageNum">The page number (directory index) of the entry to set.</param>
        /// <param name="pageTablePhysPtr">The physical address of the page table to set the entry to point at.</param>
        [NoDebug]
        private void SetDirectoryEntry(uint pageNum, uint* pageTablePhysPtr)
        {
            uint* dirPtr = GetPageDirectoryPtr();
#if PAGING_TRACE
            BasicConsole.WriteLine("Setting directory entry...");
            BasicConsole.WriteLine(((Framework.String)"dirPtr=") + (uint)dirPtr);
            BasicConsole.WriteLine(((Framework.String)"pageTablePhysPtr=") + (uint)pageTablePhysPtr);
            BasicConsole.WriteLine(((Framework.String)"pageNum=") + pageNum);
#endif

            dirPtr[pageNum] = (uint)pageTablePhysPtr | 7;
#if PAGING_TRACE
            if (dirPtr[pageNum] != ((uint)pageTablePhysPtr | 3))
            {
                BasicConsole.WriteLine("Set directory entry verification failed.");
            }
#endif
        }

        /// <summary>
        ///     Invalidates the cache of the specified page table entry.
        /// </summary>
        /// <param name="entry">The entry to invalidate.</param>
        [PluggedMethod(ASMFilePath = @"ASM\VirtualMemory\x86VirtualMemoryImplementation")]
        [SequencePriority(Priority = long.MinValue + 100)]
        private void InvalidatePTE(uint entry)
        {
        }

        /// <summary>
        ///     Gets the virtual to physical offset for the main kernel memory.
        /// </summary>
        /// <returns>The offset.</returns>
        /// <remarks>
        ///     This is the difference between the virtual address and the
        ///     physical address at which the bootloader loaded the kernel.
        /// </remarks>
        [PluggedMethod(ASMFilePath = null)]
        public static uint GetKernelVirtToPhysOffset()
        {
            return 0;
        }

        /// <summary>
        ///     Gets the page directory memory pointer.
        /// </summary>
        /// <returns>The pointer.</returns>
        [PluggedMethod(ASMFilePath = null)]
        private static uint* GetPageDirectoryPtr()
        {
            return null;
        }

        /// <summary>
        ///     Gets a pointer to the page table that is the first page table.
        /// </summary>
        /// <returns>The pointer.</returns>
        [PluggedMethod(ASMFilePath = null)]
        private static uint* GetFirstPageTablePtr()
        {
            return null;
        }

        /// <summary>
        ///     Gets a pointer to the start of the kernel in memory.
        /// </summary>
        /// <returns>The pointer.</returns>
        [PluggedMethod(ASMFilePath = null)]
        private static uint* GetKernelMemStartPtr()
        {
            return null;
        }

        /// <summary>
        ///     Gets a pointer to the end of the kernel in memory.
        /// </summary>
        /// <returns>The pointer.</returns>
        [PluggedMethod(ASMFilePath = null)]
        private static uint* GetKernelMemEndPtr()
        {
            return null;
        }

        [PluggedMethod(ASMFilePath = null)]
        private static uint* GetStaticFields_StartPtr()
        {
            return null;
        }

        [PluggedMethod(ASMFilePath = null)]
        private static uint* GetStaticFields_EndPtr()
        {
            return null;
        }

        [PluggedMethod(ASMFilePath = null)]
        private static uint* GetIsolatedKernelPtr()
        {
            return null;
        }

        [PluggedMethod(ASMFilePath = null)]
        private static uint* GetIsolatedKernel_Hardware_MultiprocessingPtr()
        {
            return null;
        }

        [PluggedMethod(ASMFilePath = null)]
        private static uint* GetIsolatedKernel_Hardware_DevicesPtr()
        {
            return null;
        }

        [PluggedMethod(ASMFilePath = null)]
        private static uint* GetIsolatedKernel_VirtualMemoryPtr()
        {
            return null;
        }

        [PluggedMethod(ASMFilePath = null)]
        private static uint* GetIsolatedKernel_FrameworkPtr()
        {
            return null;
        }

        [PluggedMethod(ASMFilePath = null)]
        private static uint* GetIsolatedKernel_PipesPtr()
        {
            return null;
        }


        [PluggedMethod(ASMFilePath = null)]
        private static uint* GetBSS_StartPtr()
        {
            return null;
        }

        [PluggedMethod(ASMFilePath = null)]
        private static uint* GetBSS_EndPtr()
        {
            return null;
        }

        [PluggedMethod(ASMFilePath = null)]
        public static uint GetCR3()
        {
            return 0;
        }
    }
}

#endif