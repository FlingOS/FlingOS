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
    
#define MSD_TRACE
//#undef MSD_TRACE

using System;
using Kernel.FOS_System.Collections;
using Kernel.Hardware.Devices;
using Kernel.Utilities;

namespace Kernel.Hardware.USB.Devices
{
    public static class MassStorageDevice_Consts
    {
        public const uint CSWMagicNotOK = 0x01010101;
        public const uint CSWMagicOK = 0x53425355; // USBS
        public const uint CBWMagic = 0x43425355; // USBC
    }
    public class MassStorageDevice : USBDevice
    {
        protected MassStorageDevice_DiskDevice diskDevice;

        public MassStorageDevice(USBDeviceInfo aDeviceInfo)
            : base(aDeviceInfo)
        {
            diskDevice = new MassStorageDevice_DiskDevice(this);

#if MSD_TRACE
            BasicConsole.WriteLine("------------------------------ Mass Storage Device -----------------------------");
            BasicConsole.WriteLine(((FOS_System.String)"MSD Interface num: ") + DeviceInfo.MSD_InterfaceNum);
            BasicConsole.DelayOutput(5);
#endif

            Setup();
        }

        protected void Setup()
        {
          //  testMSD(device); // test with some SCSI commands
        }

        public override void Destroy()
        {
            diskDevice.Destroy();
            diskDevice = null;

            base.Destroy();
        }
    }
    public class MassStorageDevice_DiskDevice : DiskDevice
    {
        MassStorageDevice msd;

        public MassStorageDevice_DiskDevice(MassStorageDevice anMSD)
        {
            msd = anMSD;
            DeviceManager.Devices.Add(this);

            //TODO - Init BlockDevice fields with data from USB stick
        }

        public override void ReadBlock(ulong aBlockNo, uint aBlockCount, byte[] aData)
        {
            ExceptionMethods.Throw(new FOS_System.Exceptions.NotSupportedException("MSD_DiskDevice.ReadBlock not implemented yet!"));
        }
        public override void WriteBlock(ulong aBlockNo, uint aBlockCount, byte[] aData)
        {
            ExceptionMethods.Throw(new FOS_System.Exceptions.NotSupportedException("MSD_DiskDevice.WriteBlock not implemented yet!"));
        }

        public void Destroy()
        {
            DeviceManager.Devices.Remove(this);
            msd = null;
        }
    }
    
    public unsafe struct usb_CommandBlockWrapper
    {
        public uint CBWSignature;
        public uint CBWTag;
        public uint CBWDataTransferLength;
        public byte CBWFlags;
        public byte CBWLUN;           // only bits 3:0
        public byte CBWCBLength;      // only bits 4:0
        public fixed byte commandByte[16];
    }
}
