using System;

using Kernel.FOS_System.Collections;
using Kernel.Hardware;

namespace Kernel.FOS_System.IO.FAT
{
    public class FATFileSystem : FileSystem
    {
        public readonly UInt32 BytesPerSector;
        public readonly UInt32 SectorsPerCluster;
        public readonly UInt32 BytesPerCluster;

        public readonly UInt32 ReservedSectorCount;
        public readonly UInt32 TotalSectorCount;
        public readonly UInt32 ClusterCount;

        public readonly UInt32 NumberOfFATs;
        public readonly UInt32 FATSectorCount;

        public readonly UInt64 RootSector = 0;      // FAT12/16
        public readonly UInt32 RootSectorCount = 0; // FAT12/16, (FAT32 this is always 0)
        public readonly UInt32 RootCluster;         // FAT32
        public readonly UInt32 RootEntryCount;

        public readonly UInt64 DataSector;          // First Data Sector
        public readonly UInt32 DataSectorCount;

        public readonly bool IsValid = false;

        public static class ListingAttribs
        {
            public const byte Test = 0x01;
            public const byte Hidden = 0x02;
            public const byte System = 0x04;
            public const byte VolumeID = 0x08;
            public const byte Directory = 0x10;
            public const byte Archive = 0x20;
            public const byte LongName = 0x0F; // Combination of above attribs.
        }

        public enum FATTypeEnum 
        { 
            Unknown, 
            FAT12, 
            FAT16, 
            FAT32 
        }
        public readonly FATTypeEnum FATType = FATTypeEnum.Unknown;

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
            
            DataSectorCount = TotalSectorCount - (ReservedSectorCount + (NumberOfFATs * FATSectorCount) + ReservedSectorCount);

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
        
        public byte[] NewClusterArray()
        {
            return new byte[BytesPerCluster];
        }
        public void ReadCluster(UInt32 aCluster, byte[] aData)
        {
            UInt64 xSector = DataSector + ((aCluster - 2) * SectorsPerCluster);
            thePartition.ReadBlock(xSector, SectorsPerCluster, aData);
        }
        public void WriteCluster(UInt32 aCluster, byte[] aData)
        {
            UInt64 xSector = DataSector + ((aCluster - 2) * SectorsPerCluster);
            thePartition.WriteBlock(xSector, SectorsPerCluster, aData);
        }
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
                    ReadFATTableSector(xNextSectorNum, xSector);
                    xSectorNum = xNextSectorNum;
                    xSectorNumHasValue = true;
                }

                xResult.Add(xClusterNum);

