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

using Kernel.FOS_System.Processes.Requests.Devices;

namespace Kernel.Hardware
{
    /// <summary>
    /// Represents any device connected to the system.
    /// </summary>
    public class Device : FOS_System.Object
    {
        public ulong Id;
        public DeviceGroup Group;
        public DeviceClass Class;
        public DeviceSubClass SubClass;

        public FOS_System.String Name;
        public uint[] Info;

        public bool Claimed;
        public uint OwnerProcessId;
    }
}
