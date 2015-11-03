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
    
using System;
using Kernel.Processes;
using Kernel.FOS_System;

namespace Kernel.Pipes.Standard
{
    public class StandardOutpoint : BasicOutpoint
    {
        public StandardOutpoint(bool OutputPipe)
            : base(PipeClasses.Standard, (OutputPipe ? PipeSubclasses.Standard_Out : PipeSubclasses.Standard_In), PipeConstants.UnlimitedConnections)
        {
        }

        public unsafe void Write(int PipeId, char Character, bool blocking)
        {
            byte[] data = new byte[1] { (byte)Character };
            base.Write(PipeId, data, 0, data.Length, blocking);
        }
        public unsafe void Write(int PipeId, FOS_System.String message, bool blocking)
        {
            byte[] data = ByteConverter.GetASCIIBytes(message);
            base.Write(PipeId, data, 0, data.Length, blocking);
        }
    }
}
