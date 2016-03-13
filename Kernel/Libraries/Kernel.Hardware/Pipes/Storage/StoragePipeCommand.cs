namespace Kernel.Pipes.Storage
{
    public enum StorageCommands : int
    {
        Invalid = -1,
        None = 0,
        DiskList,
        Read,
        Write
    }
    public struct StoragePipeCommand
    {
        public int Command;
    }
}
