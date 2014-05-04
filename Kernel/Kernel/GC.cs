using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel
{
    /// <summary>
    /// The garbage collector.
    /// </summary>
    [Compiler.PluggedClass]
    public static class GC
    {
        /// <summary>
        /// The total number of objects currently allocated by the GC.
        /// </summary>
        public static int NumObjs = 0;
        /// <summary>
        /// Whether the GC has been initialised yet or not.
        /// Used to prevent the GC running before it has been initialised properly.
        /// </summary>
        private static bool GCInitialised = false;
        /// <summary>
        /// Whether the GC is currently executing. Used to prevent the GC calling itself (or ending up in loops with
        /// called methods re-calling the GC!)
        /// </summary>
        private static bool InsideGC = false;

        //TODO - GC needs an object list to scan down quickly to find missed 0-ref count objects
        //TODO - GC needs an object reference tree to do a thorough scan to find reference loops

        /// <summary>
        /// Intialises the GC.
        /// </summary>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static void Init()
        {
            Heap.InitFixedHeap();
            GCInitialised = true;
        }

        /// <summary>
        /// Creates a new object of specified type (but does not call the default constructor).
        /// </summary>
        /// <param name="theType">The type of object to create.</param>
        /// <returns>A pointer to the new object in memory.</returns>
        [Compiler.NewObjMethod]
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static unsafe void* NewObj(FOS_System.Type theType)
        {
            if(!GCInitialised || InsideGC)
            {
                return null;
            }

            InsideGC = true;

            //Alloc space for GC header that prefixes object data
            //Alloc space for new object
            
            uint totalSize = theType.Size;
            totalSize += (uint)sizeof(GCHeader);

            GCHeader* newObjPtr = (GCHeader*)Heap.Alloc(totalSize);
            
            if((UInt32)newObjPtr == 0)
            {
                return null;
            }

            NumObjs++;

            //Initialise the GCHeader
            //Initialise the object _Type field
            SetSignature(newObjPtr);
            newObjPtr->RefCount = 1;
            ((UInt32*)newObjPtr)[sizeof(GCHeader) / 4] = (UInt32)GetHandle(theType);
            
            byte* newObjBytePtr = (byte*)newObjPtr;
            for (int i = sizeof(GCHeader) + 4; i < totalSize; i++)
            {
                newObjBytePtr[i] = 0;
            }

            //Move past GCHeader
            newObjBytePtr = (byte*)(newObjBytePtr + sizeof(GCHeader));

            InsideGC = false;

            return newObjBytePtr;
        }

        /// <summary>
        /// Gets a handle for the specified object - basically, a round-about way of casting an object to a pointer.
        /// </summary>
        /// <remarks>
        /// All the plug does is to set the return value to the argument value!
        /// </remarks>
        /// <param name="anObj">The object to get a handle of.</param>
        /// <returns>The pointer to the object.</returns>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\GC\GetHandle")]
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static unsafe void* GetHandle(object anObj)
        {
            return null;
        }

        /// <summary>
        /// Increments the ref count of a GC managed object.
        /// </summary>
        /// <remarks>
        /// Uses underlying increment ref count method.
        /// </remarks>
        /// <param name="anObj">The object to increment the ref count of.</param>
        [Compiler.IncrementRefCountMethod]
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static unsafe void IncrementRefCount(FOS_System.Object anObj)
        {
            if (!GCInitialised || InsideGC || anObj == null)
            {
                return;
            }

            InsideGC = true;

            byte* objPtr = (byte*)GetHandle(anObj);
            _IncrementRefCount(objPtr);

            InsideGC = false;
        }
        /// <summary>
        /// Underlying method that increments the ref count of a GC managed object.
        /// </summary>
        /// <remarks>
        /// This method checks that the pointer is not a null pointer and also checks for the GC signature 
        /// so string literals and the like don't accidentally get treated as normal GC managed strings.
        /// </remarks>
        /// <param name="objPtr">Pointer to the object to increment the ref count of.</param>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static unsafe void _IncrementRefCount(byte* objPtr)
        {
            objPtr -= sizeof(GCHeader);
            GCHeader* gcHeaderPtr = (GCHeader*)objPtr;
            if (CheckSignature(gcHeaderPtr))
            {
                gcHeaderPtr->RefCount++;
            }
        }

        /// <summary>
        /// Decrements the ref count of a GC managed object.
        /// </summary>
        /// <remarks>
        /// This method checks that the pointer is not a null pointer and also checks for the GC signature 
        /// so string literals and the like don't accidentally get treated as normal GC managed strings.
        /// </remarks>
        /// <param name="anObj">The object to decrement the ref count of.</param>
        [Compiler.DecrementRefCountMethod]
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static unsafe void DecrementRefCount(FOS_System.Object anObj)
        {
            if (!GCInitialised || InsideGC || anObj == null)
            {
                return;
            }

            InsideGC = true;

            byte* objPtr = (byte*)GetHandle(anObj);
            _DecrementRefCount(objPtr);

            InsideGC = false;
        }
        /// <summary>
        /// Underlying method that decrements the ref count of a GC managed object.
        /// </summary>
        /// <remarks>
        /// This method checks that the pointer is not a null pointer and also checks for the GC signature 
        /// so string literals and the like don't accidentally get treated as normal GC managed strings.
        /// </remarks>
        /// <param name="objPtr">A pointer to the object to decrement the ref count of.</param>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static unsafe void _DecrementRefCount(byte* objPtr)
        {
            objPtr = (byte*)(objPtr - sizeof(GCHeader));
            GCHeader* gcHeaderPtr = (GCHeader*)objPtr;
            if (CheckSignature(gcHeaderPtr))
            {
                gcHeaderPtr->RefCount--;

                if (gcHeaderPtr->RefCount <= 0)
                {
                    Heap.Free(objPtr);

                    NumObjs--;
                }
            }
        }

        /// <summary>
        /// Checks the GC header is valid by checking for the GC signature.
        /// </summary>
        /// <param name="headerPtr">A pointer to the header to check.</param>
        /// <returns>True if the signature is found and is correct.</returns>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static unsafe bool CheckSignature(GCHeader* headerPtr)
        {
            bool OK = headerPtr->Sig1 == 0x5C0EADE2U;
            OK = OK && headerPtr->Sig2 == 0x5C0EADE2U;
            OK = OK && headerPtr->Checksum == 0xB81D5BC4U;
            return OK;
        }
        /// <summary>
        /// Sets the GC signature in the specified GC header.
        /// </summary>
        /// <param name="headerPtr">A pointer to the header to set the signature in.</param>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static unsafe void SetSignature(GCHeader* headerPtr)
        {
            headerPtr->Sig1 = 0x5C0EADE2U;
            headerPtr->Sig2 = 0x5C0EADE2U;
            headerPtr->Checksum = 0xB81D5BC4U;
        }
    }
    
    /// <summary>
    /// Represents the GC header that is put in memory in front of every object so the GC can manage the object.
    /// </summary>
    public struct GCHeader
    {
        /// <summary>
        /// The first 4 bytes of the GC signature.
        /// </summary>
        public uint Sig1;
        /// <summary>
        /// The second 4 bytes of the GC signature.
        /// </summary>
        public uint Sig2;
        /// <summary>
        /// A checksum value.
        /// </summary>
        public UInt32 Checksum;

        /// <summary>
        /// The current reference count for the object associated with this header.
        /// </summary>
        public uint RefCount;
    }
}
