namespace Kernel.Framework.Processes
{
    public static class InterruptUtils
    {
        public static int ConstructInterruptResult(SystemCallResults result, uint value)
        {
            if ((value & 0xFF000000) != 0)
            {
                return (int)SystemCallResults.Fail;
            }

            return (int)((uint)result | (value << 8));
        }
    }
}
