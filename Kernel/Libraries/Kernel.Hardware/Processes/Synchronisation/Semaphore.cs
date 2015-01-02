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

namespace Kernel.Hardware.Processes.Synchronisation
{
    public class Semaphore : FOS_System.Object
    {
        protected int id;
        public int Id
        {
            get
            {
                return id;
            }
        }

        protected int count = 0;
        public int Count
        {
            get
            {
                return count;
            }
        }

        protected int limit = 0;
        public int Limit
        {
            get
            {
                return limit;
            }
        }

        SpinLock ExclLock = new SpinLock(-1);

        public Semaphore(int anId, int aLimit)
        {
            id = anId;
            count = limit = aLimit;
        }

        public void Wait()
        {
            bool locked = false;
            do
            {
                ExclLock.Enter();
                locked = count == 0;
                if (!locked)
                {
                    count--;
                    ExclLock.Exit();
                }
                else
                {
                    ExclLock.Exit();
                    Thread.Sleep(5);
                }
            }
            while (locked);
        }
        public void Signal()
        {
            ExclLock.Enter();
            count++;
            ExclLock.Exit();
        }
    }
}
