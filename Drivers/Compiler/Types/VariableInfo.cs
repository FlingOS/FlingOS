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

namespace Drivers.Compiler.Types
{
    /// <summary>
    ///     Container for information about a variable loaded from a method in a library being compiled.
    /// </summary>
    public class VariableInfo
    {
        /// <summary>
        ///     The offset of the variable from EBP in bytes.
        /// </summary>
        public int Offset;

        /// <summary>
        ///     The position (as an index) of the variable.
        /// </summary>
        public int Position;

        /// <summary>
        ///     Whether to skip the GC Cleanup step (inject by ILPreprocessor) for the variable or not.
        /// </summary>
        public bool SkipCleanup = false;

        /// <summary>
        ///     The type info for the variable.
        /// </summary>
        public TypeInfo TheTypeInfo;

        /// <summary>
        ///     The type of the variable.
        /// </summary>
        public Type UnderlyingType;

        /// <summary>
        ///     Gets a human-readable representation of the variable.
        /// </summary>
        /// <remarks>
        ///     Uses the variable's full name.
        /// </remarks>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return UnderlyingType.FullName;
        }
    }
}