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

using System.Collections.Generic;

namespace Drivers.Compiler.ASM
{
    /// <summary>
    ///     Represents a library (or executable) as a set of ASM blocks.
    /// </summary>
    public class ASMLibrary
    {
        /// <summary>
        ///     The ASM blocks which make up the library.
        /// </summary>
        public List<ASMBlock> ASMBlocks = new List<ASMBlock>();

        /// <summary>
        ///     Whether the ASM Preprocess step has already be run for the library or not.
        ///     Prevents the ASM Compiler from executing the Preprocessing step more than
        ///     once for a given library.
        /// </summary>
        public bool ASMPreprocessed = false;

        /// <summary>
        ///     Whether the ASM Process step has already be run for the library or not.
        ///     Prevents the ASM Compiler from executing the Processing step more than
        ///     once for a given library.
        /// </summary>
        public bool ASMProcessed = false;
    }
}