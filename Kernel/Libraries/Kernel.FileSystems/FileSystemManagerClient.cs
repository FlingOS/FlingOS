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
using Kernel.Framework.Collections;
using Kernel.Framework.Processes;
using Kernel.Pipes.File;
using Exception = System.Exception;

namespace Kernel.FileSystems
{
    public class FileSystemManagerClient : Object
    {
        public FileCmdInpoint CmdInPipe;
        public FileDataInpoint DataInPipe;
        public FileDataOutpoint DataOutPipe;
        public int DataOutPipeId;
        public uint ManagementThreadId;
        public uint RemoteProcessId;

        public bool Terminating = false;

        private List OwnedListings = new List(10);

        public unsafe void Manage()
        {
            while (!Terminating)
            {
                try
                {
                    FilePipeCommand* CommandPtr = CmdInPipe.Read();

                    switch ((FileCommands)CommandPtr->Command)
                    {
                        case FileCommands.StatFS:
                            StatFS();
                            break;
                        case FileCommands.ListDir:
                            ListDir();
                            break;
                    }
                }
                catch
                {
                    BasicConsole.WriteLine("File System Client > Error processing command!");
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                    SystemCalls.SleepThread(1000);
                }
            }
        }

        private void StatFS()
        {
            String FSPath = DataInPipe.ReadString(true);
            if (FSPath.Length > 0)
            {
                FileSystemMapping Mapping = FileSystemManager.GetMapping(FSPath);
                String[] Prefixes = new String[1];
                Prefixes[0] = Mapping.Prefix;
                DataOutPipe.WriteFSInfos(DataOutPipeId, Prefixes);
            }
            else
            {
                // Stat for all file systems
                FileSystemManager.LockMappingsList();
                String[] Prefixes = null;
                try
                {
                    Prefixes = new String[FileSystemManager.FileSystemMappings.Count];
                    for (int i = 0; i < Prefixes.Length; i++)
                    {
                        Prefixes[i] = ((FileSystemMapping)FileSystemManager.FileSystemMappings[i]).Prefix;
                    }
                }
                finally
                {
                    FileSystemManager.UnlockMappingsList();
                }
                
                DataOutPipe.WriteFSInfos(DataOutPipeId, Prefixes);
            }
        }

        private void ListDir()
        {
            String Path = DataInPipe.ReadString(true);
            Directory Listing = Directory.Find(Path);
            if (Listing != null)
            {
                if (Listing.Locked)
                {
                    DataOutPipe.WriteString(DataOutPipeId, "<LOCKED>\\");
                }
                else
                {
                    String Output = "";

                    Listing.Lock();

                    try
                    {
                        List Listings = Listing.GetListings();
                        for (int i = 0; i < Listings.Count; i++)
                        {
                            Base AListing = (Base)Listings[i];
                            if (AListing.Locked)
                            {
                                if (AListing.IsDirectory)
                                {
                                    Output += "<LOCKED>" + FileSystemManager.PathDelimiter;
                                }
                                else
                                {
                                    Output += "<LOCKED>";
                                }
                            }
                            else
                            {
                                AListing.Lock();
                                try
                                {
                                    if (AListing.IsDirectory)
                                    {
                                        Output += AListing.Name + FileSystemManager.PathDelimiter;
                                    }
                                    else
                                    {
                                        Output += AListing.Name;
                                    }
                                }
                                finally
                                {
                                    AListing.Unlock();
                                }
                            }

                            if (i < Listings.Count - 1)
                            {
                                Output += "\n";
                            }
                        }
                    }
                    finally
                    {
                        Listing.Unlock();
                    }

                    DataOutPipe.WriteString(DataOutPipeId, Output);
                }
            }
            else
            {
                DataOutPipe.WriteString(DataOutPipeId, "");
            }
        }
    }
}