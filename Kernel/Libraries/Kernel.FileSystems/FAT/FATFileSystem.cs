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

#define FATFS_TRACE
#undef FATFS_TRACE

using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Exceptions;

namespace Kernel.FileSystems.FAT
{
    /// <summary>
    ///     Represents a FAT (12/16/32) file system.
    /// </summary>
    public class FATFileSystem : FileSystem
    {
        /// <summary>
        ///     The types of FAT file system.
        /// </summary>
        public enum FATTypeEnum
        {
            /// <summary>
            ///     Unknown / unrecognized FAT file system.
            /// </summary>
            Unknown,

            /// <summary>
            ///     FAT12 (12-bit) version
            /// </summary>
            FAT12,

            /// <summary>
            ///     FAT16 (16-bit) version
            /// </summary>
            FAT16,

            /// <summary>
            ///     FAT32 (32-bit) version
            /// </summary>
            FAT32
        }

        /// <summary>
        ///     Number of bytes per cluster (= SectorsPerCluster * BytesPerSector)
        /// </summary>
        public readonly uint BytesPerCluster;

        //Note: This implementation is capable of reading FAT 12, 16 or 32 but only capable of
        //      writing FAT32. This is sensible since FAT12 and FAT16 have long been outdated.

        //Note: A FAT file system is contained within a partition. So all read / writes should 
        //      be done through the partition not the disk device. The partition handles
        //      translating the sector numbers from partition relative to disk relative.

        //Note: This implementation makes no use of caching for global file system data such as 
        //      the FAT table. This means data is written straight to disk. However, this causes
        //      a large volume of read / write traffic. A more sofisticated implementation would
        //      utilise sector caching and dirty sector marking.


        //TODO: The implementation for FAT12/16 does not fit will with the rest of the file
        //       system because there is no way to create a root directory listing (it is 
        //       possible for FAT32). A new listing class should be created to virtualise
        //       (/simulate) a root directory for FAT12/16.


        /// <summary>
        ///     Number of bytes per sector
        /// </summary>
        public readonly uint BytesPerSector;

        /// <summary>
        ///     Number of clusters (to use for storing files/directories).
        /// </summary>
        public readonly uint ClusterCount;

        /// <summary>
        ///     The sector number of the first data sector.
        /// </summary>
        public readonly ulong DataSector; // First Data Sector

        /// <summary>
        ///     The number of sectors for data storage.
        /// </summary>
        public readonly uint DataSectorCount;

        /// <summary>
        ///     Number of sectors for file allocation tables.
        /// </summary>
        public readonly uint FATSectorCount;

        /// <summary>
        ///     The FAT type of the file system. Writing is only supported for FAT32.
        /// </summary>
        public readonly FATTypeEnum FATType = FATTypeEnum.Unknown;

        /// <summary>
        ///     Number of file allocation tables.
        /// </summary>
        public readonly uint NumberOfFATs;

        /// <summary>
        ///     Number of reserved sectors.
        /// </summary>
        public readonly uint ReservedSectorCount;

        /// <summary>
        ///     The cluster number for the root cluster. Used by FAT32 only.
        /// </summary>
        public readonly uint RootCluster; // FAT32

        /// <summary>
        ///     The number of entries in the root directory table.
        /// </summary>
        public readonly uint RootEntryCount;

        /// <summary>
        ///     Root sector number - used by FAT12/16 only.
        /// </summary>
        public readonly ulong RootSector; // FAT12/16

        /// <summary>
        ///     Number of root sectors - used by FAT12/16 only. Always 0 for FAT32.
        /// </summary>
        public readonly uint RootSectorCount; // FAT12/16, (FAT32 this is always 0)

        /// <summary>
        ///     Number of sectors per cluster
        /// </summary>
        public readonly uint SectorsPerCluster;

        /// <summary>
        ///     Total number of sectors.
        /// </summary>
        public readonly uint TotalSectorCount;

        /// <summary>
        ///     The underlying root directory - used by FAT32 only.
        /// </summary>
        private FATDirectory _rootDirectoryFAT32;

        /// <summary>
        ///     The cached root directory listings - used by FAT12/16 only.
        /// </summary>
        private List _rootDirectoryListings;

        /// <summary>
        ///     The underlying root directory - used by FAT32 only.
        /// </summary>
        public FATDirectory RootDirectory_FAT32
        {
            get
            {
                if (_rootDirectoryFAT32 == null)
                {
                    GetRootDirectoryListings();
                }
                return _rootDirectoryFAT32;
            }
        }

        /// <summary>
        ///     Initializes a new FAT file system from the specified partition.
        /// </summary>
        /// <param name="aPartition">The partition on which the file system resides.</param>
        /// <remarks>
        ///     You should check IsValid after creating a new FAT file system to check a valid FAT
        ///     file system has been detected.
        /// </remarks>
        public FATFileSystem(Partition aPartition)
            : base(aPartition)
        {
            //Create an array to store the BPB data.
            byte[] BPBData = thePartition.NewBlockArray(1);
            //Load the BIOS Parameter Block - the first sector of the partition.
            thePartition.ReadBlock(0UL, 1U, BPBData);

            //Check the top two bytes for the signature
            if (BPBData[510] != 0x55 ||
                BPBData[511] != 0xAA)
            {
                //If they are wrong, this is not a valid FAT file system.
                return;
            }

            //The signature is the only thing we can check to determine if this is FAT
            //  so we must now assume this is a valid FAT file system.
            isValid = true;

            //Load or calculate the various bits of data required to use a FAT file system.

            BytesPerSector = ByteConverter.ToUInt16(BPBData, 11);
            SectorsPerCluster = BPBData[13];
            BytesPerCluster = BytesPerSector*SectorsPerCluster;
            ReservedSectorCount = ByteConverter.ToUInt16(BPBData, 14);
            NumberOfFATs = BPBData[16];
            RootEntryCount = ByteConverter.ToUInt16(BPBData, 17);

            TotalSectorCount = ByteConverter.ToUInt16(BPBData, 19);
            if (TotalSectorCount == 0)
            {
                TotalSectorCount = ByteConverter.ToUInt32(BPBData, 32);
            }

            //Valid   for FAT12/16
            //Invalid for FAT32, SectorCount always 0 in this field.
            FATSectorCount = ByteConverter.ToUInt16(BPBData, 22);
            if (FATSectorCount == 0)
            {
                //FAT32 has a different, larger field for sector count
                FATSectorCount = ByteConverter.ToUInt32(BPBData, 36);
            }

            DataSectorCount = TotalSectorCount - (ReservedSectorCount + NumberOfFATs*FATSectorCount);

            // Computation rounds down. 
            ClusterCount = DataSectorCount/SectorsPerCluster;
            // Determine the FAT type. 
            //This is the official and proper way to determine FAT type - don't alter it.
            //      - If you want to implement some hack, add a new FAT file system class /
            //          implementation and add it to the appropriate partition initialisation
            //          methods
            // Comparisons are purposefully < and not <=
            // FAT16 starts at 4085, FAT32 starts at 65525 
            if (ClusterCount < 4085)
            {
                FATType = FATTypeEnum.FAT12;
            }
            else if (ClusterCount < 65525)
            {
                FATType = FATTypeEnum.FAT16;
            }
            else
            {
                FATType = FATTypeEnum.FAT32;
            }

            if (FATType == FATTypeEnum.FAT32)
            {
                RootCluster = ByteConverter.ToUInt32(BPBData, 44);
            }
            else
            {
                RootSector = ReservedSectorCount + NumberOfFATs*FATSectorCount;
                RootSectorCount = (RootEntryCount*32 + (BytesPerSector - 1))/BytesPerSector;
            }
            DataSector = ReservedSectorCount + NumberOfFATs*FATSectorCount + RootSectorCount;
        }

