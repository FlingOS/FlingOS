using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.Processes.Synchronisation
{
    public class Mutex : FOS_System.Object
    {
        protected int id;
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
        private bool padding = false;

        public Mutex(int anId)
        {
            id = anId;
        }

        [Compiler.PluggedMethod(ASMFilePath=@"ASM\Processes\Synchronisation\Mutex")]
        public void Enter()
        {
        }
        [Compiler.PluggedMethod(ASMFilePath=null)]
        public void Exit()
        {
        }
    }
}
