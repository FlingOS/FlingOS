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
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class StackSwitch : ILOps.StackSwitch
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            int dwordsToRotate = anILOpInfo.ValueBytes == null ? 2 : BitConverter.ToInt32(anILOpInfo.ValueBytes, 0);
            
            int bytesShift = 0;
            for (int i = 0; i < dwordsToRotate; i++)
            {
                if (i == 0)
                {
                    GlobalMethods.CheckAddrFromRegister(result, aScannerState, "esp", bytesShift, OpCodes.Unbox);
                    result.AppendLine(string.Format("mov eax, [esp+{0}]", bytesShift));
                    GlobalMethods.CheckAddrFromRegister(result, aScannerState, "esp", bytesShift + 4, OpCodes.Unbox);
                    result.AppendLine(string.Format("mov dword ebx, [esp+{0}]", bytesShift + 4));
                    GlobalMethods.CheckAddrFromRegister(result, aScannerState, "esp", bytesShift, OpCodes.Unbox);
                    result.AppendLine(string.Format("mov dword [esp+{0}], ebx", bytesShift));
                }
                else if (i == dwordsToRotate - 1)
                {
                    GlobalMethods.CheckAddrFromRegister(result, aScannerState, "esp", bytesShift, OpCodes.Unbox);
                    result.AppendLine(string.Format("mov [esp+{0}], eax", bytesShift));
                }
                else
                {
                    GlobalMethods.CheckAddrFromRegister(result, aScannerState, "esp", bytesShift + 4, OpCodes.Unbox);
                    result.AppendLine(string.Format("mov dword ebx, [esp+{0}]", bytesShift + 4));
                    GlobalMethods.CheckAddrFromRegister(result, aScannerState, "esp", bytesShift, OpCodes.Unbox);
                    result.AppendLine(string.Format("mov dword [esp+{0}], ebx", bytesShift));
                }
                bytesShift += 4;
            }

            return result.ToString().Trim();
        }
    }
}
