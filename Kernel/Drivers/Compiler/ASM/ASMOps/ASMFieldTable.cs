using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.ASM
{
    [ASMOpTarget(Target = OpCodes.FieldTable)]
    public abstract class ASMFieldTable : ASMOp
    {
        public string CurrentTypeId;
        public string CurrentTypeName;
        public List<Tuple<string, string, string>> AllFieldInfos;
        public List<Tuple<string, int>> TableEntryFieldInfos;

        public ASMFieldTable(string currentTypeId, string currentTypeName, List<Tuple<string, string, string>> allFieldInfos, List<Tuple<string, int>> tableEntryFieldInfos)
        {
            CurrentTypeId = currentTypeId;
            CurrentTypeName = currentTypeName;
            AllFieldInfos = allFieldInfos;
            TableEntryFieldInfos = tableEntryFieldInfos;
        }
    }
}
