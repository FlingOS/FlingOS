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
using System.Text;
using Drivers.Compiler.ASM;
using Drivers.Compiler.ASM.ASMOps;
using Drivers.Compiler.Types;

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    public class TypeTable : ASMTypeTable
    {
        public TypeTable(string typeId, string sizeVal, string idVal, string stackSizeVal, string isValueTypeVal,
            string methodTablePointer, string isPointerTypeVal, string baseTypeIdVal, string fieldTablePointer,
            string typeSignatureLiteralLabel, string typeIdLiteralLabel, List<Tuple<string, TypeInfo>> fieldInformation)
            : base(
                typeId, sizeVal, idVal, stackSizeVal, isValueTypeVal, methodTablePointer, isPointerTypeVal,
                baseTypeIdVal, fieldTablePointer, typeSignatureLiteralLabel, typeIdLiteralLabel, fieldInformation)
        {
        }

        public override string Convert(ASMBlock TheBlock)
        {
            StringBuilder ASMResult = new StringBuilder();
            ASMResult.AppendLine(".globl " + TypeId);
            ASMResult.AppendLine(".align 2");
            ASMResult.AppendLine(TypeId + ":");

            foreach (Tuple<string, TypeInfo> aFieldInfo in FieldInformation)
            {
                string allocStr = ASMUtilities.GetAllocStringForSize(
                    aFieldInfo.Item2.IsValueType
                        ? aFieldInfo.Item2.SizeOnHeapInBytes
                        : aFieldInfo.Item2.SizeOnStackInBytes);
                switch (aFieldInfo.Item1)
                {
                    case "Size":
                        ASMResult.AppendLine(allocStr + " " + SizeVal);
                        break;
                    case "Id":
                        ASMResult.AppendLine(allocStr + " " + IdVal);
                        break;
                    case "StackSize":
                        ASMResult.AppendLine(allocStr + " " + StackSizeVal);
                        break;
                    case "IsValueType":
                        ASMResult.AppendLine(allocStr + " " + IsValueTypeVal);
                        break;
                    case "MethodTablePtr":
                        ASMResult.AppendLine(allocStr + " " + MethodTablePointer);
                        break;
                    case "IsPointer":
                        ASMResult.AppendLine(allocStr + " " + IsPointerTypeVal);
                        break;
                    case "TheBaseType":
                        ASMResult.AppendLine(allocStr + " " + BaseTypeIdVal);
                        break;
                    case "FieldTablePtr":
                        ASMResult.AppendLine(allocStr + " " + FieldTablePointer);
                        break;
                    case "Signature":
                        ASMResult.AppendLine(allocStr + " " + TypeSignatureLiteralLabel);
                        break;
                    case "IdString":
                        ASMResult.AppendLine(allocStr + " " + TypeIdLiteralLabel);
                        break;
                }
            }
            ASMResult.AppendLine();

            return ASMResult.ToString();
        }
    }
}