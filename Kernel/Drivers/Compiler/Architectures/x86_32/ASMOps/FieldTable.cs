using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drivers.Compiler;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class FieldTable : ASM.ASMFieldTable
    {
        public FieldTable(string currentTypeId, string currentTypeName, List<Tuple<string, string, string>> allFieldInfos, List<Tuple<string, int>> tableEntryFieldInfos)
            : base(currentTypeId, currentTypeName, allFieldInfos, tableEntryFieldInfos)
        {
        }

        public override string Convert(ASM.ASMBlock theBlock)
        {
            StringBuilder ASMResult = new StringBuilder();
            ASMResult.AppendLine("; Field Table - " + CurrentTypeName);
            ASMResult.AppendLine("GLOBAL " + CurrentTypeId + "_FieldTable:data");
            ASMResult.AppendLine(CurrentTypeId + "_FieldTable:");
        
            foreach(Tuple<string, string, string> aFieldInfo in AllFieldInfos)
            {
                string fieldOffsetVal = aFieldInfo.Item1;
                string fieldSizeVal = aFieldInfo.Item2;
                string fieldTypeIdVal = aFieldInfo.Item3;

                foreach(Tuple<string, int> anEntryFieldInfo in TableEntryFieldInfos)
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

            return Text = ASMResult.ToString();
        }
    }
}
