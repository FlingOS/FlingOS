using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel
{
    [Compiler.PluggedClass]
    public unsafe static class Paging
    {
        static bool LoadedPaging = false;

        [Compiler.NoDebug]
        public static void Init()
        {
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
        }

        [Compiler.NoDebug]
        public static void LoadPaging()
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
            
            //Map the first 1MB
            uint* first_page_table = GetFirstPageTablePtr();
            uint address = 0;
            for(uint i = 0; i < 1024; i++)
            {
                first_page_table[i] = address | 3;
                address += 4096;
            }
            page_directory[0] = (uint)(first_page_table) | 3;

            InitKernelPages();
        }
        [Compiler.NoDebug]
        public static void InitKernelPages()
        {
            uint* page_directory = GetPageDirectoryPtr();

            //Initially, set up identity paged memory only for the kernel's required amount
            uint* kernel_page_table = GetKernelPageTablePtr();
            uint* kernel_MemStartPtr = GetKernelMemStartPtr();
            uint* kernel_MemEndPtr = GetKernelMemEndPtr();
            uint startPDIndex = (uint)kernel_MemStartPtr >> 22;
            uint startPTIndex = ((uint)kernel_MemStartPtr >> 12) & 0x03FF;
            uint endPDIndex = (uint)kernel_MemEndPtr >> 22;
            uint endPTIndex = ((uint)kernel_MemEndPtr >> 12) & 0x03FF;

            if (endPDIndex != startPDIndex)
            {
                ExceptionMethods.Throw(new FOS_System.Exception(
                    ((FOS_System.String)"Unable to set up paging! endPDIndex != startPDIndex : ") +
                    startPDIndex + ", " + endPDIndex
                    ));
            }
            else
            {
                page_directory[startPDIndex] = ((uint)kernel_page_table) | 3;

                uint address = ((uint)kernel_MemStartPtr) & 0xFFFFF000;
                for (uint i = startPTIndex; i <= endPTIndex; i++)
                {
                    kernel_page_table[i] = address | 3;
                    address += 4096;
                }

                LoadedPaging = true;
            }
        }

        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static void EnablePaging()
        {
        }

        [Compiler.PluggedMethod(ASMFilePath = @"ASM\Paging\Paging")]
        [Compiler.SequencePriority(Priority = long.MaxValue)]
        public static uint* GetPageDirectoryPtr()
        {
            return null;
        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static uint* GetFirstPageTablePtr()
        {
            return null;
        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static uint* GetKernelPageTablePtr()
        {
            return null;
        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static uint* GetKernelMemStartPtr()
        {
            return null;
        }
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static uint* GetKernelMemEndPtr()
        {
            return null;
        }
    }
}
