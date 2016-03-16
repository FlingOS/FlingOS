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
    
//#define PERIODIC_REBOOT
   
using System;
using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.IO;
using Kernel.Hardware.Devices;

namespace Kernel.Shells
{
    /// <summary>
    /// Implementation of the main shell for the core kernel.
    /// </summary>
    public class MainShell : Shell
    {
        /// <summary>
        /// The current directory to prepend to relative paths.
        /// </summary>
        protected FOS_System.String CurrentDir = "";
        
        public MainShell()
            : base()
        {
        }
        public MainShell(Console AConsole, Keyboard AKeyboard)
            : base(AConsole, AKeyboard)
        {
        }
        
        /// <summary>
        /// See base class.
        /// </summary>
        public override void Execute()
        {
            try
            {
                Hardware.DeviceManager.AddDeviceAddedListener(MainShell.DeviceManager_DeviceAdded, this);

                try
                {
                    // Auto-init all to save us writing the command
                    //InitPCI();
                }
                catch
                {
                    console.WriteLine();
                    console.WarningColour();
                    console.WriteLine("Error initialising PCI subsystem:");
                    OutputExceptionInfo(ExceptionMethods.CurrentException);
                }
                try
                {
                    // Auto-init all to save us writing the command
                    //InitATA();
                }
                catch
                {
                    console.WriteLine();
                    console.WarningColour();
                    console.WriteLine("Error initialising ATA subsystem:");
                    OutputExceptionInfo(ExceptionMethods.CurrentException);
                }

#if PERIODIC_REBOOT
                // 60 seconds
                Hardware.Timers.PIT.Default.RegisterHandler(TriggerPeriodicReboot, 60000000000L, true, this);
#endif

                //Endlessly wait for commands until we hit a total failure condition
                //  or the user instructs us to halt
                while (!terminating)
                {
                    try
                    {
                        //Output the current command line
                        console.Write(CurrentDir + " > ");

                        //List of supported commands
                        /* Command { Req Arg } [Opt Arg] *Default val*:
                         *  - Halt
                         *  - ExInfo
                         *  - Init { ALL / PCI / ATA / USB / FS }
                         *  - Output { PCI / ATA / USB / FS / Memory }
                         *  - CheckDisk/ChkD  { Drive# }
                         *  - FormatDisk/FmtD { Drive# }
                         *  - Dir  { List / Open / New / Delete / Copy }
                         *  - File { Open/Delete/Copy }
                         *  - Test {    Interrupts  /   Delegates   /   FileSystems /
                         *              ULLTComp    /   StringConcat/   ObjArray    /
                         *              IntArray    /   DummyObj    /   DivideBy0   /
                         *              Exceptions1 /   Exceptions2 /   PCBeep      /
                         *              Timer       /   Keyboard    /   FieldsTable /
                         *              IsInst      /   VirtMem     /   Longs       /
                         *              ThreadSleep /   Heap        /   GC          /
                         *              ATA         /   USB                             }
                         *  - GC   { Cleanup }
                         *  - USB { Update / Eject }
                         *  - Start { Filename } [*KM* / UM] [*Raw* / ELF]
                         *  - ILY
                         *  - Show { c / w }
                         *  - Help { <Command Name> }
                         *  - Clear
                         *  - Easter
                         *  - Reboot
                         */

                        //Get the current input line from the user
                        FOS_System.String line = ReadLine();
                        //Split the input into command, arguments and options
                        //  All parts are in lower case
                        List cmdParts = SplitCommand(line);
                        //Check the user didn't just press enter without any text
                        if (cmdParts.Count > 0)
                        {
                            //Get the command to run - first part of the command
                            FOS_System.String cmd = (FOS_System.String)cmdParts[0];
                            //Determine which command we are to run
                            if (cmd == "halt")
                            {
                                //Cleanup devices
                                console.WriteLine("Ejecting MSDs...");
                                CleanDiskCaches();
                                console.WriteLine("Ejected.");

                                console.WriteLine("Closing...");
                                //Halt execution of the current shell
                                terminating = true;
                            }
                            else if (cmd == "exinfo")
                            {
                                //Output information about the current exception, if any.
                                //  TODO - This should be changed to Last exception. 
                                //          Because of the try-catch block inside the loop, there
                                //          will never be a current exception to output.
                                OutputExceptionInfo(ExceptionMethods.CurrentException);
                            }
                            else if (cmd == "init")
                            {
                                //Initialise the specified sub-system.

                                #region Init
                                //The user may have forgotten to input an option. Assume they
                                //  haven't, then fill in if they have.
                                FOS_System.String opt1 = null;
                                //We don't know how many extra options there might be, so we test 
                                //  for greater-than instead of equal to. It should be noted that >
                                //  is more efficient than >=. Also, the command is in the cmdParts 
                                //  not just the options. 
                                //So, we want the 1st option, which is the 2nd command part. This 
                                //  means we need > 1 command part and index 1 in the command parts 
                                //  list.
                                if (cmdParts.Count > 1)
                                {
                                    opt1 = (FOS_System.String)cmdParts[1];
                                }

                                //If the user gave us an option
                                if (opt1 != null)
                                {
                                    //Determine which option that was
                                    if (opt1 == "all")
                                    {
                                        //Initialise all sub-systems in order
                                        InitATA();
                                        InitPCI();
                                        InitUSB();
                                        InitFS();
                                    }
                                    else if (opt1 == "pci")
                                    {
                                        //Initialise the PCI sub-system
                                        InitPCI();
                                    }
                                    else if (opt1 == "ata")
                                    {
                                        //Initialise the ATA sub-system
                                        InitATA();
                                    }
                                    else if (opt1 == "usb")
                                    {
                                        //Initialise the USB sub-system
                                        //  This is dependent upon the PCI sub-system
                                        //  but we assume the user was intelligent 
                                        //  enough to have already initialised PCI. 
                                        //  (Probably a bad assumption really... ;p )
                                        InitUSB();
                                    }
                                    else if (opt1 == "fs")
                                    {
                                        //Initialise the file (sub-)system
                                        //  This is dependent upon the USB or ATA 
                                        //  sub-system but we assume the user was intelligent 
                                        //  enough to have already initialised these. 
                                        //  (Probably a bad assumption really... ;p )
                                        InitFS();
                                    }
                                    else
                                    {
                                        UnrecognisedOption(opt1);
                                    }
                                }
                                else
                                {
                                    console.WriteLine("You must specify what to init. { PCI/ATA/USB/FS }");
                                }
                                #endregion
                            }
                            else if (cmd == "output")
                            {
                                //For details on how the code here works, see Init
                                #region Output
                                FOS_System.String opt1 = null;
                                if (cmdParts.Count > 1)
                                {
                                    opt1 = (FOS_System.String)cmdParts[1];
                                }

                                if (opt1 != null)
                                {
                                    if (opt1 == "pci")
                                    {
                                        OutputPCI();
                                    }
                                    else if (opt1 == "ata")
                                    {
                                        OutputATA();
                                    }
                                    else if (opt1 == "usb")
                                    {
                                        OutputUSB();
                                    }
                                    else if (opt1 == "fs")
                                    {
                                        OutputFS();
                                    }
                                    else if (opt1 == "memory" || opt1 == "mem")
                                    {
                                        OutputMemory();
                                    }
                                    else
                                    {
                                        UnrecognisedOption(opt1);
                                    }
                                }
                                else
                                {
                                    console.WriteLine("You must specify what to output. { PCI/ATA/USB/FS }");
                                }
                                #endregion
                            }
                            else if (cmd == "checkdisk" || cmd == "chkd")
                            {
                                //For details on how the code here works, see Init
                                #region Check Disk

                                FOS_System.String opt1 = null;
                                if (cmdParts.Count > 1)
                                {
                                    opt1 = (FOS_System.String)cmdParts[1];
                                }

                                if (opt1 != null)
                                {
                                    int diskNum = (int)FOS_System.Int32.Parse_DecimalUnsigned(opt1, 0);

                                    if (Hardware.DeviceManager.Devices[diskNum] is Hardware.Devices.DiskDevice)
                                    {
                                        console.Write("Checking disk ");
                                        console.Write_AsDecimal(diskNum);
                                        console.WriteLine("...");

                                        CheckDiskFormatting((Hardware.Devices.DiskDevice)Hardware.DeviceManager.Devices[diskNum]);
                                    }
                                    else
                                    {
                                        console.WriteLine("Cancelled - Specified device is not a disk device.");
                                    }
                                }
                                else
                                {
                                    console.WriteLine("You must specify which disk to check.");
                                }

                                #endregion
                            }
                            else if (cmd == "formatdisk" || cmd == "fmtd")
                            {
                                //For details on how the code here works, see Init
                                #region Format Disk

                                FOS_System.String opt1 = null;
                                if (cmdParts.Count > 1)
                                {
                                    opt1 = (FOS_System.String)cmdParts[1];
                                }

                                if (opt1 != null)
                                {
                                    int diskNum = (int)FOS_System.Int32.Parse_DecimalUnsigned(opt1, 0);

                                    if (Hardware.DeviceManager.Devices[diskNum] is Hardware.Devices.DiskDevice)
                                    {
                                        console.Write("Are you sure you wish to continue? (Y/N) : ");
                                        FOS_System.String str = ReadLine().ToLower();
                                        if (str == "y")
                                        {
                                            console.Write("Formatting disk ");
                                            console.Write_AsDecimal(diskNum);
                                            console.WriteLine("...");

                                            FormatDisk((Hardware.Devices.DiskDevice)Hardware.DeviceManager.Devices[diskNum]);
                                        }
                                        else
                                        {
                                            console.WriteLine("Cancelled.");
                                        }
                                    }
                                    else
                                    {
                                        console.WriteLine("Cancelled - Specified device is not a disk device.");
                                    }
                                }
                                else
                                {
                                    console.WriteLine("You must specify which disk to check.");
                                }

                                #endregion
                            }
                            else if (cmd == "gc")
                            {
                                //For details on how the code here works, see Init
                                #region GC
                                FOS_System.String opt1 = null;
                                if (cmdParts.Count > 1)
                                {
                                    opt1 = (FOS_System.String)cmdParts[1];
                                }

                                if (opt1 != null)
                                {
                                    if (opt1 == "cleanup")
                                    {
                                        FOS_System.GC.Cleanup();
                                    }
                                    else
                                    {
                                        UnrecognisedOption(opt1);
                                    }
                                }
                                else
                                {
                                    console.WriteLine("You must specify what to do. { Cleanup }");
                                }
                                #endregion
                            }
                            else if (cmd == "usb")
                            {
                                //For details on how the code here works, see Init
                                #region USB
                                FOS_System.String opt1 = null;
                                if (cmdParts.Count > 1)
                                {
                                    opt1 = (FOS_System.String)cmdParts[1];
                                }

                                if (opt1 != null)
                                {
                                    if (opt1 == "update")
                                    {
                                        Hardware.USB.USBManager.Update();
                                    }
                                    else if (opt1 == "eject")
                                    {
                                        FOS_System.String opt2 = null;
                                        if (cmdParts.Count > 2)
                                        {
                                            opt2 = (FOS_System.String)cmdParts[2];
                                        }

                                        if (opt2 != null)
                                        {
                                            if (opt2 == "msd")
                                            {
                                                FOS_System.String opt3 = null;
                                                if (cmdParts.Count > 3)
                                                {
                                                    opt3 = (FOS_System.String)cmdParts[3];
                                                }
                                                if (opt3 != null)
                                                {
                                                    EjectMSD(FOS_System.Int32.Parse_DecimalSigned(opt3));
                                                }
                                                else
                                                {
                                                    console.WriteLine("You must specify a device number!");
                                                }
                                            }
                                            else
                                            {
                                                console.WriteLine("Unrecognised device type!");
                                            }
                                        }
                                        else
                                        {
                                            console.WriteLine("You must specify a device type! (msd)");
                                        }
                                    }
                                    else
                                    {
                                        UnrecognisedOption(opt1);
                                    }
                                }
                                else
                                {
                                    console.WriteLine("You must specify what to do. { Update }");
                                }
                                #endregion
                            }
                            else if (cmd == "dir")
                            {
                                //For details on how the code here works, see Init

                                //Note: "./" prefix on a dir/file path means current 
                                //      directory so it must be replaced by the 
                                //      current directory.
                                #region Dir
                                FOS_System.String opt1 = null;
                                if (cmdParts.Count > 1)
                                {
                                    opt1 = (FOS_System.String)cmdParts[1];
                                }

                                if (opt1 != null)
                                {
                                    if (opt1 == "list")
                                    {
                                        FOS_System.String opt2 = null;
                                        if (cmdParts.Count > 2)
                                        {
                                            opt2 = (FOS_System.String)cmdParts[2];
                                        }

                                        if (opt2 != null)
                                        {
                                            if (opt2.StartsWith("./"))
                                            {
                                                opt2 = CurrentDir + opt2.Substring(2, opt2.length - 2);
                                            }
                                            console.WriteLine("Listing dir: " + opt2);
                                            OutputDirectoryContents(opt2);
                                        }
                                        else
                                        {
                                            console.WriteLine("You must specify a directory path.");
                                        }
                                    }
                                    else if (opt1 == "open")
                                    {
                                        FOS_System.String opt2 = null;
                                        if (cmdParts.Count > 2)
                                        {
                                            opt2 = (FOS_System.String)cmdParts[2];
                                        }

                                        if (opt2 != null)
                                        {
                                            if (opt2.StartsWith("./"))
                                            {
                                                opt2 = CurrentDir + opt2.Substring(2, opt2.length - 2);
                                            }

                                            Directory aDir = Directory.Find(opt2);
                                            if (aDir != null)
                                            {
                                                CurrentDir = aDir.GetFullPath();
                                            }
                                            else
                                            {
                                                console.WriteLine("Directory not found!");
                                            }
                                        }
                                        else
                                        {
                                            console.WriteLine("You must specify a directory path.");
                                        }
                                    }
                                    else if (opt1 == "new")
                                    {
                                        FOS_System.String opt2 = null;
                                        if (cmdParts.Count > 2)
                                        {
                                            opt2 = (FOS_System.String)cmdParts[2];
                                        }

                                        if (opt2 != null)
                                        {
                                            if (opt2.StartsWith("./"))
                                            {
                                                opt2 = CurrentDir + opt2.Substring(2, opt2.length - 2);
                                            }
                                            console.WriteLine("Creating dir: " + opt2);
                                            NewDirectory(opt2);
                                        }
                                        else
                                        {
                                            console.WriteLine("You must specify a directory path.");
                                        }
                                    }
                                    else if (opt1 == "delete")
                                    {
                                        FOS_System.String opt2 = null;
                                        if (cmdParts.Count > 2)
                                        {
                                            opt2 = (FOS_System.String)cmdParts[2];
                                        }

                                        if (opt2 != null)
                                        {
                                            if (opt2.StartsWith("./"))
                                            {
                                                opt2 = CurrentDir + opt2.Substring(2, opt2.length - 2);
                                            }
                                            console.WriteLine("Deleting dir: " + opt2);
                                            DeleteDirectory(opt2);
                                        }
                                        else
                                        {
                                            console.WriteLine("You must specify a directory path.");
                                        }
                                    }
                                    else if (opt1 == "copy")
                                    {
                                        FOS_System.String opt2 = null;
                                        if (cmdParts.Count > 2)
                                        {
                                            opt2 = (FOS_System.String)cmdParts[2];
                                        }

                                        if (opt2 != null)
                                        {
                                            if (opt2.StartsWith("./"))
                                            {
                                                opt2 = CurrentDir + opt2.Substring(2, opt2.length - 2);
                                            }

                                            FOS_System.String opt3 = null;
                                            if (cmdParts.Count > 3)
                                            {
                                                opt3 = (FOS_System.String)cmdParts[3];
                                            }

                                            if (opt3 != null)
                                            {
                                                if (opt3.StartsWith("./"))
                                                {
                                                    opt3 = CurrentDir + opt3.Substring(2, opt3.length - 2);
                                                }

                                                console.WriteLine("Copy cmd, opt2=\"" + opt2 + "\", opt3=\"" + opt3 + "\"");
                                                CopyDirectory(opt2, opt3);
                                            }
                                            else
                                            {
                                                console.WriteLine("You must specify a destination path.");
                                            }
                                        }
                                        else
                                        {
                                            console.WriteLine("You must specify a source path.");
                                        }
                                    }
                                    else
                                    {
                                        UnrecognisedOption(opt1);
                                    }
                                }
                                else
                                {
                                    console.WriteLine("You must specify what to do. { List/Open/New/Delete/Copy }");
                                }
                                #endregion
                            }
                            else if (cmd == "file")
                            {
                                //For details on how the code here works, see Init

                                //Note: "./" prefix on a dir/file path means current 
                                //      directory so it must be replaced by the 
                                //      current directory.
                                #region File
                                FOS_System.String opt1 = null;
                                if (cmdParts.Count > 1)
                                {
                                    opt1 = (FOS_System.String)cmdParts[1];
                                }

                                if (opt1 != null)
                                {
                                    if (opt1 == "open")
                                    {
                                        FOS_System.String opt2 = null;
                                        if (cmdParts.Count > 2)
                                        {
                                            opt2 = (FOS_System.String)cmdParts[2];
                                        }

                                        if (opt2 != null)
                                        {
                                            if (opt2.StartsWith("./"))
                                            {
                                                opt2 = CurrentDir + opt2.Substring(2, opt2.length - 2);
                                            }

                                            OutputFileContents(opt2);
                                        }
                                        else
                                        {
                                            console.WriteLine("You must specify a file path.");
                                        }
                                    }
                                    else if (opt1 == "delete")
                                    {
                                        FOS_System.String opt2 = null;
                                        if (cmdParts.Count > 2)
                                        {
                                            opt2 = (FOS_System.String)cmdParts[2];
                                        }

                                        if (opt2 != null)
                                        {
                                            if (opt2.StartsWith("./"))
                                            {
                                                opt2 = CurrentDir + opt2.Substring(2, opt2.length - 2);
                                            }

                                            DeleteFile(opt2);
                                        }
                                        else
                                        {
                                            console.WriteLine("You must specify a file path.");
                                        }
                                    }
                                    else if (opt1 == "copy")
                                    {
                                        FOS_System.String opt2 = null;
                                        if (cmdParts.Count > 2)
                                        {
                                            opt2 = (FOS_System.String)cmdParts[2];
                                        }

                                        if (opt2 != null)
                                        {
                                            if (opt2.StartsWith("./"))
                                            {
                                                opt2 = CurrentDir + opt2.Substring(2, opt2.length - 2);
                                            }

                                            FOS_System.String opt3 = null;
                                            if (cmdParts.Count > 3)
                                            {
                                                opt3 = (FOS_System.String)cmdParts[3];
                                            }

                                            if (opt3 != null)
                                            {
                                                if (opt3.StartsWith("./"))
                                                {
                                                    opt3 = CurrentDir + opt3.Substring(2, opt3.length - 2);
                                                }

                                                console.WriteLine("Copy cmd, opt2=\"" + opt2 + "\", opt3=\"" + opt3 + "\"");
                                                CopyFile(opt2, opt3);
                                            }
                                            else
                                            {
                                                console.WriteLine("You must specify a destination path.");
                                            }
                                        }
                                        else
                                        {
                                            console.WriteLine("You must specify a source path.");
                                        }
                                    }
                                    else
                                    {
                                        UnrecognisedOption(opt1);
                                    }
                                }
                                else
                                {
                                    console.WriteLine("You must specify what to do. { Open/Delete/Copy }");
                                }
                                #endregion
                            }
                            else if (cmd == "test")
                            {
                                //For details on how the code here works, see Init
                                #region Test
                                FOS_System.String opt1 = null;
                                if (cmdParts.Count > 1)
                                {
                                    opt1 = (FOS_System.String)cmdParts[1];
                                }

                                if (opt1 != null)
                                {
                                    if (opt1 == "interrupts")
                                    {
                                        InterruptsTest();
                                    }
                                    else if (opt1 == "delegates")
                                    {
                                        DelegateTest();
                                    }
                                    else if (opt1 == "filesystems")
                                    {
                                        FileSystemTests();
                                    }
                                    else if (opt1 == "ulltcomp")
                                    {
                                        ULongLTComparisonTest();
                                    }
                                    else if (opt1 == "stringconcat")
                                    {
                                        StringConcatTest();
                                    }
                                    else if (opt1 == "objarray")
                                    {
                                        ObjectArrayTest();
                                    }
                                    else if (opt1 == "intarray")
                                    {
                                        IntArrayTest();
                                    }
                                    else if (opt1 == "dummyobj")
                                    {
                                        DummyObjectTest();
                                    }
                                    else if (opt1 == "divideby0")
                                    {
                                        DivideByZeroTest();
                                    }
                                    else if (opt1 == "exceptions1")
                                    {
                                        ExceptionsTestP1();
                                    }
                                    else if (opt1 == "exceptions2")
                                    {
                                        ExceptionsTestP2();
                                    }
                                    else if (opt1 == "pcbeep")
                                    {
                                        PCBeepTest();
                                    }
                                    else if (opt1 == "timer")
                                    {
                                        TimerTest();
                                    }
                                    else if (opt1 == "keyboard")
                                    {
                                        KeyboardTest();
                                    }
                                    else if (opt1 == "fieldstable")
                                    {
                                        FieldsTableTest();
                                    }
                                    else if (opt1 == "isinst")
                                    {
                                        IsInstTest();
                                    }
                                    else if (opt1 == "virtmem")
                                    {
                                        Hardware.VirtMemManager.Test();
                                    }
                                    else if (opt1 == "longs")
                                    {
                                        LongsTest();
                                    }
                                    else if (opt1 == "threadsleep")
                                    {
                                        ThreadSleepTest();
                                    }
                                    else if (opt1 == "heap")
                                    {
                                        HeapTest();
                                    }
                                    else if (opt1 == "gc")
                                    {
                                        GCTest();
                                    }
                                    else if (opt1 == "ata")
                                    {
                                        ATATest();
                                    }
                                    else if (opt1 == "usb")
                                    {
                                        USBTest();
                                    }
                                    else
                                    {
                                        UnrecognisedOption(opt1);
                                    }
                                }
                                else
                                {
                                    console.WriteLine("You must specify which test. { Interrupts  /  Delegates    /  FileSystems /\n" +
                                                      "                               ULLTComp    /  StringConcat /  ObjArray    /\n" +
                                                      "                               IntArray    /  DummyObj     /  DivideBy0   /\n" +
                                                      "                               Exceptions1 /  Exceptions2  /  PCBeep      /\n" +
                                                      "                               Timer       /  Keyboard     /  FieldsTable /\n" +
                                                      "                               IsInst      /  VirtMem                     }");
                                }
                                #endregion
                            }
                            else if (cmd == "start")
                            {
                                //For details on how the code here works, see Init
                                #region Start
                                FOS_System.String opt1 = null;
                                if (cmdParts.Count > 1)
                                {
                                    opt1 = (FOS_System.String)cmdParts[1];
                                }

                                if (opt1 != null)
                                {
                                    if (opt1.StartsWith("./"))
                                    {
                                        opt1 = CurrentDir + opt1.Substring(2, opt1.length - 2);
                                    }

                                    File aFile = File.Open(opt1);
                                    if (aFile != null)
                                    {
                                        FOS_System.String opt2 = null;
                                        if (cmdParts.Count > 2)
                                        {
                                            opt2 = (FOS_System.String)cmdParts[2];
                                        }

                                        if (opt2 != null)
                                        {
                                            if (opt2 == "km")
                                            {
                                                FOS_System.String opt3 = null;
                                                if (cmdParts.Count > 3)
                                                {
                                                    opt3 = (FOS_System.String)cmdParts[3];
                                                }

                                                if (opt3 != null)
                                                {
                                                    if (opt3 == "raw")
                                                    {
                                                        Hardware.Processes.ProcessManager.RegisterProcess(
                                                            Processes.DynamicLinkerLoader.LoadProcess_FromRawExe(aFile, false),
                                                            Hardware.Processes.Scheduler.Priority.Normal);
                                                    }
                                                    else if (opt3 == "elf")
                                                    {
                                                        Hardware.Processes.ProcessManager.RegisterProcess(
                                                            Processes.DynamicLinkerLoader.LoadProcess_FromELFExe(aFile, false).TheProcess,
                                                            Hardware.Processes.Scheduler.Priority.Normal);
                                                    }
                                                    else
                                                    {
                                                        UnrecognisedOption(opt3);
                                                    }
                                                }
                                                else
                                                {
                                                    //Run as RAW for now
                                                    Hardware.Processes.ProcessManager.RegisterProcess(
                                                        Processes.DynamicLinkerLoader.LoadProcess_FromRawExe(aFile, false),
                                                        Hardware.Processes.Scheduler.Priority.Normal);
                                                }
                                            }
                                            else if (opt2 == "um")
                                            {
                                                FOS_System.String opt3 = null;
                                                if (cmdParts.Count > 3)
                                                {
                                                    opt3 = (FOS_System.String)cmdParts[3];
                                                }

                                                if (opt3 != null)
                                                {
                                                    if (opt3 == "raw")
                                                    {
                                                        Hardware.Processes.ProcessManager.RegisterProcess(
                                                            Processes.DynamicLinkerLoader.LoadProcess_FromRawExe(aFile, true),
                                                            Hardware.Processes.Scheduler.Priority.Normal);
                                                    }
                                                    else if (opt3 == "elf")
                                                    {
                                                        Hardware.Processes.ProcessManager.RegisterProcess(
                                                            Processes.DynamicLinkerLoader.LoadProcess_FromELFExe(aFile, true).TheProcess,
                                                            Hardware.Processes.Scheduler.Priority.Normal);
                                                    }
                                                    else
                                                    {
                                                        UnrecognisedOption(opt3);
                                                    }
                                                }
                                                else
                                                {
                                                    //Run as RAW for now
                                                    Hardware.Processes.ProcessManager.RegisterProcess(
                                                        Processes.DynamicLinkerLoader.LoadProcess_FromRawExe(aFile, true),
                                                        Hardware.Processes.Scheduler.Priority.Normal);
                                                }
                                            }
                                            else
                                            {
                                                UnrecognisedOption(opt2);
                                            }
                                        }
                                        else
                                        {
                                            //Run as KM, ELF for now
                                            Hardware.Processes.ProcessManager.RegisterProcess(
                                                Processes.DynamicLinkerLoader.LoadProcess_FromELFExe(aFile, false).TheProcess,
                                                Hardware.Processes.Scheduler.Priority.Normal);
                                        }
                                    }
                                    else
                                    {
                                        console.WriteLine("File not found.");
                                    }
                                }
                                else
                                {
                                    console.WriteLine("You must specify the file path.");
                                }
                                #endregion
                            }
                            else if (cmd == "ily")
                            {
                                #region ILY

                                Hardware.Processes.ProcessManager.RegisterProcess(
                                    Processes.DynamicLinkerLoader.LoadProcess_FromRawExe(
                                        File.Open("a:/ily.bin"), false),
                                    Hardware.Processes.Scheduler.Priority.Normal);

                                #endregion
                            }
                            else if (cmd == "show")
                            {
                                #region Show

                                FOS_System.String opt1 = null;
                                if (cmdParts.Count > 1)
                                {
                                    opt1 = (FOS_System.String)cmdParts[1];
                                }
                                ShowLicense(opt1);

                                #endregion
                            }
                            else if (cmd == "help")
                            {
                                #region Help

                                FOS_System.String opt1 = null;
                                if (cmdParts.Count > 1)
                                {
                                    opt1 = (FOS_System.String)cmdParts[1];
                                }
                                ShowHelp(opt1);

                                #endregion
                            }
                            else if (cmd == "clear")
                            {
                                #region Clear

                                console.Clear();

                                #endregion
                            }
                            else if (cmd == "easter")
                            {
                                #region Easter

                                Hardware.Processes.ProcessManager.CurrentProcess.CreateThread(Tasks.EasterTask.Main, "Easter");

                                #endregion
                            }
                            else if (cmd == "reboot")
                            {
                                #region Reboot

                                Reboot();

                                #endregion
                            }
                        }
                    }
                    catch
                    {
                        OutputExceptionInfo(ExceptionMethods.CurrentException);
                    }
                }
            }
            catch
            {
                OutputExceptionInfo(ExceptionMethods.CurrentException);
                //Pause to give us the chance to read the output. 
                //  We do not know what the code outside this shell may do.
                Processes.SystemCalls.SleepThread(1000);
            }
            console.WriteLine("Shell exited.");
        }

