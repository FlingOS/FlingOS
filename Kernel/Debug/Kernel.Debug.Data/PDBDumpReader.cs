#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Kernel.Debug.Data
{
    /// <summary>
    /// Used to read data from a PDB dump file.
    /// </summary>
    public class PDBDumpReader
    {
        /// <summary>
        /// All the data from the various sections of the PDB dump file.
        /// </summary>
        Dictionary<string, List<string>> SectionsData = new Dictionary<string, List<string>>();
        /// <summary>
        /// All the symbols loaded from the PDB dump.
        /// </summary>
        public Dictionary<string, PDB_SymbolInfo> Symbols = new Dictionary<string,PDB_SymbolInfo>();

        /// <summary>
        /// Initialises a new PDM dump reader and reads the specified dump file.
        /// </summary>
        /// <param name="aDumpPath">The dump file to read.</param>
        public PDBDumpReader(string aDumpPath)
        {
            Read(aDumpPath);
        }

        /// <summary>
        /// Reads the specified dump file.
        /// </summary>
        /// <param name="aDumpPath">The dump file to read.</param>
        private void Read(string aDumpPath)
        {
            string[] Lines = File.ReadAllLines(aDumpPath);
            string currSectionKey = null;
            List<string> currSectionText = new List<string>();
            for(int i = 0; i < Lines.Length; i++)
            {
                string cLine = Lines[i];
                if(cLine.StartsWith("***"))
                {
                    if(currSectionKey != null)
                    {
                        SectionsData.Add(currSectionKey, currSectionText);
                    }
                    currSectionKey = cLine.Substring(4).Trim();
                    currSectionText = new List<string>();
                }
                else if (!string.IsNullOrEmpty(cLine))
                {
                    currSectionText.Add(cLine);
                }
            }
            if (currSectionKey != null)
            {
                SectionsData.Add(currSectionKey, currSectionText);
            }


            List<PDB_MethodInfo> AllMethodInfos = new List<PDB_MethodInfo>();

            List<string> SymbolsData = SectionsData["SYMBOLS"];
            string currSymbolKey = null;
            List<string> currSymbolText = new List<string>();
            for(int i = 0; i < SymbolsData.Count; i++)
            {
                string cLine = SymbolsData[i];
                if (cLine.StartsWith("**"))
                {
                    if (currSymbolKey != null)
                    {
                        PDB_SymbolInfo newInf = new PDB_SymbolInfo(currSymbolText);
                        AllMethodInfos.AddRange(newInf.Methods);
                        Symbols.Add(currSymbolKey, newInf);
                    }
                    currSymbolKey = cLine.Substring(11).Trim();
                    currSymbolText = new List<string>();
                }
                else if (!string.IsNullOrEmpty(cLine))
                {
                    currSymbolText.Add(cLine);
                }
            }
            if (currSymbolKey != null)
            {
                PDB_SymbolInfo newInf = new PDB_SymbolInfo(currSymbolText);
                AllMethodInfos.AddRange(newInf.Methods);
                Symbols.Add(currSymbolKey, newInf);
            }


            List<string> LinesData = SectionsData["LINES"];
            string currLineKey = null;
            List<string> currLinesText = new List<string>();
            for (int i = 0; i < LinesData.Count; i++)
            {
                string cLine = LinesData[i];
                if (cLine.StartsWith("**"))
                {
                    if (currLineKey != null)
                    {
                        string DumpStartAddressStr = currLinesText[0].Trim().Split(' ')[3].Substring(1, 8);
                        int DumpStartAddress = int.Parse(DumpStartAddressStr, System.Globalization.NumberStyles.HexNumber);
                        PDB_MethodInfo TheMethodInfo = (from infs in AllMethodInfos
                                                        where (infs.DumpStartAddress == DumpStartAddress &&
                                                        infs.FunctionName == currLineKey)
                                                        select infs).First();
                        TheMethodInfo.ParseLines(currLinesText);
                    }
                    currLineKey = cLine.Substring(3).Trim();
                    currLinesText = new List<string>();
                }
                else if(!string.IsNullOrEmpty(cLine))
                {
                    currLinesText.Add(cLine);
                }
            }
            if (currLineKey != null)
            {
                string DumpStartAddressStr = currLinesText[0].Trim().Split(' ')[3].Substring(1, 8);
                int DumpStartAddress = int.Parse(DumpStartAddressStr, System.Globalization.NumberStyles.HexNumber);
                PDB_MethodInfo TheMethodInfo = (from infs in AllMethodInfos
                                                where (infs.DumpStartAddress == DumpStartAddress &&
                                                infs.FunctionName == currLineKey)
                                                select infs).First();
                TheMethodInfo.ParseLines(currLinesText);
            }
        }
    }
    /// <summary>
    /// Represents a symbol loaded from a PDB dump.
    /// </summary>
    public class PDB_SymbolInfo
    {
        /// <summary>
        /// The methods within the symbol.
        /// </summary>
        public List<PDB_MethodInfo> Methods = new List<PDB_MethodInfo>();

        /// <summary>
        /// Creates a new, empty symbol info.
        /// </summary>
        public PDB_SymbolInfo()
        {
        }
        /// <summary>
        /// Creates a new symbol info and loads method infos from the specified
        /// lines of text.
        /// </summary>
        /// <param name="symbolText">The lines of symbol text to parse.</param>
        public PDB_SymbolInfo(List<string> symbolText)
        {
            for (int i = 0; i < symbolText.Count; i++)
            {
                string cLine = symbolText[i];
                if(cLine.StartsWith("Function"))
                {
                    string DumpStartAddressStr = cLine.Substring(31, 8);
                    int DumpStartAddress = int.Parse(DumpStartAddressStr, System.Globalization.NumberStyles.HexNumber);
                    string FunctionName = cLine.Trim().Split(' ').Last();
                    Methods.Add(new PDB_MethodInfo()
                    {
                        DumpStartAddress = DumpStartAddress,
                        FunctionName = FunctionName
                    });
                }
            }
        }
    }
    /// <summary>
    /// Represents method info loaded from a PDB dump.
    /// </summary>
    public class PDB_MethodInfo
    {
        /// <summary>
        /// The lines within this method.
        /// </summary>
        public List<PDB_LineInfo> Lines = new List<PDB_LineInfo>();
        /// <summary>
        /// The path to the C# source file for the method.
        /// </summary>
        public string SourceFilePath;
        /// <summary>
        /// The name of the function in C#.
        /// </summary>
        public string FunctionName;

        /// <summary>
        /// Dump start address - an internel number used by dump reader.
        /// Never sure where it comes from / how it's generated, but
        /// we use it for linking up bits of the PDB dump.
        /// </summary>
        internal int DumpStartAddress;

        /// <summary>
        /// Parses the specified lines of text for method line info.
        /// </summary>
        /// <param name="linesText">The lines of text to parse.</param>
        public void ParseLines(List<string> linesText)
        {
            int intLineNum = 0;
            for (int i = 0; i < linesText.Count; i++)
            {
                string cLine = linesText[i].Trim().Replace('\t', ' ');
                if (cLine.StartsWith("line"))
                {
                    string[] cLineParts = cLine.Split(' ');
                    string CSLineNumStr = cLineParts[1];
                    string ILStartNumStr = cLineParts[3].Substring(1, 8);
                    string ILLengthStr = cLineParts[6].Substring(2);

                    //-1 because of 1-based to 0-based indexing
                    int CSLineNum = int.Parse(CSLineNumStr) - 1;
                    int ILStartNum = int.Parse(ILStartNumStr, System.Globalization.NumberStyles.HexNumber) - DumpStartAddress;
                    int ILLength = int.Parse(ILLengthStr, System.Globalization.NumberStyles.HexNumber);

                    Lines.Add(new PDB_LineInfo()
                    {
                        CSLineNum = CSLineNum,
                        ILStartNum = ILStartNum,
                        ILEndNum = ILStartNum + ILLength - 1
                    });

                    if(intLineNum == 0)
                    {
                        SourceFilePath = "";
                        for(int j = 7; j < cLineParts.Length; j++)
                        {
                            SourceFilePath += cLineParts[j] + " ";
                        }
                        SourceFilePath = SourceFilePath.Trim();
                    }

                    intLineNum++;
                }
            }
        }

        /// <summary>
        /// The cached C# code text - stops us doing endless file reads etc.
        /// </summary>
        private string cachedCS = null;
        /// <summary>
        /// Gets the C# code (text) for the method. Used by debugger for 
        /// display to developer.
        /// </summary>
        /// <returns>The C# code (text) or a string with an error message starting "Error loading C#!".</returns>
        public string GetCS()
        {
            if (cachedCS == null)
            {
                try
                {
                    cachedCS = File.ReadAllText(SourceFilePath);
                }
                catch(Exception ex)
                {
                    cachedCS = "Error loading C#! Error : " + ex.Message;
                }
            }
            return cachedCS;
        }
    }
    /// <summary>
    /// Represents line info loaded from a PDB dump.
    /// </summary>
    public class PDB_LineInfo
    {
        /// <summary>
        /// The IL (byte offset) number the line starts at.
        /// </summary>
        public int ILStartNum;
        /// <summary>
        /// The IL (byte offset) number the line ends at.
        /// </summary>
        public int ILEndNum;
        /// <summary>
        /// The C# line number the line is on.
        /// </summary>
        public int CSLineNum;
    }
}
