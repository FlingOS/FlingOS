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

namespace Kernel.Framework.Exceptions
{
    /// <summary>
    ///     Represents a divide by zero exception.
    ///     Usually thrown by hardware interrupt 0 when code attempts to divide a number (always integer?) by 0.
    /// </summary>
    public class DivideByZeroException : Exception
    {
        /// <summary>
        ///     Sets the message to "Attempt to divide by zero invalid."
        /// </summary>
        public DivideByZeroException()
            : base("Attempt to divide by zero invalid.")
        {
        }

        /// <summary>
        ///     Sets the message to "Attempt to divide by zero invalid. Address: [address]"
        /// </summary>
        /// <param name="address">The address of the instruction which caused the exception.</param>
        public DivideByZeroException(uint address)
            : base((String)"Attempt to divide by zero invalid. Address: " + address)
        {
        }
    }
}