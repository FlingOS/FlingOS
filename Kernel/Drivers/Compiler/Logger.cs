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
