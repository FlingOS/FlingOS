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
    ///     Represents a page fault exception.
    ///     Usually thrown by the hardware interrupt.
    /// </summary>
    public class PageFaultException : Exception
    {
        /// <summary>
        ///     The (virtual) address that caused the exception.
        /// </summary>
        public uint address;

        /// <summary>
        ///     The error code passed with the exception.
        /// </summary>
        public uint errorCode;

        /// <summary>
        ///     Sets the message to "Page fault"
        /// </summary>
        /// <param name="anErrorCode">The error code associated with the page fault.</param>
        /// <param name="anAddress">The address which caused the fault.</param>
        public PageFaultException(uint anErrorCode, uint anAddress)
            : base("Page fault")
        {
            errorCode = anErrorCode;
            address = anAddress;
        }
    }
}