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
