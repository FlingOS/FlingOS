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
using System.Windows.Forms;
using Drivers.Compiler.Types;

namespace ASM_Method_Label_Builder
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
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
            } while (!string.IsNullOrEmpty(paramType));
            Console.WriteLine();
            Console.WriteLine();

            string signature = MethodInfo.GetMethodSignature(returnType, declaringType, methodName, paramTypes.ToArray());
            string id = MethodInfo.CreateMethodID(signature);
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