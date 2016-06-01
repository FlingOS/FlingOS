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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Drivers.Compiler.ASM;
using Drivers.Compiler.Attributes;
using Drivers.Compiler.IL;
using Drivers.Compiler.Types;

namespace Drivers.Compiler
{
    /// <summary>
    ///     Manages linking the object files into the final ELF/A file(s) or ISO file.
    /// </summary>
    public static class LinkManager
    {
        private static readonly Dictionary<string, string> DependencyNameMapping = new Dictionary<string, string>();
        private static int NameGenerator;

        /// <summary>
        ///     Performs the link.
        /// </summary>
        /// <param name="TheLibrary">The root library to link.</param>
        /// <returns>CompileResult.OK if the link succeeded. Otherwise, CompileResult.Fail.</returns>
        public static CompileResult Link(ILLibrary TheLibrary, bool dependency = false, string Name = null)
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
                foreach (ILLibrary depLib in TheLibrary.Dependencies)
                {
                    string depLibName = Utilities.CleanFileName(depLib.TheAssembly.GetName().Name);
                    if (Options.ShortenDependencyNames)
                    {
                        if (!DependencyNameMapping.ContainsKey(depLibName))
                        {
                            DependencyNameMapping.Add(depLibName, NameGenerator++.ToString());
                        }

                        depLibName = DependencyNameMapping[depLibName];
                    }
                    depLibNames.Add(depLibName);

                    OK = OK && (Link(depLib, true, depLibName) == CompileResult.OK);
                    if (!OK)
                    {
                        break;
                    }
                }

                if (!OK)
                {
                    return CompileResult.Fail;
                }

                List<ASMBlock> SequencedASMBlocks = new List<ASMBlock>();
                SequencedASMBlocks.AddRange(TheLibrary.TheASMLibrary.ASMBlocks);
                SequencedASMBlocks.Sort(GetOrder);
                SequencedASMBlocks.ForEach(delegate(ASMBlock block)
                {
                    if (block != null && block.OriginMethodInfo != null)
                    {
                        DebugDataWriter.AddMethodMapping(block.OriginMethodInfo.ID, block.ASMOutputFilePath);
                    }
                });

                // Find start method if any, use as ENTRY point
                bool ExecutableOutput = false;
                string EntryPoint = null;
                if (ILLibrary.SpecialMethods.ContainsKey(typeof(MainMethodAttribute)))
                {
                    MethodInfo mainMethodInfo = ILLibrary.SpecialMethods[typeof(MainMethodAttribute)].First();
                    ILBlock mainMethodBlock = TheLibrary.GetILBlock(mainMethodInfo, false);
                    if (mainMethodBlock != null)
                    {
                        ExecutableOutput = true;
                        EntryPoint = mainMethodInfo.ID;
                    }
                }

                if (Options.ShortenDependencyNames && string.IsNullOrWhiteSpace(Name))
                {
                    Name = "Driver";
                }

                string AssemblyName = string.IsNullOrWhiteSpace(Name)
                    ? Utilities.CleanFileName(TheLibrary.TheAssembly.GetName().Name)
                    : Name;

                DebugDataWriter.SaveDataFiles(Options.OutputPath, AssemblyName);
                DebugDataWriter.SaveLibraryInfo(Options.OutputPath, TheLibrary);

