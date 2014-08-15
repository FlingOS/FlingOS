#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
using System;
using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.IO;

namespace Kernel.Core.Shells
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

        /// <summary>
        /// See base class.
        /// </summary>
        public override void Execute()
        {
            try
            {
                while(!terminating)
                {
                    try
                    {
                        console.Write(CurrentDir + " > ");

                        /* Command { Req Arg } [Opt Arg]:
                      /   *  - Halt
                      /   *  - ExInfo
                      /   *  - Init { ALL/PCI/ATA/USB/FS }
                      /   *  - Output { PCI/ATA/USB/FS/Memory }
                      /   *  - CheckDisk/ChkD  { Drive# }
                      /   *  - FormatDisk/FmtD { Drive# }
                         *  - Dir  { List/Open/New/Delete }
                         *  - File { Open/New/Delete/Copy }
                      /   *  - Test {    Interrupts  /   Delegates   /   FileSystems /
                      /   *              ULLTComp    /   StringConcat/   ObjArray    /
                      /   *              IntArray    /   DummyObj    /   DivideBy0   /
                      /   *              Exceptions1 /   Exceptions2 /   PCBeep      /
                      /   *              Timer       /   Keyboard                        }
                      /   *  - GC   { Cleanup }
                      /   *  - USB { Update }
                         */

                        FOS_System.String line = console.ReadLine();
                        List lineParts = line.Split(' ');
                        FOS_System.String cmd = ((FOS_System.String)lineParts[0]).ToLower();
                        if (cmd == "halt")
                        {
                            terminating = true;
                        }
                        else if (cmd == "exinfo")
                        {
                            OutputCurrentExceptionInfo();
                        }
                        else if (cmd == "init")
                        {
                            #region Init
                            FOS_System.String opt1 = null;
                            if (lineParts.Count > 1)
                            {
                                opt1 = ((FOS_System.String)lineParts[1]).ToLower();
                            }

                            if (opt1 != null)
                            {
                                if (opt1 == "all")
                                {
                                    InitATA();
                                    InitPCI();
                                    InitUSB();
                                    InitFS();
                                }
                                else if (opt1 == "pci")
                                {
                                    InitPCI();
                                }
                                else if (opt1 == "ata")
                                {
                                    InitATA();
                                }
                                else if (opt1 == "usb")
                                {
                                    InitUSB();
                                }
                                else if (opt1 == "fs")
                                {
                                    InitFS();
                                }
                                else
                                {
                                    UnrecognisedOption();
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
                            #region Output
                            FOS_System.String opt1 = null;
                            if (lineParts.Count > 1)
                            {
                                opt1 = ((FOS_System.String)lineParts[1]).ToLower();
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
                                    OuptutUSB();
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
                                    UnrecognisedOption();
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
                            #region Check Disk

                            FOS_System.String opt1 = null;
                            if (lineParts.Count > 1)
                            {
                                opt1 = ((FOS_System.String)lineParts[1]).ToLower();
                            }

                            if (opt1 != null)
                            {
                                int diskNum = (int)FOS_System.Int32.Parse_DecimalUnsigned(opt1, 0);

                                console.Write("Are you sure device ");
                                console.Write_AsDecimal(diskNum);
                                console.Write(" is a disk device? (Y/N) : ");
                                FOS_System.String str = console.ReadLine().ToLower();
                                if (str == "y")
                                {
                                    console.Write("Checking disk ");
                                    console.Write_AsDecimal(diskNum);
                                    console.WriteLine("...");

                                    CheckDiskFormatting((Hardware.Devices.DiskDevice)Hardware.DeviceManager.Devices[diskNum]);
                                }
                                else
                                {
                                    console.WriteLine("Cancelled.");
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
                            #region Format Disk

                            FOS_System.String opt1 = null;
                            if (lineParts.Count > 1)
                            {
                                opt1 = ((FOS_System.String)lineParts[1]).ToLower();
                            }

                            if (opt1 != null)
                            {
                                int diskNum = (int)FOS_System.Int32.Parse_DecimalUnsigned(opt1, 0);

                                console.Write("Are you sure device ");
                                console.Write_AsDecimal(diskNum);
                                console.Write(" is a disk device? (Y/N) : ");
                                FOS_System.String str = console.ReadLine().ToLower();
                                if (str == "y")
                                {
                                    console.Write("Are you sure you wish to continue? (Y/N) : ");
                                    str = console.ReadLine().ToLower();
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
                                    console.WriteLine("Cancelled.");
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
                            #region GC
                            FOS_System.String opt1 = null;
                            if (lineParts.Count > 1)
                            {
                                opt1 = ((FOS_System.String)lineParts[1]).ToLower();
                            }

                            if (opt1 != null)
                            {
                                if (opt1 == "cleanup")
                                {
                                    FOS_System.GC.Cleanup();
                                }
                                else
                                {
                                    UnrecognisedOption();
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
                            #region USB
                            FOS_System.String opt1 = null;
                            if (lineParts.Count > 1)
                            {
                                opt1 = ((FOS_System.String)lineParts[1]).ToLower();
                            }

                            if (opt1 != null)
                            {
                                if (opt1 == "update")
                                {
                                    Hardware.USB.USBManager.Update();
                                }
                                else
                                {
                                    UnrecognisedOption();
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
                            #region Dir
                            FOS_System.String opt1 = null;
                            if (lineParts.Count > 1)
                            {
                                opt1 = ((FOS_System.String)lineParts[1]).ToLower();
                            }

                            if (opt1 != null)
                            {
                                if (opt1 == "list")
                                {
                                    FOS_System.String opt2 = null;
                                    if (lineParts.Count > 2)
                                    {
                                        opt2 = "";
                                        for (int i = 2; i < lineParts.Count; i++)
                                        {
                                            opt2 += ((FOS_System.String)lineParts[i]).ToLower();
                                            if (i < lineParts.Count - 1)
                                            {
                                                opt2 += " ";
                                            }
                                        }
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
                                else if(opt1 == "open")
                                {
                                    FOS_System.String opt2 = null;
                                    if (lineParts.Count > 2)
                                    {
                                        opt2 = "";
                                        for (int i = 2; i < lineParts.Count; i++)
                                        {
                                            opt2 += ((FOS_System.String)lineParts[i]).ToLower();
                                            if (i < lineParts.Count - 1)
                                            {
                                                opt2 += " ";
                                            }
                                        }
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
                                    if (lineParts.Count > 2)
                                    {
                                        opt2 = "";
                                        for (int i = 2; i < lineParts.Count; i++)
                                        {
                                            opt2 += ((FOS_System.String)lineParts[i]).ToLower();
                                            if (i < lineParts.Count - 1)
                                            {
                                                opt2 += " ";
                                            }
                                        }
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
                                else
                                {
                                    UnrecognisedOption();
                                }
                            }
                            else
                            {
                                console.WriteLine("You must specify what to output. { PCI/ATA/USB/FS }");
                            }
                            #endregion
                        }
                        else if (cmd == "file")
                        {
                            #region File
                            FOS_System.String opt1 = null;
                            if (lineParts.Count > 1)
                            {
                                opt1 = ((FOS_System.String)lineParts[1]).ToLower();
                            }

                            if (opt1 != null)
                            {
                                if (opt1 == "open")
                                {
                                    FOS_System.String opt2 = null;
                                    if (lineParts.Count > 2)
                                    {
                                        opt2 = "";
                                        for (int i = 2; i < lineParts.Count; i++)
                                        {
                                            opt2 += ((FOS_System.String)lineParts[i]).ToLower();
                                            if(i < lineParts.Count - 1)
                                            {
                                                opt2 += " ";
                                            }
                                        }
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
                                else
                                {
                                    UnrecognisedOption();
                                }
                            }
                            else
                            {
                                console.WriteLine("You must specify what to output. { PCI/ATA/USB/FS }");
                            }
                            #endregion
                        }
                        else if(cmd == "test")
                        {
                            #region Test
                            FOS_System.String opt1 = null;
                            if (lineParts.Count > 1)
                            {
                                opt1 = ((FOS_System.String)lineParts[1]).ToLower();
                            }

                            if (opt1 != null)
                            {
                                if(opt1 == "interrupts")
                                {
                                    InterruptsTest();
                                }
                                else if(opt1 == "delegates")
                                {
                                    DelegateTest();
                                }
                                else if(opt1 == "filesystems")
                                {
                                    FileSystemTests();
                                }
                                else if(opt1 == "ulltcomp")
                                {
                                    ULongLTComparisonTest();
                                }
                                else if(opt1 == "stringconcat")
                                {
                                    StringConcatTest();
                                }
                                else if(opt1 == "objarray")
                                {
                                    ObjectArrayTest();
                                }
                                else if(opt1 == "intarray")
                                {
                                    IntArrayTest();
                                }
                                else if(opt1 == "dummyobj")
                                {
                                    DummyObjectTest();
                                }
                                else if(opt1 == "divideby0")
                                {
                                    DivideByZeroTest();
                                }
                                else if(opt1 == "exceptions1")
                                {
                                    ExceptionsTestP1();
                                }
                                else if(opt1 == "exceptions2")
                                {
                                    ExceptionsTestP2();
                                }
                                else if(opt1 == "pcbeep")
                                {
                                    PCBeepTest();
                                }
                                else if(opt1 == "timer")
                                {
                                    TimerTest();
                                }
                                else if (opt1 == "keyboard")
                                {
                                    KeyboardTest();
                                }
                                else
                                {
                                    UnrecognisedOption();
                                }
                            }
                            else
                            {
                                console.WriteLine("You must specify which test. { Interrupts  /  Delegates    /  FileSystems /\n" +
                                                  "                               ULLTComp    /  StringConcat /  ObjArray    /\n" +
                                                  "                               IntArray    /  DummyObj     /  DivideBy0   /\n" +
                                                  "                               Exceptions1 /  Exceptions2  /  PCBeep      /\n" +
                                                  "                               Timer       /  Keyboard                     }");
                            }
                            #endregion
                        }
                    }
                    catch
                    {
                        OutputCurrentExceptionInfo();
                        Hardware.Devices.Timer.Default.Wait(5000);
                    }
                }
            }
            catch
            {
                OutputCurrentExceptionInfo();
            }
            console.WriteLine("Shell exited.");
        }

        private void DeleteDirectory(FOS_System.String path)
        {

        }
        private Directory NewDirectory(FOS_System.String path)
        {
            console.WriteLine("Searching for directory: " + path);
            Directory theDir = Directory.Find(path);
            if (theDir == null)
            {
                console.WriteLine("Creating directory...");
                FileSystemMapping mapping = FileSystemManager.GetMapping(path);
                if(mapping != null)
                {
                    if (path.EndsWith(FileSystemManager.PathDelimiter))
                    {
                        path = path.Substring(0, path.length - 1);
                    }

                    //                                                              + 1 as we wish to include the path
                    //                                                                  delimeter in parent dir name and
                    //                                                                  not in the new dir name.
                    //  Note: It is important to include the path delimeter at the end of the parent dir name
                    //        as the parent dir name may be a FS root which requires the trailing path delimeter.
                    int lastIdx = path.LastIndexOf(FileSystemManager.PathDelimiter) + 1;
                    FOS_System.String dirParentPath = path.Substring(0, lastIdx);
                    FOS_System.String newDirName = path.Substring(lastIdx, path.length - lastIdx);

                    console.WriteLine("Checking parent path: " + dirParentPath);
                    Directory parentDir = NewDirectory(dirParentPath);
                    if (parentDir != null)
                    {
                        console.WriteLine("New dir name: " + newDirName);
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
            List newPartitions = new List(1);
            newPartitions.Add(FOS_System.IO.Disk.MBR.CreateFAT32PartitionInfo(disk, false));
            FOS_System.IO.Disk.MBR.FormatDisk(disk, newPartitions);

            FOS_System.IO.FAT.FATFileSystem.FormatPartitionAsFAT32((FOS_System.IO.Partition)newPartitions[0]);
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
        /// Outputs the specified listings.
        /// </summary>
        /// <param name="Listings">The listings to output.</param>
        private void OutputListings(List Listings)
        {
            for (int j = 0; j < Listings.Count; j++)
            {
                FOS_System.IO.Base xItem = (FOS_System.IO.Base)Listings[j];

                if (xItem._Type == (FOS_System.Type)(typeof(FOS_System.IO.FAT.FATDirectory)))
                {
                    FOS_System.String name = ((FOS_System.IO.FAT.FATDirectory)Listings[j]).Name;
                    if (name != "." && name != "..")
                    {
                        console.WriteLine(((FOS_System.String)"<DIR> ") + name);
                        console.WriteLine("                 |||||||||||||||||||||||||||||||");
                        OutputListings(((FOS_System.IO.FAT.FATDirectory)Listings[j]).GetListings());
                        console.WriteLine("                 |||||||||||||||||||||||||||||||");
                    }
                }
                else if (xItem._Type == (FOS_System.Type)(typeof(FOS_System.IO.FAT.FATFile)))
                {
                    FOS_System.IO.FAT.FATFile file = ((FOS_System.IO.FAT.FATFile)Listings[j]);
                    console.WriteLine(((FOS_System.String)"<FILE> ") + file.Name + " (" + file.Size + ")");
                }
            }
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
                    byte[] xData = new byte[(int)(uint)aFile.Size];
                    int actuallyRead = fileStream.Read(xData, 0, xData.Length);
                    FOS_System.String xText = ByteConverter.GetASCIIStringFromASCII(xData, 0u, (uint)actuallyRead);
                    console.WriteLine(xText);
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

                        if (xItem._Type == (FOS_System.Type)(typeof(FOS_System.IO.FAT.FATDirectory)))
                        {
                            console.WriteLine(((FOS_System.String)"<DIR> ") + ((FOS_System.IO.FAT.FATDirectory)Listings[j]).Name);
                        }
                        else if (xItem._Type == (FOS_System.Type)(typeof(FOS_System.IO.FAT.FATFile)))
                        {
                            FOS_System.IO.FAT.FATFile file = ((FOS_System.IO.FAT.FATFile)Listings[j]);
                            console.WriteLine(((FOS_System.String)"<FILE> ") + file.Name + " (" + file.Size + ")");
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
                if (fsMapping.TheFileSystem._Type == ((FOS_System.Type)typeof(FOS_System.IO.FAT.FATFileSystem)))
                {
                    FOS_System.IO.FAT.FATFileSystem fs = (FOS_System.IO.FAT.FATFileSystem)fsMapping.TheFileSystem;
                    List Listings = fs.GetRootDirectoryTable();
                    console.WriteLine(((FOS_System.String)"FAT FS detected. Volume ID: ") + fs.ThePartition.VolumeID);
                    console.WriteLine("    - Prefix: " + fsMapping.Prefix);
                }
                else
                {
                    console.WriteLine("Non-FAT file-system added! (???)");
                }
            }
        }
        /// <summary>
        /// Outputs the USB system information.
        /// </summary>
        private void OuptutUSB()
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
                if (aDevice._Type == (FOS_System.Type)(typeof(Hardware.USB.Devices.MassStorageDevice_DiskDevice)))
                {
                    console.WriteLine();
                    console.Write("--------------------- Device ");
                    console.Write_AsDecimal(i);
                    console.WriteLine(" ---------------------");
                    console.WriteLine("USB Mass Storage Disk Device found.");
                    Hardware.USB.Devices.MassStorageDevice_DiskDevice theMSD = (Hardware.USB.Devices.MassStorageDevice_DiskDevice)aDevice;

                    console.WriteLine(((FOS_System.String)"Block Size: ") + theMSD.BlockSize + " bytes");
                    console.WriteLine(((FOS_System.String)"Block Count: ") + theMSD.BlockCount);
                    console.WriteLine(((FOS_System.String)"Size: ") + ((theMSD.BlockCount * theMSD.BlockSize) >> 20) + " MB");

                    numDrives++;
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
                if (aDevice._Type == (FOS_System.Type)(typeof(Hardware.ATA.ATAPio)))
                {
                    console.WriteLine();
                    console.Write("--------------------- Device ");
                    console.Write_AsDecimal(i);
                    console.WriteLine(" ---------------------");
                    console.WriteLine("ATAPio device found.");
                    Hardware.ATA.ATAPio theATA = (Hardware.ATA.ATAPio)aDevice;

                    console.WriteLine(((FOS_System.String)"Type: ") + (theATA.DriveType == Hardware.ATA.ATAPio.SpecLevel.ATA ? "ATA" : "ATAPI"));
                    console.WriteLine(((FOS_System.String)"Serial No: ") + theATA.SerialNo);
                    console.WriteLine(((FOS_System.String)"Firmware Rev: ") + theATA.FirmwareRev);
                    console.WriteLine(((FOS_System.String)"Model No: ") + theATA.ModelNo);
                    console.WriteLine(((FOS_System.String)"Block Size: ") + theATA.BlockSize + " bytes");
                    console.WriteLine(((FOS_System.String)"Block Count: ") + theATA.BlockCount);
                    console.WriteLine(((FOS_System.String)"Size: ") + ((theATA.BlockCount * theATA.BlockSize) >> 20) + " MB");

                    numDrives++;
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

        /// <summary>
        /// Outputs a warning to the user indicating their input was unrecognised.
        /// </summary>
        private void UnrecognisedOption()
        {
            console.WarningColour();
            console.WriteLine("Unrecognised option.");
            console.DefaultColour();
        }


        /// <summary>
        /// Tests all interrupts in the range 17 to 255 by firing them.
        /// </summary>
        [Compiler.NoGC]
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
        [Compiler.NoGC]
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

                FOS_System.Type currExceptionType = ExceptionMethods.CurrentException._Type;
                if (currExceptionType == (FOS_System.Type)typeof(FOS_System.Exceptions.DivideByZeroException))
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
        ///     [Compiler.NoGC]
        ///     get
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
                ExceptionMethods.Throw(new FOS_System.Exception("An exception."));
            }
            finally
            {
                console.WriteLine("Finally ran.");
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
                OutputCurrentExceptionInfo();
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
                OutputCurrentExceptionInfo();
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
                OutputCurrentExceptionInfo();
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
                Core.Console.InitDefault();

                Core.Console.Default.Beep();
                Core.Console.Default.WriteLine("Test write line.");
                Core.Console.Default.WriteLine("Please write a line: ");
                FOS_System.String line = Core.Console.Default.ReadLine();
                Core.Console.Default.WriteLine("Your wrote: " + line);

                Core.Console.Default.WriteLine("Pausing for 2 seconds...");
                Hardware.Devices.Timer.Default.Wait(2000);

                for (int i = 0; i < 25; i++)
                {
                    Core.Console.Default.Write("Line ");
                    Core.Console.Default.WriteLine_AsDecimal(i);
                }

                Core.Console.Default.WriteLine("Testing scrolling...");
                for (int i = 0; i < 25; i++)
                {
                    Hardware.Devices.Timer.Default.Wait(500);
                    Core.Console.Default.Scroll(-1);
                }
                Core.Console.Default.Scroll(25);

                Core.Console.Default.WriteLine("Scroll test done.");

                Core.Console.Default.WriteLine("Testing Clear and Colour...");
                Core.Console.Default.Clear();
                Core.Console.Default.WarningColour();
                Core.Console.Default.WriteLine("Warning colour test.");
                Core.Console.Default.ErrorColour();
                Core.Console.Default.WriteLine("Error colour test.");
                Core.Console.Default.DefaultColour();
                Core.Console.Default.WriteLine("Default colour test.");
            }
            catch
            {
                OutputCurrentExceptionInfo();
            }

            console.WriteLine("Ended advanced console test. Pausing for 5 seconds.");
            Hardware.Devices.Timer.Default.Wait(5000);
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
