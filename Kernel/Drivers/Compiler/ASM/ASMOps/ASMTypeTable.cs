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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.ASM
{
    [ASMOpTarget(Target=OpCodes.TypeTable)]
    public abstract class ASMTypeTable : ASMOp
    {
        public string TypeId;
        public string SizeVal;
        public string IdVal;
        public string StackSizeVal;
        public string IsValueTypeVal;
        public string MethodTablePointer;
        public string IsPointerTypeVal;
        public string BaseTypeIdVal;
        public string FieldTablePointer;
        public string TypeSignatureLiteralLabel;
        public string TypeIdLiteralLabel;
        public List<Tuple<string, Types.TypeInfo>> FieldInformation;

        public ASMTypeTable(string typeId, string sizeVal, string idVal, string stackSizeVal, string isValueTypeVal, string methodTablePointer, string isPointerTypeVal, string baseTypeIdVal, string fieldTablePointer, string typeSignatureLiteralLabel, string typeIdLiteralLabel, List<Tuple<string, Types.TypeInfo>> fieldInformation)
        {
            TypeId = typeId;
            SizeVal = sizeVal;
            IdVal = idVal;
            StackSizeVal = stackSizeVal;
            IsValueTypeVal = isValueTypeVal;
            MethodTablePointer = methodTablePointer;
            IsPointerTypeVal = isPointerTypeVal;
            BaseTypeIdVal = baseTypeIdVal;
            FieldTablePointer = fieldTablePointer;
            TypeSignatureLiteralLabel = typeSignatureLiteralLabel;
            TypeIdLiteralLabel = typeIdLiteralLabel;
            FieldInformation = fieldInformation;
        }
    }
}
