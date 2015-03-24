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
        public static string PageFaultDetectionMethod = "method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___";
        public static bool PageFaultDetectionEnabled = false;

        public static void InsertPageFaultDetection(StringBuilder result, ILScannerState aScannerState, string reg, int offset, ILOps.ILOp.OpCodes opCode)
        {
            if (PageFaultDetectionEnabled)
            {
                result.AppendLine("call " + PageFaultDetectionMethod);
            }
        }
    }
}
