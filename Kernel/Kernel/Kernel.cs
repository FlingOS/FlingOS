using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.IO;
using System;

namespace Kernel
{
    /// <summary>
    /// The main class (containing the kernel entry point) for the Fling OS kernel.
    /// </summary>
    [Compiler.PluggedClass]
    public static class Kernel
    {
        /// <summary>
        /// Initialises static stuff within the kernel (such as calling GC.Init and BasicDebug.Init)
        /// </summary>
        [Compiler.NoDebug]
        static Kernel()
        {
            BasicConsole.Init();
            BasicConsole.Clear();

            //Debug.BasicDebug.Init();
            FOS_System.GC.Init();

            BasicConsole.WriteLine();
        }

        /// <summary>
        /// Filled-in by the compiler.
        /// </summary>
        [Compiler.CallStaticConstructorsMethod]
        public static void CallStaticConstructors()
        {
        }

        /// <summary>
        /// Main kernel entry point
        /// </summary>
        [Compiler.KernelMainMethod]
        [Compiler.NoGC]
        [Compiler.NoDebug]
        static unsafe void Main()
        {
            //Necessary for exception handling stuff to work
            //  - We must have an intial, empty handler to always 
            //    "return" to.
            ExceptionMethods.AddExceptionHandlerInfo((void*)0, (void*)0);
            
            BasicConsole.WriteLine("Fling OS Running...");

            try
            {
                Paging.Init();
                
                ManagedMain();
            }
            catch
            {
                BasicConsole.SetTextColour(BasicConsole.error_colour);
                FOS_System.Type currExceptionType = ExceptionMethods.CurrentException._Type;
                if (currExceptionType == (FOS_System.Type)typeof(FOS_System.Exceptions.PageFaultException))
                {
                    BasicConsole.WriteLine("Page fault exception unhandled!");
                }
                else
                {
                    BasicConsole.WriteLine("Startup error! " + ExceptionMethods.CurrentException.Message);
                }
                BasicConsole.WriteLine("Fling OS forced to halt!");
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            BasicConsole.WriteLine("Cleaning up...");
            FOS_System.GC.Cleanup();

            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine(FOS_System.GC.NumObjs);
            BasicConsole.WriteLine(FOS_System.GC.numStrings);
            BasicConsole.SetTextColour(BasicConsole.default_colour);

            BasicConsole.WriteLine("Fling OS Ended.");

            //Necessary - no way of returning from this method since add exception info 
            //            at start cannot be "undone" so stack is "corrupted" if we try
            //            to "ret"
            //So we just halt the CPU for want of a better solution later when ACPI is 
            //implemented.
            Halt();
        }

        /// <summary>
        /// Halts the kernel and halts the CPU.
        /// </summary>
        [Compiler.HaltMethod]
        [Compiler.NoGC]
        public static void Halt()
        {
            if(ExceptionMethods.CurrentException != null)
            {
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
            }

            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine("Kernel halting!");
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            PreReqs.Reset();
        }

        private static Hardware.Devices.DiskDevice disk0;
        /// <summary>
        /// The actual main method for the kernel - by this point, all memory management, exception handling 
        /// etc has been set up properly.
        /// </summary>
        //[Compiler.NoDebug]
        private static void ManagedMain()
        {
            try
            {
                ULongMultiplicationTest();
                //StringConcatTest();
                //ObjectArrayTest();
                //IntArrayTest();
                //DummyObjectTest();
                //DivideByZeroTest();

                //ExceptionsTestP1();

                InitATA();

                try
                {
                    OutputPCIInfo();
                    BasicConsole.DelayOutput(5);
                }
                catch
                {
                    OutputCurrentExceptionInfo();
                }

                InitPCI();

                try
                {
                    OutputATAInfo();
                    BasicConsole.DelayOutput(5);
                }
                catch
                {
                    OutputCurrentExceptionInfo();
                }

                InitFileSystem();

                try
                {
                    OutputFileSystemsInfo();
                    BasicConsole.DelayOutput(20);
                }
                catch
                {
                    OutputCurrentExceptionInfo();
                }

                FOS_System.GC.Cleanup();

                FileSystemMapping A_FSMapping = FileSystemManager.GetMapping("A:/");
                FOS_System.IO.FAT.FATFileSystem A_FS = (FOS_System.IO.FAT.FATFileSystem)A_FSMapping.TheFileSystem;

                Directory P1D2Dir = Directory.Find("A:/P1D2");
                if (P1D2Dir == null)
                {
                    BasicConsole.WriteLine("Creating P1D2 directory...");
                    P1D2Dir = A_FS.NewDirectory("P1D2", A_FS.RootDirectory_FAT32);
                }
                else
                {
                    BasicConsole.WriteLine("Found P1D2 directory.");
                }

                File longNameTestFile = File.Open("A:/P1D2/LongNameTest.txt");
                if (longNameTestFile == null)
                {
                    BasicConsole.WriteLine("Creating LongNameTest.txt file...");
                    longNameTestFile = P1D2Dir.TheFileSystem.NewFile("LongNameTest.txt", P1D2Dir);
                }
                else
                {
                    BasicConsole.WriteLine("Found LongNameTest.txt file.");
                }

                File shortNameTestFile = File.Open("A:/P1D2/ShrtTest.txt");
                if (shortNameTestFile == null)
                {
                    BasicConsole.WriteLine("Creating ShrtTest.txt file...");
                    shortNameTestFile = P1D2Dir.TheFileSystem.NewFile("ShrtTest.txt", P1D2Dir);
                }
                else
                {
                    BasicConsole.WriteLine("Found ShrtTest.txt file.");
                }

                if (longNameTestFile != null)
                {
                    BasicConsole.WriteLine("Opening stream...");
                    FOS_System.IO.Streams.FileStream fileStream = longNameTestFile.GetStream();

                    FOS_System.String testStr = "This is some test file contents.";
                    byte[] testStrBytes = ByteConverter.GetASCIIBytes(testStr);

                    BasicConsole.WriteLine("Writing data...");
                    fileStream.Position = 0;
                    int size = 0;
                    //for (int i = 0; i < 20; i++)
                    {
                        fileStream.Write(testStrBytes, 0, testStrBytes.Length);
                        size += testStrBytes.Length;
                    }

                    BasicConsole.WriteLine("Reading data...");
                    fileStream.Position = 0;
                    byte[] readBytes = new byte[size];
                    fileStream.Read(readBytes, 0, readBytes.Length);
                    FOS_System.String readStr = ByteConverter.GetAsciiString(readBytes, 0u, (uint)readBytes.Length);
                    BasicConsole.WriteLine("\"" + readStr + "\"");

                    OutputDivider();
                }
                else
                {
                    BasicConsole.WriteLine("LongNameTest.txt file not found.");
                }

                if (shortNameTestFile != null)
                {
                    BasicConsole.WriteLine("Opening stream...");
                    FOS_System.IO.Streams.FileStream fileStream = shortNameTestFile.GetStream();

                    FOS_System.String testStr = "This is some test file contents.";
                    byte[] testStrBytes = ByteConverter.GetASCIIBytes(testStr);

                    BasicConsole.WriteLine("Writing data...");
                    fileStream.Position = 0;
                    int size = 0;
                    //for (int i = 0; i < 20; i++)
                    {
                        fileStream.Write(testStrBytes, 0, testStrBytes.Length);
                        size += testStrBytes.Length;
                    }

                    BasicConsole.WriteLine("Reading data...");
                    fileStream.Position = 0;
                    byte[] readBytes = new byte[size];
                    fileStream.Read(readBytes, 0, readBytes.Length);
                    FOS_System.String readStr = ByteConverter.GetAsciiString(readBytes, 0u, (uint)readBytes.Length);
                    BasicConsole.WriteLine("\"" + readStr + "\"");

                    OutputDivider();
                }
                else
                {
                    BasicConsole.WriteLine("ShortNameTest.txt file not found.");
                }
            }
            catch
            {
                OutputCurrentExceptionInfo();
            }

            BasicConsole.WriteLine();
            OutputDivider();
            BasicConsole.WriteLine();
            BasicConsole.WriteLine("End of managed main.");
        }

        /// <summary>
        /// Outputs the current exception information.
        /// </summary>
        [Compiler.NoDebug]
        private static void OutputCurrentExceptionInfo()
        {
            BasicConsole.SetTextColour(BasicConsole.warning_colour);
            BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);

            FOS_System.Type currExceptionType = ExceptionMethods.CurrentException._Type;
            if (currExceptionType == (FOS_System.Type)typeof(FOS_System.Exceptions.ArgumentException))
            {
                BasicConsole.WriteLine(((FOS_System.Exceptions.ArgumentException)ExceptionMethods.CurrentException).ExtendedMessage);
            }

            BasicConsole.SetTextColour(BasicConsole.default_colour);

            ExceptionMethods.CurrentException = null;
        }

