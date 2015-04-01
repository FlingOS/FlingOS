using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.IL
{
    public class ILPreprocessState
    {
        public ILLibrary TheILLibrary;
        public ILBlock Input;
        public int PositionOf(ILOp anOp)
        {
            return Input.PositionOf(anOp);
        }
    }
}
