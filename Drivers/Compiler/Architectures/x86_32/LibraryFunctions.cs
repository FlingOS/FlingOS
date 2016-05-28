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
using System.IO;
using System.Linq;
using System.Text;
using Drivers.Compiler.ASM;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.x86
{
    public class LibraryFunctions : TargetArchitectureFunctions
    {
        public override void CleanUpAssemblyCode(ASMBlock TheBlock, ref string ASMText)
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

            string inputCommand = string.Format("-g -f {0} -o \"{1}\" -D{3}_COMPILATION \"{2}\"",
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

        public override bool LinkISO(ILLibrary TheLibrary, LinkInformation LinkInfo)
        {
            bool OK = true;

            StreamWriter ASMWriter = new StreamWriter(LinkInfo.ASMPath, false);

            StringBuilder CommandLineArgsBuilder = new StringBuilder();
            CommandLineArgsBuilder.Append("--fatal-warnings -T \"" + LinkInfo.LinkScriptPath + "\" -o \"" +
                                          LinkInfo.BinPath + "\"");

            StringBuilder LinkScript = new StringBuilder();
            LinkScript.Append(@"ENTRY(Kernel_Start)
OUTPUT_FORMAT(elf32-i386)

GROUP(");

            LinkScript.Append(string.Join(" ", LinkInfo.SequencedASMBlocks
                .Where(x => File.Exists(x.ObjectOutputFilePath))
                .Select(x => "\"" + x.ObjectOutputFilePath + "\"")));

            LinkScript.AppendLine(@")

SECTIONS {
   /* The kernel will live at 3GB + 1MB in the virtual
      address space, which will be mapped to 1MB in the
      physical address space. */
   . = 0x" + Options.BaseAddress.ToString("X8") + @";

   Kernel_MemStart = .;

   .text : AT(ADDR(.text) - " + Options.LoadOffset + @") {
");

            for (int i = 0; i < LinkInfo.SequencedASMBlocks.Count; i++)
            {
                if (LinkInfo.SequencedASMBlocks[i].PageAlign)
                {
                    LinkScript.AppendLine(". = ALIGN(0x1000);");
                    LinkScript.AppendLine(LinkInfo.SequencedASMBlocks[i].PageAlignLabel + "_Code = .;");
                }
                LinkScript.AppendLine(string.Format("       \"{0}\" (.text);",
                    LinkInfo.SequencedASMBlocks[i].ObjectOutputFilePath));
                ASMWriter.WriteLine(File.ReadAllText(LinkInfo.SequencedASMBlocks[i].ASMOutputFilePath));
            }
            LinkScript.AppendLine(@"
          * (.text);
          * (.rodata*);
   }

   . = ALIGN(0x1000);
    data_start = .;
   .data : AT(ADDR(.data) - " + Options.LoadOffset + @") {
");

            for (int i = 0; i < LinkInfo.SequencedASMBlocks.Count; i++)
            {
                if (LinkInfo.SequencedASMBlocks[i].PageAlign)
                {
                    LinkScript.AppendLine(". = ALIGN(0x1000);");
                    LinkScript.AppendLine(LinkInfo.SequencedASMBlocks[i].PageAlignLabel + "_Data = .;");
                }
                LinkScript.AppendLine(string.Format("       \"{0}\" (.data);",
                    LinkInfo.SequencedASMBlocks[i].ObjectOutputFilePath));
            }
            LinkScript.AppendLine(@"
   }
    data_end = .;

   . = ALIGN(0x1000);
    __bss_start = .;
   .bss : AT(ADDR(.bss) - " + Options.LoadOffset + @") {
");

            for (int i = 0; i < LinkInfo.SequencedASMBlocks.Count; i++)
            {
                if (LinkInfo.SequencedASMBlocks[i].PageAlign)
                {
                    LinkScript.AppendLine(". = ALIGN(0x1000);");
                    LinkScript.AppendLine(LinkInfo.SequencedASMBlocks[i].PageAlignLabel + "_BSS = .;");
                }
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
            OK = Utilities.ExecuteProcess(LinkInfo.LdWorkingDir, Path.Combine(LinkInfo.ToolsPath, @"Cygwin\ld.exe"),
                CommandLineArgsBuilder.ToString(), "Ld");

            if (OK)
            {
                if (File.Exists(LinkInfo.ISOPath))
                {
                    File.Delete(LinkInfo.ISOPath);
                }

                OK = Utilities.ExecuteProcess(Options.OutputPath, LinkInfo.ISOGenPath,
                    string.Format("4 \"{0}\" \"{1}\" true \"{2}\"", LinkInfo.ISOPath, LinkInfo.ISOLinuxPath,
                        LinkInfo.ISODirPath), "ISO9660Generator");

                if (OK)
                {
                    if (File.Exists(LinkInfo.MapPath))
                    {
                        File.Delete(LinkInfo.MapPath);
                    }

                    OK = Utilities.ExecuteProcess(Options.OutputPath,
                        Path.Combine(LinkInfo.ToolsPath, @"Cygwin\objdump.exe"),
                        string.Format("--wide --syms \"{0}\"", LinkInfo.BinPath), "ObjDump", false, LinkInfo.MapPath);
                }
            }

            return OK;
        }

        public override bool LinkELF(ILLibrary TheLibrary, LinkInformation LinkInfo)
        {
            StringBuilder CommandLineArgsBuilder = new StringBuilder();
            if (!LinkInfo.ExecutableOutput)
            {
                CommandLineArgsBuilder.Append("-shared ");
            }
            CommandLineArgsBuilder.Append("-L .\\Output -T \"" + LinkInfo.LinkScriptPath + "\" -o \"" + LinkInfo.BinPath +
                                          "\"");

            StreamWriter ASMWriter = new StreamWriter(LinkInfo.ASMPath, false);

            StringBuilder LinkScript = new StringBuilder();
            LinkScript.Append((LinkInfo.ExecutableOutput ? "ENTRY(" + LinkInfo.EntryPoint + ")\r\n" : "") +
                              @"GROUP(");

            LinkScript.Append(string.Join(" ", LinkInfo.SequencedASMBlocks
                .Where(x => File.Exists(x.ObjectOutputFilePath))
                .Select(x => "\"" + x.ObjectOutputFilePath + "\"")));

            LinkScript.Append(@")

");
            if (LinkInfo.depLibNames.Count > 0)
            {
                LinkScript.Append("GROUP(");
                LinkScript.Append(string.Join(" ", LinkInfo.depLibNames.Select(x => "-l" + x)));
                LinkScript.Append(")");
            }

            LinkScript.AppendLine(@"

SECTIONS {
   . = 0x" + (0x40000000 + LinkInfo.depLibNames.Count*0x1000).ToString("X2") + @";

   .text : {
");

            for (int i = 0; i < LinkInfo.SequencedASMBlocks.Count; i++)
            {
                if (LinkInfo.SequencedASMBlocks[i].PageAlign)
                {
                    LinkScript.AppendLine(". = ALIGN(0x1000);");
                    LinkScript.AppendLine(LinkInfo.SequencedASMBlocks[i].PageAlignLabel + "_Code = .;");
                }
                LinkScript.AppendLine(string.Format("       \"{0}\" (.text);",
                    LinkInfo.SequencedASMBlocks[i].ObjectOutputFilePath));
                ASMWriter.WriteLine(File.ReadAllText(LinkInfo.SequencedASMBlocks[i].ASMOutputFilePath));
            }


            LinkScript.AppendLine(@"
          * (.text);
          * (.rodata*);
   }

   . = ALIGN(0x1000);
   .data : {
");

            for (int i = 0; i < LinkInfo.SequencedASMBlocks.Count; i++)
            {
                if (LinkInfo.SequencedASMBlocks[i].PageAlign)
                {
                    LinkScript.AppendLine(". = ALIGN(0x1000);");
                    LinkScript.AppendLine(LinkInfo.SequencedASMBlocks[i].PageAlignLabel + "_Data = .;");
                }
                LinkScript.AppendLine(string.Format("       \"{0}\" (.data);",
                    LinkInfo.SequencedASMBlocks[i].ObjectOutputFilePath));
            }
            LinkScript.AppendLine(@"
   }

   . = ALIGN(0x1000);
   __bss_start = .;
   .bss : {
");

            for (int i = 0; i < LinkInfo.SequencedASMBlocks.Count; i++)
            {
                if (LinkInfo.SequencedASMBlocks[i].PageAlign)
                {
                    LinkScript.AppendLine(". = ALIGN(0x1000);");
                    LinkScript.AppendLine(LinkInfo.SequencedASMBlocks[i].PageAlignLabel + "_BSS = .;");
                }
                LinkScript.AppendLine(string.Format("       \"{0}\" (.bss);",
                    LinkInfo.SequencedASMBlocks[i].ObjectOutputFilePath));
            }
            LinkScript.AppendLine(@"
   }
   __bss_end = .;
}
");
            ASMWriter.Close();

            File.WriteAllText(LinkInfo.LinkScriptCmdPath, CommandLineArgsBuilder.ToString());
            File.WriteAllText(LinkInfo.LinkScriptPath, LinkScript.ToString());
            return Utilities.ExecuteProcess(LinkInfo.LdWorkingDir, Path.Combine(LinkInfo.ToolsPath, @"Cygwin\ld.exe"),
                CommandLineArgsBuilder.ToString(), "Ld");
        }
    }
}