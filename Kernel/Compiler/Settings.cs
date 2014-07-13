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

namespace Kernel.Compiler
{
    /// <summary>
    /// Represents the compiler's settings.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// The setting key for the setting “InputFile”. The “InputFile” setting is used to specify the path to the .DLL to compile.
        /// </summary>
        public const string InputFileKey = "inputfile";
        /// <summary>
        /// The setting key for the setting “Help”. The “Help” setting can be set to “on” or “off”. “on” indicates the help text should be outputted. “off” is the default value and indicates the help text should not be outputted.
        /// </summary>
        public const string HelpKey = "help";
        /// <summary>
        /// The setting key for the setting “TargetArchitecture”. The “TargetArchitecture” setting is used to specify the target architecture to compile for.
        /// </summary>
        public const string TargetArchitectureKey = "targetarchitecture";
        /// <summary>
        /// The setting key for the setting “OuptutFile”. The “OuptutFile” setting is used to specify the path to the .ASM file to output.
        /// </summary>
        public const string OutputFileKey = "outputfile";
        /// <summary>
        /// The setting key for the setting “Kernel_Main_Method”. The “Kernel_Main_Method” setting is used to specify the label name of the kernel's main method.
        /// </summary>
        public const string KernelMainMethodKey = "kernel_main_method";
        /// <summary>
        /// The setting key for the setting “Call_Static_Constructors_Method”. The “Call_Static_Constructors_Method” setting is used to specify the label name of the kernel's method that calls all the static constructors.
        /// </summary>
        public const string CallStaticConstructorsMethodKey = "call_static_constructors_method";
        /// <summary>
        /// The setting key for the setting “ToolsPath”. The “ToolsPath” setting is used to specify the path to the tool apps folder.
        /// </summary>
        public const string ToolsPathKey = "toolspath";
        /// <summary>
        /// The setting key for the setting “DebugBuild”. The “DebugBuild” setting is used to specify the compiler should do a debug build or not.
        /// </summary>
        public const string DebugBuildKey = "debugbuild";

        /// <summary>
        /// The underlying dictionary of all the settings (arguments) stored.
        /// </summary>
        Dictionary<string, string> allArgs = new Dictionary<string, string>();

        /// <summary>
        /// Whether to do a debug or release build.
        /// </summary>
        public bool DebugBuild
        {
            get
            {
                return this[DebugBuildKey] != null; 
            }
            set
            {
                this[DebugBuildKey] = value.ToString();
            }
        }

        /// <summary>
        /// Constructor for Settings - creates empty settings instance.
        /// </summary>
        public Settings()
        {
        }
        /// <summary>
        /// Constructor for Settings takes a list of arguments and parses.
        /// </summary>
        /// <param name="aArgs">The list of arguments to parse.</param>
        public Settings(string[] aArgs)
        {
            //Parse the passed arguments
            ParseArgs(aArgs);
        }

        /// <summary>
        /// Parses a list of arguments and adds them to the dictionary.
        /// </summary>
        /// <param name="args">The args to parse.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the first argument is invalid.
        /// </exception>
        public void ParseArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string[] argParts = args[i].Split('=');
                //If there is only a value for this argument
                if (argParts.Length == 1)
                {
                    //If the current arg is the first arg, allow it to just be a file path
                    if (i == 0)
                    {
                        allArgs.Add(InputFileKey, argParts[0]);
                    }
                    else
                    {
                        throw new ArgumentException("Invalid argument! Use only \"" + HelpKey + "=on\" for help.");
                    }
                }
                else
                {
                    //Use first part as the key
                    //The remainder of the string (using Substring method) as the value (in case the value contains an equals symbol)
                    string key = argParts[0].ToLower();
                    //+1 to exclude the equals symbol
                    string val = args[i].Substring(argParts[0].Length + 1);
                    if(key == HelpKey)
                    {
                        val = val.ToLower();
                    }
                    allArgs.Add(key, val);
                }
            }
        }

        /// <summary>
        /// Checks all the arguments are valid.
        /// </summary>
        /// <returns>True if all args are valid. Otherwise, false.</returns>
        public bool CheckIfArgsValid()
        {
            bool OK = true;
            try
            {
                foreach(KeyValuePair<string, string> aVal in allArgs)
                {
                    if(aVal.Key == InputFileKey)
                    {
                        if(!File.Exists(aVal.Value))
                        {
                            OK = false;
                            break;
                        }
                    }
                    else if(aVal.Key == HelpKey)
                    {
                        if(aVal.Value != "on" && aVal.Value != "off")
                        {
                            OK = false;
                            break;
                        }
                    }
                    else if (aVal.Key == ToolsPathKey)
                    {
                        if (!Directory.Exists(aVal.Value))
                        {
                            OK = false;
                            break;
                        }
                    }
                }
            }
            catch
            {
                OK = false;
            }
            return OK;
        }
        /// <summary>
        /// Checks all the required arguments are present.
        /// </summary>
        /// <returns>True if all required args are present. Otherwise, false.</returns>
        public bool CheckForRequiredArgs()
        {
            //Assume they are all present
            bool OK = true;
            //If arg is present, this will result to true. Repeating this line format means when one fails, all fails.
            //Checking OK && TEST is more efficient than TEST && OK becuase it will check OK first and fail immediately if OK == false
            OK = OK && allArgs.ContainsKey(InputFileKey);
            OK = OK && allArgs.ContainsKey(OutputFileKey);
            OK = OK && allArgs.ContainsKey(TargetArchitectureKey);
            OK = OK && allArgs.ContainsKey(ToolsPathKey);
            return OK;
        }

        /// <summary>
        /// Gets or sets a setting by key.
        /// </summary>
        /// <param name="key">The key of the setting to retrieve.</param>
        /// <returns>Null if key not present, value otherwise.</returns>
        public string this[string key]
        {
            get
            {
                if(allArgs.ContainsKey(key))
                {
                    return allArgs[key];
                }
                return null;
            }
            set
            {
                if(allArgs.ContainsKey(key))
                {
                    allArgs[key] = value;
                }
                else
                {
                    allArgs.Add(key, value);
                }
            }
        }
    }
}
