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

namespace Drivers.Compiler
{
    public static class LinkManager
    {
        public static CompileResult Link(IL.ILLibrary TheLibrary)
        {
            bool OK = true;
            
            // If: Link to ELF and Libraries
            //      - Link sub-libs to .a files
            //      - Link main lib to .elf file (if present)
            // If: Link to ISO
            //      - Generate basic link-script
            //      - Generate full link-script by inserting necessary file location instructions for all object files
            //      - Execute ld to build bin file
            //      - Execute ISO9660Generator to build .ISO file

            if (Options.LinkMode == Options.LinkModes.ELF)
            {
                // Check for main method. If found, that library gets linked to Executable not Shared Lib

                List<string> depLibNames = new List<string>();
                foreach (IL.ILLibrary depLib in TheLibrary.Dependencies)
                {
                    depLibNames.Add(Utilities.CleanFileName(depLib.TheAssembly.GetName().Name));
                    
                    OK = OK && (Link(depLib) == CompileResult.OK);
                    if (!OK)
                    {
                        break;
                    }
                }

                if (!OK)
                {
                    return CompileResult.Fail;
                }

                List<ASM.ASMBlock> SequencedASMBlocks = new List<ASM.ASMBlock>();
                SequencedASMBlocks.AddRange(TheLibrary.TheASMLibrary.ASMBlocks);
                SequencedASMBlocks.Sort(GetOrder);

                // Find start method if any, use as ENTRY point
                bool ExecutableOutput = false;
                string EntryPoint = null;
                if (IL.ILLibrary.SpecialMethods.ContainsKey(typeof(Attributes.MainMethodAttribute)))
                {
                    Types.MethodInfo mainMethodInfo = IL.ILLibrary.SpecialMethods[typeof(Attributes.MainMethodAttribute)].First();
                    IL.ILBlock mainMethodBlock = TheLibrary.GetILBlock(mainMethodInfo, false);
                    if (mainMethodBlock != null)
                    {
                        ExecutableOutput = true;
                        EntryPoint = mainMethodInfo.ID;
                    }
                }

                string AssemblyName = Utilities.CleanFileName(TheLibrary.TheAssembly.GetName().Name);

                string LdPath = Path.Combine(Options.ToolsPath, @"Cygwin\ld.exe");
                string ObjdumpPath = Path.Combine(Options.ToolsPath, @"Cygwin\objdump.exe");
                string LinkScriptCmdPath = Path.Combine(Options.OutputPath, @"DriversCompiler\" + AssemblyName + "_linker_command.txt");
                string LinkScriptPath = Path.Combine(Options.OutputPath, @"DriversCompiler\" + AssemblyName + "_linker.ld");
                string BinPath = Path.Combine(Options.OutputPath, "Output\\" + (ExecutableOutput ? AssemblyName + ".elf" : "Lib" + AssemblyName + ".a"));
                string MapPath = Path.Combine(Options.OutputPath, AssemblyName + ".map");
                string ASMPath = Path.Combine(Options.OutputPath, AssemblyName + ".new.asm");

                string LdWorkingDir = Path.Combine(Options.OutputPath, "") + "\\";

                if (!Directory.Exists(Path.GetDirectoryName(BinPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(BinPath));
                }

                StringBuilder CommandLineArgsBuilder = new StringBuilder();
                if (!ExecutableOutput)
                {
                    CommandLineArgsBuilder.Append("-shared ");
                }
                CommandLineArgsBuilder.Append("-L .\\Output -T '" + LinkScriptPath + "' -o '" + BinPath + "'");

                StreamWriter ASMWriter = new StreamWriter(ASMPath, false);

                StringBuilder LinkScript = new StringBuilder();
                LinkScript.Append((ExecutableOutput ? "ENTRY(" + EntryPoint + ")\r\n" : "") + 
@"GROUP(");

                LinkScript.Append(string.Join(" ", SequencedASMBlocks
                    .Where(x => File.Exists(x.OutputFilePath))
                    .Select(x => "\"" + x.OutputFilePath + "\"")));
                
                LinkScript.Append(@")

");
                if (depLibNames.Count > 0)
                {
                    LinkScript.Append("GROUP(");
                    LinkScript.Append(string.Join(" ", depLibNames.Select(x => "-l" + x)));
                    LinkScript.Append(")");
                }

                LinkScript.AppendLine(@"

SECTIONS {
   . = 0x" + (0x40000000 + (depLibNames.Count * 0x1000)).ToString("X2") + @";

   .text : {
");

                for (int i = 0; i < SequencedASMBlocks.Count; i++)
                {
                    LinkScript.AppendLine(string.Format("       \"{0}\" (.text);", SequencedASMBlocks[i].OutputFilePath));
                    ASMWriter.WriteLine(File.ReadAllText(SequencedASMBlocks[i].OutputFilePath.Replace("\\Objects", "\\ASM").Replace(".o", ".asm")));
                }


                LinkScript.AppendLine(@"
          * (.text);
          * (.rodata*);
   }

   . = ALIGN(0x1000);
   .data : AT(ADDR(.data)) {
          * (.data*);
   }

   . = ALIGN(0x1000);
   .bss : AT(ADDR(.bss)) {
          * (.bss*);
   }
}
");
                ASMWriter.Close();

                File.WriteAllText(LinkScriptCmdPath, CommandLineArgsBuilder.ToString());
                File.WriteAllText(LinkScriptPath, LinkScript.ToString());
                OK = Utilities.ExecuteProcess(LdWorkingDir, LdPath, CommandLineArgsBuilder.ToString(), "Ld");
            }
            else if (Options.LinkMode == Options.LinkModes.ISO)
            {
                List<ASM.ASMBlock> SequencedASMBlocks = new List<ASM.ASMBlock>();
                List<IL.ILLibrary> FlattenedLibs = TheLibrary.Flatten();
                foreach (IL.ILLibrary depLib in FlattenedLibs)
                {
                    SequencedASMBlocks.AddRange(depLib.TheASMLibrary.ASMBlocks);
                }
                //SortBlocks(SequencedASMBlocks);
                SequencedASMBlocks.Sort(GetOrder);

                string AssemblyName = Utilities.CleanFileName(TheLibrary.TheAssembly.GetName().Name);

                string LdPath = Path.Combine(Options.ToolsPath, @"Cygwin\ld.exe");
                string ObjdumpPath = Path.Combine(Options.ToolsPath, @"Cygwin\objdump.exe");
                string ISOGenPath = Path.Combine(Options.ToolsPath, @"ISO9660Generator.exe");
                string ISOToolsDirPath = Path.Combine(Options.ToolsPath, @"ISO");
                string ISODirPath = Path.Combine(Options.OutputPath, @"DriversCompiler\ISO");
                string LinkScriptPath = Path.Combine(Options.OutputPath, @"DriversCompiler\linker.ld");
                string BinPath = Path.Combine(Options.OutputPath, @"DriversCompiler\ISO\" + AssemblyName + ".bin");
                string ISOLinuxPath = Path.Combine(Options.OutputPath, @"DriversCompiler\ISO\isolinux.bin");
                string ISOPath = Path.Combine(Options.OutputPath, AssemblyName + ".iso");
                string MapPath = Path.Combine(Options.OutputPath, AssemblyName + ".map");
                string ASMPath = Path.Combine(Options.OutputPath, AssemblyName + ".new.asm");

                StreamWriter ASMWriter = new StreamWriter(ASMPath, false);

                string LdWorkingDir = Path.Combine(Options.OutputPath, "DriversCompiler") + "\\";

                if (Directory.Exists(ISODirPath))
                {
                    Directory.Delete(ISODirPath, true);
                }
                CopyDirectory(ISOToolsDirPath, ISODirPath, true);

                StringBuilder CommandLineArgsBuilder = new StringBuilder();
                CommandLineArgsBuilder.Append("--fatal-warnings -T '" + LinkScriptPath + "' -o '" + BinPath + "'");

                StringBuilder LinkScript = new StringBuilder();
                LinkScript.Append(@"ENTRY(Kernel_Start)
OUTPUT_FORMAT(elf32-i386)

GROUP(");

                LinkScript.Append(string.Join(" ", SequencedASMBlocks
                    .Where(x => File.Exists(x.OutputFilePath))
                    .Select(x => "\"" + x.OutputFilePath + "\"")));

                LinkScript.AppendLine(@")

SECTIONS {
   /* The kernel will live at 3GB + 1MB in the virtual
      address space, which will be mapped to 1MB in the
      physical address space. */
   . = 0xC0100000;

   .text : AT(ADDR(.text) - 0xC0000000) {
");

                for (int i = 0; i < SequencedASMBlocks.Count; i++)
                {
                    LinkScript.AppendLine(string.Format("       \"{0}\" (.text);", SequencedASMBlocks[i].OutputFilePath));
                    ASMWriter.WriteLine(File.ReadAllText(SequencedASMBlocks[i].OutputFilePath.Replace("\\Objects", "\\ASM").Replace(".o", ".asm")));
                }


                LinkScript.AppendLine(@"
          * (.text);
          * (.rodata*);
   }

   . = ALIGN(0x1000);
   .data : AT(ADDR(.data) - 0xC0000000) {
          * (.data*);
   }

   . = ALIGN(0x1000);
   .bss : AT(ADDR(.bss) - 0xC0000000) {
          * (.bss*);
   }
}
");

                ASMWriter.Close();

                File.WriteAllText(LinkScriptPath, LinkScript.ToString());
                OK = Utilities.ExecuteProcess(LdWorkingDir, LdPath, CommandLineArgsBuilder.ToString(), "Ld");

                if (OK)
                {
                    if (File.Exists(ISOPath))
                    {
                        File.Delete(ISOPath);
                    }

                    OK = Utilities.ExecuteProcess(Options.OutputPath, ISOGenPath,
                        string.Format("4 \"{0}\" \"{1}\" true \"{2}\"", ISOPath, ISOLinuxPath, ISODirPath), "ISO9660Generator");

                    if (OK)
                    {
                        if (File.Exists(MapPath))
                        {
                            File.Delete(MapPath);
                        }

                        OK = Utilities.ExecuteProcess(Options.OutputPath, ObjdumpPath, string.Format("--wide --syms \"{0}\"", BinPath), "ObjDump", false, MapPath);
                    }
                }
            }

            return OK ? CompileResult.OK : CompileResult.Fail;
        }

        //From MSDN: https://msdn.microsoft.com/en-us/library/bb762914%28v=vs.110%29.aspx
        private static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    CopyDirectory(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public static int GetOrder(ASM.ASMBlock a, ASM.ASMBlock b)
        {
            return a.Priority.CompareTo(b.Priority);
        }
        private static void SortBlocks(List<ASM.ASMBlock> AllBlocks)
        {
            List<ASM.ASMBlock> LastLayerBlocks = AllBlocks.Where(x => x.ExternalLabels.Count == 0).ToList();
            long Priority = 0;
            foreach (ASM.ASMBlock Layer0Block in LastLayerBlocks)
            {
                Layer0Block.Priority = Priority;
            }

            List<ASM.ASMBlock> CurrentLayerBlocks = null;
            do
            {
                CurrentLayerBlocks = AllBlocks.Where(delegate(ASM.ASMBlock aBlock)
                {
                    foreach (ASM.ASMBlock lastBlock in LastLayerBlocks)
                    {
                        if (lastBlock.GlobalLabels.Intersect(aBlock.ExternalLabels).Count() > 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }).ToList();

                Priority = LastLayerBlocks[0].Priority + 1;
                foreach (ASM.ASMBlock aBlock in CurrentLayerBlocks)
                {
                    aBlock.Priority = Priority;
                }

                LastLayerBlocks = CurrentLayerBlocks;
            }
            while (CurrentLayerBlocks.Count > 0 &&
                LastLayerBlocks.Count != CurrentLayerBlocks.Count);
        }
    }
}
