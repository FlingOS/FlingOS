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
using Kernel.FileSystems.Streams;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Stubs;

namespace Kernel.Processes.ELF
{
    //TODO: Well, this class is now wrong because it doesn't use file system calls

    public unsafe class ELFFile : Object
    {
        private readonly File theFile;
        private readonly FileStream theStream;
        private uint baseAddress;

        public ELFDynamicSection DynamicSection;
        public ELFDynamicSymbolTableSection DynamicSymbolsSection;

        private bool FoundBaseAddress;

        private ELFHeader header;
        public ELFStringTableSection NamesTable;

        public List Sections = new List();
        public List Segments = new List();

        public File TheFile
        {
            get { return theFile; }
        }

        public FileStream TheStream
        {
            get { return theStream; }
        }

        public ELFHeader Header
        {
            get
            {
                if (header == null)
                {
                    ReadHeader();
                }
                return header;
            }
        }

        public uint BaseAddress
        {
            get
            {
                if (!FoundBaseAddress)
                {
                    FoundBaseAddress = true;

                    baseAddress = UInt32.MaxValue;
                    for (int i = 0; i < Segments.Count; i++)
                    {
                        ELFSegment segment = (ELFSegment)Segments[i];
                        if (segment.Header.Type == ELFSegmentType.Load)
                        {
                            if ((uint)segment.Header.VAddr < baseAddress)
                            {
                                baseAddress = (uint)segment.Header.VAddr;
                            }
                        }
                    }
                }
                return baseAddress;
            }
        }

        public ELFFile(File file)
        {
            theFile = file;
            if (theFile == null)
            {
                Console.Default.ErrorColour();
                Console.Default.Write("Error constructing ELF file! theFile is null");
                BasicConsole.Write("Error constructing ELF file! theFile is null");
                if (file == null)
                {
                    Console.Default.Write(" and file is null");
                    BasicConsole.Write(" and file is null");
                }
                else
                {
                    Console.Default.Write(" and file is NOT null");
                    BasicConsole.Write(" and file is NOT null");
                }
                Console.Default.WriteLine(".");
                BasicConsole.WriteLine(".");
                Console.Default.DefaultColour();
                ExceptionMethods.Throw(new Exception("Error loading ELF file! Supplied file is null."));
            }
            else
            {
                theStream = new CachedFileStream(theFile.GetStream());
                ReadHeader();

                if (IsValidFile())
                {
                    ReadSectionHeaders();
                    ReadSegmentHeaders();
                }
            }
        }

        private void ReadHeader()
        {
            byte[] headerData = new byte[ELFHeader.HEADER_SIZE];
            theStream.Position = 0;
            int bytesRead = theStream.Read(headerData, 0, headerData.Length);

            if (bytesRead == headerData.Length)
            {
                header = new ELFHeader(headerData);
            }
            else
            {
                ExceptionMethods.Throw(new Exception("Failed to read header data from file!"));
            }
        }

