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

        [Drivers.Compiler.Attributes.NoDebug]
        public void AddCodePage(uint pAddr, uint vAddr)
        {
            //bool reenable = Processes.Scheduler.Enabled;
            //if(reenable)
            //{
            //    Processes.Scheduler.Disable();
            //}

            //BasicConsole.WriteLine("Adding code page...");
            if (!CodePages.Contains(vAddr))
            {
                CodePages.Add(vAddr, pAddr);
            }

            //if (reenable)
            //{
            //    Processes.Scheduler.Enable();
            //}
        }
        [Drivers.Compiler.Attributes.NoDebug]
        public void AddDataPage(uint pAddr, uint vAddr)
        {
            //bool reenable = Processes.Scheduler.Enabled;
            //if (reenable)
            //{
            //    Processes.Scheduler.Disable();
            //}

            //BasicConsole.WriteLine("Adding data page...");
            if (!DataPages.Contains(vAddr))
            {
                DataPages.Add(vAddr, pAddr);
            }

            //if (reenable)
            //{
            //    Processes.Scheduler.Enable();
            //}
        }
        [Drivers.Compiler.Attributes.NoDebug]
        public void RemovePage(uint vAddr)
        {
            //bool reenable = Processes.Scheduler.Enabled;
            //if (reenable)
            //{
            //    Processes.Scheduler.Disable();
            //}

            //BasicConsole.WriteLine("Removing page...");
            CodePages.Remove(vAddr);
            DataPages.Remove(vAddr);

            //if (reenable)
            //{
            //    Processes.Scheduler.Enable();
            //}
        }

        //bool loadPrint = true;
        //bool unloadPrint = true;
        [Drivers.Compiler.Attributes.NoDebug]
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
                VirtMemManager.Map(pAddr, vAddr, 4096, flags, UpdateUsedPagesFlags.Virtual);
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

                VirtMemManager.Map(pAddr, vAddr, 4096, flags, UpdateUsedPagesFlags.Virtual);
            }

            //if (loadPrint)
            //{
            //    //BasicConsole.DelayOutput(1);
            //    loadPrint = false;
            //}
        }
        [Drivers.Compiler.Attributes.NoDebug]
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

                VirtMemManager.Unmap(CodePages.Keys[i], UpdateUsedPagesFlags.Virtual);
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

               VirtMemManager.Unmap(DataPages.Keys[i], UpdateUsedPagesFlags.Virtual);
            }

            //if (unloadPrint)
            //{
            //    //BasicConsole.DelayOutput(1);
            //    unloadPrint = false;
            //}
        }

        public MemoryLayout Merge(MemoryLayout y)
        {
            MemoryLayout result = new MemoryLayout();

            for (int i = 0; i < CodePages.Keys.Count; i++)
            {
                uint vAddr = CodePages.Keys[i];
                uint pAddr = CodePages[vAddr];
                result.AddCodePage(pAddr, vAddr);
            }
            for (int i = 0; i < DataPages.Keys.Count; i++)
            {
                uint vAddr = DataPages.Keys[i];
                uint pAddr = DataPages[vAddr];
                result.AddDataPage(pAddr, vAddr);
            }

            for (int i = 0; i < y.CodePages.Keys.Count; i++)
            {
                uint vAddr = y.CodePages.Keys[i];
                uint pAddr = y.CodePages[vAddr];
                result.AddCodePage(pAddr, vAddr);
            }
            for (int i = 0; i < y.DataPages.Keys.Count; i++)
            {
                uint vAddr = y.DataPages.Keys[i];
                uint pAddr = y.DataPages[vAddr];
                result.AddDataPage(pAddr, vAddr);
            }

            return result;
        }

        public FOS_System.String ToString()
        {
            FOS_System.String result = "";

            result = result + "Code pages:\r\n";
            for (int i = 0; i < CodePages.Keys.Count; i++)
            {
                uint vAddr = CodePages.Keys[i];
                uint pAddr = CodePages[vAddr];

                result = result + vAddr + " -> " + pAddr + "\r\n";
            }

            result = result + "\r\n";

            result = result + "Data pages:\r\n";
            for (int i = 0; i < DataPages.Keys.Count; i++)
            {
                uint vAddr = DataPages.Keys[i];
                uint pAddr = DataPages[vAddr];

                result = result + vAddr + " -> " + pAddr + "\r\n";
            }

            return result;
        }
    }
}
