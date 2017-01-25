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

using Kernel.Framework;

namespace Kernel.VirtualMemory.Implementations
{
    /// <summary>
    ///     Represents a specific implementation of a virtual memory system.
    /// </summary>
    public interface IVirtualMemoryImplementation : IObject
    {
        /// <summary>
        ///     Tests the virtual memory system.
        /// </summary>
        void Test();

        /// <summary>
        ///     Prints out information about the free physical and virtual pages.
        /// </summary>
        void PrintUsedPages();

        uint FindFreePhysPageAddrs(int num);
        uint FindFreeVirtPageAddrs(int num);
        uint FindFreeVirtPageAddrsForKernel(int num);

        /// <summary>
        ///     Maps the specified virtual address to the specified physical address.
        /// </summary>
        /// <remarks>
        ///     Uses the flags Present, KernelOnly and Writeable as defaults.
        /// </remarks>
        /// <param name="pAddr">The physical address to map to.</param>
        /// <param name="vAddr">The virtual address to map.</param>
        /// <param name="UpdateUsedPages">Which, if any, of the physical and virtual used pages lists to update.</param>
        void Map(uint pAddr, uint vAddr, UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both);

        /// <summary>
        ///     Maps the specified virtual address to the specified physical address.
        /// </summary>
        /// <param name="pAddr">The physical address to map to.</param>
        /// <param name="vAddr">The virtual address to map.</param>
        /// <param name="flags">The flags to apply to the allocated pages.</param>
        /// <param name="UpdateUsedPages">Which, if any, of the physical and virtual used pages lists to update.</param>
        void Map(uint pAddr, uint vAddr, PageFlags flags,
            UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both);

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
        void Unmap(uint vAddr, UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both);

        /// <summary>
        ///     Gets the physical address for the specified virtual address.
        /// </summary>
        /// <param name="vAddr">The virtual address to get the physical address of.</param>
        /// <returns>The physical address.</returns>
        /// <remarks>
        ///     This has an undefined return value and behaviour if the virtual address is not mapped.
        /// </remarks>
        uint GetPhysicalAddress(uint vAddr);

        bool IsVirtualMapped(uint vAddr);
        bool AreAnyPhysicalMapped(uint pAddrStart, uint pAddrEnd);

        /// <summary>
        ///     Maps in the main kernel memory.
        /// </summary>
        void MapKernel();

        bool IsWithinKernelFixedMemory(uint VAddr);

        void MapKernelProcessToMemoryLayout(MemoryLayout TheLayout);
        uint[] GetBuiltInProcessVAddrs();
    }
}