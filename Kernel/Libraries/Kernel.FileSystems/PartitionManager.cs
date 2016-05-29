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

#define PM_TRACE

using Kernel.Devices;
using Kernel.FileSystems.Disk;
using Kernel.FileSystems.FAT;
using Kernel.FileSystems.ISO9660;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Exceptions;

namespace Kernel.FileSystems
{
    public class PartitionManager : Object
    {
        /// <summary>
        ///     The list of initialized partitions.
        /// </summary>
        public static List Partitions;

        public static void Init()
        {
            Partitions = new List(3);
        }

        /// <summary>
        ///     Initializes the specified disk device.
        /// </summary>
        /// <param name="aDiskDevice">The disk device to initialize.</param>
        public static void InitDisk(DiskDevice TheDisk)
        {
            //TODO: Add more partitioning schemes.

            //Must check for GPT before MBR because GPT uses a protective
            //  MBR entry so will be seen as valid MBR.
            if (InitAsGPT(TheDisk))
            {
#if PM_TRACE
                BasicConsole.WriteLine("GPT formatted disk detected!");
                BasicConsole.DelayOutput(3);
#endif
            }
            else if (InitAsMBR(TheDisk))
            {
#if PM_TRACE
                BasicConsole.WriteLine("MBR formatted disk detected!");
                BasicConsole.DelayOutput(3);
#endif
            }
            else if (InitAsISO9660(TheDisk))
            {
#if PM_TRACE
                BasicConsole.WriteLine("ISO9660 CD/DVD disc detected!");
                BasicConsole.DelayOutput(3);
#endif
            }
            else
            {
                ExceptionMethods.Throw(
                    new NotSupportedException("Non MBR/EBR/GPT/ISO9660 formatted disks not supported."));
            }
        }

        private static bool InitAsISO9660(DiskDevice TheDisk)
        {
            // TODO: Should only check for ISO9660 only on CD/DVD drives

            Disk.ISO9660 TheISO9660 = new Disk.ISO9660(TheDisk);

#if PM_TRACE
            TheISO9660.Print();
#endif
            ProcessISO9660(TheISO9660, TheDisk);

            return true;
        }

        /// <summary>
        ///     Attempts to initialise a disk treating it as GPT formatted.
        /// </summary>
        /// <param name="aDiskDevice">The disk to initialise.</param>
        /// <returns>True if a valid GPT was detected and the disk was successfully initialised. Otherwise, false.</returns>
        private static bool InitAsGPT(DiskDevice TheDisk)
        {
            GPT TheGPT = new GPT(TheDisk);
            if (!TheGPT.IsValid)
            {
                return false;
            }
            ProcessGPT(TheGPT, TheDisk);
            return true;
        }

        /// <summary>
        ///     Attempts to initialise a disk treating it as MBR formatted.
        /// </summary>
        /// <param name="aDiskDevice">The disk to initialise.</param>
        /// <returns>True if a valid MBR was detected and the disk was successfully initialised. Otherwise, false.</returns>
        private static bool InitAsMBR(DiskDevice TheDisk)
        {
#if PM_TRACE
            BasicConsole.WriteLine("Attempting to read MBR...");
#endif
            byte[] MBRData = new byte[512];
            TheDisk.ReadBlock(0UL, 1U, MBRData);
#if PM_TRACE
            BasicConsole.WriteLine("Read potential MBR data. Attempting to init MBR...");
#endif
            MBR TheMBR = new MBR(MBRData);

            if (!TheMBR.IsValid)
            {
#if PM_TRACE
                BasicConsole.WriteLine("Not a valid MBR.");
#endif
                return false;
            }
#if PM_TRACE
            BasicConsole.WriteLine("Valid MBR found.");
#endif
            ProcessMBR(TheMBR, TheDisk);

            return true;
        }

        private static void ProcessISO9660(Disk.ISO9660 aISO9660, DiskDevice TheDisk)
        {
            for (int i = 0; i < aISO9660.VolumeDescriptors.Count; i++)
            {
                Disk.ISO9660.VolumeDescriptor volDescrip = (Disk.ISO9660.VolumeDescriptor)aISO9660.VolumeDescriptors[i];
                if (volDescrip is Disk.ISO9660.PrimaryVolumeDescriptor)
                {
                    AddPartition(volDescrip);
                }
            }
        }

        /// <summary>
        ///     Processes a valid GUID partition table to initialize its partitions.
        /// </summary>
        /// <param name="aGPT">The GPT to process.</param>
        /// <param name="aDiskDevice">The disk device from which the GPT was read.</param>
        private static void ProcessGPT(GPT aGPT, DiskDevice TheDisk)
        {
            for (int i = 0; i < aGPT.Partitions.Count; i++)
            {
                GPT.PartitionInfo aPartInfo = (GPT.PartitionInfo)aGPT.Partitions[i];
                AddPartition(new Partition(TheDisk, aPartInfo.FirstLBA, aPartInfo.LastLBA - aPartInfo.FirstLBA));
            }
        }

        /// <summary>
        ///     Processes a valid master boot record to initialize its partitions.
        /// </summary>
        /// <param name="anMBR">The MBR to process.</param>
        /// <param name="TheDisk">The disk device from which the MBR was read.</param>
        private static void ProcessMBR(MBR anMBR, DiskDevice TheDisk)
        {
            for (int i = 0; i < anMBR.NumPartitions; i++)
            {
                MBR.PartitionInfo aPartInfo = anMBR.Partitions[i];
                if (aPartInfo.EBRLocation != 0)
                {
                    byte[] EBRData = new byte[512];
                    TheDisk.ReadBlock(aPartInfo.EBRLocation, 1U, EBRData);
                    EBR newEBR = new EBR(EBRData);
                    ProcessMBR(newEBR, TheDisk);
                }
                else
                {
                    AddPartition(new Partition(TheDisk, aPartInfo.StartSector, aPartInfo.SectorCount));
                }
            }
        }

        public static void AddPartition(Partition partition)
        {
            BasicConsole.WriteLine("Partition Manager > Adding partition...");
            Partitions.Add(partition);

            try
            {
                if (!partition.Mapped)
                {
                    FileSystem newFS = InitPartition(partition);

                    if (newFS.IsValid)
                    {
                        BasicConsole.WriteLine("Partition Manager > Initialising file system...");

                        FileSystemManager.InitFileSystem(partition, newFS);
                    }
                    else
                    {
                        BasicConsole.WriteLine(
                            "Partition Manager > Error! Partition not formatted as valid FAT or ISO9660 file-system.");
                    }
                }
            }
            catch
            {
                BasicConsole.Write("Error initialising partition: ");
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                //BasicConsole.DelayOutput(20);
            }
        }

        /// <summary>
        ///     Initializes all available partitions looking for valid
        ///     file systems.
        /// </summary>
        private static FileSystem InitPartition(Partition aPartition)
        {
            FileSystem newFS = null;
            if (aPartition is Disk.ISO9660.PrimaryVolumeDescriptor)
            {
                newFS = new ISO9660FileSystem((Disk.ISO9660.PrimaryVolumeDescriptor)aPartition);
            }
            else
            {
                newFS = new FATFileSystem(aPartition);
            }
            return newFS;
        }
    }
}