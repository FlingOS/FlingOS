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
    
using System;
using Kernel.FOS_System.Collections;

namespace Kernel.FOS_System.IO.Disk
{
    public class ISO9660 : FOS_System.Object
    {
        public List VolumeDescriptors = new List(3);

        public ISO9660(Hardware.Devices.DiskDevice disk)
        {
            VolumeDescriptor desciptor = null;
            uint sector = 0x10;
            do
            {
                desciptor = VolumeDescriptor.CreateDescriptor(disk, sector, 1);
                VolumeDescriptors.Add(desciptor);

                sector++;
            }
            while (desciptor.Code != VolumeDescriptor.TypeCodes.SetTerminator);
        }

        public void Print()
        {
            for (int i = 0; i < VolumeDescriptors.Count; i++)
            {
                VolumeDescriptor desciptor = (VolumeDescriptor)VolumeDescriptors[i];
                desciptor.Print();
            }
        }

        public class VolumeDescriptor : Partition
        {
            public enum TypeCodes : byte
            {
                BootRecord = 0,
                Primary = 1,
                Supplementary = 2,
                VolumePartition = 3,
                SetTerminator = 255
            }

            public TypeCodes Code;
            public FOS_System.String Id;
            public byte Version;

            public VolumeDescriptor(Hardware.Devices.DiskDevice disk, uint startBlock, uint numBlocks, byte[] data)
                : base(disk, 0, 0)
            {
                Code = (TypeCodes)data[0];
                Id = ByteConverter.GetASCIIStringFromASCII(data, 1, 5);
                Version = data[6];
            }

            public static VolumeDescriptor CreateDescriptor(Hardware.Devices.DiskDevice disk, uint startBlock, uint numBlocks)
            {
                byte[] data = disk.NewBlockArray(numBlocks);
                disk.ReadBlock(startBlock, numBlocks, data);

                switch ((TypeCodes)data[0])
                {
                    case TypeCodes.BootRecord:
                        return new BootRecord(disk, startBlock, numBlocks, data);
                    case TypeCodes.Primary:
                        return new PrimaryVolumeDescriptor(disk, startBlock, numBlocks, data);
                    case TypeCodes.SetTerminator:
                        return new SetTerminatorVolumeDescriptor(disk, startBlock, numBlocks, data);
                    default:
                        return new VolumeDescriptor(disk, startBlock, numBlocks, data);
                }
            }

            public virtual void Print()
            {
                BasicConsole.WriteLine("Volume Descriptor:");
                BasicConsole.WriteLine("    > Type Code : " + (FOS_System.String)(byte)Code);
                BasicConsole.WriteLine("    > Id : " + Id);
                BasicConsole.WriteLine("    > Version : " + (FOS_System.String)Version);
            }
        }
        public class BootRecord : VolumeDescriptor
        {
            public FOS_System.String BootSystemIdentifier;
            public FOS_System.String BootSystem;

            public BootRecord(Hardware.Devices.DiskDevice disk, uint startBlock, uint numBlocks, byte[] data)
                : base(disk, startBlock, numBlocks, data)
            {
                BootSystemIdentifier = ByteConverter.GetASCIIStringFromASCII(data, 7, 32);
                BootSystem = ByteConverter.GetASCIIStringFromASCII(data, 39, 32);
            }

