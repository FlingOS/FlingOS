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

namespace Drivers.Framework
{
    /// <summary>
    /// Replacement class for methods, properties and fields usually found on standard System.String type.
    /// Also contains utility methods for low-level string manipulation.
    /// </summary>
    [Drivers.Compiler.Attributes.PluggedClass]
    [Drivers.Compiler.Attributes.StringClass]
    public class String : Object
    {
        /* If you add more fields here, remember to update the compiler and all the ASM files that depend on the string
           class structure ( i.e. do all the hard work! ;) )
         */

        /// <summary>
        /// The size of the fields in an string object that come before the actual string data.
        /// </summary>
        public const uint FieldsBytesSize = 8;

        /// <summary>
        /// The length of the string.
        /// </summary>
        public int length;

        /*   ----------- DO NOT CREATE A CONSTRUCTOR FOR THIS CLASS - IT WILL NEVER BE CALLED IF YOU DO ----------- */

        [Drivers.Compiler.Attributes.NoGC]
        [Drivers.Compiler.Attributes.NoDebug]
        public String()
        {
        }
    }
}
