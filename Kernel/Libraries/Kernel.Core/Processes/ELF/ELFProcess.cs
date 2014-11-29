using System;
using Kernel.FOS_System;
using Kernel.FOS_System.IO;
using Kernel.FOS_System.IO.Streams;
using Kernel.FOS_System.Collections;
using Kernel.Hardware.Processes;

namespace Kernel.Core.Processes.ELF
{
    public unsafe class ELFProcess : FOS_System.Object
    {
        protected Process theProcess;
        public Process TheProcess
        {
            get
            {
                return theProcess;
            }
        }

        protected ELFFile theFile;
        public ELFFile TheFile
        {
            get
            {
                return theFile;
            }
        }

        public ELFProcess(ELFFile anELFFile)
        {
            theFile = anELFFile;
        }

        public void Load(bool UserMode)
        {
            bool OK = true;
            bool reenable = Scheduler.Enabled;
            if (reenable)
            {
                Scheduler.Disable();
            }

            try
            {
                ThreadStartMethod mainMethod = (ThreadStartMethod)Utilities.ObjectUtilities.GetObject(theFile.Header.EntryPoint);
                theProcess = ProcessManager.CreateProcess(
                    mainMethod, theFile.TheFile.Name, UserMode);

                List Segments = TheFile.Segments;
                for (int i = 0; i < Segments.Count; i++)
                {
                    ELFSegment segment = (ELFSegment)Segments[i];

                    if (segment.Header.Type == ELFSegmentType.Interp ||
                        segment.Header.Type == ELFSegmentType.Dynamic)
                    {
                        OK = false;
                        ExceptionMethods.Throw(new FOS_System.Exception("Error loading ELF process! Dynamic linking required."));
                    }
                    else if (segment.Header.Type == ELFSegmentType.Load)
                    {
                        int bytesRead = segment.Read(theFile.TheStream);
                        if (bytesRead != segment.Header.FileSize)
                        {
                            OK = false;
                            ExceptionMethods.Throw(new FOS_System.Exception("Error loading ELF process! Failed to load correct segment bytes from file."));
                        }

                        uint physPageAddr = Hardware.VirtMemManager.FindFreePhysPage();
                        byte* destMemPtr = segment.Header.VAddr;
                        byte* pageAlignedDestMemPtr = (byte*)((uint)destMemPtr & 0xFFFFF000);

                        //TODO - Handle segments bigger than 4KiB (1 page)
                        if (segment.Header.MemSize > 4096)
                        {
                            ExceptionMethods.Throw(new FOS_System.Exception("Error loading ELF process! Cannot handle segment size in memory being greater than 1 page (4096 bytes)."));
                        }

                        Hardware.VirtMemManager.Map(
                            physPageAddr, (uint)pageAlignedDestMemPtr, 4096,
                            UserMode ? Hardware.VirtMem.VirtMemImpl.PageFlags.None : Hardware.VirtMem.VirtMemImpl.PageFlags.KernelOnly);

                        Utilities.MemoryUtils.MemCpy_32(
                            destMemPtr,
                            ((byte*)Utilities.ObjectUtilities.GetHandle(segment.Data)) + FOS_System.Array.FieldsBytesSize,
                            (uint)bytesRead);

                        if ((segment.Header.Flags & ELFFlags.Executable) != 0)
                        {
                            theProcess.TheMemoryLayout.AddCodePage(physPageAddr, (uint)pageAlignedDestMemPtr);
                        }
                        else
                        {
                            theProcess.TheMemoryLayout.AddDataPage(physPageAddr, (uint)pageAlignedDestMemPtr);
                        }

                        if (segment.Header.MemSize > segment.Header.FileSize)
                        {
                            OK = false;
                            ExceptionMethods.Throw(new FOS_System.Exception("Error loading ELF process! Cannot handle segment size in memory being greater than segment size in file."));
                        }
                    }
                }
            }
            finally
            {
                if (!OK)
                {
                    theProcess = null;
                }

                if (reenable)
                {
                    Scheduler.Enable();
                }
            }
        }
    }
}
