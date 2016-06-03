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

namespace Kernel.Framework
{
    /// <summary>
    ///     An exception object.
    /// </summary>
    public class Exception : Object
    {
        /// <summary>
        ///     The inner exception (may be null).
        /// </summary>
        /// <remarks>
        ///     An inner exception refers to an exception that was unhandled at the 
        ///     point when the current exception was thrown.
        /// </remarks>
        public Exception InnerException;

        /// <summary>
        ///     The address of the instruction which caused the exception.
        /// </summary>
        public uint InstructionAddress = 0;
        /// <summary>
        ///     The message for the exception not including any inner exceptions.
        /// </summary>
        private String _Message;

        /// <summary>
        ///     The exception message including messages from any inner exceptions.
        /// </summary>
        public String Message
        {
            get
            {
                if (InnerException != null)
                {
                    return _Message + "\nInner exception:\n" + InnerException.Message + "\nInstruction address: " +
                           InstructionAddress;
                }
                return _Message + "\nInstruction address: " + InstructionAddress;
            }
            private set { _Message = value; }
        }
        
        /// <summary>
        ///     Creates a new exception with specified message.
        /// </summary>
        /// <param name="Message">The exception message.</param>
        public Exception(String Message)
        {
            this.Message = Message;
        }
    }
}