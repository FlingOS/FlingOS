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

namespace Drivers.Compiler.Types
{
    public class TypeInfo
    {
        public Type UnderlyingType;
        public bool ContainsPlugs;

        public List<FieldInfo> FieldInfos = new List<FieldInfo>();
        public List<MethodInfo> MethodInfos = new List<MethodInfo>();

        public bool Processed = false;
        public bool ProcessedFields = false;
        public bool IsValueType { get { return UnderlyingType.IsValueType; } }
        public bool IsPointer { get { return UnderlyingType.IsPointer; } }

        public int SizeOnStackInBytes { get; set; }
        public int SizeOnHeapInBytes { get; set; }

        public bool IsGCManaged { get; set; }

        public int MethodIDGenerator = 0;

        public string ID
        {
            get
            {
                return "type_" + Utilities.FilterIdentifierForInvalidChars(UnderlyingType.FullName);
            }
        }

        public Types.FieldInfo GetFieldInfo(string FieldName)
        {
            foreach (Types.FieldInfo aFieldInfo in FieldInfos)
            {
                if (aFieldInfo.Name.Equals(FieldName))
                {
                    return aFieldInfo;
                }
            }
            throw new NullReferenceException("Field \"" + FieldName + "\" not found in type \"" + ToString() + "\".");
        }

        public override string ToString()
        {
            return UnderlyingType.AssemblyQualifiedName;
        }
    }
}
