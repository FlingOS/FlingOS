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

namespace Kernel.Hardware.USB.HCIs
{
    /// <summary>
    /// Represents a generic USB Host Controller Interface.
    /// </summary>
    public abstract class HCI : Device
    {
        /// <summary>
        /// The underlying PCI device for the host controller.
        /// </summary>
        protected PCI.PCIDeviceNormal pciDevice;

        /// <summary>
        /// Initializes a new generic host controller interface using the specified PCI device.
        /// </summary>
        /// <param name="aPCIDevice">The PCI device that represents the HCI device.</param>
        public HCI(PCI.PCIDeviceNormal aPCIDevice)
            : base()
        {
            pciDevice = aPCIDevice;
        }
    }
}
