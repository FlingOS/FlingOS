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
using System.IO;

namespace Drivers.Compiler.ASM
{
    public static class ASMProcessor
    {
        public static CompileResult Process(ASMLibrary TheLibrary)
        {
            CompileResult result = CompileResult.OK;

            int MaxConcurrentNASMProcesses = Environment.ProcessorCount;
            List<List<ASMBlock>> NASMLabourDivision = new List<List<ASMBlock>>();
            for (int i = 0; i < MaxConcurrentNASMProcesses; i++)
            {
                NASMLabourDivision.Add(new List<ASMBlock>());
            }

            int num = 0;
            foreach (ASMBlock aBlock in TheLibrary.ASMBlocks)
            {
                ProcessBlock(aBlock);

                NASMLabourDivision[num % MaxConcurrentNASMProcesses].Add(aBlock);

                num++;
            }

            for (int i = 0; i < MaxConcurrentNASMProcesses; i++)
            {
                ExecuteNASMAsync(NASMLabourDivision[i]);
            }
            
            return result;
        }

        private static void ProcessBlock(ASMBlock TheBlock)
        {
            string ASMText = "";
            
            if (TheBlock.Plugged)
            {
                //TODO - Load plug file text
            }
            else
            {
                foreach (ASMOp anASMOp in TheBlock.ASMOps)
                {
                    if (anASMOp.RequiresILLabel)
                    {
                        ASMText += TheBlock.GenerateILOpLabel(anASMOp.ILLabelPosition, "") + ":\r\n";
                    }
                    ASMText += anASMOp.Convert(TheBlock) + "\r\n";
                }
            }

            string Name = TheBlock.OriginMethodInfo.ToString();
            string FileName = Utilities.CleanFileName(Name + "." + Options.TargetArchitecture) + ".asm";
            string OutputPath = GetASMOutputPath();
            FileName = Path.Combine(OutputPath, FileName);
            TheBlock.OutputFilePath = FileName;
            File.WriteAllText(FileName, ASMText);
        }

        private static void ExecuteNASMAsync(List<ASMBlock> Blocks)
        {
            //TODO
        }

        private static string GetASMOutputPath()
        {
            string OutputPath = Path.Combine(Options.OutputPath, "ASM");
            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
            }
            return OutputPath;
        }
    }
}
