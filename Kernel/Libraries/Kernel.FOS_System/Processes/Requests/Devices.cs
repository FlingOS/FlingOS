namespace Kernel.FOS_System.Processes.Requests.Devices
{
    public enum DeviceGroup
    {
        System,
        Storage,
        HID,
        Time,
        Sensors,
        Serial,
        Network,
        USB
    }
    public enum DeviceClass
    {
        CPU,
        Generic,
        IO,
        Keyboard,
        Storage,
        Timer
    }
    public enum DeviceSubClass
    {
        ATA,
        USB,
        PCI,
        Virtual
    }

    public unsafe struct RegisterDeviceRequest
    {
        public ulong Id;
        public DeviceGroup Group;
        public DeviceClass Class;
        public DeviceSubClass SubClass;
        public bool AutoClaim;
    }
}