                LinkInformation LinkInfo = new LinkInformation
                {
                    ToolsPath = Options.ToolsPath,
                    LinkScriptCmdPath =
                        Path.Combine(Options.OutputPath, @"DriversCompiler\" + AssemblyName + "_linker_command.txt"),
                    LinkScriptPath = Path.Combine(Options.OutputPath, @"DriversCompiler\" + AssemblyName + "_linker.ld"),
                    BinPath =
                        Path.Combine(Options.OutputPath,
                            "Output\\" + (ExecutableOutput ? AssemblyName + ".elf" : "Lib" + AssemblyName + ".a")),
                    MapPath = Path.Combine(Options.OutputPath, AssemblyName + ".map"),
                    ASMPath = Path.Combine(Options.OutputPath, AssemblyName + ".new.asm"),
                    LdWorkingDir = Path.Combine(Options.OutputPath, "") + "\\",
                    ExecutableOutput = ExecutableOutput,
                    EntryPoint = EntryPoint,
                    SequencedASMBlocks = SequencedASMBlocks,
                    depLibNames = depLibNames
                };

                if (!Directory.Exists(Path.GetDirectoryName(LinkInfo.BinPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(LinkInfo.BinPath));
                }

                OK = TargetArchitecture.TargetFunctions.LinkELF(TheLibrary, LinkInfo);

                if (OK)
                {
                    DebugDataWriter.ProcessMapFile(LinkInfo.MapPath);
                }
            }
            else if (Options.LinkMode == Options.LinkModes.ISO)
            {
                List<ASMBlock> SequencedASMBlocks = new List<ASMBlock>();
                List<ILLibrary> FlattenedLibs = TheLibrary.Flatten();
                foreach (ILLibrary depLib in FlattenedLibs)
                {
                    SequencedASMBlocks.AddRange(depLib.TheASMLibrary.ASMBlocks);
                    DebugDataWriter.SaveLibraryInfo(Options.OutputPath, depLib);
                }
                SequencedASMBlocks.Sort(GetOrder);
                SequencedASMBlocks.ForEach(delegate(ASMBlock block)
                {
                    if (block != null && block.OriginMethodInfo != null)
                    {
                        DebugDataWriter.AddMethodMapping(block.OriginMethodInfo.ID, block.ASMOutputFilePath);
                    }
                });

                string AssemblyName = Utilities.CleanFileName(TheLibrary.TheAssembly.GetName().Name);

                DebugDataWriter.SaveDataFiles(Options.OutputPath, AssemblyName);
                DebugDataWriter.SaveLibraryInfo(Options.OutputPath, TheLibrary);

                LinkInformation LinkInfo = new LinkInformation
                {
                    ToolsPath = Options.ToolsPath,
                    ISOGenPath = Path.Combine(Options.ToolsPath, @"ISO9660Generator.exe"),
                    ISOToolsDirPath = Path.Combine(Options.ToolsPath, @"ISO"),
                    ISODirPath = Path.Combine(Options.OutputPath, @"DriversCompiler\ISO"),
                    LinkScriptPath = Path.Combine(Options.OutputPath, @"DriversCompiler\linker.ld"),
                    BinPath = Path.Combine(Options.OutputPath, @"DriversCompiler\ISO\Kernel.bin"),
                    ISOLinuxPath =
                        Path.Combine(Options.OutputPath,
                            @"DriversCompiler\ISO\" +
                            (Options.BuildMode == Options.BuildModes.Debug ? "isolinux-debug.bin" : "isolinux.bin")),
                    ISOPath = Path.Combine(Options.OutputPath, AssemblyName + ".iso"),
                    MapPath = Path.Combine(Options.OutputPath, AssemblyName + ".map"),
                    ASMPath = Path.Combine(Options.OutputPath, AssemblyName + ".new.asm"),
                    LdWorkingDir = Path.Combine(Options.OutputPath, "DriversCompiler") + "\\",
                    SequencedASMBlocks = SequencedASMBlocks
                };

                if (Directory.Exists(LinkInfo.ISODirPath))
                {
                    Directory.Delete(LinkInfo.ISODirPath, true);
                }
                CopyDirectory(LinkInfo.ISOToolsDirPath, LinkInfo.ISODirPath, true);

                OK = TargetArchitecture.TargetFunctions.LinkISO(TheLibrary, LinkInfo);

                if (OK)
                {
                    DebugDataWriter.ProcessMapFile(LinkInfo.MapPath);
                }
            }

            return OK ? CompileResult.OK : CompileResult.Fail;
        }

        /// <summary>
        ///     Copies a directory from one location to another, optionally including sub directories.
        /// </summary>
        /// <remarks>
        ///     From MSDN: https://msdn.microsoft.com/en-us/library/bb762914%28v=vs.110%29.aspx
        /// </remarks>
        /// <param name="sourceDirName">The source directory path.</param>
        /// <param name="destDirName">The destination directory path.</param>
        /// <param name="copySubDirs">Whether to copy sub directories or not.</param>
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

        /// <summary>
        ///     Gets the order of two ASM blocks based on their priorities.
        /// </summary>
        /// <param name="a">First block to order.</param>
        /// <param name="b">Second block to order.</param>
        /// <returns>/// -1 = a before b, 0 = a or b in either order, +1 = a after b.</returns>
        public static int GetOrder(ASMBlock a, ASMBlock b)
        {
            if (a.Priority == b.Priority)
            {
                if (a.OriginMethodInfo != null && b.OriginMethodInfo != null)
                {
                    return a.OriginMethodInfo.Signature.CompareTo(b.OriginMethodInfo.Signature);
                }
                return -1;
            }

            return a.Priority.CompareTo(b.Priority);
        }

        /// <summary>
        ///     Sorts the list of blocks according to their priorities.
        /// </summary>
        /// <param name="AllBlocks">The list of blocks to sort.</param>
        private static void SortBlocks(List<ASMBlock> AllBlocks)
        {
            List<ASMBlock> LastLayerBlocks = AllBlocks.Where(x => x.ExternalLabels.Count == 0).ToList();
            long Priority = 0;
            foreach (ASMBlock Layer0Block in LastLayerBlocks)
            {
                Layer0Block.Priority = Priority;
            }

            List<ASMBlock> CurrentLayerBlocks = null;
            do
            {
                CurrentLayerBlocks = AllBlocks.Where(delegate(ASMBlock aBlock)
                {
                    foreach (ASMBlock lastBlock in LastLayerBlocks)
                    {
                        if (lastBlock.GlobalLabels.Intersect(aBlock.ExternalLabels).Count() > 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }).ToList();

                Priority = LastLayerBlocks[0].Priority + 1;
                foreach (ASMBlock aBlock in CurrentLayerBlocks)
                {
                    aBlock.Priority = Priority;
                }

                LastLayerBlocks = CurrentLayerBlocks;
            } while (CurrentLayerBlocks.Count > 0 &&
                     LastLayerBlocks.Count != CurrentLayerBlocks.Count);
        }
    }

    public class LinkInformation
    {
        public string ASMPath;
        public string BinPath;
        public List<string> depLibNames;
        public string EntryPoint;
        public bool ExecutableOutput;
        public string ISODirPath;

        /* ISO Link options */
        public string ISOGenPath;
        public string ISOLinuxPath;
        public string ISOPath;
        public string ISOToolsDirPath;
        public string LdWorkingDir;

        /* ELF Link options */
        public string LinkScriptCmdPath;
        public string LinkScriptPath;
        public string MapPath;
        public List<ASMBlock> SequencedASMBlocks;
        /* General link options */
        public string ToolsPath;
    }
}