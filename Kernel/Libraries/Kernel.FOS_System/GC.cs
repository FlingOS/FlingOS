#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
#define GC_TRACE
#undef GC_TRACE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System
{
    /// <summary>
    /// The garbage collector.
    /// </summary>
    /// <remarks>
    /// Make sure all methods that the GC calls are marked with [Compiler.NoGC] (including
    /// get-set property methods! Apply the attribute to the get/set keywords not the property
    /// declaration (/name).
    /// </remarks>
    [Compiler.PluggedClass]
    public static unsafe class GC
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
        public static bool InsideGC = false;

        /// <summary>
        /// The number of strings currently allocated on the heap.
        /// </summary>
        public static int NumStrings = 0;

        /// <summary>
        /// The linked-list of objects to clean up.
        /// </summary>
        private static ObjectToCleanup* CleanupList;

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
        public static void* NewObj(FOS_System.Type theType)
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

            GCHeader* newObjPtr = (GCHeader*)Heap.AllocZeroed(totalSize);
            
            if((UInt32)newObjPtr == 0)
            {
                InsideGC = false;

                return null;
            }

            NumObjs++;

            //Initialise the GCHeader
            SetSignature(newObjPtr);
            newObjPtr->RefCount = 1;
            //Initialise the object _Type field
            FOS_System.ObjectWithType newObj = (FOS_System.ObjectWithType)Utilities.ObjectUtilities.GetObject(newObjPtr + 1);
            newObj._Type = theType;
            
            //Move past GCHeader
            byte* newObjBytePtr = (byte*)(newObjPtr + 1);

            InsideGC = false;

            return newObjBytePtr;
        }

        /// <summary>
        /// Creates a new array with specified element type (but does not call the default constructor).
        /// </summary>
        /// <remarks>"length" param placed first so that calling NewArr method is simple
        /// with regards to pushing params onto the stack.</remarks>
        /// <param name="length">The length of the array to create.</param>
        /// <param name="elemType">The type of element in the array to create.</param>
        /// <returns>A pointer to the new array in memory.</returns>
        [Compiler.NewArrMethod]
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static void* NewArr(int length, FOS_System.Type elemType)
        {
            if (!GCInitialised || InsideGC)
            {
                return null;
            }

            if (length < 0)
            {
                ExceptionMethods.Throw_OverflowException();
            }

            InsideGC = true;

            //Alloc space for GC header that prefixes object data
            //Alloc space for new array object
            //Alloc space for new array elems

            uint totalSize = ((FOS_System.Type)typeof(FOS_System.Array)).Size;
            if (elemType.IsValueType)
            {
                totalSize += elemType.Size * (uint)length;
            }
            else
            {
                totalSize += elemType.StackSize * (uint)length;
            }
            totalSize += (uint)sizeof(GCHeader);

            GCHeader* newObjPtr = (GCHeader*)Heap.AllocZeroed(totalSize);

            if ((UInt32)newObjPtr == 0)
            {
                InsideGC = false;

                ExceptionMethods.Throw(new FOS_System.Exception("Out of memory!"));
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
            
            //Move past GCHeader
            byte* newObjBytePtr = (byte*)(newObjPtr + 1);

            InsideGC = false;
            
            return newObjBytePtr;
        }

        /// <summary>
        /// DO NOT CALL DIRECTLY. Use FOS_System.String.New
        /// Creates a new string with specified length (but does not call the default constructor).
        /// </summary>
        /// <param name="length">The length of the string to create.</param>
        /// <returns>A pointer to the new string in memory.</returns>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static void* NewString(int length)
        {
            if (!GCInitialised || InsideGC)
            {
                return null;
            }

            if (length < 0)
            {
                ExceptionMethods.Throw_OverflowException();
            }

            InsideGC = true;

            //Alloc space for GC header that prefixes object data
            //Alloc space for new string object
            //Alloc space for new string chars

            uint totalSize = ((FOS_System.Type)typeof(FOS_System.String)).Size;
            totalSize += /*char size in bytes*/2 * (uint)length;
            totalSize += (uint)sizeof(GCHeader);

            GCHeader* newObjPtr = (GCHeader*)Heap.AllocZeroed(totalSize);

            if ((UInt32)newObjPtr == 0)
            {
                InsideGC = false;

                ExceptionMethods.Throw(new FOS_System.Exception("Out of memory!"));
                return null;
            }

            NumObjs++;
            NumStrings++;

            //Initialise the GCHeader
            SetSignature(newObjPtr);
            //RefCount to 0 initially because of FOS_System.String.New should be used
            //      - In theory, New should be called, creates new string and passes it back to caller
            //        Caller is then required to store the string in a variable resulting in inc.
            //        ref count so ref count = 1 in only stored location. 
            //        Caller is not allowed to just "discard" (i.e. use Pop IL op or C# that generates
            //        Pop IL op) so ref count will always at some point be incremented and later
            //        decremented by managed code. OR the variable will stay in a static var until
            //        the OS exits...

            newObjPtr->RefCount = 0;

            FOS_System.String newStr = (FOS_System.String)Utilities.ObjectUtilities.GetObject(newObjPtr + 1);
            newStr._Type = (FOS_System.Type)typeof(FOS_System.String);
            newStr.length = length;
            
            //Move past GCHeader
            byte* newObjBytePtr = (byte*)(newObjPtr + 1);

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
        public static void IncrementRefCount(FOS_System.Object anObj)
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
        public static void _IncrementRefCount(byte* objPtr)
        {
            objPtr -= sizeof(GCHeader);
            GCHeader* gcHeaderPtr = (GCHeader*)objPtr;
            if (CheckSignature(gcHeaderPtr))
            {
                gcHeaderPtr->RefCount++;

                if (gcHeaderPtr->RefCount > 0)
                {
                    RemoveObjectToCleanup(gcHeaderPtr);
                }
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
        public static void DecrementRefCount(FOS_System.Object anObj)
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
        public static void DecrementRefCount(FOS_System.Object anObj, bool overrideInside)
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
        public static void _DecrementRefCount(byte* objPtr)
        {
            GCHeader* gcHeaderPtr = (GCHeader*)(objPtr - sizeof(GCHeader));
            if (CheckSignature(gcHeaderPtr))
            {
                gcHeaderPtr->RefCount--;

                //If the ref count goes below 0 then there was a circular reference somewhere.
                //  In actuality we don't care we can just only do cleanup when the ref count is
                //  exactly 0.
                if (gcHeaderPtr->RefCount == 0)
                {
#if GC_TRACE
                    BasicConsole.WriteLine("Cleaned up object.");
#endif

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
                    //Cleanup fields
                    FieldInfo* FieldInfoPtr = obj._Type.FieldTablePtr;
                    //Loop through all fields. The if-block at the end handles moving to parent
                    //  fields. 
                    while (FieldInfoPtr != null)
                    {
                        FOS_System.Type fieldType = (FOS_System.Type)Utilities.ObjectUtilities.GetObject(FieldInfoPtr->FieldType);
                        if (!fieldType.IsValueType && 
                            !fieldType.IsPointer)
                        {
                            byte* fieldPtr = objPtr + FieldInfoPtr->Offset;
                            FOS_System.Object theFieldObj = (FOS_System.Object)Utilities.ObjectUtilities.GetObject(fieldPtr);
                            DecrementRefCount(theFieldObj, true);

#if GC_TRACE
                            BasicConsole.WriteLine("Cleaned up field.");
#endif
                        }

                        FieldInfoPtr++;

                        if (FieldInfoPtr->Size == 0)
                        {
                            FieldInfoPtr = (FieldInfo*)FieldInfoPtr->FieldType;
                        }
                    }
                    

                    AddObjectToCleanup(gcHeaderPtr, objPtr);
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
        public static void SetSignature(GCHeader* headerPtr)
        {
            headerPtr->Sig1 = 0x5C0EADE2U;
            headerPtr->Sig2 = 0x5C0EADE2U;
            headerPtr->Checksum = 0xB81D5BC4U;
        }

        /// <summary>
        /// Scans the CleanupList to free objects from memory.
        /// </summary>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static void Cleanup()
        {
            if (!GCInitialised || InsideGC)
            {
                return;
            }

            InsideGC = true;

#if GC_TRACE
            int startNumObjs = NumObjs;
            int startNumStrings = NumStrings;
#endif

            ObjectToCleanup* currObjToCleanupPtr = CleanupList;
            ObjectToCleanup* prevObjToCleanupPtr = null;
            while (currObjToCleanupPtr != null)
            {
                GCHeader* objHeaderPtr = currObjToCleanupPtr->objHeaderPtr;
                void* objPtr = currObjToCleanupPtr->objPtr;
                if(objHeaderPtr->RefCount <= 0)
                {
                    FOS_System.Object obj = (FOS_System.Object)Utilities.ObjectUtilities.GetObject(objPtr);
                    if (obj._Type == (FOS_System.Type)typeof(FOS_System.String))
                    {
                        NumStrings--;
                    }

                    Heap.Free(objPtr);

                    NumObjs--;
                }

                prevObjToCleanupPtr = currObjToCleanupPtr;
                currObjToCleanupPtr = currObjToCleanupPtr->prevPtr;
                RemoveObjectToCleanup(prevObjToCleanupPtr);
            }

            InsideGC = false;
            
#if GC_TRACE
            PrintCleanupData(startNumObjs, startNumStrings);
#endif
        }
        /// <summary>
        /// Outputs, via the basic console, how much memory was cleaned up.
        /// </summary>
        /// <param name="startNumObjs">The number of objects before the cleanup.</param>
        /// <param name="startNumStrings">The number of strings before the cleanup.</param>
        private static void PrintCleanupData(int startNumObjs, int startNumStrings)
        {
            int numObjsFreed = startNumObjs - NumObjs;
            int numStringsFreed = startNumStrings - NumStrings;
            BasicConsole.SetTextColour(BasicConsole.warning_colour);
            BasicConsole.WriteLine(((FOS_System.String)"Freed objects: ") + numObjsFreed);
            BasicConsole.WriteLine(((FOS_System.String)"Freed strings: ") + numStringsFreed);
            BasicConsole.WriteLine(((FOS_System.String)"Used memory  : ") + (Heap.FBlock->used * Heap.FBlock->bsize) + " / " + Heap.FBlock->size);
            BasicConsole.DelayOutput(2);
            BasicConsole.SetTextColour(BasicConsole.default_colour);
        }

        /// <summary>
        /// Adds an object to the cleanup list.
        /// </summary>
        /// <param name="objHeaderPtr">A pointer to the object's header.</param>
        /// <param name="objPtr">A pointer to the object.</param>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        private static void AddObjectToCleanup(GCHeader* objHeaderPtr, void* objPtr)
        {
            ObjectToCleanup* newObjToCleanupPtr = (ObjectToCleanup*)Heap.Alloc((uint)sizeof(ObjectToCleanup));
            newObjToCleanupPtr->objHeaderPtr = objHeaderPtr;
            newObjToCleanupPtr->objPtr = objPtr;

            newObjToCleanupPtr->prevPtr = CleanupList;
            CleanupList->nextPtr = newObjToCleanupPtr;

            CleanupList = newObjToCleanupPtr;
        }
        /// <summary>
        /// Removes an object from the cleanup list.
        /// </summary>
        /// <param name="objHeaderPtr">A pointer to the object's header.</param>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        private static void RemoveObjectToCleanup(GCHeader* objHeaderPtr)
        {
            ObjectToCleanup* currObjToCleanupPtr = CleanupList;
            while (currObjToCleanupPtr != null)
            {
                if (currObjToCleanupPtr->objHeaderPtr == objHeaderPtr)
                {
                    RemoveObjectToCleanup(currObjToCleanupPtr);
                    return;
                }
                currObjToCleanupPtr = currObjToCleanupPtr->prevPtr;
            }
        }
        /// <summary>
        /// Removes an object from the cleanup list.
        /// </summary>
        /// <param name="objToCleanupPtr">A pointer to the cleanup-list element.</param>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        private static void RemoveObjectToCleanup(ObjectToCleanup* objToCleanupPtr)
        {
            ObjectToCleanup* prevPtr = objToCleanupPtr->prevPtr;
            ObjectToCleanup* nextPtr = objToCleanupPtr->nextPtr;
            prevPtr->nextPtr = nextPtr;
            nextPtr->prevPtr = prevPtr;

            if(CleanupList == objToCleanupPtr)
            {
                CleanupList = prevPtr;
            }
            
            Heap.Free(objToCleanupPtr);
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
        public int RefCount;
    }
    /// <summary>
    /// Represents an object to be garbage collected (i.e. freed from memory).
    /// </summary>
    public unsafe struct ObjectToCleanup
    {
        /// <summary>
        /// The pointer to the object.
        /// </summary>
        public void* objPtr;
        /// <summary>
        /// The pointer to the object's header.
        /// </summary>
        public GCHeader* objHeaderPtr;
        /// <summary>
        /// A pointer to the previous item in the cleanup list.
        /// </summary>
        public ObjectToCleanup* prevPtr;
        /// <summary>
        /// A pointer to the next item in the cleanup list.
        /// </summary>
        public ObjectToCleanup* nextPtr;
    }
}
