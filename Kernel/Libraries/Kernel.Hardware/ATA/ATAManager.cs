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
    /// Provides methods for managing ATA access.
    /// </summary>
    public static class ATAManager
    {
        /// <summary>
        /// ATA primary IO device.
        /// </summary>
        private static readonly ATAIO ATAIO1 = new ATAIO(false);
        /// <summary>
        /// ATA secondary IO device.
        /// </summary>
        private static readonly ATAIO ATAIO2 = new ATAIO(true);

        /// <summary>
        /// Initialises all available ATA devices on the primary bus.
        /// </summary>
        public static void Init()
        {
            //Try to initialise primary IDE:ATA drives.
            InitDrive(ATA.ControllerID.Primary, ATA.BusPosition.Slave);
            InitDrive(ATA.ControllerID.Primary, ATA.BusPosition.Master);
        }

        /// <summary>
        /// Initialises a particular drive on the ATA bus.
        /// </summary>
        /// <param name="ctrlId">The controller ID of the device.</param>
        /// <param name="busPos">The bus position of the device.</param>
        public static void InitDrive(ATA.ControllerID ctrlId, ATA.BusPosition busPos)
        {
            ATAIO theIO = ctrlId == ATA.ControllerID.Primary ? ATAIO1 : ATAIO2;
            ATAPio theATAPio = new ATAPio(theIO, ctrlId, busPos);
            if (theATAPio.DriveType != ATAPio.SpecLevel.Null)
            {
                DeviceManager.Devices.Add(theATAPio);
            }
        }
    }
}
