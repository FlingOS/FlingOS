#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.Processes.Synchronisation
{
    [Compiler.PluggedClass]
    public class SpinLock : FOS_System.Object
    {
        private int id;
        public int Id
        {
            get
            {
                return id;
            }
        }

        private UInt16 locked = 0;
        public bool Locked
        {
            get
            {
                return locked != 0;
            }
        }

        public SpinLock(int anId)
        {
            id = anId;
        }

        [Compiler.PluggedMethod(ASMFilePath=@"ASM\Processes\Synchronisation\SpinLock")]
        private void _Enter()
        {
        }
        [Compiler.PluggedMethod(ASMFilePath=null)]
        private void _Exit()
        {
        }

        public void Enter()
        {
            BasicConsole.WriteLine("Entering spin lock...");
            //_Enter();
            BasicConsole.WriteLine("Lock acquired.");
        }
        public void Exit()
        {
            BasicConsole.WriteLine("Exiting spin lock...");
            //_Exit();
            BasicConsole.WriteLine("Released lock.");
        }
    }
}
