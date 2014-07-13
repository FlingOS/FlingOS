#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.ATA
{
    /// <summary>
    /// Represents an ATA device.
    /// </summary>
    public abstract class ATA : Devices.DiskDevice
    {
        /// <summary>
        /// The ATA controller IDs.
        /// </summary>
        public enum ControllerID
        { 
            /// <summary>
            /// Primary ATA controller.
            /// </summary>
            Primary,
            /// <summary>
            /// Secondary ATA controller.
            /// </summary>
            Secondary 
        }
        /// <summary>
        /// The ATA bus positions.
        /// </summary>
        public enum BusPosition
        { 
            /// <summary>
            /// Master device.
            /// </summary>
            Master,
            /// <summary>
            /// Slave device.
            /// </summary>
            Slave 
        }

        /// <summary>
        /// The device's controller ID.
        /// </summary>
        public ControllerID controllerId;
        /// <summary>
        /// The device's bus position.
        /// </summary>
        public BusPosition busPosition;

        /// <summary>
        /// Initialises a new ATA device with block size 512.
        /// </summary>
        internal ATA()
        {
            blockSize = 512;
        }
    }
}