                xClusterNum = ReadFATEntry(xSector, xClusterNum, xNextSectorOffset);
            }
            while (!FATEntryIndicatesEOF(xClusterNum));

            return xResult;
        }

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
        public class TableSectorDescrip : FOS_System.Object
        {
            public UInt64 Sector;
            public UInt32 Offset;
        }
        public void ReadFATTableSector(UInt64 xSectorNum, byte[] aData)
        {
            thePartition.ReadBlock(ReservedSectorCount + xSectorNum, 1, aData);
        }
        public void WriteFATTableSector(UInt64 xSectorNum, byte[] aData)
        {
            thePartition.WriteBlock(ReservedSectorCount + xSectorNum, 1, aData);
        }
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
        public bool FATEntryIndicatesFree(UInt32 aValue)
        {
            //Hmmm...Ubuntu 10.1 FAT driver was writing 
            //  0xFFF8 as the EOF cluster num for a FAT32 formatted drive
            //  when it should be (as per spec) the value below.
            //Thus, if we exactly equal 0xFFF8, we will assume a badly-written drive
            //  and just accept it...

            return aValue == 0;
        }
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
        public UInt32 GetNextFreeCluster(UInt32 startCluster)
        {
            byte[] xSector = new byte[BytesPerSector];
            UInt64 xSectorNum = 0;
            UInt32 xSectorOffset = 0;
            UInt32 xClusterNum = startCluster - 1;
            UInt32 xClusterPointedTo = 0xF;

            do
            {
                xClusterNum++;
                FATFileSystem.TableSectorDescrip tableSectorDesc = GetFATTableSectorPosition(xClusterNum);
                xSectorNum = tableSectorDesc.Sector;
                xSectorOffset = tableSectorDesc.Offset;

                ReadFATTableSector(xSectorNum, xSector);

                xClusterPointedTo = ReadFATEntry(xSector, xClusterNum, xSectorOffset);
            }
            while (!FATEntryIndicatesFree(xClusterPointedTo));

            return xClusterNum;
        }
        public void SetFATEntryAndSave(UInt32 clusterNum, UInt32 value)
        {
            FATFileSystem.TableSectorDescrip tableSectorDesc = GetFATTableSectorPosition(clusterNum);
            UInt64 sectorNum = tableSectorDesc.Sector;
            UInt32 sectorOffset = tableSectorDesc.Offset;
            byte[] sectorData = new byte[BytesPerSector];
            ReadFATTableSector(sectorNum, sectorData);
            WriteFATEntry(sectorData, (UInt32)sectorNum, sectorOffset, value);
            WriteFATTableSector(sectorNum, sectorData);
        }

        private FATDirectory _rootDirectoryFAT32 = null;
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
        private List _rootDirectoryListings = null;
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
        public List ParseDirectoryTable(byte[] xData, int xDataLength, FATDirectory thisDir)
        {
            List xResult = new List();

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
                        FOS_System.String xLongPart = ByteConverter.GetUtf16String(xData, i + 1, 5);
                        //BasicConsole.WriteLine("xLongPart1: " + xLongPart);
                        // We have to check the length because 0xFFFF is a valid Unicode codepoint.
                        // So we only want to stop if the 0xFFFF is AFTER a 0x0000. We can determin
                        // this by also looking at the length. Since we short circuit the or, the length
                        // is rarely evaluated.
                        if (xLongPart.length == 5)
                        {
                            xLongPart = xLongPart + ByteConverter.GetUtf16String(xData, i + 14, 6);
                            //BasicConsole.WriteLine("xLongPart2: " + xLongPart);
                            if (xLongPart.length == 11)
                            {
                                xLongPart = xLongPart + ByteConverter.GetUtf16String(xData, i + 28, 2);
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
                            FOS_System.String xEntry = ByteConverter.GetAsciiString(xData, i, 11);
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
        public byte[] EncodeDirectoryTable(List listings, bool includeVolumeID)
        {
            int LongFilenamesSize = 0;
            if (FATType == FATTypeEnum.FAT32)
            {
                for (int i = 0; i < listings.Count; i++)
                {
                    if (((FOS_System.String)((Base)listings[i]).Name.Split('.')[0]).length > 8)
                    {
                        int nameLength = ((Base)listings[i]).Name.length;
                        LongFilenamesSize += nameLength / 13;
                        if (nameLength % 13 > 0)
                        {
                            LongFilenamesSize++;
                        }
                    }
                }
            }
            LongFilenamesSize *= 32;

            //TODO: Include VolumeID
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
                Base listing = ((Base)listings[i]);
                bool isDirectory = listing._Type == (FOS_System.Type)typeof(FATDirectory);
                FOS_System.String name = listing.Name.ToUpper();
                FOS_System.String shortNameStr = name;
                List nameParts = name.Split('.');
                bool isLongName = ((FOS_System.String)nameParts[0]).length > 8;
                if (isLongName)
                {
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
                            for (int j = LDIR_Name1.Length + 1; j < 10; j++)
                            {
                                result[offset + 1 + j] = 0xFF;
                            }
                            result[offset + 11] = LDIR_Attr;
                            result[offset + 12] = LDIR_Type;
                            result[offset + 13] = LDIR_Chksum;
                            for (int j = 0; j < LDIR_Name2.Length; j++)
                            {
                                result[offset + 14 + j] = LDIR_Name2[j];
                            }
                            for (int j = LDIR_Name2.Length + 1; j < 12; j++)
                            {
                                result[offset + 14 + j] = 0xFF;
                            }
                            result[offset + 26] = LDIR_FstClusLO;
                            for (int j = 0; j < LDIR_Name3.Length; j++)
                            {
                                result[offset + 28 + j] = LDIR_Name3[j];
                            }
                            for (int j = LDIR_Name3.Length + 1; j < 4; j++)
                            {
                                result[offset + 28 + j] = 0xFF;
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
            }
            //ExceptionMethods.Throw(new Exceptions.NotSupportedException("EncodeDirectoryTable not supported yet!"));

            return result;
        }
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

        public override Base GetListing(FOS_System.String aName)
        {
            List nameParts = aName.Split(FileSystemManager.PathDelimiter);
            List listings = GetRootDirectoryTable();
            return GetListingFromListings(nameParts, listings);
        }

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

            List listings = parent.GetListings();

            name = name.ToUpper();

            bool exists = Directory.ListingExists(name, listings);
            if (!exists)
            {
                UInt32 freeCluster = GetNextFreeCluster(2);
                SetFATEntryAndSave(freeCluster, GetFATEntryEOFValue(FATType));
                FATDirectory newDir = new FATDirectory(this, (FATDirectory)parent, name, freeCluster);
                parent.AddListing(newDir);
                parent.WriteListings();
                return newDir;
            }
            else
            {
                ExceptionMethods.Throw(new IOException("Listing (directory/file) with specified name already exists!"));
            }
            return null;
        }
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

            List listings = null;
            if (parent == null)
            {
                listings = GetRootDirectoryTable();
            }
            else
            {
                listings = parent.GetListings();
            }

            name = name.ToUpper();

            bool exists = Directory.ListingExists(name, listings);
            if (!exists)
            {
                UInt32 freeCluster = GetNextFreeCluster(2);
                SetFATEntryAndSave(freeCluster, GetFATEntryEOFValue(FATType));
                File newFile = new FATFile(this, (FATDirectory)parent, name, 0, freeCluster);
                if (parent == null)
                {
                    listings.Add(newFile);
                    _rootDirectoryFAT32.WriteListings();
                }
                else
                {
                    parent.AddListing(newFile);
                    parent.WriteListings();
                }
                return newFile;
            }
            else
            {
                ExceptionMethods.Throw(new IOException("Listing (directory/file) with specified name already exists!"));
            }
            return null;
        }

    }
}
