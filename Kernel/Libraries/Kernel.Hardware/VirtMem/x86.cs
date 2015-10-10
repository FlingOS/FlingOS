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
    
#define PAGING_TRACE
#undef PAGING_TRACE

using System;
using Kernel.FOS_System.Collections;

namespace Kernel.Hardware.VirtMem
{
    /// <summary>
    /// Provides methods for setting up paged virtual memory.
    /// </summary>
    public unsafe class x86 : VirtMemImpl
    {
        static x86()
        {
            VirtMemManager.Init(new x86());
        }

        [Flags]
        public enum PTEFlags : uint
        {
            None = 0x0,
            Present = 0x1,
            Writeable = 0x2,
            UserAllowed = 0x4,
            PagelevelWriteThrough = 0x8,
            PageLevelCacheDisable = 0x10,
            PAT = 0x80,
            Global = 0x100
        }

        //1024 * 1024 = 1048576
        /// <summary>
        /// Bitmap of all the free (unmapped) physical pages of memory.
        /// </summary>
        private Bitmap UsedPhysPages = new Bitmap(1048576);
        /// <summary>
        /// Bitmap of all the free (unmapped) virtual pages of memory.
        /// </summary>
        private Bitmap UsedVirtPages = new Bitmap(1048576);

        /// <summary>
        /// Initialises the new x86 object.
        /// </summary>
        [Drivers.Compiler.Attributes.NoDebug]
        public x86()
        {
        }

        /// <summary>
        /// Tests the virtual memory system.
        /// </summary>
        public override void Test()
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
                BasicConsole.WriteLine(((FOS_System.String)"pAddr=") + pAddr);
                BasicConsole.WriteLine(((FOS_System.String)"vAddr1=") + vAddr1);
                BasicConsole.WriteLine(((FOS_System.String)"vAddr2=") + vAddr2);
                BasicConsole.WriteLine(((FOS_System.String)"vPtr1=") + (uint)vPtr1);
                BasicConsole.WriteLine(((FOS_System.String)"vPtr2=") + (uint)vPtr2);

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

            Hardware.Devices.Timer.InitDefault();
            Hardware.Devices.Timer.Default.Wait(3000);
        }
        /// <summary>
        /// Prints out information about the free physical and virtual pages.
        /// </summary>
        public override void PrintUsedPages()
        {
            BasicConsole.WriteLine("Used physical pages: " + (FOS_System.String)UsedPhysPages.Count);
            BasicConsole.WriteLine("Used virtual pages : " + (FOS_System.String)UsedVirtPages.Count);
            BasicConsole.DelayOutput(5);
        }

        [Drivers.Compiler.Attributes.NoDebug]
        public override uint FindFreePhysPageAddrs(int num)
        {
            int result = UsedPhysPages.FindContiguousClearEntries(num);
            if (result == -1)
            {
                BasicConsole.WriteLine("Error finding free physical pages!");
                BasicConsole.DelayOutput(10);

                ExceptionMethods.Throw(new FOS_System.Exceptions.OutOfMemoryException("Could not find any more free physical pages."));
            }
            
            return (uint)(result * 4096);
        }
        [Drivers.Compiler.Attributes.NoDebug]
        public override uint FindFreeVirtPageAddrs(int num)
        {
            int result = UsedVirtPages.FindContiguousClearEntries(num);
            if (result == -1)
            {
                BasicConsole.WriteLine("Error finding free virtual pages!");
                BasicConsole.DelayOutput(10);
                
                ExceptionMethods.Throw(new FOS_System.Exceptions.OutOfMemoryException("Could not find any more free virtual pages."));
            }

            return (uint)(result * 4096);
        }

