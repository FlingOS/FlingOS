using System;

using Kernel.FOS_System.Collections;

namespace Kernel.Hardware
{
    /// <summary>
    /// The global device manager for the kernel.
    /// </summary>
    public static class DeviceManager
    {
        /// <summary>
        /// The list of all the devices detected.
        /// </summary>
        /// <remarks>
        /// Some items may be more specific instances of a device so duplicate references to one physical device may 
        /// exist. For example, a PCIDevice instance and a EHCI instance would both exist for one physical EHCI device.
        /// </remarks>
        public static List Devices = new List(20);
    }
}
