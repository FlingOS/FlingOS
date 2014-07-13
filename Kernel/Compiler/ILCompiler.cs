#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using Mosa.Utility.IsoImage;

namespace Kernel.Compiler
{
    /// <summary>
    /// The main compiler class - all the magic starts here ;)
    /// </summary>
    public class ILCompiler
    {
        /// <summary>
        /// Stores a reference to the method to call for outputting an error message.
        /// </summary>
        public OutputErrorDelegate OutputError;
        /// <summary>
        /// Stores a reference to the method to call for outputting a standard message.
        /// </summary>
        public OutputMessageDelegate OutputMessage;
        /// <summary>
        /// Stores a reference to the method to call for outputting a warning message.
        /// </summary>
        public OutputWarningDelegate OutputWarning;

        /// <summary>
        /// The compiler settings to use.
        /// </summary>
        public Settings TheSettings;
        /// <summary>
        /// The assembly manager to use. (Where "assembly" means a ".dll"/"library", not "assembler" the language.)
        /// </summary>
        private AssemblyManager TheAssemblyManager;
        /// <summary>
        /// The IL reader to use.
        /// </summary>
        private ILReader TheILReader;
        /// <summary>
        /// The IL scanner to use.
        /// </summary>
        private ILScanner TheILScanner;
        /// <summary>
        /// The ASM sequencer to use.
        /// </summary>
        private ASMSequencer TheASMSequencer;
        
        /// <summary>
        /// Initialises a new ILCompiler instance with the specified settings and empty output handlers.
        /// </summary>
        /// <param name="aSettings">The settings to use for the ILCompiler.</param>
        public ILCompiler(Settings aSettings)
            : this(aSettings, null, null, null)
        {
        }
        /// <summary>
        /// Initialises a new ILCompiler instance with the specified settings and output handlers.
        /// </summary>
        /// <param name="aSettings">The settings to use for the ILCompiler.</param>
        /// <param name="anOutputError">The reference to the method to call to output an error message.</param>
        /// <param name="anOutputMessage">The reference to the method to call to output a standard message.</param>
        /// <param name="anOutputWarning">The reference to the method to call to output a warning message.</param>
        public ILCompiler(Settings aSettings, 
                          OutputErrorDelegate anOutputError, 
                          OutputMessageDelegate anOutputMessage,
                          OutputWarningDelegate anOutputWarning)
        {
            TheSettings = aSettings;
            OutputError = anOutputError;
            OutputMessage = anOutputMessage;
            OutputWarning = anOutputWarning;

            if (OutputError == null)
            {
                //Prevents null reference exceptions
                OutputError = (Exception ex) =>
                {
                    //Empty placeholder function
                };
            }
            if (OutputMessage == null)
            {
                //Prevents null reference exceptions
                OutputMessage = (string message) =>
                {
                    //Empty placeholder function
                };
            }
            if (OutputWarning == null)
            {
                //Prevents null reference exceptions
                OutputWarning = (Exception ex) =>
                {
                    //Empty placeholder function
                };
            }
        }

