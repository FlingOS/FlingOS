namespace Kernel.Pipes.Storage
{
    public enum StorageCommands : int
    {
        Invalid = -1,
        None = 0,
        Read = 1,
        Write = 2
    }
    public struct StoragePipeCommand
    {
        public int Command;
    }
}
