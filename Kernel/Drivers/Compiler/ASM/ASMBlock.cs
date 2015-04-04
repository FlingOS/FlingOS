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

namespace Drivers.Compiler.ASM
{
    public class ASMBlock
    {
        public Types.MethodInfo OriginMethodInfo;
        public string PlugPath = null;
        public bool Plugged { get { return PlugPath != null; } }

        public string OutputFilePath;

        public List<ASMOp> ASMOps = new List<ASMOp>();
        public List<string> ExternalLabels = new List<string>();
        public List<string> GlobalLabels = new List<string>();

        public long Priority = 0;

        public void Append(ASMOp anOp)
        {
            ASMOps.Add(anOp);
        }

        public void AddExternalLabel(string label)
        {
            ExternalLabels.Add(label);
        }
        public void AddGlobalLabel(string label)
        {
            GlobalLabels.Add(label);
        }

        public string GenerateMethodLabel()
        {
            if (OriginMethodInfo != null)
            {
                return GenerateLabel(OriginMethodInfo.ID);
            }
            else
            {
                return null;
            }
        }
        public string GenerateILOpLabel(int ILPosition, string Extension = null)
        {
            return GenerateLabel(null, ILPosition, Extension);
        }

        public static string GenerateLabel(string MethodID, int ILPosition = int.MinValue, string Extension = null)
        {
            if (ILPosition != int.MinValue)
            {
                if (!string.IsNullOrWhiteSpace(MethodID))
                {
                    if (!string.IsNullOrWhiteSpace(Extension))
                    {
                        return string.Format("{0}.IL_{1}_{2}", MethodID, ILPosition.ToString("X2"), Extension);
                    }

                    return string.Format("{0}.IL_{1}", MethodID, ILPosition.ToString("X2"));
                }

                if (!string.IsNullOrWhiteSpace(Extension))
                {
                    return string.Format(".IL_{0}_{1}", ILPosition.ToString("X2"), Extension);
                }
                
                return string.Format(".IL_{0}", ILPosition.ToString("X2"));
            }

            return MethodID;
        }
    }
}
