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

using Kernel.Framework;

namespace Kernel.Processes.ELF
{
    public class ELFSharedObject : Object
    {
        public uint BaseAddress = 0;
        protected ELFFile theFile;

        protected ELFProcess theProcess;

        public ELFFile TheFile
        {
            get { return theFile; }
        }

        public ELFProcess TheProcess
        {
            get { return theProcess; }
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

            String fullFilePath = theFile.TheFile.GetFullPath();
            if (theProcess.SharedObjectDependencyFilePaths.IndexOf(fullFilePath) > -1)
            {
                return;
            }

            theProcess.SharedObjectDependencyFilePaths.Add(fullFilePath);

            bool OK = true;
            bool DynamicLinkingRequired = false;

            // Load the ELF segments (i.e. the library code and data)
            //TODO: Use system calls
            //BaseAddress = Hardware.VirtMemManager.FindFreeVirtPage();
            //theProcess.LoadSegments(theFile, ref OK, ref DynamicLinkingRequired, BaseAddress);
        }
    }
}