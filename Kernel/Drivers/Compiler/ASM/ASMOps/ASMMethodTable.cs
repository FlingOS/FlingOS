using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.ASM
{
    [ASMOpTarget(Target = OpCodes.MethodTable)]
    public abstract class ASMMethodTable : ASMOp
    {
        public string CurrentTypeId;
        public string CurrentTypeName;
        public List<Tuple<string, string>> AllMethodInfos;
        public List<Tuple<string, int>> TableEntryFieldInfos;

        public ASMMethodTable(string currentTypeId, string currentTypeName, List<Tuple<string, string>> allMethodInfos, List<Tuple<string, int>> tableEntryFieldInfos)
        {
            CurrentTypeId = currentTypeId;
            CurrentTypeName = currentTypeName;
            AllMethodInfos = allMethodInfos;
            TableEntryFieldInfos = tableEntryFieldInfos;
        }
    }
}