        /// <summary>
        /// Tests the exception handling sub-system.
        /// </summary>
        /// <remarks>
        /// If the mechanism appears to work but code in Main() stops working then
        /// it is because one of the GC methods is calling a method / get-set property
        /// that is not marked with [Comnpiler.NoGC]. Make sure all methods that the 
        /// GC calls are marked with [Compiler.NoGC] attribute. See example.
        /// </remarks>
        /// <example>
        /// public int x
        /// {
        ///     [Compiler.NoGC]
        ///     get
        ///     {
        ///         return 0;
        ///     }
        /// }
        /// </example>
        private static void ExceptionsTestP1()
        {
            ExceptionsTestP2();
        }
        /// <summary>
        /// Secondary method used in testing the exception handling sub-system.
        /// </summary>
        private static void ExceptionsTestP2()
        {
            FOS_System.Object obj = new FOS_System.Object();

            try
            {
                ExceptionMethods.Throw(new FOS_System.Exception("An exception."));
            }
            finally
            {
                BasicConsole.WriteLine("Finally ran.");
            }
        }

        /// <summary>
        /// Initialises PCI sub-system.
        /// </summary>
        private static void InitPCI()
        {
            BasicConsole.WriteLine("Initialising PCI...");
            Hardware.PCI.PCI.Init();
            BasicConsole.WriteLine(((FOS_System.String)"PCI initialised. Devices: ") + Hardware.PCI.PCI.Devices.Count);
            OutputDivider();
        }
        /// <summary>
        /// Intialises the ATA sub-system.
        /// </summary>
        private static void InitATA()
        {
            BasicConsole.WriteLine();
            OutputDivider();
            BasicConsole.WriteLine("Initiailsing ATA...");
            Hardware.ATA.ATAManager.Init();
            BasicConsole.WriteLine(((FOS_System.String)"ATA initialised. Devices: ") + Hardware.DeviceManager.Devices.Count);
            OutputDivider();

            disk0 = (Hardware.Devices.DiskDevice)Hardware.DeviceManager.Devices[0];
        }
        /// <summary>
        /// Initialises the file-system.
        /// </summary>
        private static void InitFileSystem()
        {
            BasicConsole.WriteLine("Initialising file system...");
            FOS_System.IO.FileSystemManager.Init();
            BasicConsole.WriteLine(((FOS_System.String)"File system initialised. Mappings: ") + FOS_System.IO.FileSystemManager.FileSystemMappings.Count);
            OutputDivider();
        }