        /// <summary>
        ///     Creates a new byte array of the size of one cluster.
        /// </summary>
        /// <returns>The new byte array.</returns>
        public byte[] NewClusterArray()
        {
            //BasicConsole.WriteLine(((Framework.String)"Attempting to allocate ") + BytesPerCluster + " bytes");
            //BasicConsole.WriteLine(((Framework.String)"Heap free mem (bytes): ") + (Heap.FBlock->size - (Heap.FBlock->used * Heap.FBlock->bsize)));
            byte[] result = new byte[BytesPerCluster];
            //BasicConsole.WriteLine(((Framework.String)"Heap free mem (bytes): ") + (Heap.FBlock->size - (Heap.FBlock->used * Heap.FBlock->bsize)));
            return result;
        }

        /// <summary>
        ///     Reads the specified cluster from the disk into the specified array.
        /// </summary>
        /// <param name="aCluster">The cluster number to read.</param>
        /// <param name="aData">The array to store the data in.</param>
        public void ReadClusters(uint aCluster, uint numClusters, byte[] aData)
        {
            //Translate relative cluster to absolute cluster which is then
            //  converted to absolute sector number relative to the start 
            //  of the partition.
            ulong xSector = DataSector + (aCluster - 2)*SectorsPerCluster;
            thePartition.ReadBlock(xSector, SectorsPerCluster*numClusters, aData);
        }

        /// <summary>
        ///     Writes the specified data to specified cluster number on the disk.
        /// </summary>
        /// <param name="aCluster">The cluster number to write to.</param>
        /// <param name="aData">The data to write.</param>
        public void WriteCluster(uint aCluster, byte[] aData)
        {
            //See ReadCluster.
            ulong xSector = DataSector + (aCluster - 2)*SectorsPerCluster;
            thePartition.WriteBlock(xSector, SectorsPerCluster, aData);
        }

        /// <summary>
        ///     Reads the cluster numbers in a cluster chain starting at the specified cluster number.
        /// </summary>
        /// <param name="fileSize">The size of file being read (used only for estimating number of clusters). Must be non-zero.</param>
        /// <param name="FirstClusterNum">The first cluster number in the chain.</param>
        /// <returns>The list of cluster numbers in the chain.</returns>
        public UInt32List ReadClusterChain(ulong fileSize, uint FirstClusterNum)
        {
            //The capacity calculation is designed to make the internal array of the list exactly
            //  the correct size (or one bigger) for the number of cluster numbers in the chain.
            UInt32List Result = new UInt32List((int)((uint)fileSize/(SectorsPerCluster*BytesPerSector)) + 1);

            //Preallocated array for a sector of data. Used to store table data
            byte[] SectorBuffer = new byte[BytesPerSector];
            //The sector number of the sector which contains the cluster chain information
            //  for the current cluster number
            ulong SectorNum = 0;
            //Whether the current sector has been loaded or not. This allows us to load a given sector
            //  the minimum number of times. If the cluster chain stays all within one sector, then 
            //  there's no need to keep reloading the sector.
            bool SectorLoaded = false;
            //The current cluster number in the chain
            uint ClusterNum = FirstClusterNum;

            //The sector number and offset for the next entry in the cluster chain
            ulong NextSectorNum;
            uint NextSectorOffset;

            //We need to do this at least once to read in the value for the starting cluster number
            do
            {
                //Get the sector number and offset for the current cluster num in the table.
                NextSectorNum = GetFATTableSectorPosition_SectorNum(ClusterNum);
                NextSectorOffset = GetFATTableSectorPosition_Offset(ClusterNum);

                //Load the sector if it hasn't already been loaded
                if (SectorLoaded == false || SectorNum != NextSectorNum)
                {
                    ReadFATSector(NextSectorNum, SectorBuffer);
                    SectorNum = NextSectorNum;
                    SectorLoaded = true;
                }

                //Add the current cluster number
                Result.Add(ClusterNum);

                //Read the entry in the table for the current cluster number
                ClusterNum = ReadFATEntry(SectorBuffer, ClusterNum, NextSectorOffset);
            }
                //Keep looping reading the chain until we reach the end of the file.
            while (!FATEntryIndicatesEOF(ClusterNum));

            return Result;
        }

        /// <summary>
        ///     Gets the sector number containing the FAT data and offset in that sector for the specified cluster number.
        /// </summary>
        /// <param name="ClusterNum">The cluster number.</param>
        /// <returns>The sector number and offset within the sector.</returns>
        public uint GetFATTableSectorPosition_SectorNum(uint ClusterNum)
        {
            uint offset = 0;
            if (FATType == FATTypeEnum.FAT12)
            {
                // Multiply by 1.5 without using floating point, the divide by 2 rounds DOWN
                offset = ClusterNum + ClusterNum/2;
            }
            else if (FATType == FATTypeEnum.FAT16)
            {
                offset = ClusterNum*2;
            }
            else if (FATType == FATTypeEnum.FAT32)
            {
                offset = ClusterNum*4;
            }
            return offset/BytesPerSector;
        }

        /// <summary>
        ///     Gets the sector number containing the FAT data and offset in that sector for the specified cluster number.
        /// </summary>
        /// <param name="ClusterNum">The cluster number.</param>
        /// <returns>The sector number and offset within the sector.</returns>
        public uint GetFATTableSectorPosition_Offset(uint ClusterNum)
        {
            uint offset = 0;
            if (FATType == FATTypeEnum.FAT12)
            {
                // Multiply by 1.5 without using floating point, the divide by 2 rounds DOWN
                offset = ClusterNum + ClusterNum/2;
            }
            else if (FATType == FATTypeEnum.FAT16)
            {
                offset = ClusterNum*2;
            }
            else if (FATType == FATTypeEnum.FAT32)
            {
                offset = ClusterNum*4;
            }
            return offset%BytesPerSector;
        }

