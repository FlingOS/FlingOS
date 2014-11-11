using System;
using Mosa.Utility.IsoImage;

namespace ISO9660Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Generating ISO...");

            short BootLoadSize = short.Parse(args[0]);
            string IsoFileName = args[1];
            string BootFileName = args[2];
            bool BootInfoTable = bool.Parse(args[3]);
            string FilePath = args[4];

            Options opts = new Options()
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
