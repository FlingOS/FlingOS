#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
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
    public class Stind : ILOps.Stind
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if the value to store is floating point.
        /// </exception>
        /// <exception cref="System.NotImplementedException">
        /// Thrown if the op is 'StIndRef'.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Pop value
            //Pop address
            //Mov [address], value

            StackItem valueItem = aScannerState.CurrentStackFrame.Stack.Pop();
            StackItem addressItem = aScannerState.CurrentStackFrame.Stack.Pop();
            int bytesToStore = 0;
            bool isFloat = false;

            switch ((OpCodes)anILOpInfo.opCode.Value)
            {
                case OpCodes.Stind_I:
                    bytesToStore = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
                    break;
                case OpCodes.Stind_I1:
                    bytesToStore = 1;
                    break;
                case OpCodes.Stind_I2:
                    bytesToStore = 2;
                    break;
                case OpCodes.Stind_I4:
                    bytesToStore = 4;
                    break;
                case OpCodes.Stind_I8:
                    bytesToStore = 8;
                    break;
                case OpCodes.Stind_R4:
                    bytesToStore = 4;
                    isFloat = true;
                    break;
                case OpCodes.Stind_R8:
                    bytesToStore = 8;
                    isFloat = true;
                    break;
                case OpCodes.Stind_Ref:
                    //SUPPORT - What the hell is this op, how do we support it?
                    throw new NotImplementedException("Stind_Red not supported yet!");
            }

            if(isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Floats not supported yet!");
            }

            if (bytesToStore == 8)
            {
                //Pop value low bits
                result.AppendLine("pop dword eax");
                //Pop value high bits
                result.AppendLine("pop dword edx");

                //Pop address
                result.AppendLine("pop dword ebx");

                //Mov [address], value
                result.AppendLine("mov dword [ebx+4], edx");
                result.AppendLine("mov dword [ebx], eax");
            }
            else if (bytesToStore == 4)
            {
                //Pop value
                result.AppendLine("pop dword eax");

                //Pop address
                result.AppendLine("pop dword ebx");

                //Mov [address], value
                result.AppendLine("mov dword [ebx], eax");
            }
            else if (bytesToStore == 2)
            {
                //Pop value
                result.AppendLine("pop dword eax");

                //Pop address
                result.AppendLine("pop dword ebx");

                //Mov [address], value
                result.AppendLine("mov word [ebx], ax");
            }
            else if (bytesToStore == 1)
            {
                //Pop value
                result.AppendLine("pop dword eax");

                //Pop address
                result.AppendLine("pop dword ebx");

                //Mov [address], value
                result.AppendLine("mov byte [ebx], al");
            }

            return result.ToString().Trim();
        }
    }
}
