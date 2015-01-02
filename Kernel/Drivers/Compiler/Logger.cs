#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler
{
    public static class Logger
    {
        public delegate void LogMessageEventHandler(string file, int lineNumber, string message);
        public delegate void LogWarningEventHandler(string warningCode, string file, int lineNumber, string message);
        public delegate void LogErrorEventHandler(string errorCode, string file, int lineNumber, string message);

        public static event LogMessageEventHandler OnLogMessage;
        public static event LogWarningEventHandler OnLogWarning;
        public static event LogErrorEventHandler OnLogError;

        public static void LogMessage(string file, int lineNumber, string message)
        {
            OnLogMessage.Invoke(file, lineNumber, message);
        }
        public static void LogWarning(string warningCode, string file, int lineNumber, string message)
        {
            OnLogWarning.Invoke(warningCode, file, lineNumber, message);
        }
        public static void LogError(string errorCode, string file, int lineNumber, string message)
        {
            OnLogError.Invoke(errorCode, file, lineNumber, message);
        }
    }
}