        /// <summary>
        ///     Reads the specified sector of the FAT into the specified data array.
        /// </summary>
        /// <param name="xSectorNum">The sector number of the FAT to read.</param>
        /// <param name="aData">The byte array to read the data into.</param>
        public void ReadFATSector(ulong xSectorNum, byte[] aData)
        {
            thePartition.ReadBlock(ReservedSectorCount + xSectorNum, 1, aData);
        }

        /// <summary>
        ///     Writes the specified FAT data to the specified sector of the FAT on disk.
        /// </summary>
        /// <param name="xSectorNum">The sector number to write.</param>
        /// <param name="aData">The FAT sector data to write.</param>
        public void WriteFATSector(ulong xSectorNum, byte[] aData)
        {
            thePartition.WriteBlock(ReservedSectorCount + xSectorNum, 1, aData);
        }

        /// <summary>
        ///     Reads the FAT specified entry number (cluster number) from the specified FAT sector data.
        /// </summary>
        /// <param name="aFATTableSector">The FAT sector data containing the FAT entry to be read.</param>
        /// <param name="aClusterNum">The entry (cluster number) to read.</param>
        /// <param name="aOffset">The offset within the sector that the entry is at.</param>
        /// <returns>The entry's value.</returns>
        public uint ReadFATEntry(byte[] aFATTableSector, uint aClusterNum, uint aOffset)
        {
            if (FATType == FATTypeEnum.FAT12)
            {
                if (aOffset == BytesPerSector - 1)
                {
                    ExceptionMethods.Throw(new Exception("TODO: Sector Span"));
                    /* This cluster access spans a sector boundary in the FAT *
                     * There are a number of strategies to handling this. The *
                     * easiest is to always load FAT sectors into memory in   *
                     * pairs if the volume is FAT12 (if you want to load FAT  *
                     * sector N, you also load FAT sector N+1 immediately     *
                     * following it in memory unless sector N is the last FAT *
                     * sector).                                               */
                }
                // We now access the FAT entry as a WORD just as we do for FAT16, but if the cluster number is
                // EVEN, we only want the low 12-bits of the 16-bits we fetch. If the cluster number is ODD
                // we want the high 12-bits of the 16-bits we fetch. 
                uint xResult = ByteConverter.ToUInt16(aFATTableSector, aOffset);
                if ((aClusterNum & 0x01) == 0)
                {
                    // Even
                    return xResult & 0x0FFF;
                }
                // Odd
                return xResult >> 4;
            }
            if (FATType == FATTypeEnum.FAT16)
            {
                return ByteConverter.ToUInt16(aFATTableSector, aOffset);
            }
            return ByteConverter.ToUInt32(aFATTableSector, aOffset) & 0x0FFFFFFFu;
        }

        /// <summary>
        ///     Writes the specified value to the specified FAT entry number in the FAT sector data array.
        /// </summary>
        /// <param name="aFATTableSector">The FAT sector data.</param>
        /// <param name="aClusterNum">The cluster number to write.</param>
        /// <param name="aOffset">The offset within the FAT sector data of the entry to write.</param>
        /// <param name="FATEntry">The value to write.</param>
        public void WriteFATEntry(byte[] aFATTableSector, uint aClusterNum, uint aOffset, uint FATEntry)
        {
            if (FATType == FATTypeEnum.FAT12)
            {
                if (aOffset == BytesPerSector - 1)
                {
                    ExceptionMethods.Throw(new Exception("TODO: Sector Span"));
                    /* This cluster access spans a sector boundary in the FAT */
                    /* There are a number of strategies to handling this. The */
                    /* easiest is to always load FAT sectors into memory */
                    /* in pairs if the volume is FAT12 (if you want to load */
                    /* FAT sector N, you also load FAT sector N+1 immediately */
                    /* following it in memory unless sector N is the last FAT */
                    /* sector). It is assumed that this is the strategy used here */
                    /* which makes this if test for a sector boundary span */
                    /* unnecessary. */
                }
                // We now access the FAT entry as a WORD just as we do for FAT16, but if the cluster number is
                // EVEN, we only want the low 12-bits of the 16-bits we fetch. If the cluster number is ODD
                // we want the high 12-bits of the 16-bits we fetch. 
                if ((aClusterNum & 0x01) == 0)
                {
                    // Even
                    FATEntry &= 0x0FFF;

                    aFATTableSector[aOffset] = (byte)FATEntry;
                    aFATTableSector[aOffset + 1] =
                        (byte)((uint)(aFATTableSector[aOffset + 1] & 0xF0) | (FATEntry >> 8));
                }
                else
                {
                    // Odd
                    FATEntry <<= 4;
                    aFATTableSector[aOffset] = (byte)((uint)(aFATTableSector[aOffset] & 0x0F) | FATEntry);
                    aFATTableSector[aOffset + 1] = (byte)(FATEntry >> 8);
                }
            }
            else if (FATType == FATTypeEnum.FAT16)
            {
                aFATTableSector[aOffset] = (byte)FATEntry;
                aFATTableSector[aOffset + 1] = (byte)(FATEntry >> 8);
            }
            else
            {
                FATEntry = FATEntry & 0x0FFFFFFFu;
                aFATTableSector[aOffset + 0] = (byte)FATEntry;
                aFATTableSector[aOffset + 1] = (byte)(FATEntry >> 4);
                aFATTableSector[aOffset + 2] = (byte)(FATEntry >> 8);
                aFATTableSector[aOffset + 3] = (byte)(FATEntry >> 12);
                aFATTableSector[aOffset + 4] = (byte)(FATEntry >> 16);
                aFATTableSector[aOffset + 5] = (byte)(FATEntry >> 20);
                aFATTableSector[aOffset + 6] = (byte)(FATEntry >> 24);
                // --- DO NOT WRITE TOP 4 BITS --- (as per spec)
            }
        }

        /// <summary>
        ///     Determines whether the FAT entry value indicates end-of-file or not.
        /// </summary>
        /// <param name="aValue">The value to test.</param>
        /// <returns>Whether the FAT entry value indicates end-of-file or not.</returns>
        public bool FATEntryIndicatesEOF(uint aValue)
        {
            return aValue >= GetFATEntryEOFValue(FATType);
        }

        /// <summary>
        ///     Determines whether the FAT entry value indicates a free cluster or not.
        /// </summary>
        /// <param name="aValue">The value to test.</param>
        /// <returns>Whether the FAT entry value indicates a free cluster or not.</returns>
        public bool FATEntryIndicatesFree(uint aValue)
        {
            return aValue == 0;
        }

        /// <summary>
        ///     Gets the EOF value for the specified FAT type.
        /// </summary>
        /// <param name="aFATType">The FAT type.</param>
        /// <returns>The EOF value.</returns>
        public static uint GetFATEntryEOFValue(FATTypeEnum aFATType)
        {
            if (aFATType == FATTypeEnum.FAT12)
            {
                return 0x0FF8;
            }
            if (aFATType == FATTypeEnum.FAT16)
            {
                return 0xFFF8;
            }
            return 0x0FFFFFF8;
        }