        /// <summary>
        /// Outputs file systems info (including listings for the A:/ drive)
        /// </summary>
        private static void OutputFileSystemsInfo()
        {
            for (int i = 0; i < FileSystemManager.FileSystemMappings.Count; i++)
            {
                FileSystemMapping fsMapping = (FileSystemMapping)FileSystemManager.FileSystemMappings[i];
                if (fsMapping.TheFileSystem._Type == ((FOS_System.Type)typeof(FOS_System.IO.FAT.FATFileSystem)))
                {
                    FOS_System.IO.FAT.FATFileSystem fs = (FOS_System.IO.FAT.FATFileSystem)fsMapping.TheFileSystem;
                    List Listings = fs.GetRootDirectoryTable();

                    if (fs.ThePartition.VolumeID == "[NO ID]")
                    {
                        fs.ThePartition.VolumeID = "TestID";
                        fs.RootDirectory_FAT32.WriteListings();

                        BasicConsole.WriteLine("Set volume ID.");
                    }

                    BasicConsole.WriteLine(((FOS_System.String)"Volume ID: ") + fs.ThePartition.VolumeID);
                        
                    if (fsMapping.Prefix == "A:/")
                    {
                        BasicConsole.WriteLine("Mapping: " + fsMapping.Prefix);
                        OutputDivider();
                        
                        OutputListings(Listings);
                    }
                }
                else
                {
                    BasicConsole.WriteLine("Non-FAT file-system added! (???)");
                }
            }
        }
        /// <summary>
        /// Outputs specified listings.
        /// </summary>
        /// <param name="Listings">The listings to output.</param>
        private static void OutputListings(List Listings)
        {
            for (int j = 0; j < Listings.Count; j++)
            {
                FOS_System.IO.Base xItem = (FOS_System.IO.Base)Listings[j];

                if (xItem._Type == (FOS_System.Type)(typeof(FOS_System.IO.FAT.FATDirectory)))
                {
                    FOS_System.String name = ((FOS_System.IO.FAT.FATDirectory)Listings[j]).Name;
                    BasicConsole.WriteLine(((FOS_System.String)"<DIR> ") + name);

                    OutputListings(((FOS_System.IO.FAT.FATDirectory)Listings[j]).GetListings());
                }
                else if (xItem._Type == (FOS_System.Type)(typeof(FOS_System.IO.FAT.FATFile)))
                {
                    FOS_System.IO.FAT.FATFile file = ((FOS_System.IO.FAT.FATFile)Listings[j]);
                    BasicConsole.WriteLine(((FOS_System.String)"<FILE> ") + file.Name + " (" + file.Size + ")");
                }
            }
        }
        /// <summary>
        /// Outputs the content of the specified file as ASCII text.
        /// </summary>
        /// <param name="fileName">The file to output.</param>
        private static void OutputFileContents(FOS_System.String fileName)
        {
            OutputDivider();
            File aFile = File.Open(fileName);
            if (aFile != null)
            {
                if (aFile.Size > 0)
                {
                    BasicConsole.WriteLine(fileName);

                    FOS_System.IO.Streams.FileStream fileStream = FOS_System.IO.Streams.FileStream.Create(aFile);
                    byte[] xData = new byte[(int)(uint)aFile.Size];
                    int actuallyRead = fileStream.Read(xData, 0, xData.Length);
                    FOS_System.String xText = ByteConverter.GetAsciiString(xData, 0u, (uint)actuallyRead);
                    BasicConsole.WriteLine(xText);
                }
                else
                {
                    BasicConsole.WriteLine("(empty file)");
                }
            }
            else
            {
                BasicConsole.WriteLine("Failed to open file: " + fileName);
            }
            OutputDivider();
        }
        /// <summary>
        /// Outputs the contents of the specified directory as a list of files and sub-directories.
        /// </summary>
        /// <param name="dir">The directory to output.</param>
        private static void OutputDirectoryContents(FOS_System.String dir)
        {
            OutputDivider();
            Directory aDir = Directory.Find(dir);
            if (aDir != null)
            {
                BasicConsole.WriteLine(dir);
                List Listings = aDir.GetListings();
                if (Listings.Count > 0)
                {
                    for (int j = 0; j < Listings.Count; j++)
                    {
                        FOS_System.IO.Base xItem = (FOS_System.IO.Base)Listings[j];

                        if (xItem._Type == (FOS_System.Type)(typeof(FOS_System.IO.FAT.FATDirectory)))
                        {
                            BasicConsole.WriteLine(((FOS_System.String)"<DIR> ") + ((FOS_System.IO.FAT.FATDirectory)Listings[j]).Name);
                        }
                        else if (xItem._Type == (FOS_System.Type)(typeof(FOS_System.IO.FAT.FATFile)))
                        {
                            FOS_System.IO.FAT.FATFile file = ((FOS_System.IO.FAT.FATFile)Listings[j]);
                            BasicConsole.WriteLine(((FOS_System.String)"<FILE> ") + file.Name + " (" + file.Size + ")");
                        }
                    }
                }
                else
                {
                    BasicConsole.WriteLine("(empty directory)");
                }
            }
            else
            {
                BasicConsole.WriteLine("Failed to find directory: " + dir);
            }
            OutputDivider();
        }

