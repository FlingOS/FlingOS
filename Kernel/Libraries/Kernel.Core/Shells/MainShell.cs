using System;
using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.IO;

namespace Kernel.Core.Shells
{
    /// <summary>
    /// Implements the main shell for the core kernel.
    /// </summary>
    public class MainShell : Shell
    {
        /// <summary>
        /// The current directory of the shell. Printed at the start of every new command line and used as the start of relative file paths
        /// and the default path in file-based commands.
        /// </summary>
        protected FOS_System.String CurrentDir = "";

        /// <summary>
        /// Executes the main shell.
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
                      /   *  - Init { PCI/ATA/USB/FS }
                      /   *  - Output { PCI/ATA/USB/FS/Memory }
                      /   *  - CheckDisk/ChkD  { Drive# }
                      /   *  - FormatDisk/FmtD { Drive# }
                         *  - Dir  { List/Open/New/Delete }
                         *  - File { Open/New/Edit/Delete/Copy }
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
                                if (opt1 == "pci")
                                {
                                    console.Write("Initialising PCI...");
                                    Hardware.PCI.PCI.Init();
                                    console.WriteLine("done.");
                                }
                                else if (opt1 == "ata")
                                {
                                    console.Write("Initialising ATA...");
                                    Hardware.ATA.ATAManager.Init();
                                    console.WriteLine("done.");
                                }
                                else if (opt1 == "usb")
                                {
                                    console.Write("Initialising USB...");
                                    Hardware.USB.USBManager.Init();
                                    console.WriteLine("done.");
                                }
                                else if (opt1 == "fs")
                                {
                                    console.Write("Initialising file systems...");
                                    FileSystemManager.Init();
                                    console.WriteLine("done.");
                                }
                                else
                                {
                                    console.WarningColour();
                                    console.WriteLine("Unrecognised option.");
                                    console.DefaultColour();
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
                                    for (int i = 0; i < Hardware.PCI.PCI.Devices.Count; i++)
                                    {
                                        Hardware.PCI.PCIDevice aDevice = (Hardware.PCI.PCIDevice)Hardware.PCI.PCI.Devices[i];
                                        console.WriteLine(Hardware.PCI.PCIDevice.DeviceClassInfo.GetString(aDevice));
                                    }
                                }
                                else if (opt1 == "ata")
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
                                else if (opt1 == "usb")
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
                                else if (opt1 == "fs")
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
                                else if (opt1 == "memory" || opt1 == "mem")
                                {
                                    Output_Memory();
                                }
                                else
                                {
                                    console.WarningColour();
                                    console.WriteLine("Unrecognised option.");
                                    console.DefaultColour();
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
                                    console.WarningColour();
                                    console.WriteLine("Unrecognised option.");
                                    console.DefaultColour();
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
                                    console.WarningColour();
                                    console.WriteLine("Unrecognised option.");
                                    console.DefaultColour();
                                }
                            }
                            else
                            {
                                console.WriteLine("You must specify what to do. { Update }");
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

        /// <summary>
        /// Outputs current memory information.
        /// </summary>
        protected unsafe void Output_Memory()
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
        /// Checks the specified disk's formatting. Requires (attempt) to initialise file systems first.
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
    }
}