            public override void Print()
            {
                base.Print();

                BasicConsole.WriteLine("    Boot record:");
                BasicConsole.WriteLine("        > Boot System Identifier : " + BootSystemIdentifier);
                BasicConsole.WriteLine("        > Boot System            : " + BootSystem);
            }
        }
        public class PrimaryVolumeDescriptor : VolumeDescriptor
        {
            public FOS_System.String SystemIdentifier;
            public FOS_System.String VolumeIdentifier;
            public UInt32 VolumeSpaceSize;
            public UInt16 VolumeSetSize;
            public UInt16 VolumeSequenceNumber;
            public UInt16 LogicalBlockSize;
            public UInt32 PathTableSize;
            public UInt32 Location_PathTable_TypeL;
            public UInt32 Location_PathTable_Optional_TypeL;
            public DirectoryRecord RootDirectory;
            public FOS_System.String VolumeSetIdentifier;
            public FOS_System.String PublisherIdentifier;
            public FOS_System.String DataPreparerIdentifier;
            public FOS_System.String ApplicationIdentifier;
            public FOS_System.String CopyrightFileIdentifier;
            public FOS_System.String AbstractFileIdentifier;
            public FOS_System.String BibliographicFileIdentifier;
            public DateTime VolumeCreationDateTime;
            public DateTime VolumeModificationDateTime;
            public DateTime VolumeExpirationDateTime;
            public DateTime VolumeEffectiveDateTime;
            public byte FileStructureVersion;

            public PrimaryVolumeDescriptor(Hardware.Devices.DiskDevice disk, uint startBlock, uint numBlocks, byte[] data)
                : base(disk, startBlock, numBlocks, data)
            {
                SystemIdentifier = ByteConverter.GetASCIIStringFromASCII(data, 8, 32);
                VolumeIdentifier = ByteConverter.GetASCIIStringFromASCII(data, 40, 32);
                VolumeSpaceSize = ByteConverter.ToUInt32(data, 80);
                VolumeSetSize = ByteConverter.ToUInt16(data, 120);
                VolumeSequenceNumber = ByteConverter.ToUInt16(data, 124);
                LogicalBlockSize = ByteConverter.ToUInt16(data, 128);
                PathTableSize = ByteConverter.ToUInt32(data, 132);
                Location_PathTable_TypeL = ByteConverter.ToUInt32(data, 140);
                Location_PathTable_Optional_TypeL = ByteConverter.ToUInt32(data, 144);
                RootDirectory = new DirectoryRecord(data, 156, true);
                VolumeSetIdentifier = ByteConverter.GetASCIIStringFromASCII(data, 190, 128);
                PublisherIdentifier = ByteConverter.GetASCIIStringFromASCII(data, 318, 128);
                DataPreparerIdentifier = ByteConverter.GetASCIIStringFromASCII(data, 446, 128);
                ApplicationIdentifier = ByteConverter.GetASCIIStringFromASCII(data, 574, 128);
                CopyrightFileIdentifier = ByteConverter.GetASCIIStringFromASCII(data, 702, 38);
                AbstractFileIdentifier = ByteConverter.GetASCIIStringFromASCII(data, 740, 36);
                BibliographicFileIdentifier = ByteConverter.GetASCIIStringFromASCII(data, 776, 37);
                VolumeCreationDateTime = new DateTime(data, 813);
                VolumeModificationDateTime = new DateTime(data, 830);
                VolumeExpirationDateTime = new DateTime(data, 847);
                VolumeEffectiveDateTime = new DateTime(data, 864);
                FileStructureVersion = data[881];
            }

            public class DateTime : FOS_System.Object
            {
                public FOS_System.String Year;
                public FOS_System.String Month;
                public FOS_System.String Day;
                public FOS_System.String Hour;
                public FOS_System.String Minute;
                public FOS_System.String Second;
                public FOS_System.String HundrethsSecond;
                public byte Timezone;

                public DateTime(byte[] data, uint offset)
                {
                    Year = ByteConverter.GetASCIIStringFromASCII(data, offset + 0, 4);
                    Month = ByteConverter.GetASCIIStringFromASCII(data, offset + 4, 2).TrimEnd().PadLeft(2, '0');
                    Day = ByteConverter.GetASCIIStringFromASCII(data, offset + 6, 2).TrimEnd().PadLeft(2, '0');
                    Hour = ByteConverter.GetASCIIStringFromASCII(data, offset + 8, 2).TrimEnd().PadLeft(2, '0');
                    Minute = ByteConverter.GetASCIIStringFromASCII(data, offset + 10, 2).TrimEnd().PadLeft(2, '0');
                    Second = ByteConverter.GetASCIIStringFromASCII(data, offset + 12, 2).TrimEnd().PadLeft(2, '0');
                    HundrethsSecond = ByteConverter.GetASCIIStringFromASCII(data, offset + 14, 2);
                    Timezone = data[offset + 16];
                }

