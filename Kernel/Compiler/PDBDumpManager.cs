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
using Kernel.Debug.Data;

namespace Kernel.Compiler
{
    /// <summary>
    /// Manages reading the ObjDump file generated from a C# debug database (.pdb) file.
    /// </summary>
    public class PDBDumpManager
    {
        /// <summary>
        /// The assembly manager used by the PDB Dump manager.
        /// </summary>
        AssemblyManager TheAssemblyManager;

        /// <summary>
        /// All the loaded symbols from the PDB file.
        /// </summary>
        public Dictionary<string, PDB_SymbolInfo> Symbols = new Dictionary<string, PDB_SymbolInfo>();

        /// <summary>
        /// Initialises a new PDB Dump Manager loading the specified DLL
        /// as the root assembly of the assembly manager.
        /// </summary>
        /// <param name="rootDLLFileName">The path to the DLL to use as the root assembly.</param>
        public PDBDumpManager(string rootDLLFileName)
        {
            TheAssemblyManager = new AssemblyManager();
            var rootAssembly = TheAssemblyManager.LoadAssembly(rootDLLFileName);
            TheAssemblyManager.LoadReferences(rootAssembly);
            Init();
        }
        /// <summary>
        /// Initialises a new PDB Dump Manager using the specified assembly
        /// manager.
        /// </summary>
        /// <param name="anAssemblyManager">The assembly manager to use.</param>
        public PDBDumpManager(AssemblyManager anAssemblyManager)
        {
            TheAssemblyManager = anAssemblyManager;
            Init();
        }
        /// <summary>
        /// Initialise the PDB Dump manager (called by constructors for you).
        /// Loads all the pdb dumps for the root assembly and referenced assemblies.
        /// </summary>
        public void Init()
        {
            foreach (var anAssembly in TheAssemblyManager.Assemblies.Values)
            {
                string assemblyFileName = Path.Combine(Path.GetDirectoryName(anAssembly.Location), Path.GetFileNameWithoutExtension(anAssembly.Location));
                string aPDBDumpPathname = assemblyFileName + ".pdb_dump";

                PDBDumpReader theReader = new PDBDumpReader(aPDBDumpPathname);
                Symbols = Symbols.Concat(theReader.Symbols).ToDictionary(x => x.Key, y => y.Value);
            }
        }
    }
}
