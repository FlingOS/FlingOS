using System;
using Kernel.FOS_System;
using Kernel.FOS_System.IO;
using Kernel.FOS_System.IO.Streams;
using Kernel.FOS_System.Collections;
using Kernel.Hardware.Processes;

namespace Kernel.Core.Processes.ELF
{
    public class ELFSharedObject : FOS_System.Object
    {
        protected ELFFile theFile = null;
        public ELFFile TheFile
        {
            get
            {
                return theFile;
            }
        }

        protected ELFProcess theProcess;
        public ELFProcess TheProcess
        {
            get
            {
                return theProcess;
            }
        }

        public ELFSharedObject(ELFFile anELFFile, ELFProcess aProcess)
        {
            theFile = anELFFile;
            theProcess = aProcess;
        }

        public void Load()
        {
            // - Test to make sure the shared object hasn't already been loaded
            // - Add shared object file path to processes' list of shared object file paths
            //
            //          SharedObjectDependencyFilePaths.Add(sharedObjectFile.GetFullPath());
            //
            // - Read in segments / map in memory
            // - Perform relocation
            // - Perform dynamic linking
            // - Execute Init method(s)

            FOS_System.String fullFilePath = theFile.TheFile.GetFullPath();
            if (theProcess.SharedObjectDependencyFilePaths.IndexOf(fullFilePath) > -1)
            {
                return;
            }

            theProcess.SharedObjectDependencyFilePaths.Add(fullFilePath);

            bool OK = true;
            bool DynamicLinkingRequired = false;

            // Load the ELF segments (i.e. the library code and data)
            uint memBaseAddress = theFile.BaseAddress;
            theProcess.LoadSegments(theFile, ref OK, ref DynamicLinkingRequired, memBaseAddress);

            // Perform relocation

            // Perform dynamic linking


            // Execute Init methods
        }
    }
}
