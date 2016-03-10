namespace Kernel.FOS_System.Processes.Requests.Devices
{
    public enum DeviceGroup
    {
        System,
        Storage,
        HIDs,
        Timers,
        Sensors,
        Serial,
        Network,
        USB,
        Unkown
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
        Virtual,
        Timer,
        Clock
    }

    public unsafe struct DeviceDescriptor
    {
        public ulong Id;
        public DeviceGroup Group;
        public DeviceClass Class;
        public DeviceSubClass SubClass;
        
        public fixed char Name[64];
        public fixed uint Info[16];

        public bool Claimed;
        public uint OwnerProcessId;
    }
    
}
