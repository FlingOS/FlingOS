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
    
using System;
using Kernel.FOS_System.IO;
using Kernel.FOS_System.Processes;
using Kernel.Processes.ELF;

namespace Kernel.Processes
{
    public static unsafe class DynamicLinkerLoader
    {
        public static Hardware.Processes.Process LoadProcess_FromRawExe(File RawExeFile, bool UserMode)
        {
            // - Read in file contents
            // - Map enough memory for the exe file contents
            // - Copy the memory over

            //bool reenable = Scheduler.Enabled;
            //if (reenable)
            //{
            //    Scheduler.Disable();
            //}

            //TODO: Handle case of EXE being bigger than 4KiB
            //          - Need to map contiguous (virtual) pages.
            
            BasicConsole.WriteLine("Mapping free page for process...");
            void* unusedPAddr;
            byte* destMemPtr = (byte*)Hardware.VirtMemManager.MapFreePage(
                UserMode ? Hardware.VirtMem.VirtMemImpl.PageFlags.None : Hardware.VirtMem.VirtMemImpl.PageFlags.KernelOnly, out unusedPAddr);
            BasicConsole.WriteLine(((FOS_System.String)"Physical address = ") + (uint)Hardware.VirtMemManager.GetPhysicalAddress(destMemPtr));
            BasicConsole.WriteLine(((FOS_System.String)"Virtual address = ") + (uint)destMemPtr);

            // Add the page to the current processes memory layout
            //  So we can access the memory while we load the process. 
            //  Otherwise we will hit a page fault as soon as we try copying the memory
            //      further down.
            //  Note: The page fault will only be hit on starting a second process because
            //      the scheduler doesn't change the context when only one process is running.
            Hardware.Processes.ProcessManager.CurrentProcess.TheMemoryLayout.AddDataPage(
                (uint)Hardware.VirtMemManager.GetPhysicalAddress(destMemPtr),
                (uint)destMemPtr);
            // We could have been "scheduler interrupted" just after the map but just before the 
            //  add data page...
            //TODO: Hardware.Processes.ProcessManager.CurrentProcess.LoadMemLayout();

            //BasicConsole.WriteLine("Reading file...");
            int bytesRead = 0;
            byte[] readBuffer = new byte[4096];
            bytesRead = RawExeFile.GetStream().Read(readBuffer, 0, 4096);
            //BasicConsole.WriteLine("Copying data...");
            Utilities.MemoryUtils.MemCpy_32(destMemPtr, ((byte*)Utilities.ObjectUtilities.GetHandle(readBuffer)) + FOS_System.Array.FieldsBytesSize, (uint)bytesRead);

            //BasicConsole.WriteLine("Converting destMemPtr...");
            ThreadStartPoint mainMethod = (ThreadStartPoint)Utilities.ObjectUtilities.GetObject(destMemPtr);

            //BasicConsole.WriteLine("Getting process name...");
            FOS_System.String name = RawExeFile.Name;

            // - Create the process
            //BasicConsole.WriteLine("Creating process...");
            Hardware.Processes.Process process = Hardware.Processes.ProcessManager.CreateProcess(
                mainMethod, name, UserMode);
            //BasicConsole.WriteLine("Adding process' code page...");
            // Add code page to new processes memory layout
            process.TheMemoryLayout.AddCodePage((uint)Hardware.VirtMemManager.GetPhysicalAddress(destMemPtr),
                (uint)destMemPtr);

            //BasicConsole.WriteLine("Removing process' code page from current process...");
            //Remove from current processes memory layout
            Hardware.Processes.ProcessManager.CurrentProcess.TheMemoryLayout.RemovePage((uint)destMemPtr);

            //if (reenable)
            //{
            //    Scheduler.Enable();
            //}

            return process;
        }
        public static ELFProcess LoadProcess_FromELFExe(File ELFExeFile, bool UserMode)
        {
            //bool reenable = Scheduler.Enabled;
            //if (reenable)
            //{
            //    Scheduler.Disable();
            //}
            ELFProcess result = new ELFFile(ELFExeFile).LoadExecutable(UserMode);
            //if (reenable)
            //{
            //    Scheduler.Enable();
            //}
            return result;
        }
        public static ELFSharedObject LoadLibrary_FromELFSO(File ELFSharedObjectFile, ELFProcess theProcess)
        {
            //bool reenable = Scheduler.Enabled;
            //if (reenable)
            //{
            //    Scheduler.Disable();
            //}
            ELFSharedObject result = new ELFFile(ELFSharedObjectFile).LoadSharedObject(theProcess);
            //if (reenable)
            //{
            //    Scheduler.Enable();
            //}
            return result;
        }
    }
}
