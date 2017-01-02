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

#define COMPILER_ASYNC
#undef COMPILER_ASYNC

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Drivers.Compiler.ASM.ASMOps;
using Drivers.Compiler.Attributes;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.ASM
{
    /// <summary>
    ///     The ASM Processor manages converting ASM blocks into actual assembly code, saving that code
    ///     to files and then executing a build tool to convert assembly code into ELF binaries.
    /// </summary>
    public static class ASMProcessor
    {
        /// <summary>
        ///     Whether the output ASM folder has been cleaned or not.
        /// </summary>
        /// <remarks>
        ///     Prevents the compiler cleaning the output folder half-way through the compile process
        ///     i.e. between processing libraries.
        /// </remarks>
        private static bool CleanedASMOutputFolder;

        /// <summary>
        ///     Whether the output Objects folder has been cleaned or not.
        /// </summary>
        /// <remarks>
        ///     Prevents the compiler cleaning the output folder half-way through the compile process
        ///     i.e. between processing libraries.
        /// </remarks>
        private static bool CleanedObjectsOutputFolder;

        /// <summary>
        ///     Processes the given ASM library.
        /// </summary>
        /// <param name="TheLibrary">The library to process.</param>
        /// <returns>Always CompileResult.OK. In all other cases, exceptions are thrown.</returns>
        public static CompileResult Process(ASMLibrary TheLibrary)
        {
            CompileResult result = CompileResult.OK;

            if (TheLibrary.ASMProcessed)
            {
                return result;
            }
            TheLibrary.ASMProcessed = true;

            int MaxConcurrentCompilerProcesses = Environment.ProcessorCount;
            List<List<ASMBlock>> CompilerLabourDivision = new List<List<ASMBlock>>();
            for (int i = 0; i < MaxConcurrentCompilerProcesses; i++)
            {
                CompilerLabourDivision.Add(new List<ASMBlock>());
            }

            int num = 0;
            foreach (ASMBlock aBlock in TheLibrary.ASMBlocks)
            {
                ProcessBlock(aBlock);

                if (aBlock.ASMOutputFilePath != null)
                {
                    CompilerLabourDivision[num%MaxConcurrentCompilerProcesses].Add(aBlock);
                    num++;
                }
            }

            TheLibrary.ASMBlocks.RemoveAll(x => x.ASMOutputFilePath == null);

#if COMPILER_ASYNC
            List<bool> Completed = new List<bool>();
            for (int i = 0; i < MaxConcurrentCompilerProcesses; i++)
            {
                Completed.Add(false);
                ExecuteAssemblyCodeCompilerAsync(CompilerLabourDivision[i],
                    delegate(object state)
                    {
                        Completed[(int)state] = true;
                    },
                    i);
            }

            for (int i = 0; i < MaxConcurrentCompilerProcesses; i++)
            {
                while (!Completed[i])
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
#else

            for (int i = 0; i < MaxConcurrentCompilerProcesses; i++)
            {
                ExecuteAssemblyCodeCompilerSync(CompilerLabourDivision[i]);
            }

#endif

            return result;
        }

        /// <summary>
        ///     Processes the given ASM block.
        /// </summary>
        /// <param name="TheBlock">The ASM block to process.</param>
        private static void ProcessBlock(ASMBlock TheBlock)
        {
            string ASMText = "";

            if (TheBlock.Plugged)
            {
                string ASMPlugPath = Path.Combine(Options.OutputPath, TheBlock.PlugPath);
                // Legacy file name support
                if (Options.TargetArchitecture == "x86" &&
                    !File.Exists(ASMPlugPath + "." + Options.TargetArchitecture + ".asm"))
                {
                    if (!File.Exists(ASMPlugPath + ".x86_32.asm"))
                    {
                        throw new FileNotFoundException(
                            "Plug file not found! File name: " + ASMPlugPath + "." + Options.TargetArchitecture + ".asm",
                            ASMPlugPath + "." + Options.TargetArchitecture + ".asm");
                    }
                    ASMPlugPath += ".x86_32.asm";
                }
                else
                {
                    ASMPlugPath += "." + Options.TargetArchitecture + ".asm";
                }

                if (!File.Exists(ASMPlugPath))
                {
                    throw new FileNotFoundException("Plug file not found! File name: " + ASMPlugPath, ASMPlugPath);
                }

                ASMText = File.ReadAllText(ASMPlugPath);

                ASMText = ASMText.Replace("%KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD%",
                    ILLibrary.SpecialMethods[typeof(CallStaticConstructorsMethodAttribute)].First().ID);
                if (ILLibrary.SpecialMethods.ContainsKey(typeof(MainMethodAttribute)))
                {
                    ASMText = ASMText.Replace("%KERNEL_MAIN_METHOD%",
                        ILLibrary.SpecialMethods[typeof(MainMethodAttribute)].First().ID);
                }
            }
            else
            {
                foreach (ASMOp anASMOp in TheBlock.ASMOps)
                {
                    if (anASMOp.RequiresILLabel)
                    {
                        ASMText +=
                            ((ASMLabel)TargetArchitecture.CreateASMOp(OpCodes.Label, anASMOp.ILLabelPosition, ""))
                                .Convert(TheBlock) + "\r\n";
                    }

                    if (anASMOp is ASMLabel)
                    {
                        ASMLabel labelOp = (ASMLabel)anASMOp;
                        if (labelOp.IsDebugOp)
                        {
                            DebugDataWriter.AddDebugOp(TheBlock.OriginMethodInfo.ID,
                                TheBlock.GenerateILOpLabel(labelOp.ILPosition, labelOp.Extension));
                        }
                    }

                    ASMText += anASMOp.Convert(TheBlock) + "\r\n";
                }
            }

            TargetArchitecture.TargetFunctions.CleanUpAssemblyCode(TheBlock, ref ASMText);

            if (!string.IsNullOrWhiteSpace(ASMText))
            {
                string FileName =
                    Utilities.CleanFileName(Guid.NewGuid() + "." + Options.TargetArchitecture) + ".s";
                string OutputPath = GetASMOutputPath();
                FileName = Path.Combine(OutputPath, FileName);
                TheBlock.ASMOutputFilePath = FileName;
                File.WriteAllText(FileName, ASMText);
            }
            else
            {
                TheBlock.ASMOutputFilePath = null;
            }
        }

        /// <summary>
        ///     Executes the target architecture's assembly code compiler (e.g. NASM) asynchronously.
        /// </summary>
        /// <param name="Blocks">The blocks to execute the compiler tool for.</param>
        /// <param name="OnComplete">Method to call when the compiler tool has finished executing for all the blocks.</param>
        /// <param name="aState">The state object to use when calling the OnComplete method.</param>
        private static void ExecuteAssemblyCodeCompilerAsync(List<ASMBlock> Blocks, VoidDelegate OnComplete,
            object aState)
        {
            string ASMOutputPath = GetASMOutputPath();
            string ObjectsOutputPath = GetObjectsOutputPath();
            VoidDelegate onComplete = null;
            onComplete = delegate(object state)
            {
                int index = (int)state;
                if (index < Blocks.Count)
                {
                    string inputPath = Blocks[index].ASMOutputFilePath;
                    string outputPath = inputPath.Replace(ASMOutputPath, ObjectsOutputPath).Replace(".s", ".o");

                    try
                    {
                        TargetArchitecture.TargetFunctions.ExecuteAssemblyCodeCompiler(inputPath, outputPath, onComplete,
                            index + 1);

                        Blocks[index].ObjectOutputFilePath = outputPath;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(Errors.ASMCompiler_ASMCodeCompilerException_ErrorCode, inputPath, 0,
                            string.Format(Errors.ErrorMessages[Errors.ASMCompiler_ASMCodeCompilerException_ErrorCode],
                                inputPath, ex.Message));
                    }
                }
                else
                {
                    OnComplete(aState);
                }
            };
            onComplete(0);
        }

        /// <summary>
        ///     Executes the target architecture's assembly code compiler (e.g. NASM) synchronously.
        /// </summary>
        /// <param name="Blocks">The blocks to execute the compiler tool for.</param>
        private static void ExecuteAssemblyCodeCompilerSync(List<ASMBlock> Blocks)
        {
            string ASMOutputPath = GetASMOutputPath();
            string ObjectsOutputPath = GetObjectsOutputPath();

            for (int index = 0; index < Blocks.Count; index++)
            {
                string inputPath = Blocks[index].ASMOutputFilePath;
                string outputPath = inputPath.Replace(ASMOutputPath, ObjectsOutputPath).Replace(".s", ".o");

                try
                {
                    TargetArchitecture.TargetFunctions.ExecuteAssemblyCodeCompiler(inputPath, outputPath);

                    Blocks[index].ObjectOutputFilePath = outputPath;
                }
                catch (Exception ex)
                {
                    Logger.LogError(Errors.ASMCompiler_ASMCodeCompilerException_ErrorCode, inputPath, 0,
                        string.Format(Errors.ErrorMessages[Errors.ASMCompiler_ASMCodeCompilerException_ErrorCode],
                            inputPath, ex.Message));
                }
            }
        }

        /// <summary>
        ///     Gets the output path for the directory to save the ASM files into. Also, creates the output
        ///     directory if it doesn't exist or cleans the output directory if it hasn't already been cleaned.
        /// </summary>
        /// <remarks>
        ///     Currently, the path provided is the build directory (e.g. "bin\Debug") added to "DriversCompiler\ASM"
        ///     giving: "bin\Debug\DriversCompiler\ASM".
        /// </remarks>
        /// <returns>The path.</returns>
        private static string GetASMOutputPath()
        {
            string OutputPath = Path.Combine(Options.OutputPath, "DriversCompiler\\ASM");
            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
                CleanedASMOutputFolder = true;
            }
            else
            {
                if (!CleanedASMOutputFolder)
                {
                    Directory.Delete(OutputPath, true);
                    Directory.CreateDirectory(OutputPath);
                    CleanedASMOutputFolder = true;
                }
            }
            return OutputPath;
        }

        /// <summary>
        ///     Gets the output path for the directory to save the object files into. Also, creates the output
        ///     directory if it doesn't exist or cleans the output directory if it hasn't already been cleaned.
        /// </summary>
        /// <remarks>
        ///     Currently, the path provided is the build directory (e.g. "bin\Debug") added to "DriversCompiler\Objects"
        ///     giving: "bin\Debug\DriversCompiler\Objects".
        /// </remarks>
        /// <returns>The path.</returns>
        private static string GetObjectsOutputPath()
        {
            string OutputPath = Path.Combine(Options.OutputPath, "DriversCompiler\\Objects");
            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
                CleanedObjectsOutputFolder = true;
            }
            else
            {
                if (!CleanedObjectsOutputFolder)
                {
                    Directory.Delete(OutputPath, true);
                    Directory.CreateDirectory(OutputPath);
                    CleanedObjectsOutputFolder = true;
                }
            }
            return OutputPath;
        }
    }
}