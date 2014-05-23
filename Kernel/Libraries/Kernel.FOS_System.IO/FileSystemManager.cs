using System;

using Kernel.FOS_System.Collections;
using Kernel.Hardware;
using Kernel.Hardware.Devices;
using Kernel.FOS_System.IO.Disk;

namespace Kernel.FOS_System.IO
{
    public static class FileSystemManager
    {
        public const char PathDelimiter = '/';

        internal static List Partitions = new List(2);
        public static List FileSystemMappings = new List(2);

        public static void Init()
        {
            for (int i = 0; i < DeviceManager.Devices.Count; i++)
            {
                Device aDevice = (Device)DeviceManager.Devices[i];
                if (aDevice._Type == (FOS_System.Type)(typeof(Hardware.ATA.ATAPio)))
                {
                    InitDisk((DiskDevice)aDevice);
                }
            }
            
            InitPartitions();
        }

        internal static void InitDisk(DiskDevice aDiskDevice)
        {
            byte[] MBRData = new byte[512];
            aDiskDevice.ReadBlock(0UL, 1U, MBRData);
            MBR TheMBR = new MBR(MBRData);

            if (!TheMBR.IsValid)
            {
                ExceptionMethods.Throw(new FOS_System.Exceptions.NotSupportedException("Non MBR/EBR formatted disks not supported."));
            }

            ProcessMBR(TheMBR, aDiskDevice);
        }
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
                    FileSystemMappings.Add(new FileSystemMapping(mappingPrefix, newFS));
                }
                else
                {
                    BasicConsole.WriteLine("Partition not formatted as valid FAT file-system.");
                }
            }
        }

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
    }
}
