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
    /// <summary>
    /// This class has yet to be implemented.
    /// </summary>
    public static class DebugDataWriter
    {
        public static Dictionary<string, string> MethodFileMappings = new Dictionary<string, string>();
        public static Dictionary<string, List<string>> DebugOps = new Dictionary<string, List<string>>();

        public static void AddMethodMapping(string MethodLabel, string FileName)
        {
            MethodFileMappings.Add(MethodLabel, FileName);
        }

        public static void AddDebugOp(string MethodLabel, string DebugLabel)
        {
            if (!DebugOps.ContainsKey(MethodLabel))
            {
                DebugOps.Add(MethodLabel, new List<string>());
            }
            DebugOps[MethodLabel].Add(DebugLabel);
        }

        public static void SaveDataFiles(string FolderPath, string AssemblyName)
        {
            using (StreamWriter MethodFileMappingsStr = new StreamWriter(Path.Combine(FolderPath, AssemblyName + "_MethodFileMappings.txt"), false))
            {
                foreach (KeyValuePair<string, string> Mapping in MethodFileMappings)
                {
                    MethodFileMappingsStr.WriteLine(Mapping.Key + "¬" + Mapping.Value);
                }
            }

            using (StreamWriter DebugOpsStr = new StreamWriter(Path.Combine(FolderPath, AssemblyName + "DebugOps.txt"), false))
            {
                foreach (KeyValuePair<string, List<string>> MethodOps in DebugOps)
                {
                    DebugOpsStr.WriteLine("¬" + MethodOps.Key);
                    
                    foreach (string Op in MethodOps.Value)
                    {
                        DebugOpsStr.WriteLine("|" + Op);
                    }
                }
            }
        }
        public static void ProcessMapFile(string FileName)
        {
            string[] Lines = File.ReadAllLines(FileName);
            List<string> ResultLines = new List<string>(Lines.Length);

            bool FoundSymbolTable = false;
            foreach (string Line in Lines)
            {
                string ProcessedLine = Line.Trim();

                if (ProcessedLine.Length > 0)
                {
                    if (FoundSymbolTable)
                    {
                        if (ProcessedLine.Length >= 18)
                        {
                            string AddressStr = ProcessedLine.Substring(0, 8);
                            if (AddressStr != "00000000")
                            {
                                string LabelStr = ProcessedLine.Substring(17).Split(new char[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries).Last();
                                ResultLines.Add(AddressStr + "¬" + LabelStr);
                            }
                        }
                    }
                    else
                    {
                        if (ProcessedLine.StartsWith("SYMBOL TABLE:"))
                        {
                            FoundSymbolTable = true;
                        }
                    }
                }
            }

            File.Delete(FileName);
            File.WriteAllLines(FileName, ResultLines.ToArray());
        }
    }
}
