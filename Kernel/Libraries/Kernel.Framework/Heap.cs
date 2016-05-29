#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
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

//#define HEAP_TRACE
//#define PROCESS_TRACE

using Drivers.Compiler.Attributes;
using Kernel.Framework.Processes;
using Kernel.Framework.Processes.Synchronisation;
using Kernel.Utilities;

namespace Kernel.Framework
{
    //This implementation is based off of Leonard Kevin McGuire Jr's Binary Heap Implementation
    //found at http://wiki.osdev.org/User:Pancakes/BitmapHeapImplementation
    //(www.kmcg3413.net) (kmcg3413@gmail.com)

    /// <summary>
    ///     Represents a block of memory that has been allocated for use by the heap.
    /// </summary>
    public unsafe struct HeapBlock
    {
        /// <summary>
        ///     A pointer to the next heap block.
        /// </summary>
        public HeapBlock* next;

        /// <summary>
        ///     The size of the block of memory allocated.
        /// </summary>
        public uint size;

        /// <summary>
        ///     The amount of memory in the block that has been used.
        /// </summary>
        public uint used;

        /// <summary>
        ///     The size of the chunks to use when allocating memory.
        /// </summary>
        public uint bsize;

        /// <summary>
        ///     Used for optimisation.
        /// </summary>
        public uint lfb;

        public bool expanding;
    }

    /// <summary>
    ///     The kernel heap - currently a very simple implementation.
    /// </summary>
    public static unsafe class Heap
    {
        public static bool PreventAllocation = false;
        public static String PreventReason = "[NONE]";

        public static bool OutputTrace;

        /// <summary>
        ///     A pointer to the most-recently added heap block.
        /// </summary>
        private static HeapBlock* fblock;

        public static String name = "[UNINITIALISED]";

        public static SpinLock AccessLock;
        public static bool AccessLockInitialised;

        /// <summary>
        ///     A pointer to the most-recently added heap block.
        /// </summary>
        public static HeapBlock* FBlock
        {
            [NoDebug] [NoGC] get { return fblock; }
        }

        [NoDebug]
        [NoGC]
        static Heap()
        {
        }

        [NoDebug]
        [NoGC]
        private static void EnterCritical(String caller)
        {
            //BasicConsole.WriteLine("Entering critical section...");
            if (AccessLockInitialised)
            {
                if (AccessLock == null)
                {
                    BasicConsole.WriteLine("HeapAccessLock is initialised but null?!");
                    BasicConsole.DelayOutput(10);
                }
                else
                {
                    if (AccessLock.Locked && OutputTrace)
                    {
                        BasicConsole.SetTextColour(BasicConsole.warning_colour);
                        BasicConsole.WriteLine("Warning: Heap about to try to re-enter spin lock...");
                        BasicConsole.Write("Enter lock caller: ");
                        BasicConsole.WriteLine(caller);
                        BasicConsole.SetTextColour(BasicConsole.default_colour);
                    }

                    AccessLock.Enter();
                }
            }
            //else
            //{
            //    BasicConsole.WriteLine("HeapAccessLock not initialised - ignoring lock conditions.");
            //    BasicConsole.DelayOutput(5);
            //}
        }

        [NoDebug]
        [NoGC]
        private static void ExitCritical()
        {
            //BasicConsole.WriteLine("Exiting critical section...");
            if (AccessLockInitialised)
            {
                if (AccessLock == null)
                {
                    BasicConsole.WriteLine("HeapAccessLock is initialised but null?!");
                    BasicConsole.DelayOutput(10);
                }
                else
                {
                    AccessLock.Exit();
                }
            }
            //else
            //{
            //    BasicConsole.WriteLine("HeapAccessLock not initialised - ignoring lock conditions.");
            //    BasicConsole.DelayOutput(5);
            //}
        }


        /// <summary>
        ///     Calculates the total amount of memory in the heap.
        /// </summary>
        /// <returns>The total amount of memory in the heap.</returns>
        [NoDebug]
        [NoGC]
        public static uint GetTotalMem()
        {
            HeapBlock* cBlock = fblock;
            uint result = 0;
            while (cBlock != null)
            {
                result += cBlock->size;
                cBlock = cBlock->next;
            }
            return result;
        }