        /// <summary>
        /// Outputs a divider line.
        /// </summary>
        private static void OutputDivider()
        {
            BasicConsole.WriteLine("---------------------");
        }
        /// <summary>
        /// Outputs PCI information.
        /// </summary>
        private static void OutputPCIInfo()
        {
            for (int i = 0; i < Hardware.PCI.PCI.Devices.Count; i++)
            {
                Hardware.PCI.PCIDevice aDevice = (Hardware.PCI.PCIDevice)Hardware.PCI.PCI.Devices[i];
                BasicConsole.WriteLine(Hardware.PCI.PCIDevice.DeviceClass.GetString(aDevice));
                BasicConsole.DelayOutput(2);
            }
        }
        /// <summary>
        /// Outputs information about the ATA devices found.
        /// </summary>
        private static void OutputATAInfo()
        {
            int numDrives = 0;
            for (int i = 0; i < Hardware.DeviceManager.Devices.Count; i++)
            {
                Hardware.Device aDevice = (Hardware.Device)Hardware.DeviceManager.Devices[i];
                if (aDevice._Type == (FOS_System.Type)(typeof(Hardware.ATA.ATAPio)))
                {
                    BasicConsole.WriteLine();
                    BasicConsole.WriteLine("ATAPio device found.");
                    Hardware.ATA.ATAPio theATA = (Hardware.ATA.ATAPio)aDevice;

                    OutputDivider();
                    //Info
                    BasicConsole.WriteLine(((FOS_System.String)"Type: ") + (theATA.DriveType == Hardware.ATA.ATAPio.SpecLevel.ATA ? "ATA" : "ATAPI"));
                    BasicConsole.WriteLine(((FOS_System.String)"Serial No: ") + theATA.SerialNo);
                    BasicConsole.WriteLine(((FOS_System.String)"Firmware Rev: ") + theATA.FirmwareRev);
                    BasicConsole.WriteLine(((FOS_System.String)"Model No: ") + theATA.ModelNo);
                    BasicConsole.WriteLine(((FOS_System.String)"Block Size: ") + theATA.BlockSize + " bytes");
                    BasicConsole.WriteLine(((FOS_System.String)"Block Count: ") + theATA.BlockCount);
                    BasicConsole.WriteLine(((FOS_System.String)"Size: ") + ((theATA.BlockCount * theATA.BlockSize) >> 20) + " MB");
                    OutputDivider();

                    BasicConsole.DelayOutput(5);

                    numDrives++;
                }
            }
            
            BasicConsole.WriteLine(((FOS_System.String)"Total # of drives: ") + numDrives);
        }

