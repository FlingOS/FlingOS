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
    
#define PAGING_TRACE
#undef PAGING_TRACE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel
{
    /// <summary>
    /// Provides methods for setting up paged virtual memory.
    /// </summary>
    [Compiler.PluggedClass]
    public unsafe static class Paging
    {
        /// <summary>
        /// Whether paging has been loaded or not.
        /// </summary>
        static bool LoadedPaging = false;
                
        public static void TestPaging()
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

        public static void Map(uint pAddr, uint vAddr)
        {
            BasicConsole.WriteLine("Mapping addresses...");
            
            uint pdIdx = vAddr >> 22;
            uint ptIdx = (vAddr >> 12) & 0x03FF;

            BasicConsole.WriteLine(((FOS_System.String)"pAddr=") + pAddr);
            BasicConsole.WriteLine(((FOS_System.String)"vAddr=") + vAddr);
            BasicConsole.WriteLine(((FOS_System.String)"pdIdx=") + pdIdx);
            BasicConsole.WriteLine(((FOS_System.String)"ptIdx=") + ptIdx);

            uint* ptPtr = GetFixedPage(pdIdx);
            BasicConsole.WriteLine(((FOS_System.String)"ptPtr=") + (uint)ptPtr);
            
            SetPageEntry(ptPtr, ptIdx, pAddr);
            SetDirectoryEntry(pdIdx, (uint*)((uint)ptPtr - GetVToPOffset()));

            InvalidatePTE(vAddr);
        }

        private static uint* GetFixedPage(uint pageNum)
        {
            return GetFirstPageTablePtr() + (1024 * pageNum);
        }
        private static void SetPageEntry(uint* pageTablePtr, uint entry, uint addr)
        {
            BasicConsole.WriteLine("Setting page entry...");
            BasicConsole.WriteLine(((FOS_System.String)"pageTablePtr=") + (uint)pageTablePtr);
            BasicConsole.WriteLine(((FOS_System.String)"entry=") + entry);
            BasicConsole.WriteLine(((FOS_System.String)"addr=") + addr);
            pageTablePtr[entry] = addr | 3;

            if(pageTablePtr[entry] != (addr | 3))
            {
                BasicConsole.WriteLine("Set page entry verification failed.");
            }
        }
        private static void SetDirectoryEntry(uint pageNum, uint* pageTablePhysPtr)
        {
            uint* dirPtr = GetPageDirectoryPtr();
            BasicConsole.WriteLine("Setting directory entry...");
            BasicConsole.WriteLine(((FOS_System.String)"dirPtr=") + (uint)dirPtr);
            BasicConsole.WriteLine(((FOS_System.String)"pageTablePhysPtr=") + (uint)pageTablePhysPtr);
            BasicConsole.WriteLine(((FOS_System.String)"pageNum=") + pageNum);

            dirPtr[pageNum] = (uint)pageTablePhysPtr | 3;

            if (dirPtr[pageNum] != ((uint)pageTablePhysPtr | 3))
            {
                BasicConsole.WriteLine("Set directory entry verification failed.");
            }
        }
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\Paging\Paging")]
        [Compiler.SequencePriority(Priority = long.MaxValue)]
        private static void InvalidatePTE(uint entry)
        {

        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static uint GetVToPOffset()
        {
            return 0;
        }

        /// <summary>
        /// Gets the page directory memory pointer.
        /// </summary>
        /// <returns>The pointer.</returns>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static uint* GetPageDirectoryPtr()
        {
            return null;
        }
        /// <summary>
        /// Gets a pointer to the page table that is the first page table.
        /// </summary>
        /// <returns>The pointer.</returns>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static uint* GetFirstPageTablePtr()
        {
            return null;
        }
        /// <summary>
        /// Gets a pointer to the start of the kernel in memory.
        /// </summary>
        /// <returns>The pointer.</returns>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static uint* GetKernelMemStartPtr()
        {
            return null;
        }
        /// <summary>
        /// Gets a pointer to the end of the kernel in memory.
        /// </summary>
        /// <returns>The pointer.</returns>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static uint* GetKernelMemEndPtr()
        {
            return null;
        }
    }
}
