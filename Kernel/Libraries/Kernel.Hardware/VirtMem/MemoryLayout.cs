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
            bool reenable = Processes.Scheduler.Enabled;
            if(reenable)
            {
                Processes.Scheduler.Disable();
            }

            //BasicConsole.WriteLine("Adding code page...");
            if (!CodePages.Contains(vAddr))
            {
                CodePages.Add(vAddr, pAddr);
            }

            if (reenable)
            {
                Processes.Scheduler.Enable();
            }
        }
        public void AddDataPage(uint pAddr, uint vAddr)
        {
            bool reenable = Processes.Scheduler.Enabled;
            if (reenable)
            {
                Processes.Scheduler.Disable();
            }

            //BasicConsole.WriteLine("Adding data page...");
            if (!DataPages.Contains(vAddr))
            {
                DataPages.Add(vAddr, pAddr);
            }

            if (reenable)
            {
                Processes.Scheduler.Enable();
            }
        }
        public void RemovePage(uint vAddr)
        {
            bool reenable = Processes.Scheduler.Enabled;
            if (reenable)
            {
                Processes.Scheduler.Disable();
            }

            //BasicConsole.WriteLine("Removing page...");
            CodePages.Remove(vAddr);
            DataPages.Remove(vAddr);

            if (reenable)
            {
                Processes.Scheduler.Enable();
            }
        }

        //bool loadPrint = true;
        //bool unloadPrint = true;
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
                //if (loadPrint)
                //{
                //    BasicConsole.WriteLine(((FOS_System.String)"Loading code page v->p: ") + vAddr + " -> " + pAddr);
                //}
                VirtMemManager.Map(pAddr, vAddr, 4096, flags, false);
            }

            flags = ProcessIsUM ? VirtMemImpl.PageFlags.None : VirtMemImpl.PageFlags.KernelOnly;
            for (int i = 0; i < DataPages.Keys.Count; i++)
            {
                uint vAddr = DataPages.Keys[i];
                uint pAddr = DataPages[vAddr];
                
#if MEMLAYOUT_TRACE
                BasicConsole.WriteLine("Loading data page...");
#endif

                //if (loadPrint)
                //{
                //    BasicConsole.WriteLine(((FOS_System.String)"Loading data page v->p: ") + vAddr + " -> " + pAddr);
                //}

                VirtMemManager.Map(pAddr, vAddr, 4096, flags, false);
            }

            //if (loadPrint)
            //{
            //    //BasicConsole.DelayOutput(1);
            //    loadPrint = false;
            //}
        }
        public void Unload()
        {
            for (int i = 0; i < CodePages.Keys.Count && i < CodePages.Values.Count; i++)
            {
#if MEMLAYOUT_TRACE
                BasicConsole.WriteLine("Unloading code page...");
#endif

                //if (unloadPrint)
                //{
                //    BasicConsole.WriteLine(((FOS_System.String)"Unloading code page v->p: ") + CodePages.Keys[i]);
                //}

                VirtMemManager.Unmap(CodePages.Keys[i], false);
            }
            for (int i = 0; i < DataPages.Keys.Count && i < DataPages.Values.Count; i++)
            {
#if MEMLAYOUT_TRACE
                BasicConsole.WriteLine("Unloading data page...");
#endif

                //if (unloadPrint)
                //{
                //    BasicConsole.WriteLine(((FOS_System.String)"Unloading data page v->p: ") + DataPages.Keys[i]);
                //}

               VirtMemManager.Unmap(DataPages.Keys[i], false);
            }

            //if (unloadPrint)
            //{
            //    //BasicConsole.DelayOutput(1);
            //    unloadPrint = false;
            //}
        }
    }
}