        /// <summary>
        /// Tests multiplying two 64-bit numbers together
        /// </summary>
        private static void ULongMultiplicationTest()
        {
            ulong a = 0x00000000UL;
            ulong b = 0x00000000UL;
            ulong c = a * b;
            bool test1OK = c == 0x0UL;
            
            a = 0x00000001UL;
            b = 0x00000001UL;
            c = a * b;
            bool test2OK = c == 0x1;
            
            a = 0x00000010UL;
            b = 0x00000010UL;
            c = a * b;
            bool test3OK = c == 0x100UL;
            
            a = 0x10000000UL;
            b = 0x00000010UL;
            c = a * b;
            bool test4OK = c == 0x100000000UL;
            
            a = 0x100000000UL;
            b = 0x00000011UL;
            c = a * b;
            bool test5OK = c == 0x1100000000UL;
            
            a = 0x100000000UL;
            b = 0x100000000UL;
            c = a * b;
            bool test6OK = c == 0x0UL;

            BasicConsole.WriteLine(((FOS_System.String)"Tests OK: ") + test1OK + ", " + test2OK +
                                                                ", " + test3OK + ", " + test4OK +
                                                                ", " + test5OK + ", " + test6OK);
            BasicConsole.DelayOutput(10);
        }
        /// <summary>
        /// Tests dynamic string creation and string concatentation.
        /// </summary>
        private static void StringConcatTest()
        {
            BasicConsole.WriteLine("String concat test...");

            try
            {
                FOS_System.String testStr = FOS_System.String.Concat("test1", " test2");
                BasicConsole.WriteLine(testStr);
            }
            catch
            {
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            BasicConsole.WriteLine("End string concat test.");
        }
        /// <summary>
        /// Tests creating arrays where elements are reference-type and Gc managed.
        /// </summary>
        private static void ObjectArrayTest()
        {
            BasicConsole.WriteLine("Object array test...");

            try
            {
                FOS_System.Object[] objArr = new FOS_System.Object[10];
                objArr[0] = new FOS_System.Object();
                objArr[0]._Type.Size = 5;
                if (objArr[0] != null)
                {
                    BasicConsole.WriteLine("Set object in array success!");
                }
            }
            catch
            {
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            BasicConsole.WriteLine("End object array test.");
        }
        /// <summary>
        /// Tests creating an array of integers (element of type from MSCorLib type and value type)
        /// </summary>
        private static void IntArrayTest()
        {
            BasicConsole.WriteLine("Int array test...");

            try
            {
                int[] testArray = new int[1024];
                testArray[5] = 10;
                int q = testArray[5];
            }
            catch
            {
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            BasicConsole.WriteLine("Int array test.");
        }
        /// <summary>
        /// Tests creating  GC-managed reference-type object and  setting properties and enums.
        /// </summary>
        private static void DummyObjectTest()
        {
            BasicConsole.WriteLine("Dummy object test...");

            try
            {
                Dummy obj = new Dummy();
                new Dummy();
                obj = new Dummy();
                obj.x = obj.x + obj.y;
                if (obj.x == 21)
                {
                    BasicConsole.WriteLine("Addition success!");
                }

                if (obj.testEnum == Dummy.TestEnum.First)
                {
                    BasicConsole.WriteLine("TestEnum.First pre-assigned.");
                }
                obj.testEnum = Dummy.TestEnum.Second;
                if (obj.testEnum == Dummy.TestEnum.Second)
                {
                    BasicConsole.WriteLine("TestEnum.Second assignment worked.");
                }
            }
            catch
            {
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            BasicConsole.WriteLine("Dummy object test.");
        }
        /// <summary>
        /// Tests managed exception sub-system by deliberately causing hardware-level divide-by-zero exception.
        /// </summary>
        private static void DivideByZeroTest()
        {
            BasicConsole.WriteLine("Divide by zero test...");

            try
            {
                int x = 0;
                int y = 0;
                int z = 0;
                z = x / y;
            }
            catch
            {
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                BasicConsole.SetTextColour(BasicConsole.default_colour);

                FOS_System.Type currExceptionType = ExceptionMethods.CurrentException._Type;
                if (currExceptionType == (FOS_System.Type)typeof(FOS_System.Exceptions.DivideByZeroException))
                {
                    BasicConsole.WriteLine("Handled divide by zero exception.");
                }
            }

            BasicConsole.WriteLine("Divide by zero test.");
        }
    }

    public class Dummy : FOS_System.Object
    {
        public enum TestEnum
        {
            First = 1,
            Second = 2,
            Third = 3,
            NULL = 0
        }

        public TestEnum testEnum = TestEnum.First;

        public int x = 10;
        public int y = 11;

        public int Add()
        {
            return x + y;
        }
    }
}
