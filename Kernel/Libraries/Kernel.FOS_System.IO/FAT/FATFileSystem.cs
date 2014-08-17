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

#define FATFS_TRACE
#undef FATFS_TRACE

using System;

using Kernel.FOS_System.Collections;
using Kernel.Hardware;

namespace Kernel.FOS_System.IO.FAT
{
    /// <summary>
    /// Represents a FAT (12/16/32) file system.
    /// </summary>
    public class FATFileSystem : FileSystem
    {
        /// <summary>
        /// Number of bytes per sector
        /// </summary>
        public readonly UInt32 BytesPerSector;
        /// <summary>
        /// Number of sectors per cluster
        /// </summary>
        public readonly UInt32 SectorsPerCluster;
        /// <summary>
        /// Number of bytes per cluster (= SectorsPerCluster * BytesPerSector)
        /// </summary>
        public readonly UInt32 BytesPerCluster;

        /// <summary>
        /// Number of reserved sectors.
        /// </summary>
        public readonly UInt32 ReservedSectorCount;
        /// <summary>
        /// Total number of sectors.
        /// </summary>
        public readonly UInt32 TotalSectorCount;
        /// <summary>
        /// Number of clusters (to use for storing files/directories).
        /// </summary>
        public readonly UInt32 ClusterCount;

        /// <summary>
        /// Number of file allocation tables.
        /// </summary>
        public readonly UInt32 NumberOfFATs;
        /// <summary>
        /// Number of sectors for file allocation tables.
        /// </summary>
        public readonly UInt32 FATSectorCount;

        /// <summary>
        /// Root sector number - used by FAT12/16 only.
        /// </summary>
        public readonly UInt64 RootSector = 0;      // FAT12/16
        /// <summary>
        /// Number of root sectors - used by FAT12/16 only. Always 0 for FAT32.
        /// </summary>
        public readonly UInt32 RootSectorCount = 0; // FAT12/16, (FAT32 this is always 0)
        /// <summary>
        /// The cluster number for the root cluster. Used by FAT32 only.
        /// </summary>
        public readonly UInt32 RootCluster;         // FAT32
        /// <summary>
        /// The number of entries in the root directory table.
        /// </summary>
        public readonly UInt32 RootEntryCount;

        /// <summary>
        /// The sector number of the first data sector.
        /// </summary>
        public readonly UInt64 DataSector;          // First Data Sector
        /// <summary>
        /// The number of sectors for data storage.
        /// </summary>
        public readonly UInt32 DataSectorCount;

        /// <summary>
        /// Whether the file system is valid or not.
        /// </summary>
        public readonly bool IsValid = false;

        /// <summary>
        /// FAT listing attributes
        /// </summary>
        public static class ListingAttribs
        {
            /// <summary>
            /// Test
            /// </summary>
            public const byte Test = 0x01;
            /// <summary>
            /// Indicates a hidden listing
            /// </summary>
            public const byte Hidden = 0x02;
            /// <summary>
            /// Indicates a system listing
            /// </summary>
            public const byte System = 0x04;
            /// <summary>
            /// Indicates a Volume ID listing - partition name.
            /// </summary>
            public const byte VolumeID = 0x08;
            /// <summary>
            /// Indicates a directory listing.
            /// </summary>
            public const byte Directory = 0x10;
            /// <summary>
            /// Indicates an archive listing.
            /// </summary>
            public const byte Archive = 0x20;
            /// <summary>
            /// Indicates a long name entry - this is a combination 
            /// of other attributes. Test for first.
            /// </summary>
            public const byte LongName = 0x0F; // Combination of above attribs.
        }

        /// <summary>
        /// The types of FAT file system.
        /// </summary>
        public enum FATTypeEnum 
        { 
            /// <summary>
            /// Unknown / unrecognized FAT file system.
            /// </summary>
            Unknown, 
            /// <summary>
            /// FAT12 (12-bit) version
            /// </summary>
            FAT12, 
            /// <summary>
            /// FAT16 (16-bit) version
            /// </summary>
            FAT16, 
            /// <summary>
            /// FAT32 (32-bit) version
            /// </summary>
            FAT32 
        }
        /// <summary>
        /// The FAT type of the file system. Writing is only supported for FAT32.
        /// </summary>
        public readonly FATTypeEnum FATType = FATTypeEnum.Unknown;

        /// <summary>
        /// Initializes a new FAT file system from the specified partition.
        /// </summary>
        /// <param name="aPartition">The partition on which the file system resides.</param>
        /// <remarks>
        /// You should check IsValid after creating a new FAT file system to check a valid FAT 
        /// file system has been detected.
        /// </remarks>
        public FATFileSystem(Partition aPartition)
            : base(aPartition)
        {
            byte[] BPBData = thePartition.TheDiskDevice.NewBlockArray(1);
            thePartition.ReadBlock(0UL, 1U, BPBData);

            UInt16 sig = ByteConverter.ToUInt16(BPBData, 510);
            if (sig != 0xAA55)
            {
                return;
            }

            IsValid = true;

            BytesPerSector = ByteConverter.ToUInt16(BPBData, 11);
            SectorsPerCluster = BPBData[13];
            BytesPerCluster = BytesPerSector * SectorsPerCluster;
            ReservedSectorCount = ByteConverter.ToUInt16(BPBData, 14);
            NumberOfFATs = BPBData[16];
            RootEntryCount = ByteConverter.ToUInt16(BPBData, 17);

            TotalSectorCount = ByteConverter.ToUInt16(BPBData, 19);
            if (TotalSectorCount == 0)
            {
                TotalSectorCount = ByteConverter.ToUInt32(BPBData, 32);
            }

            // FAT12/16
            FATSectorCount = ByteConverter.ToUInt16(BPBData, 22);
            //For FAT32, SectorCount always 0 in this field.
            if (FATSectorCount == 0)
            {
                FATSectorCount = ByteConverter.ToUInt32(BPBData, 36);
            }
            
            DataSectorCount = TotalSectorCount - (ReservedSectorCount + (NumberOfFATs * FATSectorCount));

            // Computation rounds down. 
            ClusterCount = DataSectorCount / SectorsPerCluster;
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
                RootSector = ReservedSectorCount + (NumberOfFATs * FATSectorCount);
                RootSectorCount = (RootEntryCount * 32 + (BytesPerSector - 1)) / BytesPerSector;
            }
            DataSector = ReservedSectorCount + (NumberOfFATs * FATSectorCount) + RootSectorCount;
        }
        
