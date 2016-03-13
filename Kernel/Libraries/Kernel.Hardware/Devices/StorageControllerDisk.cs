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
            BasicConsole.WriteLine("Storage Controller Disk : Block size = " + (FOS_System.String)blockSize);
        }

        public override void ReadBlock(ulong aBlockNo, uint aBlockCount, byte[] aData)
        {
            if (aBlockCount * BlockSize > (ulong)DataInPipe.BufferSize)
            {
                BasicConsole.WriteLine("WARNING! StorageControllerDisk.Read is about to cause a buffer overflow.");
            }

            //TODO: Wrap in a loop so we don't hit buffer overflow
            CmdPipe.Send_Read(CmdPipeId, Id, aBlockNo, aBlockCount);
            DataInPipe.Read(aData, 0, (int)(aBlockCount * BlockSize), true);

            //BasicConsole.WriteLine("Data read: ");
            //for (int i = 0; i < aData.Length; i++)
            //{
            //    BasicConsole.Write(aData[i]);
            //}
        }
        public override void WriteBlock(ulong aBlockNo, uint aBlockCount, byte[] aData)
        {
            //TODO
        }

        public override void CleanCaches()
        {
            //TODO
        }
    }
}
