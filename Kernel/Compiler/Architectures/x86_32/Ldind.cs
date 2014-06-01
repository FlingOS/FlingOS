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
    public class Ldind : ILOps.Ldind
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

            //Load indirect
            //Pop address
            //Push [address]

            StackItem addressItem = aScannerState.CurrentStackFrame.Stack.Pop();
            int bytesToLoad = 0;

            switch ((OpCodes)anILOpInfo.opCode.Value)
            {
                case OpCodes.Ldind_U1:
                case OpCodes.Ldind_I1:
                    bytesToLoad = 1;
                    break;
                case OpCodes.Ldind_U2:
                case OpCodes.Ldind_I2:
                    bytesToLoad = 2;
                    break;
                case OpCodes.Ldind_U4:
                case OpCodes.Ldind_I4:
                    bytesToLoad = 4;
                    break;
                case OpCodes.Ldind_I8:
                    bytesToLoad = 8;
                    break;
            }

            //Pop address
            result.AppendLine("pop dword ebx");

            if (bytesToLoad == 1)
            {
                result.AppendLine("xor eax, eax");
                result.AppendLine("mov byte al, [ebx]");
                result.AppendLine("push dword eax");
            }
            else if (bytesToLoad == 2)
            {
                result.AppendLine("xor eax, eax");
                result.AppendLine("mov word ax, [ebx]");
                result.AppendLine("push dword eax");
            }
            else if (bytesToLoad == 4)
            {
                result.AppendLine("push dword [ebx]");
            }
            else if (bytesToLoad == 8)
            {
                result.AppendLine("push dword [ebx+4]");
                result.AppendLine("push dword [ebx]");
            }

            aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                sizeOnStackInBytes = bytesToLoad == 8 ? 8 : 4,
                isFloat = false
            });

            return result.ToString().Trim();
        }
    }
}
