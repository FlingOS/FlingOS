#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //
#endregion
    
#define HEAP_TRACE
#undef HEAP_TRACE
    
using System;

namespace Kernel.FOS_System
{
    //This implementation is based off of Leonard Kevin McGuire Jr's Binary Heap Implementation
    //found at http://wiki.osdev.org/User:Pancakes/BitmapHeapImplementation
    //(www.kmcg3413.net) (kmcg3413@gmail.com)

    /// <summary>
    /// Represents a block of memory that has been allocated for use by the heap.
    /// </summary>
    public unsafe struct HeapBlock
    {
        /// <summary>
        /// A pointer to the next heap block.
        /// </summary>
        public HeapBlock* next;
        /// <summary>
        /// The size of the block of memory allocated.
        /// </summary>
        public UInt32 size;
        /// <summary>
        /// The amount of memory in the block that has been used.
        /// </summary>
        public UInt32 used;
        /// <summary>
        /// The size of the chunks to use when allocating memory.
        /// </summary>
        public UInt32 bsize;
        /// <summary>
        /// Used for optimisation.
        /// </summary>
        public UInt32 lfb;
    }

    /// <summary>
    /// The kernel heap - currently a very simple implementation.
    /// </summary>
    [Compiler.PluggedClass]
    [Drivers.Compiler.Attributes.PluggedClass]
    public static unsafe class Heap
    {
        public static bool PreventAllocation = false;
        public static FOS_System.String PreventReason = "[NONE]";

        /// <summary>
        /// A pointer to the most-recently added heap block.
        /// </summary>
        private static HeapBlock* fblock;

        /// <summary>
        /// A pointer to the most-recently added heap block.
        /// </summary>
        public static HeapBlock* FBlock
        {
            get
            {
                return fblock;
            }
        }

        /// <summary>
        /// Calculates the total amount of free memory in the heap.
        /// </summary>
        /// <returns>The total amount of free memory in the heap.</returns>
        public static UInt32 GetTotalFreeMem()
        {
            HeapBlock* cBlock = fblock;
            UInt32 result = 0;
            while (cBlock != null)
            {
                result += GetFreeMem(cBlock);
                cBlock = cBlock->next;
            }
            return result;
        }
        /// <summary>
        /// Calculates the amount of free memory in the specified block.
        /// </summary>
        /// <param name="aBlock">The block to calculate free mem of.</param>
        /// <returns>The amount of free memory in bytes.</returns>
        public static UInt32 GetFreeMem(HeapBlock* aBlock)
        {
            return aBlock->size - (aBlock->used * aBlock->bsize);
        }

        /// <summary>
        /// Whether the kernel's fixed heap has been initialised or not.
        /// </summary>
        private static bool FixedHeapInitialised = false;

        /// <summary>
        /// Gets a pointer to the block of memory to allocate to the kernel's fixed heap.
        /// </summary>
        /// <returns>The pointer to the block of memory.</returns>
        [Compiler.PluggedMethod(ASMFilePath=@"ASM\Heap\GetFixedHeapPtr")]
        public static UInt32* GetFixedHeapPtr()
        {
            //Stub for use by testing framework
            return (UInt32*)System.Runtime.InteropServices.Marshal.AllocHGlobal((int)GetFixedHeapSize());
        }
        /// <summary>
        /// Gets the size of the block of memory to allocate to the kernel's fixed heap.
        /// </summary>
        /// <returns>The size of the block of memory.</returns>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static UInt32 GetFixedHeapSize()
        {
            //Stub for use by testing framework
            //Exact 0.5MB
            return 524288;
        }

        /// <summary>
        /// Intialises the kernel's fixed heap.
        /// </summary>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
        public static void InitFixedHeap()
        {
            if (!FixedHeapInitialised)
            {
                Heap.Init();
                Heap.AddBlock(Heap.GetFixedHeapPtr(), Heap.GetFixedHeapSize(), 16);
                FixedHeapInitialised = true;
            }
        }