        /// <summary>
        /// Blocking. Reads all the next valid (i.e. not \0) characters from the keyboard and outputs them
        /// until a new line is entered (using the Enter key). Also supports backspace and escape keys.
        /// </summary>
        /// <returns>The line of text.</returns>
        public FOS_System.String ReadLine()
        {
            //Temp store for the result
            FOS_System.String result = "";
            //Used to store the last key pressed
            KeyMapping c;
            //Loop through getting characters until the enter key is pressed
            while ((c = keyboard.ReadMapping()).Key != KeyboardKey.Enter)
            {
                //If backspace was pressed:
                if (c.Key == KeyboardKey.Backspace)
                {
                    //If we actually have something to delete:
                    if (result.length > 0)
                    {
                        //Remove the last character
                        result = result.Substring(0, result.length - 1);

                        //Print the backspace character
                        console.Write(c.Value);
                    }
                }
                else if (c.Key == KeyboardKey.Escape)
                {
                    //Clear output line
                    console.Write(((FOS_System.String)"").PadLeft(result.length, '\b'));
                
                    //Clear out the result
                    result = "";
                }
                else if (c.Key == KeyboardKey.UpArrow)
                {
                    //Scroll up the screen 1 line
                    //Scroll(-1);
                    //TODO: Work out how to handle this in the new context-----------------------------
                }
                else if (c.Key == KeyboardKey.DownArrow)
                {
                    //Scroll down the screen 1 line
                    //Scroll(1);
                    //TODO: Work out how to handle this in the new context
                }
                //If the key has a character representation
                else if (c.Value != '\0')
                {
                    //Add the character to the result
                    result += c.Value;

                    //Print the character
                    console.Write(c.Value);
                }
            }
            console.WriteLine();

            //Return the resulting line
            return result;
        }

