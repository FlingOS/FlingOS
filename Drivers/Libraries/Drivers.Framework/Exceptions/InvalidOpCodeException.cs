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
    ///     Represents an invalid op-code exception.
    ///     Usually thrown by the hardware interrupt.
    /// </summary>
    public class InvalidOpCodeException : Exception
    {
        /// <summary>
        ///     Sets the message to "Attempted to execute an invalid op code."
        /// </summary>
        public InvalidOpCodeException()
            : base("Attempted to execute an invalid op code.")
        {
        }
    }
}