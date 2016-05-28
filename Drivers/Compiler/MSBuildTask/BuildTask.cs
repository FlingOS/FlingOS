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
using Drivers.Compiler.App;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Drivers.Compiler.MSBuildTask
{
    /// <summary>
    ///     The main compiler process for the MSBuild task interface.
    /// </summary>
    public class BuildTask : Task
    {
        /// <summary>
        ///     The path to the library to compile.
        /// </summary>
        /// <value>Gets/sets an implicitly defined field.</value>
        [Required]
        public string LibraryPath { get; set; }

        /// <summary>
        ///     The name of the target architecture.
        /// </summary>
        /// <value>Gets/sets an implicitly defined field.</value>
        [Required]
        public string TargetArchitecture { get; set; }

        /// <summary>
        ///     The path to the output folder.
        /// </summary>
        /// <value>Gets/sets an implicitly defined field.</value>
        [Required]
        public string OutputPath { get; set; }

        /// <summary>
        ///     The path to the compiler's tools folder.
        /// </summary>
        /// <value>Gets/sets an implicitly defined field.</value>
        [Required]
        public string ToolsPath { get; set; }

        /// <summary>
        ///     Whether to perform a debug build or not.
        /// </summary>
        /// <value>Gets/sets an implicitly defined field.</value>
        [Required]
        public bool DebugBuild { get; set; }

        /// <summary>
        ///     Whether to link to a .ISO file or not. (Otherwise it links to a .ELF and .A's)
        /// </summary>
        /// <value>Gets/sets an implicitly defined field.</value>
        [Required]
        public bool ISOLink { get; set; }

        public string BaseAddress { get; set; }

        public string LoadOffset { get; set; }

        /// <summary>
        ///     Executes the compiler.
        /// </summary>
        /// <returns>True if the build task completes successfully. Otherwise, false.</returns>
        public override bool Execute()
        {
            Options.LibraryPath = LibraryPath;
            Options.OutputPath = OutputPath;
            Options.ToolsPath = ToolsPath;

            Options.BuildMode = DebugBuild ? Options.BuildModes.Debug : Options.BuildModes.Release;
            Options.LinkMode = ISOLink ? Options.LinkModes.ISO : Options.LinkModes.ELF;
            Options.TargetArchitecture = TargetArchitecture;

            Options.BaseAddress = Convert.ToUInt64(BaseAddress, 16);
            Options.LoadOffset = Convert.ToInt64(LoadOffset, 16);

            return CompilerProcess.Execute(
                Logger_OnLogMessage,
                Logger_OnLogWarning,
                Logger_OnLogError) == CompilerProcess.ErrorCode.NoError;
        }

        /// <summary>
        ///     Logs an error message.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="file">The file associated with the error.</param>
        /// <param name="lineNumber">The line number in the file associated with the error.</param>
        /// <param name="message">The error message.</param>
        private void Logger_OnLogError(string errorCode, string file, int lineNumber, string message)
        {
            Log.LogError(string.Empty, errorCode, string.Empty, file, lineNumber, 0, lineNumber, 0, message);
        }

        /// <summary>
        ///     Logs an warning message.
        /// </summary>
        /// <param name="warningCode">The warning code.</param>
        /// <param name="file">The file associated with the warning.</param>
        /// <param name="lineNumber">The line number in the file associated with the warning.</param>
        /// <param name="message">The warning message.</param>
        private void Logger_OnLogWarning(string warningCode, string file, int lineNumber, string message)
        {
            Log.LogWarning(string.Empty, warningCode, string.Empty, file, lineNumber, 0, lineNumber, 0, message);
        }

        /// <summary>
        ///     Logs a message.
        /// </summary>
        /// <param name="file">The file associated with the message.</param>
        /// <param name="lineNumber">The line number in the file associated with the message.</param>
        /// <param name="message">The message.</param>
        private void Logger_OnLogMessage(string file, int lineNumber, string message)
        {
            Log.LogMessage(string.Empty, string.Empty, string.Empty, file, lineNumber, 0, lineNumber, 0,
                MessageImportance.High, message);
        }
    }
}