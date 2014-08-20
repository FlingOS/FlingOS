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

        /// <summary>
        /// Initializes paged virtual memory.
        /// </summary>
        [Compiler.NoDebug]
        public static void Init()
        {
#warning This needs rewriting to allow for the whole higher-half kernel thing!
#pragma warning disable
            //TODO - Remove the warnings disables (they prevent the "unreachable code detected" warning)
            return;

            LoadPaging();
            if (LoadedPaging)
            {
                EnablePaging();

                BasicConsole.WriteLine("Paging enabled.");
            }
            else
            {
                BasicConsole.Write("Paging setup failed!");
            }
#pragma warning restore
        }

        /// <summary>
        /// Loads paged virtual memory.
        /// </summary>
        [Compiler.NoDebug]
        private static void LoadPaging()
        {
            //Load empty page directory
            uint* page_directory = GetPageDirectoryPtr();
            for (int i = 0; i < 1024; i++)
            {
                //attribute: supervisor level, read/write, not present.
                page_directory[i] = 0 | 2;
            }

            //Initially identity page everything
            //uint* page_table = GetFirstPageTablePtr();
            //uint address = 0;
            //for (int pageTableNum = 0; pageTableNum < 1024; pageTableNum++, page_table += 1024)
            //{
            //    for (int i = 0; i < 1024; i++)
            //    {
            //        page_table[i] = address | 3; // attributes: supervisor level, read/write, present.
            //        address += 4096; //advance the address to the next page boundary
            //    }

            //    page_directory[pageTableNum] = (uint)page_table;
            //    page_directory[pageTableNum] |= 3;// attributes: supervisor level, read/write, present
            //}

            //Create the self-referencing page-directory / table
            page_directory[1023] = ((uint)page_directory) | 3;
            
            //Map the first 4MB
            uint* first_page_table = GetFirstPageTablePtr();
            uint address = 0;
            for(uint i = 0; i < 1024; i++)
            {
                first_page_table[i] = address | 3;
                address += 4096;
            }
            page_directory[0] = (uint)(first_page_table) | 3;

            //TODO - Remove this nasty hack (put in place so I could develop USB EHCI driver)
            //Map the USB 4MB
            uint* usb_page_table = GetUSBPageTablePtr();
            address = 1013u * 1024u * 4096u;
            for (uint i = 0; i < 1024; i++)
            {
                usb_page_table[i] = address | 3;
                address += 4096;
            }
            page_directory[1013] = (uint)(usb_page_table) | 3;
            
            InitKernelPages();
        }
        /// <summary>
        /// Initializes the pages that cover the kernel's memory.
        /// </summary>
        [Compiler.NoDebug]
        private static void InitKernelPages()
        {
            uint* page_directory = GetPageDirectoryPtr();

            //Initially, set up identity paged memory only for the kernel's required amount
            uint* kernel_page_table = GetKernelPageTablePtr();
            uint* kernel_page_table_end = kernel_page_table + (1024 * 48);
            uint* kernel_MemStartPtr = GetKernelMemStartPtr();
            uint* kernel_MemEndPtr = GetKernelMemEndPtr();
            uint startPDIndex = (uint)kernel_MemStartPtr >> 22;
            uint startPTIndex = ((uint)kernel_MemStartPtr >> 12) & 0x03FF;
            uint endPDIndex = (uint)kernel_MemEndPtr >> 22;
            uint endPTIndex = ((uint)kernel_MemEndPtr >> 12) & 0x03FF;

            if (endPDIndex - startPDIndex > 32)
            {
                ExceptionMethods.Throw(new FOS_System.Exception(
                    ((FOS_System.String)"Unable to set up paging! Insufficient pages! : ") +
                    startPDIndex + ", " + endPDIndex
                    ));
            }
            
            uint address = ((uint)kernel_MemStartPtr) & 0xFFFFF000;
            uint startPT = startPTIndex;
            uint endPT = 0;
            for (uint j = startPDIndex; j <= endPDIndex; j++)
            {
                endPT = j == endPDIndex ? endPTIndex : 1023;

#if PAGING_TRACE
                BasicConsole.WriteLine(((FOS_System.String)"      PD Index: ") + j);
                BasicConsole.WriteLine(((FOS_System.String)"Start PT Index: ") + startPT);
                BasicConsole.WriteLine(((FOS_System.String)"  End PT Index: ") + endPT);
                BasicConsole.WriteLine(((FOS_System.String)" Start Address: ") + address);
                BasicConsole.DelayOutput(1);
#endif

                page_directory[j] = ((uint)kernel_page_table) | 3;
                
                for (uint i = startPT; i <= endPT; i++)
                {                    
                    kernel_page_table[i] = address | 3;
                    address += 4096;
                }

                if (kernel_page_table == kernel_page_table_end)
                {
                    BasicConsole.WriteLine("Out of pages to allocate for kernel!");
                    return;
                }

                startPT = 0;

                kernel_page_table += 1024;
            }
#if PAGING_TRACE
            BasicConsole.WriteLine("Kernel mapping completed.");
#endif

            LoadedPaging = true;
        }

        /// <summary>
        /// Enables paging.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static void EnablePaging()
        {
        }

        /// <summary>
        /// Gets the page directory memory pointer.
        /// </summary>
        /// <returns>The pointer.</returns>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\Paging\Paging")]
        [Compiler.SequencePriority(Priority = long.MaxValue)]
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
        /// TODO - Remove this hacky piece of junk
        /// </summary>
        /// <returns>The pointer.</returns>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static uint* GetUSBPageTablePtr()
        {
            return null;
        }
        /// <summary>
        /// Gets a pointer to the page table that covers the kernel's memory.
        /// </summary>
        /// <returns>The pointer.</returns>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static uint* GetKernelPageTablePtr()
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
