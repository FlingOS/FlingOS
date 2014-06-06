using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Kernel.Compiler.App
{
    /// <summary>
    /// The main application process.
    /// </summary>
    public class KernelCompiler : Task
    {
        /// <summary>
        /// All the error codes the compiler process might return.
        /// </summary>
        enum ErrorCodes
        {
            /// <summary>
            /// No errors during compilation
            /// </summary>
            Success = 0,
            /// <summary>
            /// Failed to initialise settings.
            /// </summary>
            InitSettingsFail = 1,
            /// <summary>
            /// Failed to initialise output and handlers.
            /// </summary>
            InitOutputAndHandlersFail = 2,
            /// <summary>
            /// Failed to initialise the IL compiler.
            /// </summary>
            InitILCompilerFail = 3,
            /// <summary>
            /// Failed to execute the IL Reader task.
            /// </summary>
            ExecuteILReaderFail = 4,
            /// <summary>
            /// Failed to execute the IL Scanner task.
            /// </summary>
            ExecuteILScannerFail = 5,
            /// <summary>
            /// Failed to execute the NASM task.
            /// </summary>
            ExecuteNASMFail = 6,
            /// <summary>
            /// Failed to execute the Finalise task.
            /// </summary>
            FinaliseFail = 7,
            /// <summary>
            /// Failed to execute the Cleanup task.
            /// </summary>
            CleanupFail = 8,
            /// <summary>
            /// Failed to execute the ASM Sequencer task.
            /// </summary>
            ExecuteASMSequencerFail = 9
        }

        /// <summary>
        /// The colour to display errors messages in.
        /// </summary>
        static ConsoleColor ErrorColour = ConsoleColor.Red;
        /// <summary>
        /// The colour to display standard messages in.
        /// </summary>
        static ConsoleColor MessageColour = ConsoleColor.White;
        /// <summary>
        /// The colour to display warning messages in.
        /// </summary>
        static ConsoleColor WarningColour = ConsoleColor.Yellow;

        /// <summary>
        /// The ILCompiler that will be used by this compiler instance.
        /// </summary>
        static ILCompiler TheILCompiler;
        /// <summary>
        /// The settings that will be used by this compiler instance.
        /// </summary>
        /// <remarks>
        /// Initially set to new but empty settings instance so that MSBuild can fill in certain settings.
        /// If executed not via MSBuild, this field is set to a new Setttings instance created from the 
        /// string array of arguments passed to the program.
        /// </remarks>
        static Settings TheSettings = new Settings();

        /// <summary>
        /// The number of errors outputted.
        /// </summary>
        static int Errors = 0;
        /// <summary>
        /// The number of messages outputted.
        /// </summary>
        static int Messages = 0;
        /// <summary>
        /// The number of warnings outputted.
        /// </summary>
        static int Warnings = 0;

        /// <summary>
        /// Indicates whether the compiler is running as an MSBuild task or not.
        /// </summary>
        static bool ExecutingUnderMSBuild = false;

        /// <summary>
        /// Used by MSBuild to set settings before execution.
        /// </summary>
        [Required]
        public string InputFile
        {
            get
            {
                return TheSettings[Settings.InputFileKey];
            }
            set
            {
                TheSettings[Settings.InputFileKey] = value;
            }
        }
        /// <summary>
        /// Used by MSBuild to set settings before execution.
        /// </summary>
        [Required]
        public string TargetArchitecture
        {
            get
            {
                return TheSettings[Settings.TargetArchitectureKey];
            }
            set
            {
                TheSettings[Settings.TargetArchitectureKey] = value;
            }
        }
        /// <summary>
        /// Used by MSBuild to set settings before execution.
        /// </summary>
        [Required]
        public string OutputFile
        {
            get
            {
                return TheSettings[Settings.OutputFileKey];
            }
            set
            {
                TheSettings[Settings.OutputFileKey] = value;
            }
        }
        /// <summary>
        /// Used by MSBuild to set settings before execution.
        /// </summary>
        [Required]
        public string ToolsPath
        {
            get
            {
                return TheSettings[Settings.ToolsPathKey];
            }
            set
            {
                TheSettings[Settings.ToolsPathKey] = value;
            }
        }
        /// <summary>
        /// Used by MSBuild to set settings before execution.
        /// </summary>
        public string DebugBuild
        {
            get
            {
                return TheSettings[Settings.DebugBuildKey];
            }
            set
            {
                TheSettings[Settings.DebugBuildKey] = value;
            }
        }

        /// <summary>
        /// A reference to the MSBuild Log for outputting errors, warnings and messages.
        /// </summary>
        private static TaskLoggingHelper MSBuild_Log;

        /// <summary>
        /// The entry point of the compiler if it is executed as an MSBuild task
        /// </summary>
        /// <returns>True if compilation successful. Otherwise false.</returns>
        public override bool Execute()
        {
            ExecutingUnderMSBuild = true;

            MSBuild_Log = Log;

            MSBuild_OutputMessage("Kernel Compiler execution started @ " + DateTime.Now.ToLongTimeString());
                        
            //For compatiblity with Visual Studio's Platform Targets we use:
            //x86 = x86_32
            //x64 = x86_64
            switch(TargetArchitecture)
            {
                case "x86":
                    TargetArchitecture = "x86_32";
                    break;
                case "x64":
                    TargetArchitecture = "x86_64";
                    break;
                default:
                    break;
            }
      
            MSBuild_OutputMessage("Input file = " + InputFile);
            MSBuild_OutputMessage("Target Architecture = " + TargetArchitecture);
            MSBuild_OutputMessage("Output file = " + OutputFile);
            MSBuild_OutputMessage("Tools path = " + ToolsPath);
            MSBuild_OutputMessage("Debug build = " + (DebugBuild != null ? "Yes" : "No"));

            bool result = Main(null) == (int)ErrorCodes.Success;

            MSBuild_OutputMessage("Kernel Compiler execution ended @ " + DateTime.Now.ToLongTimeString());

            return result;
        }

        /// <summary>
        /// The kernel compiler application's main entry point.
        /// </summary>
        /// <param name="args">A list of arguments for the compiler. See Settings for required args and format.</param>
        /// <returns>The result of compilation. 0 = success, otherwise error code.</returns>
        public static int Main(string[] args)
        {
            ErrorCodes resultCode = ErrorCodes.Success;

            if (!ExecutingUnderMSBuild)
            {
                //Increase the buffer height so more lines can be stored
                Console.BufferHeight = 300;
                //Increase the window width because:
                //  a) Most developers develop on large screens so can handle a larger window
                //  b) So increasing window width allows full lines of error messages 
                //     (e.g. stack traces) to be displayed neatly
                Console.WindowWidth = 140;
            }

            //Make sure we haven't failed any stage of processing thus far
            if (resultCode == ErrorCodes.Success)
            {
                if (!ExecutingUnderMSBuild)
                {
                    //Initialise the compiler settings
                    resultCode = InitSettings(args) ? ErrorCodes.Success : ErrorCodes.InitSettingsFail;
                    //Initialise settings may fail but if help option is specified we want to handle it regardless.
                    if (TheSettings[Settings.HelpKey] == "on")
                    {
                        OutputHelp();
                    }
                }
            }

            //Make sure we haven't failed any stage of processing thus far
            if (resultCode == ErrorCodes.Success)
            {
                //Initialise the ILCompiler
                resultCode = InitILCompiler() ? ErrorCodes.Success : ErrorCodes.InitILCompilerFail;
            }

            //Make sure we haven't failed any stage of processing thus far
            if (resultCode == ErrorCodes.Success)
            {
                //Initialise the output streams etc.
                resultCode = InitOutputAndHandlers() ? ErrorCodes.Success : ErrorCodes.InitOutputAndHandlersFail;
            }
            
            //Execute all the ILCompiler tasks in this order:
            /*  - ILReader
             *  - ILScanner
             *  - ASMSequencer
             *  - NASM
             *  - Finalise
             *  - Cleanup
             */
            if (resultCode == ErrorCodes.Success)
            {
                try
                {
                    resultCode = TheILCompiler.ExecuteILReader() ? ErrorCodes.Success : ErrorCodes.ExecuteILReaderFail;
                }
                catch (Exception ex)
                {
                    resultCode = ErrorCodes.ExecuteILReaderFail;
                    if (ExecutingUnderMSBuild)
                    {
                        MSBuild_OutputError(ex);
                    }
                    else
                    {
                        OutputError(ex);
                    }
                }
            }
            if (resultCode == ErrorCodes.Success)
            {
                try
                {
                    resultCode = TheILCompiler.ExecuteILScanner() ? ErrorCodes.Success : ErrorCodes.ExecuteILScannerFail;
                }
                catch (Exception ex)
                {
                    resultCode = ErrorCodes.ExecuteILScannerFail;
                    if (ExecutingUnderMSBuild)
                    {
                        MSBuild_OutputError(ex);
                    }
                    else
                    {
                        OutputError(ex);
                    }
                }
            }
            if (resultCode == ErrorCodes.Success)
            {
                try
                {
                    resultCode = TheILCompiler.ExecuteASMSequencer() ? ErrorCodes.Success : ErrorCodes.ExecuteASMSequencerFail;
                }
                catch (Exception ex)
                {
                    resultCode = ErrorCodes.ExecuteASMSequencerFail;
                    if (ExecutingUnderMSBuild)
                    {
                        MSBuild_OutputError(ex);
                    }
                    else
                    {
                        OutputError(ex);
                    }
                }
            }
            if (resultCode == ErrorCodes.Success)
            {
                try
                {
                    resultCode = TheILCompiler.ExecuteNASM() ? ErrorCodes.Success : ErrorCodes.ExecuteNASMFail;
                }
                catch (Exception ex)
                {
                    resultCode = ErrorCodes.ExecuteNASMFail;
                    if (ExecutingUnderMSBuild)
                    {
                        MSBuild_OutputError(ex);
                    }
                    else
                    {
                        OutputError(ex);
                    }
                }
            }
            if (resultCode == ErrorCodes.Success)
            {
                try
                {
                    resultCode = TheILCompiler.Finalise() ? ErrorCodes.Success : ErrorCodes.FinaliseFail;
                }
                catch (Exception ex)
                {
                    resultCode = ErrorCodes.FinaliseFail;
                    if (ExecutingUnderMSBuild)
                    {
                        MSBuild_OutputError(ex);
                    }
                    else
                    {
                        OutputError(ex);
                    }
                }
            }
            //Always run cleanup!
            {
                try
                {
                    TheILCompiler.Cleanup();
                }
                catch (Exception ex)
                {
                    resultCode = ErrorCodes.CleanupFail;
                    if (ExecutingUnderMSBuild)
                    {
                        MSBuild_OutputError(ex);
                    }
                    else
                    {
                        OutputError(ex);
                    }
                }
            }

            if (!ExecutingUnderMSBuild)
            {
                //Final message to user to say we have completed compilation.
                string finalMessage = "Compilation completed " + (resultCode == ErrorCodes.Success ? " successfully." : " with errors.") + "\r\n";
                finalMessage += "\r\nResult: " + resultCode.ToString() + "\r\n";
                finalMessage += "Errors: " + Errors + "\r\n";
                finalMessage += "Messages: " + Messages + "\r\n";
                finalMessage += "Warnings: " + Warnings;
                OutputMessage(finalMessage);

                Console.WriteLine("Press enter to exit...");
                //A final readline so the user (developer) can read the compiler output.
                Console.ReadLine();
            }

            //Return the final result of whether we compiled successfully or not.
            return (int)resultCode;
        }

        /// <summary>
        /// Outputs the help text.
        /// </summary>
        private static void OutputHelp()
        {
            string helpText = "Help\r\n";
            helpText += "\r\nThe first argument is allowed to have no key but must be the path to the input file. Otherwise, args can appear in any order.\r\n";
            helpText += "\r\nKey=Value\r\n\r\n";
            helpText += "InputFile = FilePath   - specifies the file path to the IL assembly to compile.\r\n";
            helpText += "Help      = On/Off     - specifies whether to output this help text or not.\r\n";
            helpText += "\r\nRequired Args:\r\n";
            helpText += "InputFile";
            OutputMessage(helpText);
        }

        /// <summary>
        /// Initialises the ILCompiler.
        /// </summary>
        /// <returns>True if initialisation completed successfully. Otherwise false.</returns>
        private static bool InitILCompiler()
        {
            bool OK = false;
            try
            {
                TheILCompiler = new ILCompiler(TheSettings);
                OK = true;
            }
            catch (Exception ex)
            {
                if (ExecutingUnderMSBuild)
                {
                    MSBuild_OutputError(ex);
                }
                else
                {
                    OutputError(ex);
                }
                OK = false;
            }
            return OK;
        }
        /// <summary>
        /// Initialises the output streams and handlers.
        /// </summary>
        /// <returns>True if initialisation completed successfully. Otherwise false.</returns>
        private static bool InitOutputAndHandlers()
        {
            if (ExecutingUnderMSBuild)
            {
                TheILCompiler.OutputError = MSBuild_OutputError;
                TheILCompiler.OutputMessage = MSBuild_OutputMessage;
                TheILCompiler.OutputWarning = MSBuild_OutputWarning;
            }
            else
            {
                TheILCompiler.OutputError = OutputError;
                TheILCompiler.OutputMessage = OutputMessage;
                TheILCompiler.OutputWarning = OutputWarning;
            }

            //SUPPORT - Support output to file
            // and make this method only return true if outputs properly setup
            return true;
        }
        /// <summary>
        /// Initialises the Settings using the supplied arguments.
        /// </summary>
        /// <param name="startArgs">The array of arguments to use for intialisation. See Settings for required args and format.</param>
        /// <returns>True if initialisation completed successfully. Otherwise false.</returns>
        private static bool InitSettings(string[] startArgs)
        {
            bool OK = false;
            try
            {
                TheSettings = new Settings(startArgs);
                TheSettings[Settings.ToolsPathKey] = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

                //Immediately check that they are valid
                if (!TheSettings.CheckIfArgsValid())
                {
                    throw new ArgumentException("Arguments invalid! Use \"help=on\" for help.");
                }
                //If the arguments are valid, check that all the required ones are there.
                else if (!TheSettings.CheckForRequiredArgs())
                {
                    throw new ArgumentException("Arguments missing! Use \"help=on\" for help.");
                }
                OK = true;
            }
            catch(Exception ex)
            {
                if (ExecutingUnderMSBuild)
                {
                    MSBuild_OutputError(ex);
                }
                else
                {
                    OutputError(ex);
                }
                OK = false;
            }
            return OK;
        }

        /// <summary>
        /// Outputs an error message to the console window.
        /// </summary>
        /// <param name="ex">The exception object to output.</param>
        public static void OutputError(Exception ex)
        {
            Errors++;

            string message = "\r\n----------------------------------------\r\nError: " + 
                ex.Message;
            if (!string.IsNullOrWhiteSpace(ex.StackTrace))
            {
                message += "\r\n" + ex.StackTrace;
            }
            message += "\r\n----------------------------------------";
            if (ex.InnerException == null)
            {
                message += "\r\n";
            }

            Console.ForegroundColor = ErrorColour;
            Console.WriteLine(message);

            if (ex.InnerException != null)
            {
                Console.Write("Inner exception:");
                OutputError(ex.InnerException);
            }
        }
        /// <summary>
        /// Outputs a standard message to the console window.
        /// </summary>
        /// <param name="message">The message to output.</param>
        public static void OutputMessage(string message)
        {
            Messages++;

            Console.ForegroundColor = MessageColour;
            Console.WriteLine("\r\n----------------------------------------\r\n" + 
                              message + 
                              "\r\n----------------------------------------\r\n");
        }
        /// <summary>
        /// Outputs a warning message to the console window.
        /// </summary>
        /// <param name="ex">The exception object that represents the warning message to output.</param>
        public static void OutputWarning(Exception ex)
        {
            Warnings++;

            string message = "\r\n----------------------------------------\r\nWarning: " + 
                ex.Message;
            if (!string.IsNullOrWhiteSpace(ex.StackTrace))
            {
                message += "\r\n" + ex.StackTrace;
            }
            message += "\r\n----------------------------------------";
            if (ex.InnerException == null)
            {
                message += "\r\n";
            }

            Console.ForegroundColor = WarningColour;
            Console.WriteLine(message);
            
            if (ex.InnerException != null)
            {
                Console.Write("Inner exception:");
                OutputWarning(ex.InnerException);
            }
        }

        /// <summary>
        /// Outputs an error message via the MSBuild task.
        /// </summary>
        /// <param name="ex">The exception object to output.</param>
        public static void MSBuild_OutputError(Exception ex)
        {
            Errors++;

            MSBuild_Log.LogErrorFromException(ex, true, true, null);
        }
        /// <summary>
        /// Outputs a standard message via the MSBuild task.
        /// </summary>
        /// <param name="message">The message to output.</param>
        public static void MSBuild_OutputMessage(string message)
        {
            Messages++;

            MSBuild_Log.LogMessage(MessageImportance.High, message);
        }
        /// <summary>
        /// Outputs a warning message via the MSBuild task.
        /// </summary>
        /// <param name="ex">The exception object that represents the warning message to output.</param>
        public static void MSBuild_OutputWarning(Exception ex)
        {
            Warnings++;

            MSBuild_Log.LogWarningFromException(ex, true);
        }
    }
}
