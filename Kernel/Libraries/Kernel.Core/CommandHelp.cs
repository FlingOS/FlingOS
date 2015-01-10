using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Core
{
    public class CommandDescription : FOS_System.Object
    {
        public String CommandName;
        public String CommandNameLower;
        public FOS_System.String Description;
    }

    public static class CommandHelp
    {
        public static FOS_System.Collections.List CommandDescriptions = new FOS_System.Collections.List();
        
        static CommandHelp()
        {
            CommandDescriptions.Add(new CommandDescription()
            {
                CommandName = "ExInfo",
                CommandNameLower = "exinfo",
                Description = "Provides you all the cached exception details."
            });

            CommandDescriptions.Add(new CommandDescription()
            {
                CommandName = "Halt",
                CommandNameLower = "halt",
                Description = "Shuts down the system."
            });

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
        }

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
            return "[No description found]";
        }
    }
}
