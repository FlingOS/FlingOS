using System;

namespace Drivers.Compiler.App
{
    public class CompilerProcess
    {
        public enum ErrorCode : int
        {
            NoError = 0
        }
        
        public static string LibraryPath
        {
            get;
            set;
        }
        public static string OutputPath
        {
            get;
            set;
        }
        public static string ToolsPath
        {
            get;
            set;
        }

        static int Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Gray;

            LibraryPath = args[0];
            OutputPath = args[1];
            ToolsPath = args[2];

            Options.BuildMode = (args[3] == "Debug" ? Options.BuildModes.Debug : Options.BuildModes.Release);
            Options.TargetArchitecture = args[4];

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
            Logger.LogMessage("", 0, "Library path             = \"" + LibraryPath + "\"");
            Logger.LogMessage("", 0, "Output path              = \"" + OutputPath + "\"");
            Logger.LogMessage("", 0, "Tools path               = \"" + ToolsPath + "\"");
            Logger.LogMessage("", 0, "Target architecture      = \"" + Options.TargetArchitecture + "\"");
            Logger.LogMessage("", 0, "Build mode               = "   + Enum.GetName(typeof(Options.BuildModes), Options.BuildMode));

            // IL Library       - In a list of libraries returned to the higher-level control app (this app)
            //                    from Library Loader
            //  : Type Info         - In a list of types in the IL Library
            //      : Field Info        - In a list of static and non-static fields in the Type Info
            //      : Method Info       - In a list of methods in the Type Info
            //          : Variable Info     - In a list of variables in the Method Info
            //  : IL Block          - In a list of blocks in the IL Library and held against the origin Method Info
            //      : IL Op             - In a list of ops in the IL Block
            //
            // ASM Library      - In a list of libraries returned to the higher-level control app (this app)
            //                    from IL Compiler
            //  : ASM Block         - In a list of blocks in the ASM Library and held against the origin IL Block
            //      : ASM Op            - In a list of ops in the ASM Block
            //
            // Options          - Compiler options used throughout the compiler

            // Library loader   - Loads IL Libraries to be compiled
            //      - Type Scanner     - Loads all the Type Infos, Field Infos and Method Infos
            // IL Compiler      - Manages the IL compile stage
            //      - IL Reader        - Loads IL ops from IL Methods in IL Types. Also load plug info.
            //      - IL Preprocessor  - Pre-scans IL ops to find things like necessary branching labels
            //                           and check for unchanged methods
            //      - IL Scanner       - Converts IL ops to ASM ops
            // ASM Compiler     - Manages the ASM compile stage
            //      - ASM Preprocessor - Pre-scans the ASM ops to store things like debug info or perform
            //                           optimisation
            //      - ASM Processor    - Converts ASM ops into ASM text then runs NASM
            // Link Manager     - Manages the linker stage. Links together all the NASM outputs using "ld".
            

            IL.ILLibrary TheLibrary = LibraryLoader.LoadILLibrary(LibraryPath);
            int NumDependencies = LibraryLoader.LoadDependencies(TheLibrary);


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
