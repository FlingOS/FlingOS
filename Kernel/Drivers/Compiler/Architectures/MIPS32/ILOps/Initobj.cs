using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Initobj : IL.ILOps.Initobj
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
        }

        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        public override void Convert(ILConversionState conversionState, ILOp theOp) 
        {
            //Ignore for now
            //TODO: Do we need to do any proper initialisation?
            conversionState.Append(new ASMOps.Add() { Src1 = "4", Src2 = "$sp", Dest = "$sp", Unsigned = false});
        }
    }
}
