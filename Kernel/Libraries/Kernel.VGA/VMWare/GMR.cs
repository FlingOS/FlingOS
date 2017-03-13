using System.Runtime.Remoting.Messaging;
using Kernel.Framework;
using Kernel.Framework.Processes;

namespace Kernel.VGA.VMWare
{
    public unsafe class GMR : Object
    {
        public const int PAGE_SIZE = 4096;
        public const int PAGE_SHIFT = 12;
        public const uint PAGE_MASK = PAGE_SIZE - 1;

        public static void* PPN_POINTER(uint ppn)
        {
            uint vpage;
            if (SystemCalls.GetVirtualAddress(ppn * PAGE_SIZE, out vpage) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("GMR : Couldn't get virtual address of page.");
                ExceptionMethods.Throw(new Exception("GMR : Couldn't get virtual address of page."));
            }
            return (void*)vpage;
        }
        
        public uint MaxIds;
        public uint MaxDescriptorLength;
        public uint MaxPages;

        private uint AllocPages(int count)
        {
            uint page;
            if (SystemCalls.RequestPages((uint)count, out page) != SystemCallResults.OK)
            {
                Error("Couldn't allocate pages.");
            }
            if (SystemCalls.GetPhysicalAddress(page, out page) != SystemCallResults.OK)
            {
                Error("Couldn't get physical address of allocated page.");
            }
            return page / PAGE_SIZE;
        }

        private void FreePages(uint page, uint count)
        {
            if (SystemCalls.UnmapPages(page, count) != SystemCallResults.OK)
            {
                Error("Couldn't unmap pages.");
            }
        }

        public void Init(SVGAII svga)
        {
            if ((svga.Capabilities & (uint)SVGAII_Registers.Capabilities.GMR) != 0)
            {
                MaxIds = svga.ReadReg((uint)SVGAII_Registers.Registers.GMR_MAX_IDS);
                MaxDescriptorLength = svga.ReadReg((uint)SVGAII_Registers.Registers.GMR_MAX_DESCRIPTOR_LENGTH);
            }
            else
            {
                svga.Error("Virtual device does not have Guest Memory Region (GMR) support.");
            }
        }

        public void Init2(SVGAII svga)
        {
            if ((svga.Capabilities & (uint)SVGAII_Registers.Capabilities.GMR2) != 0)
            {
                MaxIds = svga.ReadReg((uint)SVGAII_Registers.Registers.GMR_MAX_IDS);
                MaxPages = svga.ReadReg((uint)SVGAII_Registers.Registers.GMRS_MAX_PAGES);
            }
            else
            {
                svga.Error("Virtual device does not have Guest Memory Region version 2 (GMR2) support.");
            }
        }

        public void Error(String message)
        {
            message = "GMR : " + message;
            BasicConsole.WriteLine(message);
            ExceptionMethods.Throw(new Exception(message));
        }

        public uint AllocDescriptor(SVGAII_Registers.GuestMemoryDescriptor* descArray,
            uint numDescriptors)
        {
            uint descPerPage = PAGE_SIZE / (uint)sizeof(SVGAII_Registers.GuestMemoryDescriptor) - 1;
            SVGAII_Registers.GuestMemoryDescriptor* desc = null;
            uint firstPage = 0;
            uint page = 0;
            int i = 0;

            while (numDescriptors != 0)
            {
                if (firstPage == 0)
                {
                    page = firstPage = AllocPages(1);
                }

                desc = (SVGAII_Registers.GuestMemoryDescriptor*)PPN_POINTER(page);

                if (i == descPerPage)
                {
                    page = AllocPages(1);
                    desc[i].PPN = page;
                    desc[i].NumPages = 0;
                    i = 0;
                    continue;
                }

                desc[i] = *descArray;
                i++;
                descArray++;
                numDescriptors--;
            }

            if (desc != null)
            {
                desc[i].PPN = 0;
                desc[i].NumPages = 0;
            }

            return firstPage;
        }

        public void Define(SVGAII svga,
            uint gmrId, SVGAII_Registers.GuestMemoryDescriptor* descArray, uint numDescriptors)
        {
            uint desc = AllocDescriptor(descArray, numDescriptors);
            svga.WriteReg((uint)SVGAII_Registers.Registers.GMR_ID, gmrId);
            svga.WriteReg((uint)SVGAII_Registers.Registers.GMR_DESCRIPTOR, desc);

            //if (desc != null)
            //{
            //    FreePages(desc, 1);
            //}
        }

        public uint DefineContiguous(SVGAII svga, uint gmrId, uint numPages)
        {
            SVGAII_Registers.GuestMemoryDescriptor desc = new SVGAII_Registers.GuestMemoryDescriptor()
            {
                PPN = AllocPages((int)numPages),
                NumPages = numPages
            };

            Define(svga, gmrId, &desc, 1);

            return desc.PPN;
        }

        public uint DefineEvenPages(SVGAII svga, uint gmrId, uint numPages)
        {
            uint region = AllocPages((int)numPages * 2);

            SVGAII_Registers.GuestMemoryDescriptor* desc = (SVGAII_Registers.GuestMemoryDescriptor*)
                Heap.AllocZeroed(
                    (uint)sizeof(SVGAII_Registers.GuestMemoryDescriptor) * numPages, 4, "GMR DefineEvenPages");

            for (uint i = 0; i < numPages; i++)
            {
                desc[i].PPN = region + i * 2;
                desc[i].NumPages = 1;
            }

            Define(svga, gmrId, desc, numPages);

            return region;
        }

        public void FreeAll(SVGAII svga)
        {
            for (uint id = 0; id < MaxIds; id++)
            {
                Define(svga, id, null, 0);
            }
        }
    }
}