                public FOS_System.String ConvertToString()
                {
                    return Day + "/" + Month + "/" + Year + " " + Hour + ":" + Minute + ":" + Second + ":" + HundrethsSecond + "0";
                }
            }

            public override void Print()
            {
                base.Print();

                BasicConsole.WriteLine("    Primary Volume Descriptor:");
                BasicConsole.WriteLine("        > SystemIdentifier : " + SystemIdentifier);
                BasicConsole.WriteLine("        > VolumeIdentifier : " + VolumeIdentifier);
                BasicConsole.WriteLine("        > VolumeSpaceSize : " + (FOS_System.String)VolumeSpaceSize);
                BasicConsole.WriteLine("        > VolumeSetSize : " + (FOS_System.String)VolumeSetSize);
                BasicConsole.WriteLine("        > VolumeSequenceNumber : " + (FOS_System.String)VolumeSequenceNumber);
                BasicConsole.WriteLine("        > LogicalBlockSize : " + (FOS_System.String)LogicalBlockSize);
                BasicConsole.WriteLine("        > PathTableSize : " + (FOS_System.String)PathTableSize);
                BasicConsole.WriteLine("        > Location_PathTable_TypeL : " + (FOS_System.String)Location_PathTable_TypeL);
                BasicConsole.WriteLine("        > Location_PathTable_Optional_TypeL : " + (FOS_System.String)Location_PathTable_Optional_TypeL);
                BasicConsole.WriteLine("        > VolumeSetIdentifier : " + VolumeSetIdentifier);
                BasicConsole.WriteLine("        > PublisherIdentifier : " + PublisherIdentifier);
                BasicConsole.WriteLine("        > DataPreparerIdentifier : " + DataPreparerIdentifier);
                BasicConsole.WriteLine("        > ApplicationIdentifier : " + ApplicationIdentifier);
                BasicConsole.WriteLine("        > CopyrightFileIdentifier : " + CopyrightFileIdentifier);
                BasicConsole.WriteLine("        > AbstractFileIdentifier : " + AbstractFileIdentifier);
                BasicConsole.WriteLine("        > BibliographicFileIdentifier : " + BibliographicFileIdentifier);
                BasicConsole.WriteLine("        > VolumeCreationDateTime : " + VolumeCreationDateTime.ConvertToString());
                BasicConsole.WriteLine("        > VolumeModificationDateTime : " + VolumeModificationDateTime.ConvertToString());
                BasicConsole.WriteLine("        > VolumeExpirationDateTime : " + VolumeExpirationDateTime.ConvertToString());
                BasicConsole.WriteLine("        > VolumeEffectiveDateTime : " + VolumeEffectiveDateTime.ConvertToString());
                BasicConsole.WriteLine("        > FileStructureVersion : " + (FOS_System.String)FileStructureVersion);
                BasicConsole.WriteLine("        Root directory: ");
                BasicConsole.WriteLine(RootDirectory.ConvertToString());
            }
        }
        public class SetTerminatorVolumeDescriptor : VolumeDescriptor
        {
            public SetTerminatorVolumeDescriptor(Hardware.Devices.DiskDevice disk, uint startBlock, uint numBlocks, byte[] data)
                : base(disk, startBlock, numBlocks, data)
            {
            }

            public override void Print()
            {
                base.Print();

                BasicConsole.WriteLine("    Set Terminator Volume Descriptor");
            }
        }

