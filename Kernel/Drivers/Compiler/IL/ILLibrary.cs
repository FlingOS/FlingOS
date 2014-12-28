using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.IL
{
    public class ILLibrary
    {
        public ASM.ASMLibrary TheASMLibrary;

        public List<ILLibrary> Dependencies = new List<ILLibrary>();
    }
}
