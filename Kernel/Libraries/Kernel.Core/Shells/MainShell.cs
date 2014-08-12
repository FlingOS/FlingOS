using System;
using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.IO;

namespace Kernel.Core.Shells
{
    public class MainShell : Shell
    {
        protected FOS_System.String CurrentDir = "";

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
                      \   *  - Dir  { List/Open/New/Delete }
                      \   *  - File { Open/New/Delete/Copy }
                         *  - Test {    Interrupts  /   Delegates   /   FileSystems /
                         *              ULLTComp    /   StringConcat/   ObjArray    /
                         *              IntArray    /   DummyObj    /   DivideBy0   /
                         *              Exceptions1 /   Exceptions2 /   PCBeep      /
                         *              Timer       /   Keyboard                        }
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

        private void FormatDisk(Hardware.Devices.DiskDevice disk)
        {
            List newPartitions = new List(1);
            newPartitions.Add(FOS_System.IO.Disk.MBR.CreateFAT32PartitionInfo(disk, false));
            FOS_System.IO.Disk.MBR.FormatDisk(disk, newPartitions);

            FOS_System.IO.FAT.FATFileSystem.FormatPartitionAsFAT32((FOS_System.IO.Partition)newPartitions[0]);
        }
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
        protected unsafe void OutputMemory()
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
        private void OutputPCI()
        {
            for (int i = 0; i < Hardware.PCI.PCI.Devices.Count; i++)
            {
                Hardware.PCI.PCIDevice aDevice = (Hardware.PCI.PCIDevice)Hardware.PCI.PCI.Devices[i];
                console.WriteLine(Hardware.PCI.PCIDevice.DeviceClassInfo.GetString(aDevice));
            }
        }

        private void InitFS()
        {
            console.Write("Initialising file systems...");
            FileSystemManager.Init();
            console.WriteLine("done.");
        }
        private void InitUSB()
        {
            console.Write("Initialising USB...");
            Hardware.USB.USBManager.Init();
            console.WriteLine("done.");
        }
        private void InitATA()
        {
            console.Write("Initialising ATA...");
            Hardware.ATA.ATAManager.Init();
            console.WriteLine("done.");
        }
        private void InitPCI()
        {
            console.Write("Initialising PCI...");
            Hardware.PCI.PCI.Init();
            console.WriteLine("done.");
        }

        private void UnrecognisedOption()
        {
            console.WarningColour();
            console.WriteLine("Unrecognised option.");
            console.DefaultColour();
        }

    }
}
