using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Semaphore(int anId, int aLimit)
        {
            id = anId;
            count = limit = aLimit;
        }

        public bool Wait()
        {
            return false;
        }
        public void Signal()
        {
        }
    }
}
