namespace Kernel.Pipes.File
{
    public enum FileCommands
    {
        Invalid = -1,
        None = 0,
        StatFS,
        Open,
        Close,
        Delete,
        Read,
        Write,
        Seek,
        Stat,
        Rename,
        Sync,
        Truncate,
        MakeDir,
        DeleteDir,
        ListDir,
        ReadDirEntry
    }

    public struct FilePipeCommand
    {
        public int Command;
        public ulong DiskId;
        public ulong FileHandle;
        public ulong BlockNo;
        public uint BlockCount;
    }
}