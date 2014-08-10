#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
using System;

namespace Kernel.Hardware.USB
{
    /// <summary>
    /// The types of USB transaction.
    /// </summary>
    public enum USBTransactionType
    {
        /// <summary>
        /// Indicates the transaction is a SETUP transaction.
        /// </summary>
        SETUP,
        /// <summary>
        /// Indicates the transaction is an IN transaction.
        /// </summary>
        IN,
        /// <summary>
        /// Indicates the transaction is an OUT transaction.
        /// </summary>
        OUT
    }
    /// <summary>
    /// Represents a transaction from the high-level USB perspective.
    /// </summary>
    public class USBTransaction : FOS_System.Object
    {
        /// <summary>
        /// The implementation-specific transaction that can actually be sent by a specific host controller type.
        /// </summary>
        public HCIs.HCTransaction underlyingTz;
        /// <summary>
        /// The type of the transaction.
        /// </summary>
        public USBTransactionType type;
    }
}
