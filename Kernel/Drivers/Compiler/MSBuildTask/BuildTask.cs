using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Drivers.Compiler.MSBuildTask
{
    public class BuildTask : Task
    {
        [Required]
        public string LibraryPath
        {
            get;
            set;
        }

        [Required]
        public string TargetArchitecture
        {
            get;
            set;
        }
        [Required]
        public string OutputPath
        {
            get;
            set;
        }
        [Required]
        public string ToolsPath
        {
            get;
            set;
        }
        [Required]
        public bool DebugBuild
        {
            get;
            set;
        }
        
        public override bool Execute()
        {
            App.CompilerProcess.LibraryPath = LibraryPath;
            App.CompilerProcess.OutputPath = OutputPath;
            App.CompilerProcess.ToolsPath = ToolsPath;

            Options.BuildMode = DebugBuild ? Options.BuildModes.Debug : Options.BuildModes.Release;
            Options.TargetArchitecture = TargetArchitecture;
            
            return App.CompilerProcess.Execute(
                    Logger_OnLogMessage,
                    Logger_OnLogWarning,
                    Logger_OnLogError) != App.CompilerProcess.ErrorCode.NoError;
        }

        private void Logger_OnLogError(string errorCode, string file, int lineNumber, string message)
        {
            Log.LogError(String.Empty, errorCode, String.Empty, file, lineNumber, 0, lineNumber, 0, message);
        }
        private void Logger_OnLogWarning(string warningCode, string file, int lineNumber, string message)
        {
            Log.LogWarning(String.Empty, warningCode, String.Empty, file, lineNumber, 0, lineNumber, 0, message);
        }
        private void Logger_OnLogMessage(string file, int lineNumber, string message)
        {
            Log.LogMessage(String.Empty, String.Empty, String.Empty, file, lineNumber, 0, lineNumber, 0, MessageImportance.High, message);
        }
    }
}