        /// <summary>
        ///     Calculates the total amount of used memory in the heap.
        /// </summary>
        /// <returns>The total amount of used memory in the heap.</returns>
        [NoDebug]
        [NoGC]
        public static uint GetTotalUsedMem()
        {
            HeapBlock* cBlock = fblock;
            uint result = 0;
            while (cBlock != null)
            {
                result += GetUsedMem(cBlock);
                cBlock = cBlock->next;
            }
            return result;
        }

        /// <summary>
        ///     Calculates the total amount of free memory in the heap.
        /// </summary>
        /// <returns>The total amount of free memory in the heap.</returns>
        [NoDebug]
        [NoGC]
        public static uint GetTotalFreeMem()
        {
            HeapBlock* cBlock = fblock;
            uint result = 0;
            while (cBlock != null)
            {
                result += GetFreeMem(cBlock);
                cBlock = cBlock->next;
            }
            return result;
        }

        /// <summary>
        ///     Calculates the amount of used memory in the specified block.
        /// </summary>
        /// <param name="aBlock">The block to calculate used mem of.</param>
        /// <returns>The amount of used memory in bytes.</returns>
        [NoDebug]
        [NoGC]
        public static uint GetUsedMem(HeapBlock* aBlock)
        {
            return aBlock->used*aBlock->bsize;
        }

        /// <summary>
        ///     Calculates the amount of free memory in the specified block.
        /// </summary>
        /// <param name="aBlock">The block to calculate free mem of.</param>
        /// <returns>The amount of free memory in bytes.</returns>
        [NoDebug]
        [NoGC]
        public static uint GetFreeMem(HeapBlock* aBlock)
        {
            return aBlock->size - aBlock->used*aBlock->bsize;
        }

        public static void Load(HeapBlock* heapPtr, SpinLock heapLock)
        {
            fblock = heapPtr;
            AccessLock = heapLock;
            AccessLockInitialised = AccessLock != null;
        }

        [NoDebug]
        [NoGC]
        public static int InitBlock(HeapBlock* b, uint size, uint bsize)
        {
            uint bcnt;

            b->size = size - (uint)sizeof(HeapBlock);
            b->bsize = bsize;
            b->expanding = false;

            bcnt = size/bsize;
            byte* bm = (byte*)&b[1];

            /* clear bitmap */
            for (uint x = 0; x < bcnt; ++x)
            {
                bm[x] = 0;
            }

            /* reserve room for bitmap */
            bcnt = bcnt/bsize*bsize < bcnt ? bcnt/bsize + 1 : bcnt/bsize;
            for (uint x = 0; x < bcnt; ++x)
            {
                bm[x] = 5;
            }

            b->lfb = bcnt - 1;

            b->used = bcnt;
            b->next = null;

            return 1;
        }

        /// <summary>
        ///     Adds a contiguous block of memory to the heap so it can be used for allocating memory to objects.
        /// </summary>
        /// <returns>Returns 1 if the block was added successfully.</returns>
        [NoDebug]
        [NoGC]
        public static int AddBlock(HeapBlock* b)
        {
            bool ShouldExitCritical = false;
            if (fblock != null)
            {
                if (!fblock->expanding)
                {
                    EnterCritical("AddBlock");
                    ShouldExitCritical = true;
                }
            }

            b->next = fblock;
            fblock = b;

            if (ShouldExitCritical)
            {
                ExitCritical();
            }

            return 1;
        }

        /// <summary>
        ///     Don't understand what this actually does...anyone care to inform me?
        /// </summary>
        /// <param name="a">Umm...</param>
        /// <param name="b">Umm...</param>
        /// <returns>Umm...the NID I guess... :)</returns>
        [NoDebug]
        [NoGC]
        public static byte GetNID(byte a, byte b)
        {
            byte c;
            for (c = (byte)(a + 1); c == b || c == 0; ++c) ;
            return c;
        }

