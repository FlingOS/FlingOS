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
using System.IO;
using System.Reflection;
using Drivers.Compiler.ASM;
using Drivers.Compiler.ASM.ASMOps;
using Drivers.Compiler.IL;
using Drivers.Compiler.IL.ILOps;

namespace Drivers.Compiler
{
    /// <summary>
    ///     Static class for loading and accessing the target architecture library.
    /// </summary>
    public static class TargetArchitecture
    {
        /// <summary>
        ///     The target architecture library.
        /// </summary>
        /// <remarks>
        ///     Used for loading IL and ASM ops used to convert IL to ASM and ASM to machine code
        ///     for the target architecture.
        /// </remarks>
        private static Assembly TargetArchitectureAssembly;

        public static TargetArchitectureFunctions TargetFunctions;

        /// <summary>
        ///     Map of op codes to IL ops which are loaded from the target architecture.
        /// </summary>
        public static Dictionary<ILOp.OpCodes, ILOp> TargetILOps = new Dictionary<ILOp.OpCodes, ILOp>();

        /// <summary>
        ///     Map of op codes to ASM op classes which are loaded from the target architecture.
        /// </summary>
        public static Dictionary<OpCodes, Type> TargetASMOps = new Dictionary<OpCodes, Type>();

        /// <summary>
        ///     The method start IL op. This is a fake IL op used by the Drivers Compiler.
        /// </summary>
        public static MethodStart MethodStartOp;

        /// <summary>
        ///     The method end IL op. This is a fake IL op used by the Drivers Compiler.
        /// </summary>
        public static MethodEnd MethodEndOp;

        /// <summary>
        ///     The stack switch IL op. This is a fake IL op used by the Drivers Compiler.
        /// </summary>
        public static StackSwitch StackSwitchOp;

        /// <summary>
        ///     Initialises the IL scanner.
        /// </summary>
        /// <remarks>
        ///     Loads the target architecture library.
        /// </remarks>
        /// <returns>True if initialisation was successful. Otherwise, false.</returns>
        public static bool Init()
        {
            bool OK = true;

            OK = LoadTargetArchitecture();

            return OK;
        }

        /// <summary>
        ///     Loads the target architecture library and fills in the TargetILOps, MethodStartOp, MethodEndOp and StackSwitchOp
        ///     fields.
        /// </summary>
        /// <returns>True if fully loaded without error. Otherwise, false.</returns>
        private static bool LoadTargetArchitecture()
        {
            bool OK = false;

            try
            {
                string CurrentAssemblyDir = Path.GetDirectoryName(typeof(TargetArchitecture).Assembly.Location);
                string fileName = null;
                switch (Options.TargetArchitecture)
                {
                    case "x86":
                    {
                        fileName = Path.Combine(CurrentAssemblyDir, @"Drivers.Compiler.Architectures.x86.dll");
                        OK = true;
                    }
                        break;
                    case "mips":
                    {
                        fileName = Path.Combine(CurrentAssemblyDir, @"Drivers.Compiler.Architectures.MIPS32.dll");
                        OK = true;
                    }
                        break;
                    default:
                        OK = false;
                        throw new ArgumentException("Unrecognised target architecture!");
                }

                if (OK)
                {
                    fileName = Path.GetFullPath(fileName);
                    TargetArchitectureAssembly = Assembly.LoadFrom(fileName);
                }

                if (OK)
                {
                    Type[] AllTypes = TargetArchitectureAssembly.GetTypes();
                    foreach (Type aType in AllTypes)
                    {
                        if (aType.IsSubclassOf(typeof(ILOp)))
                        {
                            if (aType.IsSubclassOf(typeof(MethodStart)))
                            {
                                MethodStartOp = (MethodStart)Activator.CreateInstance(aType);
                            }
                            else if (aType.IsSubclassOf(typeof(MethodEnd)))
                            {
                                MethodEndOp = (MethodEnd)Activator.CreateInstance(aType);
                            }
                            else if (aType.IsSubclassOf(typeof(StackSwitch)))
                            {
                                StackSwitchOp = (StackSwitch)Activator.CreateInstance(aType);
                            }
                            else
                            {
                                ILOpTargetAttribute[] targetAttrs =
                                    (ILOpTargetAttribute[])aType.GetCustomAttributes(typeof(ILOpTargetAttribute), true);
                                if (targetAttrs == null || targetAttrs.Length == 0)
                                {
                                    throw new Exception(
                                        "ILScanner could not load target architecture ILOp because target attribute was not specified!");
                                }
                                foreach (ILOpTargetAttribute targetAttr in targetAttrs)
                                {
                                    TargetILOps.Add(targetAttr.Target, (ILOp)Activator.CreateInstance(aType));
                                }
                            }
                        }
                        else if (aType.IsSubclassOf(typeof(ASMOp)))
                        {
                            ASMOpTargetAttribute[] targetAttrs =
                                (ASMOpTargetAttribute[])aType.GetCustomAttributes(typeof(ASMOpTargetAttribute), true);
                            foreach (ASMOpTargetAttribute targetAttr in targetAttrs)
                            {
                                TargetASMOps.Add(targetAttr.Target, aType);
                            }
                        }
                        else if (aType.IsSubclassOf(typeof(TargetArchitectureFunctions)))
                        {
                            TargetFunctions = (TargetArchitectureFunctions)Activator.CreateInstance(aType);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OK = false;
                Logger.LogError(Errors.ILCompiler_LoadTargetArchError_ErrorCode, "", 0,
                    string.Format(Errors.ErrorMessages[Errors.ILCompiler_LoadTargetArchError_ErrorCode],
                        ex.Message));
            }

            return OK;
        }

        public static ASMOp CreateASMOp(OpCodes ASMOpCode, params object[] ConstructorArgs)
        {
            return (ASMOp)Activator.CreateInstance(TargetASMOps[ASMOpCode], ConstructorArgs);
        }
    }

    public abstract class TargetArchitectureFunctions
    {
        public abstract void CleanUpAssemblyCode(ASMBlock TheBlock, ref string ASMText);

        /// <summary>
        ///     Executes the assembly code compiler (e.g. NASM) for the specified file.
        /// </summary>
        /// <param name="inputFilePath">Path to the ASM file to process.</param>
        /// <param name="outputFilePath">Path to output the object file to.</param>
        /// <param name="OnComplete">Handler to call once the external tool has completed. Default: null.</param>
        /// <param name="state">The state object to use when calling the OnComplete handler. Default: null.</param>
        /// <returns>True if execution completed successfully. Otherwise false.</returns>
        public abstract bool ExecuteAssemblyCodeCompiler(string inputFilePath, string outputFilePath,
            VoidDelegate OnComplete = null, object state = null);

        public abstract bool LinkELF(ILLibrary TheLibrary, LinkInformation LinkInfo);
        public abstract bool LinkISO(ILLibrary TheLibrary, LinkInformation LinkInfo);
    }
}