        /// <summary>
        ///     Gets the next free cluster number after the specified cluster number.
        /// </summary>
        /// <param name="startCluster">The cluster number to start searching from.</param>
        /// <returns>The next free cluster number.</returns>
        /// <remarks>
        ///     At the time of writing, this method's behavior was undefined if no free clusters were left.
        ///     Predicted behavior is that it would either enter an infinite loop or cause an exception if no
        ///     free clusters are available.
        /// </remarks>
        public uint GetNextFreeCluster(uint startCluster)
        {
            byte[] SectorBuffer = new byte[BytesPerSector];
            ulong SectorNum = 0;
            uint SectorOffset = 0;
            uint ClusterNum = startCluster;
            uint ClusterPointedTo = 0xF;

            do
            {
                SectorNum = GetFATTableSectorPosition_SectorNum(ClusterNum);
                SectorOffset = GetFATTableSectorPosition_Offset(ClusterNum);

                ReadFATSector(SectorNum, SectorBuffer);

                ClusterPointedTo = ReadFATEntry(SectorBuffer, ClusterNum, SectorOffset);
            } while (!FATEntryIndicatesFree(ClusterPointedTo) && ++ClusterNum < ClusterCount);

            if (ClusterNum == ClusterCount)
            {
                ExceptionMethods.Throw(new IndexOutOfRangeException(ClusterNum, ClusterCount));
            }

            return ClusterNum;
        }

        /// <summary>
        ///     Sets the specified FAT entry to the specified value and saves it to disk.
        /// </summary>
        /// <param name="ClusterNum">The cluster number to set.</param>
        /// <param name="Value">The value to set to.</param>
        public void SetFATEntryAndSave(uint ClusterNum, uint Value)
        {
            //Get the table sector location for the specified cluster number
            uint SectorNum = GetFATTableSectorPosition_SectorNum(ClusterNum);
            uint SectorOffset = GetFATTableSectorPosition_Offset(ClusterNum);
            //Create an array to hold the table sector data
            byte[] sectorData = new byte[BytesPerSector];
            //Read the existing table sector data
            ReadFATSector(SectorNum, sectorData);
            //Set the table entry
            WriteFATEntry(sectorData, SectorNum, SectorOffset, Value);
            //Write the table sector data back to disk
            WriteFATSector(SectorNum, sectorData);

            CleanDiskCaches();
        }

        /// <summary>
        ///     Gets the root directory listings in the FAT file system.
        /// </summary>
        /// <returns>The root directory listings.</returns>
        public List GetRootDirectoryListings()
        {
            if (FATType == FATTypeEnum.FAT32)
            {
                if (_rootDirectoryFAT32 == null)
                {
                    _rootDirectoryFAT32 = new FATDirectory(this, null, "ROOT", RootCluster);
                }
                return _rootDirectoryFAT32.GetListings();
            }
            if (_rootDirectoryListings == null)
            {
                byte[] xData = thePartition.TheDiskDevice.NewBlockArray(RootSectorCount);
                thePartition.ReadBlock(RootSector, RootSectorCount, xData);

                _rootDirectoryListings = ParseDirectoryTable(xData, xData.Length, null);
            }
            return _rootDirectoryListings;
        }

        /// <summary>
        ///     Parses the specified directory file data for its listings.
        /// </summary>
        /// <param name="xData">The directory data.</param>
        /// <param name="xDataLength">The directory data length.</param>
        /// <param name="thisDir">
        ///     The FAT directory the FAT data is from.
        ///     Used when creating listings as the parent directory.
        /// </param>
        /// <returns>The directory listings.</returns>
        public List ParseDirectoryTable(byte[] xData, int xDataLength, FATDirectory thisDir)
        {
            List xResult = new List();

            //BasicConsole.WriteLine("Parsing listings...");

            String xLongName = "";
            for (uint i = 0; i < xDataLength; i = i + 32)
            {
                byte xAttrib = xData[i + 11];
                if (xAttrib == ListingAttribs.LongName)
                {
                    byte xType = xData[i + 12];
                    if (xType == 0)
                    {
                        byte xOrd = xData[i];
                        if ((xOrd & 0x40) > 0)
                        {
                            xLongName = "";
                        }
                        //TODO: Check LDIR_Ord for ordering and throw exception
                        // if entries are found out of order.
                        // Also save buffer and only copy name if a end Ord marker is found.
                        String xLongPart = ByteConverter.GetASCIIStringFromUTF16(xData, i + 1, 5);
                        //BasicConsole.WriteLine("xLongPart1: " + xLongPart);
                        // We have to check the length because 0xFFFF is a valid Unicode codepoint.
                        // So we only want to stop if the 0xFFFF is AFTER a 0x0000. We can determine
                        // this by also looking at the length. Since we short circuit the or, the Length
                        // is rarely evaluated.
                        if (xLongPart.Length == 5)
                        {
                            xLongPart = xLongPart + ByteConverter.GetASCIIStringFromUTF16(xData, i + 14, 6);
                            //BasicConsole.WriteLine("xLongPart2: " + xLongPart);
                            if (xLongPart.Length == 11)
                            {
                                xLongPart = xLongPart + ByteConverter.GetASCIIStringFromUTF16(xData, i + 28, 2);
                                //BasicConsole.WriteLine("xLongPart3: " + xLongPart);
                            }
                        }
                        xLongName = xLongPart + xLongName;
                        //BasicConsole.WriteLine("xLongName: " + xLongName);
                        //TODO: LDIR_Chksum 
                    }
                }
                else
                {
                    byte xStatus = xData[i];
                    if (xStatus == 0x00)
                    {
                        // Empty slot, and no more entries after this
                        break;
                    }
                    if (xStatus == 0x05)
                    {
                        // Japanese characters - We dont handle these
                    }
                    else if (xStatus == 0xE5)
                    {
                        // Empty slot, skip it
                    }
                    else if (xStatus >= 0x20)
                    {
                        String xName;

                        int xTest = xAttrib & (ListingAttribs.Directory | ListingAttribs.VolumeID);

                        if (xLongName.Length > 0)
                        {
                            // Leading and trailing spaces are to be ignored according to spec.
                            // Many programs (including Windows) pad trailing spaces although it 
                            // it is not required for long names.
                            xName = xLongName.Trim();

                            // As per spec, ignore trailing periods
                            //If there are trailing periods
                            int nameIndex = xName.Length - 1;
                            if (xName[nameIndex] == '.')
                            {
                                //Search backwards till we find the first non-period character
                                for (; nameIndex > -1; nameIndex--)
                                {
                                    if (xName[nameIndex] != '.')
                                    {
                                        break;
                                    }
                                }
                                //Substring to remove the periods
                                xName = xName.Substring(0, nameIndex + 1);
                            }
                        }
                        else
                        {
                            String xEntry = ByteConverter.GetASCIIStringFromASCII(xData, i, 11);
                            //Volume ID does not have same format as file-name.
                            if (xTest == ListingAttribs.VolumeID)
                            {
                                xName = xEntry;
                            }
                            else
                            {
                                //Attempt to apply original spec:
                                // - 8 chars for filename
                                // - 3 chars for extension
                                if (xEntry.Length >= 8)
                                {
                                    xName = xEntry.Substring(0, 8).TrimEnd();

                                    if (xEntry.Length >= 11)
                                    {
                                        String xExt = xEntry.Substring(8, 3).TrimEnd();
                                        if (xExt.Length > 0)
                                        {
                                            xName += "." + xExt;
                                        }
                                    }
                                }
                                else
                                {
                                    xName = xEntry;
                                }
                            }
                        }

                        uint xFirstCluster =
                            (uint)(ByteConverter.ToUInt16(xData, i + 20) << 16 | ByteConverter.ToUInt16(xData, i + 26));

                        xName = xName.ToUpper();

                        //TODO: Store attributes in the listings

                        if (xTest == 0)
                        {
                            if (xName[xName.Length - 1] != '~')
                            {
                                uint xSize = ByteConverter.ToUInt32(xData, i + 28);
                                xResult.Add(new FATFile(this, thisDir, xName, xSize, xFirstCluster));
                            }
                        }
                        else if (xTest == ListingAttribs.VolumeID)
                        {
                            thePartition.VolumeID = xName;
                        }
                        else if (xTest == ListingAttribs.Directory)
                        {
                            xResult.Add(new FATDirectory(this, thisDir, xName, xFirstCluster));
                        }
                        xLongName = "";
                    }
                }
            }

            return xResult;
        }

