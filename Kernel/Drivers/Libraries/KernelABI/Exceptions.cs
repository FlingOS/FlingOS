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
