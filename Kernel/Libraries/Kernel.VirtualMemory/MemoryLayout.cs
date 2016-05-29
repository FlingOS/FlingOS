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

//#define MEMLAYOUT_TRACE
//#define MEMLAYOUT_MERGE_TRACE

using Drivers.Compiler.Attributes;
using Kernel.Framework;
using Kernel.Framework.Collections;

namespace Kernel.VirtualMemory
{
    /// <remarks>
    ///     Bit 1 of physical addresses is used to indicate whether the mapping came from a merge or not.
    /// </remarks>
    public class MemoryLayout : Object
    {
        //TODO: When do physical pages that are no longer in use (in any memory layout) get unmapped from the Virtual Memory Manager?

        public bool AddAllDataToKernel = false;

        public UInt32Dictionary CodePages = new UInt32Dictionary();
        public UInt32Dictionary DataPages = new UInt32Dictionary();
        public UInt32Dictionary KernelPages = new UInt32Dictionary();

        [NoDebug]
        public void AddCodePage(uint pAddr, uint vAddr)
        {
            //BasicConsole.WriteLine("Adding code page...");
            if (!CodePages.ContainsKey(vAddr))
            {
                CodePages.Add(vAddr, pAddr);
            }
#if DEBUG
            else
            {
                BasicConsole.WriteLine(
                    "Cannot add code page to memory layout! Code virtual page already mapped in the memory layout.");
                //ExceptionMethods.PrintStackTrace();
                //ExceptionMethods.Throw(new Framework.Exception("Cannot add code page to memory layout! Code virtual page already mapped in the memory layout."));
            }
#endif
        }

        public void AddCodePages(uint vAddrStart, uint pAddrStart, uint count)
        {
            CodePages.AddRange(vAddrStart, 4096, pAddrStart, 0x1000, count);
        }

        public void AddCodePages(uint vAddrStart, uint[] pAddrs)
        {
            CodePages.AddRange(vAddrStart, 4096, pAddrs);
        }

        [NoDebug]
        public void AddDataPage(uint pAddr, uint vAddr)
        {
            if (AddAllDataToKernel)
            {
                AddKernelPage(pAddr, vAddr);
            }
            else
            {
                //BasicConsole.WriteLine("Adding data page...");
                if (!DataPages.ContainsKey(vAddr))
                {
#if MEMLAYOUT_TRACE
                    Framework.String str = "Adding data page: 0x         => 0x        ";
                    ExceptionMethods.FillString(vAddr, 27, str);
                    ExceptionMethods.FillString(pAddr, 41, str);
                    BasicConsole.WriteLine(str);
#endif
                    DataPages.Add(vAddr, pAddr);
                }
#if DEBUG
                else
                {
                    BasicConsole.WriteLine(
                        "Cannot add data page to memory layout! Data virtual page already mapped in the memory layout.");
                    //ExceptionMethods.PrintStackTrace();
                    //ExceptionMethods.Throw(new Framework.Exception("Cannot add data page to memory layout! Data virtual page already mapped in the memory layout."));
                }
#endif
            }
        }

        public void AddDataPages(uint vAddrStart, uint pAddrStart, uint count)
        {
            if (AddAllDataToKernel)
            {
                AddKernelPages(vAddrStart, pAddrStart, count);
            }
            else
            {
                DataPages.AddRange(vAddrStart, 4096, pAddrStart, 0x1000, count);
            }
        }

        public void AddDataPages(uint vAddrStart, uint[] pAddrs)
        {
            if (AddAllDataToKernel)
            {
                AddKernelPages(vAddrStart, pAddrs);
            }
            else
            {
                DataPages.AddRange(vAddrStart, 4096, pAddrs);
            }
        }