        /// <summary>
        ///     Encodes the specified listings into a byte array.
        /// </summary>
        /// <param name="listings">The listings to encode.</param>
        /// <param name="includeVolumeID">Whether to include the Volume ID entry (partition name). Only true for root directory.</param>
        /// <returns>The encoded listings data.</returns>
        public byte[] EncodeDirectoryTable(List listings, bool includeVolumeID, ulong minTableSize)
        {
            int LongFilenamesSize = 0;
            for (int i = 0; i < listings.Count; i++)
            {
                if (IsLongNameListing((Base)listings[i]))
                {
                    //+1 for null terminator on long name
                    int nameLength = ((Base)listings[i]).Name.Length + 1;
                    LongFilenamesSize += nameLength/13;
                    if (nameLength%13 > 0)
                    {
                        LongFilenamesSize++;
                    }
                }
            }
            LongFilenamesSize *= 32;

            //                       +32 for VolumeID entry                         + 32 for end entry
            byte[] result = new byte[
                Math.Max(32 + listings.Count*32 + LongFilenamesSize + 32, (int)minTableSize)];

            int offset = 0;

            if (includeVolumeID)
            {
                //Volume ID entry - this is only be valid for root directory.

                List shortName = GetShortName(thePartition.VolumeID, true);

                //Put in short name entry
                byte[] DIR_Name = ByteConverter.GetASCIIBytes((String)shortName[0]);
                byte DIR_Attr = ListingAttribs.VolumeID;
                for (int j = 0; j < DIR_Name.Length; j++)
                {
                    result[j] = DIR_Name[j];
                }
                result[DIR_Name.Length] = DIR_Attr;

                offset += 32;
            }

            for (int i = 0; i < listings.Count; i++)
            {
                Base currListing = (Base)listings[i];
                if (IsLongNameListing(currListing))
                {
                    offset = EncodeLongNameListing(currListing, result, offset);
                }

                offset = EncodeShortNameListing(currListing, result, offset);
            }


            return result;
        }

        /// <summary>
        ///     Determines whether the specified listing must be encoded as a long-name listing
        ///     or not.
        /// </summary>
        /// <param name="listing">The listing to check.</param>
        /// <returns>True if it is a long-named listing. Otherwise, false.</returns>
        private bool IsLongNameListing(Base listing)
        {
            return ((String)listing.Name.Split('.')[0]).Length > 8;
        }

        /// <summary>
        ///     Encodes the specified listing as a long-name listing and
        ///     sets the encoded data in the specified array starting at
        ///     the specified offset. Does not encode the short-name
        ///     listing that must immediately follow the long-name listings.
        /// </summary>
        /// <param name="listing">The listing to encode.</param>
        /// <param name="result">The array to set the encoded data in.</param>
        /// <param name="offset">The offset in the array to start storing data at.</param>
        /// <returns>The offset in the array to the first byte after the new encoded data.</returns>
        /// <remarks>
        ///     It is assumed that the data array is big enough to hold all the long name listings.
        /// </remarks>
        private int EncodeLongNameListing(Base listing, byte[] result, int offset)
        {
            //Long name entries only
            String longName = listing.Name;
            String shortName;

            List shortNameParts = GetShortName(longName, listing.IsDirectory);
            if (shortNameParts.Count == 2)
            {
                shortName = (String)shortNameParts[0] + (String)shortNameParts[1];
            }
            else
            {
                shortName = (String)shortNameParts[0];
            }
            byte ShortNameChecksum = CalculateShortNameCheckSum(ByteConverter.GetASCIIBytes(shortName));

            longName += (char)0;
            int nameLengthDiff = 13 - longName.Length%13;

            if (nameLengthDiff < 13)
            {
                longName = longName.PadRight(longName.Length + nameLengthDiff, (char)0xFFFF);
            }

            int longNameLength = longName.Length;
            int NumNameParts = longNameLength/13;
            bool first = true;
            for (int i = NumNameParts - 1; i > -1; i--)
            {
                String currPart = longName.Substring(i*13, 13);

                byte[] UTF16Bytes = ByteConverter.GetUTF16Bytes(currPart, 0, currPart.Length);

                //[offset+ 0] = Order of entry in sequence
                //[offset+ 1] = LSB UTF16Bytes[0]
                //[   ...   ] = ...
                //[offset+10] = MSB UTF16Bytes[9]
                //[offset+11] = Attribute - LongName
                //[offset+12] = Entry type. 0 for name entries.
                //[offset+13] = Short name checksum
                //[offset+14] = LSB UTF16Bytes[10]
                //[   ...   ] = ...
                //[offset+25] = MSB UTF16Bytes[21]
                //[offset+26] = 0
                //[offset+27] = 0
                //[offset+28] = LSB UTF16Bytes[22]
                //[   ...   ] = ...
                //[offset+31] = MSB UTF16Bytes[25]

                result[offset + 0] = (byte)(i + 1);
                if (first)
                {
                    result[offset + 0] = (byte)(result[offset + 0] | 0x40);
                    first = false;
                }
                for (int j = 0; j < 10; j++)
                {
                    result[offset + 1 + j] = UTF16Bytes[j];
                }
                result[offset + 11] = ListingAttribs.LongName;
                result[offset + 12] = 0;
                result[offset + 13] = ShortNameChecksum;
                for (int j = 10, k = 0; j < 22; j++, k++)
                {
                    result[offset + 14 + k] = UTF16Bytes[j];
                }
                result[offset + 26] = 0;
                result[offset + 27] = 0;
                for (int j = 22, k = 0; j < 26; j++, k++)
                {
                    result[offset + 28 + k] = UTF16Bytes[j];
                }

                offset += 32;
            }

            return offset;
        }

