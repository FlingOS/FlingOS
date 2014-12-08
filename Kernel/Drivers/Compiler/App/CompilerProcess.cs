using System;

namespace Drivers.Compiler.App
{
    public class CompilerProcess
    {
        public enum ErrorCode : int
        {
            NoError = 0
        }

        static int Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Gray;

            int result = (int)Execute(
                Logger_OnLogMessage,
                Logger_OnLogWarning,
                Logger_OnLogError);

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            return result;
        }
        public static ErrorCode Execute(
            Logger.LogMessageEventHandler messageHandler, 
            Logger.LogWarningEventHandler warningHandler,
            Logger.LogErrorEventHandler   errorHandler)
        {
            ErrorCode result = ErrorCode.NoError;

            Logger.OnLogMessage += messageHandler;
            Logger.OnLogWarning += warningHandler;
            Logger.OnLogError += errorHandler;

            DateTime startTime = DateTime.Now;
            Logger.LogMessage("", 0, "Driver compiler started  @ " + startTime.ToLongTimeString());



            DateTime endTime = DateTime.Now;
            Logger.LogMessage("", 0, "Driver compiler finished @ " + endTime.ToLongTimeString());
            Logger.LogMessage("", 0, "            Compile time : " + (endTime - startTime).ToString());
            Logger.LogMessage("", 0, "              Error code : " + System.Enum.GetName(typeof(ErrorCode), result));
            
            return result;
        }

        private static void Logger_OnLogError(string errorCode, string file, int lineNumber, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error : " + errorCode + ": " + message + " in " + file + ":" + lineNumber);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        private static void Logger_OnLogWarning(string warningCode, string file, int lineNumber, string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Warning : " + warningCode + ": " + message + " in " + file + ":" + lineNumber);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        private static void Logger_OnLogMessage(string file, int lineNumber, string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message + " in " + file + ":" + lineNumber);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