        public void AddKernelPage(uint pAddr, uint vAddr)
        {
            //BasicConsole.WriteLine("Adding kernel page...");
            if (!KernelPages.ContainsKey(vAddr))
            {
#if MEMLAYOUT_TRACE
                Framework.String str = "Adding kernel page: 0x         => 0x        ";
                ExceptionMethods.FillString(vAddr, 29, str);
                ExceptionMethods.FillString(pAddr, 43, str);
                BasicConsole.WriteLine(str);
#endif
                KernelPages.Add(vAddr, pAddr);
            }
#if DEBUG
            else
            {
                BasicConsole.WriteLine(
                    "Cannot add kernel page to memory layout! Kernel virtual page already mapped in the memory layout.");
                //ExceptionMethods.PrintStackTrace();
                //ExceptionMethods.Throw(new Framework.Exception("Cannot add kernel page to memory layout! Data virtual page already mapped in the memory layout."));
            }
#endif
        }

        public void AddKernelPages(uint vAddrStart, uint[] pAddrs)
        {
            KernelPages.AddRange(vAddrStart, 4096, pAddrs);
        }

        public void AddKernelPages(uint vAddrStart, uint pAddrStart, uint count)
        {
            KernelPages.AddRange(vAddrStart, 4096, pAddrStart, 0x1000, count);
        }

        [NoDebug]
        public void RemovePage(uint vAddr)
        {
            //BasicConsole.WriteLine("Removing page...");
            CodePages.Remove(vAddr);
            DataPages.Remove(vAddr);
            KernelPages.Remove(vAddr);
        }

        public void RemovePages(uint vAddrStart, uint numPages)
        {
            //BasicConsole.WriteLine("Removing pages...");
            CodePages.RemoveRange(vAddrStart, 4096, numPages);
            DataPages.RemoveRange(vAddrStart, 4096, numPages);
            KernelPages.RemoveRange(vAddrStart, 4096, numPages);
        }

        public void ReplaceKernelPage(uint vAddr, uint newPAddr)
        {
            KernelPages[vAddr] = newPAddr;
            VirtualMemoryManager.Map(newPAddr, vAddr, 0x1000, PageFlags.KernelOnly, UpdateUsedPagesFlags.None);
        }

