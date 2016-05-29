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

using System;
using Drivers.Compiler.ASM;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.App
{
    /// <summary>
    ///     The main compiler process for the command line app.
    /// </summary>
    public class CompilerProcess
    {
        /// <summary>
        ///     Summary error codes that can be returned by the Main method upon completion.
        /// </summary>
        public enum ErrorCode
        {
            /// <summary>
            ///     Indicates no errors occurred during execution.
            /// </summary>
            NoError = 0,

            /// <summary>
            ///     Indicates one or more of the options specified on the command line are invalid.
            /// </summary>
            InvalidOptions = 1,

            /// <summary>
            ///     Indicates the IL compiler step failed.
            /// </summary>
            ILCompilerFailed = 2,

            /// <summary>
            ///     Indicates the ASM compiler step failed.
            /// </summary>
            ASMCompilerFailed = 3,

            /// <summary>
            ///     Indicates the linker step failed.
            /// </summary>
            LinkerFailed = 4
        }

        /// <summary>
        ///     The main entry point of the program.
        /// </summary>
        /// <param name="args">The arguments specified on the command line.</param>
        /// <returns>Returns an error code.</returns>
        /// <seealso cref="ErrorCode" />
        private static int Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Gray;

            int result = -1;

            if (ValidateArguments(args))
            {
                Options.LibraryPath = args[0];
                Options.OutputPath = args[1];
                Options.ToolsPath = args[2];

                Options.BuildMode = args[3].ToLower() == "debug" ? Options.BuildModes.Debug : Options.BuildModes.Release;
                Options.LinkMode = args[5].ToLower() == "iso" ? Options.LinkModes.ISO : Options.LinkModes.ELF;
                Options.TargetArchitecture = args[4];

                if (args.Length >= 7)
                {
                    Options.BaseAddress = Convert.ToUInt64(args[6], 16);

                    if (args.Length >= 8)
                    {
                        Options.LoadOffset = Convert.ToInt64(args[7], 16);
                    }
                }

                result = (int)Execute(
                    Logger_OnLogMessage,
                    Logger_OnLogWarning,
                    Logger_OnLogError);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            return result;
        }

        /// <summary>
        ///     Validates the specified command line arguments.
        /// </summary>
        /// <param name="args">The arguments to validate.</param>
        /// <returns>True if the arguments appear to be valid, otherwise false.</returns>
        private static bool ValidateArguments(string[] args)
        {
            if (args.Length < 6)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Expected 5 arguments:");
                Console.WriteLine(@"0 : Library Path - The path to the library to compile.
1 : Output Path - The path to the output directory.
2 : Tools Path - The path to the tools directory.
3 : Build Mode - Debug or Release.
4 : Target Architecture - e.g. x86, x64, MIPS
5 : Link Mode - ISO or ELF.
6 : [Optional] ISO Kernel base address
7 : [Optional] ISO Kernel load offset (subtracted from ISO Kernel base address)
");
                Console.ForegroundColor = ConsoleColor.Gray;

                return false;
            }

            if (args[3].ToLower() != "debug" && args[3].ToLower() != "release")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Build Mode argument invalid! Should be \"Debug\" or \"Release\".");
                Console.ForegroundColor = ConsoleColor.Gray;

                return false;
            }

            if (args[5].ToLower() != "iso" && args[5].ToLower() != "elf")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Link Mode argument invalid! Should be \"ISO\" or \"ELF\".");
                Console.ForegroundColor = ConsoleColor.Gray;

                return false;
            }
            return true;
        }

        /// <summary>
        ///     Executes the Drivers Compiler using the specified logging methods.
        /// </summary>
        /// <param name="messageHandler">The handler for outputting ordinary messages.</param>
        /// <param name="warningHandler">The handler for outputting warning messages.</param>
        /// <param name="errorHandler">The handler for outputting error messages.</param>
        /// <returns>Returns an error code.</returns>
        public static ErrorCode Execute(
            Logger.LogMessageEventHandler messageHandler,
            Logger.LogWarningEventHandler warningHandler,
            Logger.LogErrorEventHandler errorHandler)
        {
            ErrorCode result = ErrorCode.NoError;

            Logger.OnLogMessage += messageHandler;
            Logger.OnLogWarning += warningHandler;
            Logger.OnLogError += errorHandler;

            Options.Format();

            DateTime startTime = DateTime.Now;
            Logger.LogMessage("", 0, "Driver compiler started  @ " + startTime.ToLongTimeString());
            Logger.LogMessage("", 0, "Library path             = \"" + Options.LibraryPath + "\"");
            Logger.LogMessage("", 0, "Output path              = \"" + Options.OutputPath + "\"");
            Logger.LogMessage("", 0, "Tools path               = \"" + Options.ToolsPath + "\"");
            Logger.LogMessage("", 0, "Target architecture      = \"" + Options.TargetArchitecture + "\"");
            Logger.LogMessage("", 0,
                "Build mode               = " + Enum.GetName(typeof(Options.BuildModes), Options.BuildMode));
            Logger.LogMessage("", 0,
                "Link mode                = " + Enum.GetName(typeof(Options.LinkModes), Options.LinkMode));
            Logger.LogMessage("", 0, "Base address             = " + Options.BaseAddress);
            Logger.LogMessage("", 0, "Load offset              = " + Options.LoadOffset);

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
            //      - Type Scanner     - Loads all the Type Infos, Field Infos and Method Infos.
            // IL Compiler      - Manages the IL compile stage
            //      - IL Reader        - Loads IL ops from IL Methods in IL Types. Also loads plug info.
            //      - IL Preprocessor  - Pre-scans IL ops to find things like necessary branching labels. 
            //                           Also handles injecting any necessary IL ops in such a way that 
            //                           IL integrity is maintained.
            //      - IL Scanner       - Converts IL ops to ASM ops
            // ASM Compiler     - Manages the ASM compile stage
            //      - ASM Preprocessor - Pre-scans the ASM ops to store things like debug info or perform
            //                           optimisation
            //      - ASM Processor    - Converts ASM ops into ASM text then runs the assembly code compiler (e.g. NASM)
            // Link Manager     - Manages the linker stage. Links together all the object files using "ld".

            // To think about:
            //      - Try-catch-finally blocks
            //      - GC (inc. wrapping try-finally, calls to inc/dec)
            //      - Release / Debug IL (differences? Potential issues?)

            // Resultant thoughts from above:
            //      - IL labels based on IL Op index NOT IL op offset
            //      - IL Preprocessor handle injecting any IL ops inc. try-catch-finally, GC stuff and special
            //        class / method stuff (e.g. static variables, static constructors etc.)
            //      - IL Preprocessor needs to maintain the integrity of the IL so that no assumption are made
            //          so that Release mode IL also works

            // TODO:
            //      - Check for unchanged methods (in IL Preprocessor) and exclude them from recompile

            Tuple<bool, string> ValidateOptions_Result = Options.Validate();
            if (ValidateOptions_Result.Item1)
            {
                try
                {
                    TargetArchitecture.Init();

                    ILLibrary TheLibrary = LibraryLoader.LoadILLibrary(Options.LibraryPath);

                    CompileResult ILCompileResult = ILCompiler.Compile(TheLibrary);

                    if (ILCompileResult == CompileResult.OK ||
                        ILCompileResult == CompileResult.PartialFailure)
                    {
                        CompileResult ASMCompileResult = ASMCompiler.Compile(TheLibrary);

                        if (ASMCompileResult == CompileResult.OK)
                        {
                            CompileResult LinkResult = LinkManager.Link(TheLibrary);

                            if (LinkResult == CompileResult.OK)
                            {
                                if (ILCompileResult == CompileResult.PartialFailure)
                                {
                                    result = ErrorCode.ILCompilerFailed;
                                }

                                //Success
                                Logger.LogMessage("", 0, "Compilation succeeded.");
                            }
                            else
                            {
                                //Fail
                                Logger.LogError(Errors.Linker_LinkFailed_ErrorCode, "", 0,
                                    Errors.ErrorMessages[Errors.Linker_LinkFailed_ErrorCode]);
                                result = ErrorCode.LinkerFailed;
                            }
                        }
                        else
                        {
                            //Fail
                            Logger.LogError(Errors.ASMCompiler_CompileFailed_ErrorCode, "", 0,
                                Errors.ErrorMessages[Errors.ASMCompiler_CompileFailed_ErrorCode]);
                            result = ErrorCode.ASMCompilerFailed;
                        }
                    }
                    else
                    {
                        //Fail
                        Logger.LogError(Errors.ILCompiler_CompileFailed_ErrorCode, "", 0,
                            Errors.ErrorMessages[Errors.ILCompiler_CompileFailed_ErrorCode]);
                        result = ErrorCode.ILCompilerFailed;
                    }
                }
                catch (NullReferenceException ex)
                {
                    Logger.LogError(Errors.ILCompiler_NullRefException_ErrorCode, "", 0,
                        string.Format(
                            Errors.ErrorMessages[Errors.ILCompiler_NullRefException_ErrorCode],
                            ex.Message, ex.StackTrace));
                    result = ErrorCode.ILCompilerFailed;
                }
                catch (Exception ex)
                {
                    Logger.LogError(Errors.ILCompiler_UnexpectedException_ErrorCode, "", 0,
                        string.Format(
                            Errors.ErrorMessages[Errors.ILCompiler_UnexpectedException_ErrorCode],
                            ex.Message, ex.StackTrace));
                    result = ErrorCode.ILCompilerFailed;
                }
            }
            else
            {
                //Fail
                Logger.LogError(Errors.PreReqs_OptionsInvalid_ErrorCode, "", 0,
                    string.Format(
                        Errors.ErrorMessages[Errors.PreReqs_OptionsInvalid_ErrorCode],
                        ValidateOptions_Result.Item2));
                result = ErrorCode.InvalidOptions;
            }

            DateTime endTime = DateTime.Now;
            Logger.LogMessage("", 0, "Driver compiler finished @ " + endTime.ToLongTimeString());
            Logger.LogMessage("", 0, "            Compile time : " + (endTime - startTime));
            Logger.LogMessage("", 0, "              Error code : " + Enum.GetName(typeof(ErrorCode), result));

            return result;
        }

        /// <summary>
        ///     Logs an error message.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="file">The file associated with the error.</param>
        /// <param name="lineNumber">The line number in the file associated with the error.</param>
        /// <param name="message">The error message.</param>
        private static void Logger_OnLogError(string errorCode, string file, int lineNumber, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error : " + errorCode + ": " + message + " in " + file + ":" + lineNumber);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        ///     Logs an warning message.
        /// </summary>
        /// <param name="warningCode">The warning code.</param>
        /// <param name="file">The file associated with the warning.</param>
        /// <param name="lineNumber">The line number in the file associated with the warning.</param>
        /// <param name="message">The warning message.</param>
        private static void Logger_OnLogWarning(string warningCode, string file, int lineNumber, string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Warning : " + warningCode + ": " + message + " in " + file + ":" + lineNumber);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        ///     Logs a message.
        /// </summary>
        /// <param name="file">The file associated with the message.</param>
        /// <param name="lineNumber">The line number in the file associated with the message.</param>
        /// <param name="message">The message.</param>
        private static void Logger_OnLogMessage(string file, int lineNumber, string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message + " in " + file + ":" + lineNumber);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}