        /// <summary>
        /// Maps the specified virtual address to the specified physical address.
        /// </summary>
        /// <param name="pAddr">The physical address to map to.</param>
        /// <param name="vAddr">The virtual address to map.</param>
        [Drivers.Compiler.Attributes.NoDebug]
        public override void Map(uint pAddr, uint vAddr, PageFlags flags, UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
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
        [Drivers.Compiler.Attributes.NoDebug]
        private void Map(uint pAddr, uint vAddr, PTEFlags flags, UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
#if PAGING_TRACE
            BasicConsole.WriteLine("Mapping addresses...");
#endif
            //Calculate page directory and page table indices
            uint virtPDIdx = vAddr >> 22;
            uint virtPTIdx = (vAddr >> 12) & 0x03FF;

            uint physPDIdx = pAddr >> 22;
            uint physPTIdx = (pAddr >> 12) & 0x03FF;

#if PAGING_TRACE
            BasicConsole.WriteLine(((FOS_System.String)"pAddr=") + pAddr);
            BasicConsole.WriteLine(((FOS_System.String)"vAddr=") + vAddr);
            BasicConsole.WriteLine(((FOS_System.String)"physPDIdx=") + physPDIdx);
            BasicConsole.WriteLine(((FOS_System.String)"physPTIdx=") + physPTIdx);
            BasicConsole.WriteLine(((FOS_System.String)"virtPDIdx=") + virtPDIdx);
            BasicConsole.WriteLine(((FOS_System.String)"virtPTIdx=") + virtPTIdx);
#endif
            if ((UpdateUsedPages & UpdateUsedPagesFlags.Physical) != 0)
            {
                UsedPhysPages.Set((int)((physPDIdx * 1024) + physPTIdx));
            }
            if((UpdateUsedPages & UpdateUsedPagesFlags.Virtual) != 0)
            {
                UsedVirtPages.Set((int)((virtPDIdx * 1024) + virtPTIdx));
            }

            //Get a pointer to the pre-allocated page table
            uint* virtPTPtr = GetFixedPage(virtPDIdx);
#if PAGING_TRACE
            BasicConsole.WriteLine(((FOS_System.String)"ptPtr=") + (uint)virtPTPtr);
#endif
            //Set the page table entry
            SetPageEntry(virtPTPtr, virtPTIdx, pAddr, flags);
            //Set directory table entry
            SetDirectoryEntry(virtPDIdx, (uint*)GetPhysicalAddress((uint)virtPTPtr));

            //Invalidate the page table entry so that mapping isn't CPU cached.
            InvalidatePTE(vAddr);
        }
        [Drivers.Compiler.Attributes.NoDebug]
        public override void Unmap(uint vAddr, UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
            uint pAddr = GetPhysicalAddress(vAddr);

            uint virtPDIdx = vAddr >> 22;
            uint virtPTIdx = (vAddr >> 12) & 0x03FF;

            uint physPDIdx = pAddr >> 22;
            uint physPTIdx = (pAddr >> 12) & 0x03FF;

            if ((UpdateUsedPages & UpdateUsedPagesFlags.Physical) != 0)
            {
                UsedPhysPages.Clear((int)((physPDIdx * 1024) + physPTIdx));
            }
            if ((UpdateUsedPages & UpdateUsedPagesFlags.Virtual) != 0)
            {
                UsedVirtPages.Clear((int)((virtPDIdx * 1024) + virtPTIdx));
            }

            uint* virtPTPtr = GetFixedPage(virtPDIdx);
            SetPageEntry(virtPTPtr, virtPTIdx, 0, PTEFlags.None);
            InvalidatePTE(vAddr);            
        }
        /// <summary>
        /// Gets the physical address for the specified virtual address.
        /// </summary>
        /// <param name="vAddr">The virtual address to get the physical address of.</param>
        /// <returns>The physical address.</returns>
        /// <remarks>
        /// This has an undefined return value and behaviour if the virtual address is not mapped.
        /// </remarks>
        [Drivers.Compiler.Attributes.NoDebug]
        public override uint GetPhysicalAddress(uint vAddr)
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
            return ((ptPtr[ptIdx] & 0xFFFFF000) + (vAddr & 0xFFF));
        }

        /// <summary>
        /// Maps in the main kernel memory.
        /// </summary>
        [Drivers.Compiler.Attributes.NoDebug]
        public override void MapKernel()
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

#if PAGING_TRACE
            BasicConsole.WriteLine("Done.");
            BasicConsole.WriteLine("Mapping kernel...");
#endif

            //Map in the main kernel memory

            //Map all the required pages in between these two pointers.
            uint KernelMemStartPtr = (uint)GetKernelMemStartPtr();
            uint KernelMemEndPtr = (uint)GetKernelMemEndPtr();
            
#if PAGING_TRACE
            BasicConsole.WriteLine("Start pointer : " + (FOS_System.String)KernelMemStartPtr);
            BasicConsole.WriteLine("End pointer : " + (FOS_System.String)KernelMemEndPtr);
#endif

            // Round the start pointer down to nearest page
            KernelMemStartPtr = ((KernelMemStartPtr / 4096) * 4096);