        /// <summary>
        ///     Attempts to allocate the specified amount of memory from the heap.
        /// </summary>
        /// <param name="size">The amount of memory to try and allocate.</param>
        /// <returns>
        ///     A pointer to the start of the allocated memory or a null pointer if not enough
        ///     contiguous memory is available.
        /// </returns>
        [NoDebug]
        [NoGC]
        public static void* Alloc(uint size, String caller)
        {
            return Alloc(size, 1, caller);
        }

        /// <summary>
        ///     Attempts to allocate the specified amount of memory from the heap and then zero all of it.
        /// </summary>
        /// <param name="size">The amount of memory to try and allocate.</param>
        /// <returns>
        ///     A pointer to the start of the allocated memory or a null pointer if not enough
        ///     contiguous memory is available.
        /// </returns>
        [NoDebug]
        [NoGC]
        public static void* AllocZeroed(uint size, String caller)
        {
            return AllocZeroed(size, 1, caller);
        }

        /// <summary>
        ///     Avoids Page Boundary.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="boundary"></param>
        /// <returns></returns>
        [NoDebug]
        [NoGC]
        public static void* AllocZeroedAPB(uint size, uint boundary, String caller)
        {
            void* result = null;
            void* oldValue = null;
            uint resultAddr;
            do
            {
                oldValue = result;
                result = AllocZeroed(size, boundary, caller);
                resultAddr = (uint)result;
                if (oldValue != null)
                {
                    Free(oldValue);
                }
            } while (resultAddr/0x1000 != (resultAddr + size - 1)/0x1000);

            return result;
        }

        /// <summary>
        ///     Attempts to allocate the specified amount of memory from the heap and then zero all of it.
        /// </summary>
        /// <param name="size">The amount of memory to try and allocate.</param>
        /// <param name="boundary">The boundary on which the data must be allocated. 1 = no boundary. Must be power of 2.</param>
        /// <returns>
        ///     A pointer to the start of the allocated memory or a null pointer if not enough
        ///     contiguous memory is available.
        /// </returns>
        [NoDebug]
        [NoGC]
        public static void* AllocZeroed(uint size, uint boundary, String caller)
        {
            void* result = Alloc(size, boundary, caller);
            if (result == null)
            {
                return null;
            }
            return MemoryUtils.ZeroMem(result, size);
        }

        /// <summary>
        ///     Attempts to allocate the specified amount of memory from the heap.
        /// </summary>
        /// <param name="size">The amount of memory to try and allocate.</param>
        /// <param name="boundary">The boundary on which the data must be allocated. 1 = no boundary. Must be power of 2.</param>
        /// <returns>
        ///     A pointer to the start of the allocated memory or a null pointer if not enough
        ///     contiguous memory is available.
        /// </returns>
        [NoDebug]
        [NoGC]
        public static void* Alloc(uint size, uint boundary, String caller)
        {
#if HEAP_TRACE
            if (OutputTrace)
            {
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine("Attempt to alloc mem....");
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }
#endif

            if (PreventAllocation)
            {
                bool BCPOEnabled = BasicConsole.PrimaryOutputEnabled;
                BasicConsole.PrimaryOutputEnabled = true;
                BasicConsole.SetTextColour(BasicConsole.error_colour);
                BasicConsole.Write("Allocation of memory prevented! Reason: ");
                BasicConsole.WriteLine(PreventReason);
                BasicConsole.Write("    > Caller: ");
                BasicConsole.WriteLine(caller);
                BasicConsole.DelayOutput(5);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
                BasicConsole.PrimaryOutputEnabled = BCPOEnabled;
                return null;
            }

            EnterCritical("Alloc");

            int retry = 1;

            do
            {
                HeapBlock* b = null;
                byte* bm = null;
                uint bcnt = 0;
                uint x, y, z = 0;
                uint bneed = 0;
                byte nid = 0;

                if (boundary > 1)
                {
                    size += boundary - 1;
                }

                /* iterate blocks */
                for (b = fblock; (uint)b != 0; b = b->next)
                {
                    /* check if block has enough room */
                    if (b->size - b->used*b->bsize >= size)
                    {
                        bcnt = b->size/b->bsize;
                        bneed = size/b->bsize*b->bsize < size ? size/b->bsize + 1 : size/b->bsize;
                        bm = (byte*)&b[1];

                        for (x = b->lfb + 1 >= bcnt ? 0 : b->lfb + 1; x != b->lfb; ++x)
                        {
                            /* just wrap around */
                            if (x >= bcnt)
                            {
                                x = 0;
                            }

                            if (bm[x] == 0)
                            {
                                /* count free blocks */
                                for (y = 0; bm[x + y] == 0 && y < bneed && x + y < bcnt; ++y) ;

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
                                    b->lfb = x + bneed - 2;

                                    /* count used blocks NOT bytes */
                                    b->used += y;

                                    void* result = (void*)(x*b->bsize + (uint)&b[1]);
                                    if (boundary > 1)
                                    {
                                        result = (void*)(((uint)result + (boundary - 1)) & ~(boundary - 1));

                                        //#if HEAP_TRACE
                                        //                                      ExitCritical();
                                        //                                      BasicConsole.WriteLine(((Framework.String)"Allocated address ") + (uint)result + " on boundary " + boundary + " for " + caller);
                                        //                                      EnterCritical("Alloc:Boundary condition");
                                        //#endif
                                    }

                                    ExitCritical();
                                    return result;
                                }

                                /* x will be incremented by one ONCE more in our FOR loop */
                                x += y - 1;
                            }
                        }
                    }
                }

                BasicConsole.Write(name);
                BasicConsole.WriteLine(" heap needs to expand!");

                if (!ExpandHeap(false))
                {
                    retry--;
                }

                BasicConsole.WriteLine("Heap expansion complete.");
            } while (retry > 0);

