#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.ComponentModel;

namespace Kernel.Compiler
{
    /// <summary>
    /// Manages loading root and referenced assemblies and the types 
    /// they declare.
    /// </summary>
    /// <remarks>
    /// Assemblies are loaded into the current execution context so that 
    /// attributes can easily be scanned.
    /// </remarks>
    public class AssemblyManager
    {
        /// <summary>
        /// The dictionary of all the assemblies loaded by the assembly manager.
        /// The key is the assembly’s full name. The value is the loaded assembly.
        /// </summary>
        public Dictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();
        /// <summary>
        /// The list of all the types that don’t contain plugs loaded by the assembly manager.
        /// The key is the type’s full name. The value is the loaded type.
        /// </summary>
        public Dictionary<string, Type> UnpluggedTypes = new Dictionary<string, Type>();
        /// <summary>
        /// The list of all the types that contain plugs loaded by the assembly manager.
        /// The key is the type’s full name. The value is the loaded type.
        /// </summary>
        public Dictionary<string, Type> PluggedTypes = new Dictionary<string, Type>();

        /// <summary>
        /// All of the types (plugged and unplugged) that the assembly manager has loaded.
        /// </summary>
        public List<Type> AllTypes
        {
            get
            {
                return PluggedTypes.Concat(UnpluggedTypes).Select(x => x.Value).ToList();
            }
        }

        /// <summary>
        /// Empty constructor – no need to do anything in constructor as this class has no initialisation.
        /// </summary>
        public AssemblyManager()
        {
        }

        /// <summary>
        /// Loads an assembly from the specified path. The path should be to a valid .Net .DLL or .EXE file.
        /// </summary>
        /// <param name="path">The path to the assembly to load.</param>
        /// <returns>The loaded assembly.</returns>
        /// <exception cref="System.IO.FileNotFoundException">
        /// Thrown if the assembly file specified cannot be found.
        /// </exception>
        public Assembly LoadAssembly(string path)
        {
            Assembly result = null;

            //Check the file actually exists
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Could not find assembly file!", path);
            }
            else
            {
                //Load the assembly
                result = Assembly.LoadFrom(path);
                //Add it to our full list of loaded assemblies
                //Check we haven't already added this assembly
                if(!Assemblies.ContainsKey(result.FullName))
                {
                    Assemblies.Add(result.FullName, result);
                }
            }

            return result;
        }
        /// <summary>
        /// Recursively loads all the referenced assemblies of the specified assembly.
        /// </summary>
        /// <param name="theAssembly">The assembly to load references of.</param>
        /// <remarks>
        /// Ignores any references to the kernel compiler or mscorlib.
        /// </remarks>
        public void LoadReferences(Assembly theAssembly)
        {
            //Get the list of referenced assemblies.
            //It returns a list of names of assemblies not the loaded assemblies themselves,
            //presumably becuase the referenced assemblies haven't actually been loaded.
            AssemblyName[] refAssemblyNames = theAssembly.GetReferencedAssemblies();
            //For each referenced assembly name...
            foreach (AssemblyName aRefName in refAssemblyNames)
            {
                //Check we haven't already loaded this assembly
                // If we have, we don't need to again
                if (!Assemblies.ContainsKey(aRefName.FullName))
                {
                    //Ignore mscorlib
                    //Ignore Kernel.Compiler
                    if(aRefName.FullName.Contains("mscorlib") ||
                       aRefName.FullName.Contains("Kernel.Compiler"))
                    {
                        continue;
                    }

                    //Load the referenced assembly by file path based on theAssembly's location.
                    //Note: Trying to load using full name fails since the referenced assembly won't be in the compiler's
                    //start directory so the CLR won't be able to find the assembly to load.
                    //However, since it is a referenced assembly, it will (or should) be in the same folder as the 
                    //assembly which referenced it. Therefore, we load based of the referenced assembly name using
                    //theAssembly's location as the base path.

                    string refFilePath = Path.Combine(Path.GetDirectoryName(theAssembly.Location), aRefName.Name + ".dll");
                    Assembly result = Assembly.LoadFrom(refFilePath);
                    //Add it to our full list of loaded assemblies
                    Assemblies.Add(result.FullName, result);

                    //Also load its references - recursive.
                    LoadReferences(result);
                }
            }
        }
        /// <summary>
        /// Loads all the modules that are in the loaded the assemblies.
        /// </summary>
        public void LoadAllTypes()
        {
            //Note: By having ignored the reference to mscorlib, 
            //we will not have loaded any standard C# types
            //so we don't need to check for them here

            //Note: If exception is thrown here due to duplicate
            //key additions then we are loading the same type from 
            //two different assemblies. This shouldn't be possible!

            //For each loaded assembly...
            foreach(Assembly anAssembly in Assemblies.Values)
            {
                //Get all the types in that assembly
                Type[] types = anAssembly.GetTypes();
                //For each defined type...
                foreach (Type aType in types)
                {
                    //Check for PluggedClass attribute
                    if (aType.GetCustomAttributes(typeof(PluggedClassAttribute), false).Length > 0)
                    {
                        //Add it to our list of plugged types
                        PluggedTypes.Add(aType.FullName, aType);
                    }
                    else
                    {
                        //Add it to our list of unplugged types
                        UnpluggedTypes.Add(aType.FullName, aType);
                    }
                }
            }
        }
    }
}
