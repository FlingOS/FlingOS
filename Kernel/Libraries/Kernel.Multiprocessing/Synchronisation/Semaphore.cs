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
using Kernel.Framework.Collections;
using Kernel.Framework.Processes.Synchronisation;

namespace Kernel.Multiprocessing.Synchronisation
{
    public class Semaphore : Object
    {
        private readonly SpinLock ExclLock = new SpinLock();
        private readonly UInt64List WaitingThreads = new UInt64List();
        protected int count;
        protected int id;

        protected int limit;

        public UInt32List OwnerProcesses = new UInt32List(2);

        public int Id
        {
            get { return id; }
        }

        public int Count
        {
            get { return count; }
        }

        public int Limit
        {
            get { return limit; }
        }

        public Semaphore(int anId, int aLimit)
        {
            id = anId;
            count = (limit = aLimit) == -1 ? 0 : limit;
        }

        public void Wait()
        {
            ulong identifier = ((ulong)ProcessManager.CurrentProcess.Id << 32) | ProcessManager.CurrentThread.Id;

            ExclLock.Enter();
            bool notLocked = count > 0;
            if (notLocked)
            {
                count--;
                ExclLock.Exit();
            }
            else
            {
                WaitingThreads.Add(identifier);
                ExclLock.Exit();
                ProcessManager.CurrentThread._Sleep_Indefinitely();
            }
        }

        public bool WaitOnBehalf(Process aProcess, Thread aThread)
        {
            ulong identifier = ((ulong)aProcess.Id << 32) | aThread.Id;

            ExclLock.Enter();
            bool notLocked = count > 0;
            if (notLocked)
            {
                count--;
                //String str = "Semaphore not locked: Process/thread identifier: 0x-------- --------";
                //ExceptionMethods.FillString((uint)identifier, 58, str);
                //ExceptionMethods.FillString((uint)(identifier >> 32), 67, str);
                //BasicConsole.WriteLine(str);
            }
            else
            {
                aThread._EnterSleep(Thread.IndefiniteSleep);
                WaitingThreads.Add(identifier);
                //String str = "Semaphore added waiting process/thread identifier: 0x-------- --------";
                //ExceptionMethods.FillString((uint)identifier, 60, str);
                //ExceptionMethods.FillString((uint)(identifier >> 32), 69, str);
                //BasicConsole.WriteLine(str);
            }
            ExclLock.Exit();
            return notLocked;
        }

        public void SignalOnBehalf()
        {
            //BasicConsole.WriteLine("SignalOnBehalf: Enter exclusion lock...");

            ExclLock.Enter();

            //BasicConsole.WriteLine("SignalOnBehalf: Entered.");

            if (WaitingThreads.Count > 0)
            {
                //BasicConsole.WriteLine("Waiting threads > 0");
                ulong identifier = 0;
                do
                {
                    identifier = WaitingThreads[0];
                    WaitingThreads.RemoveAt(0);
                    //String str = "Semaphore next process/thread identifier: 0x-------- --------";
                    //ExceptionMethods.FillString((uint)identifier, 51, str);
                    //ExceptionMethods.FillString((uint)(identifier >> 32), 60, str);
                    //BasicConsole.WriteLine(str);
                } while (!ProcessManager.WakeThread((uint)(identifier >> 32), (uint)identifier) &&
                         WaitingThreads.Count > 0);
            }
            else if (count < limit || limit == -1)
            {
                //String str = "Semaphore no next process/thread";
                //BasicConsole.WriteLine(str);

                count++;
            }

            //BasicConsole.WriteLine("SignalOnBehalf: Exiting exclusion lock...");

            ExclLock.Exit();

            //BasicConsole.WriteLine("SignalOnBehalf: Exited.");
        }
    }
}