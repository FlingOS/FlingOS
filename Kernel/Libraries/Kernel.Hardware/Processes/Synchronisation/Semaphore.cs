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
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.Processes.Synchronisation;

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
        UInt64List WaitingThreads = new UInt64List();

        public UInt32List OwnerProcesses = new UInt32List(2);

        public Semaphore(int anId, int aLimit)
        {
            id = anId;
            count = limit = aLimit;
        }

        public bool WaitOnBehalf(Process aProcess, Thread aThread)
        {
            ulong identifier = ((UInt64)aProcess.Id << 32) | aThread.Id;
        
            ExclLock.Enter();
            bool notLocked = count > 0;
            if (notLocked)
            {
                count--;
            }
            else
            {
                aThread._EnterSleep(Thread.IndefiniteSleep);
                WaitingThreads.Add(identifier);
            }
            ExclLock.Exit();
            return notLocked;
        }
        public void SignalOnBehalf()
        {
            ExclLock.Enter();
            count++;

            if (WaitingThreads.Count > 0)
            {
                //BasicConsole.WriteLine("Waiting threads > 0");
                ulong identifier = 0;
                do
                {
                    identifier = WaitingThreads[0];
                    WaitingThreads.RemoveAt(0);
                    //BasicConsole.Write("Identifier: ");
                    //BasicConsole.WriteLine(identifier);
                }
                while (!ProcessManager.WakeProcess((uint)(identifier >> 32), (uint)identifier) && WaitingThreads.Count > 0);
            }
            ExclLock.Exit();
        }
        //public void Wait()
        //{
        //    bool locked = false;
        //    bool addedIdentifier = false;
        //    ulong identifier = ((UInt64)ProcessManager.CurrentProcess.Id << 32) | ProcessManager.CurrentThread.Id;
        //    do
        //    {
        //        ExclLock.Enter();
        //        locked = count == 0;
        //        if (!locked)
        //        {
        //            if (addedIdentifier)
        //            {
        //                WaitingThreads.Remove(identifier);
        //            }

        //            count--;
        //            ExclLock.Exit();
        //        }
        //        else
        //        {
        //            if (!addedIdentifier)
        //            {
        //                WaitingThreads.Add(identifier);
        //                addedIdentifier = true;
        //            }

        //            ExclLock.Exit();

        //            Thread.Sleep_Indefinitely();
        //        }
        //    }
        //    while (locked);
        //}
        //public void Signal()
        //{
        //    ExclLock.Enter();
        //    count++;
        //    if (WaitingThreads.Count > 0)
        //    {
        //        ulong identifier = WaitingThreads[0];
        //        while (!ProcessManager.WakeProcess((uint)(identifier >> 32), (uint)identifier))
        //        {
        //            WaitingThreads.RemoveAt(0);
        //            identifier = WaitingThreads[0];
        //        }
        //    }
        //    ExclLock.Exit();
        //}
    }
}
