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
            Log.WriteLine(" ");
            Log.WriteLine("---Addition:");
            Log.WriteLine("  Unsigned");
            Test_Add_UInt32_Zero_UInt32_Zero();
            Log.WriteLine("  Signed");

            Log.WriteLine(" ");

            Log.WriteLine("---Struct:");
            Test_Sizeof_Struct();
            Test_Instance_Struct();
            Log.WriteLine(" ");

            Log.WriteLine("---Variables and pointers:");
            Test_Locals_And_Pointers();
            Log.WriteLine(" ");

            Log.WriteLine("---Modulus:");
            Log.WriteLine("  Unsigned");
            Test_Mod_UInt32_9_UInt32_3();
            Test_Mod_UInt32_10_UInt32_3();
            Log.WriteLine("  Signed");
            Test_Mod_Int32_9_Int32_3();
            Test_Mod_Int32_9_Int32_Neg3();
            Test_Mod_Int32_10_Int32_3();
            Test_Mod_Int32_10_Int32_Neg3();
            Test_Mod_Int32_Neg9_Int32_3();
            Test_Mod_Int32_Neg9_Int32_Neg3();
            Test_Mod_Int32_Neg10_Int32_3();
            Test_Mod_Int32_Neg10_Int32_Neg3();            
            Log.WriteLine(" ");

            Log.WriteLine("Division:");
            Log.WriteLine("  Unsigned");
            Test_Div_UInt32_9_UInt32_3();
            Test_Div_UInt32_10_UInt32_3();
            Log.WriteLine("  Signed");
            Test_Div_Int32_9_Int32_3();
            Test_Div_Int32_9_Int32_Neg3();
            Test_Div_Int32_10_Int32_3();
            Test_Div_Int32_10_Int32_Neg3();
            Test_Div_Int32_Neg9_Int32_3();
            Test_Div_Int32_Neg9_Int32_Neg3();
            Test_Div_Int32_Neg10_Int32_3();
            Test_Div_Int32_Neg10_Int32_Neg3();
            Log.WriteLine(" ");

            Log.WriteLine("Subtraction:");
            Log.WriteLine(" 32-32");
            Log.WriteLine("  Unsigned");
            Test_Sub_UInt32_9_UInt32_4();
            Log.WriteLine("  Signed");
            Test_Sub_Int32_9_Int32_4();
            Test_Sub_Int32_9_Int32_Neg4();
            Test_Sub_Int32_Neg9_Int32_4();
            Test_Sub_Int32_Neg9_Int32_Neg4();
            Log.WriteLine(" 64-32");
            Log.WriteLine("  Unsigned");
            Test_Sub_UInt64_Large_UInt32_4();
            Log.WriteLine("  Signed");
            Test_Sub_Int64_LargePos_Int32_4();
            Test_Sub_Int64_Zero_Int32_4();
            Test_Sub_Int64_Zero_Int32_LargePos();
            Test_Sub_Int64_Zero_Int32_LargeNeg();
            Log.WriteLine(" 64-64");
            Log.WriteLine("  Unsigned");
            Test_Sub_UInt64_Large_UInt64_Large();
            Log.WriteLine("  Signed");
            Test_Sub_Int64_LargePos_Int64_4();
            Test_Sub_Int64_LargePos_Int64_LargePos();
            Test_Sub_Int64_LargePos_Int64_LargeNeg();
            Test_Sub_Int64_LargeNeg_Int64_LargePos();
            Test_Sub_Int64_LargeNeg_Int64_LargeNeg();
            Test_Sub_Int64_Zero_Int64_4();
            Test_Sub_Int64_Zero_Int64_LargePos();
            Test_Sub_Int64_Zero_Int64_LargeNeg();
            Log.WriteLine(" ");
            
            Log.WriteLine("Tests completed.");
        }

        #region Addition

        /// <summary>
        /// Tests: Addition operation using unsigned 32-bit integers, 
        /// Inputs: 0, 0, 
        /// Result: 0
        /// </summary>
        [NoGC]
        public static void Test_Add_UInt32_Zero_UInt32_Zero()
        {
            UInt32 a = 0;
            UInt32 b = 0;
            a = a + b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_Add_UInt32_Zero_UInt32_Zero okay.");
            }
            else
            {
                Log.WriteError("Test_Add_UInt32_Zero_UInt32_Zero not okay.");
            }
        }

        #endregion

        #region Struct

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

        #endregion

        #region Variables and pointers

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

        #endregion

        #region Modulus

        /// <summary>
        /// Tests: Modulus (remainder) operation using unsigned 32-bit integers, 
        /// Inputs: 9, 3, 
        /// Result: 0
        /// </summary>
        [NoGC]
        public static void Test_Mod_UInt32_9_UInt32_3()
        {
            UInt32 a = 9;
            UInt32 b = 3;
            a = a % b;
            if(a == 0)
            {
                Log.WriteSuccess("Test_Mod_UInt32_9_UInt32_3 okay.");
            }
            else
            {
                Log.WriteError("Test_Mod_UInt32_9_UInt32_3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Modulus (remainder) operation using unsigned 32-bit integers, 
        /// Inputs: 10, 3, 
        /// Result: 1
        /// </summary>
        [NoGC]
        public static void Test_Mod_UInt32_10_UInt32_3()
        {
            UInt32 a = 10;
            UInt32 b = 3;
            a = a % b;
            if (a == 1)
            {
                Log.WriteSuccess("Test_Mod_UInt32_10_UInt32_3 okay.");
            }
            else
            {
                Log.WriteError("Test_Mod_UInt32_10_UInt32_3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Modulus (remainder) operation using signed 32-bit integers, 
        /// Inputs: -9, 3, 
        /// Result: 0
        /// </summary>
        [NoGC]
        public static void Test_Mod_Int32_Neg9_Int32_3()
        {
            Int32 a = -9;
            Int32 b = 3;
            a = a % b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_Mod_Int32_Neg9_Int32_3 okay.");
            }
            else
            {
                Log.WriteError("Test_Mod_Int32_Neg9_Int32_3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Modulus (remainder) operation using signed 32-bit integers, 
        /// Inputs: 9, -3, 
        /// Result: 0
        /// </summary>
        [NoGC]
        public static void Test_Mod_Int32_9_Int32_Neg3()
        {
            Int32 a = 9;
            Int32 b = -3;
            a = a % b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_Mod_Int32_9_Int32_Neg3 okay.");
            }
            else
            {
                Log.WriteError("Test_Mod_Int32_9_Int32_Neg3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Modulus (remainder) operation using signed 32-bit integers, 
        /// Inputs: -9, -3, 
        /// Result: 0
        /// </summary>
        [NoGC]
        public static void Test_Mod_Int32_Neg9_Int32_Neg3()
        {
            Int32 a = -9;
            Int32 b = -3;
            a = a % b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_Mod_Int32_Neg9_Int32_Neg3 okay.");
            }
            else
            {
                Log.WriteError("Test_Mod_Int32_Neg9_Int32_Neg3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Modulus (remainder) operation using signed 32-bit integers, 
        /// Inputs: 9, 3, 
        /// Result: 0
        /// </summary>
        [NoGC]
        public static void Test_Mod_Int32_9_Int32_3()
        {
            Int32 a = 9;
            Int32 b = 3;
            a = a % b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_Mod_Int32_9_Int32_3 okay.");
            }
            else
            {
                Log.WriteError("Test_Mod_Int32_9_Int32_3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Modulus (remainder) operation using signed 32-bit integers, 
        /// Inputs: -10, 3, 
        /// Result: -1
        /// </summary>
        [NoGC]
        public static void Test_Mod_Int32_Neg10_Int32_3()
        {
            Int32 a = -10;
            Int32 b = 3;
            a = a % b;
            if (a == -1)
            {
                Log.WriteSuccess("Test_Mod_Int32_Neg10_Int32_3 okay.");
            }
            else
            {
                Log.WriteError("Test_Mod_Int32_Neg10_Int32_3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Modulus (remainder) operation using signed 32-bit integers, 
        /// Inputs: 10, -3, 
        /// Result: 1
        /// </summary>
        [NoGC]
        public static void Test_Mod_Int32_10_Int32_Neg3()
        {
            Int32 a = 10;
            Int32 b = -3;
            a = a % b;
            if (a == 1)
            {
                Log.WriteSuccess("Test_Mod_Int32_10_Int32_Neg3 okay.");
            }
            else
            {
                Log.WriteError("Test_Mod_Int32_10_Int32_Neg3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Modulus (remainder) operation using signed 32-bit integers, 
        /// Inputs: -10, -3, 
        /// Result: -1
        /// </summary>
        [NoGC]
        public static void Test_Mod_Int32_Neg10_Int32_Neg3()
        {
            Int32 a = -10;
            Int32 b = -3;
            a = a % b;
            if (a == -1)
            {
                Log.WriteSuccess("Test_Mod_Int32_Neg10_Int32_Neg3 okay.");
            }
            else
            {
                Log.WriteError("Test_Mod_Int32_Neg10_Int32_Neg3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Modulus (remainder) operation using signed 32-bit integers, 
        /// Inputs: 10, 3, 
        /// Result: 1
        /// </summary>
        [NoGC]
        public static void Test_Mod_Int32_10_Int32_3()
        {
            Int32 a = 10;
            Int32 b = 3;
            a = a % b;
            if (a == 1)
            {
                Log.WriteSuccess("Test_Mod_Int32_10_Int32_3 okay.");
            }
            else
            {
                Log.WriteError("Test_Mod_Int32_10_Int32_3 not okay.");
            }
        }

        #endregion

        #region Division

        /// <summary>
        /// Tests: Division operation using unsigned 32-bit integers, 
        /// Inputs: 9, 3, 
        /// Result: 3
        /// </summary>
        [NoGC]
        public static void Test_Div_UInt32_9_UInt32_3()
        {
            UInt32 a = 9;
            UInt32 b = 3;
            a = a / b;
            if(a == 3)
            {
                Log.WriteSuccess("Test_Div_UInt32_9_UInt32_3 okay.");
            }
            else
            {
                Log.WriteError("Test_Div_UInt32_9_UInt32_3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Division operation using unsigned 32-bit integers, 
        /// Inputs: 10, 3, 
        /// Result: 3
        /// </summary>
        [NoGC]
        public static void Test_Div_UInt32_10_UInt32_3()
        {
            UInt32 a = 10;
            UInt32 b = 3;
            a = a / b;
            if (a == 3)
            {
                Log.WriteSuccess("Test_Div_UInt32_10_UInt32_3 okay.");
            }
            else
            {
                Log.WriteError("Test_Div_UInt32_10_UInt32_3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Division operation using signed 32-bit integers, 
        /// Inputs: -9, 3, 
        /// Result: -3
        /// </summary>
        [NoGC]
        public static void Test_Div_Int32_Neg9_Int32_3()
        {
            Int32 a = -9;
            Int32 b = 3;
            a = a / b;
            if (a == -3)
            {
                Log.WriteSuccess("Test_Div_Int32_Neg9_Int32_3 okay.");
            }
            else
            {
                Log.WriteError("Test_Div_Int32_Neg9_Int32_3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Division operation using signed 32-bit integers, 
        /// Inputs: 9, -3, 
        /// Result: -3
        /// </summary>
        [NoGC]
        public static void Test_Div_Int32_9_Int32_Neg3()
        {
            Int32 a = 9;
            Int32 b = -3;
            a = a / b;
            if (a == -3)
            {
                Log.WriteSuccess("Test_Div_Int32_9_Int32_Neg3 okay.");
            }
            else
            {
                Log.WriteError("Test_Div_Int32_9_Int32_Neg3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Division operation using signed 32-bit integers, 
        /// Inputs: -9, -3, 
        /// Result: 3
        /// </summary>
        [NoGC]
        public static void Test_Div_Int32_Neg9_Int32_Neg3()
        {
            Int32 a = -9;
            Int32 b = -3;
            a = a / b;
            if (a == 3)
            {
                Log.WriteSuccess("Test_Div_Int32_Neg9_Int32_Neg3 okay.");
            }
            else
            {
                Log.WriteError("Test_Div_Int32_Neg9_Int32_Neg3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Division operation using signed 32-bit integers, 
        /// Inputs: 9, 3, 
        /// Result: 3
        /// </summary>
        [NoGC]
        public static void Test_Div_Int32_9_Int32_3()
        {
            Int32 a = 9;
            Int32 b = 3;
            a = a / b;
            if (a == 3)
            {
                Log.WriteSuccess("Test_Div_Int32_9_Int32_3 okay.");
            }
            else
            {
                Log.WriteError("Test_Div_Int32_9_Int32_3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Division operation using signed 32-bit integers, 
        /// Inputs: -10, 3, 
        /// Result: -3
        /// </summary>
        [NoGC]
        public static void Test_Div_Int32_Neg10_Int32_3()
        {
            Int32 a = -10;
            Int32 b = 3;
            a = a / b;
            if (a == -3)
            {
                Log.WriteSuccess("Test_Div_Int32_Neg10_Int32_3 okay.");
            }
            else
            {
                Log.WriteError("Test_Div_Int32_Neg10_Int32_3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Division operation using signed 32-bit integers, 
        /// Inputs: 10, -3, 
        /// Result: -3
        /// </summary>
        [NoGC]
        public static void Test_Div_Int32_10_Int32_Neg3()
        {
            Int32 a = 10;
            Int32 b = -3;
            a = a / b;
            if (a == -3)
            {
                Log.WriteSuccess("Test_Div_Int32_10_Int32_Neg3 okay.");
            }
            else
            {
                Log.WriteError("Test_Div_Int32_10_Int32_Neg3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Division operation using signed 32-bit integers, 
        /// Inputs: -10, -3, 
        /// Result: 3
        /// </summary>
        [NoGC]
        public static void Test_Div_Int32_Neg10_Int32_Neg3()
        {
            Int32 a = -10;
            Int32 b = -3;
            a = a / b;
            if (a == 3)
            {
                Log.WriteSuccess("Test_Div_Int32_Neg10_Int32_Neg3 okay.");
            }
            else
            {
                Log.WriteError("Test_Div_Int32_Neg10_Int32_Neg3 not okay.");
            }
        }

        /// <summary>
        /// Tests: Division operation using signed 32-bit integers, 
        /// Inputs: 10, 3, 
        /// Result: 3
        /// </summary>
        [NoGC]
        public static void Test_Div_Int32_10_Int32_3()
        {
            Int32 a = 10;
            Int32 b = 3;
            a = a / b;
            if (a == 3)
            {
                Log.WriteSuccess("Test_Div_Int32_10_Int32_3 okay.");
            }
            else
            {
                Log.WriteError("Test_Div_Int32_10_Int32_3 not okay.");
            }
        }

        #endregion

        #region Subtraction

        /// <summary>
        /// Tests: Subtraction operation using unsigned 32-bit integers, 
        /// Inputs: 9, 4, 
        /// Result: 5
        /// </summary>
        [NoGC]
        public static void Test_Sub_UInt32_9_UInt32_4()
        {
            UInt32 a = 9;
            UInt32 b = 4;
            a = a - b;
            if (a == 5)
            {
                Log.WriteSuccess("Test_Sub_UInt32_9_UInt32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_UInt32_9_UInt32_4 not okay.");
            }
        }

        /// <summary>
        /// Tests: Subtraction operation using signed 32-bit integers, 
        /// Inputs: -9, 4, 
        /// Result: -13
        /// </summary>
        [NoGC]
        public static void Test_Sub_Int32_Neg9_Int32_4()
        {
            Int32 a = -9;
            Int32 b = 4;
            a = a - b;
            if (a == -13)
            {
                Log.WriteSuccess("Test_Sub_Int32_Neg9_Int32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int32_Neg9_Int32_4 not okay.");
            }
        }

        /// <summary>
        /// Tests: Subtraction operation using signed 32-bit integers, 
        /// Inputs: -9, -4, 
        /// Result: -5
        /// </summary>
        [NoGC]
        public static void Test_Sub_Int32_Neg9_Int32_Neg4()
        {
            Int32 a = -9;
            Int32 b = -4;
            a = a - b;
            if (a == -5)
            {
                Log.WriteSuccess("Test_Sub_Int32_Neg9_Int32_Neg4 okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int32_Neg9_Int32_Neg4 not okay.");
            }
        }

        /// <summary>
        /// Tests: Subtraction operation using signed 32-bit integers, 
        /// Inputs: 9, -4, 
        /// Result: 13
        /// </summary>
        [NoGC]
        public static void Test_Sub_Int32_9_Int32_Neg4()
        {
            Int32 a = 9;
            Int32 b = -4;
            a = a - b;
            if (a == 13)
            {
                Log.WriteSuccess("Test_Sub_Int32_9_Int32_Neg4 okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int32_9_Int32_Neg4 not okay.");
            }
        }

        /// <summary>
        /// Tests: Subtraction operation using signed 32-bit integers, 
        /// Inputs: 9, 4, 
        /// Result: 5
        /// </summary>
        [NoGC]
        public static void Test_Sub_Int32_9_Int32_4()
        {
            Int32 a = 9;
            Int32 b = 4;
            a = a - b;
            if (a == 5)
            {
                Log.WriteSuccess("Test_Sub_Int32_9_Int32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int32_9_Int32_4 not okay.");
            }
        } 

        /// <summary>
        /// Tests: Subtraction operation using unsigned 64- and 32-bit integers, 
        /// Inputs: Large, 4, 
        /// Result: Large
        /// </summary>
        [NoGC]
        public static void Test_Sub_UInt64_Large_UInt32_4()
        {
            UInt64 a = 1080863910568919030;
            UInt32 b = 4;
            a = a - b;
            if (a == 1080863910568919026)
            {
                Log.WriteSuccess("Test_Sub_UInt64_Large_UInt32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_UInt64_Large_UInt32_4 not okay.");
            }
        }

        /// <summary>
        /// Tests: Subtraction operation using signed 64- and 32-bit integers, 
        /// Inputs: Large +ve, 4, 
        /// Result: Large +ve
        /// </summary>
        /// <remarks>
        /// <para> 
        /// Here a 32-bit signed integer is subtracted from a 64-bit signed integer producing a 64-bit signed value. 
        /// A large value is used for the first operand to ensure that the high 32-bits are non-zero, result is large +ve. 
        /// While testing subtraction using 64-bit integers, it is important to handle the "borrow-bit" correctly. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Sub_Int64_LargePos_Int32_4()
        {
            Int64 a = 1080863910568919040;
            Int32 b = 4;
            a = a - b;
            if (a == 1080863910568919036)
            {
                Log.WriteSuccess("Test_Sub_Int64_LargePos_Int32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int64_LargePos_Int32_4 not okay.");
            }
        }

        /// <summary>
        /// Tests: Subtraction operation using signed 64- and 32-bit integers, 
        /// Inputs: 0, 4, 
        /// Result: -4
        /// </summary>
        /// <remarks>
        /// <para> 
        /// Here a 32-bit signed integer is subtracted from a 64-bit signed integer producing a 64-bit signed value. 
        /// Zero is used for the first operand to ensure that the 64-bit negative result is produced correctly. 
        /// While testing subtraction using 64-bit integers, it is important to handle the "borrow-bit" correctly. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Sub_Int64_Zero_Int32_4()
        {
            Int64 a = 0;
            Int32 b = 4;
            a = a - b;
            if (a == -4)
            {
                Log.WriteSuccess("Test_Sub_Int64_Zero_Int32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int64_Zero_Int32_4 not okay.");
            }
        }
        
        /// <summary>
        /// Tests: Subtraction operation using signed 64- and 32-bit integers, 
        /// Inputs: 0, Large +ve, 
        /// Result: Large -ve
        /// </summary>
        /// <remarks>
        /// <para> 
        /// Here a 32-bit signed integer is subtracted from a 64-bit signed integer producing a 64-bit signed value. 
        /// Zero is used for the first operand and the 64-bit negative result must be a large -ve. 
        /// While testing subtraction using 64-bit integers, it is important to handle the "borrow-bit" correctly. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Sub_Int64_Zero_Int32_LargePos()
        {
            Int64 a = 0;
            Int32 b = 429496729;
            a = a - b;
            if (a == -429496729)
            {
                Log.WriteSuccess("Test_Sub_Int64_Zero_Int32_LargePos okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int64_Zero_Int32_LargePos not okay.");
            }
        }

        /// <summary>
        /// Tests: Subtraction operation using signed 64- and 32-bit integers, 
        /// Inputs: 0, Large -ve, 
        /// Result: Large +ve
        /// </summary>
        /// <remarks>
        /// <para> 
        /// Here a 32-bit signed integer is subtracted from a 64-bit signed integer producing a 64-bit signed value. 
        /// Zero is used for the first operand and the 64-bit result must be a large +ve. 
        /// While testing subtraction using 64-bit integers, it is important to handle the "borrow-bit" correctly. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Sub_Int64_Zero_Int32_LargeNeg()
        {
            Int64 a = 0;
            Int32 b = -429496729;
            a = a - b;
            if (a == 429496729)
            {
                Log.WriteSuccess("Test_Sub_Int64_Zero_Int32_LargeNeg okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int64_Zero_Int32_LargeNeg not okay.");
            }
        }
        
        /// <summary>
        /// Tests: Subtraction operation using signed 64-bit integers, 
        /// Inputs: Large +ve, 4, 
        /// Result: Large +ve
        /// </summary>
        /// <remarks>
        /// <para> 
        /// Here a 64-bit signed integer is subtracted from a 64-bit signed integer producing a 64-bit signed value. 
        /// A large value is used for the first operand to ensure that the 64-bit result is produced correctly, result is large +ve. 
        /// While testing subtraction using 64-bit integers, it is important to handle the "borrow-bit" correctly. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Sub_Int64_LargePos_Int64_4()
        {
            Int64 a = 1080863910568919040;
            Int64 b = 4;
            a = a - b;
            if (a == 1080863910568919036)
            {
                Log.WriteSuccess("Test_Sub_Int64_LargePos_Int64_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int64_LargePos_Int64_4 not okay.");
            }
        }

        /// <summary>
        /// Tests: Subtraction operation using signed 64-bit integers, 
        /// Inputs: 0, 4, 
        /// Result: -4
        /// </summary>
        /// <remarks>
        /// <para> 
        /// Here a 64-bit signed integer is subtracted from a 64-bit signed integer producing a 64-bit signed value. 
        /// Zero is used for the first operand to ensure that the 64-bit negative result is produced correctly. 
        /// While testing subtraction using 64-bit integers, it is important to handle the "borrow-bit" correctly. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Sub_Int64_Zero_Int64_4()
        {
            Int64 a = 0;
            Int64 b = 4;
            a = a - b;
            if (a == -4)
            {
                Log.WriteSuccess("Test_Sub_Int64_Zero_Int64_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int64_Zero_Int64_4 not okay.");
            }
        }

        /// <summary>
        /// Tests: Subtraction operation using unsigned 64-bit integers, 
        /// Inputs: Large +ve, Large +ve, 
        /// Result: +ve
        /// </summary>
        /// <remarks>
        /// <para> 
        /// Here a 64-bit unsigned integer is subtracted from a 64-bit unsigned integer producing a 64-bit unsigned value. 
        /// Both operands are large values but op1 > op2, therefore result must be +ve. 
        /// While testing subtraction using 64-bit integers, it is important to handle the "borrow-bit" correctly. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Sub_UInt64_Large_UInt64_Large()
        {
            UInt64 a = 1080863910568919040;
            UInt64 b = 844424930131968;
            a = a - b;
            if (a == 1080019485638787072)
            {
                Log.WriteSuccess("Test_Sub_UInt64_Large_UInt64_Large okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_UInt64_Large_UInt64_Large not okay.");
            }
        }

        /// <summary>
        /// Tests: Subtraction operation using signed 64-bit integers, 
        /// Inputs: Large -ve, Large +ve, 
        /// Result: Large -ve
        /// </summary>
        /// <remarks>
        /// <para> 
        /// Here a 64-bit signed integer is subtracted from a 64-bit signed integer producing a 64-bit signed value. 
        /// Both operands are large values but op1 is -ve while op2 is +ve, therefore result must be a large -ve. 
        /// While testing subtraction using 64-bit integers, it is important to handle the "borrow-bit" correctly. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Sub_Int64_LargeNeg_Int64_LargePos()
        {
            Int64 a = -1080863910568919040;
            Int64 b = 844424930131968;
            a = a - b;
            if (a == -1081708335499051008)
            {
                Log.WriteSuccess("Test_Sub_Int64_LargeNeg_Int64_LargePos okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int64_LargeNeg_Int64_LargePos not okay.");
            }
        }

        /// <summary>
        /// Tests: Subtraction operation using signed 64-bit integers, 
        /// Inputs: 0, Large +ve, 
        /// Result: Large -ve
        /// </summary>
        /// <remarks>
        /// <para> 
        /// Here a 64-bit signed integer is subtracted from a 64-bit signed integer producing a 64-bit signed value. 
        /// Zero is used for the first operand and the 64-bit negative result must be large. 
        /// While testing subtraction using 64-bit integers, it is important to handle the "borrow-bit" correctly. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Sub_Int64_Zero_Int64_LargePos()
        {
            Int64 a = 0;
            Int64 b = 844424930131968;
            a = a - b;
            if (a == -844424930131968)
            {
                Log.WriteSuccess("Test_Sub_Int64_Zero_Int64_LargePos okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int64_Zero_Int64_LargePos not okay.");
            }
        }

        /// <summary>
        /// Tests: Subtraction operation using signed 64-bit integers, 
        /// Inputs: 0, Large -ve, 
        /// Result: Large +ve
        /// </summary>
        /// <remarks>
        /// <para> 
        /// Here a 64-bit signed integer is subtracted from a 64-bit signed integer producing a 64-bit signed value. 
        /// Zero is used for the first operand and the 64-bit result must be a large +ve. 
        /// While testing subtraction using 64-bit integers, it is important to handle the "borrow-bit" correctly. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Sub_Int64_Zero_Int64_LargeNeg()
        {
            Int64 a = 0;
            Int64 b = -844424930131968;
            a = a - b;
            if (a == 844424930131968)
            {
                Log.WriteSuccess("Test_Sub_Int64_Zero_Int64_LargeNeg okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int64_Zero_Int64_LargeNeg not okay.");
            }
        }

        /// <summary>
        /// Tests: Subtraction operation using signed 64-bit integers, 
        /// Inputs: Large -ve, Large -ve, 
        /// Result: Large -ve
        /// </summary>
        /// <remarks>
        /// <para> 
        /// Here a 64-bit signed integer is subtracted from a 64-bit signed integer producing a 64-bit signed value. 
        /// A large -ve value is used for both operands, result must be a large -ve. 
        /// While testing subtraction using 64-bit integers, it is important to handle the "borrow-bit" correctly. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Sub_Int64_LargeNeg_Int64_LargeNeg()
        {
            Int64 a = -1080863910568919040;
            Int64 b = -844424930131968;
            a = a - b;
            if (a == -1080019485638787072)
            {
                Log.WriteSuccess("Test_Sub_Int64_LargeNeg_Int64_LargeNeg okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int64_LargeNeg_Int64_LargeNeg not okay.");
            }
        }

        /// <summary>
        /// Tests: Subtraction operation using signed 64-bit integers, 
        /// Inputs: Large +ve, Large -ve, 
        /// Result: Large +ve
        /// </summary>
        /// <remarks>
        /// <para> 
        /// Here a 64-bit signed integer is subtracted from a 64-bit signed integer producing a 64-bit signed value. 
        /// A large +ve value is used for op1 and a large -ve for op2, here the result is a large +ve. 
        /// While testing subtraction using 64-bit integers, it is important to handle the "borrow-bit" correctly. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Sub_Int64_LargePos_Int64_LargeNeg()
        {
            Int64 a = 1080863910568919040;
            Int64 b = -844424930131968;
            a = a - b;
            if (a == 1081708335499051008)
            {
                Log.WriteSuccess("Test_Sub_Int64_LargePos_Int64_LargeNeg okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int64_LargePos_Int64_LargeNeg not okay.");
            }
        }

        /// <summary>
        /// Tests: Subtraction operation using signed 64-bit integers, 
        /// Inputs: Large +ve, Large +ve, 
        /// Result: Large +ve
        /// </summary>
        /// <remarks>
        /// <para> 
        /// Here a 64-bit signed integer is subtracted from a 64-bit signed integer producing a 64-bit signed value. 
        /// A large +ve value is used for both operands, here the result is a large +ve. 
        /// While testing subtraction using 64-bit integers, it is important to handle the "borrow-bit" correctly. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Sub_Int64_LargePos_Int64_LargePos()
        {
            Int64 a = 1080863910568919040;
            Int64 b = 844424930131968;
            a = a - b;
            if (a == 1080019485638787072)
            {
                Log.WriteSuccess("Test_Sub_Int64_LargePos_Int64_LargePos okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int64_LargePos_Int64_LargePos not okay.");
            }
        }

        #endregion
    }
}
