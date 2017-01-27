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

#define FSM_TRACE

using Kernel.Devices.Controllers;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Exceptions;
using Kernel.Framework.Processes;
using Kernel.Framework.Processes.Requests.Pipes;
using Kernel.Pipes.File;
using Kernel.Utilities;

namespace Kernel.FileSystems
{
    /// <summary>
    ///     Provides management for file systems in the kernel.
    /// </summary>
    public static class FileSystemManager
    {
        /// <summary>
        ///     The delimiter that separates mapping prefixes and directory/file names in a path.
        /// </summary>
        public const char PathDelimiter = '/';

        private static int MappingsListSemaphoreId;

        /// <summary>
        ///     The list of file system mappings.
        /// </summary>
        public static List FileSystemMappings;

        private static List Clients;
        private static int ClientListSemaphoreId;
        private static uint WaitForClientsThreadId;
        private static FileDataOutpoint DataOutpoint;

        public static bool Terminating;

        /// <summary>
        ///     Initializes all available file systems by searching for
        ///     valid partitions on the available disk devices.
        /// </summary>
        public static void Init()
        {
            Terminating = false;

            FileSystemMappings = new List(3);

            Clients = new List(5);

            if (SystemCalls.CreateSemaphore(1, out MappingsListSemaphoreId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("File System Manager > Failed to create a semaphore! (1)");
                ExceptionMethods.Throw(new NullReferenceException());
            }
            
            if (SystemCalls.StartThread(WaitForClients, out WaitForClientsThreadId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("File System Manager > Failed to create client listener thread!");
            }
        }

        public static void InitFileSystem(Partition aPartition, FileSystem newFS)
        {
            //TODO: Don't use the count to generate the drive letter
            //  Because if we allowed detach/removal/eject in future, it could cause naming conflicts
            String mappingPrefix = String.New(3);
            mappingPrefix[0] = (char)('A' + FileSystemMappings.Count);
            mappingPrefix[1] = ':';
            mappingPrefix[2] = PathDelimiter;
            newFS.TheMapping = new FileSystemMapping(mappingPrefix, newFS);
            aPartition.Mapped = true;

            FileSystemMappings.Add(newFS.TheMapping);

            //TODO
            BasicConsole.WriteLine("Calling InitFS system call...");
            SystemCalls.InitFS();
        }

        /// <summary>
        ///     Gets the file system mapping for the specified path.
        /// </summary>
        /// <param name="aPath">The path to get the mapping for.</param>
        /// <returns>The file system mapping or null if none exists.</returns>
        public static FileSystemMapping GetMapping(String aPath)
        {
            FileSystemMapping result = null;

            LockMappingsList();
            try
            {
                for (int i = 0; i < FileSystemMappings.Count; i++)
                {
                    FileSystemMapping aMapping = (FileSystemMapping)FileSystemMappings[i];
                    if (aMapping.PathMatchesMapping(aPath))
                    {
                        result = aMapping;
                        break;
                    }
                }
            }
            finally
            {
                UnlockMappingsList();
            }

            return result;
        }

        /// <summary>
        ///     Determines whether the specified partition has any file system mappings associated with it.
        /// </summary>
        /// <param name="part">The partition to check.</param>
        /// <returns>Whether there are any file system mappings for the partition.</returns>
        public static bool HasMapping(Partition part)
        {
            for (int i = 0; i < FileSystemMappings.Count; i++)
            {
                FileSystemMapping mapping = (FileSystemMapping)FileSystemMappings[i];
                if (mapping.TheFileSystem.ThePartition == part)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool LockMappingsList()
        {
            return SystemCalls.WaitSemaphore(MappingsListSemaphoreId) == SystemCallResults.OK;
        }

        public static bool UnlockMappingsList()
        {
            return SystemCalls.SignalSemaphore(MappingsListSemaphoreId) == SystemCallResults.OK;
        }

        private static unsafe void WaitForClients()
        {
            DataOutpoint = new FileDataOutpoint(PipeConstants.UnlimitedConnections, true);

            while (!Terminating)
            {
                uint InProcessId;
                int DataOutPipeId = DataOutpoint.WaitForConnect(out InProcessId);
                BasicConsole.WriteLine("File System Manager > File data output connected.");

                FileCmdInpoint CmdInPipe = new FileCmdInpoint(InProcessId);
                FileDataInpoint DataInPipe = new FileDataInpoint(InProcessId, false);

                FileSystemManagerClient NewInfo = new FileSystemManagerClient
                {
                    CmdInPipe = CmdInPipe,
                    DataInPipe = DataInPipe,
                    DataOutPipe = DataOutpoint,
                    DataOutPipeId = DataOutPipeId,
                    RemoteProcessId = InProcessId
                };
                
                Clients.Add(NewInfo);

                uint NewThreadId;
                if (SystemCalls.StartThread(ObjectUtilities.GetHandle((ManageClientDelegate)ManageClient), out NewThreadId, new uint[] { (uint)ObjectUtilities.GetHandle(NewInfo) }) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("File System Manager > Failed to create client management thread!");
                }

                NewInfo.ManagementThreadId = NewThreadId;
            }
        }

        private delegate void ManageClientDelegate(FileSystemManagerClient TheClient, uint CS);
        private static void ManageClient(FileSystemManagerClient TheClient, uint CS)
        {
            BasicConsole.WriteLine("File System Manager > Client manager started.");

            TheClient.Manage();
        }
    }
}