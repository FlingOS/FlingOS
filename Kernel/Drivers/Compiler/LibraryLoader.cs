using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Drivers.Compiler.IL;

namespace Drivers.Compiler
{
    public static class LibraryLoader
    {
        private static ILLibrary LoadILLibraryFromFile(string FilePath)
        {
            return null;
        }

        public static ILLibrary LoadILLibrary(string FilePath)
        {
            ILLibrary TheLibrary = LoadILLibraryFromFile(FilePath);

            LoadDependencies(TheLibrary);

            Types.TypeScanner.ScanTypes(TheLibrary);

            return TheLibrary;
        }
        public static int LoadDependencies(ILLibrary aLibrary)
        {
            return 0;
        }
    }
}
