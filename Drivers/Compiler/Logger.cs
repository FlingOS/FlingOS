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

namespace Drivers.Compiler
{
    /// <summary>
    ///     Used to log output of the compiler.
    /// </summary>
    /// <remarks>
    ///     Output can be directed either to MSBuild or to the Console or to any other output, depending on what is required.
    /// </remarks>
    public static class Logger
    {
        /// <summary>
        ///     Delegate for handling a log error event.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="file">The file associated with the message.</param>
        /// <param name="lineNumber">The line number within the associated file.</param>
        /// <param name="message">The message to log.</param>
        public delegate void LogErrorEventHandler(string errorCode, string file, int lineNumber, string message);

        /// <summary>
        ///     Delegate for handling a log message event.
        /// </summary>
        /// <param name="file">The file associated with the message.</param>
        /// <param name="lineNumber">The line number within the associated file.</param>
        /// <param name="message">The message to log.</param>
        public delegate void LogMessageEventHandler(string file, int lineNumber, string message);

        /// <summary>
        ///     Delegate for handling a log warning event.
        /// </summary>
        /// <param name="warningCode">The warning code.</param>
        /// <param name="file">The file associated with the message.</param>
        /// <param name="lineNumber">The line number within the associated file.</param>
        /// <param name="message">The message to log.</param>
        public delegate void LogWarningEventHandler(string warningCode, string file, int lineNumber, string message);

        /// <summary>
        ///     The handler to use for logging a message. Must not be left as null.
        /// </summary>
        public static event LogMessageEventHandler OnLogMessage;

        /// <summary>
        ///     The handler to use for logging a warning. Must not be left as null.
        /// </summary>
        public static event LogWarningEventHandler OnLogWarning;

        /// <summary>
        ///     The handler to use for logging an error. Must not be left as null.
        /// </summary>
        public static event LogErrorEventHandler OnLogError;

        /// <summary>
        ///     Logs a message.
        /// </summary>
        /// <param name="file">The file associated with the message.</param>
        /// <param name="lineNumber">The line number within the associated file.</param>
        /// <param name="message">The message to log.</param>
        public static void LogMessage(string file, int lineNumber, string message)
        {
            OnLogMessage.Invoke(file, lineNumber, message);
        }

        /// <summary>
        ///     Logs a warning.
        /// </summary>
        /// <param name="warningCode">The warning code.</param>
        /// <param name="file">The file associated with the message.</param>
        /// <param name="lineNumber">The line number within the associated file.</param>
        /// <param name="message">The message to log.</param>
        public static void LogWarning(string warningCode, string file, int lineNumber, string message)
        {
            OnLogWarning.Invoke(warningCode, file, lineNumber, message);
        }

        /// <summary>
        ///     Logs an error.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="file">The file associated with the message.</param>
        /// <param name="lineNumber">The line number within the associated file.</param>
        /// <param name="message">The message to log.</param>
        public static void LogError(string errorCode, string file, int lineNumber, string message)
        {
            OnLogError.Invoke(errorCode, file, lineNumber, message);
        }
    }
}