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
            // If: Link to Libraries
            //      - TODO
            // If: Link to ELF and Libraries
            //      - TODO
            // If: Link to ISO
            //      - Generate basic link-script
            //      - Generate full link-script by inserting necessary file location instructions for all object files
            //      - Execute ld to build bin file
            //      - Execute ISO9660Generator to build .ISO file

            List<ASM.ASMBlock> SequencedASMBlocks = new List<ASM.ASMBlock>();
            List<IL.ILLibrary> FlattenedLibs = TheLibrary.Flatten();
            foreach (IL.ILLibrary depLib in FlattenedLibs)
            {
                SequencedASMBlocks.AddRange(depLib.TheASMLibrary.ASMBlocks);
            }
            SequencedASMBlocks.Sort(GetOrder);

            string AssemblyName = Utilities.CleanFileName(TheLibrary.TheAssembly.GetName().Name);

            string LdPath = Path.Combine(Options.ToolsPath, @"Cygwin\ld.exe");
            string ObjdumpPath = Path.Combine(Options.ToolsPath, @"Cygwin\ld.exe");
            string ISOGenPath = Path.Combine(Options.ToolsPath, @"ISO9660Generator.exe");
            string ISOToolsDirPath = Path.Combine(Options.ToolsPath, @"ISO");
            string ISODirPath = Path.Combine(Options.OutputPath, @"DriversCompiler\\ISO");
            string LinkScriptPath = Path.Combine(Options.OutputPath, "DriversCompiler\\linker.ld");
            string BinPath = Path.Combine(Options.OutputPath, "DriversCompiler\\ISO\\" + AssemblyName + ".bin");
            string ISOLinuxPath = Path.Combine(Options.OutputPath, "DriversCompiler\\ISO\\isolinux.bin");
            string ISOPath = Path.Combine(Options.OutputPath, AssemblyName + ".iso");
            string MapPath = Path.Combine(Options.OutputPath, AssemblyName + ".map");

            string LdWorkingDir = Path.Combine(Options.OutputPath, "DriversCompiler") + "\\";

            if (Directory.Exists(ISODirPath))
            {
                Directory.Delete(ISODirPath, true);
            }
            CopyDirectory(ISOToolsDirPath, ISODirPath, true);

            StringBuilder CommandLineArgsBuilder = new StringBuilder();
            CommandLineArgsBuilder.Append("-T '" + LinkScriptPath + "' -o '" + BinPath + "'");

            StringBuilder LinkScript = new StringBuilder();
            LinkScript.Append(@"ENTRY(Kernel_Start)
OUTPUT_FORMAT(elf32-i386)

INPUT(");

            LinkScript.Append(string.Join(", ", SequencedASMBlocks.Select(x => "\"" + x.OutputFilePath + "\"")));
            
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
                LinkScript.AppendLine(string.Format("       {0} (.text);", SequencedASMBlocks[i].OutputFilePath.Replace(LdWorkingDir + "Objects", "")));
            }


            LinkScript.AppendLine(@"
       * (.text);
       * (.rodata*);
       * (.data*);
   }
}
");

            File.WriteAllText(LinkScriptPath, LinkScript.ToString());
            bool OK = Utilities.ExecuteProcess(LdWorkingDir, LdPath, CommandLineArgsBuilder.ToString(), "Ld");

            if (OK)
            {
                if (File.Exists(ISOPath))
                {
                    File.Delete(ISOPath);
                }

                OK = Utilities.ExecuteProcess(Options.OutputPath, ISOGenPath, 
                    string.Format("4 '{0}' '{1}' '{2}' '{3}'", ISOPath, ISOLinuxPath, ISODirPath), "ISO9660Generator");

                if (OK)
                {
                    if (File.Exists(MapPath))
                    {
                        File.Delete(MapPath);
                    }

                    OK = Utilities.ExecuteProcess(Options.OutputPath, ObjdumpPath, string.Format("--wide --syms \"{0}\"", BinPath), "ObjDump", false, MapPath);
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
    }
}
