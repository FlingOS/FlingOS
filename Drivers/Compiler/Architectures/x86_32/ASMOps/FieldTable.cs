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

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class FieldTable : ASMFieldTable
    {
        public FieldTable(string currentTypeId, string currentTypeName,
            List<Tuple<string, string, string>> allFieldInfos, List<Tuple<string, int>> tableEntryFieldInfos)
            : base(currentTypeId, currentTypeName, allFieldInfos, tableEntryFieldInfos)
        {
        }

        public override string Convert(ASMBlock TheBlock)
        {
            StringBuilder ASMResult = new StringBuilder();
            ASMResult.AppendLine("; Field Table - " + CurrentTypeName);
            ASMResult.AppendLine("GLOBAL " + CurrentTypeId + "_FieldTable:data");
            ASMResult.AppendLine(CurrentTypeId + "_FieldTable:");

            foreach (Tuple<string, string, string> aFieldInfo in AllFieldInfos)
            {
                string fieldOffsetVal = aFieldInfo.Item1;
                string fieldSizeVal = aFieldInfo.Item2;
                string fieldTypeIdVal = aFieldInfo.Item3;

                foreach (Tuple<string, int> anEntryFieldInfo in TableEntryFieldInfos)
                {
                    string allocStr = ASMUtilities.GetAllocStringForSize(anEntryFieldInfo.Item2);
                    switch (anEntryFieldInfo.Item1)
                    {
                        case "Offset":
                            ASMResult.AppendLine(allocStr + " " + fieldOffsetVal);
                            break;
                        case "Size":
                            ASMResult.AppendLine(allocStr + " " + fieldSizeVal);
                            break;
                        case "FieldType":
                            ASMResult.AppendLine(allocStr + " " + fieldTypeIdVal);
                            break;
                    }
                }
            }
            ASMResult.AppendLine("; Field Table End - " + CurrentTypeName);

            return ASMResult.ToString();
        }
    }
}