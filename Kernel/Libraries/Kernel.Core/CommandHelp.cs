using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Core
{
    public class CommandDescription : FOS_System.Object
    {
        public String CommandName
        { get; set; }
        public FOS_System.String Description
        { get; set; }
    }

    public static class CommandHelp
    {
        public static FOS_System.Collections.List CommandDescriptions = new FOS_System.Collections.List();
        
        static CommandHelp()
        {
            
            CommandDescriptions.Add(new CommandDescription()
            {
                CommandName = "ExInfo",
                Description = "Provides you all the cached exception details"
            });

            CommandDescriptions.Add(new CommandDescription()
            {
                CommandName = "Halt",
                Description = "Shutdowns the system"
            });

            CommandDescriptions.Add(new CommandDescription()
            {
                CommandName = "Init",
                Description = @"Init { ALL / PCI / ATA / USB / FS }
 Init All - Initialize all the sub-systems altogether
 To initialize individual sub-system, call 'init <option>'
 Possible options: 
 PCI => Initialize PCI devices
 ATA => Initialize ATA/PATA/SATA devices
 USB => Initialize USB interfaces
 FS  => Initialize File system"
            });

            CommandDescriptions.Add(new CommandDescription()
            {
                CommandName = "Output",
                Description = @"Output { PCI / ATA / USB / FS / Memory }
 Displays the status of the devices after initialization
 Usage: Output <option>
 To get status of individual sub-system, substitute '<option>' with possible options below

 PCI => Output PCI devices
 ATA => Output ATA/PATA/SATA devices
 USB => Output USB interfaces
 FS  => Output File system
 Memory = > Output Memory status",
            });
        }

        public static FOS_System.String GetCommandDescription(String command)
        {
            for(int i=0; i<CommandDescriptions.Count; i++)
            {
                var cmdDesc = (CommandDescription)CommandHelp.CommandDescriptions[i];
                if (cmdDesc.CommandName == command)
                {
                    return cmdDesc.Description;
                }
            }
            return "";
        }
    }
}
