using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Drivers.Compiler.Architectures.x86
{
    public class LibraryFunctions : TargetArchitectureFunctions
    {
        public override void CleanUpAssemblyCode(ASM.ASMBlock TheBlock, ref string ASMText)
        {
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
        }

        /// <summary>
        /// Executes NASM on the output file. It is assumed the output file now exists.
        /// </summary>
        /// <param name="inputFilePath">Path to the ASM file to process.</param>
        /// <param name="outputFilePath">Path to output the object file to.</param>
        /// <param name="OnComplete">Handler to call once NASM has completed. Default: null.</param>
        /// <param name="state">The state object to use when calling the OnComplete handler. Default: null.</param>
        /// <returns>True if execution completed successfully. Otherwise false.</returns>
        public override bool ExecuteAssemblyCodeCompiler(string inputFilePath, string outputFilePath, VoidDelegate OnComplete = null, object state = null)
        {
            bool OK = true;

            //Compile the .ASM file to .BIN file
            string NasmPath = Path.Combine(Options.ToolsPath, @"NASM\nasm.exe");
            //Delete an existing output file so we start from scratch
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }
            if (!File.Exists(inputFilePath))
            {
                throw new NullReferenceException("ASM file does not exist! Path: \"" + inputFilePath + "\"");
            }

            string inputCommand = String.Format("-g -f {0} -o \"{1}\" -D{3}_COMPILATION \"{2}\"",
                                                  "elf",
                                                  outputFilePath,
                                                  inputFilePath,
                                                  "ELF");

            //Logger.LogMessage(inputFilePath, 0, inputCommand);

            OK = Utilities.ExecuteProcess(Path.GetDirectoryName(outputFilePath), NasmPath, inputCommand, "NASM",
                                                  false,
                                                  null,
                                                  OnComplete,
                                                  state);

            return OK;
        }
    }
}
