using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Drivers.Compiler
{
    public delegate void VoidDelegate(object state);

    public static class Utilities
    {
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
        /// Reads a signed integer 16 from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static Int16 ReadInt16(byte[] bytes, int offset)
        {
            return BitConverter.ToInt16(bytes, offset);
        }
        /// <summary>
        /// Reads a signed integer 32 from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static Int32 ReadInt32(byte[] bytes, int offset)
        {
            return BitConverter.ToInt32(bytes, offset);
        }
        /// <summary>
        /// Reads an unsigned integer 32 from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static UInt32 ReadUInt32(byte[] bytes, int offset)
        {
            return BitConverter.ToUInt32(bytes, offset);
        }
        /// <summary>
        /// Reads a signed integer 64 from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static Int64 ReadInt64(byte[] bytes, int offset)
        {
            return BitConverter.ToInt64(bytes, offset);
        }
        /// <summary>
        /// Reads a single-precision (32-bit) floating point number from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static float ReadFloat32(byte[] bytes, int offset)
        {
            return (float)(BitConverter.ToSingle(bytes, 0));
        }
        /// <summary>
        /// Reads a double-precision (64-bit) floating point number from the specified bytes starting at the specified offset.
        /// </summary>
        /// <param name="bytes">The bytes to read from.</param>
        /// <param name="offset">The offset in the bytes to read from.</param>
        /// <returns>The number.</returns>
        public static double ReadFloat64(byte[] bytes, int offset)
        {
            return (double)(BitConverter.ToDouble(bytes, 0));
        }


        public static string CleanFileName(string filename)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, '_');
            }
            return filename;
        }


        public const string IllegalIdentifierChars = "&,+$<>{}-`\'/\\ ()[]*!=.";
        public static string FilterIdentifierForInvalidChars(string x)
        {
            string xTempResult = x;
            foreach (char c in IllegalIdentifierChars)
            {
                xTempResult = xTempResult.Replace(c, '_');
            }
            return String.Intern(xTempResult);
        }

        private static Dictionary<int, Process> Processes = new Dictionary<int, Process>();
        private static Dictionary<int, Tuple<VoidDelegate, object>> OnCompleteCallbacks = new Dictionary<int, Tuple<VoidDelegate, object>>();
        private static Object CallbacksLock = new Object();

        /// <summary>
        /// Uses Process class to start a new instance of the specified process on the machine with specified start arguments.
        /// Note: This is a blocking function.
        /// Note: Waits a maximum of 15 minutes before assuming the process has failed to execute.
        /// </summary>
        /// <param name="workingDir">The working directory for the new process instance.</param>
        /// <param name="processFile">The process file (.EXE file)</param>
        /// <param name="args">The start arguments to pass the process.</param>
        /// <param name="displayName">The display name of the process to show in messages.</param>
        /// <param name="ignoreErrors">Whether to ignore messages and errors from the process or not.</param>
        /// <param name="outputMessagesToFileName">A file path to output error and standard messages to instead of the console window. 
        /// Ignore errors should be set to false.</param>
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
                            Logger.LogMessage("", 0,
                                string.Format("Utilities: message from external process: {0}",
                                displayName + ": " + e.Data));
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
                    System.Threading.Thread.Sleep(1000);

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
                process.Disposed += delegate(object sender, EventArgs e)
                {
                };

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
                process.WaitForExit(4 * 60 * 60 * 1000); // wait 4 hours max. for process to exit
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
