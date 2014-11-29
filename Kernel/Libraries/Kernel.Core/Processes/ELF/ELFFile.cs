using System;
using Kernel.FOS_System;
using Kernel.FOS_System.IO;
using Kernel.FOS_System.IO.Streams;
using Kernel.FOS_System.Collections;

namespace Kernel.Core.Processes.ELF
{
    public unsafe class ELFFile : FOS_System.Object
    {
        private File theFile;
        private FileStream theStream;
        public File TheFile
        {
            get
            {
                return theFile;
            }
        }
        public FileStream TheStream
        {
            get
            {
                return theStream;
            }
        }

        private ELFHeader header;
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

        public List Sections = new List();
        public List Segments = new List();
        public ELFStringTableSection NamesTable;

        public ELFFile(File file)
        {
            theFile = file;
            theStream = theFile.GetStream();
            ReadHeader();

            if (IsValidFile())
            {
                ReadSectionHeaders();
                ReadSegmentHeaders();
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
                ExceptionMethods.Throw(new FOS_System.Exception("Failed to read header data from file!"));
            }
        }
        public void ReadSectionHeaders()
        {
            byte[] sectionsData = new byte[header.SecHeaderEntrySize * header.SecHeaderNumEntries];
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
                            ExceptionMethods.Throw(new FOS_System.Exception("Expected Strings Table section was not a strings table section!"));
                        }
                        NamesTable = (ELFStringTableSection)newSection;
                    }

                    newSection.Read(theStream);

