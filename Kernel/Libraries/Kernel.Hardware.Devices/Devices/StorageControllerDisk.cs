using Kernel.FOS_System.Processes;
using Kernel.FOS_System.Processes.Requests.Pipes;
using Kernel.FOS_System.Processes.Requests.Devices;
using Kernel.Pipes.Storage;

namespace Kernel.Hardware.Devices
{
    public class StorageControllerDisk : DiskDevice
    {
        public uint RemoteProcessId;
        public int CmdPipeId;
        public int DataOutPipeId;
        public StorageCmdOutpoint CmdPipe;
        public StorageDataOutpoint DataOutPipe;
        public StorageDataInpoint DataInPipe;

        private byte[] JunkData;

        public StorageControllerDisk(ulong AnId, uint ARemoteProcessId, int ACmdPipeId, StorageCmdOutpoint ACmdPipe, int ADataOutPipeId, StorageDataOutpoint ADataOutPipe, StorageDataInpoint ADataInPipe)
            : base(DeviceGroup.Storage, DeviceClass.Storage, DeviceSubClass.Virtual, "Storage Controller Disk", new uint[0], true)
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
            blockSize = FOS_System.ByteConverter.ToUInt64(blockSizeData, 0);
            //BasicConsole.WriteLine("Storage Controller Disk : Block size = " + (FOS_System.String)blockSize);

            JunkData = NewBlockArray(1);
        }

        public override void ReadBlock(ulong aBlockNo, uint aBlockCount, byte[] aData)
        {
            if (aBlockCount * BlockSize > (ulong)DataInPipe.BufferSize)
            {
                BasicConsole.WriteLine("WARNING! StorageControllerDisk.Read is about to cause a buffer overflow.");
            }

            //BasicConsole.WriteLine("Storage controller disk > Issuing read (storage cmd) " + (FOS_System.String)aBlockCount + " blocks from " + (FOS_System.String)aBlockNo + " blocks offset.");
            //TODO: Wrap in a loop so we don't hit buffer overflow
            CmdPipe.Send_Read(CmdPipeId, Id, aBlockNo, aBlockCount);
            int FullBytesToRead = (int)(aBlockCount * (uint)BlockSize);
            int BytesRead = DataInPipe.Read(aData, 0, FullBytesToRead, true);
            if (BytesRead != aData.Length)
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
        public override void WriteBlock(ulong aBlockNo, uint aBlockCount, byte[] aData)
        {
            if (aBlockCount * BlockSize > (ulong)DataInPipe.BufferSize)
            {
                BasicConsole.WriteLine("WARNING! StorageControllerDisk.Write might be about to cause a buffer overflow.");
            }
            else if (aData.Length < (uint)(aBlockCount * BlockSize))
            {
                BasicConsole.WriteLine("ERROR! Data buffer supplied to StorageControllerDisk.Write is not long enough for the requested number of blocks.");
                ExceptionMethods.Throw(new FOS_System.Exception("ERROR! Data buffer supplied to StorageControllerDisk.Write is not long enough for the requested number of sectors."));
            }

            //TODO: Wrap in a loop so we don't hit buffer overflow
            CmdPipe.Send_Write(CmdPipeId, Id, aBlockNo, aBlockCount);
            DataOutPipe.Write(DataOutPipeId, aData, 0, (int)(aBlockCount * (uint)BlockSize), true);
        }

        public override void CleanCaches()
        {
            CmdPipe.Send_CleanCaches(CmdPipeId, Id);
        }
    }
}
