using Drivers.Compiler.Attributes;
using Log = FlingOops.BasicConsole;

namespace FlingOops.Interfaces
{
    public interface ITest
    {
        int Test();
        int Test2();
    }

    public class TestITest : Object, ITest
    {
        public int Test()
        {
            Log.WriteSuccess("TestITest.Test called.");
            return 1;
        }

        public int Test2()
        {
            Log.WriteSuccess("TestITest.Test2 called.");
            return 2;
        }
    }

    public class TestITest2 : Object, ITest
    {
        public int Test2()
        {
            Log.WriteSuccess("TestITest2.Test2 called.");
            return 4;
        }

        public int Test()
        {
            Log.WriteSuccess("TestITest2.Test called.");
            return 3;
        }
    }

    public static class InterfaceTests
    {
        [NoGC]
        public static void RunTests()
        {
            Log.WriteLine("---Interfaces:");

            TestITest x = new TestITest();
            Log.WriteLine(" TestITest.Test()");
            if (x.Test() == 1)
            {
                Log.WriteSuccess("    Correct call.");
            }
            else
            {
                Log.WriteError("    Incorrect call.");
            }
            Log.WriteLine(" TestITest.Test2()");
            if (x.Test2() == 2)
            {
                Log.WriteSuccess("    Correct call.");
            }
            else
            {
                Log.WriteError("    Incorrect call.");
            }

            ITest y = x;
            Log.WriteLine(" ITest.Test() (TestITest1)");
            if (y.Test() == 1)
            {
                Log.WriteSuccess("    Correct call.");
            }
            else
            {
                Log.WriteError("    Incorrect call.");
            }
            Log.WriteLine(" ITest.Test2()");
            if (y.Test2() == 2)
            {
                Log.WriteSuccess("    Correct call.");
            }
            else
            {
                Log.WriteError("    Incorrect call.");
            }


            TestITest2 z = new TestITest2();
            Log.WriteLine(" TestITest2.Test()");
            if (z.Test() == 3)
            {
                Log.WriteSuccess("    Correct call.");
            }
            else
            {
                Log.WriteError("    Incorrect call.");
            }
            Log.WriteLine(" TestITest2.Test2()");
            if (z.Test2() == 4)
            {
                Log.WriteSuccess("    Correct call.");
            }
            else
            {
                Log.WriteError("    Incorrect call.");
            }

            y = z;
            Log.WriteLine(" ITest.Test() (TestITest2)");
            if (y.Test() == 3)
            {
                Log.WriteSuccess("    Correct call.");
            }
            else
            {
                Log.WriteError("    Incorrect call.");
            }
            Log.WriteLine(" ITest.Test2()");
            if (y.Test2() == 4)
            {
                Log.WriteSuccess("    Correct call.");
            }
            else
            {
                Log.WriteError("    Incorrect call.");
            }

            Log.WriteLine(" ");
        }
    }
}