        public void SwitchFrom(bool ProcessIsUM, MemoryLayout old)
        {
            int unloaded = 0;
            int loaded = 0;

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.1");
            //}

            if (old != null)
            {
                UInt32Dictionary.Iterator iterator = old.CodePages.GetIterator();
                while (iterator.HasNext())
                {
                    UInt32Dictionary.KeyValuePair pair = iterator.Next();
                    uint vAddr = pair.Key;

#if MEMLAYOUT_TRACE
                BasicConsole.WriteLine("Unloading code page...");
#endif
                    if (!CodePages.ContainsKey(vAddr))
                    {
                        unloaded++;
                        VirtualMemoryManager.Unmap(vAddr, UpdateUsedPagesFlags.Virtual);
                    }
                }
                iterator.RestoreState();

                //if (Processes.Scheduler.OutputMessages)
                //{
                //    BasicConsole.WriteLine("Debug Point 9.1.2");
                //}

                iterator = old.DataPages.GetIterator();
                while (iterator.HasNext())
                {
                    //if (Processes.Scheduler.OutputMessages)
                    //{
                    //    BasicConsole.WriteLine("Debug Point 9.1.2-1");
                    //}

                    UInt32Dictionary.KeyValuePair pair = iterator.Next();

                    //if (Processes.Scheduler.OutputMessages)
                    //{
                    //    BasicConsole.WriteLine("Debug Point 9.1.2-3");
                    //}

                    uint vAddr = pair.Key;

                    //if (Processes.Scheduler.OutputMessages)
                    //{
                    //    BasicConsole.WriteLine("Debug Point 9.1.2-4");
                    //}

#if MEMLAYOUT_TRACE
                BasicConsole.WriteLine("Unloading data page...");
#endif
                    if (!DataPages.ContainsKey(vAddr))
                    {
                        //if (Processes.Scheduler.OutputMessages)
                        //{
                        //    BasicConsole.WriteLine("Debug Point 9.1.2-5");
                        //}

                        unloaded++;
                        VirtualMemoryManager.Unmap(vAddr, UpdateUsedPagesFlags.Virtual);

                        //if (Processes.Scheduler.OutputMessages)
                        //{
                        //    BasicConsole.WriteLine("Debug Point 9.1.2-6");
                        //}
                    }

                    //if (Processes.Scheduler.OutputMessages)
                    //{
                    //    BasicConsole.WriteLine("Debug Point 9.1.2-7");
                    //}
                }

                //if (Processes.Scheduler.OutputMessages)
                //{
                //    BasicConsole.WriteLine("Debug Point 9.1.2-8");
                //}

                iterator.RestoreState();

                //if (Processes.Scheduler.OutputMessages)
                //{
                //    BasicConsole.WriteLine("Debug Point 9.1.2-9");
                //}
            }

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.3");
            //}

            {
                PageFlags flags = ProcessIsUM
                    ? PageFlags.None
                    : PageFlags.KernelOnly;

                UInt32Dictionary.Iterator iterator = CodePages.GetIterator();
                while (iterator.HasNext())
                {
                    UInt32Dictionary.KeyValuePair pair = iterator.Next();
                    uint vAddr = pair.Key;
                    uint pAddr = pair.Value & 0xFFFFF000;

#if MEMLAYOUT_TRACE
                BasicConsole.WriteLine("Loading code page...");
#endif
                    loaded++;
                    VirtualMemoryManager.Map(pAddr, vAddr, 4096, flags, UpdateUsedPagesFlags.Virtual);
                }
                iterator.RestoreState();

                //if (Processes.Scheduler.OutputMessages)
                //{
                //    BasicConsole.WriteLine("Debug Point 9.1.4");
                //}

                flags = ProcessIsUM
                    ? PageFlags.None
                    : PageFlags.KernelOnly;
                iterator = DataPages.GetIterator();
                while (iterator.HasNext())
                {
                    //if (Processes.Scheduler.OutputMessages)
                    //{
                    //    BasicConsole.WriteLine("Debug Point 9.1.4.1");
                    //}

                    UInt32Dictionary.KeyValuePair pair = iterator.Next();
                    //if (Processes.Scheduler.OutputMessages)
                    //{
                    //    BasicConsole.WriteLine("Debug Point 9.1.4.2");
                    //}

                    uint vAddr = pair.Key;
                    //if (Processes.Scheduler.OutputMessages)
                    //{
                    //    BasicConsole.WriteLine("Debug Point 9.1.4.3");
                    //}

                    uint pAddr = pair.Value & 0xFFFFF000;
                    //if (Processes.Scheduler.OutputMessages)
                    //{
                    //    BasicConsole.WriteLine("Debug Point 9.1.4.4");
                    //}


#if MEMLAYOUT_TRACE
                BasicConsole.WriteLine("Loading data page...");
#endif

                    loaded++;
                    //if (Processes.Scheduler.OutputMessages)
                    //{
                    //    BasicConsole.WriteLine("Debug Point 9.1.4.5");
                    //}

                    VirtualMemoryManager.Map(pAddr, vAddr, 4096, flags, UpdateUsedPagesFlags.Virtual);

                    //if (Processes.Scheduler.OutputMessages)
                    //{
                    //    BasicConsole.WriteLine("Debug Point 9.1.4.6");
                    //}
                }
                iterator.RestoreState();

                flags = PageFlags.KernelOnly;
                iterator = KernelPages.GetIterator();
                while (iterator.HasNext())
                {
                    UInt32Dictionary.KeyValuePair pair = iterator.Next();
                    uint vAddr = pair.Key;
                    uint pAddr = pair.Value & 0xFFFFF000;
#if MEMLAYOUT_TRACE
                BasicConsole.WriteLine("Loading kernel page...");
#endif

                    loaded++;
                    VirtualMemoryManager.Map(pAddr, vAddr, 4096, flags, UpdateUsedPagesFlags.Virtual);
                }
                iterator.RestoreState();
            }

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.5");
            //}

            //Framework.String unloadedMsg = "SF: Unloaded: 0x        ";
            //Framework.String loadedMsg = "SF: Loaded: 0x        ";
            //ExceptionMethods.FillString((uint)unloaded, 23, unloadedMsg);
            //ExceptionMethods.FillString((uint)loaded, 21, loadedMsg);
            //BasicConsole.WriteLine(unloadedMsg);
            //BasicConsole.WriteLine(loadedMsg);

            //if (Processes.Scheduler.OutputMessages)
            //{
            //    BasicConsole.WriteLine("Debug Point 9.1.6");
            //}
        }

