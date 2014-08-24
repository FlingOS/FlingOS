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
        public static List Partitions = new List(2);
        /// <summary>
        /// The list of file system mappings.
        /// </summary>
        public static List FileSystemMappings = new List(2);

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
                if (aDevice._Type == (FOS_System.Type)(typeof(Hardware.ATA.ATAPio)))
                {
                    try
                    {
                        InitDisk((DiskDevice)aDevice);
                    }
                    catch
                    {
                        BasicConsole.WriteLine("Error initializing disk: " + ExceptionMethods.CurrentException.Message);
                    }
                }
                else if (aDevice._Type == (FOS_System.Type)(typeof(Hardware.USB.Devices.MassStorageDevice_DiskDevice)))
                {
                    try
                    {
                        InitDisk((DiskDevice)aDevice);
                    }
                    catch
                    {
                        BasicConsole.WriteLine("Error initializing MSD: " + ExceptionMethods.CurrentException.Message);
                    }
                }
                //TODO - Add more device types e.g. USB
            }
            
            InitPartitions();
        }

        /// <summary>
        /// Initializes the specified disk device.
        /// </summary>
        /// <param name="aDiskDevice">The disk device to initialize.</param>
        public static void InitDisk(DiskDevice aDiskDevice)
        {
            //TODO - Add more partitioning schemes.

            //Must check for GPT before MBR because GPT uses a protective
            //  MBR entry so will be seen as valid MBR.
            if (InitAsGPT(aDiskDevice))
            {
#if DEBUG
                BasicConsole.WriteLine("GPT formatted disk detected!");
                BasicConsole.DelayOutput(3);
#endif
            }
            else if(!InitAsMBR(aDiskDevice))
            {
                ExceptionMethods.Throw(new FOS_System.Exceptions.NotSupportedException("Non MBR/EBR/GPT formatted disks not supported."));
            }
        }

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
        private static void ProcessGPT(GPT aGPT, DiskDevice aDiskDevice)
        {
            for (int i = 0; i < aGPT.Partitions.Count; i++)
            {
                GPT.PartitionInfo aPartInfo = (GPT.PartitionInfo)aGPT.Partitions[i];
                Partitions.Add(new Partition(aDiskDevice, aPartInfo.FirstLBA, aPartInfo.LastLBA - aPartInfo.FirstLBA));
            }
        }
        /// <summary>
        /// Processes a valid master boot record to initialize 
        /// its partitions.
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
        /// Initializes all available partitions looking for valid 
        /// file systems.
        /// </summary>
        private static void InitPartitions()
        {
            for (int i = 0; i < Partitions.Count; i++)
            {
                Partition aPartition = (Partition)Partitions[i];
                FOS_System.IO.FAT.FATFileSystem newFS = new FOS_System.IO.FAT.FATFileSystem(aPartition);
                if (newFS.IsValid)
                {
                    FOS_System.String mappingPrefix = FOS_System.String.New(3);
                    mappingPrefix[0] = (char)((int)('A') + i);
                    mappingPrefix[1] = ':';
                    mappingPrefix[2] = PathDelimiter;
                    newFS.TheMapping = new FileSystemMapping(mappingPrefix, newFS);
                    FileSystemMappings.Add(newFS.TheMapping);
                }
                else
                {
                    BasicConsole.WriteLine("Partition not formatted as valid FAT file-system.");
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
