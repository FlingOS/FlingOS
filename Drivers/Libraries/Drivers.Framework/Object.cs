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

namespace Drivers.Framework
{
    /// <summary>
    ///     All objects (that are GC managed) should derive from this type.
    /// </summary>
    public class Object : ObjectWithType
    {
    }

    /// <summary>
    ///     Represents an object with a type. You should use the <see cref="Drivers.Framework.Object" /> class.
    /// </summary>
    /// <remarks>
    ///     We implement it like this so that _Type field is always the first
    ///     field in memory of all objects.
    /// </remarks>
    public class ObjectWithType
    {
        /// <summary>
        ///     The underlying, specific type of the object specified when it was created.
        /// </summary>
        public Type _Type;
    }
}