            // Round the end pointer up to nearest page
            KernelMemEndPtr = (((KernelMemEndPtr / 4096) + 1) * 4096);
            
#if PAGING_TRACE
            BasicConsole.WriteLine("Start pointer : " + (FOS_System.String)KernelMemStartPtr);
            BasicConsole.WriteLine("End pointer : " + (FOS_System.String)KernelMemEndPtr);
#endif
            
            physAddr = KernelMemStartPtr - VirtToPhysOffset;
            
#if PAGING_TRACE
            BasicConsole.WriteLine("Phys addr : " + (FOS_System.String)physAddr);
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

        /// <summary>
        /// Gets the specified built-in, pre-allocated page table. 
        /// These page tables are not on the heap, they are part of the kernel pre-allocated memory.
        /// </summary>
        /// <param name="pageNum">The page number (directory index) of the page table to get.</param>
        /// <returns>A uint pointer to the page table.</returns>
        [Drivers.Compiler.Attributes.NoDebug]
        private uint* GetFixedPage(uint pageNum)
        {
            return GetFirstPageTablePtr() + (1024 * pageNum);
        }
        /// <summary>
        /// Sets the specified page table entry.
        /// </summary>
        /// <param name="pageTablePtr">The page table to set the value in.</param>
        /// <param name="entry">The entry index (page table index) of the entry to set.</param>
        /// <param name="addr">The physical address to map the entry to.</param>
        [Drivers.Compiler.Attributes.NoDebug]
        private void SetPageEntry(uint* pageTablePtr, uint entry, uint addr, PTEFlags flags)
        {
#if PAGING_TRACE
            BasicConsole.WriteLine("Setting page entry...");
            BasicConsole.WriteLine(((FOS_System.String)"pageTablePtr=") + (uint)pageTablePtr);
            BasicConsole.WriteLine(((FOS_System.String)"entry=") + entry);
            BasicConsole.WriteLine(((FOS_System.String)"addr=") + addr);
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
        /// Sets the specified page directory entry.
        /// </summary>
        /// <param name="pageNum">The page number (directory index) of the entry to set.</param>
        /// <param name="pageTablePhysPtr">The physical address of the page table to set the entry to point at.</param>
        [Drivers.Compiler.Attributes.NoDebug]
        private void SetDirectoryEntry(uint pageNum, uint* pageTablePhysPtr)
        {
            uint* dirPtr = GetPageDirectoryPtr();
#if PAGING_TRACE
            BasicConsole.WriteLine("Setting directory entry...");
            BasicConsole.WriteLine(((FOS_System.String)"dirPtr=") + (uint)dirPtr);
            BasicConsole.WriteLine(((FOS_System.String)"pageTablePhysPtr=") + (uint)pageTablePhysPtr);
            BasicConsole.WriteLine(((FOS_System.String)"pageNum=") + pageNum);
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
        /// Invalidates the cache of the specified page table entry.
        /// </summary>
        /// <param name="entry">The entry to invalidate.</param>
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = @"ASM\VirtMem\x86")]
        [Drivers.Compiler.Attributes.SequencePriority(Priority = long.MaxValue)]
        private void InvalidatePTE(uint entry)
        {

        }

        /// <summary>
        /// Gets the virtual to physical offset for the main kernel memory.
        /// </summary>
        /// <returns>The offset.</returns>
        /// <remarks>
        /// This is the difference between the virtual address and the 
        /// physical address at which the bootloader loaded the kernel.
        /// </remarks>
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static uint GetKernelVirtToPhysOffset()
        {
            return 0;
        }
        /// <summary>
        /// Gets the page directory memory pointer.
        /// </summary>
        /// <returns>The pointer.</returns>
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        private static uint* GetPageDirectoryPtr()
        {
            return null;
        }
        /// <summary>
        /// Gets a pointer to the page table that is the first page table.
        /// </summary>
        /// <returns>The pointer.</returns>
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        private static uint* GetFirstPageTablePtr()
        {
            return null;
        }
        /// <summary>
        /// Gets a pointer to the start of the kernel in memory.
        /// </summary>
        /// <returns>The pointer.</returns>
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        private static uint* GetKernelMemStartPtr()
        {
            return null;
        }
        /// <summary>
        /// Gets a pointer to the end of the kernel in memory.
        /// </summary>
        /// <returns>The pointer.</returns>
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        private static uint* GetKernelMemEndPtr()
        {
            return null;
        }


        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static uint GetCR3()
        {
            return 0;
        }
    }
}
