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
    ///     An exception object.
    /// </summary>
    public class Exception : Object
    {
        public Exception InnerException;

        public uint InstructionAddress = 0;
        protected String message;

        /// <summary>
        ///     The exception message.
        /// </summary>
        public String Message
        {
            get
            {
                if (InnerException != null)
                {
                    return message + "\nInner exception:\n" + InnerException.Message + "\nInstruction address: " +
                           InstructionAddress;
                }
                return message + "\nInstruction address: " + InstructionAddress;
            }
            set { message = value; }
        }

        /// <summary>
        ///     Creates a new, empty exception.
        /// </summary>
        public Exception()
        {
        }

        /// <summary>
        ///     Creates a new exception with specified message.
        /// </summary>
        /// <param name="aMessage">The exception message.</param>
        public Exception(String aMessage)
        {
            Message = aMessage;
        }
    }
}