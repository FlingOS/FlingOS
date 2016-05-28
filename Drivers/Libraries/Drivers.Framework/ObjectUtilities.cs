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

using Drivers.Compiler.Attributes;

namespace Drivers.Utilities
{
    /// <summary>
    ///     Utility methods for object manipulation.
    /// </summary>
    public static class ObjectUtilities
    {
        /// <summary>
        ///     Gets a handle for the specified object - basically, a round-about way of casting an object to a pointer.
        /// </summary>
        /// <remarks>
        ///     All the plug does is to set the return value to the argument value!
        /// </remarks>
        /// <param name="anObj">The object to get a handle of.</param>
        /// <returns>The pointer to the object.</returns>
        [PluggedMethod(ASMFilePath = @"ASM\ObjectUtilities\GetHandle")]
        [NoDebug]
        [NoGC]
        public static unsafe void* GetHandle(object anObj)
        {
            return null;
        }

        /// <summary>
        ///     Gets an object for the specified pointer - basically, a round-about way of casting a pointer to an object.
        /// </summary>
        /// <remarks>
        ///     All the plug does is to set the return value to the argument value!
        /// </remarks>
        /// <param name="anObjPtr">The pointer to get an object of.</param>
        /// <returns>The object the pointer points to.</returns>
        [PluggedMethod(ASMFilePath = @"ASM\ObjectUtilities\GetObject")]
        [NoDebug]
        [NoGC]
        public static unsafe object GetObject(void* anObjPtr)
        {
            return null;
        }
    }
}