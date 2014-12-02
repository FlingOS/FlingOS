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

        private bool locked = false;
        public bool Locked
        {
            get
            {
                return locked;
            }
        }
        private bool padding = false;

        public SpinLock(int anId)
        {
            id = anId;
        }

        [Compiler.PluggedMethod(ASMFilePath=@"ASM\Processes\Synchronisation\SpinLock")]
        public void Enter()
        {
        }
        [Compiler.PluggedMethod(ASMFilePath=null)]
        public void Exit()
        {
        }
    }
}
