using System;
using Kernel.FOS_System.IO;
using Kernel.Hardware.Processes;

namespace Kernel.Core.Processes
{
    public static unsafe class DynamicLinkerLoader
    {
        public static Process LoadProcess_FromRawExe(File RawExeFile, bool UserMode)
        {
            // - Read in file contents
            // - Map enough memory for the exe file contents
            // - Copy the memory over

            //TODO - Handle case of EXE being bigger than 4KiB?
            //          - Would need to map contiguous (virtual) pages.
            
            //BasicConsole.WriteLine("Mapping free page for process...");
            byte* destMemPtr = (byte*)Hardware.VirtMemManager.MapFreePage(
                UserMode ? Hardware.VirtMem.VirtMemImpl.PageFlags.None : Hardware.VirtMem.VirtMemImpl.PageFlags.KernelOnly);
            //BasicConsole.WriteLine(((FOS_System.String)"destMemPtr=") + (uint)destMemPtr);

            //BasicConsole.WriteLine("Reading file...");
            int bytesRead = 0;
            byte[] readBuffer = new byte[4096];
            bytesRead = RawExeFile.GetStream().Read(readBuffer, 0, 4096);
            //BasicConsole.WriteLine("Copying data...");
            Utilities.MemoryUtils.MemCpy_32(destMemPtr, ((byte*)Utilities.ObjectUtilities.GetHandle(readBuffer)) + FOS_System.Array.FieldsBytesSize, (uint)bytesRead);

            // - Create the process
            //BasicConsole.WriteLine("Creating process...");
            Process process = ProcessManager.CreateProcess(
                (ThreadStartMethod)Utilities.ObjectUtilities.GetObject(destMemPtr), RawExeFile.Name, UserMode);
            //BasicConsole.WriteLine("Adding process' code page...");
            process.TheMemoryLayout.AddCodePage((uint)Hardware.VirtMemManager.GetPhysicalAddress(destMemPtr),
                (uint)destMemPtr);
            return process;
        }
    }
}
