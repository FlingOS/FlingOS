using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.ATA
{
    public static class ATAManager
    {
        private static readonly ATAIO ATAIO1 = new ATAIO(false);
        private static readonly ATAIO ATAIO2 = new ATAIO(true);

        public static void Init()
        {
            //Try to initialise primary (S)ATA drives.
            InitDrive(ATA.ControllerId.Primary, ATA.BusPosition.Slave);
            InitDrive(ATA.ControllerId.Primary, ATA.BusPosition.Master);
        }

        public static void InitDrive(ATA.ControllerId ctrlId, ATA.BusPosition busPos)
        {
            ATAIO theIO = ctrlId == ATA.ControllerId.Primary ? ATAIO1 : ATAIO2;
            ATAPio theATAPio = new ATAPio(theIO, ctrlId, busPos);
            if (theATAPio.DriveType != ATAPio.SpecLevel.Null)
            {
                DeviceManager.Devices.Add(theATAPio);
            }
        }
    }
}
