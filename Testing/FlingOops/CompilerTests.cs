using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NoGCAttribute = Drivers.Compiler.Attributes.NoGCAttribute;
using Log = FlingOops.BasicConsole;

namespace FlingOops
{
    /// <summary>
    /// This class contains behavioural tests of the compiler.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class contains what are intended to be behavioural tests of the FlingOS Compiler
    /// IL Op to ASM ops conversions. Unfortunately, even the most basic test has to use a
    /// significant number of IL ops just to be able to output something useful.
    /// </para>
    /// <para>
    /// As a result, the tests provided cannot be run in an automated fashion. The compiler
    /// tester/developer will need to select which tests to run, execute them and observe
    /// the output to determine if it has worked or not. If not, manual debugging will be
    /// required.
    /// </para>
    /// <para>
    /// Instead of altering the test itself, a copy should be made to an architecture-specific
    /// test class and modifications be made to the copy. Once testing is complete and the bug
    /// has been fixed, it should be documented thoroughly for future reference. The architecture
    /// specific test class should then be removed.
    /// </para>
    /// </remarks>

    /// <summary>
    /// Test struct for testing structs.
    /// </summary>
    public struct AStruct
    {
        public byte a;      // 1 byte - 1 byte on heap
        public short b;     // 2 bytes - 2 bytes on heap
        public int c;       // 4 bytes - 4 bytes on heap
        public long d;      // 8 bytes - 8 bytes on heap
                            // Total : 15 bytes
    }

    public static class CompilerTests
    {
        /// <summary>
        /// Executes all the tests in the CompilerTests class.
        /// </summary>
        [NoGC]
        public static void RunTests()
        {
            Test_AddUInt32_Zero_Zero();
            Test_Sizeof_Struct();
            Test_Instance_Struct();
            Test_Locals_And_Pointers();
            Test_ModUInt32_9_3();
            Test_ModUInt32_10_3();
            Test_ModInt32_Neg9_3();
            Test_ModInt32_9_Neg3();
            Test_ModInt32_Neg9_Neg3();
            Test_ModInt32_9_3();

            Log.WriteLine("Tests completed.");
        }

        /// <summary>
        /// Tests: Adding two UInt32s, 
        /// Inputs: 0, 0, 
        /// Result: 0
        /// </summary>
        [NoGC]
        public static void Test_AddUInt32_Zero_Zero()
        {
            uint x = 0;
            uint y = 0;
            uint z = x + y;
            if (z == 0)
            {
                Log.WriteSuccess("Test_AddUInt32_Zero_Zero okay.");
            }
            else
            {
                Log.WriteError("Test_AddUInt32_Zero_Zero not okay.");
            }
        }

        /// <summary>
        /// Tests: Sizeof a struct in bytes, 
        /// Inputs: AStruct, 
        /// Result: Sum of the individual elements of the struct in bytes (e.g.: byte = 1, short = 2, int = 4, long = 8)
        /// </summary>
        [NoGC]
        public static unsafe void Test_Sizeof_Struct()
        {
            int size = sizeof(AStruct);
            if (size == 15)
            {
                Log.WriteSuccess("Test_Sizeof_Struct okay.");
            }
            else
            {
                Log.WriteError("Test_Sizeof_Struct not okay.");
            }
        }

        /// <summary>
        /// Tests: Elements of a new instance of a struct stored and read correctly, 
        /// Inputs: AStruct, 
        /// Result: Values declared for each element
        /// </summary>
        [NoGC]
        public static void Test_Instance_Struct()
        {
            AStruct Inst = new AStruct();
            Inst.a = 1;
            Inst.b = 2;
            Inst.c = 4;
            Inst.d = 8;
            if ((Inst.a == 1) && (Inst.b == 2) && (Inst.c == 4) && (Inst.d == 8))
            {
                Log.WriteSuccess("Test_Instance_Struct okay.");
            }
            else
            {
                Log.WriteError("Test_Instance_Struct not okay.");
            }
        }

        /// <summary>
        /// Tests: Local variable declaration and pointer dereferencing, 
        /// Inputs: 0xDEADBEEF, 
        /// Result: 0xDEADBEEF
        /// </summary>
        [NoGC]
        public static unsafe void Test_Locals_And_Pointers()
        {
            uint testVal = 0xDEADBEEF;
            uint* testValPtr = &testVal;
            if ((testVal == 0xDEADBEEF) && (*testValPtr == 0xDEADBEEF))
            {
                Log.WriteSuccess("Test_Locals_And_Pointers okay.");
            }
            else
            {
                Log.WriteError("Test_Locals_And_Pointers not okay.");
            }
        }

        /// <summary>
        /// Tests: Modulus (remainder) operation using unsigned 32-bit integers, 
        /// Inputs: 9, 3, 
        /// Result: 0
        /// </summary>
        [NoGC]
        public static void Test_ModUInt32_9_3()
        {
            UInt32 a = 9;
            UInt32 b = 3;
            a = a % b;
            if(a == 0)
            {
                Log.WriteSuccess("Test_ModUInt32_9_3 okay.");
            }
            else
            {
                Log.WriteError("Test_ModUInt32_9_3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Modulus (remainder) operation using unsigned 32-bit integers, 
        /// Inputs: 10, 3, 
        /// Result: 1
        /// </summary>
        [NoGC]
        public static void Test_ModUInt32_10_3()
        {
            UInt32 a = 10;
            UInt32 b = 3;
            a = a % b;
            if (a == 1)
            {
                Log.WriteSuccess("Test_ModUInt32_10_3 okay.");
            }
            else
            {
                Log.WriteError("Test_ModUInt32_10_3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Modulus (remainder) operation using signed 32-bit integers, 
        /// Inputs: -9, 3, 
        /// Result: 0
        /// </summary>
        [NoGC]
        public static void Test_ModInt32_Neg9_3()
        {
            Int32 a = -9;
            Int32 b = 3;
            a = a % b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_ModInt32_Neg9_3 okay.");
            }
            else
            {
                Log.WriteError("Test_ModInt32_Neg9_3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Modulus (remainder) operation using signed 32-bit integers, 
        /// Inputs: 9, -3, 
        /// Result: 0
        /// </summary>
        [NoGC]
        public static void Test_ModInt32_9_Neg3()
        {
            Int32 a = 9;
            Int32 b = -3;
            a = a % b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_ModInt32_9_Neg3 okay.");
            }
            else
            {
                Log.WriteError("Test_ModInt32_9_Neg3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Modulus (remainder) operation using signed 32-bit integers, 
        /// Inputs: -9, -3, 
        /// Result: 0
        /// </summary>
        [NoGC]
        public static void Test_ModInt32_Neg9_Neg3()
        {
            Int32 a = -9;
            Int32 b = -3;
            a = a % b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_ModInt32_Neg9_Neg3 okay.");
            }
            else
            {
                Log.WriteError("Test_ModInt32_Neg9_Neg3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Modulus (remainder) operation using signed 32-bit integers, 
        /// Inputs: 9, 3, 
        /// Result: 0
        /// </summary>
        [NoGC]
        public static void Test_ModInt32_9_3()
        {
            Int32 a = 9;
            Int32 b = 3;
            a = a % b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_ModInt32_9_3 okay.");
            }
            else
            {
                Log.WriteError("Test_ModInt32_9_3 not okay.");
            }
        }
    }
}
