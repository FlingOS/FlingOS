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

using Kernel.FileSystems;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Processes;
using Kernel.Multiprocessing;
using Kernel.Utilities;
using Kernel.VirtualMemory;

namespace Kernel.Processes.ELF
{
    public unsafe class ELFProcess : Object
    {
        public uint BaseAddress;
        public List SharedObjectDependencies = new List();

        public List SharedObjectDependencyFilePaths = new List();

        protected ELFFile theFile;
        protected Process theProcess;

        public Process TheProcess
        {
            get { return theProcess; }
        }

        public ELFFile TheFile
        {
            get { return theFile; }
        }

        public ELFProcess(ELFFile anELFFile)
        {
            theFile = anELFFile;
        }

        public void Load(bool UserMode)
        {
            // TODO: Well now that system calls have been implemented, this entire function is just plain wrong.

            //bool OK = true;

            //try
            //{
            //    bool DynamicLinkingRequired = false;

            //    ThreadStartPoint mainMethod = (ThreadStartPoint)ObjectUtilities.GetObject(theFile.Header.EntryPoint);
            //    theProcess = ProcessManager.CreateProcess(
            //        mainMethod, theFile.TheFile.Name, UserMode);

            //    uint threadStackVirtAddr = (uint)((Thread)theProcess.Threads[0]).State->ThreadStackTop -
            //                               Thread.ThreadStackTopOffset;
            //    uint threadStackPhysAddr = VirtualMemoryManager.GetPhysicalAddress(threadStackVirtAddr);
            //    ProcessManager.CurrentProcess.TheMemoryLayout.AddDataPage(threadStackPhysAddr, threadStackVirtAddr);

            //    // Load the ELF segments (i.e. the program code and data)
            //    BaseAddress = theFile.BaseAddress;
            //    LoadSegments(theFile, ref OK, ref DynamicLinkingRequired, BaseAddress);

            //    //BasicConsole.WriteLine();

            //    #region Relocations

            //    // Useful articles / specifications on Relocations:
            //    //      - Useful / practical explanation of various relocation types: http://eli.thegreenplace.net/2011/08/25/load-time-relocation-of-shared-libraries/#id20
            //    //      - Orcale : ELF Specification copy: http://docs.oracle.com/cd/E23824_01/html/819-0690/chapter6-54839.html

            //    if (DynamicLinkingRequired)
            //    {
            //        Console.Default.WriteLine("Dynamic Linking");
            //        BasicConsole.WriteLine("Dynamic Linking");

            //        ELFDynamicSection dynamicSection = theFile.DynamicSection;
            //        ELFDynamicSymbolTableSection dynamicSymbolsSection = theFile.DynamicSymbolsSection;

            //        ELFStringTable DynamicsStringTable = new ELFStringTable(
            //            dynamicSection.StrTabDynamic.Val_Ptr, dynamicSection.StrTabSizeDynamic.Val_Ptr);

            //        for (uint i = 0; i < dynamicSection.Dynamics.Count; i++)
            //        {
            //            ELFDynamicSection.Dynamic theDyn = dynamicSection[i];

            //            //BasicConsole.WriteLine("     - Dynamic : ");
            //            //BasicConsole.Write("         - Tag : ");
            //            //BasicConsole.WriteLine((int)theDyn.Tag);
            //            //BasicConsole.Write("         - Value or Pointer : ");
            //            //BasicConsole.WriteLine(theDyn.Val_Ptr);

            //            if (theDyn.Tag == ELFDynamicSection.DynamicTag.Needed)
            //            {
            //                BasicConsole.Write("         - Needed library name : ");

            //                String libFullPath = DynamicsStringTable[theDyn.Val_Ptr];
            //                Console.Default.WriteLine(libFullPath);
            //                BasicConsole.WriteLine(libFullPath);

            //                String libFileName = (String)libFullPath.Split('\\').Last();
            //                libFileName = (String)libFileName.Split('/').Last();
            //                String libTestPath = theFile.TheFile.Parent.GetFullPath() + libFileName;
            //                File sharedObjectFile = File.Open(libTestPath);
            //                if (sharedObjectFile == null)
            //                {
            //                    Console.Default.WarningColour();
            //                    Console.Default.WriteLine("Failed to find needed library file!");
            //                    BasicConsole.WriteLine("Failed to find needed library file!");
            //                    Console.Default.DefaultColour();
            //                    OK = false;
            //                }
            //                else
            //                {
            //                    Console.Default.WriteLine("Found library file. Loading library...");
            //                    BasicConsole.WriteLine("Found library file. Loading library...");

            //                    ELFSharedObject sharedObject =
            //                        DynamicLinkerLoader.LoadLibrary_FromELFSO(sharedObjectFile, this);
            //                    SharedObjectDependencies.Add(sharedObject);

            //                    Console.Default.WriteLine("Library loaded.");
            //                    BasicConsole.WriteLine("Library loaded.");
            //                }
            //            }
            //        }

            //        Console.Default.WriteLine("Library Relocations");
            //        BasicConsole.WriteLine("Library Relocations");

            //        // Perform relocation / dynamic linking of all libraries
            //        for (int i = 0; i < SharedObjectDependencies.Count; i++)
            //        {
            //            ELFSharedObject SO = (ELFSharedObject)SharedObjectDependencies[i];

            //            //BasicConsole.WriteLine("Shared Object base address : " + (Framework.String)SO.BaseAddress);
            //            //BasicConsole.WriteLine("Shared Object file base address : " + (Framework.String)SO.TheFile.BaseAddress);

            //            List SOSections = SO.TheFile.Sections;
            //            for (int j = 0; j < SOSections.Count; j++)
            //            {
            //                ELFSection SOSection = (ELFSection)SOSections[j];
            //                if (SOSection is ELFRelocationTableSection)
            //                {
            //                    //BasicConsole.WriteLine(" - Normal Relocation");

            //                    ELFRelocationTableSection relocTableSection = (ELFRelocationTableSection)SOSection;
            //                    ELFSymbolTableSection symbolTable =
            //                        (ELFSymbolTableSection)
            //                            SO.TheFile.Sections[relocTableSection.SymbolTableSectionIndex];
            //                    ELFStringTableSection symbolNamesTable =
            //                        (ELFStringTableSection)SO.TheFile.Sections[symbolTable.StringsSectionIndex];

            //                    List Relocations = relocTableSection.Relocations;
            //                    for (int k = 0; k < Relocations.Count; k++)
            //                    {
            //                        // Reference: http://docs.oracle.com/cd/E19683-01/817-3677/chapter6-26/index.html

            //                        ELFRelocationTableSection.Relocation relocation =
            //                            (ELFRelocationTableSection.Relocation)Relocations[k];
            //                        if (relocation.Type == ELFRelocationTableSection.RelocationType.R_386_NONE)
            //                        {
            //                            continue;
            //                        }

            //                        uint* resolvedRelLocation =
            //                            (uint*)(SO.BaseAddress + (relocation.Offset - SO.TheFile.BaseAddress));
            //                        ELFSymbolTableSection.Symbol symbol =
            //                            symbolTable[relocation.Symbol];
            //                        String symbolName = symbolNamesTable[symbol.NameIdx];

            //                        //BasicConsole.WriteLine("Relocation:");
            //                        ////BasicConsole.WriteLine("    > Symbol index : " + (Framework.String)relocation.Symbol);
            //                        //BasicConsole.WriteLine("    > Type : " + (Framework.String)(uint)relocation.Type);
            //                        //BasicConsole.WriteLine("    > Offset : " + (Framework.String)(uint)relocation.Offset);
            //                        //BasicConsole.WriteLine(((Framework.String)"    > Resolved location address: ") + (uint)resolvedRelLocation);
            //                        ////BasicConsole.WriteLine(((Framework.String)"    > Resolved location start value: ") + *resolvedRelLocation);
            //                        //BasicConsole.Write("    > Symbol name : ");
            //                        //BasicConsole.WriteLine(symbolName);

            //                        uint newValue = 0;
            //                        switch (relocation.Type)
            //                        {
            //                            case ELFRelocationTableSection.RelocationType.R_386_32:
            //                                newValue = GetSymbolAddress(symbol, symbolName) + *resolvedRelLocation;
            //                                break;
            //                            case ELFRelocationTableSection.RelocationType.R_386_PC32:
            //                                newValue = GetSymbolAddress(symbol, symbolName) + *resolvedRelLocation -
            //                                           (uint)resolvedRelLocation;
            //                                break;
            //                            case ELFRelocationTableSection.RelocationType.R_386_RELATIVE:
            //                                newValue = SO.BaseAddress + *resolvedRelLocation;
            //                                break;
            //                            //TODO: Support more relocation types
            //                            default:
            //                                Console.Default.WarningColour();
            //                                Console.Default.Write("WARNING: Unrecognised relocation type! (");
            //                                Console.Default.Write_AsDecimal((uint)relocation.Type);
            //                                Console.Default.WriteLine(")");
            //                                Console.Default.DefaultColour();

            //                                BasicConsole.Write("WARNING: Unrecognised relocation type! (");
            //                                BasicConsole.Write((uint)relocation.Type);
            //                                BasicConsole.WriteLine(")");
            //                                break;
            //                        }

            //                        *resolvedRelLocation = newValue;

            //                        //BasicConsole.WriteLine("    > New value: " + (Framework.String)(newValue));
            //                        //BasicConsole.WriteLine("    > Resolved location end value: " + (Framework.String)(*resolvedRelLocation));
            //                    }
            //                }
            //                else if (SOSection is ELFRelocationAddendTableSection)
            //                {
            //                    //BasicConsole.WriteLine(" - Addend Relocation");

            //                    ELFRelocationAddendTableSection relocTableSection =
            //                        (ELFRelocationAddendTableSection)SOSection;
            //                    ELFSymbolTableSection symbolTable =
            //                        (ELFSymbolTableSection)
            //                            SO.TheFile.Sections[relocTableSection.SymbolTableSectionIndex];
            //                    ELFStringTableSection symbolNamesTable =
            //                        (ELFStringTableSection)SO.TheFile.Sections[symbolTable.StringsSectionIndex];

            //                    List Relocations = relocTableSection.Relocations;
            //                    for (int k = 0; k < Relocations.Count; k++)
            //                    {
            //                        ELFRelocationAddendTableSection.RelocationAddend relocation =
            //                            (ELFRelocationAddendTableSection.RelocationAddend)Relocations[k];
            //                        if (relocation.Type == ELFRelocationTableSection.RelocationType.R_386_NONE)
            //                        {
            //                            continue;
            //                        }

            //                        ELFSymbolTableSection.Symbol symbol =
            //                            symbolTable[relocation.Symbol];
            //                        String symbolName = symbolNamesTable[symbol.NameIdx];
            //                        uint* resolvedRelLocation =
            //                            (uint*)(SO.BaseAddress + (relocation.Offset - SO.TheFile.BaseAddress));

            //                        //BasicConsole.WriteLine("Relocation:");
            //                        ////BasicConsole.WriteLine("    > Symbol index : " + (Framework.String)relocation.Symbol);
            //                        //BasicConsole.WriteLine("    > Type : " + (Framework.String)(uint)relocation.Type);
            //                        //BasicConsole.WriteLine("    > Offset : " + (Framework.String)(uint)relocation.Offset);
            //                        //BasicConsole.WriteLine(((Framework.String)"    > Resolved location address: ") + (uint)resolvedRelLocation);
            //                        ////BasicConsole.WriteLine(((Framework.String)"    > Resolved location start value: ") + *resolvedRelLocation);
            //                        //BasicConsole.Write("    > Symbol name : ");
            //                        //BasicConsole.WriteLine(symbolName);

            //                        uint newValue = 0;
            //                        switch (relocation.Type)
            //                        {
            //                            //TODO: Support more relocation types
            //                            default:
            //                                Console.Default.WarningColour();
            //                                Console.Default.Write("WARNING: Unrecognised relocation type! (");
            //                                Console.Default.Write_AsDecimal((uint)relocation.Type);
            //                                Console.Default.WriteLine(")");
            //                                Console.Default.DefaultColour();

            //                                BasicConsole.Write("WARNING: Unrecognised relocation type! (");
            //                                BasicConsole.Write((uint)relocation.Type);
            //                                BasicConsole.WriteLine(")");
            //                                break;
            //                        }

            //                        *resolvedRelLocation = newValue;

            //                        //BasicConsole.WriteLine("    > New value: " + (Framework.String)(newValue));
            //                        //BasicConsole.WriteLine("    > Resolved location end value: " + (Framework.String)(*resolvedRelLocation));
            //                    }
            //                }
            //            }
            //        }

            //        Console.Default.WriteLine("Executable Relocations");
            //        BasicConsole.WriteLine("Executable Relocations");

            //        //BasicConsole.WriteLine("Executable base address : " + (Framework.String)BaseAddress);
            //        //BasicConsole.WriteLine("Executable file base address : " + (Framework.String)theFile.BaseAddress);

            //        // Perform dynamic linking of executable
            //        List ExeSections = theFile.Sections;
            //        for (int j = 0; j < ExeSections.Count; j++)
            //        {
            //            ELFSection ExeSection = (ELFSection)ExeSections[j];
            //            if (ExeSection is ELFRelocationTableSection)
            //            {
            //                //BasicConsole.WriteLine(" - Normal Relocations");

            //                ELFRelocationTableSection relocTableSection = (ELFRelocationTableSection)ExeSection;
            //                ELFSymbolTableSection symbolTable =
            //                    (ELFSymbolTableSection)theFile.Sections[relocTableSection.SymbolTableSectionIndex];
            //                ELFStringTableSection symbolNamesTable =
            //                    (ELFStringTableSection)theFile.Sections[symbolTable.StringsSectionIndex];

            //                List Relocations = relocTableSection.Relocations;
            //                for (int k = 0; k < Relocations.Count; k++)
            //                {
            //                    ELFRelocationTableSection.Relocation relocation =
            //                        (ELFRelocationTableSection.Relocation)Relocations[k];
            //                    if (relocation.Type == ELFRelocationTableSection.RelocationType.R_386_NONE)
            //                    {
            //                        continue;
            //                    }

            //                    uint* resolvedRelLocation =
            //                        (uint*)(BaseAddress + (relocation.Offset - theFile.BaseAddress));
            //                    ELFSymbolTableSection.Symbol symbol =
            //                        symbolTable[relocation.Symbol];
            //                    String symbolName = symbolNamesTable[symbol.NameIdx];

            //                    //BasicConsole.WriteLine("Relocation:");
            //                    ////BasicConsole.WriteLine("    > Symbol index : " + (Framework.String)relocation.Symbol);
            //                    //BasicConsole.WriteLine("    > Type : " + (Framework.String)(uint)relocation.Type);
            //                    //BasicConsole.WriteLine("    > Offset : " + (Framework.String)(uint)relocation.Offset);
            //                    //BasicConsole.WriteLine(((Framework.String)"    > Resolved location address: ") + (uint)resolvedRelLocation);
            //                    ////BasicConsole.WriteLine(((Framework.String)"    > Resolved location start value: ") + *resolvedRelLocation);
            //                    //BasicConsole.Write("    > Symbol name : ");
            //                    //BasicConsole.WriteLine(symbolName);

            //                    bool setFromNewValue = true;
            //                    uint newValue = 0;
            //                    switch (relocation.Type)
            //                    {
            //                        //TODO: Support more relocation types
            //                        case ELFRelocationTableSection.RelocationType.R_386_JMP_SLOT:
            //                            newValue = GetSymbolAddress(symbol, symbolName);
            //                            break;
            //                        case ELFRelocationTableSection.RelocationType.R_386_COPY:
            //                            // Created by the link-editor for dynamic executables to preserve a read-only text segment. 
            //                            // Its offset member refers to a location in a writable segment. The symbol table index 
            //                            // specifies a symbol that should exist both in the current object file and in a shared object. 
            //                            // During execution, the runtime linker copies data associated with the shared object's symbol 
            //                            // to the location specified by the offset.
            //                            // See Copy Relocations:
            //                            //      http://docs.oracle.com/cd/E19683-01/817-3677/6mj8mbtbs/index.html#chapter4-84604

            //                            setFromNewValue = false;
            //                            uint symbolAddress = 0;
            //                            uint symbolSize = 0;

            //                            if (GetSymbolAddressAndSize(symbol, symbolName, ref symbolAddress,
            //                                ref symbolSize))
            //                            {
            //                                byte* symbolValuePtr = (byte*)symbolAddress;

            //                                //BasicConsole.Write("    > Symbol size : ");
            //                                //BasicConsole.WriteLine(symbolSize);

            //                                for (int i = 0; i < symbolSize; i++)
            //                                {
            //                                    resolvedRelLocation[i] = symbolValuePtr[i];
            //                                }
            //                            }
            //                            else
            //                            {
            //                                BasicConsole.WriteLine(
            //                                    "Failed to get symbol address and size for R_386_COPY relocation!");
            //                            }
            //                            break;
            //                        default:
            //                            Console.Default.WarningColour();
            //                            Console.Default.Write("WARNING: Unrecognised relocation type! (");
            //                            Console.Default.Write_AsDecimal((uint)relocation.Type);
            //                            Console.Default.WriteLine(")");
            //                            Console.Default.DefaultColour();

            //                            BasicConsole.Write("WARNING: Unrecognised relocation type! (");
            //                            BasicConsole.Write((uint)relocation.Type);
            //                            BasicConsole.WriteLine(")");
            //                            break;
            //                    }
            //                    if (setFromNewValue)
            //                    {
            //                        *resolvedRelLocation = newValue;
            //                        //BasicConsole.WriteLine("    > New value: " + (Framework.String)(newValue));
            //                        //BasicConsole.WriteLine("    > Resolved location end value: " + (Framework.String)(*resolvedRelLocation));
            //                    }
            //                }
            //            }
            //            else if (ExeSection is ELFRelocationAddendTableSection)
            //            {
            //                //BasicConsole.WriteLine(" - Addend Relocations");

            //                ELFRelocationAddendTableSection relocTableSection =
            //                    (ELFRelocationAddendTableSection)ExeSection;
            //                ELFSymbolTableSection symbolTable =
            //                    (ELFSymbolTableSection)theFile.Sections[relocTableSection.SymbolTableSectionIndex];
            //                ELFStringTableSection symbolNamesTable =
            //                    (ELFStringTableSection)theFile.Sections[symbolTable.StringsSectionIndex];

            //                List Relocations = relocTableSection.Relocations;
            //                for (int k = 0; k < Relocations.Count; k++)
            //                {
            //                    ELFRelocationAddendTableSection.RelocationAddend relocation =
            //                        (ELFRelocationAddendTableSection.RelocationAddend)Relocations[k];
            //                    if (relocation.Type == ELFRelocationTableSection.RelocationType.R_386_NONE)
            //                    {
            //                        continue;
            //                    }

            //                    uint* resolvedRelLocation =
            //                        (uint*)(BaseAddress + (relocation.Offset - theFile.BaseAddress));
            //                    ELFSymbolTableSection.Symbol symbol =
            //                        symbolTable[relocation.Symbol];
            //                    String symbolName = symbolNamesTable[symbol.NameIdx];

            //                    //BasicConsole.WriteLine("Relocation:");
            //                    ////BasicConsole.WriteLine("    > Symbol index : " + (Framework.String)relocation.Symbol);
            //                    //BasicConsole.WriteLine("    > Type : " + (Framework.String)(uint)relocation.Type);
            //                    //BasicConsole.WriteLine("    > Offset : " + (Framework.String)(uint)relocation.Offset);
            //                    //BasicConsole.WriteLine(((Framework.String)"    > Resolved location address: ") + (uint)resolvedRelLocation);
            //                    ////BasicConsole.WriteLine(((Framework.String)"    > Resolved location start value: ") + *resolvedRelLocation);
            //                    //BasicConsole.Write("    > Symbol name : ");
            //                    //BasicConsole.WriteLine(symbolName);

            //                    uint newValue = 0;
            //                    switch (relocation.Type)
            //                    {
            //                        //TODO: Support more relocation types
            //                        default:
            //                            Console.Default.WarningColour();
            //                            Console.Default.Write("WARNING: Unrecognised relocation type! (");
            //                            Console.Default.Write_AsDecimal((uint)relocation.Type);
            //                            Console.Default.WriteLine(")");
            //                            Console.Default.DefaultColour();

            //                            BasicConsole.Write("WARNING: Unrecognised relocation type! (");
            //                            BasicConsole.Write((uint)relocation.Type);
            //                            BasicConsole.WriteLine(")");
            //                            break;
            //                    }
            //                    *resolvedRelLocation = newValue;
            //                    //BasicConsole.WriteLine("    > New value: " + (Framework.String)(newValue));
            //                    //BasicConsole.WriteLine("    > Resolved location end value: " + (Framework.String)(*resolvedRelLocation));
            //                }
            //            }
            //        }

            //        // TODO: Call Init functions of libraries
            //    }

            //    // Unmap processes' memory from current processes' memory
            //    for (int i = 0; i < SharedObjectDependencies.Count; i++)
            //    {
            //        ELFSharedObject SO = (ELFSharedObject)SharedObjectDependencies[i];
            //        uint FileBaseAddress = SO.TheFile.BaseAddress;
            //        uint MemBaseAddress = SO.BaseAddress;

            //        List SOSegments = SO.TheFile.Segments;
            //        for (int j = 0; j < SOSegments.Count; j++)
            //        {
            //            ELFSegment SOSegment = (ELFSegment)SOSegments[j];
            //            ProcessManager.CurrentProcess.TheMemoryLayout.RemovePage(
            //                (MemBaseAddress + ((uint)SOSegment.Header.VAddr - FileBaseAddress)) & 0xFFFFF000);
            //        }
            //    }
            //    {
            //        uint FileBaseAddress = theFile.BaseAddress;
            //        uint MemBaseAddress = BaseAddress;

            //        List ExeSegments = theFile.Segments;
            //        for (int j = 0; j < ExeSegments.Count; j++)
            //        {
            //            ELFSegment ExeSegment = (ELFSegment)ExeSegments[j];
            //            ProcessManager.CurrentProcess.TheMemoryLayout.RemovePage(
            //                (MemBaseAddress + ((uint)ExeSegment.Header.VAddr - FileBaseAddress)) & 0xFFFFF000);
            //        }
            //    }

            //    #endregion

            //    ProcessManager.CurrentProcess.TheMemoryLayout.RemovePage(threadStackVirtAddr);
            //}
            //finally
            //{
            //    if (!OK)
            //    {
            //        theProcess = null;
            //    }
            //}
        }

