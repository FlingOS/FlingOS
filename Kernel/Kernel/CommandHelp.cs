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

namespace Kernel
{
    /// <summary>
    ///     Class to describe the command descriptions.
    /// </summary>
    public class CommandDescription : Object
    {
        /// <summary>
        ///     Name of the command
        /// </summary>
        public string CommandName;

        /// <summary>
        ///     Command name in lower case
        /// </summary>
        public string CommandNameLower;

        /// <summary>
        ///     Description of the command
        /// </summary>
        public String Description;
    }

    /// <summary>
    ///     Class that has all the command descriptions and related methods.
    /// </summary>
    public static class CommandHelp
    {
        /// <summary>
        ///     List of descriptions for the available commands.
        /// </summary>
        public static List CommandDescriptions = new List();

        /// <summary>
        ///     CommandHelp constructor
        /// </summary>
        static CommandHelp()
        {
            #region ExInfo

            CommandDescriptions.Add(new CommandDescription
            {
                CommandName = "ExInfo",
                CommandNameLower = "exinfo",
                Description = "Provides you all the cached exception details."
            });

            #endregion

            #region Halt

            CommandDescriptions.Add(new CommandDescription
            {
                CommandName = "Halt",
                CommandNameLower = "halt",
                Description = "Shuts down the system."
            });

            #endregion

            #region Init

            CommandDescriptions.Add(new CommandDescription
            {
                CommandName = "Init",
                CommandNameLower = "init",
                Description = @"Init { ALL / PCI / ATA / USB / FS }
To initialize an individual sub-system or group of sub-systems, call 'init <option>'.

Possible options: 
    All => (group) Initializes all the sub-systems in the correct order. 
    PCI => Initialize PCI devices.
    ATA => Initialize ATA/PATA/SATA devices.
    USB => Initialize USB sub-system (EHCI, USB and USB MSDs). Dependent upon PCI sub-system. PCI must be initialised first.
    FS  => Initialize partitions and file systems. Dependent upon ATA or USB sub-systems. One or more must be initialised prior to calling this."
            });

            #endregion

            #region Output

            CommandDescriptions.Add(new CommandDescription
            {
                CommandName = "Output",
                CommandNameLower = "output",
                Description = @"Output { PCI / ATA / USB / FS / Memory }
Displays the status of an individual sub-system. To get the status of an individual sub-system, call 'output <option>'.

Possible options:
    PCI => Output information about PCI devices,
    ATA => Output information about ATA/PATA/SATA device,
    USB => Output information about USB interfaces and USB devices,
    FS  => Output partition and file system statuses,
    Memory = > Output Heap memory and GC status."
            });

            #endregion

            #region CheckDisk

            CommandDescriptions.Add(new CommandDescription
            {
                CommandName = "CheckDisk",
                CommandNameLower = "checkdisk",
                Description = @"CheckDisk/ChkD  { Drive# }
Check disks passed in option for errors.
"
            });

            #endregion

            #region chkd

            CommandDescriptions.Add(new CommandDescription
            {
                CommandName = "Chkd",
                CommandNameLower = "chkd",
                Description = @"CheckDisk/ChkD  { Drive# }
Alias for CheckDisk commmand.
"
            });

            #endregion

            #region FormatDisk

            CommandDescriptions.Add(new CommandDescription
            {
                CommandName = "FormatDisk",
                CommandNameLower = "formatdisk",
                Description = @"FormatDisk/FmtD { Drive# }
Formats a disk passed in option in FAT 32 format.
"
            });

            #endregion

            #region Fmtd

            CommandDescriptions.Add(new CommandDescription
            {
                CommandName = "Fmtd",
                CommandNameLower = "fmtd",
                Description = @"FormatDisk/FmtD { Drive# }
Alias for FormatDisk.
"
            });

            #endregion

            #region Dir

            CommandDescriptions.Add(new CommandDescription
            {
                CommandName = "Dir",
                CommandNameLower = "dir",
                Description = @"Dir  { List / Open / New / Delete / Copy }
Dir List <Path> : List files/directories in the path specified.
Dir Open <Path> : Open specified directory.
Dir New <Path>  : Create a new directory.
Dir Delete <Path>: Delete specified directory.
Dir Copy <SrcPath> <DestPath> : Copy specified directory in SrcPath to DestPath.
On changing directory, it will set ./ to current directory.
To refer current directory, you can use ./ and for the parent directory, use ../
"
            });

            #endregion

            #region File

            CommandDescriptions.Add(new CommandDescription
            {
                CommandName = "File",
                CommandNameLower = "file",
                Description = @"File { Open/Delete/Copy }
File Open <File> : Open a file  and outputs its content.
File Delete <File> : Delete a specified File.
File Copy <SrcFile> <DestFile>  : Copies specified source file <SrcFile> to destination.
"
            });

            #endregion

            #region GC

            CommandDescriptions.Add(new CommandDescription
            {
                CommandName = "GC",
                CommandNameLower = "gc",
                Description = @"GC { [Cleanup] }
Calls garbage collection and performs memory clean up.
Cleanup: This argument is optional and doesn't modify the behaviour.
"
            });

            #endregion

            #region Clear

            CommandDescriptions.Add(new CommandDescription
            {
                CommandName = "Clear",
                CommandNameLower = "clear",
                Description = @"Clear
Clears the command shell and displays the empty prompt."
            });

            #endregion

            #region Show

            CommandDescriptions.Add(new CommandDescription
            {
                CommandName = "Show",
                CommandNameLower = "show",
                Description = @"Show {w/c}
Show: Show the License information.
Show w: Show the license warnings.
Show c: Show the license conditions."
            });

            #endregion

            #region Reboot

            CommandDescriptions.Add(new CommandDescription
            {
                CommandName = "Reboot",
                CommandNameLower = "reboot",
                Description = @"Reboot
Safely reboots the computer."
            });

            #endregion
        }

        /// <summary>
        ///     Returns Description of the command passed to it
        /// </summary>
        /// <param name="command">Name of the command to find the description of</param>
        /// <returns>Description of the command (Framework.String)</returns>
        public static String GetCommandDescription(String command)
        {
            for (int i = 0; i < CommandDescriptions.Count; i++)
            {
                CommandDescription cmdDesc = (CommandDescription)CommandDescriptions[i];

                if (cmdDesc.CommandNameLower == command)
                {
                    return cmdDesc.Description;
                }
            }
            return "[No command description found]";
        }
    }
}