            {
                bool BCPOEnabled = BasicConsole.PrimaryOutputEnabled;
                BasicConsole.PrimaryOutputEnabled = true;
                BasicConsole.Write("Heap (");
                BasicConsole.Write(name);
                BasicConsole.WriteLine(") out of memory!");
                if (fblock == null)
                {
                    BasicConsole.WriteLine(" !! fblock == null");
                }
                else if (GetTotalFreeMem() < size)
                {
                    if (GetTotalMem() == 0)
                    {
                        BasicConsole.WriteLine(" !! Out of free mem because total mem is zero.");
                    }
                    else if (GetTotalMem() <= 8092)
                    {
                        BasicConsole.WriteLine(" !! Out of free mem because total mem is <= 8092 bytes.");
                    }
                    else
                    {
                        BasicConsole.WriteLine(" !! Genuinely out of memory.");
                    }
                }
                BasicConsole.PrimaryOutputEnabled = BCPOEnabled;
            }

            ExitCritical();

            return null;
        }

        /// <summary>
        ///     Frees the specified memory giving it back to the heap.
        /// </summary>
        /// <param name="ptr">A pointer to the memory to free.</param>
        [NoDebug]
        [NoGC]
        public static void Free(void* ptr)
        {
            EnterCritical("Free");

            HeapBlock* b;
            uint ptroff;
            uint bi, x;
            byte* bm;
            byte id;
            uint max;

            for (b = fblock; (uint)b != 0; b = b->next)
            {
                if ((uint)ptr > (uint)b && (uint)ptr < (uint)b + b->size)
                {
                    /* found block */
                    ptroff = (uint)ptr - (uint)&b[1]; /* get offset to get block */
                    /* block offset in BM */
                    bi = ptroff/b->bsize;
                    /* .. */
                    bm = (byte*)&b[1];
                    /* clear allocation */
                    id = bm[bi];
                    /* oddly.. HeapC did not optimize this */
                    max = b->size/b->bsize;
                    for (x = bi; bm[x] == id && x < max; ++x)
                    {
                        bm[x] = 0;
                    }
                    /* update free block count */
                    b->used -= x - bi;

                    ExitCritical();
                    return;
                }
            }

            /* this error needs to be raised or reported somehow */
            ExitCritical();
        }

