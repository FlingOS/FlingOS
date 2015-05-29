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
    
using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Kernel;
using Kernel.FOS_System;

namespace Testing._Kernel
{
    /// <summary>
    /// Test class for Kernel.Heap class.
    /// </summary>
    [TestClass]
    public unsafe class HeapTest
    {
        /// <summary>
        /// List of IntPtrs to memory allocated during tests that needs cleaning up
        /// </summary>
        static List<IntPtr> MemoryToCleanup = new List<IntPtr>();

        /// <summary>
        /// Initialises the test class before each test.
        /// </summary>
        [TestInitialize]
        public void TestInit()
        {
            Heap.Init();
        }
        /// <summary>
        /// Cleans up the test class after each test.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            foreach (IntPtr memBlock in MemoryToCleanup)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(memBlock);
            }
            MemoryToCleanup.Clear();
        }

        /// <summary>
        /// Tests the GetNID() method.
        /// </summary>
        [TestMethod]
        [TestCategory("Memory")]
        public void GetNID_Test()
        {
            byte a = 0;
            byte b = 5;
            byte c = Heap.GetNID(a, b);
            Assert.AreNotEqual(0, c, "NID returned invalid ID. c == 0");
            Assert.AreNotEqual(a, c, "NID returned invalid ID. c == a");
            Assert.AreNotEqual(b, c, "NID returned invalid ID. b == a");
        }

        /// <summary>
        /// Tests the AddBlock() method.
        /// </summary>
        [TestMethod]
        [TestCategory("Memory")]
        public void AddBlock_Test()
        {
            //Alloc 0.5MB
            for (int i = 0; i < 100; i++)
            {
                uint memSize = 524288;
                uint bSize = 16;
                IntPtr newMemBlock = System.Runtime.InteropServices.Marshal.AllocHGlobal((int)memSize);
                MemoryToCleanup.Add(newMemBlock);

                Random rnd = new Random();
                for (int j = 0; j < memSize / 4; j++)
                {
                    ((int*)newMemBlock)[j] = rnd.Next();
                }

                HeapBlock* fblockBefore = Heap.FBlock;
                int ok = Heap.AddBlock((uint*)newMemBlock, memSize, bSize);
                Assert.AreEqual(1, ok, "Add block returned error code!");
                HeapBlock* fblockAfter = Heap.FBlock;

                Assert.AreEqual((int)fblockBefore, (int)fblockAfter->next, "New heap block not initialised correctly! \"next\" not set to last heap block.");
                Assert.AreEqual(bSize, fblockAfter->bsize, "New heap block not initialised correctly! \"bsize\" not set correctly.");
                Assert.AreEqual(memSize - sizeof(HeapBlock), fblockAfter->size, "New heap block not initialised correctly! \"size\" not set to correctly.");
                
                uint bcnt = memSize / bSize;
                bcnt = (bcnt / bSize) * bSize < bcnt ? bcnt / bSize + 1 : bcnt / bSize;
                Assert.AreEqual(bcnt, fblockAfter->used, "New heap block not initialised correctly! \"used\" not set to correctly.");
                Assert.AreEqual(bcnt - 1, fblockAfter->lfb, "New heap block not initialised correctly! \"lfb\" not set to correctly.");

                for (byte* j = (byte*)(&fblockAfter[1]); j < (((byte*)(&fblockAfter[1])) + bcnt); j++)
                {
                    Assert.AreEqual(5, j[0], "Memory not initialised properly!");
                }
                for (byte* j = ((byte*)(&fblockAfter[1])) + fblockAfter->used; j < (((byte*)fblockAfter) + fblockAfter->size); j++)
                {
                    Assert.IsNotNull(j[0], "Memory inaccessible!");
                }
            }
        }

        /// <summary>
        /// Tests the Alloc() method.
        /// </summary>
        [TestMethod]
        [TestCategory("Memory")]
        public void Alloc_Test()
        {
            //Repeat the test many times
            //To begin with, attempt to allocate all the memory in one go
            //Then double the number of allocations each time
            for (uint i = 1; i <= 262144; i *= 16)
            {
                Heap.Init();

                uint memSize = 524288;
                uint bSize = 16;

                IntPtr newMemBlock = System.Runtime.InteropServices.Marshal.AllocHGlobal((int)memSize);
                MemoryToCleanup.Add(newMemBlock);

                Heap.AddBlock((uint*)newMemBlock, memSize, bSize);

                memSize -= (uint)sizeof(HeapBlock);
                uint bcnt = (memSize / Heap.FBlock->bsize) - Heap.FBlock->used;
                memSize = bcnt * Heap.FBlock->bsize;

                //Attempt to allocate all of the memory
                uint sizeToAlloc = memSize / i;
                uint blocksPerAlloc = (sizeToAlloc / bSize) * bSize < sizeToAlloc ? (sizeToAlloc / bSize) + 1 : sizeToAlloc / bSize;
                for (uint j = 0; j + blocksPerAlloc < bcnt; j += blocksPerAlloc)
                {
                    void* allocatedPtr = Heap.Alloc(sizeToAlloc);
                    Assert.IsTrue(allocatedPtr != null, "Size " + sizeToAlloc + " allocation resulted in null pointer! i=" + i + "; j=" + j);
                }
            }
        }

        /// <summary>
        /// Tests the Free() method.
        /// </summary>
        [TestMethod]
        [TestCategory("Memory")]
        public void Free_Test()
        {
            uint memSize = 524288;
            uint bSize = 16;

            Heap.Init();

            IntPtr newMemBlock = System.Runtime.InteropServices.Marshal.AllocHGlobal((int)memSize);
            MemoryToCleanup.Add(newMemBlock);

            Heap.AddBlock((uint*)newMemBlock, memSize, bSize);

            memSize -= (uint)sizeof(HeapBlock);
            uint bcnt = (memSize / Heap.FBlock->bsize) - Heap.FBlock->used;
            memSize = bcnt * Heap.FBlock->bsize;

            uint minUsedBlocks = Heap.FBlock->used;

            //Repeat the test many times
            //To begin with, attempt to allocate all the memory in one go then free
            //Then double the number of allocations and frees each time
            for (uint i = 1; i <= 262144; i *= 16)
            {
                //Attempt to allocate all of the memory
                uint sizeToAlloc = memSize / i;
                uint blocksPerAlloc = (sizeToAlloc / bSize) * bSize < sizeToAlloc ? (sizeToAlloc / bSize) + 1 : sizeToAlloc / bSize;
                List<IntPtr> memPtrs = new List<IntPtr>();
                for (uint j = 0; j + blocksPerAlloc < bcnt; j += blocksPerAlloc)
                {
                    void* allocatedPtr = Heap.Alloc(sizeToAlloc);
                    Assert.IsTrue(allocatedPtr != null, "Size " + sizeToAlloc + " allocation resulted in null pointer! i=" + i + "; j=" + j);
                    memPtrs.Add((IntPtr)allocatedPtr);
                }
                for (int j = 0; j < memPtrs.Count; j++)
                {
                    //Attempt to free
                    Heap.Free(((void*)memPtrs[j]));
                    //Re-allocate to prove it is freed! If not freed, the first attempt to re-allocate
                    //  will fail as all memory should have been allocated above.
                    void* allocatedPtr = Heap.Alloc(sizeToAlloc);
                    Assert.IsTrue(allocatedPtr != null, "Free'd memory unavailable for re-allocation = free failed. i=" + i + "; j=" + j);
                    //Re-free the memory
                    Heap.Free(allocatedPtr);
                }
                Assert.AreEqual(minUsedBlocks, Heap.FBlock->used, "Memory not returned to minimum used amount after all should be free'd. i=" + i);
            }
        }
    }
}
