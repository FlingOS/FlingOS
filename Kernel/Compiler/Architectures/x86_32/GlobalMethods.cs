#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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

namespace Kernel.Compiler.Architectures.x86_32
{
    public static class GlobalMethods
    {
        public static string CheckAddrTestAddress = "0xF000FF69"/*"0xA5F000C8"*/;
        public static string CheckAddrDisplayMethod = "method_System_Void_RETEND_Kernel_Kernel_DECLEND_OutputAddressDetectedMethod_NAMEEND__System_UInt32_System_UInt32_";
        public static int CheckAddrCount = 0;
        
        public static void CheckAddrFromRegister(StringBuilder result, ILScannerState aScannerState, string reg, int offset, ILOps.ILOp.OpCodes opCode)
        {
            if(!string.IsNullOrWhiteSpace(CheckAddrTestAddress))
            {
                string jumpToLabel = string.Format("{0}.IL_{1}_0",
                    aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                    "TestAddress_NotEqual_" + CheckAddrCount++);

                result.AppendLine("pushad");
                result.AppendLine(string.Format("mov eax, {0}", reg));
                if (offset > 0)
                {
                    result.AppendLine(string.Format("add eax, {0}", offset));
                }

                result.AppendLine(string.Format("cmp eax, {0}", CheckAddrTestAddress));
                result.AppendLine("jne " + jumpToLabel);

                result.AppendLine("call GetEIP");
                result.AppendLine(string.Format("push dword {0} ; OpCode:{0}={1}", (int)opCode, System.Enum.GetName(typeof(ILOps.ILOp.OpCodes), opCode)));
                result.AppendLine("call " + CheckAddrDisplayMethod);
                result.AppendLine("add esp, 4");
                
                result.AppendLine(jumpToLabel + ":");
                result.AppendLine("popad");
            }
        }
    }
}