        /// <summary>
        /// Intialises the heap.
        /// </summary>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
        public static void Init()
        {
            fblock = (HeapBlock*)0;
        }
        /// <summary>
        /// Adds a contiguous block of memory to the heap so it can be used for allocating memory to objects.
        /// </summary>
        /// <param name="addr">The address of the start of the block of memory.</param>
        /// <param name="size">The size of the block of memory to add.</param>
        /// <param name="bsize">The size of the chunks to use when allocating memory.</param>
        /// <returns>Returns 1 if the block was added successfully.</returns>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
        public static int AddBlock(UInt32* addr, UInt32 size, UInt32 bsize)
        {
            HeapBlock* b;
            UInt32 bcnt;
            UInt32 x;
            byte* bm;

            b = (HeapBlock*)addr;
            b->size = size - (UInt32)sizeof(HeapBlock);
            b->bsize = bsize;

            b->next = fblock;
            fblock = b;

            bcnt = size / bsize;
            bm = (byte*)&b[1];

            /* clear bitmap */
            for (x = 0; x < bcnt; ++x)
            {
                bm[x] = 0;
            }

            /* reserve room for bitmap */
            bcnt = (bcnt / bsize) * bsize < bcnt ? bcnt / bsize + 1 : bcnt / bsize;
            for (x = 0; x < bcnt; ++x)
            {
                bm[x] = 5;
            }

            b->lfb = bcnt - 1;

            b->used = bcnt;

            return 1;
        }

        /// <summary>
        /// Don't understand what this actually does...anyone care to inform me?
        /// </summary>
        /// <param name="a">Umm...</param>
        /// <param name="b">Umm...</param>
        /// <returns>Umm...the NID I guess... :)</returns>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
        public static byte GetNID(byte a, byte b)
        {
            byte c;
            for (c = (byte)(a + 1); c == b || c == 0; ++c) ;
            return c;
        }