        /// <summary>
        ///     Encodes the specified listing as a short-name listing and
        ///     sets the encoded data in the specified array starting at
        ///     the specified offset.
        /// </summary>
        /// <param name="listing">The listing to encode.</param>
        /// <param name="result">The array to set the encoded data in.</param>
        /// <param name="offset">The offset in the array to start storing data at.</param>
        /// <returns>The offset in the array to the first byte after the new encoded data.</returns>
        /// <remarks>
        ///     It is assumed that the data array is big enough to hold all the short name listings.
        /// </remarks>
        private int EncodeShortNameListing(Base listing, byte[] result, int offset)
        {
            //Short name entry only

            String longName = listing.Name;
            List shortNameParts = GetShortName(longName, listing.IsDirectory);
            String shortName;
            if (shortNameParts.Count == 2)
            {
                shortName = (String)shortNameParts[0] + (String)shortNameParts[1];
            }
            else
            {
                shortName = (String)shortNameParts[0];
            }

            byte[] ASCIIBytes = ByteConverter.GetASCIIBytes(shortName);
            for (int i = 0; i < ASCIIBytes.Length; i++)
            {
                result[offset + i] = ASCIIBytes[i];
            }
            //TODO: Encode other attributes from listings
            if (listing.IsDirectory)
            {
                result[offset + 11] = ListingAttribs.Directory;
            }
            //TODO: CrtTimeTenth
            //TODO: CrtTime
            //TODO: CrtDate
            //TODO: LstAccDate

            uint firstClusterNum;
            if (listing.IsDirectory)
            {
                firstClusterNum = ((FATDirectory)listing).FirstClusterNum;
            }
            else
            {
                firstClusterNum = ((FATFile)listing).FirstClusterNum;
            }

            result[offset + 20] = (byte)(firstClusterNum >> 16);
            result[offset + 21] = (byte)(firstClusterNum >> 24);

            //TODO: WrtTime
            //TODO: WrtDate

            result[offset + 26] = (byte)firstClusterNum;
            result[offset + 27] = (byte)(firstClusterNum >> 8);

            if (!listing.IsDirectory)
            {
                uint fileSize = (uint)listing.Size;

                result[offset + 28] = (byte)fileSize;
                result[offset + 29] = (byte)(fileSize >> 8);
                result[offset + 30] = (byte)(fileSize >> 16);
                result[offset + 31] = (byte)(fileSize >> 24);
            }

            offset += 32;

            return offset;
        }

        /// <summary>
        ///     Gets the short name for the specified long name.
        /// </summary>
        /// <param name="longName">The long name to shorten.</param>
        /// <param name="isDirectory">Whether the long name is for a directory or not.</param>
        /// <returns>The short name parts. Directory=1 part, file=2 parts (name + extension).</returns>
        private static List GetShortName(String longName, bool isDirectory)
        {
            if (isDirectory)
            {
                List result = new List(1);
                result.Add(longName.Substring(0, 8).PadRight(11, ' '));
                return result;
            }
            else
            {
                List result = new List(2);
                List nameParts = longName.Split('.');
                if (nameParts.Count > 1)
                {
                    result.Add(((String)nameParts[0]).Substring(0, 8).PadRight(8, ' '));
                    result.Add(((String)nameParts[1]).Substring(0, 3).PadRight(3, ' '));
                }
                else
                {
                    result.Add(longName.Substring(0, 8).PadRight(8, ' '));
                    result.Add(((String)"").PadRight(3, ' '));
                }
                return result;
            }
        }

        /// <summary>
        ///     Calculates the short name checksum.
        /// </summary>
        /// <param name="shortNameBytes">The short name.</param>
        /// <returns>The checksum value.</returns>
        private static byte CalculateShortNameCheckSum(byte[] shortNameBytes)
        {
            short FcbNameLen;
            byte Sum;

            Sum = 0;
            int charIdx = 0;
            for (FcbNameLen = 11; FcbNameLen != 0; FcbNameLen--, charIdx++)
            {
                // NOTE: The operation is an unsigned char rotate right
                Sum = (byte)(((Sum & 1) == 0x1 ? 0x80 : 0) + (Sum >> 1) + shortNameBytes[charIdx]);
            }
            return Sum;
        }

        /// <summary>
        ///     Gets the listing for the specified file or directory.
        /// </summary>
        /// <param name="aName">The full path to the file or directory.</param>
        /// <returns>The listing or null if not found.</returns>
        public override Base GetListing(String aName)
        {
            if (aName == "")
            {
                return RootDirectory_FAT32;
            }
            List nameParts = aName.Split(FileSystemManager.PathDelimiter);
            List listings = GetRootDirectoryListings();
            return GetListingFromListings(nameParts, null, listings);
        }

        /// <summary>
        ///     Creates a new directory within the file system.
        /// </summary>
        /// <param name="name">The name of the directory to create.</param>
        /// <param name="parent">The parent directory of the new directory.</param>
        /// <returns>The new directory listing.</returns>
        public override Directory NewDirectory(String name, Directory parent)
        {
            if (FATType != FATTypeEnum.FAT32)
            {
                ExceptionMethods.Throw(
                    new NotSupportedException("FATFileSystem.NewDirectory for non-FAT32 not supported!"));
            }
            if (parent == null)
            {
                ExceptionMethods.Throw(new NullReferenceException());
            }
            if (!(parent is FATDirectory))
            {
                ExceptionMethods.Throw(
                    new NotSupportedException(
                        "FATFileSystem.NewDirectory parent directory must be of type FATDirectory!"));
            }

            //BasicConsole.WriteLine("Getting listings...");
            List listings = parent.GetListings();

            //BasicConsole.WriteLine("Got listings. Converting name to upper...");
            name = name.ToUpper();

            //BasicConsole.WriteLine("Name converted. Checking listing exists...");
            bool exists = Directory.ListingExists(name, listings);
            //BasicConsole.WriteLine("Checked.");
            if (!exists)
            {
                //BasicConsole.WriteLine("Getting next free cluster...");
                uint freeCluster = GetNextFreeCluster(2);
                //BasicConsole.WriteLine("Got next free. Clearing cluster...");
                WriteCluster(freeCluster, null);
                //BasicConsole.WriteLine("Cleared. Setting FAT entry...");
                SetFATEntryAndSave(freeCluster, GetFATEntryEOFValue(FATType));
                //BasicConsole.WriteLine("Set FAT entry. Creating new directory...");
                FATDirectory newDir = new FATDirectory(this, (FATDirectory)parent, name, freeCluster);
                //BasicConsole.WriteLine("Adding listing to parent...");
                parent.AddListing(newDir);
                //BasicConsole.WriteLine("Added listing. Writing listings...");
                parent.WriteListings();
                //BasicConsole.WriteLine("Written listings.");
                return newDir;
            }
            ExceptionMethods.Throw(new IOException("Listing (directory/file) with specified name already exists!"));
            return null;
        }