        public void Merge(MemoryLayout y, bool ProcessIsUM)
        {
            int loaded = 0;

            PageFlags flags = ProcessIsUM
                ? PageFlags.None
                : PageFlags.KernelOnly;

            UInt32Dictionary.Iterator iterator = y.CodePages.GetIterator();
            while (iterator.HasNext())
            {
#if MEMLAYOUT_MERGE_TRACE
                BasicConsole.WriteLine("~M-C~");
#endif

                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                uint vAddr = pair.Key;
                uint pAddr = pair.Value & 0xFFFFF000;

                if (!CodePages.ContainsKey(vAddr))
                {
                    // 0x1 indicates mapping was added by a merge
                    CodePages.Add(vAddr, pAddr | 0x1);

                    loaded++;
                    VirtualMemoryManager.Map(pAddr, vAddr, 4096, flags, UpdateUsedPagesFlags.Virtual);
                }
#if MEMLAYOUT_MERGE_TRACE
                else
                {
                    if ((CodePages[vAddr] & 0xFFFFF000) != pAddr)
                    {
                        BasicConsole.WriteLine("Error merging layouts! Code virtual address would be mapped to two different physical addresses.");
                        BasicConsole.WriteLine(vAddr);
                        BasicConsole.WriteLine(DataPages[vAddr]);
                        BasicConsole.WriteLine(pAddr);
                    }
                }
#endif
            }

            flags = ProcessIsUM
                ? PageFlags.None
                : PageFlags.KernelOnly;
            iterator = y.DataPages.GetIterator();
            while (iterator.HasNext())
            {
#if MEMLAYOUT_MERGE_TRACE
                BasicConsole.WriteLine("~M-D~");
#endif

                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                uint vAddr = pair.Key;
                uint pAddr = pair.Value & 0xFFFFF000;

                if (AddAllDataToKernel)
                {
                    if (!KernelPages.ContainsKey(vAddr))
                    {
                        // 0x1 indicates mapping was added by a merge
                        KernelPages.Add(vAddr, pAddr | 0x1);

                        loaded++;
                        VirtualMemoryManager.Map(pAddr, vAddr, 4096, flags, UpdateUsedPagesFlags.Virtual);
                    }
#if MEMLAYOUT_MERGE_TRACE
                    else
                    {
                        if ((KernelPages[vAddr] & 0xFFFFF000) != pAddr)
                        {
                            BasicConsole.WriteLine("Error merging layouts! Data virtual address would be mapped to two different physical addresses. (Kernel)");
                            BasicConsole.WriteLine(vAddr);
                            BasicConsole.WriteLine(KernelPages[vAddr]);
                            BasicConsole.WriteLine(pAddr);
                        }
                    }
#endif
                }
                else
                {
                    if (!DataPages.ContainsKey(vAddr))
                    {
                        // 0x1 indicates mapping was added by a merge
                        DataPages.Add(vAddr, pAddr | 0x1);

                        loaded++;
                        VirtualMemoryManager.Map(pAddr, vAddr, 4096, flags, UpdateUsedPagesFlags.Virtual);
                    }
#if MEMLAYOUT_MERGE_TRACE
                    else
                    {
                        if ((DataPages[vAddr] & 0xFFFFF000) != pAddr)
                        {
                            BasicConsole.WriteLine("Error merging layouts! Data virtual address would be mapped to two different physical addresses.");
                            BasicConsole.WriteLine(vAddr);
                            BasicConsole.WriteLine(DataPages[vAddr]);
                            BasicConsole.WriteLine(pAddr);
                        }
                    }
#endif
                }
            }

            flags = PageFlags.KernelOnly;
            iterator = y.KernelPages.GetIterator();
            while (iterator.HasNext())
            {
#if MEMLAYOUT_MERGE_TRACE
                BasicConsole.WriteLine("~M-K~");
#endif

                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                uint vAddr = pair.Key;
                uint pAddr = pair.Value & 0xFFFFF000;

                if (!KernelPages.ContainsKey(vAddr))
                {
                    // 0x1 indicates mapping was added by a merge
                    KernelPages.Add(vAddr, pAddr | 0x1);

                    loaded++;
                    VirtualMemoryManager.Map(pAddr, vAddr, 4096, flags, UpdateUsedPagesFlags.Virtual);
                }
#if MEMLAYOUT_MERGE_TRACE
                else
                {
                    if ((KernelPages[vAddr] & 0xFFFFF000) != pAddr)
                    {
                        BasicConsole.WriteLine("Error merging layouts! Kernel virtual address would be mapped to two different physical addresses.");
                        BasicConsole.WriteLine(vAddr);
                        BasicConsole.WriteLine(KernelPages[vAddr]);
                        BasicConsole.WriteLine(pAddr);
                    }
                }
#endif
            }

            //Framework.String loadedMsg = "ME: Loaded: 0x        ";
            //ExceptionMethods.FillString((uint)loaded, 21, loadedMsg);
            //BasicConsole.WriteLine(loadedMsg);
        }

