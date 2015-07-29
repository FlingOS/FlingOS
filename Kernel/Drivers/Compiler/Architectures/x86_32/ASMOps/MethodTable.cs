using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drivers.Compiler;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class MethodTable : ASM.ASMMethodTable
    {
        public MethodTable(string currentTypeId, string currentTypeName, List<Tuple<string, string>> allMethodInfos, List<Tuple<string, int>> tableEntryFieldInfos)
            : base(currentTypeId, currentTypeName, allMethodInfos, tableEntryFieldInfos)
        {
        }

        public override string Convert(ASM.ASMBlock theBlock)
        {
            StringBuilder ASMResult = new StringBuilder();
            ASMResult.AppendLine("; Method Table - " + CurrentTypeName);
            ASMResult.AppendLine("GLOBAL " + CurrentTypeId + "_MethodTable:data");
            ASMResult.AppendLine(CurrentTypeId + "_MethodTable:");

            foreach (Tuple<string, string> aMethodInfo in AllMethodInfos)
            {
                string MethodID = aMethodInfo.Item1;
                string MethodIDValue = aMethodInfo.Item2;

                foreach (Tuple<string, int> anEntryFieldInfo in TableEntryFieldInfos)
                {
                    string allocString = ASMUtilities.GetAllocStringForSize(anEntryFieldInfo.Item2);
                    switch(anEntryFieldInfo.Item1)
                    {
                        case "MethodID":
                            ASMResult.AppendLine(allocString + " " + MethodIDValue);
                            break;
                        case "MethodPtr":
                            ASMResult.AppendLine(allocString + " " + MethodID);
                            break;
                    }
                }
            }


            ASMResult.AppendLine("; Method Table End - " + CurrentTypeName);
            
            return ASMResult.ToString();
        }
    }
}
