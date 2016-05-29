#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //

#endregion

using Kernel.Framework;
using Kernel.Framework.Processes.Requests.Devices;

namespace Kernel.Devices
{
    /// <summary>
    ///     Represents any device connected to the system.
    /// </summary>
    public class Device : Object
    {
        public bool Claimed;
        public DeviceClass Class;
        public DeviceGroup Group;
        public ulong Id;
        public uint[] Info;

        public String Name;
        public uint OwnerProcessId;
        public DeviceSubClass SubClass;

        public Device()
        {
        }

        public Device(DeviceGroup Group, DeviceClass Class, DeviceSubClass SubClass, String Name, uint[] SomeInfo,
            bool IsClaimed)
        {
            this.Group = Group;
            this.Class = Class;
            this.SubClass = SubClass;
            this.Name = Name;
            Info = SomeInfo;
            Claimed = IsClaimed;
            OwnerProcessId = 0;
        }

        public unsafe Device(DeviceDescriptor* descriptor)
        {
            FillDevice(descriptor);
        }

        public unsafe void FillDevice(DeviceDescriptor* descriptor)
        {
            Id = descriptor->Id;
            Group = descriptor->Group;
            Class = descriptor->Class;
            SubClass = descriptor->SubClass;

            int NameLength = 0;
            for (; NameLength < 64; NameLength++)
            {
                if (descriptor->Name[NameLength] == '\0')
                {
                    break;
                }
            }
            Name = String.New(NameLength);
            for (int i = 0; i < NameLength; i++)
            {
                Name[i] = descriptor->Name[i];
            }

            Info = new uint[16];
            for (int i = 0; i < 16; i++)
            {
                Info[i] = descriptor->Info[i];
            }

            Claimed = descriptor->Claimed;
            OwnerProcessId = descriptor->OwnerProcessId;
        }

        public unsafe void FillDeviceDescriptor(DeviceDescriptor* TheDescriptor, bool IncludeInfo)
        {
            TheDescriptor->Id = Id;
            TheDescriptor->Group = Group;
            TheDescriptor->Class = Class;
            TheDescriptor->SubClass = SubClass;

            int j = 0;
            if (Name != null)
            {
                for (; j < Name.Length; j++)
                {
                    TheDescriptor->Name[j] = Name[j];
                }
            }
            for (; j < 64; j++)
            {
                TheDescriptor->Name[j] = '\0';
            }

            if (IncludeInfo && Info != null)
            {
                for (j = 0; j < 16 && j < Info.Length; j++)
                {
                    TheDescriptor->Info[j] = Info[j];
                }
            }
            else
            {
                j = 0;
            }
            for (; j < 16; j++)
            {
                TheDescriptor->Info[j] = 0xFFFFFFFF;
            }

            TheDescriptor->Claimed = Claimed;
            TheDescriptor->OwnerProcessId = OwnerProcessId;
        }
    }
}