        public void Unmerge(MemoryLayout y)
        {
            int unloaded = 0;

            UInt32Dictionary.Iterator iterator = y.CodePages.GetIterator();
            while (iterator.HasNext())
            {
                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                uint vAddr = pair.Key;

                if (CodePages.ContainsKey(vAddr))
                {
                    // If the mapping was added by a merge
                    if ((CodePages[vAddr] & 0x1) == 1)
                    {
                        CodePages.Remove(vAddr);
                        unloaded++;
                        VirtualMemoryManager.Unmap(vAddr, UpdateUsedPagesFlags.Virtual);
                    }
                }
            }
            iterator = y.DataPages.GetIterator();
            while (iterator.HasNext())
            {
                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                uint vAddr = pair.Key;

                if (AddAllDataToKernel)
                {
                    if (KernelPages.ContainsKey(vAddr))
                    {
                        // If the mapping was added by a merge
                        if ((KernelPages[vAddr] & 0x1) == 1)
                        {
                            KernelPages.Remove(vAddr);
                            unloaded++;
                        }
                    }
                }
                else
                {
                    if (DataPages.ContainsKey(vAddr))
                    {
                        // If the mapping was added by a merge
                        if ((DataPages[vAddr] & 0x1) == 1)
                        {
                            DataPages.Remove(vAddr);
                            unloaded++;
                            VirtualMemoryManager.Unmap(vAddr, UpdateUsedPagesFlags.Virtual);
                        }
                    }
                }
            }
            iterator = y.KernelPages.GetIterator();
            while (iterator.HasNext())
            {
                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                uint vAddr = pair.Key;

                if (KernelPages.ContainsKey(vAddr))
                {
                    // If the mapping was added by a merge
                    if ((KernelPages[vAddr] & 0x1) == 1)
                    {
                        KernelPages.Remove(vAddr);
                        unloaded++;
                    }
                }
            }

            //Framework.String unloadedMsg = "UM: Unloaded: 0x        ";
            //ExceptionMethods.FillString((uint)unloaded, 23, unloadedMsg);
            //BasicConsole.WriteLine(unloadedMsg);
        }

        public bool ContainsAnyVirtualAddresses(uint startAddr, int count)
        {
            return CodePages.ContainsAnyKeyInRange(startAddr, startAddr + (uint)count) ||
                   DataPages.ContainsAnyKeyInRange(startAddr, startAddr + (uint)count) ||
                   KernelPages.ContainsAnyKeyInRange(startAddr, startAddr + (uint)count);
        }

        public bool ContainsAllVirtualAddresses(uint startAddr, uint count, uint step)
        {
            bool OK = true;

            uint endAddr = startAddr + count*step;
            for (; startAddr < endAddr; startAddr += step)
            {
                if (!CodePages.ContainsKey(startAddr) &&
                    !DataPages.ContainsKey(startAddr) &&
                    !KernelPages.ContainsKey(startAddr))
                {
                    OK = false;
                    break;
                }
            }

            return OK;
        }