        /// <summary>
        /// Creates a new byte array of the size of one cluster.
        /// </summary>
        /// <returns>The new byte array.</returns>
        public unsafe byte[] NewClusterArray()
        {
            //BasicConsole.WriteLine(((FOS_System.String)"Attempting to allocate ") + BytesPerCluster + " bytes");
            //BasicConsole.WriteLine(((FOS_System.String)"Heap free mem (bytes): ") + (Heap.FBlock->size - (Heap.FBlock->used * Heap.FBlock->bsize)));
            byte[] result = new byte[BytesPerCluster];
            //BasicConsole.WriteLine(((FOS_System.String)"Heap free mem (bytes): ") + (Heap.FBlock->size - (Heap.FBlock->used * Heap.FBlock->bsize)));
            return result;
        }
        /// <summary>
        /// Reads the specified cluster from the disk into the specified array.
        /// </summary>
        /// <param name="aCluster">The cluster number to read.</param>
        /// <param name="aData">The array to store the data in.</param>
        public void ReadCluster(UInt32 aCluster, byte[] aData)
        {
            UInt64 xSector = DataSector + ((aCluster - 2) * SectorsPerCluster);
            thePartition.ReadBlock(xSector, SectorsPerCluster, aData);
        }
        /// <summary>
        /// Writes the specified data to specified cluster number on the disk.
        /// </summary>
        /// <param name="aCluster">The cluster number to write to.</param>
        /// <param name="aData">The data to write.</param>
        public void WriteCluster(UInt32 aCluster, byte[] aData)
        {
            UInt64 xSector = DataSector + ((aCluster - 2) * SectorsPerCluster);
            thePartition.WriteBlock(xSector, SectorsPerCluster, aData);
        }
        /// <summary>
        /// Reads the cluster numbers in a cluster chain starting at the specified cluster number.
        /// </summary>
        /// <param name="fileSize">The size of file being read (used only for estimating number of clusters)</param>
        /// <param name="FirstClusterNum">The first cluster number in the chain.</param>
        /// <returns>The list of cluster numbers in the chain.</returns>
        public UInt32List ReadClusterChain(UInt64 fileSize, UInt32 FirstClusterNum)
        {
            UInt32List xResult = new UInt32List((int)((UInt32)fileSize / (SectorsPerCluster * BytesPerSector)));

            byte[] xSector = new byte[BytesPerSector];
            UInt64 xSectorNum = 0;
            bool xSectorNumHasValue = false;
            UInt32 xClusterNum = FirstClusterNum;

            UInt64 xNextSectorNum;
            UInt32 xNextSectorOffset;

            do
            {
                FATFileSystem.TableSectorDescrip tableSectorDesc = GetFATTableSectorPosition(xClusterNum);
                xNextSectorNum = tableSectorDesc.Sector;
                xNextSectorOffset = tableSectorDesc.Offset;

                if (xSectorNumHasValue == false || xSectorNum != xNextSectorNum)
                {
                    ReadFATSector(xNextSectorNum, xSector);
                    xSectorNum = xNextSectorNum;
                    xSectorNumHasValue = true;
                }

                xResult.Add(xClusterNum);

                xClusterNum = ReadFATEntry(xSector, xClusterNum, xNextSectorOffset);
            }
            while (!FATEntryIndicatesEOF(xClusterNum));

            return xResult;
        }

