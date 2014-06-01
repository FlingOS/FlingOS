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
