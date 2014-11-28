using System;
using Kernel.FOS_System;
using Kernel.FOS_System.IO;
using Kernel.FOS_System.IO.Streams;

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

        public ELFFile(File file)
        {
            theFile = file;
            theStream = theFile.GetStream();
            ReadHeader();
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
                ExceptionMethods.Throw(new FOS_System.Exception("Failed to read data from file!"));
            }
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
        

    }
}
