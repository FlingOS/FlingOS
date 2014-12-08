using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Drivers.Compiler.MSBuildTask
{
    public class BuildTask : Task
    {
        
        public override bool Execute()
        {
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
