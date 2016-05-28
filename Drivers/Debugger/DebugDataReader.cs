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
using System.Globalization;
using System.IO;

namespace Drivers.Debugger
{
    public class DebugDataReader
    {
        private readonly Dictionary<string, string> MethodASMCache = new Dictionary<string, string>();
        private readonly List<string> ProcessedAssemblies = new List<string>();
        public Dictionary<uint, List<string>> AddressMappings = new Dictionary<uint, List<string>>();
        public Dictionary<string, List<string>> DebugOps = new Dictionary<string, List<string>>();
        public Dictionary<string, uint> LabelMappings = new Dictionary<string, uint>();
        public Dictionary<string, string> MethodFileMappings = new Dictionary<string, string>();

        public Dictionary<string, MethodInfo> Methods = new Dictionary<string, MethodInfo>();
        public Dictionary<string, TypeInfo> Types = new Dictionary<string, TypeInfo>();

        public void ReadDataFiles(string FolderPath, string AssemblyName)
        {
            using (
                StreamReader MethodFileMappingsStr =
                    new StreamReader(Path.Combine(FolderPath, AssemblyName + "_MethodFileMappings.txt")))
            {
                string line;
                while ((line = MethodFileMappingsStr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (!string.IsNullOrEmpty(line))
                    {
                        string[] LineParts = line.Split('¬');
                        string MethodLabel = LineParts[0];
                        string FileName = LineParts[1];
                        MethodFileMappings.Add(MethodLabel, FileName);
                    }
                }
            }

            using (StreamReader DebugOpsStr = new StreamReader(Path.Combine(FolderPath, AssemblyName + "_DebugOps.txt"))
                )
            {
                string line;
                string CurrentMethodLabel = "";
                while ((line = DebugOpsStr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (!string.IsNullOrEmpty(line))
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
                    if (!string.IsNullOrEmpty(line))
                    {
                        string[] LineParts = line.Split('¬');
                        string AddressStr = LineParts[0];
                        string Label = LineParts[1];
                        uint Address = uint.Parse(AddressStr, NumberStyles.HexNumber);
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

        public void ReadLibraryInfo(string FolderPath, string AssemblyName)
        {
            using (StreamReader Str = new StreamReader(Path.Combine(FolderPath, AssemblyName + "_Dependencies.txt")))
            {
                string line;
                while ((line = Str.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (!string.IsNullOrEmpty(line))
                    {
                        if (!ProcessedAssemblies.Contains(line))
                        {
                            ProcessedAssemblies.Add(line);
                            ReadLibraryInfo(FolderPath, line);
                        }
                    }
                }
            }

            using (StreamReader Str = new StreamReader(Path.Combine(FolderPath, AssemblyName + "_Library.txt")))
            {
                TypeInfo CurrentTypeInfo = null;
                MethodInfo CurrentMethodInfo = null;
                FieldInfo CurrentFieldInfo = null;

                bool skip = false;
                string line;
                while ((line = Str.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (!string.IsNullOrEmpty(line))
                    {
                        if (line.StartsWith("¬"))
                        {
                            if (!skip)
                            {
                                //¬BaseTypeID:[ID]
                                //¬IsGCManaged:[Boolean]
                                //¬IsPointer:[Boolean]
                                //¬IsValueType:[Boolean]
                                //¬SizeOnHeapInBytes:[Integer]
                                //¬SizeOnStackInBytes:[Integer]

                                line = line.Substring(1);

                                string[] LineParts = line.Split(':');
                                switch (LineParts[0])
                                {
                                    case "BaseTypeID":
                                        CurrentTypeInfo.BaseTypeID = LineParts[1];
                                        break;
                                    case "IsGCManaged":
                                        CurrentTypeInfo.IsGCManaged = bool.Parse(LineParts[1]);
                                        break;
                                    case "IsPointer":
                                        CurrentTypeInfo.IsPointer = bool.Parse(LineParts[1]);
                                        break;
                                    case "IsValueType":
                                        CurrentTypeInfo.IsValueType = bool.Parse(LineParts[1]);
                                        break;
                                    case "SizeOnHeapInBytes":
                                        CurrentTypeInfo.SizeOnHeapInBytes = int.Parse(LineParts[1]);
                                        break;
                                    case "SizeOnStackInBytes":
                                        CurrentTypeInfo.SizeOnStackInBytes = int.Parse(LineParts[1]);
                                        break;
                                }
                            }
                        }
                        else if (line.StartsWith("|"))
                        {
                            if (!skip)
                            {
                                //|Field:[ID]
                                //|Method:[ID]

                                line = line.Substring(1);

                                string[] LineParts = line.Split(':');
                                switch (LineParts[0])
                                {
                                    case "Field":
                                        CurrentFieldInfo = new FieldInfo
                                        {
                                            ID = LineParts[1]
                                        };
                                        CurrentTypeInfo.Fields.Add(CurrentFieldInfo.ID, CurrentFieldInfo);
                                        CurrentMethodInfo = null;
                                        break;
                                    case "Method":
                                        CurrentMethodInfo = new MethodInfo
                                        {
                                            ID = LineParts[1]
                                        };
                                        CurrentTypeInfo.Methods.Add(CurrentMethodInfo.ID, CurrentMethodInfo);
                                        Methods.Add(CurrentMethodInfo.ID, CurrentMethodInfo);
                                        CurrentFieldInfo = null;
                                        break;
                                }
                            }
                        }
                        else if (line.StartsWith("~"))
                        {
                            if (!skip)
                            {
                                line = line.Substring(1);
                                string[] LineParts = line.Split(':');

                                if (CurrentFieldInfo != null)
                                {
                                    //~Type:[TypeID]
                                    //~IsStatic:[Boolean]
                                    //~Name:[String]
                                    //~OffsetInBytes:[Integer]

                                    switch (LineParts[0])
                                    {
                                        case "Type":
                                            CurrentFieldInfo.TypeID = LineParts[1];
                                            break;
                                        case "IsStatic":
                                            CurrentFieldInfo.IsStatic = bool.Parse(LineParts[1]);
                                            break;
                                        case "Name":
                                            CurrentFieldInfo.Name = LineParts[1];
                                            break;
                                        case "OffsetInBytes":
                                            CurrentFieldInfo.OffsetInBytes = int.Parse(LineParts[1]);
                                            break;
                                    }
                                }
                                else if (CurrentMethodInfo != null)
                                {
                                    //~ApplyDebug:[Boolean]
                                    //~ApplyGC:[Boolean]
                                    //~IDValue:[Integer]
                                    //~IsConstructor:[Boolean]
                                    //~IsPlugged:[Boolean]
                                    //~IsStatic:[Boolean]
                                    //~Signature:[String]
                                    //~Argument:Offset|Position|TypeID
                                    //~Local:Offset|Position|TypeID

                                    switch (LineParts[0])
                                    {
                                        case "ApplyDebug":
                                            CurrentMethodInfo.ApplyDebug = bool.Parse(LineParts[1]);
                                            break;
                                        case "ApplyGC":
                                            CurrentMethodInfo.ApplyGC = bool.Parse(LineParts[1]);
                                            break;
                                        case "IDValue":
                                            CurrentMethodInfo.IDValue = int.Parse(LineParts[1]);
                                            break;
                                        case "IsConstructor":
                                            CurrentMethodInfo.IsConstructor = bool.Parse(LineParts[1]);
                                            break;
                                        case "IsPlugged":
                                            CurrentMethodInfo.IsPlugged = bool.Parse(LineParts[1]);
                                            break;
                                        case "IsStatic":
                                            CurrentMethodInfo.IsStatic = bool.Parse(LineParts[1]);
                                            break;
                                        case "Signature":
                                            CurrentMethodInfo.Signature = LineParts[1];
                                            break;
                                        case "ReturnSize":
                                            CurrentMethodInfo.ReturnSize = int.Parse(LineParts[1]);
                                            break;
                                        case "Argument":
                                        {
                                            string[] SubParts = LineParts[1].Split('|');

                                            //Offset|Position|TypeID
                                            int Offset = int.Parse(SubParts[0]);
                                            int Position = int.Parse(SubParts[1]);
                                            string TypeID = SubParts[2];
                                            CurrentMethodInfo.Arguments.Add(Offset, new VariableInfo
                                            {
                                                Offset = Offset,
                                                Position = Position,
                                                TypeID = TypeID
                                            });
                                        }
                                            break;
                                        case "Local":
                                        {
                                            string[] SubParts = LineParts[1].Split('|');

                                            //Offset|Position|TypeID
                                            int Offset = int.Parse(SubParts[0]);
                                            int Position = int.Parse(SubParts[1]);
                                            string TypeID = SubParts[2];
                                            CurrentMethodInfo.Locals.Add(Offset, new VariableInfo
                                            {
                                                Offset = Offset,
                                                Position = Position,
                                                TypeID = TypeID
                                            });
                                        }
                                            break;
                                    }
                                }
                                else
                                {
                                    //Whoops...

                                    throw new NullReferenceException("No current field or method info!");
                                }
                            }
                        }
                        else
                        {
                            //TypeID

                            if (!Types.ContainsKey(line))
                            {
                                skip = false;

                                CurrentTypeInfo = new TypeInfo
                                {
                                    ID = line
                                };
                                Types.Add(CurrentTypeInfo.ID, CurrentTypeInfo);
                            }
                            else
                            {
                                skip = true;
                            }
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
            return "";
        }
    }
}