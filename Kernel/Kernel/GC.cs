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
            BasicConsole.WriteLine("Initialising GC...");

            Heap.InitFixedHeap();
            GCInitialised = true;

            BasicConsole.WriteLine("GC initialised.");
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
            SetSignature(newObjPtr);
            newObjPtr->RefCount = 1;
            //Initialise the object _Type field
            FOS_System.ObjectWithType newObj = (FOS_System.ObjectWithType)Utilities.ObjectUtilities.GetObject(newObjPtr + 1);
            newObj._Type = theType;
            
            byte* newObjBytePtr = (byte*)newObjPtr;
            for (int i = sizeof(GCHeader) + 4/*For _Type field*/; i < totalSize; i++)
            {
                newObjBytePtr[i] = 0;
            }

            //Move past GCHeader
            newObjBytePtr = (byte*)(newObjBytePtr + sizeof(GCHeader));

            InsideGC = false;

            return newObjBytePtr;
        }

        /// <summary>
        /// Creates a new array with specified element type (but does not call the default constructor).
        /// </summary>
        /// <remarks>"length" param placed first so that calling NewArr method is simple
        /// with regards to pushing params onto the stack.</remarks>
        /// <param name="theType">The type of element in the array to create.</param>
        /// <returns>A pointer to the new array in memory.</returns>
        [Compiler.NewArrMethod]
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static unsafe void* NewArr(int length, FOS_System.Type elemType)
        {
            int arrayObjSize = 8;

            if (!GCInitialised || InsideGC)
            {
                return null;
            }

            if (length < 0)
            {
                Exceptions.Throw_OverflowException();
            }

            InsideGC = true;

            //Alloc space for GC header that prefixes object data
            //Alloc space for new array object
            //Alloc space for new array elems

            uint totalSize = ((FOS_System.Type)typeof(FOS_System.Array)).Size;
            totalSize += elemType.StackSize * (uint)length;
            totalSize += (uint)sizeof(GCHeader);

            GCHeader* newObjPtr = (GCHeader*)Heap.Alloc(totalSize);

            if ((UInt32)newObjPtr == 0)
            {
                return null;
            }

            NumObjs++;

            //Initialise the GCHeader
            SetSignature(newObjPtr);
            newObjPtr->RefCount = 1;

            FOS_System.Array newArr = (FOS_System.Array)Utilities.ObjectUtilities.GetObject(newObjPtr + 1);
            newArr._Type = (FOS_System.Type)typeof(FOS_System.Array);
            newArr.length = length;
            newArr.elemType = elemType;
            
            byte* newObjBytePtr = (byte*)newObjPtr;
            for (int i = sizeof(GCHeader) + arrayObjSize + 4/*For _Type field*/; i < totalSize; i++)
            {
                newObjBytePtr[i] = 0;
            }

            //Move past GCHeader
            newObjBytePtr = (byte*)(newObjBytePtr + sizeof(GCHeader));

            InsideGC = false;
            
            return newObjBytePtr;
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

            byte* objPtr = (byte*)Utilities.ObjectUtilities.GetHandle(anObj);
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
            DecrementRefCount(anObj, false);
        }
        /// <summary>
        /// Decrements the ref count of a GC managed object.
        /// </summary>
        /// <remarks>
        /// This method checks that the pointer is not a null pointer and also checks for the GC signature 
        /// so string literals and the like don't accidentally get treated as normal GC managed strings.
        /// </remarks>
        /// <param name="anObj">The object to decrement the ref count of.</param>
        /// <param name="overrideInside">Whether to ignore the InsideGC test or not.</param>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static unsafe void DecrementRefCount(FOS_System.Object anObj, bool overrideInside)
        {
            if (!GCInitialised || (InsideGC && !overrideInside) || anObj == null)
            {
                return;
            }

            if (!overrideInside)
            {
                InsideGC = true;
            }

            byte* objPtr = (byte*)Utilities.ObjectUtilities.GetHandle(anObj);
            _DecrementRefCount(objPtr);

            if (!overrideInside)
            {
                InsideGC = false;
            }
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
            GCHeader* gcHeaderPtr = (GCHeader*)(objPtr - sizeof(GCHeader));
            if (CheckSignature(gcHeaderPtr))
            {
                gcHeaderPtr->RefCount--;

                FOS_System.Object obj = (FOS_System.Object)Utilities.ObjectUtilities.GetObject(objPtr);
                if (obj._Type == (FOS_System.Type)typeof(FOS_System.Array))
                {
                    //Decrement ref count of elements
                    FOS_System.Array arr = (FOS_System.Array)obj;
                    if (!arr.elemType.IsValueType)
                    {
                        FOS_System.Object[] objArr = (FOS_System.Object[])Utilities.ObjectUtilities.GetObject(objPtr);
                        for (int i = 0; i < arr.length; i++)
                        {
                            DecrementRefCount(objArr[i], true);
                        }
                    }
                }

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
