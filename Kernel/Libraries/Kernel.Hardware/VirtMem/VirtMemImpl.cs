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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.VirtMem
{
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
        public virtual void Map(uint pAddr, uint vAddr, bool UpdateUsedPages = true)
        {
            Map(pAddr, vAddr, PageFlags.Present | PageFlags.KernelOnly | PageFlags.Writeable, UpdateUsedPages);
        }
        public abstract void Map(uint pAddr, uint vAddr, PageFlags flags, bool UpdateUsedPages = true);
        public abstract void Unmap(uint vAddr, bool UpdateUsedPages = true);
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