        /// <summary>
        ///     Creates a new file within the file system.
        /// </summary>
        /// <param name="name">The name of the file to create.</param>
        /// <param name="parent">The parent directory of the new file.</param>
        /// <returns>The new file listing.</returns>
        public override File NewFile(String name, Directory parent)
        {
            if (FATType != FATTypeEnum.FAT32)
            {
                ExceptionMethods.Throw(new NotSupportedException("FATFileSystem.NewFile for non-FAT32 not supported!"));
            }
            if (parent == null)
            {
                ExceptionMethods.Throw(new NullReferenceException());
            }
            if (!(parent is FATDirectory))
            {
                ExceptionMethods.Throw(
                    new NotSupportedException("FATFileSystem.NewFile parent directory must be of type FATDirectory!"));
            }

            //BasicConsole.WriteLine("Getting directory listings...");

            List listings = null;
            if (parent == null)
            {
                listings = GetRootDirectoryListings();
            }
            else
            {
                listings = parent.GetListings();
            }

            //BasicConsole.WriteLine("Got directory listings. Converting name...");

            name = name.ToUpper();

            //BasicConsole.WriteLine("Converted name. Checking if file exists...");

            bool exists = Directory.ListingExists(name, listings);

            //BasicConsole.WriteLine("Check done.");

            if (!exists)
            {
                //BasicConsole.WriteLine("Getting next free cluster...");
                uint freeCluster = GetNextFreeCluster(2);
                //BasicConsole.WriteLine("Got next free. Clearing cluster...");
                WriteCluster(freeCluster, null);
                //BasicConsole.WriteLine("Cleared. Setting FAT entry...");
                SetFATEntryAndSave(freeCluster, GetFATEntryEOFValue(FATType));
                //BasicConsole.WriteLine("Set FAT entry. Creating new file...");
                File newFile = new FATFile(this, (FATDirectory)parent, name, 0, freeCluster);
                //BasicConsole.WriteLine("File created. Adding listing to parent...");
                if (parent == null)
                {
                    listings.Add(newFile);
                    //BasicConsole.WriteLine("Added. Writing listings...");
                    _rootDirectoryFAT32.WriteListings();
                }
                else
                {
                    parent.AddListing(newFile);
                    //BasicConsole.WriteLine("Added. Writing listings...");
                    parent.WriteListings();
                }
                //BasicConsole.WriteLine("Written listings.");
                return newFile;
            }
            ExceptionMethods.Throw(new IOException("Listing (directory/file) with specified name already exists!"));
            return null;
        }

