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
        }

        public void ReadHeader()
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
            if (header == null)
            {
                ReadHeader();
            }

            if (header != null)
            {
                bool OK = header.ident[0] == 0x7F;
                OK = OK && header.ident[1] == 'E';
                OK = OK && header.ident[2] == 'L';
                OK = OK && header.ident[3] == 'F';
                return OK;
            }
            else
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Failed to load ELF header so cannot check signature!"));
            }
            return false;
        }
    }
}
