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
using Mosa.Utility.IsoImage;

namespace ISO9660Generator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Generating ISO...");
            Console.WriteLine("Args: " + string.Join(", ", args));

            short BootLoadSize = short.Parse(args[0]);
            string IsoFileName = args[1];
            string BootFileName = args[2];
            bool BootInfoTable = bool.Parse(args[3]);
            string FilePath = args[4];

            Options opts = new Options
            {
                BootLoadSize = BootLoadSize,
                IsoFileName = IsoFileName,
                BootFileName = BootFileName,
                BootInfoTable = BootInfoTable
            };

            opts.IncludeFiles.Add(FilePath);
            Iso9660Generator generator = new Iso9660Generator(opts);
            generator.Generate();

            Console.WriteLine("ISO generated.");
        }
    }
}