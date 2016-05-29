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

using Drivers.Compiler.ASM;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class Mul : ASMOp
    {
        /// <summary>
        ///     Arg (which is Dest for signed mul with src/aux options).
        /// </summary>
        public string Arg;

        /// <summary>
        ///     Optional (only available for signed mul, requires Src)
        /// </summary>
        public string Aux = null;

        public bool Signed = false;

        /// <summary>
        ///     Optional (only available for signed mul)
        /// </summary>
        public string Src = null;

        public override string Convert(ASMBlock TheBlock)
        {
            if (Signed)
            {
                if (!string.IsNullOrWhiteSpace(Aux))
                {
                    return "imul " + Arg + ", " + Src + ", " + Aux;
                }
                if (!string.IsNullOrWhiteSpace(Src))
                {
                    return "imul " + Arg + ", " + Src;
                }
                return "imul " + Arg;
            }
            return "mul " + Arg;
        }
    }
}