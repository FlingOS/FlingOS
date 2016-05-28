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
using System.IO;
using System.Text;
using Drivers.Compiler.ASM;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
    public class LibraryFunctions : TargetArchitectureFunctions
    {
        public override void CleanUpAssemblyCode(ASMBlock TheBlock, ref string ASMText)
        {
        }

        /// <summary>
        ///     Executes NASM on the output file. It is assumed the output file now exists.
        /// </summary>
        /// <param name="inputFilePath">Path to the ASM file to process.</param>
        /// <param name="outputFilePath">Path to output the object file to.</param>
        /// <param name="OnComplete">Handler to call once NASM has completed. Default: null.</param>
        /// <param name="state">The state object to use when calling the OnComplete handler. Default: null.</param>
        /// <returns>True if execution completed successfully. Otherwise false.</returns>
        public override bool ExecuteAssemblyCodeCompiler(string inputFilePath, string outputFilePath,
            VoidDelegate OnComplete = null, object state = null)
        {
            bool OK = true;

            //Compile the .ASM file to .BIN file
            string ToolPath = Path.Combine(Options.ToolsPath, @"MIPS\mips-linux-gnu-as.exe");
            //Delete an existing output file so we start from scratch
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }
            if (!File.Exists(inputFilePath))
            {
                throw new NullReferenceException("ASM file does not exist! Path: \"" + inputFilePath + "\"");
            }

            string inputCommand = string.Format("-mips32 -Os -EL -o \"{1}\" \"{2}\"",
                "elf",
                outputFilePath,
                inputFilePath,
                "ELF");

            //Logger.LogMessage(inputFilePath, 0, inputCommand);

            OK = Utilities.ExecuteProcess(Path.GetDirectoryName(outputFilePath), ToolPath, inputCommand, "MIPS:GCC",
                false,
                null,
                OnComplete,
                state);

            return OK;
        }

        public override bool LinkISO(ILLibrary TheLibrary, LinkInformation LinkInfo)
        {
            //TODO: Update this to match the LinkISO of the x86 kernel

            bool OK = true;

            string BinPath = LinkInfo.ISOPath.Replace(".iso", ".bin");
            string ElfPath = LinkInfo.ISOPath.Replace(".iso", ".elf");

            StreamWriter ASMWriter = new StreamWriter(LinkInfo.ASMPath, false);

            StringBuilder CommandLineArgsBuilder = new StringBuilder();
            CommandLineArgsBuilder.Append("--fatal-warnings -EL -Os -T \"" + LinkInfo.LinkScriptPath + "\" -o \"" +
                                          ElfPath + "\"");

            StringBuilder LinkScript = new StringBuilder();
            LinkScript.Append(@"ENTRY(Kernel_Start)
OUTPUT_ARCH(mips)

SECTIONS {
   . = 0x" + Options.BaseAddress.ToString("X8") + @";

   Kernel_MemStart = .;

   .text : AT(ADDR(.text) - " + Options.LoadOffset + @") {
");

            for (int i = 0; i < LinkInfo.SequencedASMBlocks.Count; i++)
            {
                LinkScript.AppendLine(string.Format("       \"{0}\" (.text);",
                    LinkInfo.SequencedASMBlocks[i].ObjectOutputFilePath));
                ASMWriter.WriteLine(File.ReadAllText(LinkInfo.SequencedASMBlocks[i].ASMOutputFilePath));
            }
            LinkScript.AppendLine(@"
          * (.text);
   }

    . = ALIGN(0x4);
   .data : AT(ADDR(.data) - " + Options.LoadOffset + @") {
");

            for (int i = 0; i < LinkInfo.SequencedASMBlocks.Count; i++)
            {
                LinkScript.AppendLine(string.Format("       \"{0}\" (.data);",
                    LinkInfo.SequencedASMBlocks[i].ObjectOutputFilePath));
            }
            LinkScript.AppendLine(@"
   }

   . = ALIGN(0x4);
   __bss_start = .;
   .bss : AT(ADDR(.bss) - " + Options.LoadOffset + @") {
");

            for (int i = 0; i < LinkInfo.SequencedASMBlocks.Count; i++)
            {
                LinkScript.AppendLine(string.Format("       \"{0}\" (.bss);",
                    LinkInfo.SequencedASMBlocks[i].ObjectOutputFilePath));
            }
            LinkScript.AppendLine(@"
   }
   __bss_end = .;

   Kernel_MemEnd = .;

}
");

            ASMWriter.Close();

            File.WriteAllText(LinkInfo.LinkScriptPath, LinkScript.ToString());
            OK = Utilities.ExecuteProcess(LinkInfo.LdWorkingDir,
                Path.Combine(LinkInfo.ToolsPath, @"MIPS\mips-linux-gnu-ld.exe"), CommandLineArgsBuilder.ToString(), "Ld");

            if (OK)
            {
                if (File.Exists(BinPath))
                {
                    File.Delete(BinPath);
                }

                OK = Utilities.ExecuteProcess(Options.OutputPath,
                    Path.Combine(LinkInfo.ToolsPath, @"MIPS\mips-linux-gnu-objcopy.exe"),
                    string.Format("-O binary \"{0}\" \"{1}\"", ElfPath, BinPath), "MIPS:ObjCopy");

                if (OK)
                {
                    if (File.Exists(LinkInfo.MapPath))
                    {
                        File.Delete(LinkInfo.MapPath);
                    }

                    OK = Utilities.ExecuteProcess(Options.OutputPath,
                        Path.Combine(LinkInfo.ToolsPath, @"MIPS\mips-linux-gnu-objdump.exe"),
                        string.Format("--wide --syms \"{0}\"", ElfPath), "MIPS:ObjDump", false, LinkInfo.MapPath);
                }
            }

            return OK;
        }

        public override bool LinkELF(ILLibrary TheLibrary, LinkInformation LinkInfo)
        {
            return false;
        }
    }
}