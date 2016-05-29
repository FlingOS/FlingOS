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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Drivers.Compiler
{
    /// <summary>
    ///     Basic delegate for a void method with a single state object.
    /// </summary>
    /// <remarks>
    ///     This is used in various places within the compiler. The most obvious example
    ///     is in executing external processes asynchronously. The delegate is used as
    ///     completion handler.
    /// </remarks>
    /// <param name="state">The object supplied with the handler.</param>
    public delegate void VoidDelegate(object state);

    /// <summary>
    ///     Contains static utility methods used in various places in the compiler.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        ///     Treated as a character array of all identifiers that are illegal to use in a label name.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         One or two of the characters may be valid but by eliminating them, labels in the FlingOS
        ///         output have much more consistency.
        ///     </para>
        ///     <para>
        ///         The invalid characters are "&amp;,+$&lt;&gt;{}-`\'/\\ ()[]*!=."
        ///     </para>
        /// </remarks>
        public const string IllegalIdentifierChars = "&,+$<>{}-`\'/\\ ()[]*!=.";

        /// <summary>
        ///     A list of all processes started by the compiler.
        /// </summary>
        /// <remarks>
        ///     Maps processes' unique identifiers to the process objects themselves.
        /// </remarks>
        private static readonly Dictionary<int, Process> Processes = new Dictionary<int, Process>();

        /// <summary>
        ///     Callbacks to be called when processes finish executing.
        /// </summary>
        private static readonly Dictionary<int, Tuple<VoidDelegate, object>> OnCompleteCallbacks =
            new Dictionary<int, Tuple<VoidDelegate, object>>();

        /// <summary>
        ///     Lock used to prevent multiple threads updating the OnCompleteCallbacks dictionary simultaneously.
        /// </summary>
        private static readonly object CallbacksLock = new object();

        /// <summary>
        ///     Determiens whether the specified type is a floating point number type or not.
        /// </summary>
        /// <param name="aType">The type to check.</param>
        /// <returns>True if the type is a floating point number type. Otherwise, false.</returns>
        public static bool IsFloat(Type aType)
        {
            bool isFloat = false;

            if (aType.Equals(typeof(float)) ||
                aType.Equals(typeof(double)))
            {
                isFloat = true;
            }

            return isFloat;
        }

        /// <summary>
        ///     Reads a signed integer 16 from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static short ReadInt16(byte[] bytes, int offset)
        {
            return BitConverter.ToInt16(bytes, offset);
        }

        /// <summary>
        ///     Reads a signed integer 32 from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static int ReadInt32(byte[] bytes, int offset)
        {
            return BitConverter.ToInt32(bytes, offset);
        }

        /// <summary>
        ///     Reads an unsigned integer 32 from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static uint ReadUInt32(byte[] bytes, int offset)
        {
            return BitConverter.ToUInt32(bytes, offset);
        }

        /// <summary>
        ///     Reads a signed integer 64 from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static long ReadInt64(byte[] bytes, int offset)
        {
            return BitConverter.ToInt64(bytes, offset);
        }

        /// <summary>
        ///     Reads a single-precision (32-bit) floating point number from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static float ReadFloat32(byte[] bytes, int offset)
        {
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        ///     Reads a double-precision (64-bit) floating point number from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static double ReadFloat64(byte[] bytes, int offset)
        {
            return BitConverter.ToDouble(bytes, 0);
        }

        /// <summary>
        ///     Cleans up the specified file name of any illegal characters.
        /// </summary>
        /// <param name="filename">The file name to clean up.</param>
        /// <returns>The cleaned up file name.</returns>
        public static string CleanFileName(string filename)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, '_');
            }
            return filename;
        }

        /// <summary>
        ///     Filters the specified identifier (/assembly code label) for invalid characters.
        /// </summary>
        /// <remarks>
        ///     Imvalid characters are replaced by underscores.
        /// </remarks>
        /// <param name="x">The string to filter.</param>
        /// <returns>The filtered string.</returns>
        public static string FilterIdentifierForInvalidChars(string x)
        {
            string xTempResult = x;
            // & can result in conflict with * in type names, where both are used singularly in the same position
            //  e.g. Type* and Type&
            xTempResult = xTempResult.Replace("&", "_AMP_");
            foreach (char c in IllegalIdentifierChars)
            {
                xTempResult = xTempResult.Replace(c, '_');
            }
            return string.Intern(xTempResult);
        }

        /// <summary>
        ///     Uses Process class to start a new instance of the specified process on the machine with specified start arguments.
        ///     Note: This is a blocking function.
        ///     Note: Waits a maximum of 15 minutes before assuming the process has failed to execute.
        /// </summary>
        /// <param name="workingDir">The working directory for the new process instance.</param>
        /// <param name="processFile">The process file (.EXE file)</param>
        /// <param name="args">The start arguments to pass the process.</param>
        /// <param name="displayName">The display name of the process to show in messages.</param>
        /// <param name="ignoreErrors">Whether to ignore messages and errors from the process or not.</param>
        /// <param name="outputMessagesToFileName">
        ///     A file path to output error and standard messages to instead of the console window.
        ///     Ignore errors should be set to false.
        /// </param>
        /// <param name="OnComplete">Callback to call when the process finishes executing.</param>
        /// <param name="state">The state object to use when calling the OnComplete callback.</param>
        /// <returns>True if process executed successfully without errors. Otherwise false.</returns>
        public static bool ExecuteProcess(string workingDir,
            string processFile,
            string args,
            string displayName,
            bool ignoreErrors = false,
            string outputMessagesToFileName = null,
            VoidDelegate OnComplete = null,
            object state = null)
        {
            bool OK = true;

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.WorkingDirectory = workingDir;
            processStartInfo.FileName = processFile;
            processStartInfo.Arguments = args;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.CreateNoWindow = true;
            var process = new Process();

            StreamWriter outputStream = null;

            if (!ignoreErrors)
            {
                if (outputMessagesToFileName != null && OnComplete == null)
                {
                    outputStream = new StreamWriter(outputMessagesToFileName);
                    process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
                    {
                        if (e.Data != null)
                        {
                            outputStream.WriteLine(e.Data);
                        }
                    };
                    process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
                    {
                        if (e.Data != null)
                        {
                            outputStream.WriteLine(e.Data);
                        }
                    };
                }
                else
                {
                    process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
                    {
                        if (e.Data != null)
                        {
                            Logger.LogError(Errors.Utilities_ExternalError_ErrorCode, "", 0,
                                string.Format(Errors.ErrorMessages[Errors.Utilities_ExternalError_ErrorCode],
                                    displayName + ": " + e.Data));
                        }
                    };
                    process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
                    {
                        if (e.Data != null)
                        {
                            Logger.LogMessage("", 0, displayName + ": " + e.Data);
                        }
                    };
                }
            }
            process.StartInfo = processStartInfo;
            process.EnableRaisingEvents = true;

            if (OnComplete != null)
            {
                process.Exited += delegate(object sender, EventArgs e)
                {
                    //System.Threading.Thread.Sleep(1000);

                    try
                    {
                        Tuple<VoidDelegate, object> Callback = null;
                        lock (CallbacksLock)
                        {
                            Process theProc = (Process)sender;
                            Callback = OnCompleteCallbacks[theProc.Id];
                            OnCompleteCallbacks.Remove(theProc.Id);
                            Processes.Remove(theProc.Id);
                            theProc.Dispose();
                        }
                        Callback.Item1(Callback.Item2);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("", "", 0, ex.Message);
                    }
                };
                process.Disposed += delegate { };

                process.Start();
                lock (CallbacksLock)
                {
                    OnCompleteCallbacks.Add(process.Id, new Tuple<VoidDelegate, object>(OnComplete, state));
                    Processes.Add(process.Id, process);
                }
            }
            else
            {
                process.Start();

                if (!ignoreErrors)
                {
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                }
                process.WaitForExit(4*60*60*1000); // wait 4 hours max. for process to exit
                if (process.ExitCode != 0)
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                        Logger.LogError(Errors.Utilities_ExternalError_ErrorCode, "", 0,
                            string.Format(Errors.ErrorMessages[Errors.Utilities_ExternalError_ErrorCode],
                                displayName + ": Timed out."));
                    }
                    else
                    {
                        Logger.LogError(Errors.Utilities_ExternalError_ErrorCode, "", 0,
                            string.Format(Errors.ErrorMessages[Errors.Utilities_ExternalError_ErrorCode],
                                displayName + ": Error occurred while invoking the process."));
                    }
                }
                if (outputStream != null)
                {
                    outputStream.Flush();
                    outputStream.Close();
                    outputStream.Dispose();
                }
                OK = process.ExitCode == 0;
            }
            return OK;
        }
    }
}