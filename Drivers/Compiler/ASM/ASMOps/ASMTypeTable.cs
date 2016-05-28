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
using Drivers.Compiler.Types;

namespace Drivers.Compiler.ASM.ASMOps
{
    [ASMOpTarget(Target = OpCodes.TypeTable)]
    public abstract class ASMTypeTable : ASMOp
    {
        public string BaseTypeIdVal;
        public List<Tuple<string, TypeInfo>> FieldInformation;
        public string FieldTablePointer;
        public string IdVal;
        public string IsPointerTypeVal;
        public string IsValueTypeVal;
        public string MethodTablePointer;
        public string SizeVal;
        public string StackSizeVal;
        public string TypeId;
        public string TypeIdLiteralLabel;
        public string TypeSignatureLiteralLabel;

        public ASMTypeTable(string typeId, string sizeVal, string idVal, string stackSizeVal, string isValueTypeVal,
            string methodTablePointer, string isPointerTypeVal, string baseTypeIdVal, string fieldTablePointer,
            string typeSignatureLiteralLabel, string typeIdLiteralLabel, List<Tuple<string, TypeInfo>> fieldInformation)
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