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

namespace Drivers.Debugger
{
    public class DebugDataReader
    {
        public Dictionary<string, string> MethodFileMappings = new Dictionary<string, string>();
        public Dictionary<string, List<string>> DebugOps = new Dictionary<string, List<string>>();
        public Dictionary<uint, List<string>> AddressMappings = new Dictionary<uint, List<string>>();
        public Dictionary<string, uint> LabelMappings = new Dictionary<string, uint>();

        private Dictionary<string, string> MethodASMCache = new Dictionary<string, string>();

        public void ReadDataFiles(string FolderPath, string AssemblyName)
        {
            using (StreamReader MethodFileMappingsStr = new StreamReader(Path.Combine(FolderPath, AssemblyName + "_MethodFileMappings.txt")))
            {
                string line;
                while ((line = MethodFileMappingsStr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (!String.IsNullOrEmpty(line))
                    {
                        string[] LineParts = line.Split('¬');
                        string MethodLabel = LineParts[0];
                        string FileName = LineParts[1];
                        MethodFileMappings.Add(MethodLabel, FileName);
                    }
                }
            }

            using (StreamReader DebugOpsStr = new StreamReader(Path.Combine(FolderPath, AssemblyName + "DebugOps.txt")))
            {
                string line;
                string CurrentMethodLabel = "";
                while ((line = DebugOpsStr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (!String.IsNullOrEmpty(line))
                    {
                        if (line.StartsWith("¬"))
                        {
                            CurrentMethodLabel = line.Substring(1);
                            DebugOps.Add(CurrentMethodLabel, new List<string>());
                        }
                        else if (line.StartsWith("|"))
                        {
                            string LocalLabel = line.Substring(1);
                            DebugOps[CurrentMethodLabel].Add(LocalLabel);
                        }
                    }
                }
            }

            using (StreamReader LabelMappingsStr = new StreamReader(Path.Combine(FolderPath, AssemblyName + ".map")))
            {
                string line;
                while ((line = LabelMappingsStr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (!String.IsNullOrEmpty(line))
                    {
                        string[] LineParts = line.Split('¬');
                        string AddressStr = LineParts[0];
                        string Label = LineParts[1];
                        uint Address = uint.Parse(AddressStr, System.Globalization.NumberStyles.HexNumber);
                        if (!LabelMappings.ContainsKey(Label))
                        {
                            LabelMappings.Add(Label, Address);
                            if (!AddressMappings.ContainsKey(Address))
                            {
                                AddressMappings.Add(Address, new List<string>());
                            }
                            AddressMappings[Address].Add(Label);
                        }
                    }
                }
            }
        }

        public string ReadMethodASM(string MethodLabel)
        {
            if (MethodFileMappings.ContainsKey(MethodLabel))
            {
                if (!MethodASMCache.ContainsKey(MethodLabel))
                {
                    MethodASMCache.Add(MethodLabel, File.ReadAllText(MethodFileMappings[MethodLabel]));
                }
                return MethodASMCache[MethodLabel];
            }
            else
            {
                return "";
            }
        }
    }
}
