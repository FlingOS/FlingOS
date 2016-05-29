namespace Kernel.Pipes.File
{
    public struct FilePipeDataHeader
    {
        public int Count;
    }

    public unsafe struct FilePipeDataFSInfo
    {
        public fixed char Prefix [10];
    }
}