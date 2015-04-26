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
    
#define NASM_ASYNC
#undef NASM_ASYNC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Drivers.Compiler.ASM
{
    public static class ASMProcessor
    {
        public static CompileResult Process(ASMLibrary TheLibrary)
        {
            CompileResult result = CompileResult.OK;

            if (TheLibrary.ASMProcessed)
            {
                return result;
            }
            TheLibrary.ASMProcessed = true;
            
            int MaxConcurrentNASMProcesses = Environment.ProcessorCount;
            List<List<ASMBlock>> NASMLabourDivision = new List<List<ASMBlock>>();
            for (int i = 0; i < MaxConcurrentNASMProcesses; i++)
            {
                NASMLabourDivision.Add(new List<ASMBlock>());
            }

            int num = 0;
            foreach (ASMBlock aBlock in TheLibrary.ASMBlocks)
            {
                ProcessBlock(aBlock);

                NASMLabourDivision[num % MaxConcurrentNASMProcesses].Add(aBlock);

                num++;
            }

#if NASM_ASYNC
            List<bool> Completed = new List<bool>();
            for (int i = 0; i < MaxConcurrentNASMProcesses; i++)
            {
                Completed.Add(false);
                ExecuteNASMAsync(NASMLabourDivision[i],
                    delegate(object state)
                    {
                        Completed[(int)state] = true;
                    },
                    i);
            }

            for (int i = 0; i < MaxConcurrentNASMProcesses; i++)
            {
                while (!Completed[i])
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
#else

            for (int i = 0; i < MaxConcurrentNASMProcesses; i++)
            {
                ExecuteNASMSync(NASMLabourDivision[i]);
            }

#endif
            
            return result;
        }

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
                    ASMPlugPath += ".x86_32.asm";
                }
                else
                {
                    ASMPlugPath += "." + Options.TargetArchitecture + ".asm";
                }
                ASMText = File.ReadAllText(ASMPlugPath);

                ASMText = ASMText.Replace("%KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD%", IL.ILLibrary.SpecialMethods[typeof(Attributes.CallStaticConstructorsMethodAttribute)].First().ID);
                if (IL.ILLibrary.SpecialMethods.ContainsKey(typeof(Attributes.MainMethodAttribute)))
                {
                    ASMText = ASMText.Replace("%KERNEL_MAIN_METHOD%", IL.ILLibrary.SpecialMethods[typeof(Attributes.MainMethodAttribute)].First().ID);
                }
            }
            else
            {
                foreach (ASMOp anASMOp in TheBlock.ASMOps)
                {
                    if (anASMOp.RequiresILLabel)
                    {
                        ASMText += TheBlock.GenerateILOpLabel(anASMOp.ILLabelPosition, "") + ":\r\n";
                    }
                    ASMText += anASMOp.Convert(TheBlock) + "\r\n";
                }
            }

            // Create lists of extern and global labels
            TheBlock.ExternalLabels.Clear();
            List<string> ExternLines = ASMText.Replace("\r", "")
                                              .Split('\n')
                                              .Where(x => x.ToLower().Contains("extern "))
                                              .Select(x => x.Split(' ')[1].Split(':')[0])
                                              .ToList();
            TheBlock.ExternalLabels.AddRange(ExternLines);

            TheBlock.GlobalLabels.Clear();
            List<string> GlobalLines = ASMText.Replace("\r", "")
                                              .Split('\n')
                                              .Where(x => x.ToLower().Contains("global "))
                                              .Select(x => x.Split(' ')[1].Split(':')[0])
                                              .ToList();
            TheBlock.GlobalLabels.AddRange(GlobalLines);

            ASMText = ASMText.Replace("GLOBAL ", "global ");
            ASMText = ASMText.Replace("EXTERN ", "extern ");

            string FileName = Utilities.CleanFileName(Guid.NewGuid().ToString() + "." + Options.TargetArchitecture) + ".asm";
            string OutputPath = GetASMOutputPath();
            FileName = Path.Combine(OutputPath, FileName);
            TheBlock.OutputFilePath = FileName;
            File.WriteAllText(FileName, ASMText);
        }

        private static void ExecuteNASMAsync(List<ASMBlock> Blocks, VoidDelegate OnComplete, object aState)
        {
            string ASMOutputPath = GetASMOutputPath();
            string ObjectsOutputPath = GetObjectsOutputPath();
            VoidDelegate onComplete = null;
            onComplete = delegate(object state)
            {
                int index = (int)state;
                if (index < Blocks.Count)
                {
                    string inputPath = Blocks[index].OutputFilePath;
                    string outputPath = inputPath.Replace(ASMOutputPath, ObjectsOutputPath).Replace(".asm", ".o");

                    try
                    {
                        ExecuteNASM(inputPath, outputPath, onComplete, index + 1);

                        Blocks[index].OutputFilePath = outputPath;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(Errors.ASMCompiler_NASMException_ErrorCode, inputPath, 0,
                            string.Format(Errors.ErrorMessages[Errors.ASMCompiler_NASMException_ErrorCode], inputPath));
                    }
                }
                else
                {
                    OnComplete(aState);
                }
            };
            onComplete(0);
        }

        private static void ExecuteNASMSync(List<ASMBlock> Blocks)
        {
            string ASMOutputPath = GetASMOutputPath();
            string ObjectsOutputPath = GetObjectsOutputPath();

            for (int index = 0; index < Blocks.Count; index++)
            {
                string inputPath = Blocks[index].OutputFilePath;
                string outputPath = inputPath.Replace(ASMOutputPath, ObjectsOutputPath).Replace(".asm", ".o");

                try
                {
                    ExecuteNASM(inputPath, outputPath);

                    Blocks[index].OutputFilePath = outputPath;
                }
                catch(Exception ex)
                {
                    Logger.LogError(Errors.ASMCompiler_NASMException_ErrorCode, inputPath, 0,
                        string.Format(Errors.ErrorMessages[Errors.ASMCompiler_NASMException_ErrorCode], inputPath));
                }
            }
        }

        private static bool CleanedASMOutputFolder = false;
        private static string GetASMOutputPath()
        {
            string OutputPath = Path.Combine(Options.OutputPath, "DriversCompiler\\ASM");
            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
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
        private static bool CleanedObjectsOutputFolder = false;
        private static string GetObjectsOutputPath()
        {
            string OutputPath = Path.Combine(Options.OutputPath, "DriversCompiler\\Objects");
            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
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


        /// <summary>
        /// Executes NASM on the output file. It is assumed the output file now exists.
        /// </summary>
        /// <returns>True if execution completed successfully. Otherwise false.</returns>
        private static bool ExecuteNASM(string inputFilePath, string outputFilePath, VoidDelegate OnComplete = null, object state = null)
        {
            bool OK = true;

            //Compile the .ASM file to .BIN file
            string NasmPath = Path.Combine(Options.ToolsPath, @"NASM\nasm.exe");
            //Delete an existing output file so we start from scratch
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }

            OK = Utilities.ExecuteProcess(Path.GetDirectoryName(outputFilePath), NasmPath, String.Format("-g -f {0} -o \"{1}\" -D{3}_COMPILATION \"{2}\"",
                                                  "elf",
                                                  outputFilePath,
                                                  inputFilePath,
                                                  "ELF"), "NASM",
                                                  false,
                                                  null,
                                                  OnComplete,
                                                  state);

            return OK;
        }

    }
}
