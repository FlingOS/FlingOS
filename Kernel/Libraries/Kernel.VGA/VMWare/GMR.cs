using System.Runtime.Remoting.Messaging;
using Kernel.Framework;

namespace Kernel.VGA.VMWare
{
    public unsafe class GMR : Object
    {
        public const int PAGE_SIZE = 4096;
        public const int PAGE_SHIFT = 12;
        public const uint PAGE_MASK = PAGE_SIZE - 1;

        public static void* PPN_POINTER(void* ppn)
        {
            return (void*)((uint)ppn * PAGE_SIZE);
        }

        public struct State
        {
            public uint MaxIds;
            public uint MaxDescriptorLength;
            public uint MaxPages;
        }


    }
}