        /// <summary>
        /// Gets the sector number containing the FAT data and offset in that sector for the specified cluster number.
        /// </summary>
        /// <param name="aClusterNum">The cluster number.</param>
        /// <returns>The sector number and offset within the sector.</returns>
        public TableSectorDescrip GetFATTableSectorPosition(UInt32 aClusterNum)
        {
            TableSectorDescrip result = new TableSectorDescrip();

            UInt32 xOffset = 0;
            if (FATType == FATTypeEnum.FAT12)
            {
                // Multiply by 1.5 without using floating point, the divide by 2 rounds DOWN
                xOffset = aClusterNum + (aClusterNum / 2);
            }
            else if (FATType == FATTypeEnum.FAT16)
            {
                xOffset = aClusterNum * 2;
            }
            else if (FATType == FATTypeEnum.FAT32)
            {
                xOffset = aClusterNum * 4;
            }
            result.Sector = (xOffset / BytesPerSector);
            result.Offset = xOffset % BytesPerSector;

            return result;
        }
        /// <summary>
        /// Describes the position of FAT data on the disk.
        /// </summary>
        public class TableSectorDescrip : FOS_System.Object
        {
            /// <summary>
            /// The sector number.
            /// </summary>
            public UInt64 Sector;
            /// <summary>
            /// The offset to the cluster entry within the sector.
            /// </summary>
            public UInt32 Offset;
        }
        /// <summary>
        /// Reads the specified sector of the FAT into the specified data array.
        /// </summary>
        /// <param name="xSectorNum">The sector number of the FAT to read.</param>
        /// <param name="aData">The byte array to read the data into.</param>
        public void ReadFATSector(UInt64 xSectorNum, byte[] aData)
        {
            thePartition.ReadBlock(ReservedSectorCount + xSectorNum, 1, aData);
        }
        /// <summary>
        /// Writes the specified FAT data to the specified sector of the FAT on disk.
        /// </summary>
        /// <param name="xSectorNum">The sector number to write.</param>
        /// <param name="aData">The FAT sector data to write.</param>
        public void WriteFATSector(UInt64 xSectorNum, byte[] aData)
        {
            thePartition.WriteBlock(ReservedSectorCount + xSectorNum, 1, aData);
        }
        /// <summary>
        /// Reads the FAT specified entry number (cluster number) from the specified FAT sector data.
        /// </summary>
        /// <param name="aFATTableSector">The FAT sector data containing the FAT entry to be read.</param>
        /// <param name="aClusterNum">The entry (cluster number) to read.</param>
        /// <param name="aOffset">The offset within the sector that the entry is at.</param>
        /// <returns>The entry's value.</returns>
        public UInt32 ReadFATEntry(byte[] aFATTableSector, UInt32 aClusterNum, UInt32 aOffset)
        {
            if (FATType == FATTypeEnum.FAT12)
            {
                if (aOffset == (BytesPerSector - 1))
                {
                    ExceptionMethods.Throw(new FOS_System.Exception("TODO: Sector Span"));
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
                UInt32 xResult = ByteConverter.ToUInt16(aFATTableSector, aOffset);
                if ((aClusterNum & 0x01) == 0)
                { // Even
                    return xResult & 0x0FFF;
                }
                else
                { // Odd
                    return xResult >> 4;
                }
            }
            else if (FATType == FATTypeEnum.FAT16)
            {
                return ByteConverter.ToUInt16(aFATTableSector, aOffset);
            }
            else
            {
                return ByteConverter.ToUInt16(aFATTableSector, aOffset) & 0x0FFFFFFFu;
            }
        }
        /// <summary>
        /// Writes the specified value to the specified FAT entry number in the FAT sector data array.
        /// </summary>
        /// <param name="aFATTableSector">The FAT sector data.</param>
        /// <param name="aClusterNum">The cluster number to write.</param>
        /// <param name="aOffset">The offset within the FAT sector data of the entry to write.</param>
        /// <param name="FATEntry">The value to write.</param>
        public void WriteFATEntry(byte[] aFATTableSector, UInt32 aClusterNum, UInt32 aOffset, UInt32 FATEntry)
        {
            if (FATType == FATTypeEnum.FAT12)
            {
                if (aOffset == (BytesPerSector - 1))
                {
                    ExceptionMethods.Throw(new FOS_System.Exception("TODO: Sector Span"));
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
                { // Even
                    FATEntry &= 0x0FFF;

                    aFATTableSector[aOffset] = (byte)(FATEntry);
                    aFATTableSector[aOffset + 1] = (byte)((UInt32)(aFATTableSector[aOffset + 1] & 0xF0) | (FATEntry >> 8));
                }
                else
                { // Odd
                    FATEntry <<= 4;
                    aFATTableSector[aOffset] = (byte)((UInt32)(aFATTableSector[aOffset] & 0x0F) | FATEntry);
                    aFATTableSector[aOffset + 1] = (byte)(FATEntry >> 8);
                }
            }
            else if (FATType == FATTypeEnum.FAT16)
            {
                aFATTableSector[aOffset] = (byte)(FATEntry);
                aFATTableSector[aOffset + 1] = (byte)(FATEntry >> 8);
            }
            else
            {
                FATEntry = FATEntry & 0x0FFFFFFFu;
                aFATTableSector[aOffset + 0] = (byte)(FATEntry);
                aFATTableSector[aOffset + 1] = (byte)(FATEntry >> 4);
                aFATTableSector[aOffset + 2] = (byte)(FATEntry >> 8);
                aFATTableSector[aOffset + 3] = (byte)(FATEntry >> 12);
                aFATTableSector[aOffset + 4] = (byte)(FATEntry >> 16);
                aFATTableSector[aOffset + 5] = (byte)(FATEntry >> 20);
                aFATTableSector[aOffset + 6] = (byte)(FATEntry >> 24);
                // --- DO NOT WRITE TOP 4 BITS --- (as per spec)
                //aFATTableSector[aOffset + 7] = (byte)(FATEntry >> 28);
            }
        }
        /// <summary>
        /// Determines whether the FAT entry value indicates end-of-file or not.
        /// </summary>
        /// <param name="aValue">The value to test.</param>
        /// <returns>Whether the FAT entry value indicates end-of-file or not.</returns>
        public bool FATEntryIndicatesEOF(UInt32 aValue)
        {
            //Hmmm...Ubuntu 10.1 FAT driver was writing 
            //  0xFFF8 as the EOF cluster num for a FAT32 formatted drive
            //  when it should be (as per spec) the value below.
            //Thus, if we exactly equal 0xFFF8, we will assume a badly-written drive
            //  and just accept it...

            return aValue >= GetFATEntryEOFValue(FATType) ||
                aValue == GetFATEntryEOFValue(FATTypeEnum.FAT16);
        }
        /// <summary>
        /// Determines whether the FAT entry value indicates a free cluster or not.
        /// </summary>
        /// <param name="aValue">The value to test.</param>
        /// <returns>Whether the FAT entry value indicates a free cluster or not.</returns>
        public bool FATEntryIndicatesFree(UInt32 aValue)
        {
            //Hmmm...Ubuntu 10.1 FAT driver was writing 
            //  0xFFF8 as the EOF cluster num for a FAT32 formatted drive
            //  when it should be (as per spec) the value below.
            //Thus, if we exactly equal 0xFFF8, we will assume a badly-written drive
            //  and just accept it...

            return aValue == 0;
        }
        /// <summary>
        /// Gets the EOF value for the specified FAT type.
        /// </summary>
        /// <param name="aFATType">The FAT type.</param>
        /// <returns>The EOF value.</returns>
        public static UInt32 GetFATEntryEOFValue(FATTypeEnum aFATType)
        {
            if (aFATType == FATTypeEnum.FAT12)
            {
                return 0x0FF8;
            }
            else if (aFATType == FATTypeEnum.FAT16)
            {
                return 0xFFF8;
            }
            else
            {
                return 0x0FFFFFF8;
            }
        }
        /// <summary>
        /// Gets the next free cluster number after the specified cluster number.
        /// </summary>
        /// <param name="startCluster">The cluster number to start searching from.</param>
        /// <returns>The next free cluster number.</returns>
        /// <remarks>
        /// At the time of writing, this method's behavior was undefined if no free clusters were left.
        /// Predicted behavior is that it would either enter an infinite loop or cause an exception if no
        /// free clusters are available.
        /// </remarks>
        public UInt32 GetNextFreeCluster(UInt32 startCluster)
        {
            byte[] xSector = new byte[BytesPerSector];
            UInt64 xSectorNum = 0;
            UInt32 xSectorOffset = 0;
            UInt32 xClusterNum = startCluster - 1;
            UInt32 xClusterPointedTo = 0xF;

            //TODO - Umm...fix this! This will hit infinite loop if no free clusters left!
            do
            {
                xClusterNum++;
                FATFileSystem.TableSectorDescrip tableSectorDesc = GetFATTableSectorPosition(xClusterNum);
                xSectorNum = tableSectorDesc.Sector;
                xSectorOffset = tableSectorDesc.Offset;

                ReadFATSector(xSectorNum, xSector);

                xClusterPointedTo = ReadFATEntry(xSector, xClusterNum, xSectorOffset);
            }
            while (!FATEntryIndicatesFree(xClusterPointedTo));

            return xClusterNum;
        }
        /// <summary>
        /// Sets the specified FAT entry to the specified value and saves it to disk.
        /// </summary>
        /// <param name="clusterNum">The cluster number to set.</param>
        /// <param name="value">The value to set to.</param>
        public void SetFATEntryAndSave(UInt32 clusterNum, UInt32 value)
        {
            FATFileSystem.TableSectorDescrip tableSectorDesc = GetFATTableSectorPosition(clusterNum);
            UInt64 sectorNum = tableSectorDesc.Sector;
            UInt32 sectorOffset = tableSectorDesc.Offset;
            byte[] sectorData = new byte[BytesPerSector];
            ReadFATSector(sectorNum, sectorData);
            WriteFATEntry(sectorData, (UInt32)sectorNum, sectorOffset, value);
            WriteFATSector(sectorNum, sectorData);
        }

        /// <summary>
        /// The underlying root directory - used by FAT32 only.
        /// </summary>
        private FATDirectory _rootDirectoryFAT32 = null;
        /// <summary>
        /// The underlying root directory - used by FAT32 only.
        /// </summary>
        public FATDirectory RootDirectory_FAT32
        {
            get
            {
                if (_rootDirectoryFAT32 == null)
                {
                    GetRootDirectoryTable();
                }
                return _rootDirectoryFAT32;
            }
        }
        /// <summary>
        /// The cached root directory listings - used by FAT12/16 only.
        /// </summary>
        private List _rootDirectoryListings = null;
        /// <summary>
        /// Gets the root directory listings in the FAT file system.
        /// </summary>
        /// <returns>The root directory listings.</returns>
        public List GetRootDirectoryTable()
        {
            if (FATType == FATTypeEnum.FAT32)
            {
                if (_rootDirectoryFAT32 == null)
                {
                    _rootDirectoryFAT32 = new FATDirectory(this, null, "ROOT", RootCluster);
                }
                return _rootDirectoryFAT32.GetListings();
            }
            else
            {
                if (_rootDirectoryListings == null)
                {
                    byte[] xData = thePartition.TheDiskDevice.NewBlockArray(RootSectorCount);
                    thePartition.ReadBlock(RootSector, RootSectorCount, xData);

                    _rootDirectoryListings = ParseDirectoryTable(xData, xData.Length, null);
                }
                return _rootDirectoryListings;
            }
        }
        /// <summary>
        /// Parses the specified directory file data for its listings.
        /// </summary>
        /// <param name="xData">The directory data.</param>
        /// <param name="xDataLength">The directory data length.</param>
        /// <param name="thisDir">
        /// The FAT directory the FAT data is from. 
        /// Used when creating listings as the parent directory.
        /// </param>
        /// <returns>The directory listings.</returns>
        public List ParseDirectoryTable(byte[] xData, int xDataLength, FATDirectory thisDir)
        {
            List xResult = new List();

            //BasicConsole.WriteLine("Parsing listings...");

            FOS_System.String xLongName = "";
            for (UInt32 i = 0; i < xDataLength; i = i + 32)
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
                        FOS_System.String xLongPart = ByteConverter.GetASCIIStringFromUTF16(xData, i + 1, 5);
                        //BasicConsole.WriteLine("xLongPart1: " + xLongPart);
                        // We have to check the length because 0xFFFF is a valid Unicode codepoint.
                        // So we only want to stop if the 0xFFFF is AFTER a 0x0000. We can determin
                        // this by also looking at the length. Since we short circuit the or, the length
                        // is rarely evaluated.
                        if (xLongPart.length == 5)
                        {
                            xLongPart = xLongPart + ByteConverter.GetASCIIStringFromUTF16(xData, i + 14, 6);
                            //BasicConsole.WriteLine("xLongPart2: " + xLongPart);
                            if (xLongPart.length == 11)
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
                    else if (xStatus == 0x05)
                    {
                        // Japanese characters - We dont handle these
                    }
                    else if (xStatus == 0xE5)
                    {
                        // Empty slot, skip it
                    }
                    else if (xStatus >= 0x20)
                    {
                        FOS_System.String xName;

                        int xTest = xAttrib & (ListingAttribs.Directory | ListingAttribs.VolumeID);

                        if (xLongName.length > 0)
                        {
                            // Leading and trailing spaces are to be ignored according to spec.
                            // Many programs (including Windows) pad trailing spaces although it 
                            // it is not required for long names.
                            xName = xLongName.Trim();

                            // As per spec, ignore trailing periods
                            //If there are trailing periods
                            int nameIndex = xName.length - 1;
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
                            FOS_System.String xEntry = ByteConverter.GetASCIIStringFromASCII(xData, i, 11);
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
                                if (xEntry.length >= 8)
                                {
                                    xName = xEntry.Substring(0, 8).TrimEnd();

                                    if (xEntry.length >= 11)
                                    {
                                        FOS_System.String xExt = xEntry.Substring(8, 3).TrimEnd();
                                        if (xExt.length > 0)
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

                        UInt32 xFirstCluster = (UInt32)(ByteConverter.ToUInt16(xData, i + 20) << 16 | ByteConverter.ToUInt16(xData, i + 26));

                        xName = xName.ToUpper();
                        if (xTest == 0)
                        {
                            if (xName[xName.length - 1] != '~')
                            {
                                UInt32 xSize = ByteConverter.ToUInt32(xData, i + 28);
                                xResult.Add(new FATFile(this, thisDir, xName, xSize, xFirstCluster));
                            }
                            else
                            {
                                //BasicConsole.WriteLine("Ignoring file: " + xName);
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
        /// Encodes the specified listings into a byte array.
        /// </summary>
        /// <param name="listings">The listings to encode.</param>
        /// <param name="includeVolumeID">Whether to include the Volume ID entry (partition name). Only true for root directory.</param>
        /// <returns>The encoded listings data.</returns>
        public byte[] EncodeDirectoryTable(List listings, bool includeVolumeID)
        {
            int LongFilenamesSize = 0;
            if (FATType == FATTypeEnum.FAT32)
            {
#if FATFS_TRACE
                BasicConsole.WriteLine(((FOS_System.String)"Checking listings (") + listings.Count + ") for long file names...");
                BasicConsole.DelayOutput(2);
#endif
                for (int i = 0; i < listings.Count; i++)
                {
#if FATFS_TRACE
                    BasicConsole.WriteLine("Checking listing...");
                    BasicConsole.WriteLine(((Base)listings[i]).Name);
                    BasicConsole.DelayOutput(2);
#endif
                    if (((FOS_System.String)((Base)listings[i]).Name.Split('.')[0]).length > 8)
                    {
#if FATFS_TRACE
                        BasicConsole.WriteLine("Long name detected.");
#endif
                        int nameLength = ((Base)listings[i]).Name.length;
                        LongFilenamesSize += nameLength / 13;
                        if (nameLength % 13 > 0)
                        {
                            LongFilenamesSize++;
                        }
                    }

#if FATFS_TRACE
                    BasicConsole.WriteLine("Check completed.");
#endif
                }
            }
#if FATFS_TRACE
            BasicConsole.WriteLine("Calculating long file name size...");
#endif
            LongFilenamesSize *= 32;

#if FATFS_TRACE
            BasicConsole.WriteLine("Allocating data for directory bytes...");
#endif
            //                       +32 for VolumeID entry                         + 32 for end entry
            byte[] result = new byte[32 + (listings.Count * 32) + LongFilenamesSize + 32];

            int shortNameReplacements = 0;
            int offset = 0;

            if(includeVolumeID)
            {
                //Volume ID entry - this is only be valid for root directory.

                List shortName = GetShortName(thePartition.VolumeID, true);
                
                //Put in short name entry
                byte[] DIR_Name = ByteConverter.GetASCIIBytes((FOS_System.String)shortName[0]);
                byte DIR_Attr = ListingAttribs.VolumeID;                
                for (int j = 0; j < DIR_Name.Length; j++)
                {
                    result[offset++] = DIR_Name[j];
                }
                result[offset++] = DIR_Attr;
                offset = 32;
            }

            for (int i = 0; i < listings.Count; i++)
            {
#if FATFS_TRACE
                BasicConsole.WriteLine("Encoding listing...");
#endif
                Base listing = ((Base)listings[i]);
                bool isDirectory = listing._Type == (FOS_System.Type)typeof(FATDirectory);
                FOS_System.String name = listing.Name.ToUpper();
                FOS_System.String shortNameStr = name;
                List nameParts = name.Split('.');
                bool isLongName = ((FOS_System.String)nameParts[0]).length > 8;
                if (isLongName)
                {
#if FATFS_TRACE
                    BasicConsole.WriteLine("Long name detected.");
#endif
                    shortNameReplacements++;
                    if (isDirectory)
                    {
                        shortNameStr = (FOS_System.String)shortNameReplacements;
                    }
                    else
                    {
                        shortNameStr = (FOS_System.String)shortNameReplacements + "." + ((FOS_System.String)nameParts[1]);
                    }
                }
                List shortName = GetShortName(shortNameStr, isDirectory);
                byte[] shortNameBytes;
                if (isDirectory)
                {
                    shortNameBytes = ByteConverter.GetASCIIBytes((FOS_System.String)shortName[0]);
                }
                else
                {
                    shortNameBytes = ByteConverter.GetASCIIBytes((FOS_System.String)shortName[0] + (FOS_System.String)shortName[1]);
                }
                if (FATType == FATTypeEnum.FAT32)
                {
                    //Put in long name entries
                    if (isLongName)
                    {
                        int count = name.length;
                        int nameOffset = count;
                        byte LDIR_Ord = (byte)(0x40 | ((name.length / 13) + 1));
                        bool first = true;

                        while (count > 0)
                        {
                            nameOffset -= (count > 13 ? 13 : count);
                            int getCharsCount = count > 5 ? 5 : count;
                            byte[] LDIR_Name1 = ByteConverter.GetUTF16Bytes(name, nameOffset, getCharsCount);
                            count -= getCharsCount;

                            byte LDIR_Attr = ListingAttribs.LongName;
                            byte LDIR_Type = 0;
                            byte LDIR_Chksum = CheckSum(shortNameBytes);

                            getCharsCount = count > 6 ? 6 : count;
                            byte[] LDIR_Name2 = ByteConverter.GetUTF16Bytes(name, nameOffset + 5, getCharsCount);
                            count -= getCharsCount;

                            byte LDIR_FstClusLO = 0;

                            getCharsCount = count > 2 ? 2 : count;
                            byte[] LDIR_Name3 = ByteConverter.GetUTF16Bytes(name, nameOffset + 11, getCharsCount);
                            count -= getCharsCount;

                            result[offset + 0] = LDIR_Ord;
                            for (int j = 0; j < LDIR_Name1.Length; j++)
                            {
                                result[offset + 1 + j] = LDIR_Name1[j];
                            }
                            //As per spec, insert trailing periods
                            for (int j = LDIR_Name1.Length + 1; j < 10; j++)
                            {
                                if (j % 2 == 0)
                                {
                                    result[offset + 1 + j] = 0x2E;
                                }
                                else
                                {
                                    result[offset + 1 + j] = 0x00;
                                }
                            }
                            result[offset + 11] = LDIR_Attr;
                            result[offset + 12] = LDIR_Type;
                            result[offset + 13] = LDIR_Chksum;
                            for (int j = 0; j < LDIR_Name2.Length; j++)
                            {
                                result[offset + 14 + j] = LDIR_Name2[j];
                            }
                            //As per spec, insert trailing periods
                            for (int j = LDIR_Name2.Length + 1; j < 12; j++)
                            {
                                if (j % 2 == 0)
                                {
                                    result[offset + 14 + j] = 0x2E;
                                }
                                else
                                {
                                    result[offset + 14 + j] = 0x00;
                                }
                            }
                            result[offset + 26] = LDIR_FstClusLO;
                            for (int j = 0; j < LDIR_Name3.Length; j++)
                            {
                                result[offset + 28 + j] = LDIR_Name3[j];
                            }
                            //As per spec, insert trailing periods
                            for (int j = LDIR_Name3.Length + 1; j < 4; j++)
                            {
                                if (j % 2 == 0)
                                {
                                    result[offset + 28 + j] = 0x2E;
                                }
                                else
                                {
                                    result[offset + 28 + j] = 0x00;
                                }
                            }

                            if (first)
                            {
                                first = false;
                                LDIR_Ord = (byte)(name.length / 13); //No +1 because we just did 1 already!
                            }
                            else
                            {
                                LDIR_Ord++;
                            }
                            offset += 32;
                        }
                    }
                }

                //Put in short name entry
                byte[] DIR_Name = shortNameBytes;
                byte DIR_Attr = isDirectory ? ListingAttribs.Directory : (byte)0;
                byte DIR_NTRes = 0;
                //TODO: Creation time
                byte[] DIR_FstClusHI = new byte[2];
                byte[] DIR_FstClusLO = new byte[2];
                UInt32 firstClusterNum = 0;
                if (isDirectory)
                {
                    firstClusterNum = ((FATDirectory)listing).FirstClusterNum;
                }
                else
                {
                    firstClusterNum = ((FATFile)listing).FirstClusterNum;
                }

                DIR_FstClusLO[0] = (byte)(firstClusterNum);
                DIR_FstClusLO[1] = (byte)(firstClusterNum >> 8);

                if (FATType == FATTypeEnum.FAT32)
                {
                    DIR_FstClusHI[0] = (byte)(firstClusterNum >> 16);
                    DIR_FstClusHI[1] = (byte)(firstClusterNum >> 24);
                }

                //TODO: Write Time
                byte[] DIR_FileSize = new byte[4];
                if (!isDirectory)
                {
                    UInt32 size = (UInt32)((FATFile)listing).Size;
                    DIR_FileSize[0] = (byte)(size);
                    DIR_FileSize[1] = (byte)(size >> 8);
                    DIR_FileSize[2] = (byte)(size >> 16);
                    DIR_FileSize[3] = (byte)(size >> 24);
                }
                for (int j = 0; j < DIR_Name.Length; j++)
                {
                    result[offset++] = DIR_Name[j];
                }
                result[offset++] = DIR_Attr;
                result[offset++] = DIR_NTRes;
                offset += 7;
                result[offset++] = DIR_FstClusHI[0];
                result[offset++] = DIR_FstClusHI[1];
                offset += 4;
                result[offset++] = DIR_FstClusLO[0];
                result[offset++] = DIR_FstClusLO[1];

                result[offset++] = DIR_FileSize[0];
                result[offset++] = DIR_FileSize[1];
                result[offset++] = DIR_FileSize[2];
                result[offset++] = DIR_FileSize[3];

#if FATFS_TRACE
                BasicConsole.WriteLine("Encoded listing.");
#endif
            }
            
            return result;
        }
        /// <summary>
        /// Gets the short name for the specified long name.
        /// </summary>
        /// <param name="longName">The long name to shorten.</param>
        /// <param name="isDirectory">Whether the long name is for a directory or not.</param>
        /// <returns>The short name parts. Directory=1 part, file=2 parts (name + extension).</returns>
        private static List GetShortName(FOS_System.String longName, bool isDirectory)
        {
            if (isDirectory)
            {
                List result = new List(1);
                result.Add(longName.Substring(0, 11).PadRight(11, (char)0));
                return result;
            }
            else
            {
                List result = new List(2);
                List nameParts = longName.Split('.');
                if (nameParts.Count > 1)
                {
                    result.Add(((FOS_System.String)nameParts[0]).Substring(0, 8).PadRight(8, (char)0));
                    result.Add(((FOS_System.String)nameParts[1]).Substring(0, 3).PadLeft(3, (char)0));
                }
                else
                {
                    result.Add(longName.Substring(0, 8).PadRight(8, (char)0));
                    result.Add(((FOS_System.String)"").PadLeft(3, (char)0));
                }
                return result;
            }
        }
        /// <summary>
        /// Calculates the short name checksum.
        /// </summary>
        /// <param name="shortName">The short name.</param>
        /// <returns>The checksum value.</returns>
        private static byte CheckSum(byte[] shortName)
        {
            short FcbNameLen;
		    byte Sum;

		    Sum = 0;
		    for (FcbNameLen = 10; FcbNameLen > -1; FcbNameLen--)
            {
			    // NOTE: The operation is an unsigned char rotate right
			    Sum = (byte)(((Sum & 0x1) == 0x1 ? 0x80 : 0) + (Sum >> 1) + shortName[FcbNameLen]);
		    }
		    return Sum;
        }

        /// <summary>
        /// Gets the listing for the specified file or directory.
        /// </summary>
        /// <param name="aName">The full path to the file or directory.</param>
        /// <returns>The listing or null if not found.</returns>
        public override Base GetListing(FOS_System.String aName)
        {
            if (aName == "")
            {
                return RootDirectory_FAT32;
            }
            else
            {
                List nameParts = aName.Split(FileSystemManager.PathDelimiter);
                List listings = GetRootDirectoryTable();
                return GetListingFromListings(nameParts, null, listings);
            }
        }

        /// <summary>
        /// Creates a new directory within the file system.
        /// </summary>
        /// <param name="name">The name of the directory to create.</param>
        /// <param name="parent">The parent directory of the new directory.</param>
        /// <returns>The new directory listing.</returns>
        public override Directory NewDirectory(String name, Directory parent)
        {
            if (FATType != FATTypeEnum.FAT32)
            {
                ExceptionMethods.Throw(new Exceptions.NotSupportedException("FATFileSystem.NewDirectory for non-FAT32 not supported!"));
            }
            if (parent == null)
            {
                ExceptionMethods.Throw(new Exceptions.NullReferenceException());
            }
            if (parent._Type != (FOS_System.Type)typeof(FATDirectory))
            {
                ExceptionMethods.Throw(new Exceptions.NotSupportedException("FATFileSystem.NewDirectory parent directory must be of type FATDirectory!"));
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
                UInt32 freeCluster = GetNextFreeCluster(2);
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
            else
            {
                ExceptionMethods.Throw(new IOException("Listing (directory/file) with specified name already exists!"));
            }
            return null;
        }
        /// <summary>
        /// Creates a new file within the file system.
        /// </summary>
        /// <param name="name">The name of the file to create.</param>
        /// <param name="parent">The parent directory of the new file.</param>
        /// <returns>The new file listing.</returns>
        public override File NewFile(String name, Directory parent)
        {
            if (FATType != FATTypeEnum.FAT32)
            {
                ExceptionMethods.Throw(new Exceptions.NotSupportedException("FATFileSystem.NewFile for non-FAT32 not supported!"));
            }
            if (parent == null)
            {
                ExceptionMethods.Throw(new Exceptions.NullReferenceException());
            }
            if (parent._Type != (FOS_System.Type)typeof(FATDirectory))
            {
                ExceptionMethods.Throw(new Exceptions.NotSupportedException("FATFileSystem.NewFile parent directory must be of type FATDirectory!"));
            }

            //BasicConsole.WriteLine("Getting directory listings...");
            
            List listings = null;
            if (parent == null)
            {
                listings = GetRootDirectoryTable();
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
                UInt32 freeCluster = GetNextFreeCluster(2);
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
            else
            {
                ExceptionMethods.Throw(new IOException("Listing (directory/file) with specified name already exists!"));
            }
            return null;
        }

        /// <summary>
        /// Formats the specified partition as FAT32.
        /// </summary>
        /// <param name="thePartition">The partition to format.</param>
        public static void FormatPartitionAsFAT32(Partition thePartition)
        {
#if FATFS_TRACE
            BasicConsole.WriteLine(((FOS_System.String)"Creating block array... Block size: ") + thePartition.BlockSize);
#endif

            byte[] newBPBData = thePartition.TheDiskDevice.NewBlockArray(1);

#if FATFS_TRACE
            BasicConsole.WriteLine("Block array created.");
#endif

            //FAT signature
            newBPBData[510] = 0x55;
            newBPBData[511] = 0xAA;

            //Bytes per sector - 512
            UInt16 bytesPerSector = 512;
            newBPBData[11] = (byte)(bytesPerSector);
            newBPBData[12] = (byte)(bytesPerSector >> 8);
            ulong partitionSize = thePartition.BlockCount * thePartition.BlockSize;

#if FATFS_TRACE
            BasicConsole.WriteLine(((FOS_System.String)"partitionSize: ") + partitionSize);
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
                ExceptionMethods.Throw(new Exceptions.NotSupportedException("Drive too big! Max. size 2TB for FAT32."));
            }
            //Sectors per cluster - 32 KiB clusters = 64 sectors per cluster
            newBPBData[13] = sectorsPerCluster;
            //Reserved sector count - 32 for FAT32 (by convention... and FAT32 does not imply 32 sectors)
            UInt16 reservedSectors = 32;
            newBPBData[14] = (byte)(reservedSectors);
            newBPBData[15] = (byte)(reservedSectors >> 8);
            //Number of FATs - always 2
            newBPBData[16] = 0x02;
            //Root entry count - always 0 for FAT32
            // - Do nothing
            
            //Total sector count
            // - At newBPBData[19] - N/A for FAT32
            //      - Do nothing
            // - At newBPBData[32] - Total number of sectors in the file system
            uint totalSectors = (uint)thePartition.BlockCount;
            newBPBData[32] = (byte)(totalSectors);
            newBPBData[33] = (byte)(totalSectors >> 8);
            newBPBData[34] = (byte)(totalSectors >> 16);
            newBPBData[35] = (byte)(totalSectors >> 24);


            //FAT sector count
            // - At newBPBData[22] - always 0 for FAT32
            // - At newBPBData[36] - See calculation below

            //FAT sector count = 2 * RoundUp(Number of bytes for 1 FAT / Bytes per sector)
            
#if FATFS_TRACE
            BasicConsole.WriteLine(((FOS_System.String)"totalSectors: ") + totalSectors +
                                                       ", reservedSectors: " + reservedSectors +
                                                       ", sectorsPerCluster: " + sectorsPerCluster +
                                                       ", bytesPerSector: " + bytesPerSector);
#endif

            // Number of bytes for 2 FAT  = 4 * Number of data clusters
            //                            = 4 * (RndDown((totalSectors - ReservedSectors) / sectorsPerCluster) - RndUp(Clusters for 2 FATs))
            //               bytesPer2FAT = 4 * (X - RndUp((bytesPerFAT * 2) / bytesPerCluster))
            //               bytesPer2FAT = (4 * X * bytesPerCluster) / (bytesPerCluster + 8)
            uint dataClusters = (totalSectors - reservedSectors) / sectorsPerCluster;
#if FATFS_TRACE
            BasicConsole.WriteLine(((FOS_System.String)"dataClusters: ") + dataClusters);
#endif
            uint bytesPerCluster = (uint)sectorsPerCluster * bytesPerSector;
#if FATFS_TRACE
            BasicConsole.WriteLine(((FOS_System.String)"bytesPerCluster: ") + bytesPerCluster);
            BasicConsole.WriteLine(((FOS_System.String)"4 * dataClusters: ") + (4 * dataClusters));
            BasicConsole.WriteLine(((FOS_System.String)"4 * dataClusters * bytesPerCluster: ") + (4 * dataClusters * bytesPerCluster));
            BasicConsole.WriteLine(((FOS_System.String)"bytesPerCluster + 8: ") + (bytesPerCluster + 8));
#endif
            
            uint bytesPer2FAT = (uint)Math.Divide((4 * (ulong)dataClusters * bytesPerCluster), (bytesPerCluster + 8)); //Calculation rounds down
#if FATFS_TRACE
            BasicConsole.WriteLine(((FOS_System.String)"bytesPer2FAT: ") + bytesPer2FAT);
#endif
            uint FATSectorCount = bytesPer2FAT / bytesPerSector;
#if FATFS_TRACE
            BasicConsole.WriteLine(((FOS_System.String)"FATSectorCount: ") + FATSectorCount);
#endif
            newBPBData[36] = (byte)(FATSectorCount);
            newBPBData[37] = (byte)(FATSectorCount >> 8);
            newBPBData[38] = (byte)(FATSectorCount >> 16);
            newBPBData[39] = (byte)(FATSectorCount >> 24);
            
#if FATFS_TRACE
            BasicConsole.WriteLine(((FOS_System.String)"totalSectors: ") + totalSectors +
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

            FATFileSystem fs = new FATFileSystem(thePartition);
            if (!fs.IsValid)
            {
#if FATFS_TRACE
                BasicConsole.WriteLine("Failed to format properly. Scrubbing new BPB...");
#endif
                byte[] scrubBPB = thePartition.TheDiskDevice.NewBlockArray(1);
                thePartition.WriteBlock(0UL, 1U, scrubBPB);
#if FATFS_TRACE
                BasicConsole.WriteLine("Scrub done.");
#endif

                ExceptionMethods.Throw(new FOS_System.Exception("Failed to format properly! FATFileSystem did not recognise system as valid."));
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

                ExceptionMethods.Throw(new FOS_System.Exception(((FOS_System.String)"Failed to format properly! FATFileSystem recognised incorrect FAT type. Type recognised: ") + (uint)fs.FATType));
            }
            
#if FATFS_TRACE
            BasicConsole.WriteLine("FAT recognised. Setting up empty FAT table...");
#endif

            //Mark all clusters as empty
            fs.ThePartition.WriteBlock(fs.ReservedSectorCount, FATSectorCount, null);
            
#if FATFS_TRACE
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
        }
    }
}