        /// <summary>
        ///     Formats the specified partition as FAT32.
        /// </summary>
        /// <param name="thePartition">The partition to format.</param>
        public static void FormatPartitionAsFAT32(Partition thePartition)
        {
#if FATFS_TRACE
            BasicConsole.WriteLine(((Framework.String)"Creating block array... Block size: ") + thePartition.BlockSize);
#endif

            byte[] newBPBData = thePartition.TheDiskDevice.NewBlockArray(1);

#if FATFS_TRACE
            BasicConsole.WriteLine("Block array created.");
#endif

            //FAT signature
            newBPBData[510] = 0x55;
            newBPBData[511] = 0xAA;

            //Bytes per sector - 512
            ushort bytesPerSector = 512;
            newBPBData[11] = (byte)bytesPerSector;
            newBPBData[12] = (byte)(bytesPerSector >> 8);
            ulong partitionSize = thePartition.Blocks*thePartition.BlockSize;

#if FATFS_TRACE
            BasicConsole.WriteLine(((Framework.String)"partitionSize: ") + partitionSize);
#endif

            byte sectorsPerCluster = 0x1;
            //See http://www.win.tue.nl/~aeb/linux/fs/fat/fat-1.html
            if (partitionSize < 0x10400000UL /*(260MiB)*/)
            {
                sectorsPerCluster = 0x1; //1
            }
            else if (partitionSize < 0x200000000 /*(8GiB)*/)
            {
                sectorsPerCluster = 0x8; //8
            }
            else if (partitionSize < 0x400000000UL /*(16GiB)*/)
            {
                sectorsPerCluster = 0x10; //16
            }
            else if (partitionSize < 0x800000000UL /*(32GiB)*/)
            {
                sectorsPerCluster = 0x20; //32
            }
            else if (partitionSize < 0x20000000000UL /*(2TiB)*/)
            {
                sectorsPerCluster = 0x40; //64
            }
            //Max. 2TB - if greater, then error!
            else
            {
                ExceptionMethods.Throw(new NotSupportedException("Drive too big! Max. size 2TB for FAT32."));
            }
            //Sectors per cluster - 32 KiB clusters = 64 sectors per cluster
            newBPBData[13] = sectorsPerCluster;
            //Reserved sector count - 32 for FAT32 (by convention... and FAT32 does not imply 32 sectors)
            ushort reservedSectors = 32;
            newBPBData[14] = (byte)reservedSectors;
            newBPBData[15] = (byte)(reservedSectors >> 8);
            //Number of FATs - always 2
            newBPBData[16] = 0x02;
            //Root entry count - always 0 for FAT32
            // - Do nothing

            //Total sector count
            // - At newBPBData[19] - N/A for FAT32
            //      - Do nothing
            // - At newBPBData[32] - Total number of sectors in the file system
            uint totalSectors = (uint)thePartition.Blocks;
            newBPBData[32] = (byte)totalSectors;
            newBPBData[33] = (byte)(totalSectors >> 8);
            newBPBData[34] = (byte)(totalSectors >> 16);
            newBPBData[35] = (byte)(totalSectors >> 24);


            //FAT sector count
            // - At newBPBData[22] - always 0 for FAT32
            // - At newBPBData[36] - See calculation below

            //FAT sector count = 2 * RoundUp(Number of bytes for 1 FAT / Bytes per sector)

#if FATFS_TRACE
            BasicConsole.WriteLine(((Framework.String)"totalSectors: ") + totalSectors +
                                                       ", reservedSectors: " + reservedSectors +
                                                       ", sectorsPerCluster: " + sectorsPerCluster +
                                                       ", bytesPerSector: " + bytesPerSector);
#endif

            // Number of bytes for 2 FAT  = 4 * Number of data clusters
            //                            = 4 * (RndDown((totalSectors - ReservedSectors) / sectorsPerCluster) - RndUp(Clusters for 2 FATs))
            //               bytesPer2FAT = 4 * (X - RndUp((bytesPerFAT * 2) / bytesPerCluster))
            //               bytesPer2FAT = (4 * X * bytesPerCluster) / (bytesPerCluster + 8)
            uint dataClusters = (totalSectors - reservedSectors)/sectorsPerCluster;
#if FATFS_TRACE
            BasicConsole.WriteLine(((Framework.String)"dataClusters: ") + dataClusters);
#endif
            uint bytesPerCluster = (uint)sectorsPerCluster*bytesPerSector;
#if FATFS_TRACE
            BasicConsole.WriteLine(((Framework.String)"bytesPerCluster: ") + bytesPerCluster);
            BasicConsole.WriteLine(((Framework.String)"4 * dataClusters: ") + (4 * dataClusters));
            BasicConsole.WriteLine(((Framework.String)"4 * dataClusters * bytesPerCluster: ") + (4 * dataClusters * bytesPerCluster));
            BasicConsole.WriteLine(((Framework.String)"bytesPerCluster + 8: ") + (bytesPerCluster + 8));
#endif

            uint bytesPer2FAT = (uint)Math.Divide(4*(ulong)dataClusters*bytesPerCluster, bytesPerCluster + 8);
            //Calculation rounds down
#if FATFS_TRACE
            BasicConsole.WriteLine(((Framework.String)"bytesPer2FAT: ") + bytesPer2FAT);
#endif
            uint FATSectorCount = bytesPer2FAT/bytesPerSector;
#if FATFS_TRACE
            BasicConsole.WriteLine(((Framework.String)"FATSectorCount: ") + FATSectorCount);
#endif
            newBPBData[36] = (byte)FATSectorCount;
            newBPBData[37] = (byte)(FATSectorCount >> 8);
            newBPBData[38] = (byte)(FATSectorCount >> 16);
            newBPBData[39] = (byte)(FATSectorCount >> 24);

#if FATFS_TRACE
            BasicConsole.WriteLine(((Framework.String)"totalSectors: ") + totalSectors +
                                                       ", reservedSectors: " + reservedSectors +
                                                       ", sectorsPerCluster: " + sectorsPerCluster +
                                                       ", bytesPerSector: " + bytesPerSector +
                                                       ", bytesPerCluster: " + bytesPerCluster +
                                                       ", dataClusters: " + dataClusters +
                                                       ", bytesPer2FAT: " + bytesPer2FAT +
                                                       ", FATSectorCount: " + FATSectorCount);
            BasicConsole.DelayOutput(10);
#endif

            //Root cluster (number/index - min value is 2)
            newBPBData[44] = 0x02;

#if FATFS_TRACE
            BasicConsole.WriteLine("Writing new BPB...");
            BasicConsole.DelayOutput(1);
#endif

            thePartition.WriteBlock(0UL, 1U, newBPBData);

#if FATFS_TRACE
            BasicConsole.WriteLine("Written new BPB. Attempting to load new file system...");
#endif

            thePartition.CleanCaches();

            FATFileSystem fs = new FATFileSystem(thePartition);
            if (!fs.IsValid)
            {
#if FATFS_TRACE
                BasicConsole.WriteLine("Failed to format properly. Scrubbing new BPB...");
#endif
                thePartition.WriteBlock(0UL, 1U, null);
#if FATFS_TRACE
                BasicConsole.WriteLine("Scrub done.");
#endif

                ExceptionMethods.Throw(
                    new Exception("Failed to format properly! FATFileSystem did not recognise system as valid."));
            }
            else if (fs.FATType != FATTypeEnum.FAT32)
            {
#if FATFS_TRACE
                BasicConsole.WriteLine("Failed to format properly. Scrubbing new BPB...");
#endif
                byte[] scrubBPB = thePartition.TheDiskDevice.NewBlockArray(1);
                thePartition.WriteBlock(0UL, 1U, scrubBPB);
#if FATFS_TRACE
                BasicConsole.WriteLine("Scrub done.");
#endif

                ExceptionMethods.Throw(
                    new Exception(
                        (String)
                            "Failed to format properly! FATFileSystem recognised incorrect FAT type. Type recognised: " +
                        (uint)fs.FATType));
            }

#if FATFS_TRACE
            BasicConsole.WriteLine("FAT recognised. Setting up empty FAT table...");
            try
            {
#endif
            //Mark all clusters as empty
            fs.ThePartition.WriteBlock(fs.ReservedSectorCount, FATSectorCount, null);
#if FATFS_TRACE
            }
            catch
            {
                BasicConsole.WriteLine("Failed to clear potentially pre-existing FAT table! File system may not function as expected.");
                Framework.GC.Cleanup();
                Framework.GC.Cleanup();
            }

            BasicConsole.WriteLine("Marking root directory cluster as used...");
#endif
            //Mark root cluster as being 1 cluster in size.
            fs.SetFATEntryAndSave(2, GetFATEntryEOFValue(FATTypeEnum.FAT32));

#if FATFS_TRACE
            BasicConsole.WriteLine("Done. Clearing the root directory...");
#endif
            //Empty the root directory (in case of junk data)
            fs.WriteCluster(2, null);

#if FATFS_TRACE
            BasicConsole.WriteLine("Format complete.");
#endif
            fs.thePartition.CleanCaches();
        }

        public override void CleanDiskCaches()
        {
            thePartition.CleanCaches();
        }

        /// <summary>
        ///     FAT listing attributes
        /// </summary>
        public static class ListingAttribs
        {
            /// <summary>
            ///     Test
            /// </summary>
            public const byte Test = 0x01;

            /// <summary>
            ///     Indicates a hidden listing
            /// </summary>
            public const byte Hidden = 0x02;

            /// <summary>
            ///     Indicates a system listing
            /// </summary>
            public const byte System = 0x04;

            /// <summary>
            ///     Indicates a Volume ID listing - partition name.
            /// </summary>
            public const byte VolumeID = 0x08;

            /// <summary>
            ///     Indicates a directory listing.
            /// </summary>
            public const byte Directory = 0x10;

            /// <summary>
            ///     Indicates an archive listing.
            /// </summary>
            public const byte Archive = 0x20;

            /// <summary>
            ///     Indicates a long name entry - this is a combination
            ///     of other attributes. Test for first.
            /// </summary>
            public const byte LongName = 0x0F; // Combination of above attribs.
        }
    }
}