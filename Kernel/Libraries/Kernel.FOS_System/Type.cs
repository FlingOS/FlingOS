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
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System
{
    /// <summary>
    /// Represents an object type specification. Please see remarks before using.
    /// </summary>
    /// <remarks>
    /// The inheritance of the System.Type class is simply so the typeof operator can be used.
    /// Do not use any of the members, properties or methods which are inherited from the base
    /// class!
    /// 
    /// The layout of this class must match exactly what the compiler
    ///  outputs. Do not reorder fields in this class.
    /// </remarks>
    [Compiler.TypeClass]
    public unsafe abstract class Type : System.Type
    {
        /// <summary>
        /// The size of the object in memory.
        /// </summary>
        public uint Size;
        /// <summary>
        /// The type ID.
        /// </summary>
        public uint Id;
        /// <summary>
        /// The size of the type when on the stack or in an array.
        /// </summary>
        public uint StackSize;
        /// <summary>
        /// Whether the type is a value type or not.
        /// </summary>
        public new bool IsValueType;

        /// <summary>
        /// A pointer to the start of the method table.
        /// </summary>
        public MethodInfo* MethodTablePtr;
        
        /// <summary>
        /// Whether the type is a pointer type or not.
        /// </summary>
        public new bool IsPointer;

        /// <summary>
        /// The base type from which this type inherits. If this field is null,
        /// it indicates there is no base type.
        /// </summary>
        public Type TheBaseType;

        /// <summary>
        /// A pointer to the start of the field table.
        /// </summary>
        public FieldInfo* FieldTablePtr;

        /// <summary>
        /// The human readable type signature string.
        /// </summary>
        public FOS_System.String Signature;

        /// <summary>
        /// The compiler-assigned type ID string.
        /// </summary>
        public FOS_System.String IdString;

        /// <summary>
        /// Compares two types by ID to see if they represent the same type.
        /// </summary>
        /// <param name="x">The first type to compare with the second.</param>
        /// <param name="y">The second type to compare with the first.</param>
        /// <returns>True if they are equal, otherwise false.</returns>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static bool operator ==(Type x, Type y)
        {
            //Prevent recursive calls to this "==" implicit method!
            if (Utilities.ObjectUtilities.GetHandle(x) == null ||
                Utilities.ObjectUtilities.GetHandle(y) == null)
            {
                if (Utilities.ObjectUtilities.GetHandle(x) == null &&
                    Utilities.ObjectUtilities.GetHandle(y) == null)
                {
                    return true;
                }
                return false;
            }
            return x.Id == y.Id;
        }
        /// <summary>
        /// Compares two types by ID to see if they represent the different types.
        /// </summary>
        /// <param name="x">The first type to compare with the second.</param>
        /// <param name="y">The second type to compare with the first.</param>
        /// <returns>True if they are not equal, otherwise false.</returns>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static bool operator !=(Type x, Type y)
        {
            return !(x == y);
        }
    }

    /// <summary>
    /// Represents the information in the methods type table.
    /// </summary>
    /// <remarks>
    /// The layout of this struct must match exactly what the compiler
    /// outputs. Do not reorder fields in this struct.
    /// </remarks>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MethodInfo
    {
        /// <summary>
        /// The ID of the method or 0. 0 indicates that the method pointer is actually a pointer to
        /// the parent method table.
        /// </summary>
        public uint MethodID;

        /// <summary>
        /// A pointer to the method or, if MethodID is 0, a pointer to the parent method table. 
        /// If this pointer is 0, it indicates there is no parent type (i.e. 0 is not a valid pointer).
        /// </summary>
        public byte* MethodPtr;
    }

    /// <summary>
    /// Represents the information in the fields type table.
    /// </summary>
    /// <remarks>
    /// The layout of this struct must match exactly what the compiler
    /// outputs. Do not reorder fields in this struct.
    /// </remarks>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FieldInfo
    {
        /// <summary>
        /// The offset from the start of the object memory to the field.
        /// </summary>
        public uint Offset;
        /// <summary>
        /// The size of the field in the parent object's memory or, if size is 0, 
        /// this indicates that FieldType is pointer to the parent type's fields table.
        /// </summary>
        public uint Size;
        /// <summary>
        /// A pointer to the type of the field or, if size is 0, a pointer to the
        /// parent type's fields table. If this pointer is 0, it indicates there is no
        /// parent type (i.e. 0 is not a valid pointer).
        /// </summary>
        public byte* FieldType;
    }
}