        /// <summary>
        /// Attempts to allocate the specified amount of memory from the heap.
        /// </summary>
        /// <param name="size">The amount of memory to try and allocate.</param>
        /// <returns>A pointer to the start of the allocated memory or a null pointer if not enough 
        /// contiguous memory is available.</returns>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
        public static void* Alloc(UInt32 size)
        {
            return Alloc(size, 1);
        }
        /// <summary>
        /// Attempts to allocate the specified amount of memory from the heap and then zero all of it.
        /// </summary>
        /// <param name="size">The amount of memory to try and allocate.</param>
        /// <returns>A pointer to the start of the allocated memory or a null pointer if not enough 
        /// contiguous memory is available.</returns>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
        public static void* AllocZeroed(UInt32 size)
        {
            return AllocZeroed(size, 1);
        }
        /// <summary>
        /// Attempts to allocate the specified amount of memory from the heap and then zero all of it.
        /// </summary>
        /// <param name="size">The amount of memory to try and allocate.</param>
        /// <param name="boundary">The boundary on which the data must be allocated. 1 = no boundary. Must be power of 2.</param>
        /// <returns>A pointer to the start of the allocated memory or a null pointer if not enough 
        /// contiguous memory is available.</returns>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
        public static void* AllocZeroed(UInt32 size, UInt32 boundary)
        {
            return Utilities.MemoryUtils.ZeroMem(Alloc(size, boundary), size);
        }
        /// <summary>
        /// Attempts to allocate the specified amount of memory from the heap.
        /// </summary>
        /// <param name="size">The amount of memory to try and allocate.</param>
        /// <param name="boundary">The boundary on which the data must be allocated. 1 = no boundary. Must be power of 2.</param>
        /// <returns>A pointer to the start of the allocated memory or a null pointer if not enough 
        /// contiguous memory is available.</returns>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
        public static void* Alloc(UInt32 size, UInt32 boundary)
        {
#if HEAP_TRACE
            BasicConsole.SetTextColour(BasicConsole.warning_colour);
            BasicConsole.WriteLine("Attempt to alloc mem....");
            BasicConsole.SetTextColour(BasicConsole.default_colour);
#endif

            if (PreventAllocation)
            {
                BasicConsole.SetTextColour(BasicConsole.error_colour);
                BasicConsole.WriteLine("Allocation of memory prevented! Reason:");
                BasicConsole.WriteLine(PreventReason);
                BasicConsole.DelayOutput(5);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
                return null;
            }

            HeapBlock* b;
            byte* bm;
            UInt32 bcnt;
            UInt32 x, y, z;
            UInt32 bneed;
            byte nid;

            if(boundary > 1)
            {
                size += (boundary - 1);
            }

            /* iterate blocks */
            for (b = fblock; (UInt32)b != 0; b = b->next)
            {
                /* check if block has enough room */
                if (b->size - (b->used * b->bsize) >= size)
                {

                    bcnt = b->size / b->bsize;
                    bneed = (size / b->bsize) * b->bsize < size ? size / b->bsize + 1 : size / b->bsize;
                    bm = (byte*)&b[1];

                    for (x = (b->lfb + 1 >= bcnt ? 0 : b->lfb + 1); x != b->lfb; ++x)
                    {
                        /* just wrap around */
                        if (x >= bcnt)
                        {
                            x = 0;
                        }

                        if (bm[x] == 0)
                        {
                            /* count free blocks */
                            for (y = 0; bm[x + y] == 0 && y < bneed && (x + y) < bcnt; ++y) ;

                            /* we have enough, now allocate them */
                            if (y == bneed)
                            {
                                /* find ID that does not match left or right */
                                nid = GetNID(bm[x - 1], bm[x + y]);

                                /* allocate by setting id */
                                for (z = 0; z < y; ++z)
                                {
                                    bm[x + z] = nid;
                                }

                                /* optimization */
                                b->lfb = (x + bneed) - 2;

                                /* count used blocks NOT bytes */
                                b->used += y;

                                void* result = (void*)(x * b->bsize + (UInt32*)&b[1]);
                                if (boundary > 1)
                                {
                                    result = (void*)((((UInt32)result) + (boundary - 1)) & ~(boundary - 1));
                                }
                                return result;
                            }

                            /* x will be incremented by one ONCE more in our FOR loop */
                            x += (y - 1);
                            continue;
                        }
                    }
                }
            }

#if HEAP_TRACE
            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine("!!Heap out of memory!!");
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            BasicConsole.DelayOutput(2);
#endif
            return null;
        }
        /// <summary>
        /// Frees the specified memory giving it back to the heap.
        /// </summary>
        /// <param name="ptr">A pointer to the memory to free.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
        public static void Free(void* ptr)
        {
            HeapBlock* b;
            UInt32* ptroff;
            UInt32 bi, x;
            byte* bm;
            byte id;
            UInt32 max;

            for (b = fblock; (UInt32)b != 0; b = b->next)
            {
                if ((UInt32*)ptr > (UInt32*)b && (UInt32*)ptr < (UInt32*)b + b->size)
                {
                    /* found block */
                    ptroff = (UInt32*)((UInt32*)ptr - (UInt32*)&b[1]);  /* get offset to get block */
                    /* block offset in BM */
                    bi = (UInt32)ptroff / b->bsize;
                    /* .. */
                    bm = (byte*)&b[1];
                    /* clear allocation */
                    id = bm[bi];
                    /* oddly.. GCC did not optimize this */
                    max = b->size / b->bsize;
                    for (x = bi; bm[x] == id && x < max; ++x)
                    {
                        bm[x] = 0;
                    }
                    /* update free block count */
                    b->used -= x - bi;
                    return;
                }
            }

            /* this error needs to be raised or reported somehow */
            return;
        }
    }
}
