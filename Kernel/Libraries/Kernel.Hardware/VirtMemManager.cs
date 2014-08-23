using System;
using Kernel.Hardware.VirtMem;

namespace Kernel.Hardware
{
    public static unsafe class VirtMemManager
    {
        private static VirtMemImpl impl;

        public static void Init()
        {
            impl = new x86();
        }

        public static void Map(void* pAddr, void* vAddr, uint size)
        {
            Map((uint)pAddr, (uint)vAddr, size);
        }
        public static void Map(uint pAddr, void* vAddr, uint size)
        {
            Map(pAddr, (uint)vAddr, size);
        }
        public static void Map(void* pAddr, uint vAddr, uint size)
        {
            Map((uint)pAddr, vAddr, size);
        }
        public static void Map(uint pAddr, uint vAddr, uint size)
        {
            while (size > 0)
            {
                impl.Map(pAddr, vAddr);
                size -= 4096;
                pAddr += 4096;
                vAddr += 4096;
            }
        }

        public static void* GetPhysicalAddress(void* vAddr)
        {
            return (void*)GetPhysicalAddress((uint)vAddr);
        }
        public static uint GetPhysicalAddress(uint vAddr)
        {
            return impl.GetPhysicalAddress(vAddr);
        }

        public static void Test()
        {
            impl.Test();
        }
    }
}