        /// <summary>
        ///     Intialises the heap.
        /// </summary>
        [NoDebug]
        [NoGC]
        public static void InitForKernel()
        {
            fblock = null;
        }

        public static void InitForProcess()
        {
            // The next block will set references to `null`,
            //  these increments ensure those assignments won't cause any objects to hit zero ref count
            GC.IncrementRefCount(GC.State);
            GC.IncrementRefCount(AccessLock);

            GC.State = null;
            AccessLockInitialised = false;
            AccessLock = null;
            fblock = null;
            OutputTrace = false;
            GC.UseCurrentState = true;
            ExceptionMethods.UseCurrentState = true;

#if PROCESS_TRACE
            BasicConsole.WriteLine(" > Initialising process heap...");
#endif
            // Initial heap creation
            ExpandHeap(false);

#if PROCESS_TRACE
            BasicConsole.WriteLine(" >> Creating heap lock...");
#endif
            AccessLock = new SpinLock();

#if PROCESS_TRACE
            BasicConsole.WriteLine(" >> Setting heap lock initialised...");
#endif
            AccessLockInitialised = true;

#if PROCESS_TRACE
            BasicConsole.WriteLine(" >> Creating new GC state...");
#endif
            GCState TheGCState = new GCState();

            if ((uint)TheGCState.CleanupList == 0xFFFFFFFF)
            {
                BasicConsole.WriteLine(" !!! PANIC !!! ");
                BasicConsole.WriteLine(" GC.state.CleanupList is 0xFFFFFFFF NOT null!");
                BasicConsole.WriteLine(" !-!-!-!-!-!-! ");
            }
#if PROCESS_TRACE
            BasicConsole.WriteLine(" >> Creating new GC lock...");
#endif
            TheGCState.AccessLock = new SpinLock();
#if PROCESS_TRACE
            BasicConsole.WriteLine(" >> Setting GC lock initialised...");
#endif
            TheGCState.AccessLockInitialised = true;
#if PROCESS_TRACE
            BasicConsole.WriteLine(" >> Setting GC state...");
#endif
            GC.State = TheGCState;
#if PROCESS_TRACE
            BasicConsole.WriteLine(" >> Done.");
#endif
        }

        public static bool ExpandHeap(bool print)
        {
            if (fblock == null)
            {
                // Creates initial heap of 40KiB
                if (!DoExpandHeap(0xA000))
                {
                    BasicConsole.WriteLine("New heap NOT created (via expand)!");
                    return false;
                }
                return true;
                //else
                //{
                //    BasicConsole.WriteLine("New heap created (via expand)!");
                //    if (print)
                //    {
                //        BasicConsole.Write("New heap size: ");
                //        BasicConsole.Write(GetTotalMem() / 1024);
                //        BasicConsole.WriteLine("KiB");
                //    }
                //}
            }
            if (!fblock->expanding)
            {
                HeapBlock* oldFBlock = fblock;
                oldFBlock->expanding = true;
                // Expand by 1MiB
                if (!DoExpandHeap(0x100000))
                {
                    BasicConsole.WriteLine("Couldn't expand heap!");
                    return false;
                }
                //else
                //{
                //    BasicConsole.WriteLine("Heap expanded successfully.");
                //    if (print)
                //    {
                //        BasicConsole.Write("New heap size: ");
                //        BasicConsole.Write(GetTotalMem() / 1024);
                //        BasicConsole.WriteLine("KiB");
                //    }
                //}
                oldFBlock->expanding = false;
                return true;
            }
            return false;
        }

        private static bool DoExpandHeap(uint Size)
        {
            uint NumPages = (Size + 4095)/4096;
            uint FinalSize = NumPages*4096;
            uint StartAddress;
            SystemCallResults MapPagesResult = SystemCalls.RequestPages(NumPages, out StartAddress);
            if (MapPagesResult != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("Request for pages (to expand heap) failed!");
                return false;
            }
            HeapBlock* NewBlockPtr = (HeapBlock*)StartAddress;
            InitBlock(NewBlockPtr, FinalSize, 32);
            AddBlock(NewBlockPtr);
            return true;
        }
    }
}