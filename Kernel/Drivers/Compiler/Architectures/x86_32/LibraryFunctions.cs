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
