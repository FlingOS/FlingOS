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
                    console.Write(CurrentDir + " > ");

                    /* Command { Req Arg } [Opt Arg]:
                     *  - Halt
                     *  - ExInfo
                     *  - Init { PCI/ATA/USB/FS }
                     *  - Output { PCI/ATA/USB/FS/Memory }
                     *  - CheckDisk/ChkD  { Drive# }
                     *  - FormatDisk/FmtD { Drive# }
                     *  - Dir  { List/Open/New/Delete }
                     *  - File { Open/New/Edit/Delete/Copy }
                     *  - Test {    Interrupts  /   Delegates   /   FileSystems /
                     *              ULLTComp    /   StringConcat/   ObjArray    /
                     *              IntArray    /   DummyObj    /   DivideBy0   /
                     *              Exceptions1 /   Exceptions2 /   PCBeep      /
                     *              Timer       /   Keyboard                        }
                     */

                    FOS_System.String line = console.ReadLine();
                    List lineParts = line.Split(' ');
                    FOS_System.String cmd = ((FOS_System.String)lineParts[0]).ToLower();
                    if (cmd == "halt")
                    {
                        terminating = true;
                    }
                    else if(cmd == "exinfo")
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
                        //TODO: Memory
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
                                        console.WriteLine("ATAPio device found.");
                                        Hardware.ATA.ATAPio theATA = (Hardware.ATA.ATAPio)aDevice;

                                        console.WriteLine(((FOS_System.String)"Type: ") + (theATA.DriveType == Hardware.ATA.ATAPio.SpecLevel.ATA ? "ATA" : "ATAPI"));
                                        console.WriteLine(((FOS_System.String)"Serial No: ") + theATA.SerialNo);
                                        console.WriteLine(((FOS_System.String)"Firmware Rev: ") + theATA.FirmwareRev);
                                        console.WriteLine(((FOS_System.String)"Model No: ") + theATA.ModelNo);
                                        console.WriteLine(((FOS_System.String)"Block Size: ") + theATA.BlockSize + " bytes");
                                        console.WriteLine(((FOS_System.String)"Block Count: ") + theATA.BlockCount);
                                        console.WriteLine(((FOS_System.String)"Size: ") + ((theATA.BlockCount * theATA.BlockSize) >> 20) + " MB");
                                        OutputDivider();

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
                }
            }
            catch
            {
                OutputCurrentExceptionInfo();
            }
            console.WriteLine("Shell exited.");
        }
    }
}
