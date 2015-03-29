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
using System.Reflection;
using System.ComponentModel;

namespace Drivers.Compiler.IL
{
    public class ILLibrary
    {
        public Assembly TheAssembly;

        public ASM.ASMLibrary TheASMLibrary;

        public List<ILLibrary> Dependencies = new List<ILLibrary>();
        
        public List<Types.TypeInfo> TypeInfos = new List<Types.TypeInfo>();

        public Dictionary<Types.MethodInfo, ILBlock> ILBlocks = new Dictionary<Types.MethodInfo, ILBlock>();

        public Dictionary<Type, List<Types.TypeInfo>> SpecialClasses = new Dictionary<Type, List<Types.TypeInfo>>();
        public Dictionary<Type, List<Types.MethodInfo>> SpecialMethods = new Dictionary<Type, List<Types.MethodInfo>>();

        public Types.TypeInfo GetTypeInfo(Type theType, bool FullyProcess = true)
        {
            return GetTypeInfo(theType, true);
        }
        private Types.TypeInfo GetTypeInfo(Type theType, bool topLevel, bool FullyProcess)
        {
            foreach (Types.TypeInfo aTypeInfo in TypeInfos)
            {
                if (aTypeInfo.UnderlyingType.Equals(theType))
                {
                    return aTypeInfo;
                }
            }

            foreach (ILLibrary depLib in Dependencies)
            {
                Types.TypeInfo result = depLib.GetTypeInfo(theType, false, FullyProcess);
                if (result != null)
                {
                    return result;
                }
            }

            if (topLevel)
            {
                Types.TypeInfo theTypeInfo = Types.TypeScanner.ScanType(this, theType);
                if (FullyProcess)
                {
                    Types.TypeScanner.ProcessType(this, theTypeInfo);
                    Types.TypeScanner.ProcessTypeFields(this, theTypeInfo);
                }
                return theTypeInfo;
            }
            else
            {
                return null;
            }
        }

        public override int GetHashCode()
        {
            return TheAssembly.FullName.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if(obj is ILLibrary)
            {
                return this.GetHashCode() == ((ILLibrary)obj).GetHashCode();
            }

            return base.Equals(obj);
        }
        public override string ToString()
        {
            return TheAssembly.FullName;
        }
    }
}
