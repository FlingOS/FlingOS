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

using System.Runtime.InteropServices;
using Drivers.Compiler.Attributes;

namespace FlingOops
{
    /// <summary>
    ///     Represents an object type specification. Please see remarks before using.
    /// </summary>
    /// <remarks>
    ///     The inheritance of the System.Type class is simply so the typeof operator can be used.
    ///     Do not use any of the members, properties or methods which are inherited from the base
    ///     class!
    /// </remarks>
    [TypeClass]
    public abstract unsafe class Type : System.Type
    {
        /// <summary>
        ///     A pointer to the start of the field table.
        /// </summary>
        public FieldInfo* FieldTablePtr;

        /// <summary>
        ///     The type ID.
        /// </summary>
        public uint Id;

        /// <summary>
        ///     The compiler-assigned type ID string. This is a hash string which is not the same as the Id.
        /// </summary>
        public String IdString;

        /// <summary>
        ///     Whether the type is a pointer type or not.
        /// </summary>
        public new bool IsPointer;

        /// <summary>
        ///     Whether the type is a value type or not.
        /// </summary>
        public new bool IsValueType;

        /// <summary>
        ///     A pointer to the start of the method table.
        /// </summary>
        public MethodInfo* MethodTablePtr;

        /// <summary>
        ///     The human readable type signature.
        /// </summary>
        public String Signature;

        /// <summary>
        ///     The size of the object in memory.
        /// </summary>
        public uint Size;

        /// <summary>
        ///     The size of the type when on the stack or in an array.
        /// </summary>
        public uint StackSize;

        /// <summary>
        ///     The base type from which this type inherits. If this field is null,
        ///     it indicates there is no base type.
        /// </summary>
        public Type TheBaseType;

        [NoGC]
        [NoDebug]
        public Type()
        {
        }

        /// <summary>
        ///     Compares two types by ID to see if they represent the same type.
        /// </summary>
        /// <param name="x">The first type to compare with the second.</param>
        /// <param name="y">The second type to compare with the first.</param>
        /// <returns>True if they are equal, otherwise false.</returns>
        [NoDebug]
        [NoGC]
        public static bool operator ==(Type x, Type y)
        {
            //Prevent recursive calls to this "==" implicit method!
            //if (Utilities.ObjectUtilities.GetHandle(x) == null ||
            //    Utilities.ObjectUtilities.GetHandle(y) == null)
            //{
            //    if (Utilities.ObjectUtilities.GetHandle(x) == null &&
            //        Utilities.ObjectUtilities.GetHandle(y) == null)
            //    {
            //        return true;
            //    }
            //    return false;
            //}
            return x.Id == y.Id;
        }

        /// <summary>
        ///     Compares two types by ID to see if they represent the different types.
        /// </summary>
        /// <param name="x">The first type to compare with the second.</param>
        /// <param name="y">The second type to compare with the first.</param>
        /// <returns>True if they are not equal, otherwise false.</returns>
        [NoDebug]
        [NoGC]
        public static bool operator !=(Type x, Type y)
        {
            return !(x == y);
        }
    }

    /// <summary>
    ///     Represents the information in the methods type table.
    /// </summary>
    [MethodInfoStruct]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MethodInfo
    {
        /// <summary>
        ///     The ID of the method or 0. 0 indicates that the method pointer is actually a pointer to
        ///     the parent method table.
        /// </summary>
        public uint MethodID;

        /// <summary>
        ///     A pointer to the method or, if MethodID is 0, a pointer to the parent method table.
        ///     If this pointer is 0, it indicates there is no parent type (i.e. 0 is not a valid pointer).
        /// </summary>
        public byte* MethodPtr;
    }

    /// <summary>
    ///     Represents the information in the fields type table.
    /// </summary>
    [FieldInfoStruct]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FieldInfo
    {
        /// <summary>
        ///     The offset from the start of the object memory to the field.
        /// </summary>
        public uint Offset;

        /// <summary>
        ///     The size of the field in the parent object's memory or, if size is 0,
        ///     this indicates that FieldType is pointer to the parent type's fields table.
        /// </summary>
        public uint Size;

        /// <summary>
        ///     A pointer to the type of the field or, if size is 0, a pointer to the
        ///     parent type's fields table. If this pointer is 0, it indicates there is no
        ///     parent type (i.e. 0 is not a valid pointer).
        /// </summary>
        public byte* FieldType;
    }
}