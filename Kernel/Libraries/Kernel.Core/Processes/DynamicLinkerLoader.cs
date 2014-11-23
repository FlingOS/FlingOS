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
            //          - Would need to map contiguous pages.
            byte* destMemPtr = (byte*)Hardware.VirtMemManager.MapFreePage(
                UserMode ? Hardware.VirtMem.VirtMemImpl.PageFlags.None : Hardware.VirtMem.VirtMemImpl.PageFlags.KernelOnly);

            int bytesRead = 0;
            byte[] readBuffer = new byte[4096];
            bytesRead = RawExeFile.GetStream().Read(readBuffer, 0, 4096);
            Utilities.MemoryUtils.MemCpy_32(destMemPtr, ((byte*)Utilities.ObjectUtilities.GetHandle(readBuffer)) + FOS_System.Array.FieldsBytesSize, (uint)bytesRead);

            // - Create the process
            return ProcessManager.CreateProcess((ThreadStartMethod)Utilities.ObjectUtilities.GetObject(destMemPtr), RawExeFile.Name, UserMode);
        }
    }
}
