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

namespace Drivers.Framework.Exceptions
{
    /// <summary>
    ///     Represents an index out of range exception.
    /// </summary>
    public class IndexOutOfRangeException : Exception
    {
        /// <summary>
        ///     Sets the message to "Index out of range exception. Index: 0xXXXXXXXX, Range: 0xXXXXXXXX"
        /// </summary>
        public IndexOutOfRangeException(int index, int range)
            : base("Index out of range exception. Index: " + (String)index + ", Range: " + range)
        {
        }

        /// <summary>
        ///     Sets the message to "Index out of range exception. Index: 0xXXXXXXXX, Range: 0xXXXXXXXX"
        /// </summary>
        public IndexOutOfRangeException(uint index, uint range)
            : base("Index out of range exception. Index: " + (String)index + ", Range: " + range)
        {
        }

        /// <summary>
        ///     Sets the message to "Index out of range exception. Index: 0xXXXXXXXX, Range: 0xXXXXXXXX"
        /// </summary>
        public IndexOutOfRangeException(long index, long range)
            : base("Index out of range exception. Index: " + (String)index + ", Range: " + range)
        {
        }

        /// <summary>
        ///     Sets the message to "Index out of range exception. Index: 0xXXXXXXXX, Range: 0xXXXXXXXX"
        /// </summary>
        public IndexOutOfRangeException(ulong index, ulong range)
            : base("Index out of range exception. Index: " + (String)index + ", Range: " + range)
        {
        }
    }
}