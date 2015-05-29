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

namespace Drivers.Compiler.IL
{
    public class ILBlock
    {
        public Types.MethodInfo TheMethodInfo;

        public string PlugPath = null;
        public bool Plugged { get { return PlugPath != null; } }

        public List<ILOp> ILOps = new List<ILOp>();
        
        //public ILOp Next(ILOp current)
        //{
        //    int index = ILOps.IndexOf(current);
        //    if (index + 1 < ILOps.Count)
        //    {
        //        return ILOps[index + 1];
        //    }
        //    return null;
        //}
        public int PositionOf(ILOp anOp)
        {
            return ILOps.IndexOf(anOp);
        }
        public ILOp At(int offset)
        {
            List<ILOp> potOps = (from ops in ILOps
                                 where ops.Offset == offset
                                 select ops).ToList();
            if (potOps.Count > 0)
            {
                return potOps.OrderBy(x => ILOps.IndexOf(x)).First();
            }
            return null;
        }

        public List<ExceptionHandledBlock> ExceptionHandledBlocks = new List<ExceptionHandledBlock>();
        public ExceptionHandledBlock GetExactExceptionHandledBlock(int Offset)
        {
            List<ExceptionHandledBlock> potBlocks = (from blocks in ExceptionHandledBlocks
                                                     where (blocks.Offset == Offset)
                                                     select blocks)
                                                     .ToList();
            if (potBlocks.Count > 0)
            {
                return potBlocks.First();
            }
            return null;
        }
        public ExceptionHandledBlock GetExceptionHandledBlock(int Offset)
        {
            List<ExceptionHandledBlock> potBlocks = (from blocks in ExceptionHandledBlocks
                                                     where (
                                                     (blocks.Offset <= Offset &&
                                                      blocks.Offset + blocks.Length >= Offset)
                                                     || (from catchBlocks in blocks.CatchBlocks
                                                         where (
                                                           catchBlocks.Offset <= Offset &&
                                                           catchBlocks.Offset + catchBlocks.Length >= Offset
                                                         )
                                                         select catchBlocks).Count() > 0
                                                     || (from finallyBlocks in blocks.FinallyBlocks
                                                         where (
                                                            finallyBlocks.Offset <= Offset &&
                                                            finallyBlocks.Offset + finallyBlocks.Length >= Offset
                                                         )
                                                         select finallyBlocks).Count() > 0
                                                     )
                                                     select blocks).OrderByDescending(x => x.Offset)
                                                     .ToList();
            if (potBlocks.Count > 0)
            {
                return potBlocks.First();
            }
            return null;
        }
    }
}
