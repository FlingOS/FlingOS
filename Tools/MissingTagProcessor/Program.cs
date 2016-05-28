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
using System.IO;

namespace MissingTagProcessor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Missing argument: Path to read unprocessed errors file.");
                Console.ReadLine();
                return;
            }
            if (args.Length < 2)
            {
                Console.WriteLine("Missing argument: Path to save processed errors file.");
                Console.ReadLine();
                return;
            }
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Invalid argument: Unprocessed errors file not found.");
                Console.ReadLine();
                return;
            }
            if (File.Exists(args[1]))
            {
                File.Delete(args[1]);
            }

            string[] lines = File.ReadAllLines(args[0]);
            List<string> outputLines = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("Warning"))
                {
                    string[] lineParts = line.Split('\t');
                    string infoPart = lineParts[2];
                    if (infoPart.StartsWith("BuildAssembler : warning : ShowMissingComponent: "))
                    {
                        infoPart = infoPart.Replace("BuildAssembler : warning : ShowMissingComponent: ", "");

                        int componentNameEnd = infoPart.IndexOf("Missing ");
                        string componentType = infoPart.Substring(1, 1);
                        string componentName = infoPart.Substring(3, componentNameEnd - 5);

                        infoPart = infoPart.Substring(componentNameEnd, infoPart.Length - componentNameEnd);
                        infoPart = infoPart.Substring(0, infoPart.IndexOf('[') - 1);

                        string tagType = infoPart.Substring(infoPart.IndexOf(' ') + 1,
                            infoPart.LastIndexOf(' ') - (infoPart.IndexOf(' ') + 1));

                        switch (componentType)
                        {
                            case "N":
                                componentType = "Namespace";
                                break;
                            case "T":
                                componentType = "Type";
                                break;
                            case "M":
                                componentType = "Method";
                                break;
                            case "F":
                                componentType = "Field";
                                break;
                            case "P":
                                componentType = "Property";
                                break;
                        }
                        outputLines.Add(componentType + "\t" + componentName + "\t" + tagType);
                    }
                }
            }

            File.WriteAllLines(args[1], outputLines);
            Console.WriteLine("Successful!");
        }
    }
}