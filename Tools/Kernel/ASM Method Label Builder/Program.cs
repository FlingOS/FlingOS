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
