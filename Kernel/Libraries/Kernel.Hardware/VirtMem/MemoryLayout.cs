#define MEMLAYOUT_TRACE
#undef MEMLAYOUT_TRACE

using System;
using Kernel.FOS_System.Collections;

namespace Kernel.Hardware.VirtMem
{
    public class MemoryLayout : FOS_System.Object
    {
        public UInt32Dictionary CodePages = new UInt32Dictionary();
        public UInt32Dictionary DataPages = new UInt32Dictionary();

        public void AddCodePage(uint pAddr, uint vAddr)
        {
            if (!CodePages.Contains(vAddr))
            {
                CodePages.Add(vAddr, pAddr);
            }
        }
        public void AddDataPage(uint pAddr, uint vAddr)
        {
            if (!DataPages.Contains(vAddr))
            {
                DataPages.Add(vAddr, pAddr);
            }
        }
        public void RemovePage(uint vAddr)
        {
            CodePages.Remove(vAddr);
            DataPages.Remove(vAddr);
        }

        public void Load(bool ProcessIsUM)
        {
            VirtMemImpl.PageFlags flags = ProcessIsUM ? VirtMemImpl.PageFlags.None : VirtMemImpl.PageFlags.KernelOnly;
            for (int i = 0; i < CodePages.Keys.Count; i++)
            {
                uint vAddr = CodePages.Keys[i];
                uint pAddr = CodePages[vAddr];

#if MEMLAYOUT_TRACE
                BasicConsole.WriteLine("Loading code page...");
#endif

                //VirtMemManager.Map(pAddr, vAddr, 4096, flags, false);
            }

            flags = ProcessIsUM ? VirtMemImpl.PageFlags.None : VirtMemImpl.PageFlags.KernelOnly;
            for (int i = 0; i < DataPages.Keys.Count; i++)
            {
                uint vAddr = DataPages.Keys[i];
                uint pAddr = DataPages[vAddr];
                
#if MEMLAYOUT_TRACE
                BasicConsole.WriteLine("Loading data page...");
#endif

                //VirtMemManager.Map(pAddr, vAddr, 4096, flags, false);
            }
        }
        public void Unload()
        {
            for (int i = 0; i < CodePages.Keys.Count && i < CodePages.Values.Count; i++)
            {
#if MEMLAYOUT_TRACE
                BasicConsole.WriteLine("Unloading code page...");
#endif

                //VirtMemManager.Unmap(CodePages.Keys[i], false);
            }
            for (int i = 0; i < DataPages.Keys.Count && i < DataPages.Values.Count; i++)
            {
#if MEMLAYOUT_TRACE
                BasicConsole.WriteLine("Unloading data page...");
#endif

                //VirtMemManager.Unmap(DataPages.Keys[i], false);
            }
        }
    }
}
