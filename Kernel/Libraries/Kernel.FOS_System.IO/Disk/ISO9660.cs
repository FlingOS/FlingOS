using System;
using Kernel.FOS_System.Collections;

namespace Kernel.FOS_System.IO.Disk
{
    public class ISO9660 : FOS_System.Object
    {
        public List VolumeDescriptors = new List(3);

        public ISO9660(Hardware.Devices.DiskDevice disk)
        {
            byte[] data = disk.NewBlockArray(1);
            VolumeDescriptor desciptor = null;
            uint sector = 0x10;
            do
            {
                disk.ReadBlock(sector, 1, data);
                desciptor = VolumeDescriptor.CreateDescriptor(data);
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

        public class VolumeDescriptor : FOS_System.Object
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

            public VolumeDescriptor(byte[] data)
            {
                Code = (TypeCodes)data[0];
                Id = ByteConverter.GetASCIIStringFromASCII(data, 1, 5);
                Version = data[6];
            }

            public static VolumeDescriptor CreateDescriptor(byte[] data)
            {
                switch ((TypeCodes)data[0])
                {
                    case TypeCodes.BootRecord:
                        return new BootRecord(data);
                    case TypeCodes.Primary:
                        return new PrimaryVolumeDescriptor(data);
                    case TypeCodes.SetTerminator:
                        return new SetTerminatorVolumeDescriptor(data);
                    default:
                        return new VolumeDescriptor(data);
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

            public BootRecord(byte[] data)
                : base(data)
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
            
            public PrimaryVolumeDescriptor(byte[] data)
                : base(data)
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
                RootDirectory = new DirectoryRecord(data, 156);
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
            }
        }
        public class SetTerminatorVolumeDescriptor : VolumeDescriptor
        {
            public SetTerminatorVolumeDescriptor(byte[] data)
                : base(data)
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
            public DirectoryRecord(byte[] data, uint offset)
            {
            }

            public FOS_System.String ConvertToString()
            {
                return "";
            }
        }
    }
}