        public class DirectoryRecord : FOS_System.Object
        {
            public enum FileFlags : byte
            {
                /// <summary>
                /// If set, the existence of this file need not be made known to the user (basically a 'hidden' flag).
                /// </summary>
                Hidden = 0x1,
                /// <summary>
                /// If set, this record describes a directory (in other words, it is a subdirectory extent).
                /// </summary>
                Directory = 0x2,
                /// <summary>
                /// If set, this file is an "Associated File".
                /// </summary>
                AssociatedFile = 0x4,
                /// <summary>
                /// If set, the extended attribute record contains information about the format of this file.
                /// </summary>
                ExtAttrContainsFormatInfo = 0x8,
                /// <summary>
                /// If set, owner and group permissions are set in the extended attribute record.
                /// </summary>
                ExtAttrContainsOwnerAndGroupPermissions = 0x10,
                /// <summary>
                /// If set, this is not the final directory record for this file (for files spanning several extents, for example files over 4GiB long.
                /// </summary>
                RemainingRecords = 0x80,
            }

            public byte RecordLength;
            public byte ExtAttrRecordLength;
            public UInt32 LBALocation;
            public UInt32 DataLength;
            public DateTime RecordingDateTime;
            public FileFlags TheFileFlags;
            public byte FileUnitSize;
            public byte FileInterleaveGapSize;
            public UInt16 VolumeSequenceNumber;
            public byte FileIdentifierLength;
            public FOS_System.String FileIdentifier;

            public bool IsRootDirectory;

            public DirectoryRecord(byte[] data, uint offset, bool isRootDirectory = false)
            {
                IsRootDirectory = isRootDirectory;

                RecordLength = data[offset + 0];
                ExtAttrRecordLength = data[offset + 1];
                LBALocation = ByteConverter.ToUInt32(data, offset + 2);
                DataLength = ByteConverter.ToUInt32(data, offset + 10);
                RecordingDateTime = new DateTime(data, offset + 18);
                TheFileFlags = (FileFlags)data[offset + 25];
                FileUnitSize = data[offset + 26];
                FileInterleaveGapSize = data[offset + 27];
                VolumeSequenceNumber = ByteConverter.ToUInt16(data, offset + 28);
                FileIdentifierLength = data[offset + 32];
                FileIdentifier = IsRootDirectory ? "ROOT_DIRECTORY" : ByteConverter.GetASCIIStringFromASCII(data, offset + 33, FileIdentifierLength);
            }

            public FOS_System.String ConvertToString()
            {
                FOS_System.String result = "Directory record: ";

                result += "\r\n     > RecordLength : " + (FOS_System.String)RecordLength;
                result += "\r\n     > ExtAttrRecordLength : " + (FOS_System.String)ExtAttrRecordLength;
                result += "\r\n     > LBALocation : " + (FOS_System.String)LBALocation;
                result += "\r\n     > DataLength : " + (FOS_System.String)DataLength;
                result += "\r\n     > RecordingDateTime : " + RecordingDateTime.ConvertToString();
                result += "\r\n     > TheFileFlags : " + (FOS_System.String)(byte)TheFileFlags;
                result += "\r\n     > FileUnitSize : " + (FOS_System.String)FileUnitSize;
                result += "\r\n     > FileInterleaveGapSize : " + (FOS_System.String)FileInterleaveGapSize;
                result += "\r\n     > VolumeSequenceNumber : " + (FOS_System.String)VolumeSequenceNumber;
                result += "\r\n     > FileIdentifierLength : " + (FOS_System.String)FileIdentifierLength;
                result += "\r\n     > FileIdentifier : " + FileIdentifier;

                return result;
            }

            public class DateTime : FOS_System.Object
            {
                public byte YearsSince1990;
                public byte Month;
                public byte Day;
                public byte Hour;
                public byte Minute;
                public byte Second;
                public byte GMTOffsetInterval;

                public DateTime(byte[] data, uint offset)
                {
                    YearsSince1990 = data[offset + 0];
                    Month = data[offset + 1];
                    Day = data[offset + 2];
                    Hour = data[offset + 3];
                    Minute = data[offset + 4];
                    Second = data[offset + 5];
                    GMTOffsetInterval = data[offset + 6];
                }

                public FOS_System.String ConvertToString()
                {
                    return (FOS_System.String)Day + "/" + Month + "/" + (YearsSince1990 + 1990) + " " + Hour + ":" + Minute + ":" + Second;
                }
            }
        }
    }
}