using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.Processes.Synchronisation
{
    public class Mutex : Semaphore
    {
        public Mutex(int anId)
            : base(1, anId)
        {
        }
    }
}
