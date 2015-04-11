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
        public StackFrame CurrentStackFrame = new StackFrame();
        
        public int PositionOf(ILOp anOp)
        {
            return Input.PositionOf(anOp);
        }

        public Types.FieldInfo GetFieldInfo(Type aType, string FieldName)
        {
            Types.TypeInfo aTypeInfo = TheILLibrary.GetTypeInfo(aType);
            return TheILLibrary.GetFieldInfo(aTypeInfo, FieldName);
        }
        
    }
}
