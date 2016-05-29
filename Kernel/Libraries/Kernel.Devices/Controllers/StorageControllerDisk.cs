using Kernel.Framework;
using Kernel.Framework.Exceptions;
using Kernel.Framework.Processes.Requests.Devices;
using Kernel.Pipes.Storage;

namespace Kernel.Devices.Controllers
{
    /// <summary>
    ///     The client-side counterpart to <see cref="StorageController"/>. Provides seemless access to a disk
    ///     that is controlled by storage controller in a remote process.
    /// </summary>
    /// <remarks>
    ///     Multiple disks can be accessed down a single pipe, so care should be taken when using multiple 
    ///     instances of this class. The owner/user should ensure that only one thread per pipe used, not per 
    ///     disk used, is allowed to call methods in this class. This essentially means not allowing simultaneous
    ///     calls to instances of this class i.e. only use instances from a single thread.
    /// 
    ///     Why is this an issue? Right now it isn't bu if the Write/Read methods are implemented to use 
    ///     appropriate (and necessary) chunking, data-jumbling could occur if simultaneous calls are made.
    /// </remarks>
    public class StorageControllerDisk : DiskDevice
    {
        private readonly byte[] JunkData;
        private readonly StorageCmdOutpoint CommandPipe;
        private readonly int CommandPipeId;
        private readonly StorageDataInpoint DataInPipe;
        private readonly StorageDataOutpoint DataOutPipe;
        private readonly int DataOutPipeId;
        private readonly uint RemoteProcessId;

        /// <summary>
        ///     Initialises a new virtual disk with the specified information.
        /// </summary>
        /// <param name="DiskId">The device Id of the remote disk.</param>
        /// <param name="RemoteProcessId">The Id of the remote (storage controller) process to which the command and data pipes are connected.</param>
        /// <param name="CommandPipeId">The Id of the command pipe.</param>
        /// <param name="CommandPipe">The command pipe instance.</param>
        /// <param name="DataOutPipeId">The Id of the data output pipe.</param>
        /// <param name="DataOutPipe">The data output pipe instance.</param>
        /// <param name="DataInPipe">The Id of the data input pipe.</param>
        public StorageControllerDisk(ulong DiskId, uint RemoteProcessId, int CommandPipeId, StorageCmdOutpoint CommandPipe,
            int DataOutPipeId, StorageDataOutpoint DataOutPipe, StorageDataInpoint DataInPipe)
            : base(
                DeviceGroup.Storage, DeviceClass.Storage, DeviceSubClass.Virtual, "Storage Controller Disk", new uint[0],
                true)
        {
            Id = DiskId;
            this.RemoteProcessId = RemoteProcessId;
            this.CommandPipeId = CommandPipeId;
            this.CommandPipe = CommandPipe;
            this.DataOutPipeId = DataOutPipeId;
            this.DataOutPipe = DataOutPipe;
            this.DataInPipe = DataInPipe;

            this.CommandPipe.Send_BlockSize(this.CommandPipeId, Id);
            byte[] BlockSizeData = new byte[8];
            this.DataInPipe.Read(BlockSizeData, 0, 8, true);
            blockSize = ByteConverter.ToUInt64(BlockSizeData, 0);
            //BasicConsole.WriteLine("Storage Controller Disk : Block size = " + (Framework.String)blockSize);

            JunkData = NewBlockArray(1);
        }

        /// <summary>
        ///     Reads contiguous logical blocks from the device.
        /// </summary>
        /// <param name="BlockNo">The logical block number to read.</param>
        /// <param name="BlockCount">The number of blocks to read.</param>
        /// <param name="Data">The byte array to store the data in.</param>
        public override void ReadBlock(ulong BlockNo, uint BlockCount, byte[] Data)
        {
            if (BlockCount*BlockSize > (ulong)DataInPipe.BufferSize)
            {
                BasicConsole.WriteLine("WARNING! StorageControllerDisk.Read is about to cause a buffer overflow.");
            }

            //BasicConsole.WriteLine("Storage controller disk > Issuing read (storage cmd) " + (Framework.String)BlockCount + " blocks from " + (Framework.String)BlockNo + " blocks offset.");
            //TODO: Wrap in a loop so we don't hit buffer overflow
            CommandPipe.Send_Read(CommandPipeId, Id, BlockNo, BlockCount);
            int FullBytesToRead = (int)(BlockCount*(uint)BlockSize);
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

        /// <summary>
        ///     Writes contiguous logical blocks to the device.
        /// </summary>
        /// <param name="BlockNo">The number of the first block to write.</param>
        /// <param name="BlockCount">The number of blocks to write.</param>
        /// <param name="Data">The data to write. Pass null to efficiently write 0s to the device.</param>
        /// <remarks>
        ///     If data is null, all data to be written should be assumed to be 0.
        /// </remarks>
        public override void WriteBlock(ulong BlockNo, uint BlockCount, byte[] Data)
        {
            if (Data == null)
            {
                ExceptionMethods.Throw(new NotImplementedException("Writing 0s using null array through Storage Controller Disk is not implemented yet!"));
            }
            else if (BlockCount*BlockSize > (ulong)DataInPipe.BufferSize)
            {
                BasicConsole.WriteLine("WARNING! StorageControllerDisk.Write might be about to cause a buffer overflow.");
            }
            else if (Data.Length < (uint)(BlockCount*BlockSize))
            {
                BasicConsole.WriteLine(
                    "ERROR! Data buffer supplied to StorageControllerDisk.Write is not long enough for the requested number of blocks.");
                ExceptionMethods.Throw(
                    new Exception(
                        "ERROR! Data buffer supplied to StorageControllerDisk.Write is not long enough for the requested number of sectors."));
            }

            //TODO: Wrap in a loop so we don't hit buffer overflow
            CommandPipe.Send_Write(CommandPipeId, Id, BlockNo, BlockCount);
            DataOutPipe.Write(DataOutPipeId, Data, 0, (int)(BlockCount*(uint)BlockSize), true);
        }

        /// <summary>
        ///     Cleans the software and hardware caches (if any) by writing necessary data
        ///     to disk before wiping the caches.
        /// </summary>
        public override void CleanCaches()
        {
            CommandPipe.Send_CleanCaches(CommandPipeId, Id);
        }
    }
}