        /// <summary>
        /// Initialises the compiler environment including initialising TheAssemblyManager.
        /// </summary>
        /// <returns>True if initialisation completed successfully. Otherwise false.</returns>
        private bool InitEnvironment()
        {
            bool OK = true;

            //Init the assembly manager
            TheAssemblyManager = new AssemblyManager();
            Assembly rootAssembly = null;
            //Get it to load the assembly specified in Settings
            try
            {
                rootAssembly = TheAssemblyManager.LoadAssembly(TheSettings[Settings.InputFileKey]);
                if (rootAssembly == null)
                {
                    OK = false;
                }

                //Debug database is still used even for release builds. 
                //  - At the time of writing, (6/6/14) the Type and ComplexTypeLinks tables were the only
                //    ones required during a release build.
                Debug.Data.DebugDatabase.Empty();
            }
            catch(Exception ex)
            {
                OK = false;
                OutputError(ex);
            }
            //Get it to load references (ignoring mscorlib and kernel compiler assemblies)
            if (OK)
            {
                try
                {
                    TheAssemblyManager.LoadReferences(rootAssembly);
                    TheAssemblyManager.LoadAllTypes();

                    string outputFilePath = TheSettings[Settings.OutputFileKey];
                    string executionPath = TheSettings[Settings.ToolsPathKey];
                    string Dia2DumpPath = Path.Combine(executionPath, @"Dia2Dump\Dia2Dump.exe");
                    string workingDir = Path.GetDirectoryName(outputFilePath);

                    //Debug data - takes the pdb file and dumps the source files and source lines info

                    //Need to dump data for all referenced DLLs as well as the main dll

                    foreach (Assembly anAssembly in TheAssemblyManager.Assemblies.Values)
                    {
                        string assemblyFileName = Path.Combine(Path.GetDirectoryName(anAssembly.Location), Path.GetFileNameWithoutExtension(anAssembly.Location));
                        string aPDBPathname = assemblyFileName + ".pdb";
                        string aPDBDumpPathname = assemblyFileName + ".pdb_dump";

                        if (File.Exists(aPDBDumpPathname))
                        {
                            File.Delete(aPDBDumpPathname);
                        }

                        if (!ExecuteProcess(workingDir, Dia2DumpPath, string.Format("-all \"{0}\"", aPDBPathname), "Dia2Dump", false, aPDBDumpPathname))
                        {
                            OutputError(new Exception("Failed to execute Dia2Dump. Debug information will be missing. " + assemblyFileName));
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    OK = false;
                    OutputError(ex);
                    foreach (Exception anEx in ex.LoaderExceptions)
                    {
                        OutputError(anEx);
                    }
                }
                catch (Exception ex)
                {
                    OK = false;
                    OutputError(ex);
                }
            }

            return OK;
        }
        /// <summary>
        /// Intialises TheILReader.
        /// </summary>
        /// <returns>True if initialisation completed successfully. Otherwise false.</returns>
        private bool InitILReader()
        {
            bool OK = true;
            try
            {
                TheILReader = new ILReader(TheSettings,
                                           TheAssemblyManager,
                                           OutputError,
                                           OutputMessage,
                                           OutputWarning);
            }
            catch (Exception ex)
            {
                OK = false;
                OutputError(ex);
            }
            return OK;
        }
        /// <summary>
        /// Intialises TheILScanner.
        /// </summary>
        /// <returns>True if initialisation completed successfully. Otherwise false.</returns>
        private bool InitILScanner()
        {
            bool OK = true;
            try
            {
                TheILScanner = new ILScanner(TheSettings,
                                             OutputError,
                                             OutputMessage,
                                             OutputWarning);
            }
            catch (Exception ex)
            {
                OK = false;
                OutputError(ex);
            }
            return OK;
        }
        /// <summary>
        /// Intialises TheASMSequencer.
        /// </summary>
        /// <returns>True if initialisation completed successfully. Otherwise false.</returns>
        private bool InitASMSequencer()
        {
            bool OK = true;
            try
            {
                TheASMSequencer = new ASMSequencer(TheSettings,
                                                   OutputError,
                                                   OutputMessage,
                                                   OutputWarning);
            }
            catch (Exception ex)
            {
                OK = false;
                OutputError(ex);
            }
            return OK;
        }

        /// <summary>
        /// Initialises the ILReader then executes the ILReader
        /// </summary>
        /// <returns>True if execution completed successfully. Otherwise false.</returns>
        /// <exception cref="System.Exception">
        /// Thrown if either the environment or the IL Readder fail to initialise.
        /// </exception>
        public bool ExecuteILReader()
        {
            bool OK = false;
            if (OK = InitEnvironment())
            {
                if (OK = InitILReader())
                {
                    OK = TheILReader.Execute();
                }
                else
                {
                    throw new Exception("ILCompiler failed to initialise ILReader!");
                }
            }
            else
            {
                throw new Exception("ILCompiler failed to initialise environment!");
            }
            return OK;
        }
        /// <summary>
        /// Initialises the ILScanner then executes the ILScanner. It is assumed the ILReader has been executed successfully.
        /// </summary>
        /// <returns>True if execution completed successfully. Otherwise false.</returns>
        public bool ExecuteILScanner()
        {
            bool OK = false;
            if(OK = InitILScanner())
            {
                List<Type> AllTypes = TheAssemblyManager.AllTypes;

                //Add in the system standard types
                AllTypes.Add(typeof(object));

                AllTypes.Add(typeof(float));
                AllTypes.Add(typeof(double));
                AllTypes.Add(typeof(decimal));
                AllTypes.Add(typeof(string));
                AllTypes.Add(typeof(IntPtr));

                AllTypes.Add(typeof(void));
                AllTypes.Add(typeof(bool));
                AllTypes.Add(typeof(byte));
                AllTypes.Add(typeof(sbyte));
                AllTypes.Add(typeof(char));
                AllTypes.Add(typeof(int));
                AllTypes.Add(typeof(long));
                AllTypes.Add(typeof(Int16));
                AllTypes.Add(typeof(Int32));
                AllTypes.Add(typeof(Int64));
                AllTypes.Add(typeof(UInt16));
                AllTypes.Add(typeof(UInt32));
                AllTypes.Add(typeof(UInt64));

                AllTypes.Add(typeof(void*));
                AllTypes.Add(typeof(bool*));
                AllTypes.Add(typeof(byte*));
                AllTypes.Add(typeof(sbyte*));
                AllTypes.Add(typeof(char*));
                AllTypes.Add(typeof(int*));
                AllTypes.Add(typeof(long*));
                AllTypes.Add(typeof(Int16*));
                AllTypes.Add(typeof(Int32*));
                AllTypes.Add(typeof(Int64*));
                AllTypes.Add(typeof(UInt16*));
                AllTypes.Add(typeof(UInt32*));
                AllTypes.Add(typeof(UInt64*));
                
                OK = TheILScanner.Execute(AllTypes, TheILReader.ILChunks, TheILReader.TheStaticConstructorDependencyTree);
            }
            return OK;
        }
        /// <summary>
        /// Initialises the ASMSequencer then executes the ASMSequencer. It is assumed the ILScanner has been executed successfully.
        /// It will then output all the ASM chunks from the ASMSequencer to file.
        /// </summary>
        /// <returns>True if execution completed successfully. Otherwise false.</returns>
        public bool ExecuteASMSequencer()
        {
            bool OK = false;
            if (OK = InitASMSequencer())
            {
                OK = TheASMSequencer.Execute(TheILScanner.ASMChunks);
                if (OK)
                {
                    //Output to file
                    string outputFilePath = TheSettings[Settings.OutputFileKey] + ".asm";
                    //Delete an existing output file so we start from scratch
                    if (File.Exists(outputFilePath))
                    {
                        File.Delete(outputFilePath);
                    }
                    //Create the ouptut file and open an output stream to it
                    using (StreamWriter writer = File.CreateText(outputFilePath))
                    {
                        try
                        {
                            int ASMLength = 0;
                            foreach (ASMChunk aChunk in TheASMSequencer.SequencedASMChunks)
                            {
                                if (aChunk.DBMethod != null)
                                {
                                    aChunk.DBMethod.ASMStartPos = ASMLength;
                                }

                                aChunk.ASM.AppendLine();
                                aChunk.ASM.AppendLine();
                                aChunk.Output(writer);

                                ASMLength += aChunk.ASM.Length;
                                if (aChunk.DBMethod != null)
                                {
                                    aChunk.DBMethod.ASMEndPos = ASMLength;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            OK = false;
                            OutputError(ex);
                        }
                    }
                }
            }
            return OK;
        }
        /// <summary>
        /// Executes NASM on the output file. It is assumed the output file now exists.
        /// </summary>
        /// <returns>True if execution completed successfully. Otherwise false.</returns>
        public bool ExecuteNASM()
        {
            bool OK = true;

            //Compile the .ASM file to .BIN file
            string outputFilePath = TheSettings[Settings.OutputFileKey];
            string executionPath = TheSettings[Settings.ToolsPathKey];
            string NasmPath = Path.Combine(executionPath, @"NAsm\nasm.exe");
            //Delete an existing output file so we start from scratch
            if (File.Exists(outputFilePath + ".obj"))
            {
                File.Delete(outputFilePath + ".obj");
            }
            
            OK = ExecuteProcess(Path.GetDirectoryName(outputFilePath), NasmPath, String.Format("-g -f {0} -o \"{1}\" -D{3}_COMPILATION \"{2}\"",
                                                  "elf",
                                                  outputFilePath + ".obj",
                                                  outputFilePath + ".asm",
                                                  "ELF"), "NASM");

            return OK;
        }

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
        private bool ExecuteProcess(string workingDir, string processFile, string args, string displayName, bool ignoreErrors = false, string outputMessagesToFileName = null)
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
            using (var process = new Process())
            {
                StreamWriter outputStream = null;

                if (!ignoreErrors)
                {
                    if (outputMessagesToFileName != null)
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
                                OutputError(new Exception(displayName + ": " + e.Data));
                            }
                        };
                        process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
                        {
                            if (e.Data != null)
                            {
                                OutputMessage(displayName + ": " + e.Data);
                            }
                        };
                    }
                }
                process.StartInfo = processStartInfo;
                process.Start();
                if (!ignoreErrors)
                {
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                }
                process.WaitForExit(15 * 60 * 1000); // wait 15 minutes max. for process to exit
                if (process.ExitCode != 0)
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                        OutputError(new Exception(displayName + " timed out."));
                    }
                    else
                    {
                        OutputError(new Exception("Error occurred while invoking " + displayName + "."));
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
         
        /// <summary>
        /// Finalises the compilation by turning the .BIn file into a usable .ISO file.
        /// </summary>
        /// <returns>True if finalisation completed successfully. Otherwise false.</returns>
        public bool Finalise()
        {
            //Convert the .BIN file to usable .ISO file

            Debug.Data.DebugDatabase.SubmitChanges();

            bool OK = true;

            //Compile the .ASM file to .BIN file
            string outputFilePath = TheSettings[Settings.OutputFileKey];
            string aIsoPathname = outputFilePath + ".iso";
            string aObjPathname = outputFilePath + ".obj";
            string aBinPathname = outputFilePath + ".bin";
            string aMapPathname = outputFilePath + ".map";
            string executionPath = TheSettings[Settings.ToolsPathKey];
            string compilerExecutionPath = Path.Combine(executionPath, @"ISO");
            string LdPath = Path.Combine(executionPath, @"cygwin\ld.exe");
            string ObjDumpPath = Path.Combine(executionPath, @"cygwin\objdump.exe");
            string workingDir = Path.GetDirectoryName(outputFilePath);


            //Delete an existing output file so we start from scratch
            if (File.Exists(aBinPathname))
            {
                File.Delete(aBinPathname);
            }
            OK = ExecuteProcess(workingDir, LdPath, String.Format("-Ttext 0x2000000 -Tdata 0x1000000 -e Kernel_Start -o '{0}' '{1}'",
                                                  aBinPathname,
                                                  aObjPathname), 
                                                  "Ld");

            if (OK)
            {
                if (File.Exists(aIsoPathname))
                {
                    File.Delete(aIsoPathname);
                }

                // We copy and rename in the process to FlingOS.bin becaue the .cfg is currently
                // hardcoded to FlingOS.bin.
                string finalOutputBin = Path.Combine(compilerExecutionPath, "FlingOS.bin");
                File.Copy(aBinPathname, finalOutputBin, true);

                string isoLinuxPath = Path.Combine(compilerExecutionPath, "isolinux.bin");
                File.SetAttributes(isoLinuxPath, FileAttributes.Normal);

                var xOptions = new Options()
                {
                    BootLoadSize = 4,
                    IsoFileName = aIsoPathname,
                    BootFileName = isoLinuxPath,
                    BootInfoTable = true
                };
                // TODO - Use move or see if we can do this without copying first the FlingOS.bin as it will start to get larger
                xOptions.IncludeFiles.Add(compilerExecutionPath);

                var xISO = new Iso9660Generator(xOptions);
                xISO.Generate();

                OK = true;
            }

            if (OK)
            {
                //Debug data - takes the bin file and dumps the address->label mappings
                
                if (File.Exists(aMapPathname))
                {
                    File.Delete(aMapPathname);
                }

                if (!ExecuteProcess(workingDir, ObjDumpPath, string.Format("--wide --syms \"{0}\"", aBinPathname), "ObjDump", false, aMapPathname))
                {
                    OutputError(new Exception("Failed to execute Obj Dump. Debug information will be missing."));
                }
            }

            return OK;
        }

        /// <summary>
        /// Cleans up any temporary files, directories or streams.
        /// </summary>
        public void Cleanup()
        {
            Debug.Data.DebugDatabase.Dispose();

            string outputFilePath = TheSettings[Settings.OutputFileKey];
            if (File.Exists(outputFilePath + ".obj"))
            {
                //OutputMessage("Deleting temp. file " + outputFilePath + ".obj");
                File.Delete(outputFilePath + ".obj");
            }
            if (File.Exists(outputFilePath + ".bin"))
            {
                //OutputMessage("Deleting temp. file " + outputFilePath + ".bin");
                File.Delete(outputFilePath + ".bin");
            }
            string xPath = Path.Combine(Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]), @"ISO");
            string xOutputBin = Path.Combine(xPath, "FlingOS.bin");
            if (File.Exists(xOutputBin))
            {
                //OutputMessage("Deleting temp. file " + outputFilePath + ".bin");
                File.Delete(xOutputBin);
            }
        }
    }
}
