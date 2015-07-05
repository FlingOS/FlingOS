using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.Testing
{
    public delegate void OutputMessageDel(FOS_System.String TestName, FOS_System.String Message);
    public delegate void OutputWarningDel(FOS_System.String TestName, FOS_System.String message);
    public delegate void OutputErrorDel(FOS_System.String TestName, FOS_System.String message);

    public class Test : FOS_System.Object
    {
    }
}