                    Sections.Add(newSection);
                }

                Console.Default.WriteLine();

                #region Sections Output

                for (int i = 0; i < Sections.Count; i++)
                {
                    ELFSection theSection = (ELFSection)Sections[i];
                    ELFSectionHeader theHeader = theSection.Header;
                    Console.Default.WriteLine("ELF section: ");
                    Console.Default.Write(" - Name index : ");
                    Console.Default.WriteLine_AsDecimal(theHeader.NameIndex);
                    Console.Default.Write(" - Name : ");
                    Console.Default.WriteLine(NamesTable[theHeader.NameIndex]);
                    Console.Default.Write(" - Type : ");
                    Console.Default.WriteLine_AsDecimal((uint)theHeader.SectionType);
                    Console.Default.Write(" - Flags : ");
                    Console.Default.WriteLine_AsDecimal((uint)theHeader.Flags);
                    Console.Default.Write(" - Offset : ");
                    Console.Default.WriteLine_AsDecimal(theHeader.SectionFileOffset);
                    Console.Default.Write(" - Size : ");
                    Console.Default.WriteLine_AsDecimal(theHeader.SectionSize);
                    Console.Default.Write(" - Load address : ");
                    Console.Default.WriteLine_AsDecimal((uint)theHeader.LoadAddress);

                    if (theSection is ELFSymbolTableSection)
                    {
                        #region ELFSymbolTableSection

                        Console.Default.WriteLine(" - Symbol table :");

                        ELFSymbolTableSection theSymTable = (ELFSymbolTableSection)theSection;
                        ELFStringTableSection theStringTable = (ELFStringTableSection)(Sections[theSymTable.StringsSectionIndex]);
                        for (uint j = 0; j < theSymTable.Symbols.Count; j++)
                        {
                            ELFSymbolTableSection.Symbol theSym = theSymTable[j];

                            Console.Default.Write("     - Symbol : ");
                            Console.Default.WriteLine(theStringTable[theSym.NameIdx]);
                            Console.Default.Write("         - Type : ");
                            Console.Default.WriteLine_AsDecimal((uint)theSym.Type);
                            Console.Default.Write("         - Binding : ");
                            Console.Default.WriteLine_AsDecimal((uint)theSym.Binding);
                            Console.Default.Write("         - Section index : ");
                            Console.Default.WriteLine_AsDecimal(theSym.SectionIndex);
                            Console.Default.Write("         - Value : ");
                            Console.Default.WriteLine_AsDecimal((uint)theSym.Value);
                            Console.Default.Write("         - Size : ");
                            Console.Default.WriteLine_AsDecimal(theSym.Size);
                        }

                        #endregion
                    }
                    else if (theSection is ELFRelocationAddendTableSection)
                    {
                        #region ELFRelocationAddendTableSection

                        ELFRelocationAddendTableSection theRelASection = (ELFRelocationAddendTableSection)theSection;

                        Console.Default.WriteLine(" - Relocation (with addends) table :");
                        Console.Default.Write("     - Symbol table index : ");
                        Console.Default.WriteLine_AsDecimal(theRelASection.SymbolTableSectionIndex);
                        Console.Default.Write("     - Section to relocate index : ");
                        Console.Default.WriteLine_AsDecimal(theRelASection.SectionToRelocateIndex);

                        for (uint j = 0; j < theRelASection.Relocations.Count; j++)
                        {
                            ELFRelocationAddendTableSection.RelocationAddend theRel = theRelASection[j];

                            Console.Default.WriteLine("     - Relocation : ");
                            Console.Default.Write("         - Type : ");
                            Console.Default.WriteLine_AsDecimal(theRel.Type);
                            Console.Default.Write("         - Symbol : ");
                            Console.Default.WriteLine_AsDecimal(theRel.Symbol);
                            Console.Default.Write("         - Offset : ");
                            Console.Default.WriteLine_AsDecimal((uint)theRel.Offset);
                            Console.Default.Write("         - Addend : ");
                            Console.Default.WriteLine_AsDecimal(theRel.Addend);
                        }

                        #endregion
                    }
                    else if (theSection is ELFRelocationTableSection)
                    {
                        #region ELFRelocationTableSection

                        ELFRelocationTableSection theRelSection = (ELFRelocationTableSection)theSection;

                        Console.Default.WriteLine(" - Relocation table :");
                        Console.Default.Write("     - Symbol table index : ");
                        Console.Default.WriteLine_AsDecimal(theRelSection.SymbolTableSectionIndex);
                        Console.Default.Write("     - Section to relocate index : ");
                        Console.Default.WriteLine_AsDecimal(theRelSection.SectionToRelocateIndex);

                        for (uint j = 0; j < theRelSection.Relocations.Count; j++)
                        {
                            ELFRelocationTableSection.Relocation theRel = theRelSection[j];

                            Console.Default.WriteLine("     - Relocation : ");
                            Console.Default.Write("         - Type : ");
                            Console.Default.WriteLine_AsDecimal(theRel.Type);
                            Console.Default.Write("         - Symbol : ");
                            Console.Default.WriteLine_AsDecimal(theRel.Symbol);
                            Console.Default.Write("         - Offset : ");
                            Console.Default.WriteLine_AsDecimal((uint)theRel.Offset);
                        }

                        #endregion
                    }
                }

                #endregion
            }
            else
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Failed to read sections table data from file!"));
            }
        }
        public void ReadSegmentHeaders()
        {
            byte[] segmentsData = new byte[header.ProgHeaderEntrySize * header.ProgHeaderNumEntries];
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
                
                Console.Default.WriteLine();

                #region Segments Output

                for (int i = 0; i < Segments.Count; i++)
                {
                    ELFSegment theSegment = (ELFSegment)Segments[i];
                    ELFSegmentHeader theHeader = theSegment.Header;
                    Console.Default.WriteLine("ELF Segment: ");
                    Console.Default.Write(" - Type : ");
                    Console.Default.WriteLine_AsDecimal((uint)theHeader.Type);
                    Console.Default.Write(" - File offset : ");
                    Console.Default.WriteLine_AsDecimal(theHeader.FileOffset);
                    Console.Default.Write(" - Virtual address : ");
                    Console.Default.WriteLine_AsDecimal((uint)theHeader.VAddr);
                    Console.Default.Write(" - Physical address : ");
                    Console.Default.WriteLine_AsDecimal((uint)theHeader.PAddr);
                    Console.Default.Write(" - File size : ");
                    Console.Default.WriteLine_AsDecimal(theHeader.FileSize);
                    Console.Default.Write(" - Memory size : ");
                    Console.Default.WriteLine_AsDecimal(theHeader.MemSize);
                    Console.Default.Write(" - Flags : ");
                    Console.Default.WriteLine_AsDecimal((uint)theHeader.Flags);
                    Console.Default.Write(" - Alignment : ");
                    Console.Default.WriteLine_AsDecimal(theHeader.Align);
                }

                #endregion
            }
            else
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Failed to read segments table data from file!"));
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
                return false;
            }
            else
            {
                Console.Default.Colour(0x2F);
                Console.Default.WriteLine("ELF signature check passed.");
                Console.Default.DefaultColour();
            }
            #endregion
            #region CHECK : File Class
            if (!CheckFileClass())
            {
                Console.Default.WarningColour();
                Console.Default.WriteLine("ELF file class check failed!");
                Console.Default.DefaultColour();
                return false;
            }
            else
            {
                Console.Default.Colour(0x2F);
                Console.Default.WriteLine("ELF file class check passed.");
                Console.Default.DefaultColour();
            }
            #endregion
            #region CHECK : Data Encoding
            if (!CheckDataEncoding())
            {
                Console.Default.WarningColour();
                Console.Default.WriteLine("ELF data encoding check failed!");
                Console.Default.DefaultColour();
                return false;
            }
            else
            {
                Console.Default.Colour(0x2F);
                Console.Default.WriteLine("ELF data encoding check passed.");
                Console.Default.DefaultColour();
            }
            #endregion
            #region CHECK : File Type
            if (!CheckFileType())
            {
                Console.Default.WarningColour();
                Console.Default.WriteLine("ELF file type check failed!");
                Console.Default.DefaultColour();
                return false;
            }
            else
            {
                Console.Default.Colour(0x2F);
                Console.Default.WriteLine("ELF file type check passed.");
                Console.Default.DefaultColour();
            }
            #endregion
            #region CHECK : Machine
            if (!CheckMachine())
            {
                Console.Default.WarningColour();
                Console.Default.WriteLine("ELF machine check failed!");
                Console.Default.DefaultColour();
                return false;
            }
            else
            {
                Console.Default.Colour(0x2F);
                Console.Default.WriteLine("ELF machine check passed.");
                Console.Default.DefaultColour();
            }
            #endregion
            #region INFO : Header Version
            Console.Default.Write("ELF Header version: ");
            Console.Default.WriteLine_AsDecimal(Header.HeaderVersion);
            #endregion

            return true;
        }
        public bool CheckSiganture()
        {
            if (header != null)
            {
                return header.SignatureOK;
            }
            else
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Failed to load ELF header so cannot check signature!"));
            }
            return false;
        }
        public bool CheckFileClass()
        {
            if (header != null)
            {
                ELFFileClass fileClass = header.FileClass;
                //TODO - Support 64-bit executables
                return fileClass == ELFFileClass.None || fileClass == ELFFileClass.Class32;
            }
            else
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Failed to load ELF header so cannot check file class!"));
            }
            return false;
        }
        public bool CheckDataEncoding()
        {
            if (header != null)
            {
                return header.DataEncoding == ELFDataEncoding.LSB;
            }
            else
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Failed to load ELF header so cannot check data encoding!"));
            }
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
            else
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Failed to load ELF header so cannot check file class!"));
            }
            return false;
        }
        public bool CheckMachine()
        {
            if (header != null)
            {
                return header.Machine == ELFMachines.Intel80386;
            }
            else
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Failed to load ELF header so cannot check file class!"));
            }
            return false;
        }

        public bool IsExecutable()
        {
            if (header != null)
            {
                return header.FileType == ELFFileType.Executable;
            }
            else
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Failed to load ELF header so cannot check file class!"));
            }
            return false;
        }

        public ELFProcess LoadExecutable(bool UserMode)
        {
            if (!IsExecutable())
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Attempted to load non-executable ELF as executable!"));
            }

            ELFProcess process = new ELFProcess(this);
            process.Load(UserMode);
            return process;
        }
    }
}
