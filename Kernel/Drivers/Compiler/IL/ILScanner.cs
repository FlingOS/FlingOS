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

namespace Drivers.Compiler.IL
{
    public static class ILScanner
    {
        private static System.Reflection.Assembly TargetArchitectureAssembly = null;
        private static Dictionary<ILOp.OpCodes, ILOp> TargetILOps = new Dictionary<ILOp.OpCodes, ILOp>();
        private static ILOps.MethodStart MethodStartOp;
        private static ILOps.MethodEnd MethodEndOp;
        private static ILOps.StackSwitch StackSwitchOp;

        public static bool Init()
        {
            bool OK = true;

            OK = LoadTargetArchiecture();

            return OK;
        }
        private static bool LoadTargetArchiecture()
        {
            bool OK = false;

            try
            {
                switch (Options.TargetArchitecture)
                {
                    case "x86":
                        {
                            string dir = System.IO.Path.GetDirectoryName(typeof(ILCompiler).Assembly.Location);
                            string fileName = System.IO.Path.Combine(dir, @"Drivers.Compiler.Architectures.x86.dll");
                            fileName = System.IO.Path.GetFullPath(fileName);
                            TargetArchitectureAssembly = System.Reflection.Assembly.LoadFrom(fileName);
                            OK = true;
                        }
                        break;
                    default:
                        OK = false;
                        throw new ArgumentException("Unrecognised target architecture!");
                }

                if (OK)
                {
                    Type[] AllTypes = TargetArchitectureAssembly.GetTypes();
                    foreach (Type aType in AllTypes)
                    {
                        if (aType.IsSubclassOf(typeof(ILOp)))
                        {
                            if (aType.IsSubclassOf(typeof(ILOps.MethodStart)))
                            {
                                MethodStartOp = (ILOps.MethodStart)aType.GetConstructor(new Type[0]).Invoke(new object[0]);
                            }
                            else if (aType.IsSubclassOf(typeof(ILOps.MethodEnd)))
                            {
                                MethodEndOp = (ILOps.MethodEnd)aType.GetConstructor(new Type[0]).Invoke(new object[0]);
                            }
                            else if (aType.IsSubclassOf(typeof(ILOps.StackSwitch)))
                            {
                                StackSwitchOp = (ILOps.StackSwitch)aType.GetConstructor(new Type[0]).Invoke(new object[0]);
                            }
                            else
                            {
                                ILOps.ILOpTargetAttribute[] targetAttrs = (ILOps.ILOpTargetAttribute[])aType.GetCustomAttributes(typeof(ILOps.ILOpTargetAttribute), true);
                                if (targetAttrs == null || targetAttrs.Length == 0)
                                {
                                    throw new Exception("ILScanner could not load target architecture ILOp because target attribute was not specified!");
                                }
                                else
                                {
                                    foreach (ILOps.ILOpTargetAttribute targetAttr in targetAttrs)
                                    {
                                        TargetILOps.Add(targetAttr.Target, (ILOp)aType.GetConstructor(new Type[0]).Invoke(new object[0]));
                                    }
                                }
                            }
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

        public static void Scan(ILLibrary TheLibrary)
        {
            foreach (Types.MethodInfo aMethodInfo in TheLibrary.ILBlocks.Keys)
            {
                ILBlock anILBlock = TheLibrary.ILBlocks[aMethodInfo];
                if (anILBlock.Plugged)
                {
                    ScanPluggedILBlock(TheLibrary, aMethodInfo, anILBlock);
                }
                else
                {
                    ScanNonpluggedILBlock(TheLibrary, aMethodInfo, anILBlock);
                }
            }
        }
        private static void ScanPluggedILBlock(ILLibrary TheLibrary, Types.MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            TheLibrary.TheASMLibrary.ASMBlocks.Add(new ASM.ASMBlock()
            {
                PlugPath = theILBlock.PlugPath,
                OriginMethodInfo = theMethodInfo
            });
        }
        private static void ScanNonpluggedILBlock(ILLibrary TheLibrary, Types.MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            ASM.ASMBlock result = new ASM.ASMBlock()
            {
                OriginMethodInfo = theMethodInfo
            };

            foreach (ILOp anOp in theILBlock.ILOps)
            {
                try
                {
                    ILOp ConverterOp = TargetILOps[(ILOp.OpCodes)anOp.opCode.Value];
                    ILConversionState convState = new ILConversionState()
                    {
                        TheILLibrary = TheLibrary
                    };
                    ConverterOp.Convert(convState, anOp);
                }
                catch (KeyNotFoundException)
                {
                    Logger.LogError(Errors.ILCompiler_ScanILOpFailure_ErrorCode, theMethodInfo.ToString(), anOp.Offset,
                        string.Format(Errors.ErrorMessages[Errors.ILCompiler_ScanILOpFailure_ErrorCode], "Conversion IL op not found."));
                }
                catch (Exception ex)
                {
                    Logger.LogError(Errors.ILCompiler_ScanILOpFailure_ErrorCode, theMethodInfo.ToString(), anOp.Offset,
                        string.Format(Errors.ErrorMessages[Errors.ILCompiler_ScanILOpFailure_ErrorCode], ex.Message));
                }
            }

            TheLibrary.TheASMLibrary.ASMBlocks.Add(result);
        }
    }
}