        public void LoadSegments(ELFFile fileToLoadFrom, ref bool OK, ref bool DynamicLinkingRequired,
            uint memBaseAddress)
        {
            uint fileBaseAddress = fileToLoadFrom.BaseAddress;
            List Segments = fileToLoadFrom.Segments;

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
                    int bytesRead = segment.Read(fileToLoadFrom.TheStream);
                    if (bytesRead != segment.Header.FileSize)
                    {
                        OK = false;
                        ExceptionMethods.Throw(
                            new Exception("Error loading ELF segments! Failed to load correct segment bytes from file."));
                    }

                    byte* destMemPtr = segment.Header.VAddr - fileBaseAddress + memBaseAddress;
                    byte* pageAlignedDestMemPtr = (byte*)((uint)destMemPtr & 0xFFFFF000);

                    Console.Default.Write(" Loading segment from ");
                    Console.Default.Write_AsDecimal((uint)segment.Header.VAddr);
                    Console.Default.Write(" to ");
                    Console.Default.WriteLine_AsDecimal((uint)destMemPtr);

                    BasicConsole.Write(" Loading segment from ");
                    BasicConsole.Write((uint)segment.Header.VAddr);
                    BasicConsole.Write(" to ");
                    BasicConsole.WriteLine((uint)destMemPtr);
                    SystemCalls.SleepThread(1000);

                    uint copyOffset = (uint)(destMemPtr - pageAlignedDestMemPtr);
                    uint copyFromOffset = 0;

                    bool executable = (segment.Header.Flags & ELFFlags.Executable) != 0;

                    for (uint pageOffset = 0; pageOffset < segment.Header.MemSize; pageOffset += 4096)
                    {
                        //uint physPageAddr = Hardware.VirtMemManager.FindFreePhysPage();
                        //uint virtPageAddr = (uint)pageAlignedDestMemPtr + pageOffset;

                        //TODO: Need to update this to use system calls
                        //Hardware.VirtMemManager.Map(
                        //    physPageAddr,
                        //    virtPageAddr,
                        //    4096,
                        //    theProcess.UserMode ? Hardware.VirtMem.VirtMemImpl.PageFlags.None : Hardware.VirtMem.VirtMemImpl.PageFlags.KernelOnly);
                        //Hardware.Processes.ProcessManager.CurrentProcess.TheMemoryLayout.AddDataPage(physPageAddr, virtPageAddr);

                        //if (executable)
                        //{
                        //    theProcess.TheMemoryLayout.AddCodePage(physPageAddr, virtPageAddr);
                        //}
                        //else
                        //{
                        //    theProcess.TheMemoryLayout.AddDataPage(physPageAddr, virtPageAddr);
                        //}

                        //uint copySize = Framework.Math.Min((uint)bytesRead, 4096 - copyOffset);
                        //if (copySize > 0)
                        //{
                        //    Utilities.MemoryUtils.MemCpy_32(
                        //        (byte*)(virtPageAddr + copyOffset),
                        //        ((byte*)Utilities.ObjectUtilities.GetHandle(segment.Data)) + Framework.Array.FieldsBytesSize + pageOffset - copyFromOffset,
                        //        copySize);

                        //    bytesRead -= (int)copySize;
                        //}

                        //for (uint j = copySize + copyOffset; j < 4096; j++)
                        //{
                        //    *(byte*)(virtPageAddr + j) = 0;
                        //}

                        if (copyOffset > 0)
                        {
                            copyFromOffset += copyOffset;
                            copyOffset = 0;
                        }
                    }
                }
            }
        }

        public uint GetSymbolAddress(ELFSymbolTableSection.Symbol theSymbol, String theSymbolName)
        {
            uint address = 0;
            uint size = 0;
            GetSymbolAddressAndSize(theSymbol, theSymbolName, ref address, ref size);
            return address;
        }

        public bool GetSymbolAddressAndSize(ELFSymbolTableSection.Symbol theSymbol, String theSymbolName,
            ref uint address, ref uint size)
        {
            //BasicConsole.WriteLine("Searching for symbol...");
            //BasicConsole.Write("     - Name : ");
            //BasicConsole.WriteLine(theSymbolName);

            //BasicConsole.WriteLine("     Searching executable's symbols...");
            for (int i = 0; i < theFile.Sections.Count; i++)
            {
                ELFSection aSection = (ELFSection)theFile.Sections[i];
                if (aSection is ELFSymbolTableSection)
                {
                    ELFSymbolTableSection symTabSection = (ELFSymbolTableSection)aSection;
                    ELFStringTableSection strTabSection =
                        (ELFStringTableSection)theFile.Sections[symTabSection.StringsSectionIndex];

                    for (int j = 0; j < symTabSection.Symbols.Count; j++)
                    {
                        ELFSymbolTableSection.Symbol aSymbol = (ELFSymbolTableSection.Symbol)symTabSection.Symbols[j];

                        if (aSymbol.Type == theSymbol.Type &&
                            aSymbol.Binding == ELFSymbolTableSection.SymbolBinding.Global &&
                            aSymbol.SectionIndex > 0)
                        {
                            if (strTabSection.IsMatch(aSymbol.NameIdx, theSymbolName))
                            {
                                //BasicConsole.WriteLine("     Found symbol.");
                                //BasicConsole.Write("     aSymbol Address : ");
                                uint result = (uint)aSymbol.Value - theFile.BaseAddress + BaseAddress;
                                //BasicConsole.WriteLine(result);

                                address = result;
                                size = aSymbol.Size;
                                return true;
                            }
                        }
                    }
                }
            }
            for (int k = 0; k < SharedObjectDependencies.Count; k++)
            {
                //BasicConsole.WriteLine("     Searching shared object's symbols...");

                ELFSharedObject SO = (ELFSharedObject)SharedObjectDependencies[k];
                for (int i = 0; i < SO.TheFile.Sections.Count; i++)
                {
                    ELFSection aSection = (ELFSection)SO.TheFile.Sections[i];
                    if (aSection is ELFSymbolTableSection)
                    {
                        ELFSymbolTableSection symTabSection = (ELFSymbolTableSection)aSection;
                        ELFStringTableSection strTabSection =
                            (ELFStringTableSection)SO.TheFile.Sections[symTabSection.StringsSectionIndex];
                        for (int j = 0; j < symTabSection.Symbols.Count; j++)
                        {
                            ELFSymbolTableSection.Symbol aSymbol =
                                (ELFSymbolTableSection.Symbol)symTabSection.Symbols[j];
                            if (aSymbol.Type == theSymbol.Type &&
                                aSymbol.Binding == ELFSymbolTableSection.SymbolBinding.Global &&
                                aSymbol.SectionIndex > 0)
                            {
                                if (strTabSection.IsMatch(aSymbol.NameIdx, theSymbolName))
                                {
                                    //BasicConsole.WriteLine("     Found symbol.");
                                    //BasicConsole.Write("     aSymbol Address : ");
                                    uint result = (uint)aSymbol.Value - SO.TheFile.BaseAddress + SO.BaseAddress;
                                    //BasicConsole.WriteLine(result);

                                    address = result;
                                    size = aSymbol.Size;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}