        public void ReadSectionHeaders()
        {
            byte[] sectionsData = new byte[header.SecHeaderEntrySize*header.SecHeaderNumEntries];
            theStream.Position = header.SecHeaderTableOffset;
            int bytesRead = theStream.Read(sectionsData, 0, sectionsData.Length);

            if (bytesRead == sectionsData.Length)
            {
                uint offset = 0;
                while (offset < sectionsData.Length)
                {
                    ELFSectionHeader newHeader = new ELFSectionHeader(sectionsData, ref offset);
                    ELFSection newSection = ELFSection.GetSection(newHeader);

                    if (Sections.Count == header.SecHeaderIdxForSecNameStrings)
                    {
                        if (!(newSection is ELFStringTableSection))
                        {
                            ExceptionMethods.Throw(
                                new Exception("Expected Strings Table section was not a strings table section!"));
                        }
                        NamesTable = (ELFStringTableSection)newSection;
                    }

                    newSection.Read(theStream);

                    if (newSection is ELFDynamicSection)
                    {
                        DynamicSection = (ELFDynamicSection)newSection;
                    }
                    else if (newSection is ELFDynamicSymbolTableSection)
                    {
                        DynamicSymbolsSection = (ELFDynamicSymbolTableSection)newSection;
                    }

                    Sections.Add(newSection);
                }

                //BasicConsole.WriteLine();

                #region Sections Output

                //for (int i = 0; i < Sections.Count; i++)
                //{
                //    ELFSection theSection = (ELFSection)Sections[i];
                //    ELFSectionHeader theHeader = theSection.Header;
                //    BasicConsole.WriteLine("ELF section: ");
                //    BasicConsole.Write(" - Name index : ");
                //    BasicConsole.WriteLine(theHeader.NameIndex);
                //    BasicConsole.Write(" - Name : ");
                //    BasicConsole.WriteLine(NamesTable[theHeader.NameIndex]);
                //    BasicConsole.Write(" - Type : ");
                //    BasicConsole.WriteLine((uint)theHeader.SectionType);
                //    BasicConsole.Write(" - Flags : ");
                //    BasicConsole.WriteLine((uint)theHeader.Flags);
                //    BasicConsole.Write(" - Offset : ");
                //    BasicConsole.WriteLine(theHeader.SectionFileOffset);
                //    BasicConsole.Write(" - Size : ");
                //    BasicConsole.WriteLine(theHeader.SectionSize);
                //    BasicConsole.Write(" - Load address : ");
                //    BasicConsole.WriteLine((uint)theHeader.LoadAddress);

                //    if (theSection is ELFSymbolTableSection)
                //    {
                //        #region ELFSymbolTableSection

                //        BasicConsole.WriteLine(" - Symbol table :");

                //        ELFSymbolTableSection theSymTable = (ELFSymbolTableSection)theSection;
                //        ELFStringTableSection theStringTable = (ELFStringTableSection)(Sections[theSymTable.StringsSectionIndex]);
                //        for (uint j = 0; j < theSymTable.Symbols.Count; j++)
                //        {
                //            ELFSymbolTableSection.Symbol theSym = theSymTable[j];

                //            BasicConsole.Write("     - Symbol : ");
                //            BasicConsole.WriteLine(theStringTable[theSym.NameIdx]);
                //            BasicConsole.Write("         - Type : ");
                //            BasicConsole.WriteLine((uint)theSym.Type);
                //            BasicConsole.Write("         - Binding : ");
                //            BasicConsole.WriteLine((uint)theSym.Binding);
                //            BasicConsole.Write("         - Section index : ");
                //            BasicConsole.WriteLine(theSym.SectionIndex);
                //            BasicConsole.Write("         - Value : ");
                //            BasicConsole.WriteLine((uint)theSym.Value);
                //            BasicConsole.Write("         - Blocks : ");
                //            BasicConsole.WriteLine(theSym.Blocks);
                //        }

                //        #endregion
                //    }
                //    else if (theSection is ELFRelocationAddendTableSection)
                //    {
                //        #region ELFRelocationAddendTableSection

                //        ELFRelocationAddendTableSection theRelASection = (ELFRelocationAddendTableSection)theSection;

                //        BasicConsole.WriteLine(" - Relocation (with addends) table :");
                //        BasicConsole.Write("     - Symbol table index : ");
                //        BasicConsole.WriteLine(theRelASection.SymbolTableSectionIndex);
                //        BasicConsole.Write("     - Section to relocate index : ");
                //        BasicConsole.WriteLine(theRelASection.SectionToRelocateIndex);

                //        for (uint j = 0; j < theRelASection.Relocations.Count; j++)
                //        {
                //            ELFRelocationAddendTableSection.RelocationAddend theRel = theRelASection[j];

                //            BasicConsole.WriteLine("     - Relocation : ");
                //            BasicConsole.Write("         - Type : ");
                //            BasicConsole.WriteLine((uint)theRel.Type);
                //            BasicConsole.Write("         - Symbol : ");
                //            BasicConsole.WriteLine(theRel.Symbol);
                //            BasicConsole.Write("         - Offset : ");
                //            BasicConsole.WriteLine((uint)theRel.Offset);
                //            BasicConsole.Write("         - Addend : ");
                //            BasicConsole.WriteLine(theRel.Addend);
                //        }

                //        #endregion
                //    }
                //    else if (theSection is ELFRelocationTableSection)
                //    {
                //        #region ELFRelocationTableSection

                //        ELFRelocationTableSection theRelSection = (ELFRelocationTableSection)theSection;

                //        BasicConsole.WriteLine(" - Relocation table :");
                //        BasicConsole.Write("     - Symbol table index : ");
                //        BasicConsole.WriteLine(theRelSection.SymbolTableSectionIndex);
                //        BasicConsole.Write("     - Section to relocate index : ");
                //        BasicConsole.WriteLine(theRelSection.SectionToRelocateIndex);

                //        for (uint j = 0; j < theRelSection.Relocations.Count; j++)
                //        {
                //            ELFRelocationTableSection.Relocation theRel = theRelSection[j];

                //            BasicConsole.WriteLine("     - Relocation : ");
                //            BasicConsole.Write("         - Type : ");
                //            BasicConsole.WriteLine((uint)theRel.Type);
                //            BasicConsole.Write("         - Symbol : ");
                //            BasicConsole.WriteLine(theRel.Symbol);
                //            BasicConsole.Write("         - Offset : ");
                //            BasicConsole.WriteLine((uint)theRel.Offset);
                //        }

                //        #endregion
                //    }
                //    if (theSection is ELFDynamicSection)
                //    {
                //        #region ELFDynamicSection

                //        BasicConsole.WriteLine(" - Dynamics table :");

                //        ELFDynamicSection theDynTable = (ELFDynamicSection)theSection;
                //        ELFDynamicSection.Dynamic StrTabDynamic = theDynTable.StrTabDynamic;
                //        ELFDynamicSection.Dynamic StrTabSizeDynamic = theDynTable.StrTabSizeDynamic;
                //        if (StrTabDynamic == null ||
                //            StrTabSizeDynamic == null)
                //        {
                //            Console.Default.WarningColour();
                //            BasicConsole.WriteLine("WARNING: Dynamic Table's String Table not found!");
                //            Console.Default.DefaultColour();
                //        }
                //        else
                //        {
                //            BasicConsole.Write("     - String table offset : ");
                //            BasicConsole.WriteLine(StrTabDynamic.Val_Ptr);
                //            BasicConsole.Write("     - String table size : ");
                //            BasicConsole.WriteLine(StrTabSizeDynamic.Val_Ptr);

                //            for (uint j = 0; j < theDynTable.Dynamics.Count; j++)
                //            {
                //                ELFDynamicSection.Dynamic theDyn = theDynTable[j];

                //                BasicConsole.WriteLine("     - Dynamic : ");
                //                BasicConsole.Write("         - Tag : ");
                //                BasicConsole.WriteLine((int)theDyn.Tag);
                //                BasicConsole.Write("         - Value or Pointer : ");
                //                BasicConsole.WriteLine(theDyn.Val_Ptr);
                //            }
                //        }

                //        #endregion
                //    }

                //    Kernel.Processes.SystemCalls.SleepThread(500);
                //}

                #endregion
            }
            else
            {
                ExceptionMethods.Throw(new Exception("Failed to read sections table data from file!"));
            }
        }

