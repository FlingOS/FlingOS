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
using Drivers.Compiler.Attributes;

namespace Drivers.Compiler.Types
{
    /// <summary>
    ///     Container for information about a field loaded from a type in a library being compiled.
    /// </summary>
    public class FieldInfo
    {
        private string group;

        /// <summary>
        ///     The underlying System.Reflection.FieldInfo obtained from the library's Assembly.
        /// </summary>
        public System.Reflection.FieldInfo UnderlyingInfo;

        /// <summary>
        ///     The type of the field.
        /// </summary>
        /// <value>Gets the value of underlying info's FieldType property.</value>
        public Type FieldType
        {
            get { return UnderlyingInfo.FieldType; }
        }

        /// <summary>
        ///     Whether the field is static or not.
        /// </summary>
        /// <value>Gets the value of underlying info's IsStatic property.</value>
        public bool IsStatic
        {
            get { return UnderlyingInfo.IsStatic; }
        }

        public string Group
        {
            get
            {
                if (group == null)
                {
                    object[] groupAttrs = UnderlyingInfo.GetCustomAttributes(typeof(GroupAttribute), false);
                    if (groupAttrs.Length > 0)
                    {
                        GroupAttribute priorAttr = (GroupAttribute)groupAttrs[0];
                        group = priorAttr.Name;
                    }
                    else
                    {
                        group = "default";
                    }
                }
                return group;
            }
        }

        /// <summary>
        ///     The offset to the beginning of the field from the start of the type, in bytes.
        /// </summary>
        /// <value>Gets/sets an implicitly defined field.</value>
        public int OffsetInBytes { get; set; }

        /// <summary>
        ///     Generates a unique ID for the field (which can also be used as a label in assembly code).
        /// </summary>
        /// <value>Generates the ID from the field information and filters it to make it valid for use as an ASM label.</value>
        public string ID
        {
            get
            {
                if (IsStatic)
                {
                    return "staticfield_" +
                           Utilities.FilterIdentifierForInvalidChars(UnderlyingInfo.FieldType.FullName + "-" +
                                                                     UnderlyingInfo.DeclaringType.FullName + "." +
                                                                     UnderlyingInfo.Name);
                }
                return "field_" +
                       Utilities.FilterIdentifierForInvalidChars(UnderlyingInfo.FieldType.FullName + "-" +
                                                                 UnderlyingInfo.DeclaringType.FullName + "." +
                                                                 UnderlyingInfo.Name);
            }
        }

        /// <summary>
        ///     Gets the name of the field.
        /// </summary>
        /// <value>Gets the value of underlying info's Name property.</value>
        public string Name
        {
            get { return UnderlyingInfo.Name; }
        }

        /// <summary>
        ///     Gets a human-readable representation of the field.
        /// </summary>
        /// <remarks>
        ///     Uses the field's name.
        /// </remarks>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return UnderlyingInfo.Name;
        }
    }
}