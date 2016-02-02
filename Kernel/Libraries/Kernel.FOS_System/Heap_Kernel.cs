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

namespace Kernel.FOS_System
{
    public static unsafe class Heap_Kernel
    {
        static Heap_Kernel()
        {
            InitFixedHeap();
            GC.Init();
        }

        /// <summary>
        /// Whether the kernel's fixed heap has been initialised or not.
        /// </summary>
        private static bool FixedHeapInitialised = false;

        /// <summary>
        /// Gets a pointer to the block of memory to allocate to the kernel's fixed heap.
        /// </summary>
        /// <returns>The pointer to the block of memory.</returns>
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = @"ASM\Heap\GetFixedHeapPtr")]
        [Drivers.Compiler.Attributes.SequencePriority(Priority = long.MinValue + 101)]
        public static UInt32* GetFixedHeapPtr()
        {
            return null;
        }
        /// <summary>
        /// Gets the size of the block of memory to allocate to the kernel's fixed heap.
        /// </summary>
        /// <returns>The size of the block of memory.</returns>
        [Drivers.Compiler.Attributes.PluggedMethod(ASMFilePath = null)]
        public static UInt32 GetFixedHeapSize()
        {
            //Stub for use by testing framework
            //Exact 0.5MB
            return 524288;
        }

        /// <summary>
        /// Intialises the kernel's fixed heap.
        /// </summary>
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static void InitFixedHeap()
        {
            if (!FixedHeapInitialised)
            {
                Heap.InitForKernel();

                HeapBlock* heapPtr = (HeapBlock*)GetFixedHeapPtr();
                Heap.InitBlock(heapPtr, GetFixedHeapSize(), 32);
                Heap.AddBlock(heapPtr);

                FixedHeapInitialised = true;
            }
        }

    }
}