        /// <summary>
        /// Handler for the periodic reboot timer event.
        /// </summary>
        /// <param name="state">The state object. Should be null.</param>
        private void TriggerPeriodicReboot(object state)
        {
            ((MainShell)state).Reboot();
        }
        /// <summary>
        /// Reboots the computer
        /// </summary>
        private void Reboot()
        {
            if (Hardware.Keyboards.PS2.ThePS2 != null)
            {
                console.WarningColour();
                console.Write("Attempting 8042 reset...");

                Hardware.Keyboards.PS2.ThePS2.Reset();

                console.ErrorColour();
                console.WriteLine("failed.");
            }
            else
            {
                console.ErrorColour();
                console.WriteLine("No reboot method available.");
            }

            console.DefaultColour();
        }

        /// <summary>
        /// Displays command
        /// </summary>
        /// <param name="commandName">Name of the command to show help for or null to show general help / complete list of commands.</param>
        private void ShowHelp(FOS_System.String commandName = null)
        {
            if(commandName != null)
            {
                console.WriteLine(CommandHelp.GetCommandDescription(commandName));
            }
            else
            {
                console.WriteLine("For more information on a specific command, type help <command-name>.");
                console.WriteLine("Possible commands are: ");
                for (int i = 0; i < CommandHelp.CommandDescriptions.Count; i++)
                {
                    CommandDescription cmdDesc = (CommandDescription)CommandHelp.CommandDescriptions[i];
                    console.WriteLine(cmdDesc.CommandName);
                }
            }
        }

