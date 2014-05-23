using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.IO.Disk
{
    public class EBR : MBR
    {
        public EBR(byte[] aEBR)
            : base()
        {
            ParsePartition(aEBR, 446);
            ParsePartition(aEBR, 462);
        }
    }
}
