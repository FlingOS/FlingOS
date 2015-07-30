using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drivers.Compiler.IL;

namespace Drivers.Compiler
{
    /// <summary>
    /// Static class for loading and accessing the target architecture library.
    /// </summary>
    public static class TargetArchitecture
    {
        /// <summary>
        /// The target architecture library.
        /// </summary>
        /// <remarks>
        /// Used for loading IL and ASM ops used to convert IL to ASM and ASM to machine code
        /// for the target architecture.
        /// </remarks>
        private static System.Reflection.Assembly TargetArchitectureAssembly = null;

        public static TargetArchitectureFunctions TargetFunctions;

        /// <summary>
        /// Map of op codes to IL ops which are loaded from the target architecture.
        /// </summary>
        public static Dictionary<ILOp.OpCodes, ILOp> TargetILOps = new Dictionary<ILOp.OpCodes, ILOp>();
        /// <summary>
        /// Map of op codes to ASM op classes which are loaded from the target architecture.
        /// </summary>
        public static Dictionary<ASM.OpCodes, Type> TargetASMOps = new Dictionary<ASM.OpCodes, Type>();

        /// <summary>
        /// The method start IL op. This is a fake IL op used by the Drivers Compiler.
        /// </summary>
        public static IL.ILOps.MethodStart MethodStartOp;
        /// <summary>
        /// The method end IL op. This is a fake IL op used by the Drivers Compiler.
        /// </summary>
        public static IL.ILOps.MethodEnd MethodEndOp;
        /// <summary>
        /// The stack switch IL op. This is a fake IL op used by the Drivers Compiler.
        /// </summary>
        public static IL.ILOps.StackSwitch StackSwitchOp;
        
        /// <summary>
        /// Initialises the IL scanner.
        /// </summary>
        /// <remarks>
        /// Loads the target architecture library.
        /// </remarks>
        /// <returns>True if initialisation was successful. Otherwise, false.</returns>
        public static bool Init()
        {
            bool OK = true;

            OK = LoadTargetArchiecture();

            return OK;
        }
        /// <summary>
        /// Loads the target architecture library and fills in the TargetILOps, MethodStartOp, MethodEndOp and StackSwitchOp
        /// fields.
        /// </summary>
        /// <returns>True if fully loaded without error. Otherwise, false.</returns>
        private static bool LoadTargetArchiecture()
        {
            bool OK = false;

            try
            {
                string CurrentAssemblyDir = System.IO.Path.GetDirectoryName(typeof(TargetArchitecture).Assembly.Location);
                string fileName = null;
                switch (Options.TargetArchitecture)
                {
                    case "x86":
                        {
                            fileName = System.IO.Path.Combine(CurrentAssemblyDir, @"Drivers.Compiler.Architectures.x86.dll");
                            OK = true;
                        }
                        break;
                    case "mips":
                        {
                            fileName = System.IO.Path.Combine(CurrentAssemblyDir, @"Drivers.Compiler.Architectures.MIPS32.dll");
                            OK = true;
                        }
                        break;
                    default:
                        OK = false;
                        throw new ArgumentException("Unrecognised target architecture!");
                }

                if (OK)
                {
                    fileName = System.IO.Path.GetFullPath(fileName);
                    TargetArchitectureAssembly = System.Reflection.Assembly.LoadFrom(fileName);
                }
                
                if (OK)
                {
                    Type[] AllTypes = TargetArchitectureAssembly.GetTypes();
                    foreach (Type aType in AllTypes)
                    {
                        if (aType.IsSubclassOf(typeof(ILOp)))
                        {
                            if (aType.IsSubclassOf(typeof(IL.ILOps.MethodStart)))
                            {
                                MethodStartOp = (IL.ILOps.MethodStart)Activator.CreateInstance(aType);
                            }
                            else if (aType.IsSubclassOf(typeof(IL.ILOps.MethodEnd)))
                            {
                                MethodEndOp = (IL.ILOps.MethodEnd)Activator.CreateInstance(aType);
                            }
                            else if (aType.IsSubclassOf(typeof(IL.ILOps.StackSwitch)))
                            {
                                StackSwitchOp = (IL.ILOps.StackSwitch)Activator.CreateInstance(aType);
                            }
                            else
                            {
                                IL.ILOps.ILOpTargetAttribute[] targetAttrs = (IL.ILOps.ILOpTargetAttribute[])aType.GetCustomAttributes(typeof(IL.ILOps.ILOpTargetAttribute), true);
                                if (targetAttrs == null || targetAttrs.Length == 0)
                                {
                                    throw new Exception("ILScanner could not load target architecture ILOp because target attribute was not specified!");
                                }
                                else
                                {
                                    foreach (IL.ILOps.ILOpTargetAttribute targetAttr in targetAttrs)
                                    {
                                        TargetILOps.Add(targetAttr.Target, (ILOp)Activator.CreateInstance(aType));
                                    }
                                }
                            }
                        }
                        else if (aType.IsSubclassOf(typeof(ASM.ASMOp)))
                        {
                            ASM.ASMOpTargetAttribute[] targetAttrs = (ASM.ASMOpTargetAttribute[])aType.GetCustomAttributes(typeof(ASM.ASMOpTargetAttribute), true);
                            foreach (ASM.ASMOpTargetAttribute targetAttr in targetAttrs)
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

        public static ASM.ASMOp CreateASMOp(ASM.OpCodes ASMOpCode, params object[] ConstructorArgs)
        {
            return (ASM.ASMOp)Activator.CreateInstance(TargetASMOps[ASMOpCode], ConstructorArgs);
        }
    }

    public abstract class TargetArchitectureFunctions
    {
        public abstract void CleanUpAssemblyCode(ASM.ASMBlock TheBlock, ref string ASMText);

        /// <summary>
        /// Executes the assembly code compiler (e.g. NASM) for the specified file.
        /// </summary>
        /// <param name="inputFilePath">Path to the ASM file to process.</param>
        /// <param name="outputFilePath">Path to output the object file to.</param>
        /// <param name="OnComplete">Handler to call once the external tool has completed. Default: null.</param>
        /// <param name="state">The state object to use when calling the OnComplete handler. Default: null.</param>
        /// <returns>True if execution completed successfully. Otherwise false.</returns>
        public abstract bool ExecuteAssemblyCodeCompiler(string inputFilePath, string outputFilePath, VoidDelegate OnComplete = null, object state = null);
    }
}
