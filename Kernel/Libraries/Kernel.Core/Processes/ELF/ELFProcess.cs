#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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

        public List SharedObjectDependencyFilePaths = new List();
        public List SharedObjectDependencies = new List();

        public uint BaseAddress = 0;

        public ELFProcess(ELFFile anELFFile)
        {
            theFile = anELFFile;
        }

        public void Load(bool UserMode)
        {
            bool OK = true;
            
            try
            {
                bool DynamicLinkingRequired = false;

                ThreadStartMethod mainMethod = (ThreadStartMethod)Utilities.ObjectUtilities.GetObject(theFile.Header.EntryPoint);
                theProcess = ProcessManager.CreateProcess(
                    mainMethod, theFile.TheFile.Name, UserMode);

                // Load the ELF segments (i.e. the program code and data)
                BaseAddress = theFile.BaseAddress;
                LoadSegments(theFile, ref OK, ref DynamicLinkingRequired, BaseAddress);

                //Console.Default.WriteLine();

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

                        //Console.Default.WriteLine("     - Dynamic : ");
                        //Console.Default.Write("         - Tag : ");
                        //Console.Default.WriteLine_AsDecimal((int)theDyn.Tag);
                        //Console.Default.Write("         - Value or Pointer : ");
                        //Console.Default.WriteLine_AsDecimal(theDyn.Val_Ptr);

                        if (theDyn.Tag == ELFDynamicSection.DynamicTag.Needed)
                        {
                            //Console.Default.Write("         - Needed library name : ");

                            FOS_System.String libFullPath = DynamicsStringTable[theDyn.Val_Ptr];
                            Console.Default.WriteLine(libFullPath);

                            FOS_System.String libFileName = (FOS_System.String)libFullPath.Split('\\').Last();
                            libFileName = (FOS_System.String)libFileName.Split('/').Last();
                            FOS_System.String libTestPath = theFile.TheFile.Parent.GetFullPath() + libFileName;
                            File sharedObjectFile = File.Open(libTestPath);
                            if (sharedObjectFile == null)
                            {
                                Console.Default.WarningColour();
                                Console.Default.WriteLine("Failed to find needed library file!");
                                Console.Default.DefaultColour();
                                OK = false;
                            }
                            else
                            {
                                Console.Default.WriteLine("Found library file. Loading library...");

                                ELFSharedObject sharedObject = DynamicLinkerLoader.LoadLibrary_FromELFSO(sharedObjectFile, this);
                                SharedObjectDependencies.Add(sharedObject);

                                Console.Default.WriteLine("Library loaded.");
                            }
                        }
                    }

                    FOS_System.GC.Cleanup();

                    Console.Default.WriteLine("Library Relocations");

                    // Perform relocation / dynamic linking of all libraries
                    for (int i = 0; i < SharedObjectDependencies.Count; i++)
                    {
                        ELFSharedObject SO = (ELFSharedObject)SharedObjectDependencies[i];
                        List SOSections = SO.TheFile.Sections;
                        for (int j = 0; j < SOSections.Count; j++)
                        {
                            ELFSection SOSection = (ELFSection)SOSections[j];
                            if (SOSection is ELFRelocationTableSection)
                            {
                                //Console.Default.WriteLine(" - Normal Relocation");

                                ELFRelocationTableSection relocTableSection = (ELFRelocationTableSection)SOSection;
                                List Relocations = relocTableSection.Relocations;
                                for (int k = 0; k < Relocations.Count; k++)
                                {
                                    ELFRelocationTableSection.Relocation relocation = (ELFRelocationTableSection.Relocation)Relocations[k];
                                    if (relocation.Type == ELFRelocationTableSection.RelocationType.R_386_NONE)
                                    {
                                        continue;
                                    }

                                    uint* resolvedRelLocation = (uint*)(SO.BaseAddress + (relocation.Offset - SO.TheFile.BaseAddress));
                                    uint newValue = 0;
                                    switch (relocation.Type)
                                    {
                                        //TODO: Support more relocation types
                                        default:
                                            Console.Default.WarningColour();
                                            Console.Default.Write("WARNING: Unrecognised relocation type! (");
                                            Console.Default.Write_AsDecimal((uint)relocation.Type);
                                            Console.Default.WriteLine(")");
                                            Console.Default.DefaultColour();
                                            break;
                                    }
                                    *resolvedRelLocation = newValue;
                                }
                            }
                            else if (SOSection is ELFRelocationAddendTableSection)
                            {
                                //Console.Default.WriteLine(" - Addend Relocation");

                                ELFRelocationAddendTableSection relocTableSection = (ELFRelocationAddendTableSection)SOSection;
                                List Relocations = relocTableSection.Relocations;
                                for (int k = 0; k < Relocations.Count; k++)
                                {
                                    ELFRelocationAddendTableSection.RelocationAddend relocation = (ELFRelocationAddendTableSection.RelocationAddend)Relocations[k];
                                    if (relocation.Type == ELFRelocationTableSection.RelocationType.R_386_NONE)
                                    {
                                        continue;
                                    }

                                    uint* resolvedRelLocation = (uint*)(SO.BaseAddress + (relocation.Offset - SO.TheFile.BaseAddress));
                                    uint newValue = 0;
                                    switch (relocation.Type)
                                    {
                                        //TODO: Support more relocation types
                                        default:
                                            Console.Default.WarningColour();
                                            Console.Default.Write("WARNING: Unrecognised relocation type! (");
                                            Console.Default.Write_AsDecimal((uint)relocation.Type);
                                            Console.Default.WriteLine(")");
                                            Console.Default.DefaultColour();
                                            break;
                                    }
                                    *resolvedRelLocation = newValue;
                                }
                            }
                        }
                    }

                    FOS_System.GC.Cleanup();

                    Console.Default.WriteLine("Executable Relocations");

                    // Perform dynamic linking of executable
                    List ExeSections = theFile.Sections;
                    for (int j = 0; j < ExeSections.Count; j++)
                    {
                        ELFSection ExeSection = (ELFSection)ExeSections[j];
                        if (ExeSection is ELFRelocationTableSection)
                        {
                            //Console.Default.WriteLine(" - Normal Relocations");

                            ELFRelocationTableSection relocTableSection = (ELFRelocationTableSection)ExeSection;
                            ELFSymbolTableSection symbolTable = (ELFSymbolTableSection)theFile.Sections[relocTableSection.SymbolTableSectionIndex];
                            ELFStringTableSection symbolNamesTable = (ELFStringTableSection)theFile.Sections[symbolTable.StringsSectionIndex];

                            List Relocations = relocTableSection.Relocations;
                            for (int k = 0; k < Relocations.Count; k++)
                            {
                                ELFRelocationTableSection.Relocation relocation = (ELFRelocationTableSection.Relocation)Relocations[k];
                                if (relocation.Type == ELFRelocationTableSection.RelocationType.R_386_NONE)
                                {
                                    continue;
                                }

                                //Console.Default.WriteLine("     - Relocation :");
                                //Console.Default.Write("         - Type : ");
                                //Console.Default.WriteLine_AsDecimal((uint)relocation.Type);

                                ELFSymbolTableSection.Symbol symbol = (ELFSymbolTableSection.Symbol)symbolTable[relocation.Symbol];
                                FOS_System.String symbolName = symbolNamesTable[symbol.NameIdx];

                                //Console.Default.Write("         - Symbol Name : ");
                                //Console.Default.WriteLine(symbolName);

                                uint* resolvedRelLocation = (uint*)(BaseAddress + (relocation.Offset - theFile.BaseAddress));
                                uint newValue = 0;
                                switch (relocation.Type)
                                {
                                    //TODO: Support more relocation types
                                    case ELFRelocationTableSection.RelocationType.R_386_JMP_SLOT:
                                        newValue = GetSymbolAddress(symbol, symbolName);
                                        break;
                                    default:
                                        Console.Default.WarningColour();
                                        Console.Default.Write("WARNING: Unrecognised relocation type! (");
                                        Console.Default.Write_AsDecimal((uint)relocation.Type);
                                        Console.Default.WriteLine(")");
                                        Console.Default.DefaultColour();
                                        break;
                                }
                                *resolvedRelLocation = newValue;
                            }
                        }
                        else if (ExeSection is ELFRelocationAddendTableSection)
                        {
                            //Console.Default.WriteLine(" - Addend Relocations");

                            ELFRelocationAddendTableSection relocTableSection = (ELFRelocationAddendTableSection)ExeSection;
                            List Relocations = relocTableSection.Relocations;
                            for (int k = 0; k < Relocations.Count; k++)
                            {
                                ELFRelocationAddendTableSection.RelocationAddend relocation = (ELFRelocationAddendTableSection.RelocationAddend)Relocations[k];
                                if (relocation.Type == ELFRelocationTableSection.RelocationType.R_386_NONE)
                                {
                                    continue;
                                }

                                uint* resolvedRelLocation = (uint*)(BaseAddress + (relocation.Offset - theFile.BaseAddress));
                                uint newValue = 0;
                                switch (relocation.Type)
                                {
                                    //TODO: Support more relocation types
                                    default:
                                        Console.Default.WarningColour();
                                        Console.Default.Write("WARNING: Unrecognised relocation type! (");
                                        Console.Default.Write_AsDecimal((uint)relocation.Type);
                                        Console.Default.WriteLine(")");
                                        Console.Default.DefaultColour();
                                        break;
                                }
                                *resolvedRelLocation = newValue;
                            }
                        }
                    }

                    FOS_System.GC.Cleanup();

                    // TODO: Call Init functions of libraries
                }

                // Unmap processes' memory from current processes' memory
                for (int i = 0; i < SharedObjectDependencies.Count; i++)
                {
                    ELFSharedObject SO = (ELFSharedObject)SharedObjectDependencies[i];
                    uint FileBaseAddress = SO.TheFile.BaseAddress;
                    uint MemBaseAddress = SO.BaseAddress;

                    List SOSegments = SO.TheFile.Segments;
                    for (int j = 0; j < SOSegments.Count; j++)
                    {
                        ELFSegment SOSegment = (ELFSegment)SOSegments[j];
                        ProcessManager.CurrentProcess.TheMemoryLayout.RemovePage(
                            (MemBaseAddress + ((uint)SOSegment.Header.VAddr - FileBaseAddress)) & 0xFFFFF000);
                    }
                }
                {
                    uint FileBaseAddress = theFile.BaseAddress;
                    uint MemBaseAddress = BaseAddress;

                    List ExeSegments = theFile.Segments;
                    for (int j = 0; j < ExeSegments.Count; j++)
                    {
                        ELFSegment ExeSegment = (ELFSegment)ExeSegments[j];
                        ProcessManager.CurrentProcess.TheMemoryLayout.RemovePage(
                            (MemBaseAddress + ((uint)ExeSegment.Header.VAddr - FileBaseAddress)) & 0xFFFFF000);
                    }
                }
            }
            finally
            {
                if (!OK)
                {
                    theProcess = null;
                }
            }
        }

        public void LoadSegments(ELFFile fileToLoadFrom, ref bool OK, ref bool DynamicLinkingRequired, uint memBaseAddress)
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
                        ExceptionMethods.Throw(new FOS_System.Exception("Error loading ELF segments! Failed to load correct segment bytes from file."));
                    }

                    byte* destMemPtr = (segment.Header.VAddr - fileBaseAddress) + memBaseAddress;
                    byte* pageAlignedDestMemPtr = (byte*)((uint)destMemPtr & 0xFFFFF000);

                    Console.Default.Write(" Loading segment from ");
                    Console.Default.Write_AsDecimal((uint)segment.Header.VAddr);
                    Console.Default.Write(" to ");
                    Console.Default.WriteLine_AsDecimal((uint)destMemPtr);
                    Hardware.Processes.Thread.Sleep(1000);

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
                            theProcess.UserMode ? Hardware.VirtMem.VirtMemImpl.PageFlags.None : Hardware.VirtMem.VirtMemImpl.PageFlags.KernelOnly);
                        ProcessManager.CurrentProcess.TheMemoryLayout.AddDataPage(physPageAddr, virtPageAddr);

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

            FOS_System.GC.Cleanup();
        }

        public uint GetSymbolAddress(ELFDynamicSymbolTableSection.Symbol theSymbol, FOS_System.String theSymbolName)
        {
            //Console.Default.WriteLine("Searching for symbol...");
            //Console.Default.Write("     - Name : ");
            //Console.Default.WriteLine(theSymbolName);

            //Console.Default.WriteLine("     Searching executable's symbols...");
            for(int i = 0; i < theFile.Sections.Count; i++)
            {
                ELFSection aSection = (ELFSection)theFile.Sections[i];
                if (aSection is ELFSymbolTableSection)
                {
                    ELFSymbolTableSection symTabSection = (ELFSymbolTableSection)aSection;
                    ELFStringTableSection strTabSection = (ELFStringTableSection)theFile.Sections[symTabSection.StringsSectionIndex];
                    for (int j = 0; j < symTabSection.Symbols.Count; j++)
                    {
                        ELFSymbolTableSection.Symbol aSymbol = (ELFSymbolTableSection.Symbol)symTabSection.Symbols[j];
                        if (aSymbol.Type == theSymbol.Type &&
                            aSymbol.Binding == ELFSymbolTableSection.SymbolBinding.Global &&
                            aSymbol.SectionIndex > 0)
                        {
                            if (strTabSection[aSymbol.NameIdx] == theSymbolName)
                            {
                                //Console.Default.WriteLine("     Found symbol.");
                                //Console.Default.Write("     Address : ");
                                uint result = ((uint)aSymbol.Value - theFile.BaseAddress) + BaseAddress;
                                //Console.Default.WriteLine_AsDecimal(result);
                                return result;
                            }
                        }
                    }
                }
            }
            for (int k = 0; k < SharedObjectDependencies.Count; k++)
            {
                //Console.Default.WriteLine("     Searching shared object's symbols...");

                ELFSharedObject SO = (ELFSharedObject)SharedObjectDependencies[k];
                for (int i = 0; i < SO.TheFile.Sections.Count; i++)
                {
                    ELFSection aSection = (ELFSection)SO.TheFile.Sections[i];
                    if (aSection is ELFSymbolTableSection)
                    {
                        ELFSymbolTableSection symTabSection = (ELFSymbolTableSection)aSection;
                        ELFStringTableSection strTabSection = (ELFStringTableSection)SO.TheFile.Sections[symTabSection.StringsSectionIndex];
                        for (int j = 0; j < symTabSection.Symbols.Count; j++)
                        {
                            ELFSymbolTableSection.Symbol aSymbol = (ELFSymbolTableSection.Symbol)symTabSection.Symbols[j];
                            if (aSymbol.Type == theSymbol.Type &&
                                aSymbol.Binding == ELFSymbolTableSection.SymbolBinding.Global &&
                                aSymbol.SectionIndex > 0)
                            {
                                if (strTabSection[aSymbol.NameIdx] == theSymbolName)
                                {
                                    //Console.Default.WriteLine("     Found symbol.");
                                    //Console.Default.Write("     Address : ");
                                    uint result  =((uint)aSymbol.Value - SO.TheFile.BaseAddress) + SO.BaseAddress;
                                    //Console.Default.WriteLine_AsDecimal(result);
                                    return result;
                                }
                            }
                        }
                    }
                }
            }

            return 0;
        }
    }
}
