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
using System.Windows.Forms;

namespace ASM_Method_Label_Builder
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the method declaration using fully-qualified names:");
            
            Console.Write("Return type: ");
            string returnType = Console.ReadLine().Trim();
            
            Console.Write("Declaring type: ");
            string declaringType = Console.ReadLine().Trim();
            
            Console.Write("Method name: ");
            string methodName = Console.ReadLine().Trim();
            
            Console.WriteLine();
            Console.WriteLine("Param types (one per line. Enter a blank line to complete): ");
            List<string> paramTypes = new List<string>();
            string paramType = "";
            do
            {
                paramType = Console.ReadLine().Trim();
                if (!string.IsNullOrEmpty(paramType))
                {
                    paramTypes.Add(paramType);
                }
            }
            while(!string.IsNullOrEmpty(paramType));
            Console.WriteLine();
            Console.WriteLine();

            string signature = Kernel.Compiler.Utils.GetMethodSignature(returnType, declaringType, methodName, paramTypes.ToArray());
            string id = Kernel.Compiler.Utils.CreateMethodID(signature);
            Console.WriteLine("ASM Label: " + id);

            Console.WriteLine();

            Clipboard.SetText(id);
            Console.WriteLine("Copied to clipboard.");

            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}
