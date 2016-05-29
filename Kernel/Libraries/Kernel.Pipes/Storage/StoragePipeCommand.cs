namespace Kernel.Pipes.Storage
{
    public enum StorageCommands
    {
        Invalid = -1,
        None = 0,
        DiskList,
        Read,
        Write,
        BlockSize,
        CleanCaches
    }

    public struct StoragePipeCommand
    {
        public int Command;
        public ulong DiskId;
        public ulong BlockNo;
        public uint BlockCount;
    }
}