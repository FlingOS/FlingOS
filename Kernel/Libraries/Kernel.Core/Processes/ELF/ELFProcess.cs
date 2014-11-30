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
        protected Process theProcess = null;
        public Process TheProcess
        {
            get
            {
                return theProcess;
            }
        }

        protected ELFFile theFile = null;
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
                bool DynamicLinkingRequired = false;

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
                        DynamicLinkingRequired = true;
                    }
                    else if (segment.Header.Type == ELFSegmentType.Load)
                    {
                        int bytesRead = segment.Read(theFile.TheStream);
                        if (bytesRead != segment.Header.FileSize)
                        {
                            OK = false;
                            ExceptionMethods.Throw(new FOS_System.Exception("Error loading ELF process! Failed to load correct segment bytes from file."));
                        }

                        byte* destMemPtr = segment.Header.VAddr;
                        byte* pageAlignedDestMemPtr = (byte*)((uint)destMemPtr & 0xFFFFF000);

                        uint copyOffset = (uint)(destMemPtr - pageAlignedDestMemPtr);
                        uint copyFromOffset = 0;

                        bool executable = (segment.Header.Flags & ELFFlags.Executable) != 0;

                        for (uint pageOffset = 0; pageOffset < segment.Header.MemSize; pageOffset += 4096)
                        {
                            uint physPageAddr = Hardware.VirtMemManager.FindFreePhysPage();
                            uint virtPageAddr = (uint)pageAlignedDestMemPtr + pageOffset;

                            Hardware.VirtMemManager.Map(
                                physPageAddr,
                                virtPageAddr,
                                4096,
                                UserMode ? Hardware.VirtMem.VirtMemImpl.PageFlags.None : Hardware.VirtMem.VirtMemImpl.PageFlags.KernelOnly);

                            if (executable)
                            {
                                theProcess.TheMemoryLayout.AddCodePage(physPageAddr, virtPageAddr);
                            }
                            else
                            {
                                theProcess.TheMemoryLayout.AddDataPage(physPageAddr, virtPageAddr);
                            }

                            uint copySize = FOS_System.Math.Min((uint)bytesRead, 4096 - copyOffset);
                            if (copySize > 0)
                            {
                                Utilities.MemoryUtils.MemCpy_32(
                                    (byte*)(virtPageAddr + copyOffset),
                                    ((byte*)Utilities.ObjectUtilities.GetHandle(segment.Data)) + FOS_System.Array.FieldsBytesSize + pageOffset - copyFromOffset,
                                    copySize);

                                bytesRead -= (int)copySize;
                            }

                            for (uint j = copySize + copyOffset; j < 4096; j++)
                            {
                                *(byte*)(virtPageAddr + j) = 0;
                            }

                            if (copyOffset > 0)
                            {
                                copyFromOffset += copyOffset;
                                copyOffset = 0;
                            }
                        }
                    }
                }

                Console.Default.WriteLine();

                //Relocation happens here if this were a library / shared object

                if (DynamicLinkingRequired)
                {
                    Console.Default.WriteLine("Dynamic Linking");

                    ELFDynamicSection dynamicSection = theFile.DynamicSection;
                    ELFDynamicSymbolTableSection dynamicSymbolsSection = theFile.DynamicSymbolsSection;

                    ELFStringTable DynamicsStringTable = new ELFStringTable(
                        dynamicSection.StrTabDynamic.Val_Ptr, dynamicSection.StrTabSizeDynamic.Val_Ptr);
                    
                    for (uint i = 0; i < dynamicSection.Dynamics.Count; i++)
                    {
                        ELFDynamicSection.Dynamic theDyn = dynamicSection[i];

                        Console.Default.WriteLine("     - Dynamic : ");
                        Console.Default.Write("         - Tag : ");
                        Console.Default.WriteLine_AsDecimal((int)theDyn.Tag);
                        Console.Default.Write("         - Value or Pointer : ");
                        Console.Default.WriteLine_AsDecimal(theDyn.Val_Ptr);

                        if (theDyn.Tag == ELFDynamicSection.DynamicTag.Needed)
                        {
                            Console.Default.Write("         - Needed library name : ");
                            Console.Default.WriteLine(DynamicsStringTable[theDyn.Val_Ptr]);
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
