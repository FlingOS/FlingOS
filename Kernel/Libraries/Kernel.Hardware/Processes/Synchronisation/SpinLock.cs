using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.Processes.Synchronisation
{
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

        private bool locked = false;
        public bool Locked
        {
            get
            {
                return locked;
            }
        }

        public SpinLock(int anId)
        {
            id = anId;
        }

        public void Enter()
        {
        }
        public void Exit()
        {
        }
    }
}