        public bool ContainsAnyPhysicalAddresses(uint startAddr, int count)
        {
            return CodePages.ContainsAnyValueInRange(startAddr, startAddr + (uint)count) ||
                   DataPages.ContainsAnyValueInRange(startAddr, startAddr + (uint)count) ||
                   KernelPages.ContainsAnyValueInRange(startAddr, startAddr + (uint)count);
        }

        public bool ContainsAllPhysicalAddresses(uint startAddr, uint count, uint step)
        {
            bool OK = true;

            uint endAddr = startAddr + count*step;
            for (; startAddr < endAddr; startAddr += step)
            {
                if (!CodePages.ContainsValue(startAddr) &&
                    !DataPages.ContainsValue(startAddr) &&
                    !KernelPages.ContainsValue(startAddr) &&
                    !CodePages.ContainsValue(startAddr | 0x1) &&
                    !DataPages.ContainsValue(startAddr | 0x1) &&
                    !KernelPages.ContainsValue(startAddr | 0x1))
                {
                    OK = false;
                    break;
                }
            }

            return OK;
        }

        public uint GetPhysicalAddress(uint virtAddr)
        {
            if (CodePages.ContainsKey(virtAddr))
            {
                return CodePages[virtAddr] & 0xFFFFF000;
            }
            if (DataPages.ContainsKey(virtAddr))
            {
                return DataPages[virtAddr] & 0xFFFFF000;
            }
            if (KernelPages.ContainsKey(virtAddr))
            {
                return KernelPages[virtAddr] & 0xFFFFF000;
            }
            return 0xFFFFFFFF;
        }

        public uint[] GetPhysicalAddresses(uint startAddr, uint count)
        {
            uint[] result = new uint[count];

            for (int i = 0; i < count; startAddr += 4096, i++)
            {
                result[i] = GetPhysicalAddress(startAddr) & 0xFFFFF000;
            }

            return result;
        }

        public uint GetVirtualAddress(uint physAddr)
        {
            if (CodePages.ContainsValue(physAddr))
            {
                return CodePages.GetFirstKeyOfValue(physAddr);
            }
            if (DataPages.ContainsValue(physAddr))
            {
                return DataPages.GetFirstKeyOfValue(physAddr);
            }
            if (KernelPages.ContainsValue(physAddr))
            {
                return KernelPages.GetFirstKeyOfValue(physAddr);
            }
            if (CodePages.ContainsValue(physAddr | 0x1))
            {
                return CodePages.GetFirstKeyOfValue(physAddr | 0x1);
            }
            if (DataPages.ContainsValue(physAddr | 0x1))
            {
                return DataPages.GetFirstKeyOfValue(physAddr | 0x1);
            }
            if (KernelPages.ContainsValue(physAddr | 0x1))
            {
                return KernelPages.GetFirstKeyOfValue(physAddr | 0x1);
            }
            return 0xFFFFFFFF;
        }

        public String ToString()
        {
            String result = "";

            result = result + "Code pages:\r\n";
            UInt32Dictionary.Iterator iterator = CodePages.GetIterator();
            while (iterator.HasNext())
            {
                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                uint vAddr = pair.Key;
                uint pAddr = pair.Value;

                result = result + vAddr + " -> " + pAddr + "\r\n";
            }

            result = result + "\r\n";

            result = result + "Data pages:\r\n";
            iterator = DataPages.GetIterator();
            while (iterator.HasNext())
            {
                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                uint vAddr = pair.Key;
                uint pAddr = pair.Value;

                result = result + vAddr + " -> " + pAddr + "\r\n";
            }

            result = result + "\r\n";

            result = result + "Kernel pages:\r\n";
            iterator = KernelPages.GetIterator();
            while (iterator.HasNext())
            {
                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                uint vAddr = pair.Key;
                uint pAddr = pair.Value;

                result = result + vAddr + " -> " + pAddr + "\r\n";
            }

            return result;
        }
    }
}