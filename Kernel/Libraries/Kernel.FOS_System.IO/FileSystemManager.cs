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
#undef FSM_TRACE

using System;

using Kernel.FOS_System.Collections;
using Kernel.Hardware;
using Kernel.Hardware.Devices;
using Kernel.FOS_System.IO.Disk;

namespace Kernel.FOS_System.IO
{
    /// <summary>
    /// Provides management for file systems in the kernel.
    /// </summary>
    public static class FileSystemManager
    {
        /// <summary>
        /// The delimiter that separates mapping prefixes and directory/file names in a path.
        /// </summary>
        public const char PathDelimiter = '/';

        /// <summary>
        /// The list of initialized partitions.
        /// </summary>
        public static List Partitions = new List(3);
        /// <summary>
        /// The list of file system mappings.
        /// </summary>
        public static List FileSystemMappings = new List(3);

        /// <summary>
        /// Initializes all available file systems by searching for 
        /// valid partitions on the available disk devices.
        /// </summary>
        public static void Init()
        {
            Partitions.Empty();
            FileSystemMappings.Empty();

            for (int i = 0; i < DeviceManager.Devices.Count; i++)
            {
                Device aDevice = (Device)DeviceManager.Devices[i];
                if (aDevice is DiskDevice)
                {
                    try
                    {
                        InitDisk((DiskDevice)aDevice);
                    }
                    catch
                    {
                        BasicConsole.WriteLine("Error initializing disk: " + (FOS_System.String)i);
                        BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                        //BasicConsole.DelayOutput(20);
                    }
                }
            }
            
            InitPartitions();
        }

        /// <summary>
        /// Initializes the specified disk device.
        /// </summary>
        /// <param name="aDiskDevice">The disk device to initialize.</param>
        public static void InitDisk(DiskDevice aDiskDevice)
        {
            //TODO: Add more partitioning schemes.


            if (InitAsISO9660(aDiskDevice))
            {
#if FSM_TRACE
                BasicConsole.WriteLine("ISO9660 CD/DVD disc detected!");
                BasicConsole.DelayOutput(3);
#endif
            }
            //Must check for GPT before MBR because GPT uses a protective
            //  MBR entry so will be seen as valid MBR.
            else if (InitAsGPT(aDiskDevice))
            {
#if FSM_TRACE
                BasicConsole.WriteLine("GPT formatted disk detected!");
                BasicConsole.DelayOutput(3);
#endif
            }
            else if (!InitAsMBR(aDiskDevice))
            {
                ExceptionMethods.Throw(new FOS_System.Exceptions.NotSupportedException("Non MBR/EBR/GPT/ISO9660 formatted disks not supported."));
            }
        }
        /// <summary>
        /// Initializes all available partitions looking for valid 
        /// file systems.
        /// </summary>
        public static void InitPartitions()
        {
            for (int i = 0; i < Partitions.Count; i++)
            {
                try
                {
                    Partition aPartition = (Partition)Partitions[i];
                    if (!aPartition.Mapped)
                    {
                        //BasicConsole.WriteLine("Attempting to create FAT File System...");
                        FileSystem newFS = null;
                        if (aPartition is Disk.ISO9660.PrimaryVolumeDescriptor)
                        {
                            newFS = new ISO9660.ISO9660FileSystem((Disk.ISO9660.PrimaryVolumeDescriptor)aPartition);
                        }
                        else
                        {
                            newFS = new FOS_System.IO.FAT.FATFileSystem(aPartition);
                        }

                        if (newFS.IsValid)
                        {
                            FOS_System.String mappingPrefix = FOS_System.String.New(3);
                            mappingPrefix[0] = (char)((int)('A') + i);
                            mappingPrefix[1] = ':';
                            mappingPrefix[2] = PathDelimiter;
                            newFS.TheMapping = new FileSystemMapping(mappingPrefix, newFS);
                            FileSystemMappings.Add(newFS.TheMapping);
                            aPartition.Mapped = true;
                        }
                        //else
                        //{
                        //    BasicConsole.WriteLine("Partition not formatted as valid FAT file-system.");
                        //}
                    }
                }
                catch
                {
                    BasicConsole.Write("Error initialising partition: ");
                    BasicConsole.WriteLine(i);
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                    //BasicConsole.DelayOutput(20);
                }
            }
        }

