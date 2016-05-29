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

using Drivers.Compiler.Attributes;

namespace Kernel.Framework.Processes.Synchronisation
{
    public class SpinLock : Object
    {
        private readonly int id;

        private readonly ushort locked = 0;

        public int Id
        {
            [NoGC] [NoDebug] get { return id; }
        }

        public bool Locked
        {
            [NoGC] [NoDebug] get { return locked != 0; }
        }

        [NoDebug]
        public SpinLock()
            : this(-1)
        {
        }

        [NoDebug]
        public SpinLock(int anId)
        {
            id = anId;
        }

        [PluggedMethod(ASMFilePath = @"ASM\Processes\Synchronisation\SpinLock")]
        private void _Enter()
        {
        }

        [PluggedMethod(ASMFilePath = null)]
        private void _Exit()
        {
        }

        [NoGC]
        [NoDebug]
        public void Enter()
        {
            //BasicConsole.WriteLine("Entering spin lock...");
            _Enter();
            //BasicConsole.WriteLine("Lock acquired.");
        }

        [NoGC]
        [NoDebug]
        public void Exit()
        {
            //BasicConsole.WriteLine("Exiting spin lock...");
            _Exit();
            //BasicConsole.WriteLine("Released lock.");
        }
    }
}