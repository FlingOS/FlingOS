using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Core
{
    /// <summary>
    /// Class to describe the command descriptions.
    /// </summary>
    public class CommandDescription : FOS_System.Object
    {
        public String CommandName;  // Name of the command
        public String CommandNameLower; // Command name in lower case
        public FOS_System.String Description; // Description of the command
    }

    /// <summary>
    /// Class that has all the command descriptions and related methods.
    /// </summary>
    public static class CommandHelp
    {
        public static FOS_System.Collections.List CommandDescriptions = new FOS_System.Collections.List();  // List of descriptions for the available commands
        
        /// <summary>
        /// CommandHelp constructor
        /// </summary>
        static CommandHelp()
        {
            #region ExInfo
            CommandDescriptions.Add(new CommandDescription()
            {
                CommandName = "ExInfo",
                CommandNameLower = "exinfo",
                Description = "Provides you all the cached exception details."
            });
            #endregion

            #region Halt
            CommandDescriptions.Add(new CommandDescription()
            {
                CommandName = "Halt",
                CommandNameLower = "halt",
                Description = "Shuts down the system."
            });
            #endregion

            #region Init
            CommandDescriptions.Add(new CommandDescription()
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
            CommandDescriptions.Add(new CommandDescription()
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
    Memory = > Output Heap memory and GC status.",
            });
            #endregion

            #region CheckDisk
            CommandDescriptions.Add(new CommandDescription()
            {
                CommandName = "CheckDisk",
                CommandNameLower = "checkdisk",
                Description = @"CheckDisk/ChkD  { Drive# }
Check disks passed in option for errors.
",
            });
            #endregion

            #region chkd
            CommandDescriptions.Add(new CommandDescription()
            {
                CommandName = "Chkd",
                CommandNameLower = "chkd",
                Description = @"CheckDisk/ChkD  { Drive# }
Alias for CheckDisk commmand.
",
            });
            #endregion

            #region FormatDisk
            CommandDescriptions.Add(new CommandDescription()
            {
                CommandName = "FormatDisk",
                CommandNameLower = "formatdisk",
                Description = @"FormatDisk/FmtD { Drive# }
Formats a disk passed in option in FAT 32 format.
",
            });
            #endregion

            #region Fmtd
            CommandDescriptions.Add(new CommandDescription()
            {
                CommandName = "Fmtd",
                CommandNameLower = "fmtd",
                Description = @"FormatDisk/FmtD { Drive# }
Alias for FormatDisk.
",
            });
            #endregion

            #region Dir
            CommandDescriptions.Add(new CommandDescription()
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
",
            });
            #endregion

            #region File
            CommandDescriptions.Add(new CommandDescription()
            {
                CommandName = "File",
                CommandNameLower = "file",
                Description = @"File { Open/Delete/Copy }
File Open <File> : Open a file  and outputs its content.
File Delete <File> : Delete a specified File.
File Copy <SrcFile> <DestFile>  : Copies specified source file <SrcFile> to destination.
",
            });
            #endregion

            #region GC
            CommandDescriptions.Add(new CommandDescription()
            {
                CommandName = "GC",
                CommandNameLower = "gc",
                Description = @"GC { [Cleanup] }
Calls garbage collection and performs memory clean up.
Cleanup: This argument is optional and doesn't modify the behaviour.
",
            });
            #endregion

            #region Clear
            CommandDescriptions.Add(new CommandDescription()
            {
                CommandName = "Clear",
                CommandNameLower = "clear",
                Description = @"Clear
Clears the command shell and displays the empty prompt.",
            });
            #endregion

            #region Show
            CommandDescriptions.Add(new CommandDescription()
            {
                CommandName = "Show",
                CommandNameLower = "show",
                Description = @"Show {w/c}
Show: Show the License information.
Show w: Show the license warnings.
Show c: Show the license conditions."
            });
            #endregion

           }
        


        /// <summary>
        /// Returns Description of the command passed to it
        /// </summary>
        /// <param name="command">Name of the command to find the description of</param>
        /// <returns>Description of the command (FOS_System.String)</returns>
        public static FOS_System.String GetCommandDescription(FOS_System.String command)
        {
            for (int i = 0; i < CommandDescriptions.Count; i++)
            {
                CommandDescription cmdDesc = (CommandDescription)CommandHelp.CommandDescriptions[i];
                
                if (cmdDesc.CommandNameLower == command)
                {
                    return cmdDesc.Description;
                }
            }
            return "[No command description found]";
        }
    }
}
