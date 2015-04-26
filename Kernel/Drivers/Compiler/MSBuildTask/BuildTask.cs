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
        [Required]
        public bool ISOLink
        {
            get;
            set;
        }
        
        public override bool Execute()
        {
            Options.LibraryPath = LibraryPath;
            Options.OutputPath = OutputPath;
            Options.ToolsPath = ToolsPath;

            Options.BuildMode = DebugBuild ? Options.BuildModes.Debug : Options.BuildModes.Release;
            Options.LinkMode = ISOLink ? Options.LinkModes.ISO : Options.LinkModes.ELF;
            Options.TargetArchitecture = TargetArchitecture;
            
            return App.CompilerProcess.Execute(
                    Logger_OnLogMessage,
                    Logger_OnLogWarning,
                    Logger_OnLogError) == App.CompilerProcess.ErrorCode.NoError;
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
