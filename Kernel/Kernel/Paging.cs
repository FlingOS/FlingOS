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
        public static void Init()
        {
            LoadPaging();
            EnablePaging();
        }

        [Compiler.NoDebug]
        public static void LoadPaging()
        {
            uint* page_directory = GetPageDirectoryPtr();
            for (int i = 0; i < 1024; i++)
            {
                //attribute: supervisor level, read/write, not present.
                page_directory[i] = 0 | 2;
            }

            uint* page_table = GetFirstPageTablePtr();
            uint address = 0;
            for (int pageTableNum = 0; pageTableNum < 1024; pageTableNum++, page_table += 1024)
            {
                for (int i = 0; i < 1024; i++)
                {
                    page_table[i] = address | 3; // attributes: supervisor level, read/write, present.
                    address = address + 4096; //advance the address to the next page boundary
                }

                page_directory[pageTableNum] = (uint)page_table;
                page_directory[pageTableNum] |= 3;// attributes: supervisor level, read/write, present
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
