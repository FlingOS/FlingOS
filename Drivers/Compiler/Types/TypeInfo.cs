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
using System.Collections.Generic;

namespace Drivers.Compiler.Types
{
    /// <summary>
    ///     Container for information about a type loaded from an IL library being compiled.
    /// </summary>
    public class TypeInfo
    {
        /// <summary>
        ///     List of information about fields in the type.
        /// </summary>
        public List<FieldInfo> FieldInfos = new List<FieldInfo>();

        /// <summary>
        ///     ID generator for the methods' IDs in this type.
        /// </summary>
        public int MethodIDGenerator = 0;

        /// <summary>
        ///     List of information about methods in the type.
        /// </summary>
        public List<MethodInfo> MethodInfos = new List<MethodInfo>();

        /// <summary>
        ///     Whether the type has been processed or not (excludes fields).
        /// </summary>
        public bool Processed = false;

        /// <summary>
        ///     Whether the type's fields have been processed or not.
        /// </summary>
        public bool ProcessedFields = false;

        /// <summary>
        ///     The underlying System.Type obtained from the library's Assembly.
        /// </summary>
        public Type UnderlyingType;

        /// <summary>
        ///     Whether the type is a value type or not.
        /// </summary>
        /// <value>Gets the value of the underlying info's IsValueType property.</value>
        public bool IsValueType
        {
            get { return UnderlyingType.IsValueType; }
        }

        /// <summary>
        ///     Whether the type is a pointer type or not.
        /// </summary>
        /// <value>Gets the value of the underlying info's IsPointer property.</value>
        public bool IsPointer
        {
            get { return UnderlyingType.IsPointer; }
        }

        /// <summary>
        ///     The size of an object of this type when it is represented on the stack.
        /// </summary>
        /// <value>Gets/sets the value of an implicitly defined field.</value>
        public int SizeOnStackInBytes { get; set; }

        /// <summary>
        ///     The size of an object of this type when it is allocated on the heap.
        /// </summary>
        /// <value>Gets/sets the value of an implicitly defined field.</value>
        public int SizeOnHeapInBytes { get; set; }

        /// <summary>
        ///     Whether objects of this type are managed by the GC or not.
        /// </summary>
        /// <value>Gets/sets the value of an implicitly defined field.</value>
        public bool IsGCManaged { get; set; }

        /// <summary>
        ///     The ID of this type (can be used as a label).
        /// </summary>
        /// <value>Generates the value from the underlying info's FullName and filters it to make it usable as an ASM label.</value>
        public string ID
        {
            get { return "type_" + Utilities.FilterIdentifierForInvalidChars(UnderlyingType.FullName); }
        }

        /// <summary>
        ///     Gets a human-readable representation of the type.
        /// </summary>
        /// <remarks>
        ///     Uses the type's assembly qualified name.
        /// </remarks>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return UnderlyingType.AssemblyQualifiedName;
        }
    }
}