        public void ReadSegmentHeaders()
        {
            byte[] segmentsData = new byte[header.ProgHeaderEntrySize*header.ProgHeaderNumEntries];
            theStream.Position = header.ProgHeaderTableOffset;
            int bytesRead = theStream.Read(segmentsData, 0, segmentsData.Length);

            if (bytesRead == segmentsData.Length)
            {
                uint offset = 0;
                while (offset < segmentsData.Length)
                {
                    ELFSegmentHeader newHeader = new ELFSegmentHeader(segmentsData, ref offset);
                    ELFSegment newSegment = ELFSegment.GetSegment(newHeader);

                    newSegment.Read(theStream);

                    Segments.Add(newSegment);
                }

                //BasicConsole.WriteLine();

                #region Segments Output

                //for (int i = 0; i < Segments.Count; i++)
                //{
                //    ELFSegment theSegment = (ELFSegment)Segments[i];
                //    ELFSegmentHeader theHeader = theSegment.Header;
                //    BasicConsole.WriteLine("ELF Segment: ");
                //    BasicConsole.Write(" - Type : ");
                //    BasicConsole.WriteLine((uint)theHeader.Type);
                //    BasicConsole.Write(" - File offset : ");
                //    BasicConsole.WriteLine(theHeader.FileOffset);
                //    BasicConsole.Write(" - Virtual address : ");
                //    BasicConsole.WriteLine((uint)theHeader.VAddr);
                //    BasicConsole.Write(" - Physical address : ");
                //    BasicConsole.WriteLine((uint)theHeader.PAddr);
                //    BasicConsole.Write(" - File size : ");
                //    BasicConsole.WriteLine(theHeader.FileSize);
                //    BasicConsole.Write(" - Memory size : ");
                //    BasicConsole.WriteLine(theHeader.MemSize);
                //    BasicConsole.Write(" - Flags : ");
                //    BasicConsole.WriteLine((uint)theHeader.Flags);
                //    BasicConsole.Write(" - Alignment : ");
                //    BasicConsole.WriteLine(theHeader.Align);

                //    Kernel.Processes.SystemCalls.SleepThread(500);
                //}

                #endregion
            }
            else
            {
                ExceptionMethods.Throw(new Exception("Failed to read segments table data from file!"));
            }
        }

