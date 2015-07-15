using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KernelABI
{
    public static class Exceptions
    {
        [Drivers.Compiler.Attributes.AddExceptionHandlerInfoMethod]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static unsafe void AddExceptionHandlerInfo(
            void* handlerPtr,
            void* filterPtr)
        {
        }

        [Drivers.Compiler.Attributes.ExceptionsHandleLeaveMethod]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static unsafe void HandleLeave(void* continuePtr)
        {
        }

        [Drivers.Compiler.Attributes.ExceptionsHandleEndFinallyMethod]
        [Drivers.Compiler.Attributes.NoDebug]
        [Drivers.Compiler.Attributes.NoGC]
        public static unsafe void HandleEndFinally()
        {
        }
    }
}