        private static bool InitAsISO9660(DiskDevice aDiskDevice)
        {
            // Must check for ISO9660 only on CD/DVD drives
            if (aDiskDevice is Hardware.ATA.PATAPI)
            {
                Disk.ISO9660 TheISO9660 = new Disk.ISO9660(aDiskDevice);

#if FSM_TRACE
                TheISO9660.Print();
#endif
                ProcessISO9660(TheISO9660, aDiskDevice);

                return true;
            }

            return false;
        }
        /// <summary>
        /// Attempts to initialise a disk treating it as GPT formatted.
        /// </summary>
        /// <param name="aDiskDevice">The disk to initialise.</param>
        /// <returns>True if a valid GPT was detected and the disk was successfully initialised. Otherwise, false.</returns>
        private static bool InitAsGPT(DiskDevice aDiskDevice)
        {
            GPT TheGPT = new GPT(aDiskDevice);
            if (!TheGPT.IsValid)
            {
                return false;
            }
            else
            {
                ProcessGPT(TheGPT, aDiskDevice);
                return true;
            }
        }
        /// <summary>
        /// Attempts to initialise a disk treating it as MBR formatted.
        /// </summary>
        /// <param name="aDiskDevice">The disk to initialise.</param>
        /// <returns>True if a valid MBR was detected and the disk was successfully initialised. Otherwise, false.</returns>
        private static bool InitAsMBR(DiskDevice aDiskDevice)
        {
#if FSM_TRACE
            BasicConsole.WriteLine("Attempting to read MBR...");
#endif
            byte[] MBRData = new byte[512];
            aDiskDevice.ReadBlock(0UL, 1U, MBRData);
#if FSM_TRACE
            BasicConsole.WriteLine("Read potential MBR data. Attempting to init MBR...");
#endif
            MBR TheMBR = new MBR(MBRData);

            if (!TheMBR.IsValid)
            {
                return false;
            }
            else
            {
#if FSM_TRACE
                BasicConsole.WriteLine("Valid MBR found.");
#endif
                ProcessMBR(TheMBR, aDiskDevice);

                return true;
            }
        }
        private static void ProcessISO9660(Disk.ISO9660 aISO9660, DiskDevice aDiskDevice)
        {
            for (int i = 0; i < aISO9660.VolumeDescriptors.Count; i++)
            {
                Disk.ISO9660.VolumeDescriptor volDescrip = (Disk.ISO9660.VolumeDescriptor)aISO9660.VolumeDescriptors[i];
                if (volDescrip is Disk.ISO9660.PrimaryVolumeDescriptor)
                {
                    Partitions.Add(volDescrip);
                }
            }
        }
        /// <summary>
        /// Processes a valid GUID partition table to initialize its partitions.
        /// </summary>
        /// <param name="aGPT">The GPT to process.</param>
        /// <param name="aDiskDevice">The disk device from which the GPT was read.</param>
        private static void ProcessGPT(GPT aGPT, DiskDevice aDiskDevice)
        {
            for (int i = 0; i < aGPT.Partitions.Count; i++)
            {
                GPT.PartitionInfo aPartInfo = (GPT.PartitionInfo)aGPT.Partitions[i];
                Partitions.Add(new Partition(aDiskDevice, aPartInfo.FirstLBA, aPartInfo.LastLBA - aPartInfo.FirstLBA));
            }
        }
        /// <summary>
        /// Processes a valid master boot record to initialize its partitions.
        /// </summary>
        /// <param name="anMBR">The MBR to process.</param>
        /// <param name="aDiskDevice">The disk device from which the MBR was read.</param>
        private static void ProcessMBR(MBR anMBR, DiskDevice aDiskDevice)
        {
            for (int i = 0; i < anMBR.NumPartitions; i++)
            {
                MBR.PartitionInfo aPartInfo = anMBR.Partitions[i];
                if (aPartInfo.EBRLocation != 0)
                {
                    byte[] EBRData = new byte[512];
                    aDiskDevice.ReadBlock(aPartInfo.EBRLocation, 1U, EBRData);
                    EBR newEBR = new EBR(EBRData);
                    ProcessMBR(newEBR, aDiskDevice);
                }
                else
                {
                    Partitions.Add(new Partition(aDiskDevice, aPartInfo.StartSector, aPartInfo.SectorCount));
                }
            }
        }

        /// <summary>
        /// Gets the file system mapping for the specified path.
        /// </summary>
        /// <param name="aPath">The path to get the mapping for.</param>
        /// <returns>The file system mapping or null if none exists.</returns>
        public static FileSystemMapping GetMapping(FOS_System.String aPath)
        {
            FileSystemMapping result = null;

            for (int i = 0; i < FileSystemMappings.Count; i++)
            {
                FileSystemMapping aMapping = (FileSystemMapping)FileSystemMappings[i];
                if (aMapping.PathMatchesMapping(aPath))
                {
                    result = aMapping;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the specified partition has any file system mappings associated with it.
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
    }
}