        /// <summary>
        /// Displayes license information on the console, also called at start of the shell session
        /// </summary>
        /// <param name="option">if "c", displays license conditions and if "w" displayes warnings</param>
        private void ShowLicense(FOS_System.String option = null)
        {

            string LicenseConditions = "This program is distributed under GPL V2; See GPL V2 License for details.";
            
            string LicenseCommandUnrecognized = @"Unrecognized option passed, to see the license, enter 'show'.
To see license warnings, enter 'show w'.
To see license conditions, enter 'show c'.";

            string LicenseWarnings = @"This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details";

            if(option == null) // If no options is passed, then just display the License
            {
                console.WriteLine(@"Fling OS  Copyright (C) 2015  Edward Nutting
This program comes with ABSOLUTELY NO WARRANTY.
This is free software, and you are welcome to redistribute it
under certain conditions; See GPL V2 for details, a copy of
which should have been provided with the executable.");
            }
            else
            {
                if(option == "c")   // If option is conditions
                {
                    console.WriteLine(LicenseConditions); // Show conditions
                }
                else if (option == "w") // If option is license warnings
                {
                    console.WriteLine(LicenseWarnings);
                }
                else
                {
                    console.WriteLine(LicenseCommandUnrecognized);  // In case it is not a valid option
                }
            }
        }

        /// <summary>
        /// Splits the input string into commands including handling quoted parts.
        /// </summary>
        /// <param name="input">The input to split.</param>
        /// <returns>The list of command parts.</returns>
        private List SplitCommand(FOS_System.String input)
        {
            //This method splits the input into parts separated by spaces
            //  However, it must then also search for grouped parts which
            //  are indicated by start and end quote marks (").

            //Split the input by space
            List parts = input.Split(' ');
            //Create a list for the result - capacity 4 is the usual maximum we expect so this just
            //  optimises the internal array creation a bit.
            List result = new List(4);
            //Stores the current part being constructed.
            FOS_System.String currPart = "";
            //Indicates whether we are constructing a grouped part or not.
            bool waitingForCloseQuote = false;
            //Loop through all parts
            for(int i = 0; i < parts.Count; i++)
            {
                //If we are constructing a grouped part
                if (waitingForCloseQuote)
                {
                    //Add the part (including the space which was removed by split)
                    //  to the currently constructing part
                    currPart += " " + (FOS_System.String)parts[i];

                    //If the part ends with a quote, then we have found our closing quote
                    //  which terminates the group part
                    if(currPart.EndsWith("\""))
                    {
                        //Remove the closing quote
                        currPart = currPart.Substring(0, currPart.length - 1);
                        //End the search
                        waitingForCloseQuote = false;
                        //Add the part to the result
                        result.Add(currPart.ToLower());
                    }
                }
                else
                {
                    //Set the current part
                    currPart = (FOS_System.String)parts[i];

                    //If it starts with a quote, it is the start of a group part
                    if(currPart.StartsWith("\""))
                    {
                        //If it ends with a quote, it is also the end of the group part
                        //  so essentially the user grouped something which didn't 
                        //  actually contain any spaces.
                        if (currPart.EndsWith("\""))
                        {
                            //Remove the start and end quotes
                            currPart = currPart.Substring(1, currPart.length - 2);
                            //Add the part to the result
                            result.Add(currPart.ToLower());
                        }
                        else
                        {
                            //Remove the start quote
                            currPart = currPart.Substring(1, currPart.length - 1);
                            //Begin the search for the end of the group part
                            waitingForCloseQuote = true;
                        }
                    }
                    else
                    {
                        //This is a normal, ungrouped part so just add it to
                        //  the result
                        result.Add(currPart.ToLower());
                    }
                }
            }
            return result;
        }
         
        /// <summary>
        /// Cleans the caches of all known disk devices.
        /// </summary>
        private void CleanDiskCaches()
        {
            //Loop through all devices looking for Disk devices.
            for (int i = 0; i < Hardware.DeviceManager.Devices.Count; i++)
            {
                Hardware.Device aDevice = (Hardware.Device)Hardware.DeviceManager.Devices[i];
                if (aDevice is Hardware.Devices.DiskDevice)
                {
                    //Clean caches of the device.
                    ((Hardware.Devices.DiskDevice)aDevice).CleanCaches();
                }
            }
        }
        /// <summary>
        /// Ejects the specified mass storage device.
        /// </summary>
        /// <param name="deviceNum">The index of the MSD in the Hardware.DeviceManager.Devices list.</param>
        /// <seealso cref="Kernel.Hardware.USB.Devices.MassStorageDevice.Eject"/>
        private void EjectMSD(int deviceNum)
        {
            //Output info to the user...
            console.Write("Ejecting MSD ");
            console.Write_AsDecimal(deviceNum);
            console.WriteLine("...");

            //Get the device
            Hardware.USB.Devices.MassStorageDevice msd = (Hardware.USB.Devices.MassStorageDevice)
                Hardware.DeviceManager.Devices[deviceNum];
            
            //Eject the MSD
            msd.Eject();

            //Output info to the user
            console.WriteLine("Ejected.");
        }

        /// <summary>
        /// Copies the specified file.
        /// </summary>
        /// <param name="src">The path to the file to copy.</param>
        /// <param name="dst">The path to copy to.</param>
        private void CopyFile(FOS_System.String src, FOS_System.String dst)
        {
            //Attempt to open the source file. If it is not found, null will be passed
            //  and the CopyFile method will catch the failure.
            CopyFile(File.Open(src), dst);
        }
        /// <summary>
        /// Copies the specified file.
        /// </summary>
        /// <param name="srcFile">The file to copy.</param>
        /// <param name="dst">The path to copy to.</param>
        private void CopyFile(File srcFile, FOS_System.String dst)
        {
            //If source file is null, it means it wasn't found by the caller (or some
            //  other error but we will assume not found since that is the expected use 
            //  case from the other overload of CopyFile).
            if(srcFile == null)
            {
                console.WriteLine("Source file not found!");
                return;
            }

            //Attempt to open the destination file.
            File dstFile = File.Open(dst);
            //If opening failed, the file was not found so either the path
            //  was invalid or the file does not currently exist. We assume
            //  the latter. If the path is invalid, it will be caught later.
            if (dstFile == null)
            {
                //console.WriteLine("Creating destination file...");
                //If the path is invalid because of path mapping, it will be 
                //  caught here.
                FileSystemMapping mapping = FileSystemManager.GetMapping(dst);
                if (mapping == null)
                {
                    console.WriteLine("Destination file system not found!");
                    return;
                }

                //+1 to include the slash in dir name
                int lastIdx = dst.LastIndexOf(FileSystemManager.PathDelimiter) + 1;
                FOS_System.String dstDir = dst.Substring(0, lastIdx);
                FOS_System.String dstName = dst.Substring(lastIdx, dst.length - lastIdx);

                //console.WriteLine("dstDir: " + dstDir);
                //console.WriteLine("dstName: " + dstName);

                //New directory either creates the directory, or returns the existing directory
                Directory parentDir = NewDirectory(dstDir);
                //If the parent dir is null, the path must be invalid so we cannot create the
                //  dest file.
                if (parentDir != null)
                {
                    dstFile = mapping.TheFileSystem.NewFile(dstName, parentDir);
                }
                //If we still failed to create the file, then the path was totally invalid
                if (dstFile == null)
                {
                    console.WriteLine("Failed to create destination file!");
                    return;
                }

                console.WriteLine("Destination file created.");
            }
            else
            {
                console.WriteLine("Destination file already exists.");
            }

            //Get full path resolves the path of the file without using short-hands
            //  such as ./ and .. which can be used in the arguments to this method.
            //  So, GetFullPath allows us to do a consistent comparison of the paths.
            FOS_System.String srcFullPath = srcFile.GetFullPath();
            FOS_System.String dstFullPath = dstFile.GetFullPath();
            //If we are about to copy a file onto itself, well that wouldn't technically
            //  give us an issue given our copy implementation, but it is pretty pointless.
            //  Also, it would give a more sofisticated copy algorithm (e.g. block copying
            //  for large files) a big problem!
            if (srcFullPath == dstFullPath)
            {
                console.WriteLine("Atempted to copy a file to itself! (" + srcFullPath + ")");
                return;
            }
            else
            {
                console.WriteLine("Copying " + srcFullPath + " to " + dstFullPath);
            }

            //Get the streams to read from / write to
            FOS_System.IO.Streams.FileStream srcStr = srcFile.GetStream();
            FOS_System.IO.Streams.FileStream dstStr = dstFile.GetStream();

            //Temporary storage. Note: If the file is to big, this will just fail 
            //  as there won't be enough heap memory available
            byte[] data = new byte[(uint)srcFile.TheFileSystem.ThePartition.BlockSize];

            //Force stream positions
            srcStr.Position = 0;
            dstStr.Position = 0;

            console.Write("[");

            int percentile = (int)(uint)FOS_System.Math.Divide(srcFile.Size, 78u);
            int dist = 0;
            while ((ulong)srcStr.Position < srcFile.Size)
            {
                //Read in source data.
                srcStr.Read(data, 0, data.Length);

                //Write out data to destination.
                dstStr.Write(data, 0, data.Length);

                dist += data.Length;

                if (dist >= percentile)
                {
                    console.Write(".");
                    dist -= percentile;
                }
            }

            console.WriteLine("]");
            console.WriteLine("Copied successfully.");
        }
  
        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="fileName">The path to the file to delete.</param>
        private void DeleteFile(FOS_System.String fileName)
        {
            //Attempt to delete the file
            if (File.Delete(fileName))
            {
                //Output info to the user.
                console.WriteLine("File deleted: " + fileName);
            }
            else
            {
                //Output info to the user.
                console.WriteLine("File not found: " + fileName);
            }
        }
  
        /// <summary>
        /// Copies the specified directory.
        /// </summary>
        /// <param name="src">The path to the directory to copy.</param>
        /// <param name="dst">The path to copy to.</param>
        private void CopyDirectory(FOS_System.String src, FOS_System.String dst)
        {
            //Attempt to load the source directory. If it is not found, null will be passed
            //  and the CopyDirectory method will catch the failure.
            CopyDirectory(Directory.Find(src), dst);
        }
 
        /// <summary>
        /// Copies the specified directory.
        /// </summary>
        /// <param name="srcDir">The directory to copy.</param>
        /// <param name="dst">The path to copy to.</param>
        private void CopyDirectory(Directory srcDir, FOS_System.String dst)
        {
            //If source directory is null, it means it wasn't found by the caller (or some
            //  other error but we will assume not found since that is the expected use 
            //  case from the other overload of CopyDirectory).
            if (srcDir == null)
            {
                console.WriteLine("Source directory not found!");
                return;
            }

            //Add a trailing "/" to the destination path name
            if(!dst.EndsWith(FileSystemManager.PathDelimiter))
            {
                dst += FileSystemManager.PathDelimiter;
            }

            //Creates the entire directory tree as required or returns
            //  the existing directory.
            Directory dstDir = NewDirectory(dst);

            //For explanation of this, see CopyFile
            FOS_System.String srcFullPath = srcDir.GetFullPath();
            FOS_System.String dstFullPath = dstDir.GetFullPath();
            if (srcFullPath == dstFullPath)
            {
                console.WriteLine("Atempted to copy a directory to itself! (" + srcFullPath + ")");
                return;
            }
            else
            {
                console.WriteLine("Copying " + srcFullPath + " to " + dstFullPath);
            }

            //Copy listings
            //  This causes CopyDirectory to be a recursive, self-calling method
            //  which could potentially overflow. It has the benefit though that 
            //  the entire sub-directory/sub-file tree will be copied.
            List listings = srcDir.GetListings();
            for(int i = 0; i < listings.Count; i++)
            {
                Base listing = (Base)listings[i];
                if(listing.IsDirectory)
                {
                    if (listing.Name != "." && listing.Name != "..")
                    {
                        CopyDirectory((Directory)listing, dst + listing.Name + FileSystemManager.PathDelimiter);
                    }
                }
                else
                {
                    CopyFile((File)listing, dst + listing.Name);
                }
            }
        }
   
        /// <summary>
        /// Deletes the specified directory.
        /// </summary>
        /// <param name="path">The path to the directory to delete.</param>
        private void DeleteDirectory(FOS_System.String path)
        {
            //Attempt to delete the directory.
            if (Directory.Delete(path))
            {
                //Output info to the user.
                console.WriteLine("Directory deleted: " + path);
            }
            else
            {
                //Output info to the user.
                console.WriteLine("Directory not found: " + path);
            }
        }
 
        /// <summary>
        /// Creates a new directory (and parent directories). Used recursively.
        /// </summary>
        /// <param name="path">The full path of the directory (and parent directories) to create.</param>
        /// <returns>The new (or existing) directory.</returns>
        private Directory NewDirectory(FOS_System.String path)
        {
            //Output info to the user.
            console.WriteLine("Searching for directory: " + path);

            //Attempt to find the directory. If it already exists, we don't want to
            //  accidentally re-create it!
            Directory theDir = Directory.Find(path);
            //If the directory does not exist:
            if (theDir == null)
            {
                //Output info to the user.
                console.WriteLine("Creating directory...");

                //Attempt to get the file system mapping for the new directory
                FileSystemMapping mapping = FileSystemManager.GetMapping(path);
                //If the mapping was found:
                if(mapping != null)
                {
                    //Remove trailing "/" if there is one else the code below would end
                    //  up with a blank "new directory name"
                    if (path.EndsWith(FileSystemManager.PathDelimiter))
                    {
                        path = path.Substring(0, path.length - 1);
                    }

                    //  + 1 as we wish to include the path delimeter in parent dir name and
                    //      not in the new dir name.
                    //  Note: It is important to include the path delimeter at the end of the parent dir name
                    //        as the parent dir name may be a FS root which requires the trailing path delimeter.
                    int lastIdx = path.LastIndexOf(FileSystemManager.PathDelimiter) + 1;
                    FOS_System.String dirParentPath = path.Substring(0, lastIdx);
                    FOS_System.String newDirName = path.Substring(lastIdx, path.length - lastIdx);

                    console.WriteLine("Checking parent path: " + dirParentPath);
                    //This causes NewDirectory to become a recursive, self-calling
                    //  method which could potentially overflow. However, if has
                    //  the benefit that the entire new directory tree can be created
                    //  in one call, rather than having to create each directory and 
                    //  sub-directory one at a time.
                    Directory parentDir = NewDirectory(dirParentPath);
                    if (parentDir != null)
                    {
                        console.WriteLine("New dir name: " + newDirName);
                        //Create the directory
                        theDir = mapping.TheFileSystem.NewDirectory(newDirName, parentDir);
                        console.WriteLine("Directory created.");
                    }
                    else
                    {
                        console.WriteLine("Failed to find or create parent directory.");
                    }
                }
                else
                {
                    console.WriteLine("File system mapping not found.");
                }
            }
            else
            {
                console.WriteLine("Directory already exists.");
            }

            return theDir;
        }

        /// <summary>
        /// Formats the specified disk.
        /// </summary>
        /// <param name="disk">The disk to format.</param>
        private void FormatDisk(Hardware.Devices.DiskDevice disk)
        {
            List newPartitionInfos = new List(1);
            
            console.WriteLine("Creating partition info...");
            newPartitionInfos.Add(FOS_System.IO.Disk.MBR.CreateFAT32PartitionInfo(disk, false));
            
            console.WriteLine("Done. Doing MBR format...");
            FOS_System.IO.Disk.MBR.FormatDisk(disk, newPartitionInfos);
            
            console.WriteLine("Done. Initialising disk...");
            FileSystemManager.InitDisk(disk);
            
            console.WriteLine("Done. Finding partition...");
            Partition thePart = null;
            for (int i = 0; i < FileSystemManager.Partitions.Count; i++)
            {
                Partition aPart = (Partition)FileSystemManager.Partitions[i];
                if(aPart.TheDiskDevice == disk)
                {
                    thePart = aPart;
                    break;
                }
            }
            if (thePart != null)
            {
                console.WriteLine("Done. Formatting as FAT32...");
                FOS_System.IO.FAT.FATFileSystem.FormatPartitionAsFAT32(thePart);
                
                console.WriteLine("Done.");
                console.WriteLine("Format completed successfully.");
            }
            else
            {
                console.WriteLine("Done. Partition not found.");
                console.WriteLine("Format failed.");
            }
        }
  
        /// <summary>
        /// Checks the specified disk's formatting.
        /// </summary>
        /// <param name="disk">The disk to check.</param>
        private void CheckDiskFormatting(Hardware.Devices.DiskDevice disk)
        {
            if (disk == null)
            {
                console.WriteLine("Can't check formatting of null disk!");
                return;
            }

            FOS_System.IO.Partition part = FOS_System.IO.Partition.GetFirstPartition(disk);
            if (part == null)
            {
                console.WriteLine("Disk not formatted correctly! No partitions found on disk.");
                return;
            }
            else if (!FOS_System.IO.FileSystemManager.HasMapping(part))
            {
                console.WriteLine("Disk not formatted correctly! File system mapping not found. (Did you remember to initialise file systems?)");
                return;
            }

            console.WriteLine("Disk formatting OK.");
        }

        /// <summary>
        /// Outputs the contents of the specified file if it exists.
        /// </summary>
        /// <param name="fileName">The file to try and output.</param>
        private void OutputFileContents(FOS_System.String fileName)
        {
            OutputDivider();
            File aFile = File.Open(fileName);
            if (aFile != null)
            {
                if (aFile.Size > 0)
                {
                    console.WriteLine(fileName);

                    FOS_System.IO.Streams.FileStream fileStream = FOS_System.IO.Streams.FileStream.Create(aFile);
                    
                    byte[] DataBuffer = aFile.TheFileSystem.ThePartition.NewBlockArray(1);
                    Tasks.SystemStatusTask.MainConsole.Write("[");
                    ulong percentile = FOS_System.Math.Divide(aFile.Size, 53u);
                    ulong pos = 0;
                    while ((ulong)fileStream.Position < aFile.Size)
                    {
                        int actuallyRead = fileStream.Read(DataBuffer, 0, DataBuffer.Length);
                        FOS_System.String xText = ByteConverter.GetASCIIStringFromASCII(DataBuffer, 0u, (uint)actuallyRead);
                        console.Write(xText);
                        
                        pos += (ulong)actuallyRead;
                        if (pos >= percentile)
                        {
                            pos -= percentile;
                            Tasks.SystemStatusTask.MainConsole.Write(".");
                        }

                        if (actuallyRead == 0)
                        {
                            break;
                        }
                    }
                    Tasks.SystemStatusTask.MainConsole.WriteLine("]");
                }
                else
                {
                    console.WriteLine("(empty file)");
                }
            }
            else
            {
                console.WriteLine("Failed to open file: " + fileName);
            }
            OutputDivider();
        }
   
        /// <summary>
        /// Outputs the contents of the specified directory if it exists.
        /// </summary>
        /// <param name="dir">The directory to try and output.</param>
        private void OutputDirectoryContents(FOS_System.String dir)
        {
            OutputDivider();
            Directory aDir = Directory.Find(dir);
            if (aDir != null)
            {
                console.WriteLine(dir);

                List Listings = aDir.GetListings();
                if (Listings.Count > 0)
                {
                    for (int j = 0; j < Listings.Count; j++)
                    {
                        FOS_System.IO.Base xItem = (FOS_System.IO.Base)Listings[j];

                        if (xItem.IsDirectory)
                        {
                            console.WriteLine(((FOS_System.String)"<DIR> '") + ((FOS_System.IO.Directory)Listings[j]).Name + "'");
                        }
                        else
                        {
                            FOS_System.IO.File file = (FOS_System.IO.File)Listings[j];
                            console.WriteLine(((FOS_System.String)"<FILE> '") + file.Name + "' (" + file.Size + ")");
                        }
                    }
                }
                else
                {
                    console.WriteLine("(empty directory)");
                }
            }
            else
            {
                console.WriteLine("Failed to find directory: " + dir);
            }
            OutputDivider();
        }
  
        /// <summary>
        /// Outputs the current memory information.
        /// </summary>
        private unsafe void OutputMemory()
        {
            console.Write("GC num objs: ");
            console.WriteLine(FOS_System.GC.NumObjs);
            
            console.Write("GC num strings: ");
            console.WriteLine(FOS_System.GC.NumStrings);
            
            console.Write("Heap memory use: ");
            console.Write_AsDecimal(Heap.FBlock->used * Heap.FBlock->bsize);
            console.Write(" / ");
            console.WriteLine_AsDecimal(Heap.FBlock->size);
        }
  
        /// <summary>
        /// Outputs the file systems information.
        /// </summary>
        private void OutputFS()
        {
            console.WriteLine(((FOS_System.String)"Num partitions: ") + FOS_System.IO.FileSystemManager.Partitions.Count);

            for (int i = 0; i < FileSystemManager.FileSystemMappings.Count; i++)
            {
                FileSystemMapping fsMapping = (FileSystemMapping)FileSystemManager.FileSystemMappings[i];
                if (fsMapping.TheFileSystem is FOS_System.IO.FAT.FATFileSystem)
                {
                    FOS_System.IO.FAT.FATFileSystem fs = (FOS_System.IO.FAT.FATFileSystem)fsMapping.TheFileSystem;
                    
                    console.WriteLine(((FOS_System.String)"FAT file system detected. Volume ID: ") + fs.ThePartition.VolumeID);
                }
                else if (fsMapping.TheFileSystem is FOS_System.IO.ISO9660.ISO9660FileSystem)
                {
                    console.WriteLine("ISO9660 file system detected.");
                }
                else
                {
                    console.WriteLine("Unrecognised file system type added! (Did we implement a new one?)");
                }
                console.WriteLine("    - Prefix: " + fsMapping.Prefix);
            }
        }

        /// <summary>
        /// Outputs the USB system information.
        /// </summary>
        private void OutputUSB()
        {
            console.WriteLine(((FOS_System.String)"USB system initialised.        HCIs : ") + Hardware.USB.USBManager.HCIDevices.Count);
            console.WriteLine(((FOS_System.String)"                              UHCIs : ") + Hardware.USB.USBManager.NumUHCIDevices);
            console.WriteLine(((FOS_System.String)"                              OHCIs : ") + Hardware.USB.USBManager.NumOHCIDevices);
            console.WriteLine(((FOS_System.String)"                              EHCIs : ") + Hardware.USB.USBManager.NumEHCIDevices);
            console.WriteLine(((FOS_System.String)"                              xHCIs : ") + Hardware.USB.USBManager.NumxHCIDevices);
            console.WriteLine(((FOS_System.String)"                        USB devices : ") + Hardware.USB.USBManager.Devices.Count);

            int numDrives = 0;
            for (int i = 0; i < Hardware.DeviceManager.Devices.Count; i++)
            {
                Hardware.Device aDevice = (Hardware.Device)Hardware.DeviceManager.Devices[i];

                if (aDevice is Hardware.USB.HCIs.HCI)
                {
                    Hardware.USB.HCIs.HCI hciDevice = (Hardware.USB.HCIs.HCI)aDevice;
                    console.WriteLine();

                    console.Write("--------------------- HCI ");
                    console.Write_AsDecimal(i);
                    console.WriteLine(" ---------------------");

                    FOS_System.String statusText = "";
                    switch (hciDevice.Status)
                    {
                        case Hardware.USB.HCIs.HCI.HCIStatus.Dead:
                            statusText = "Dead";
                            break;
                        case Hardware.USB.HCIs.HCI.HCIStatus.Unset:
                            statusText = "Unset";
                            break;
                        case Hardware.USB.HCIs.HCI.HCIStatus.Active:
                            statusText = "Active";
                            break;
                        default:
                            statusText = "Uncreognised (was a new status type added?)";
                            break;
                    }
                    console.WriteLine("Status: " + statusText);
                }
                else if (aDevice is Hardware.USB.Devices.USBDevice)
                {
                    Hardware.USB.Devices.USBDevice usbDevice = (Hardware.USB.Devices.USBDevice)aDevice;
                    Hardware.USB.Devices.USBDeviceInfo usbDeviceInfo = usbDevice.DeviceInfo;
                    console.WriteLine();

                    console.Write("--------------------- Device ");
                    console.Write_AsDecimal(i);
                    console.WriteLine(" ---------------------");

                    if (aDevice is Hardware.USB.Devices.MassStorageDevice)
                    {
                        console.WriteLine("USB Mass Storage Device found.");
                        Hardware.USB.Devices.MassStorageDevice theMSD = (Hardware.USB.Devices.MassStorageDevice)usbDevice;
                        Hardware.USB.Devices.MassStorageDevice_DiskDevice theMSDDisk = theMSD.diskDevice;

                        console.Write("Disk device num: ");
                        console.WriteLine_AsDecimal(Hardware.DeviceManager.Devices.IndexOf(theMSDDisk));
                        console.WriteLine(((FOS_System.String)"Block Size: ") + theMSDDisk.BlockSize + " bytes");
                        console.WriteLine(((FOS_System.String)"Block Count: ") + theMSDDisk.BlockCount);
                        console.WriteLine(((FOS_System.String)"Size: ") + ((theMSDDisk.BlockCount * theMSDDisk.BlockSize) >> 20) + " MB");

                        numDrives++;
                    }
                    else
                    {
                        console.WriteLine("Unrecognised USB device found.");
                    }

                    console.WriteLine();

                    if (usbDeviceInfo.usbSpec == 0x0100 || usbDeviceInfo.usbSpec == 0x0110 || usbDeviceInfo.usbSpec == 0x0200 || usbDeviceInfo.usbSpec == 0x0201 || usbDeviceInfo.usbSpec == 0x0210 || usbDeviceInfo.usbSpec == 0x0213 || usbDeviceInfo.usbSpec == 0x0300)
                    {
                        console.Write("USB ");
                        console.Write_AsDecimal((usbDeviceInfo.usbSpec >> 8) & 0xFF);
                        console.Write(".");
                        console.WriteLine_AsDecimal(usbDeviceInfo.usbSpec & 0xFF);
                    }
                    else
                    {
                        console.ErrorColour();
                        console.Write("Invalid USB version ");
                        console.Write_AsDecimal((usbDeviceInfo.usbSpec >> 8) & 0xFF);
                        console.Write(".");
                        console.WriteLine_AsDecimal(usbDeviceInfo.usbSpec & 0xFF);
                        console.DefaultColour();
                    }

                    if (usbDeviceInfo.usbClass == 0x09)
                    {
                        switch (usbDeviceInfo.usbProtocol)
                        {
                            case 0:
                                console.WriteLine(" - Full speed USB hub");
                                break;
                            case 1:
                                console.WriteLine(" - Hi-speed USB hub with single TT");
                                break;
                            case 2:
                                console.WriteLine(" - Hi-speed USB hub with multiple TTs");
                                break;
                        }
                    }

                    console.Write("Endpoint 0 mps: ");
                    console.Write_AsDecimal(((Hardware.USB.Endpoint)usbDeviceInfo.Endpoints[0]).MPS);
                    console.WriteLine(" byte.");
                    console.Write("Vendor:            ");
                    console.WriteLine(usbDeviceInfo.vendor);
                    console.Write("Product:           ");
                    console.WriteLine(usbDeviceInfo.product);
                    console.Write("Release number:    ");
                    console.Write_AsDecimal((usbDeviceInfo.releaseNumber >> 8) & 0xFF);
                    console.Write(".");
                    console.WriteLine_AsDecimal(usbDeviceInfo.releaseNumber & 0xFF);
                    console.Write("Manufacturer:      ");
                    console.WriteLine(usbDeviceInfo.ManufacturerString.Value);
                    console.Write("Product:           ");
                    console.WriteLine(usbDeviceInfo.ProductString.Value);
                    console.Write("Serial number:     ");
                    console.WriteLine(usbDeviceInfo.SerialNumberString.Value);
                    console.Write("Number of config.: ");
                    console.WriteLine_AsDecimal(usbDeviceInfo.numConfigurations); // number of possible configurations
                    console.Write("MSDInterfaceNum:   ");
                    console.WriteLine_AsDecimal(usbDeviceInfo.MSD_InterfaceNum);
                    Hardware.Devices.Timer.Default.Wait(1000);
                }
            }
        }
 
        /// <summary>
        /// Outputs the ATA system information.
        /// </summary>
        private void OutputATA()
        {
            int numDrives = 0;
            for (int i = 0; i < Hardware.DeviceManager.Devices.Count; i++)
            {
                Hardware.Device aDevice = (Hardware.Device)Hardware.DeviceManager.Devices[i];
                if (aDevice is Hardware.ATA.PATA)
                {
                    console.WriteLine();
                    console.Write("--------------------- Device ");
                    console.Write_AsDecimal(i);
                    console.WriteLine(" ---------------------");
                    console.WriteLine("Type: PATA");
                    
                    Hardware.ATA.PATA theATA = (Hardware.ATA.PATA)aDevice;
                    console.WriteLine(((FOS_System.String)"Serial No: ") + theATA.SerialNo);
                    console.WriteLine(((FOS_System.String)"Firmware Rev: ") + theATA.FirmwareRev);
                    console.WriteLine(((FOS_System.String)"Model No: ") + theATA.ModelNo);
                    console.WriteLine(((FOS_System.String)"Block Size: ") + theATA.BlockSize + " bytes");
                    console.WriteLine(((FOS_System.String)"Block Count: ") + theATA.BlockCount);
                    console.WriteLine(((FOS_System.String)"Size: ") + ((theATA.BlockCount * theATA.BlockSize) >> 20) + " MB");
                    console.WriteLine(((FOS_System.String)"Max Write Pio Blocks: ") + (theATA.MaxWritePioBlocks));

                    numDrives++;
                }
                else if (aDevice is Hardware.ATA.PATAPI)
                {
                    console.WriteLine();
                    console.Write("--------------------- Device ");
                    console.Write_AsDecimal(i);
                    console.WriteLine(" ---------------------");
                    console.WriteLine("Type: PATAPI");
                    console.WriteLine("Warning: Read-only support.");

                    Hardware.ATA.PATAPI theATA = (Hardware.ATA.PATAPI)aDevice;
                    console.WriteLine(((FOS_System.String)"Serial No: ") + theATA.SerialNo);
                    console.WriteLine(((FOS_System.String)"Firmware Rev: ") + theATA.FirmwareRev);
                    console.WriteLine(((FOS_System.String)"Model No: ") + theATA.ModelNo);
                    console.WriteLine(((FOS_System.String)"Block Size: ") + theATA.BlockSize + " bytes");
                    console.WriteLine(((FOS_System.String)"Block Count: ") + theATA.BlockCount);
                    console.WriteLine(((FOS_System.String)"Size: ") + ((theATA.BlockCount * theATA.BlockSize) >> 20) + " MB");
                    console.WriteLine(((FOS_System.String)"Max Write Pio Blocks: ") + (theATA.MaxWritePioBlocks));
                }
                else if (aDevice is Hardware.ATA.SATA)
                {
                    console.WriteLine();
                    console.Write("--------------------- Device ");
                    console.Write_AsDecimal(i);
                    console.WriteLine(" ---------------------");
                    console.WriteLine("Type: SATA");
                    console.WriteLine("Warning: This disk device type is not supported.");
                }
                else if (aDevice is Hardware.ATA.SATAPI)
                {
                    console.WriteLine();
                    console.Write("--------------------- Device ");
                    console.Write_AsDecimal(i);
                    console.WriteLine(" ---------------------");
                    console.WriteLine("Type: SATAPI");
                    console.WriteLine("Warning: This disk device type is not supported.");
                }
            }

            console.Write("Total # of drives: ");
            console.WriteLine_AsDecimal(numDrives);
        }
 
        /// <summary>
        /// Outputs the PCI system information.
        /// </summary>
        private void OutputPCI()
        {
            for (int i = 0; i < Hardware.PCI.PCI.Devices.Count; i++)
            {
                Hardware.PCI.PCIDevice aDevice = (Hardware.PCI.PCIDevice)Hardware.PCI.PCI.Devices[i];
                console.WriteLine(Hardware.PCI.PCIDevice.DeviceClassInfo.GetString(aDevice));
                console.Write(" - Address: ");
                console.Write(aDevice.bus);
                console.Write(":");
                console.Write(aDevice.slot);
                console.Write(":");
                console.WriteLine(aDevice.function);

                console.Write(" - Vendor Id: ");
                console.WriteLine(aDevice.VendorID);

                console.Write(" - Device Id: ");
                console.WriteLine(aDevice.DeviceID);
            }
        }

        /// <summary>
        /// Initialises the file systems.
        /// </summary>
        private void InitFS()
        {
            console.Write("Initialising file systems...");
            FileSystemManager.Init();
            console.WriteLine("done.");
        }
  
        /// <summary>
        /// Initialises the USB system.
        /// </summary>
        private void InitUSB()
        {
            console.Write("Initialising USB...");
            Hardware.USB.USBManager.Init();
            console.WriteLine("done.");
        }
  
        /// <summary>
        /// Initialises the ATA system.
        /// </summary>
        private void InitATA()
        {
            console.Write("Initialising ATA...");
            Hardware.ATA.ATAManager.Init();
            console.WriteLine("done.");
        }
  
        /// <summary>
        /// Initialises the PCI system.
        /// </summary>
        private void InitPCI()
        {
            console.Write("Initialising PCI...");
            Hardware.PCI.PCI.Init();
            console.WriteLine("done.");
        }

        private static void DeviceManager_DeviceAdded(FOS_System.Object state, Hardware.Device device)
        {
            ((MainShell)state)._DeviceManager_DeviceAdded(device);
        }

        private void _DeviceManager_DeviceAdded(Hardware.Device device)
        {
            if (device is Hardware.Devices.DiskDevice)
            {
                try
                {
                    FileSystemManager.InitDisk((Hardware.Devices.DiskDevice)device);
                    FileSystemManager.InitPartitions();
                }
                catch
                {
                    if (!(ExceptionMethods.CurrentException is FOS_System.Exceptions.NotSupportedException) &&
                        !(ExceptionMethods.CurrentException is Hardware.Exceptions.NoDiskException))
                    {
                        console.ErrorColour();
                        console.WriteLine("Error initialising disk device:");
                        console.WriteLine(ExceptionMethods.CurrentException.Message);
                        console.DefaultColour();
                    }
                }
            }
            else if (device is Hardware.PCI.PCIDeviceNormal)
            {
                try
                {
                    Hardware.USB.USBManager.CheckDeviceForHCI((Hardware.PCI.PCIDeviceNormal)device);
                }
                catch
                {
                    if (!(ExceptionMethods.CurrentException is FOS_System.Exceptions.NotSupportedException))
                    {
                        console.ErrorColour();
                        console.WriteLine("Error initialising PCI device:");
                        console.WriteLine(ExceptionMethods.CurrentException.Message);
                        console.DefaultColour();
                    }
                }
            }
        }

        /// <summary>
        /// Outputs a warning to the user indicating their input was unrecognised.
        /// </summary>
        private void UnrecognisedOption(FOS_System.String value)
        {
            console.WarningColour();
            console.WriteLine("Unrecognised option: " + value);
            console.DefaultColour();
        }

        private unsafe void HeapTest()
        {
            try
            {
                console.WriteLine("Testing heap...");
                uint allocSize = 16;
                while (Heap.GetTotalFreeMem() > 0x100000)
                {
                    byte* val = (byte*)Heap.Alloc(allocSize, "MainShell : HeapTest");
                    if (val == null)
                    {
                        break;
                    }
                    else
                    {
                        for (int i = 0; i < allocSize; i++)
                        {
                            val[i] = 0xFF;
                        }

                        allocSize *= 2;
                        if (allocSize > 4096)
                        {
                            allocSize = 16;
                        }
                    }
                }
                console.WriteLine("Complete without error.");
            }
            catch
            {
                console.WriteLine("Complete with errors.");
                OutputExceptionInfo(ExceptionMethods.CurrentException);
            }
            console.WriteLine("Returning.");
        }

        private unsafe void GCTest()
        {
            try
            {
                console.WriteLine("Testing GC...");
                while (Heap.GetTotalFreeMem() - 0x10000 > 0)
                {
                    FOS_System.GC.NewObj((FOS_System.Type)typeof(FOS_System.Object));
                }
                console.WriteLine("Complete without error.");
            }
            catch
            {
                console.WriteLine("Complete with errors.");
                OutputExceptionInfo(ExceptionMethods.CurrentException);
            }
            console.WriteLine("Returning.");
        }

        private void ATATest()
        {
            new Hardware.Testing.ATATests().Test_LongRead(OutputMessageFromTest, OutputWarningFromTest, OutputErrorFromTest);
        }

        private void USBTest()
        {
            new Hardware.Testing.USBTests().Test_LongRead(OutputMessageFromTest, OutputWarningFromTest, OutputErrorFromTest);
        }

        private static void OutputMessageFromTest(FOS_System.String TestName, FOS_System.String Message)
        {
            Console.Default.WriteLine(TestName + " : " + Message);
        }

        private static void OutputWarningFromTest(FOS_System.String TestName, FOS_System.String Message)
        {
            Console.Default.WarningColour();
            OutputMessageFromTest(TestName, Message);
            Console.Default.DefaultColour();
        }

        private static void OutputErrorFromTest(FOS_System.String TestName, FOS_System.String Message)
        {
            Console.Default.ErrorColour();
            OutputMessageFromTest(TestName, Message);
            Console.Default.DefaultColour();
        }

        /// <summary>
        /// Tests all interrupts in the range 17 to 255 by firing them.
        /// </summary>
        [Drivers.Compiler.Attributes.NoGC]
        private void InterruptsTest()
        {
            for (uint i = 17; i < 256; i++)
            {
                console.WriteLine(((FOS_System.String)"Attempting to invoke interrupt: ") + i);
                Hardware.Interrupts.Interrupts.InvokeInterrupt(i);
            }
        }
  
        /// <summary>
        /// Tests delegates.
        /// </summary>
        [Drivers.Compiler.Attributes.NoGC]
        private void DelegateTest()
        {
            IntDelegate del = CallbackMethod;
            int x = del(new FOS_System.Object());
            if (x == -1)
            {
                console.WriteLine("Delegate return value OK.");
            }
            else
            {
                console.WriteLine("Delegate return value NOT OK!");
            }
            Hardware.Devices.Timer.Default.Wait(1000 * 10);
        }
  
        /// <summary>
        /// Delegate used by delegates test.
        /// </summary>
        /// <param name="data">Test data to pass in.</param>
        /// <returns>A test value.</returns>
        private delegate int IntDelegate(object data);
   
        /// <summary>
        /// Method called by delegates test.
        /// </summary>
        /// <param name="data">Test data to pass in.</param>
        /// <returns>A test value.</returns>
        private int CallbackMethod(object data)
        {
            console.WriteLine("Callback method executed!");
            Hardware.Devices.Timer.Default.Wait(1000 * 10);
            return -1;
        }
    
        /// <summary>
        /// Runs a series of tests on the file system, currently:
        ///  - Finds or creates A:/ drive
        ///  - Attempts to use FAT file system for A drive
        ///  - Finds or creates a folder called "P1D2"
        ///  - Finds or creates short and long name files in "P1D2"
        ///  - Writes and reads from above test files.
        /// </summary>
        private void FileSystemTests()
        {
            try
            {
                FileSystemMapping A_FSMapping = FileSystemManager.GetMapping("A:/");
                if (A_FSMapping != null)
                {
                    FOS_System.IO.FAT.FATFileSystem A_FS = (FOS_System.IO.FAT.FATFileSystem)A_FSMapping.TheFileSystem;

                    Directory P1D2Dir = Directory.Find("A:/P1D2");
                    if (P1D2Dir == null)
                    {
                        console.WriteLine("Creating P1D2 directory...");
                        P1D2Dir = A_FS.NewDirectory("P1D2", A_FS.RootDirectory_FAT32);
                        console.WriteLine("Directory created.");
                    }
                    else
                    {
                        console.WriteLine("Found P1D2 directory.");
                    }

                    console.WriteLine("Finding P1D2 directory...");
                    File longNameTestFile = File.Open("A:/P1D2/LongNameTest.txt");
                    if (longNameTestFile == null)
                    {
                        console.WriteLine("Creating LongNameTest.txt file...");
                        longNameTestFile = P1D2Dir.TheFileSystem.NewFile("LongNameTest.txt", P1D2Dir);
                    }
                    else
                    {
                        console.WriteLine("Found LongNameTest.txt file.");
                    }

                    File shortNameTestFile = File.Open("A:/P1D2/ShrtTest.txt");
                    if (shortNameTestFile == null)
                    {
                        console.WriteLine("Creating ShrtTest.txt file...");
                        shortNameTestFile = P1D2Dir.TheFileSystem.NewFile("ShrtTest.txt", P1D2Dir);
                    }
                    else
                    {
                        console.WriteLine("Found ShrtTest.txt file.");
                    }

                    if (longNameTestFile != null)
                    {
                        console.WriteLine("Opening stream...");
                        FOS_System.IO.Streams.FileStream fileStream = longNameTestFile.GetStream();

                        FOS_System.String testStr = "This is some test file contents.";
                        byte[] testStrBytes = ByteConverter.GetASCIIBytes(testStr);

                        console.WriteLine("Writing data...");
                        fileStream.Position = 0;
                        int size = 0;
                        //for (int i = 0; i < 20; i++)
                        {
                            fileStream.Write(testStrBytes, 0, testStrBytes.Length);
                            size += testStrBytes.Length;
                        }

                        console.WriteLine("Reading data...");
                        fileStream.Position = 0;
                        byte[] readBytes = new byte[size];
                        fileStream.Read(readBytes, 0, readBytes.Length);
                        FOS_System.String readStr = ByteConverter.GetASCIIStringFromASCII(readBytes, 0u, (uint)readBytes.Length);
                        console.WriteLine("\"" + readStr + "\"");

                        OutputDivider();
                    }
                    else
                    {
                        console.WriteLine("LongNameTest2.txt file not found.");
                    }

                    if (shortNameTestFile != null)
                    {
                        console.WriteLine("Opening stream...");
                        FOS_System.IO.Streams.FileStream fileStream = shortNameTestFile.GetStream();

                        FOS_System.String testStr = "This is some test file contents.";
                        byte[] testStrBytes = ByteConverter.GetASCIIBytes(testStr);

                        console.WriteLine("Writing data...");
                        fileStream.Position = 0;
                        uint size = (uint)shortNameTestFile.Size;
                        //for (int i = 0; i < 20; i++)
                        {
                            //fileStream.Write(testStrBytes, 0, testStrBytes.Length);
                            //size += testStrBytes.Length;
                        }

                        console.WriteLine("Reading data...");
                        fileStream.Position = 0;
                        byte[] readBytes = new byte[size];
                        fileStream.Read(readBytes, 0, readBytes.Length);
                        FOS_System.String readStr = ByteConverter.GetASCIIStringFromASCII(readBytes, 0u, (uint)readBytes.Length);
                        console.WriteLine("\"" + readStr + "\"");

                        OutputDivider();
                    }
                    else
                    {
                        console.WriteLine("ShortNameTest.txt file not found.");
                    }
                }
                else
                {
                    console.WriteLine("Could not find \"A:/\" mapping.");
                }


                FileSystemMapping B_FSMapping = FileSystemManager.GetMapping("B:/");
                if (B_FSMapping != null)
                {
                    if (A_FSMapping != null)
                    {
                        File FileToCopy = File.Open("B:/Doc in Root Dir.txt");
                        File shortNameTestFile = File.Open("A:/P1D2/ShrtTest.txt");
                        if (shortNameTestFile != null)
                        {
                            FOS_System.IO.Streams.FileStream FromFileStream = FileToCopy.GetStream();

                            if (shortNameTestFile != null)
                            {
                                FOS_System.IO.Streams.FileStream ToFileStream = shortNameTestFile.GetStream();

                                console.WriteLine("Copying data...");

                                FromFileStream.Position = 0;
                                byte[] readBytes = new byte[(uint)FileToCopy.Size];
                                FromFileStream.Read(readBytes, 0, readBytes.Length);
                                FOS_System.String readStr = ByteConverter.GetASCIIStringFromASCII(readBytes, 0u, (uint)readBytes.Length);
                                console.WriteLine("\"" + readStr + "\"");

                                ToFileStream.Position = 0;
                                ToFileStream.Write(readBytes, 0, readBytes.Length);

                                console.WriteLine("Copied!");
                                console.WriteLine("Reading back data from target file...");

                                ToFileStream.Position = 0;
                                readBytes = new byte[(uint)FileToCopy.Size];
                                ToFileStream.Read(readBytes, 0, readBytes.Length);
                                readStr = ByteConverter.GetASCIIStringFromASCII(readBytes, 0u, (uint)readBytes.Length);
                                console.WriteLine("\"" + readStr + "\"");
                            }
                            else
                            {
                                console.WriteLine("Could not find file to copy to!");
                            }
                        }
                        else
                        {
                            console.WriteLine("Could not find file to copy!");
                        }
                    }
                    else
                    {
                        console.WriteLine("\"B:/\" mapping found but no \"A:/\" mapping!");
                    }
                }
                else
                {
                    console.WriteLine("Could not find \"B:/\" mapping.");
                }
            }
            catch
            {
                console.WarningColour();
                console.WriteLine(ExceptionMethods.CurrentException.Message);
                console.DefaultColour();
            }
        }
  
        /// <summary>
        /// Tests unsigned less-than comparison of ulongs.
        /// </summary>
        private void ULongLTComparisonTest()
        {
            ulong x = 0x01;
            ulong y = 0x20000000000;
            bool c = x < y;
            if (!c)
            {
                console.WriteLine("Test 1 failed.");
            }
            x = 0x01000000000;
            c = x < y;
            if (!c)
            {
                console.WriteLine("Test 2 failed.");
            }
            x = 0x20000000000;
            c = x < y;
            if (c)
            {
                console.WriteLine("Test 3 failed.");
            }
            Hardware.Devices.Timer.Default.Wait(1000 * 10);
        }
    
        /// <summary>
        /// Tests multiplying two 64-bit numbers together
        /// </summary>
        private void ULongMultiplicationTest()
        {
            ulong a = 0x00000000UL;
            ulong b = 0x00000000UL;
            ulong c = a * b;
            bool test1OK = c == 0x0UL;

            a = 0x00000001UL;
            b = 0x00000001UL;
            c = a * b;
            bool test2OK = c == 0x1;

            a = 0x00000010UL;
            b = 0x00000010UL;
            c = a * b;
            bool test3OK = c == 0x100UL;

            a = 0x10000000UL;
            b = 0x00000010UL;
            c = a * b;
            bool test4OK = c == 0x100000000UL;

            a = 0x100000000UL;
            b = 0x00000011UL;
            c = a * b;
            bool test5OK = c == 0x1100000000UL;

            a = 0x100000000UL;
            b = 0x100000000UL;
            c = a * b;
            bool test6OK = c == 0x0UL;

            console.WriteLine(((FOS_System.String)"Tests OK: ") + test1OK + ", " + test2OK +
                                                                ", " + test3OK + ", " + test4OK +
                                                                ", " + test5OK + ", " + test6OK);
            Hardware.Devices.Timer.Default.Wait(1000 * 10);
        }
    
        /// <summary>
        /// Tests dynamic string creation and string concatentation.
        /// </summary>
        private void StringConcatTest()
        {
            console.WriteLine("String concat test...");

            try
            {
                FOS_System.String testStr = FOS_System.String.Concat("test1", " test2");
                console.WriteLine(testStr);
            }
            catch
            {
                console.WarningColour();
                console.WriteLine(ExceptionMethods.CurrentException.Message);
                console.DefaultColour();
            }

            console.WriteLine("End string concat test.");
        }
  
        /// <summary>
        /// Tests creating arrays where elements are reference-type and Gc managed.
        /// </summary>
        private void ObjectArrayTest()
        {
            console.WriteLine("Object array test...");

            try
            {
                FOS_System.Object[] objArr = new FOS_System.Object[10];
                objArr[0] = new FOS_System.Object();
                objArr[0]._Type.Size = 5;
                if (objArr[0] != null)
                {
                    console.WriteLine("Set object in array success!");
                }
            }
            catch
            {
                console.WarningColour();
                console.WriteLine(ExceptionMethods.CurrentException.Message);
                console.DefaultColour();
            }

            console.WriteLine("End object array test.");
        }
  
        /// <summary>
        /// Tests creating an array of integers (element of type from MSCorLib type and value type)
        /// </summary>
        private void IntArrayTest()
        {
            console.WriteLine("Int array test...");

            try
            {
                int[] testArray = new int[1024];
                testArray[5] = 10;
                int q = testArray[5];
            }
            catch
            {
                console.WarningColour();
                console.WriteLine(ExceptionMethods.CurrentException.Message);
                console.DefaultColour();
            }

            console.WriteLine("Int array test.");
        }
  
        /// <summary>
        /// Tests creating  GC-managed reference-type object and  setting properties and enums.
        /// </summary>
        private void DummyObjectTest()
        {
            console.WriteLine("Dummy object test...");

            try
            {
                Dummy obj = new Dummy();
                new Dummy();
                obj = new Dummy();
                obj.x = obj.x + obj.y;
                if (obj.x == 21)
                {
                    console.WriteLine("Addition success!");
                }

                if (obj.testEnum == Dummy.TestEnum.First)
                {
                    console.WriteLine("TestEnum.First pre-assigned.");
                }
                obj.testEnum = Dummy.TestEnum.Second;
                if (obj.testEnum == Dummy.TestEnum.Second)
                {
                    console.WriteLine("TestEnum.Second assignment worked.");
                }
            }
            catch
            {
                console.WarningColour();
                console.WriteLine(ExceptionMethods.CurrentException.Message);
                console.DefaultColour();
            }

            console.WriteLine("Dummy object test.");
            //console.WriteLine("Dummy object test disabled.");
        }
   
        /// <summary>
        /// Tests managed exception sub-system by deliberately causing hardware-level divide-by-zero exception.
        /// </summary>
        private void DivideByZeroTest()
        {
            console.WriteLine("Divide by zero test...");

            try
            {
                int x = 0;
                int y = 0;
                int z = 0;
                z = x / y;
            }
            catch
            {
                console.WarningColour();
                console.WriteLine(ExceptionMethods.CurrentException.Message);
                console.DefaultColour();

                if (ExceptionMethods.CurrentException is FOS_System.Exceptions.DivideByZeroException)
                {
                    console.WriteLine("Handled divide by zero exception.");
                }
            }

            console.WriteLine("Divide by zero test.");
        }
  
        /// <summary>
        /// Tests the exception handling sub-system.
        /// </summary>
        /// <remarks>
        /// If the mechanism appears to work but code in Main() stops working then
        /// it is because one of the GC methods is calling a method / get-set property
        /// that is not marked with [Comnpiler.NoGC]. Make sure all methods that the 
        /// GC calls are marked with [Compiler.NoGC] attribute. See example.
        /// </remarks>
        /// <example>
        /// public int x
        /// {
        ///        ///     get
        ///     {
        ///         return 0;
        ///     }
        /// }
        /// </example>
        private void ExceptionsTestP1()
        {
            ExceptionsTestP2();
        }
  
        /// <summary>
        /// Secondary method used in testing the exception handling sub-system.
        /// </summary>
        private void ExceptionsTestP2()
        {
            FOS_System.Object obj = new FOS_System.Object();

            try
            {
                ExceptionMethods.Throw(new FOS_System.Exception("An inner exception."));
            }
            finally
            {
                console.WriteLine("Finally ran.");
                ExceptionMethods.Throw(new FOS_System.Exception("An outer exception."));
            }
        }
   
        /// <summary>
        /// Tests the PC speaker beep feature (part of the PIT).
        /// </summary>
        private void PCBeepTest()
        {
            console.WriteLine("Running PC Beep test...");

            try
            {
                console.WriteLine("Enabling beep...");
                Hardware.Timers.PIT.ThePIT.PlaySound(247); //261 ~ B3
                console.WriteLine("Beep enabled. Waiting 10s...");
                Hardware.Devices.Timer.Default.Wait(10000);
                console.WriteLine("Wait finished. Muting beep...");
                Hardware.Timers.PIT.ThePIT.MuteSound();
                console.WriteLine("Muted beep.");
            }
            catch
            {
                OutputExceptionInfo(ExceptionMethods.CurrentException);
            }

            console.WriteLine("Ended PC Beep test.");
        }
  
        /// <summary>
        /// Tests the default timer device.
        /// </summary>
        private void TimerTest()
        {
            console.WriteLine("Running PIT test...");

            try
            {
                console.Write("Waiting for 5 lot(s) of 1 second(s)");
                for (int i = 0; i < 5; i++)
                {
                    Hardware.Devices.Timer.Default.Wait(1000);
                    console.Write(".");
                }
                console.WriteLine("completed.");


                console.Write("Waiting for 1 lot(s) of 5 second(s)");
                //for (int i = 0; i < 5; i++)
                {
                    Hardware.Devices.Timer.Default.Wait(5000);
                    console.Write(".");
                }
                console.WriteLine("completed.");
            }
            catch
            {
                OutputExceptionInfo(ExceptionMethods.CurrentException);
            }

            console.WriteLine("Ended PIT test.");
        }
  
        /// <summary>
        /// Tests the default keyboard device.
        /// </summary>
        private void KeyboardTest()
        {
            try
            {
                console.WriteLine("Running PS2 Keyboard test. Type for a bit, eventually it will end (if the keyboard works that is)...");

                int charsPrinted = 0;
                char c;
                for (int i = 0; i < 240; i++)
                {
                    c = Hardware.Devices.Keyboard.Default.ReadChar();
                    if (c != '\0')
                    {
                        charsPrinted++;
                        if (charsPrinted % 80 == 0)
                        {
                            console.WriteLine(c);
                        }
                        else
                        {
                            console.Write(c);
                        }
                    }
                    else
                    {
                        console.WriteLine();
                        console.WarningColour();
                        console.WriteLine("Undisplayable key pressed.");
                        console.DefaultColour();
                    }
                }

                console.WriteLine();
            }
            catch
            {
                OutputExceptionInfo(ExceptionMethods.CurrentException);
            }

            console.WriteLine();
            console.WriteLine("Ended keyboard test.");
        }
  
        /// <summary>
        /// Tests the advanced console class.
        /// </summary>
        private void AdvancedConsoleTest()
        {
            console.WriteLine("Starting advanced console test.");

            try
            {
                Console.InitDefault();

                Console.Default.Beep();
                Console.Default.WriteLine("Test write line.");
                Console.Default.WriteLine("Please write a line: ");
                FOS_System.String line = ReadLine();
                Console.Default.WriteLine("Your wrote: " + line);

                Console.Default.WriteLine("Pausing for 2 seconds...");
                Hardware.Devices.Timer.Default.Wait(2000);

                for (int i = 0; i < 25; i++)
                {
                    Console.Default.Write("Line ");
                    Console.Default.WriteLine_AsDecimal(i);
                }

                Console.Default.WriteLine("Testing scrolling...");
                for (int i = 0; i < 25; i++)
                {
                    Hardware.Devices.Timer.Default.Wait(500);
                    Console.Default.Scroll(-1);
                }
                Console.Default.Scroll(25);

                Console.Default.WriteLine("Scroll test done.");

                Console.Default.WriteLine("Testing Clear and Colour...");
                Console.Default.Clear();
                Console.Default.WarningColour();
                Console.Default.WriteLine("Warning colour test.");
                Console.Default.ErrorColour();
                Console.Default.WriteLine("Error colour test.");
                Console.Default.DefaultColour();
                Console.Default.WriteLine("Default colour test.");
            }
            catch
            {
                OutputExceptionInfo(ExceptionMethods.CurrentException);
            }

            console.WriteLine("Ended advanced console test. Pausing for 5 seconds.");
            Hardware.Devices.Timer.Default.Wait(5000);
        }
  
        /// <summary>
        /// Tests the fields type table by outputting to the screen and 
        /// letting the user decide if the output is correct.
        /// </summary>
        private unsafe void FieldsTableTest()
        {
            console.WriteLine("Starting fields table test.");

            try
            {
                FOS_System.Type theType = (FOS_System.Type)typeof(FOS_System.Type);
                if (theType == null)
                {
                    console.WriteLine("The type object is null!!");
                }
                else
                {
                    console.WriteLine("Type signature: " + theType.Signature);
                    console.WriteLine("Type id: " + theType.IdString);

                    FOS_System.Type baseType = theType.TheBaseType;
                    int inset = 0;
                    while(baseType != null)
                    {
                        console.WriteLine(baseType.Signature.PadLeft(baseType.Signature.length + inset, ' '));
                        inset += 4;
                        baseType = baseType.TheBaseType;
                    }

                    FieldInfo* fieldInfoPtr = theType.FieldTablePtr;
                    while (fieldInfoPtr->Size != 0)
                    {
                        FOS_System.Type fieldType = (FOS_System.Type)Utilities.ObjectUtilities.GetObject(fieldInfoPtr->FieldType);

                        try
                        {
                            console.Write("Field: " + fieldType.Signature + "\n @ Offset=");
                            console.Write_AsDecimal(fieldInfoPtr->Offset);
                            console.Write(", Size=");
                            console.Write_AsDecimal(fieldInfoPtr->Size);
                            console.Write(", Value type?: ");
                            console.Write(fieldType.IsValueType);
                            console.Write(", Pointer type?: ");
                            console.WriteLine(fieldType.IsPointer);
                        }
                        catch
                        {
                            console.WriteLine("Error printing field info.");
                        }

                        fieldInfoPtr++;
                    }
                    console.Write("Parent?: ");
                    console.WriteLine(fieldInfoPtr->FieldType != null);
                }
            }
            catch
            {
                OutputExceptionInfo(ExceptionMethods.CurrentException);
            }

            console.WriteLine("Ended fields table test. Pausing for 5 seconds.");
            Hardware.Devices.Timer.Default.Wait(5000);
        }

        /// <summary>
        /// Tests the "is" operator (i.e. the IsInst IL op)
        /// </summary>
        private void IsInstTest()
        {
            console.WriteLine("Starting IsInst test.");

            try
            {
                FOS_System.Object anStr = (FOS_System.String)"test";
                if (anStr is FOS_System.String)
                {
                    console.WriteLine("Is a String.");
                }
                else
                {
                    console.WriteLine("Is not a String.");
                }
                if (anStr is FOS_System.Object)
                {
                    console.WriteLine("Is an Object.");
                }
                else
                {
                    console.WriteLine("Is not an Object.");
                }
                if (anStr is FOS_System.Exception)
                {
                    console.WriteLine("Is an Exception.");
                }
                else
                {
                    console.WriteLine("Is not an Exception.");
                }
            }
            catch
            {
                OutputExceptionInfo(ExceptionMethods.CurrentException);
            }

            console.WriteLine("Ended IsInst test. Pausing for 5 seconds.");
            Hardware.Devices.Timer.Default.Wait(5000);
        }

        private void LongsTest()
        {
            console.WriteLine("Starting Longs test...");
            try
            {
                byte passColour = 0x2;

                console.WriteLine("Unsigned tests...");
                console.Colour(passColour);
                #region Unsigned

                #region Addition & Subtraction
                {
                    ulong x = 0xEFFFFFFFFFFFFFFF;
                    ulong y = 1;
                    ulong z = x + y;
                    if (z == 0xF000000000000000)
                    {
                        console.WriteLine("Pass: z == 0xF000000000000000");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0xF000000000000000");
                        console.Colour(passColour);
                    }
                }
                {
                    ulong x = 0xF000000000000000;
                    ulong y = 1;
                    ulong z = x - y;
                    if (z == 0xEFFFFFFFFFFFFFFF)
                    {
                        console.WriteLine("Pass: z == 0xEFFFFFFFFFFFFFFF");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0xEFFFFFFFFFFFFFFF");
                        console.Colour(passColour);
                    }
                }
                #endregion

                #region Multiplication, (DISABLED - division) operators
                {
                    ulong x = 0x7000000000000082;
                    ulong y = 2;
                    ulong z = x * y;
                    if (z == 0xE000000000000104)
                    {
                        console.WriteLine("Pass: z == 0xE000000000000104");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0xE000000000000104");
                        console.Colour(passColour);
                    }
                }
                //{
                //    ulong x = 0xE000000000000104;
                //    ulong y = 2;
                //    ulong z = x / y;
                //    if (z == 0x7000000000000082)
                //    {
                //        console.WriteLine("Pass: z == 0x7000000000000082");
                //    }
                //    else
                //    {
                //        console.ErrorColour();
                //        console.WriteLine("Fail: z != 0x7000000000000082");
                //        console.Colour(passColour);
                //    }
                //}
                #endregion

                #region DISABLED - Modulo (%) operator
                //{
                //    ulong x = 0x7000000000000000;
                //    ulong y = 5;
                //    ulong z = x % y;
                //    if (z == 2)
                //    {
                //        console.WriteLine("Pass: z == 2");
                //    }
                //    else
                //    {
                //        console.ErrorColour();
                //        console.WriteLine("Fail: z != 2");
                //        console.Colour(passColour);
                //    }
                //}
                #endregion

                #region Comparison operators (==, !=, >, <, >=, <=)
                {
                    ulong x = 0xDEADBEEFDEADBEEF;
                    ulong y = 0xDEADBEEFDEADBEEF;
                    if (x == y)
                    {
                        console.WriteLine("Pass: x == y == 0xDEADBEEFDEADBEEF");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: x != y != 0xDEADBEEFDEADBEEF");
                        console.Colour(passColour);
                    }
                }
                {
                    ulong x = 0xDEADBEEFDEADBEEF;
                    ulong y = 0xFEADBEEFDEADBEEF;
                    if (x != y)
                    {
                        console.WriteLine("Pass: x != y (0xDEADBEEFDEADBEEF != 0xFEADBEEFDEADBEEF)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: x == y (0xDEADBEEFDEADBEEF == 0xFEADBEEFDEADBEEF)");
                        console.Colour(passColour);
                    }
                }
                {
                    ulong x = 0xDEADBEEFDEADBEEF;
                    ulong y = 0xFEADBEEFDEADBEEF;
                    if (x < y)
                    {
                        console.WriteLine("Pass: x < y (0xDEADBEEFDEADBEEF < 0xFEADBEEFDEADBEEF)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: x >= y (0xDEADBEEFDEADBEEF >= 0xFEADBEEFDEADBEEF)");
                        console.Colour(passColour);
                    }
                }
                {
                    ulong x = 0xFFADBEEFDEADBEEF;
                    ulong y = 0xFEADBEEFDEADBEEF;
                    if (x > y)
                    {
                        console.WriteLine("Pass: x > y (0xFFADBEEFDEADBEEF < 0xFEADBEEFDEADBEEF)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: x <= y (0xFFADBEEFDEADBEEF >= 0xFEADBEEFDEADBEEF)");
                        console.Colour(passColour);
                    }
                }
                {
                    ulong x = 0xDEADBEEFDEADBEEF;
                    ulong y = 0xDEADBEEFDEADBEEF;
                    if (x <= y)
                    {
                        console.WriteLine("Pass: x <= y (0xDEADBEEFDEADBEEF)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: x > y (0xDEADBEEFDEADBEEF)");
                        console.Colour(passColour);
                    }
                }
                {
                    ulong x = 0xAEADBEEFDEADBEEF;
                    ulong y = 0xDEADBEEFDEADBEEF;
                    if (x <= y)
                    {
                        console.WriteLine("Pass: x <= y (0xAEADBEEFDEADBEEF)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: x > y (0xAEADBEEFDEADBEEF)");
                        console.Colour(passColour);
                    }
                }
                {
                    ulong x = 0xDEADBEEFDEADBEEF;
                    ulong y = 0xDEADBEEFDEADBEEF;
                    if (x >= y)
                    {
                        console.WriteLine("Pass: x >= y (0xDEADBEEFDEADBEEF)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: x < y (0xDEADBEEFDEADBEEF)");
                        console.Colour(passColour);
                    }
                }
                {
                    ulong x = 0xDEADBEEFDEADBEEF;
                    ulong y = 0xAEADBEEFDEADBEEF;
                    if (x >= y)
                    {
                        console.WriteLine("Pass: x >= y (0xAEADBEEFDEADBEEF)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: x < y (0xAEADBEEFDEADBEEF)");
                        console.Colour(passColour);
                    }
                }
                #endregion
                
                #region Shift operators (<<, >>) by less and more than 32 bits and 64 bits
                {
                    ulong x = 0x2;
                    int y = 2;
                    ulong z = x << y;
                    if (z == 0x8)
                    {
                        console.WriteLine("Pass: z == 8 (<<)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 8 (<<)");
                        console.Colour(passColour);
                    }
                }
                {
                    ulong x = 0x2;
                    int y = 62;
                    ulong z = x << y;
                    if (z == 0x8000000000000000)
                    {
                        console.WriteLine("Pass: z == 0x8000000000000000 (<<)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0x8000000000000000 (<<)");
                        console.Colour(passColour);
                    }
                }
                {
                    ulong x = 0x8;
                    int y = 2;
                    ulong z = x >> y;
                    if (z == 0x2)
                    {
                        console.WriteLine("Pass: z == 2 (>>)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 2 (>>)");
                        console.Colour(passColour);
                    }
                }
                {
                    ulong x = 0x8000000000000000;
                    int y = 62;
                    ulong z = x >> y;
                    if (z == 0x2)
                    {
                        console.WriteLine("Pass: z == 0x2 (>>)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0x2 (>>)");
                        console.Colour(passColour);
                    }
                }
                #endregion
                
                #region Bitwise operators (&, |, ^, ~) 
                {
                    ulong x = 0xFFFFFFFFFFFFFFFF;
                    ulong y = 0xF0FDFFF0FFFDFFFF;
                    ulong z = x & y;
                    if (z == 0xF0FDFFF0FFFDFFFF)
                    {
                        console.WriteLine("Pass: z == 0xF0FDFFF0FFFDFFFF (&)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0xF0FDFFF0FFFDFFFF (&)");
                        console.Colour(passColour);
                    }
                }
                {
                    ulong x = 0xF1000002000F0000;
                    ulong y = 0xF0FDFFF0FFFDFFFF;
                    ulong z = x | y;
                    if (z == 0xF1FDFFF2FFFFFFFF)
                    {
                        console.WriteLine("Pass: z == 0xF1FDFFF2FFFFFFFF (|)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0xF1FDFFF2FFFFFFFF (|)");
                        console.Colour(passColour);
                    }
                }
                {
                    ulong x = 0xFFFFFFFFFFFFFFFF;
                    ulong y = 0xF0FDFFF0FFFDFFFF;
                    ulong z = x ^ y;
                    if (z == 0x0F02000F00020000)
                    {
                        console.WriteLine("Pass: z == 0x0F02000F00020000 (^)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0x0F02000F00020000 (^)");
                        console.Colour(passColour);
                    }
                }
                {
                    ulong x = 0x8FFFFFFFFFFFF8FF;
                    ulong z = ~x;
                    if (z == 0x7000000000000700)
                    {
                        console.WriteLine("Pass: z == 0x7000000000000700 (~)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0x7000000000000700 (~)");
                        console.Colour(passColour);
                    }
                }
                #endregion

                #region Cast to FOS_System.String
                {
                    ulong x = 0x8FFFFFFFFFFFF8FF;
                    FOS_System.String casted = (FOS_System.String)x;
                    FOS_System.String expected = "0x8FFFFFFF 0xFFFFF8FF";
                    if (casted == expected)
                    {
                        console.WriteLine("Pass: casted == expected");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: casted != expected (" + casted + " != " + expected);
                        console.Colour(passColour);
                    }
                }
                #endregion

                #endregion
                console.DefaultColour();
                console.WriteLine("Unsigned tests ended.");

                console.WriteLine("Signed tests...");
                console.Colour(passColour);
                #region Signed

                #region Addition & Subtraction
                {
                    long x = 0x6FFFFFFFFFFFFFFF;
                    long y = 1;
                    long z = x + y;
                    if (z == 0x7000000000000000)
                    {
                        console.WriteLine("Pass: z == 0x7000000000000000");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0x7000000000000000");
                        console.Colour(passColour);
                    }
                }
                {
                    long x = 0x7000000000000000;
                    long y = 1;
                    long z = x - y;
                    if (z == 0x6FFFFFFFFFFFFFFF)
                    {
                        console.WriteLine("Pass: z == 0x6FFFFFFFFFFFFFFF");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0x6FFFFFFFFFFFFFFF");
                        console.Colour(passColour);
                    }
                }
                {
                    long x = -1;
                    long y = 1;
                    long z = x + y;
                    if (z == 0)
                    {
                        console.WriteLine("Pass: z == 0");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0");
                        console.Colour(passColour);
                    }
                }
                {
                    long x = -9223372036854775808;
                    long y = 0x0F00000000000000;
                    long z = x + y;
                    if (z == -8142508126285856768)
                    {
                        console.WriteLine("Pass: z == -8142508126285856768");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != -8142508126285856768");
                        console.Colour(passColour);
                    }
                }
                {
                    long x = -9223372036854775808;
                    long y = 0x7FFFFFFFFFFFFFFF;
                    long z = x + y;
                    if (z == -1)
                    {
                        console.WriteLine("Pass: z == -1");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != -1");
                        console.Colour(passColour);
                    }
                }
                {
                    long x = -1;
                    long y = 0x7FFFFFFFFFFFFFFF;
                    long z = x + y;
                    if (z == 0x7FFFFFFFFFFFFFFE)
                    {
                        console.WriteLine("Pass: z == 0x7FFFFFFFFFFFFFFE");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0x7FFFFFFFFFFFFFFE");
                        console.Colour(passColour);
                    }
                }
                #endregion

                #region Multiplication, (DISABLED - division) operators
                {
                    long x = 0x20FFFFFFFFFFFFFF;
                    long y = 2;
                    long z = x * y;
                    if (z == 0x41FFFFFFFFFFFFFE)
                    {
                        console.WriteLine("Pass: z == 0x41FFFFFFFFFFFFFE");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0x41FFFFFFFFFFFFFE");
                        console.Colour(passColour);
                    }
                }
                //{
                //    long x = 0x6066666666666666;
                //    long y = 2;
                //    long z = x / y;
                //    if (z == 0x3033333333333333)
                //    {
                //        console.WriteLine("Pass: z == 0x3033333333333333");
                //    }
                //    else
                //    {
                //        console.ErrorColour();
                //        console.WriteLine("Fail: z != 0x3033333333333333");
                //        console.Colour(passColour);
                //    }
                //}
                #endregion

                #region DISABLED - Modulo (%) operator
                //{
                //    long x = 0x7000000000000000;
                //    long y = 5;
                //    long z = x % y;
                //    if (z == 2)
                //    {
                //        console.WriteLine("Pass: z == 2");
                //    }
                //    else
                //    {
                //        console.ErrorColour();
                //        console.WriteLine("Fail: z != 2");
                //        console.Colour(passColour);
                //    }
                //}
                #endregion

                #region Comparison operators (==, !=, >, <, >=, <=)
                {
                    long x = 0x6EADBEEFDEADBEEF;
                    long y = 0x6EADBEEFDEADBEEF;
                    if (x == y)
                    {
                        console.WriteLine("Pass: x == y == 0x6EADBEEFDEADBEEF");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: x != y != 0x6EADBEEFDEADBEEF");
                        console.Colour(passColour);
                    }
                }
                {
                    long x = 0x5EADBEEFDEADBEEF;
                    long y = 0x6EADBEEFDEADBEEF;
                    if (x != y)
                    {
                        console.WriteLine("Pass: x != y (0x5EADBEEFDEADBEEF != 0x6EADBEEFDEADBEEF)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: x == y (0x5EADBEEFDEADBEEF == 0x6EADBEEFDEADBEEF)");
                        console.Colour(passColour);
                    }
                }
                {
                    long x = 0x5EADBEEFDEADBEEF;
                    long y = 0x6EADBEEFDEADBEEF;
                    if (x < y)
                    {
                        console.WriteLine("Pass: x < y (0x5EADBEEFDEADBEEF < 0x6EADBEEFDEADBEEF)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: x >= y (0x5EADBEEFDEADBEEF >= 0x6EADBEEFDEADBEEF)");
                        console.Colour(passColour);
                    }
                }
                {
                    long x = 0x6FADBEEFDEADBEEF;
                    long y = 0x6EADBEEFDEADBEEF;
                    if (x > y)
                    {
                        console.WriteLine("Pass: x > y (0x6FADBEEFDEADBEEF < 0x6EADBEEFDEADBEEF)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: x <= y (0x6FADBEEFDEADBEEF >= 0x6EADBEEFDEADBEEF)");
                        console.Colour(passColour);
                    }
                }
                {
                    long x = 0x6EADBEEFDEADBEEF;
                    long y = 0x6EADBEEFDEADBEEF;
                    if (x <= y)
                    {
                        console.WriteLine("Pass: x <= y (0x6EADBEEFDEADBEEF)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: x > y (0x6EADBEEFDEADBEEF)");
                        console.Colour(passColour);
                    }
                }
                {
                    long x = 0x3EADBEEFDEADBEEF;
                    long y = 0x6EADBEEFDEADBEEF;
                    if (x <= y)
                    {
                        console.WriteLine("Pass: x <= y (0x3EADBEEFDEADBEEF)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: x > y (0x3EADBEEFDEADBEEF)");
                        console.Colour(passColour);
                    }
                }
                {
                    ulong x = 0x6EADBEEFDEADBEEF;
                    ulong y = 0x6EADBEEFDEADBEEF;
                    if (x >= y)
                    {
                        console.WriteLine("Pass: x >= y (0x6EADBEEFDEADBEEF)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: x < y (0x6EADBEEFDEADBEEF)");
                        console.Colour(passColour);
                    }
                }
                {
                    ulong x = 0x6EADBEEFDEADBEEF;
                    ulong y = 0x3EADBEEFDEADBEEF;
                    if (x >= y)
                    {
                        console.WriteLine("Pass: x >= y (0x3EADBEEFDEADBEEF)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: x < y (0x3EADBEEFDEADBEEF)");
                        console.Colour(passColour);
                    }
                }
                #endregion

                #region Shift operators (<<, >>) by less and more than 32 bits and 64 bits
                {
                    long x = 0x2;
                    int y = 2;
                    long z = x << y;
                    if (z == 0x8)
                    {
                        console.WriteLine("Pass: z == 8 (<<)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 8 (<<)");
                        console.Colour(passColour);
                    }
                }
                {
                    long x = 0x2;
                    int y = 61;
                    long z = x << y;
                    if (z == 0x4000000000000000)
                    {
                        console.WriteLine("Pass: z == 0x4000000000000000 (<<)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0x4000000000000000 (<<)");
                        console.Colour(passColour);
                    }
                }
                {
                    long x = 0x8;
                    int y = 2;
                    long z = x >> y;
                    if (z == 0x2)
                    {
                        console.WriteLine("Pass: z == 2 (>>)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 2 (>>)");
                        console.Colour(passColour);
                    }
                }
                {
                    long x = 0x4000000000000000;
                    int y = 61;
                    long z = x >> y;
                    if (z == 0x2)
                    {
                        console.WriteLine("Pass: z == 0x2 (>>)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0x2 (>>)");
                        console.Colour(passColour);
                    }
                }
                #endregion

                #region Bitwise operators (&, |, ^, ~)
                {
                    long x = 0x0FFFFFFFFFFFFFFF;
                    long y = 0x00FDFFF0FFFDFFFF;
                    long z = x & y;
                    if (z == 0x00FDFFF0FFFDFFFF)
                    {
                        console.WriteLine("Pass: z == 0x00FDFFF0FFFDFFFF (&)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0x00FDFFF0FFFDFFFF (&)");
                        console.Colour(passColour);
                    }
                }
                {
                    long x = 0x71000002000F0000;
                    long y = 0x70FDFFF0FFFDFFFF;
                    long z = x | y;
                    if (z == 0x71FDFFF2FFFFFFFF)
                    {
                        console.WriteLine("Pass: z == 0x71FDFFF2FFFFFFFF (|)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0x71FDFFF2FFFFFFFF (|)");
                        console.Colour(passColour);
                    }
                }
                {
                    long x = 0x0FFFFFFFFFFFFFFF;
                    long y = 0x00FDFFF0FFFDFFFF;
                    long z = x ^ y;
                    if (z == 0x0F02000F00020000)
                    {
                        console.WriteLine("Pass: z == 0x0F02000F00020000 (^)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0x0F02000F00020000 (^)");
                        console.Colour(passColour);
                    }
                }
                {
                    long x = -8070450532247930625;
                    long z = ~x;
                    if (z == 0x7000000000000700)
                    {
                        console.WriteLine("Pass: z == 0x7000000000000700 (~)");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: z != 0x7000000000000700 (~)");
                        console.Colour(passColour);
                    }
                }
                #endregion

                #region Cast to FOS_System.String
                {
                    long x = -8070450532247930625;
                    FOS_System.String casted = (FOS_System.String)x;
                    FOS_System.String expected = "0x8FFFFFFF 0xFFFFF8FF";
                    if (casted == expected)
                    {
                        console.WriteLine("Pass: casted == expected");
                    }
                    else
                    {
                        console.ErrorColour();
                        console.WriteLine("Fail: casted != expected (" + casted + " != " + expected);
                        console.Colour(passColour);
                    }
                }
                #endregion

                #endregion
                console.DefaultColour();
                console.WriteLine("Signed tests ended.");
            }
            catch
            {
                OutputExceptionInfo(ExceptionMethods.CurrentException);
            }
            console.WriteLine("Finished Longs test.");
        }
        private void ThreadSleepTest()
        {
            console.WriteLine("Running Thread Sleep test...");

            try
            {
                console.Write("Sleeping for 5 lot(s) of 1 second(s)");
                for (int i = 0; i < 5; i++)
                {
                    Processes.SystemCalls.SleepThread(1000);
                    console.Write(".");
                }
                console.WriteLine("completed.");


                console.Write("Sleeping for 1 lot(s) of 5 second(s)");
                //for (int i = 0; i < 5; i++)
                {
                    Processes.SystemCalls.SleepThread(5000);
                    console.Write(".");
                }
                console.WriteLine("completed.");
            }
            catch
            {
                OutputExceptionInfo(ExceptionMethods.CurrentException);
            }

            console.WriteLine("Ended Thread Sleep test.");
        }
    }

    /// <summary>
    /// Dummy class used in Dummy Object test.
    /// </summary>
    public class Dummy : FOS_System.Object
    {
        /// <summary>
        /// Test enumeration.
        /// </summary>
        public enum TestEnum
        {
            /// <summary>
            /// A test value.
            /// </summary>
            First = 1,
            /// <summary>
            /// A test value.
            /// </summary>
            Second = 2,
            /// <summary>
            /// A test value.
            /// </summary>
            Third = 3,
            /// <summary>
            /// A test value.
            /// </summary>
            NULL = 0
        }

        /// <summary>
        /// Test field.
        /// </summary>
        public TestEnum testEnum = TestEnum.First;

        /// <summary>
        /// Test field.
        /// </summary>
        public int x = 10;
        /// <summary>
        /// Test field.
        /// </summary>
        public int y = 11;

        /// <summary>
        /// Test method.
        /// </summary>
        /// <returns>x + y</returns>
        public int Add()
        {
            return x + y;
        }
    }
}
