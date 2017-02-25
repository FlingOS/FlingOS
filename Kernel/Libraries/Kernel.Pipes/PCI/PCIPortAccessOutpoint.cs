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

using System.Data;
using Kernel.Framework;
using Kernel.Framework.Processes.Requests.Pipes;
using Kernel.Utilities;

namespace Kernel.Pipes.PCI
{
    /// <summary>
    ///     Represents an outpoint for a standard in or standard out pipe.
    /// </summary>
    public class PCIPortAccessOutpoint : BasicOutpoint
    {
        /// <summary>
        ///     Creates and registers a PCI Port Access outpoint.
        /// </summary>
        public PCIPortAccessOutpoint(int MaxConnections, bool ReadBackChannel)
            : base(PipeClasses.PCI, ReadBackChannel ? PipeSubclasses.PCI_PortAccess_In : PipeSubclasses.PCI_PortAccess_Out, MaxConnections)
        {
        }

        public void SendCommand(int PipeId, uint Address, bool Read, byte DataSize, uint Data = 0)
        {
            //BasicConsole.WriteLine("PCIPortAccessOutpoint: Sending address: " + (String)Address);
            //BasicConsole.WriteLine("PCIPortAccessOutpoint: Sending data: " + (String)Data);
            byte[] buffer = new byte[10];
            buffer[0] = (byte)(Address >> 0);
            buffer[1] = (byte)(Address >> 8);
            buffer[2] = (byte)(Address >> 16);
            buffer[3] = (byte)(Address >> 24);
            buffer[4] = (byte)(Read ? 1 : 0);
            buffer[5] = DataSize;
            buffer[6] = (byte)(Data >> 0);
            buffer[7] = (byte)(Data >> 8);
            buffer[8] = (byte)(Data >> 16);
            buffer[9] = (byte)(Data >> 24);
            Write(PipeId, buffer, 0, buffer.Length, true);
        }

        public void SendData(int PipeId, uint Data)
        {
            byte[] buffer = new byte[4];
            buffer[0] = (byte)(Data >> 0);
            buffer[1] = (byte)(Data >> 8);
            buffer[2] = (byte)(Data >> 16);
            buffer[3] = (byte)(Data >> 24);
            Write(PipeId, buffer, 0, buffer.Length, false);
        }
    }
}