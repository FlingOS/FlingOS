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
using Kernel.Framework.Processes.Requests.Pipes;
using Kernel.Utilities;

namespace Kernel.Pipes.PCI
{
    /// <summary>
    ///     Represents an inpoint for a standard in or standard out pipe.
    /// </summary>
    public unsafe class PCIPortAccessInpoint : BasicInpoint
    {
        /// <summary>
        ///     The buffer to use when reading commands from the pipe.
        /// </summary>
        protected byte[] ReadBuffer;

        /// <summary>
        ///     Creates and connects a new PCI Port Access pipe to the target process.
        /// </summary>
        /// <param name="aOutProcessId">The target process Id.</param>
        public PCIPortAccessInpoint(uint aOutProcessId, bool ReadBackChannel)
            : base(aOutProcessId, PipeClasses.PCI, ReadBackChannel ? PipeSubclasses.PCI_PortAccess_In : PipeSubclasses.PCI_PortAccess_Out, sizeof(uint)*30 /*3 commands or 30 data items*/)
        {
            ReadBuffer = new byte[BufferSize];
        }

        public bool ReadCommand(out uint Address, out bool Read, out byte DataSize, out uint Data)
        {
            int bytesRead = base.Read(ReadBuffer, 0, 10, true);
            if (bytesRead == 10)
            {
                Address = (uint)ReadBuffer[0] | ((uint)ReadBuffer[1] << 8) | ((uint)ReadBuffer[2] << 16) |
                          ((uint)ReadBuffer[3] << 24);
                Read = ReadBuffer[4] == 1;
                DataSize = ReadBuffer[5];
                Data = (uint)ReadBuffer[6] | ((uint)ReadBuffer[7] << 8) | ((uint)ReadBuffer[8] << 16) |
                       ((uint)ReadBuffer[9] << 24);

                //BasicConsole.WriteLine("PCIPortAccessInpoint: Read address: " + (String)Address);
                //BasicConsole.WriteLine("PCIPortAccessInpoint: Read data: " + (String)Data);

                return true;
            }
            Address = 0;
            Read = true;
            DataSize = 0;
            Data = 0;
            return false;
        }

        public bool ReadData(out uint Data)
        {
            int bytesRead = base.Read(ReadBuffer, 0, 4, true);
            if (bytesRead > 0)
            {
                Data = (uint)ReadBuffer[0] | ((uint)ReadBuffer[1] << 8) | ((uint)ReadBuffer[2] << 16) |
                          ((uint)ReadBuffer[3] << 24);
                return true;
            }
            Data = 0;
            return false;
        }
    }
}