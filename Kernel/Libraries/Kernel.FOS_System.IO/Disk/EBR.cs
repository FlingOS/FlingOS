using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.IO.Disk
{
    /// <summary>
    /// Represents an extended boot record.
    /// </summary>
    public class EBR : MBR
    {
        /// <summary>
        /// Initializes an EBR from the specified data.
        /// </summary>
        /// <param name="aEBR">The EBR data.</param>
        public EBR(byte[] aEBR)
            : base()
        {
            ParsePartition(aEBR, 446);
            ParsePartition(aEBR, 462);
        }
    }
}
