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
    public static class ILPreprocessor
    {
        /* Tasks of the IL Preprocessor:
         *      - Pre-processing for special classes / methods:
         *              - Static constructors
         *      - Pre-scan IL ops to:
         *              - Type Scan any local variable which are of an unscanned types
         *      - Inject general ops (method start, method end, etc.)
         *      - Inject GC IL ops
         *      - Inject wrapping try-finally for GC
         *      - Inject IL ops for try-catch-finally
         */

        public static void Preprocess(ILLibrary TheLibrary)
        {
            if (TheLibrary == null)
            {
                return;
            }

            if (TheLibrary.ILPreprocessed)
            {
                return;
            }
            TheLibrary.ILPreprocessed = true;

            foreach (IL.ILLibrary aDependency in TheLibrary.Dependencies)
            {
                Preprocess(aDependency);
            }
            
            PreprocessSpecialClasses(TheLibrary);
            PreprocessSpecialMethods(TheLibrary);

            foreach (Types.MethodInfo aMethodInfo in TheLibrary.ILBlocks.Keys)
            {
                PreprocessMethodInfo(TheLibrary, aMethodInfo);
                PreprocessILOps(TheLibrary, aMethodInfo, TheLibrary.ILBlocks[aMethodInfo]);
            }

            foreach (Types.MethodInfo aMethodInfo in TheLibrary.ILBlocks.Keys)
            {
                InjectGeneral(aMethodInfo, TheLibrary.ILBlocks[aMethodInfo]);
                InjectGC(aMethodInfo, TheLibrary.ILBlocks[aMethodInfo]);
                InjectTryCatchFinally(aMethodInfo, TheLibrary.ILBlocks[aMethodInfo]);
            }
        }

        private static void PreprocessMethodInfo(ILLibrary TheLibrary, Types.MethodInfo theMethodInfo)
        {
            Types.TypeInfo aTypeInfo = TheLibrary.GetTypeInfo(theMethodInfo.UnderlyingInfo.DeclaringType);
            int ID = GetMethodIDGenerator(TheLibrary, aTypeInfo);
            theMethodInfo.IDValue = ID + 1;
            aTypeInfo.MethodIDGenerator++;
        }
        private static int GetMethodIDGenerator(ILLibrary TheLibrary, Type aType)
        {
            Types.TypeInfo aTypeInfo = TheLibrary.GetTypeInfo(aType);
            return GetMethodIDGenerator(TheLibrary, aTypeInfo);
        }
        private static int GetMethodIDGenerator(ILLibrary TheLibrary, Types.TypeInfo aTypeInfo)
        {
            int totalGen = 0;
            if (aTypeInfo.UnderlyingType.BaseType != null)
            {
                if (!aTypeInfo.UnderlyingType.BaseType.AssemblyQualifiedName.Contains("mscorlib"))
                {
                    totalGen += GetMethodIDGenerator(TheLibrary, aTypeInfo.UnderlyingType.BaseType);
                }
            }
            return totalGen;
        }
        private static void PreprocessILOps(ILLibrary TheLibrary, Types.MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            int totalLocalsOffset = 0;
            foreach (Types.VariableInfo aVarInfo in theMethodInfo.LocalInfos)
            {
                //Causes processing of the type - in case it hasn't already been processed
                Types.TypeInfo aTypeInfo = TheLibrary.GetTypeInfo(aVarInfo.UnderlyingType);
                aVarInfo.TheTypeInfo = aTypeInfo;
                aVarInfo.Offset = totalLocalsOffset;
                totalLocalsOffset += aTypeInfo.SizeOnStackInBytes;
            }

            int totalArgsSize = 0;
            if (!theMethodInfo.IsStatic)
            {
                Types.VariableInfo newVarInfo = new Types.VariableInfo()
                {
                    UnderlyingType = theMethodInfo.UnderlyingInfo.DeclaringType,
                    Position = 0,
                    TheTypeInfo = TheLibrary.GetTypeInfo(theMethodInfo.UnderlyingInfo.DeclaringType)
                };

                theMethodInfo.ArgumentInfos.Add(newVarInfo);

                totalArgsSize += newVarInfo.TheTypeInfo.SizeOnStackInBytes;
            }
            System.Reflection.ParameterInfo[] args = theMethodInfo.UnderlyingInfo.GetParameters();
            foreach (System.Reflection.ParameterInfo argItem in args)
            {
                Types.VariableInfo newVarInfo = new Types.VariableInfo()
                {
                    UnderlyingType = argItem.ParameterType,
                    Position = theMethodInfo.ArgumentInfos.Count,
                    TheTypeInfo = TheLibrary.GetTypeInfo(argItem.ParameterType)
                };

                theMethodInfo.ArgumentInfos.Add(newVarInfo);
                totalArgsSize += newVarInfo.TheTypeInfo.SizeOnStackInBytes;
            }

            System.Reflection.ParameterInfo returnArgItem = (theMethodInfo.IsConstructor ? null : ((System.Reflection.MethodInfo)theMethodInfo.UnderlyingInfo).ReturnParameter);
            if (returnArgItem != null)
            {
                Types.VariableInfo newVarInfo = new Types.VariableInfo()
                {
                    UnderlyingType = returnArgItem.ParameterType,
                    Position = theMethodInfo.ArgumentInfos.Count,
                    TheTypeInfo = TheLibrary.GetTypeInfo(returnArgItem.ParameterType)
                };

                theMethodInfo.ArgumentInfos.Add(newVarInfo);
                totalArgsSize += newVarInfo.TheTypeInfo.SizeOnStackInBytes;
            }

            int offset = totalArgsSize;
            for (int i = 0; i < theMethodInfo.ArgumentInfos.Count; i++)
            {
                offset -= theMethodInfo.ArgumentInfos[i].TheTypeInfo.SizeOnStackInBytes;
                theMethodInfo.ArgumentInfos[i].Offset = offset;
            }
        }
        private static void PreprocessSpecialClasses(ILLibrary TheLibrary)
        {
            //TODO
        }
        private static void PreprocessSpecialMethods(ILLibrary TheLibrary)
        {
            //TODO
        }
        private static void InjectGeneral(Types.MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            //TODO
        }
        private static void InjectGC(Types.MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            //TODO
        }
        private static void InjectTryCatchFinally(Types.MethodInfo theMethodInfo, ILBlock theILBlock)
        {
            //TODO
        }
    }
}
