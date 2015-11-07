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

            using (StreamWriter DebugOpsStr = new StreamWriter(Path.Combine(FolderPath, AssemblyName + "_DebugOps.txt"), false))
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
        public static void SaveLibraryInfo(string FolderPath, IL.ILLibrary TheLibrary)
        {
            string RootAssemblyName = Utilities.CleanFileName(TheLibrary.TheAssembly.GetName().Name);

            using (StreamWriter Str = new StreamWriter(Path.Combine(FolderPath, RootAssemblyName + "_Dependencies.txt"), false))
            {
                foreach (IL.ILLibrary DependencyLibrary in TheLibrary.Dependencies)
                {
                    Str.WriteLine(Utilities.CleanFileName(DependencyLibrary.TheAssembly.GetName().Name));
                }
            }

            using (StreamWriter Str = new StreamWriter(Path.Combine(FolderPath, RootAssemblyName + "_Library.txt"), false))
            {
                foreach (Types.TypeInfo ATypeInfo in TheLibrary.TypeInfos)
                {
                    //TypeID
                    //¬BaseTypeID:[ID]
                    //¬IsGCManaged:[Boolean]
                    //¬IsPointer:[Boolean]
                    //¬IsValueType:[Boolean]
                    //¬SizeOnHeapInBytes:[Integer]
                    //¬SizeOnStackInBytes:[Integer]
                    //|Field:[ID]
                    //~Type:[TypeID]
                    //~IsStatic:[Boolean]
                    //~Name:[String]
                    //~OffsetInBytes:[Integer]
                    //|Method:[ID]
                    //~ApplyDebug:[Boolean]
                    //~ApplyGC:[Boolean]
                    //~IDValue:[Integer]
                    //~IsConstructor:[Boolean]
                    //~IsPlugged:[Boolean]
                    //~IsStatic:[Boolean]
                    //~Signature:[String]
                    //~Argument:Offset|Position|TypeID
                    //~Local:Offset|Position|TypeID

                    Str.WriteLine(ATypeInfo.ID);
                    if (ATypeInfo.UnderlyingType.BaseType != null && 
                        !ATypeInfo.UnderlyingType.BaseType.AssemblyQualifiedName.Contains("mscorlib"))
                    {
                        Str.WriteLine("¬BaseTypeID:" + TheLibrary.GetTypeInfo(ATypeInfo.UnderlyingType.BaseType).ID);
                    }
                    Str.WriteLine("¬IsGCManaged:" + ATypeInfo.IsGCManaged.ToString());
                    Str.WriteLine("¬IsPointer:" + ATypeInfo.IsPointer.ToString());
                    Str.WriteLine("¬IsValueType:" + ATypeInfo.IsValueType.ToString());
                    Str.WriteLine("¬SizeOnHeapInBytes:" + ATypeInfo.SizeOnHeapInBytes.ToString());
                    Str.WriteLine("¬SizeOnStackInBytes:" + ATypeInfo.SizeOnStackInBytes.ToString());

                    foreach (Types.FieldInfo AFieldInfo in ATypeInfo.FieldInfos)
                    {
                        Str.WriteLine("|Field:" + AFieldInfo.ID);
                        Str.WriteLine("~Type:" + TheLibrary.GetTypeInfo(AFieldInfo.FieldType).ID);
                        Str.WriteLine("~IsStatic:" + AFieldInfo.IsStatic.ToString());
                        Str.WriteLine("~Name:" + AFieldInfo.Name);
                        Str.WriteLine("~OffsetInBytes:" + AFieldInfo.OffsetInBytes.ToString());
                    }

                    foreach (Types.MethodInfo AMethodInfo in ATypeInfo.MethodInfos)
                    {
                        Str.WriteLine("|Method:" + AMethodInfo.ID);
                        Str.WriteLine("~ApplyDebug:" + AMethodInfo.ApplyDebug.ToString());
                        Str.WriteLine("~ApplyGC:" + AMethodInfo.ApplyGC.ToString());
                        Str.WriteLine("~IDValue:" + AMethodInfo.IDValue.ToString());
                        Str.WriteLine("~IsConstructor:" + AMethodInfo.IsConstructor.ToString());
                        Str.WriteLine("~IsPlugged:" + AMethodInfo.IsPlugged.ToString());
                        Str.WriteLine("~IsStatic:" + AMethodInfo.IsStatic.ToString());
                        Str.WriteLine("~Signature:" + AMethodInfo.Signature);

                        Type RetType = (AMethodInfo.IsConstructor ?
                                            typeof(void) : ((System.Reflection.MethodInfo)AMethodInfo.UnderlyingInfo).ReturnType);
                        Str.WriteLine("~ReturnSize:" + Types.TypeScanner.GetSizeOnStackInBytes(RetType));

                        foreach (Types.VariableInfo AnArgumentInfo in AMethodInfo.ArgumentInfos)
                        {
                            Str.WriteLine("~Argument:" + AnArgumentInfo.Offset.ToString() + "|" + AnArgumentInfo.Position.ToString() + "|" + AnArgumentInfo.TheTypeInfo.ID);
                        }
                        foreach (Types.VariableInfo ALocalInfo in AMethodInfo.LocalInfos)
                        {
                            Str.WriteLine("~Local:" + ALocalInfo.Offset.ToString() + "|" + ALocalInfo.Position.ToString() + "|" + ALocalInfo.TheTypeInfo.ID);
                        }
                    }
                }
            }
        }
    }
}
