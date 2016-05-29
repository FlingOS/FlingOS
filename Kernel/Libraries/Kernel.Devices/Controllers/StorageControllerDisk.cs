using Kernel.Framework;
using Kernel.Framework.Processes.Requests.Devices;
using Kernel.Pipes.Storage;

namespace Kernel.Devices.Controllers
{
    public class StorageControllerDisk : DiskDevice
    {
        private readonly byte[] JunkData;
        public StorageCmdOutpoint CmdPipe;
        public int CmdPipeId;
        public StorageDataInpoint DataInPipe;
        public StorageDataOutpoint DataOutPipe;
        public int DataOutPipeId;
        public uint RemoteProcessId;

        public StorageControllerDisk(ulong AnId, uint ARemoteProcessId, int ACmdPipeId, StorageCmdOutpoint ACmdPipe,
            int ADataOutPipeId, StorageDataOutpoint ADataOutPipe, StorageDataInpoint ADataInPipe)
            : base(
                DeviceGroup.Storage, DeviceClass.Storage, DeviceSubClass.Virtual, "Storage Controller Disk", new uint[0],
                true)
        {
            Id = AnId;
            RemoteProcessId = ARemoteProcessId;
            CmdPipeId = ACmdPipeId;
            CmdPipe = ACmdPipe;
            DataOutPipeId = ADataOutPipeId;
            DataOutPipe = ADataOutPipe;
            DataInPipe = ADataInPipe;

            CmdPipe.Send_BlockSize(CmdPipeId, Id);
            byte[] blockSizeData = new byte[8];
            DataInPipe.Read(blockSizeData, 0, 8, true);
            blockSize = ByteConverter.ToUInt64(blockSizeData, 0);
            //BasicConsole.WriteLine("Storage Controller Disk : Block size = " + (Framework.String)blockSize);

            JunkData = NewBlockArray(1);
        }

        public override void ReadBlock(ulong BlockNo, uint BlockCount, byte[] Data)
        {
            if (BlockCount*BlockSize > (ulong) DataInPipe.BufferSize)
            {
                BasicConsole.WriteLine("WARNING! StorageControllerDisk.Read is about to cause a buffer overflow.");
            }

            //BasicConsole.WriteLine("Storage controller disk > Issuing read (storage cmd) " + (Framework.String)BlockCount + " blocks from " + (Framework.String)BlockNo + " blocks offset.");
            //TODO: Wrap in a loop so we don't hit buffer overflow
            CmdPipe.Send_Read(CmdPipeId, Id, BlockNo, BlockCount);
            int FullBytesToRead = (int) (BlockCount*(uint) BlockSize);
            int BytesRead = DataInPipe.Read(Data, 0, FullBytesToRead, true);
            if (BytesRead != Data.Length)
            {
                BasicConsole.WriteLine("Storage controller disk > Error! Data NOT read in full.");
            }

            while (BytesRead < FullBytesToRead)
            {
                int TempBytesRead = DataInPipe.Read(JunkData, 0, FullBytesToRead, true);
                if (TempBytesRead == 0)
                {
                    BasicConsole.WriteLine("Storage controller disk > Error! Buffer could not be emptied properly.");
                    break;
                }
                BytesRead += TempBytesRead;
            }
        }

        public override void WriteBlock(ulong BlockNo, uint BlockCount, byte[] Data)
        {
            if (BlockCount*BlockSize > (ulong) DataInPipe.BufferSize)
            {
                BasicConsole.WriteLine("WARNING! StorageControllerDisk.Write might be about to cause a buffer overflow.");
            }
            else if (Data.Length < (uint) (BlockCount*BlockSize))
            {
                BasicConsole.WriteLine(
                    "ERROR! Data buffer supplied to StorageControllerDisk.Write is not long enough for the requested number of blocks.");
                ExceptionMethods.Throw(
                    new Exception(
                        "ERROR! Data buffer supplied to StorageControllerDisk.Write is not long enough for the requested number of sectors."));
            }

            //TODO: Wrap in a loop so we don't hit buffer overflow
            CmdPipe.Send_Write(CmdPipeId, Id, BlockNo, BlockCount);
            DataOutPipe.Write(DataOutPipeId, Data, 0, (int) (BlockCount*(uint) BlockSize), true);
        }

        public override void CleanCaches()
        {
            CmdPipe.Send_CleanCaches(CmdPipeId, Id);
        }
    }
}