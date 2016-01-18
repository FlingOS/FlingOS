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

using Kernel.FOS_System.Collections;

namespace Kernel.Hardware.VirtMem
{
    public class MemoryLayout : FOS_System.Object
    {
        public bool NoUnload = false;
        
        public UInt32Dictionary CodePages = new UInt32Dictionary();
        public UInt32Dictionary DataPages = new UInt32Dictionary();
        
        [Drivers.Compiler.Attributes.NoDebug]
        public void AddCodePage(uint pAddr, uint vAddr)
        {
            //BasicConsole.WriteLine("Adding code page...");
            if (!CodePages.Contains(vAddr))
            {
                CodePages.Add(vAddr, pAddr);
            }
#if DEBUG
            else
            {
                BasicConsole.WriteLine("Cannot add code page to memory layout! Code virtual page already mapped in the memory layout.");
                //ExceptionMethods.PrintStackTrace();
                //ExceptionMethods.Throw(new FOS_System.Exception("Cannot add code page to memory layout! Code virtual page already mapped in the memory layout."));
            }
#endif
        }
        [Drivers.Compiler.Attributes.NoDebug]
        public void AddDataPage(uint pAddr, uint vAddr)
        {
            //BasicConsole.WriteLine("Adding data page...");
            if (!DataPages.Contains(vAddr))
            {
                DataPages.Add(vAddr, pAddr);
            }
#if DEBUG
            else
            {
                BasicConsole.WriteLine("Cannot add data page to memory layout! Data virtual page already mapped in the memory layout.");
                //ExceptionMethods.PrintStackTrace();
                //ExceptionMethods.Throw(new FOS_System.Exception("Cannot add data page to memory layout! Data virtual page already mapped in the memory layout."));
            }
#endif
        }
        public void AddDataPages(uint vAddrStart, uint[] pAddrs)
        {
            DataPages.AddRange(vAddrStart, 4096, pAddrs);
        }
        [Drivers.Compiler.Attributes.NoDebug]
        public void RemovePage(uint vAddr)
        {
            //BasicConsole.WriteLine("Removing page...");
            CodePages.Remove(vAddr);
            DataPages.Remove(vAddr);
        }
        public void RemovePages(uint vAddrStart, uint numPages)
        {
            //BasicConsole.WriteLine("Removing pages...");
            CodePages.RemoveRange(vAddrStart, 4096, numPages);
            DataPages.RemoveRange(vAddrStart, 4096, numPages);
        }
        
        [Drivers.Compiler.Attributes.NoDebug]
        public void Load(bool ProcessIsUM)
        {
            VirtMemImpl.PageFlags flags = ProcessIsUM ? VirtMemImpl.PageFlags.None : VirtMemImpl.PageFlags.KernelOnly;

            UInt32Dictionary.Iterator iterator = CodePages.GetIterator();
            while (iterator.HasNext())
            {
                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                uint vAddr = pair.Key;
                uint pAddr = pair.Value;

#if MEMLAYOUT_TRACE
                BasicConsole.WriteLine("Loading code page...");
#endif
                VirtMemManager.Map(pAddr, vAddr, 4096, flags, UpdateUsedPagesFlags.Virtual);
            }
            iterator.RestoreState();

            flags = ProcessIsUM ? VirtMemImpl.PageFlags.None : VirtMemImpl.PageFlags.KernelOnly;
            iterator = DataPages.GetIterator();
            while (iterator.HasNext())
            {
                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                uint vAddr = pair.Key;
                uint pAddr = pair.Value;

#if MEMLAYOUT_TRACE
                BasicConsole.WriteLine("Loading data page...");
#endif
                VirtMemManager.Map(pAddr, vAddr, 4096, flags, UpdateUsedPagesFlags.Virtual);
            }
            iterator.RestoreState();
        }
        [Drivers.Compiler.Attributes.NoDebug]
        public void Unload()
        {
            if (NoUnload)
            {
                return;
            }

            UInt32Dictionary.Iterator iterator = CodePages.GetIterator();
            while (iterator.HasNext())
            {
                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                uint vAddr = pair.Key;
                
#if MEMLAYOUT_TRACE
                BasicConsole.WriteLine("Unloading code page...");
#endif
                VirtMemManager.Unmap(vAddr, UpdateUsedPagesFlags.Virtual);
            }
            iterator.RestoreState();

            iterator = DataPages.GetIterator();
            while (iterator.HasNext())
            {
                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                uint vAddr = pair.Key;

#if MEMLAYOUT_TRACE
                BasicConsole.WriteLine("Unloading data page...");
#endif
                VirtMemManager.Unmap(vAddr, UpdateUsedPagesFlags.Virtual);
            }
            iterator.RestoreState();
        }

        public void Merge(MemoryLayout y)
        {
            UInt32Dictionary.Iterator iterator = y.CodePages.GetIterator();
            while (iterator.HasNext())
            {
                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                uint vAddr = pair.Key;
                uint pAddr = pair.Value;

                AddCodePage(pAddr, vAddr);
            }
            iterator = y.DataPages.GetIterator();
            while (iterator.HasNext())
            {
                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                uint vAddr = pair.Key;
                uint pAddr = pair.Value;

                AddDataPage(pAddr, vAddr);
            }
        }
        public void Unmerge(MemoryLayout y)
        {
            UInt32Dictionary.Iterator iterator = y.CodePages.GetIterator();
            while (iterator.HasNext())
            {
                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                uint vAddr = pair.Key;
                uint pAddr = pair.Value;

                RemovePage(vAddr);
            }
            iterator = y.DataPages.GetIterator();
            while (iterator.HasNext())
            {
                UInt32Dictionary.KeyValuePair pair = iterator.Next();
                uint vAddr = pair.Key;
                uint pAddr = pair.Value;

                RemovePage(vAddr);
            }
        }

        public FOS_System.String ToString()
        {
            FOS_System.String result = "";

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

            return result;
        }
    }
}