        public bool IsValidFile()
        {
            #region CHECK : Signature

            if (!CheckSiganture())
            {
                Console.Default.WarningColour();
                Console.Default.WriteLine("ELF signature check failed!");
                Console.Default.DefaultColour();

                BasicConsole.WriteLine("ELF signature check failed!");
                return false;
            }
            Console.Default.Colour(0x2F);
            Console.Default.WriteLine("ELF signature check passed.");
            Console.Default.DefaultColour();

            BasicConsole.WriteLine("ELF signature check passed.");

            #endregion

            #region CHECK : File Class

            if (!CheckFileClass())
            {
                Console.Default.WarningColour();
                Console.Default.WriteLine("ELF file class check failed!");
                Console.Default.DefaultColour();

                BasicConsole.WriteLine("ELF file class check failed!");
                return false;
            }
            Console.Default.Colour(0x2F);
            Console.Default.WriteLine("ELF file class check passed.");
            Console.Default.DefaultColour();

            BasicConsole.WriteLine("ELF file class check passed.");

            #endregion

            #region CHECK : Data Encoding

            if (!CheckDataEncoding())
            {
                Console.Default.WarningColour();
                Console.Default.WriteLine("ELF data encoding check failed!");
                Console.Default.DefaultColour();

                BasicConsole.WriteLine("ELF data encoding check failed!");
                return false;
            }
            Console.Default.Colour(0x2F);
            Console.Default.WriteLine("ELF data encoding check passed.");
            Console.Default.DefaultColour();

            BasicConsole.WriteLine("ELF data encoding check passed.");

            #endregion

            #region CHECK : File Type

            if (!CheckFileType())
            {
                Console.Default.WarningColour();
                Console.Default.WriteLine("ELF file type check failed!");
                Console.Default.DefaultColour();

                BasicConsole.WriteLine("ELF file type check failed!");
                return false;
            }
            Console.Default.Colour(0x2F);
            Console.Default.WriteLine("ELF file type check passed.");
            Console.Default.DefaultColour();

            BasicConsole.WriteLine("ELF file type check passed.");

            #endregion

            #region CHECK : Machine

            if (!CheckMachine())
            {
                Console.Default.WarningColour();
                Console.Default.WriteLine("ELF machine check failed!");
                Console.Default.DefaultColour();

                BasicConsole.WriteLine("ELF machine check failed!");
                return false;
            }
            Console.Default.Colour(0x2F);
            Console.Default.WriteLine("ELF machine check passed.");
            Console.Default.DefaultColour();

            BasicConsole.WriteLine("ELF machine check passed.");

            #endregion

            #region INFO : Header Version

            Console.Default.Write("ELF Header version: ");
            Console.Default.WriteLine_AsDecimal(Header.HeaderVersion);

            BasicConsole.Write("ELF Header version: ");
            BasicConsole.WriteLine(Header.HeaderVersion);

            #endregion

            return true;
        }

        public bool CheckSiganture()
        {
            if (header != null)
            {
                return header.SignatureOK;
            }
            ExceptionMethods.Throw(new Exception("Failed to load ELF header so cannot check signature!"));
            return false;
        }

        public bool CheckFileClass()
        {
            if (header != null)
            {
                ELFFileClass fileClass = header.FileClass;
                //TODO: Support 64-bit executables
                return fileClass == ELFFileClass.None || fileClass == ELFFileClass.Class32;
            }
            ExceptionMethods.Throw(new Exception("Failed to load ELF header so cannot check file class!"));
            return false;
        }

        public bool CheckDataEncoding()
        {
            if (header != null)
            {
                return header.DataEncoding == ELFDataEncoding.LSB;
            }
            ExceptionMethods.Throw(new Exception("Failed to load ELF header so cannot check data encoding!"));
            return false;
        }

        public bool CheckFileType()
        {
            if (header != null)
            {
                ELFFileType fileType = header.FileType;
                return fileType == ELFFileType.None ||
                       fileType == ELFFileType.Executable ||
                       fileType == ELFFileType.Relocatable ||
                       fileType == ELFFileType.Shared;
            }
            ExceptionMethods.Throw(new Exception("Failed to load ELF header so cannot check file class!"));
            return false;
        }

        public bool CheckMachine()
        {
            if (header != null)
            {
                return header.Machine == ELFMachines.Intel80386;
            }
            ExceptionMethods.Throw(new Exception("Failed to load ELF header so cannot check file class!"));
            return false;
        }

        public bool IsExecutable()
        {
            if (header != null)
            {
                return header.FileType == ELFFileType.Executable;
            }
            ExceptionMethods.Throw(new Exception("Failed to load ELF header so cannot check file class!"));
            return false;
        }

        public bool IsSharedObject()
        {
            if (header != null)
            {
                return header.FileType == ELFFileType.Shared;
            }
            ExceptionMethods.Throw(new Exception("Failed to load ELF header so cannot check file class!"));
            return false;
        }

        public ELFProcess LoadExecutable(bool UserMode)
        {
            if (!IsExecutable())
            {
                ExceptionMethods.Throw(new Exception("Attempted to load non-executable ELF as executable!"));
            }

            ELFProcess process = new ELFProcess(this);
            process.Load(UserMode);
            return process;
        }

        public ELFSharedObject LoadSharedObject(ELFProcess theProcess)
        {
            if (!IsSharedObject())
            {
                ExceptionMethods.Throw(new Exception("Attempted to load non-shared ELF as shared object!"));
            }

            ELFSharedObject sharedObject = new ELFSharedObject(this, theProcess);
            sharedObject.Load();
            return sharedObject;
        }
    }
}