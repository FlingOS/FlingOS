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
    /// The USB Request structure used for sending USB requests under the USB protocol.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct USBRequest
    {
        /// <summary>
        /// The USB request type.
        /// </summary>
        public byte type;
        /// <summary>
        /// The specific USB request.
        /// </summary>
        public byte request;
        /// <summary>
        /// The USB request lo-val.
        /// </summary>
        public byte valueLo;
        /// <summary>
        /// The USB request hi-val.
        /// </summary>
        public byte valueHi;
        /// <summary>
        /// The request index.
        /// </summary>
        public ushort index;
        /// <summary>
        /// The length of the request.
        /// </summary>
        public ushort length;
    }
}
