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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.VirtMem
{
    [Flags]
    public enum UpdateUsedPagesFlags : byte
    {
        None = 0,
        Physical = 1,
        Virtual = 2,
        Both = 3
    }

    /// <summary>
    /// Represents a specific implementation of a virtual memory system.
    /// </summary>
    public abstract class VirtMemImpl : FOS_System.Object
    {
        [Flags]
        public enum PageFlags : uint
        {
            None = 0,
            Present = 1,
            Writeable = 2,
            KernelOnly = 4
        }

        /// <summary>
        /// Tests the virtual memory system.
        /// </summary>
        public abstract void Test();
        /// <summary>
        /// Prints out information about the free physical and virtual pages.
        /// </summary>
        public abstract void PrintUsedPages();

        public abstract uint FindFreePhysPageAddr();
        public abstract uint FindFreeVirtPageAddr();

        /// <summary>
        /// Maps the specified virtual address to the specified physical address.
        /// </summary>
        /// <param name="pAddr">The physical address to map to.</param>
        /// <param name="vAddr">The virtual address to map.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        public virtual void Map(uint pAddr, uint vAddr, UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both)
        {
            Map(pAddr, vAddr, PageFlags.Present | PageFlags.KernelOnly | PageFlags.Writeable, UpdateUsedPages);
        }
        public abstract void Map(uint pAddr, uint vAddr, PageFlags flags, UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both);
        public abstract void Unmap(uint vAddr, UpdateUsedPagesFlags UpdateUsedPages = UpdateUsedPagesFlags.Both);
        /// <summary>
        /// Gets the physical address for the specified virtual address.
        /// </summary>
        /// <param name="vAddr">The virtual address to get the physical address of.</param>
        /// <returns>The physical address.</returns>
        /// <remarks>
        /// This has an undefined return value and behaviour if the virtual address is not mapped.
        /// </remarks>
        public abstract uint GetPhysicalAddress(uint vAddr);

        /// <summary>
        /// Maps in the main kernel memory.
        /// </summary>
        public abstract void MapKernel();
    }
}
