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
    /// <para>
    /// For MIPS UART_Num is set to 0 for UBoot serial booting and is set to 4 for UBS OTG booting.
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
            #region Addition calls

            Log.WriteLine(" ");
            Log.WriteLine("---Addition:");
            Log.WriteLine("  Unsigned");
            Test_Add_UInt32_Zero_UInt32_Zero();
            Log.WriteLine("  Signed");

            Log.WriteLine(" ");

            #endregion

            #region Struct calls

            Log.WriteLine("---Struct:");
            Test_Sizeof_Struct();
            Test_Instance_Struct();
            Log.WriteLine(" ");

            #endregion

            #region Variables and pointers calls

            Log.WriteLine("---Variables and pointers:");
            Test_Locals_And_Pointers();
            Log.WriteLine(" ");

            #endregion

            #region Modulus calls

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

            #endregion

            #region Division calls 

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

            #endregion

            #region Subtraction calls

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
            Test_Sub_UInt64_LargestPos_UInt32_4();
            Log.WriteLine("  Signed");
            Test_Sub_Int64_LargestNeg_Int32_4();
            Test_Sub_Int64_LargestPos_Int32_4();
            Test_Sub_Int64_Zero_Int32_4();
            Test_Sub_Int64_Zero_Int32_LargestPos();
            Test_Sub_Int64_Zero_Int32_LargestNeg();
            Log.WriteLine(" 64-64");
            Log.WriteLine("  Unsigned");
            Test_Sub_UInt64_Large_UInt64_Large();
            Log.WriteLine("  Signed");
            Test_Sub_Int64_LargePos_Int64_4();
            Test_Sub_Int64_LargePos_Int64_LargePos();
            Test_Sub_Int64_LargePos_Int64_LargeNeg();
            Test_Sub_Int64_LargestNeg_Int64_1();
            Test_Sub_Int64_LargeNeg_Int64_LargeNeg();
            Test_Sub_Int64_Zero_Int64_4();
            Test_Sub_Int64_Zero_Int64_LargePos();
            Test_Sub_Int64_Zero_Int64_LargeNeg();
            Log.WriteLine(" ");

            #endregion

            #region Right shift calls

            Log.WriteLine("Right shift:");
            Log.WriteLine(" 32");
            Log.WriteLine("  Unsigned");
            Test_RShift_UInt32_Small_Int32_6();
            Test_RShift_UInt32_Largest_Int32_6();
            Log.WriteLine("  Signed");
            Test_RShift_Int32_SmallPos_Int32_6();
            Test_RShift_Int32_SmallNeg_Int32_6();
            Test_RShift_Int32_LargestPos_Int32_6();
            Test_RShift_Int32_LargestNeg_Int32_6();
            Log.WriteLine(" 64");
            Log.WriteLine("  Unsigned");
            Log.WriteLine("   dist<32");
            Test_RShift_UInt64_Large_Int32_10();
            Test_RShift_UInt64_Largest_Int32_10();
            Log.WriteLine("   dist>=32");
            Test_RShift_UInt64_Largest_Int32_63();
            Log.WriteLine("  Signed");
            Log.WriteLine("   dist<32");
            Test_RShift_Int64_LargeNeg_Int32_6();
            Log.WriteLine("   dist>=32");
            Test_RShift_Int64_LargestPos_Int32_40();
            Test_RShift_Int64_LargestNeg_Int32_40();
            Test_RShift_Int64_LargeNeg_Int32_40();
            Test_RShift_Int64_Neg1_Int32_63();
            Log.WriteLine(" ");

            #endregion

            #region Left shift calls

            Log.WriteLine("Left shift:");
            Log.WriteLine(" 32");
            Log.WriteLine("  Unsigned");
            Test_LShift_UInt32_Small_Int32_6();
            Test_LShift_UInt32_Largest_Int32_6();
            Log.WriteLine("  Signed");
            Test_LShift_Int32_SmallPos_Int32_6();
            Test_LShift_Int32_SmallNeg_Int32_6();
            Test_LShift_Int32_LargestPos_Int32_6();
            Test_LShift_Int32_LargestNeg_Int32_6();
            Log.WriteLine(" 64");
            Log.WriteLine("  Unsigned");
            Log.WriteLine("   dist<32");
            Test_LShift_UInt64_Large_Int32_2();
            Test_LShift_UInt64_Largest_Int32_10();
            Log.WriteLine("   dist>=32");
            Test_LShift_UInt64_Largest_Int32_63();
            Log.WriteLine("  Signed");
            Log.WriteLine("   dist<32");
            Test_LShift_Int64_LargeNeg_Int32_6();
            Log.WriteLine("   dist>=32");
            Test_LShift_Int64_LargestPos_Int32_40();
            Test_LShift_Int64_LargestNeg_Int32_40();
            Test_LShift_Int64_LargeNeg_Int32_40();
            Test_LShift_Int64_Neg1_Int32_63();
            Log.WriteLine(" ");


            
            #endregion

            #region Negation calls

            Log.WriteLine("Negation:");
            Log.WriteLine(" 32");
            Log.WriteLine("  Unsigned");
            Log.WriteLine("UInt32 cannot be negated into Int32, only into Int64 in C#.");
            Test_Neg_UInt32_Small_Int64();
            Test_Neg_UInt32_Largest_Int64();
            Log.WriteLine("  Signed");
            Test_Neg_Int32_SmallPos_Int32();
            Test_Neg_Int32_SmallNeg_Int32();
            Test_Neg_Int32_LargePos_Int64();
            Test_Neg_Int32_LargeNeg_Int64();
            Log.WriteLine(" 64");
            Log.WriteLine("  Unsigned");
            Log.WriteLine("UInt64 cannot be negated in C#.");
            Log.WriteLine("  Signed");
            Test_Neg_Int64_LargePos_Int64();
            Test_Neg_Int64_LargeNeg_Int64();
            Test_Neg_Int64_LargestPos_Int64();
            Test_Neg_Int64_LargestNeg_Int64();
            Log.WriteLine(" ");

            #endregion

            #region Not calls

            Log.WriteLine("Not:");
            Log.WriteLine(" 32");
            Log.WriteLine("  Unsigned");
            //Log.WriteLine("UInt32 cannot be negated into Int32, only into Int64 in C#.");
            //Test_Not_UInt32_Small_Int64();
            //Test_Not_UInt32_Largest_Int64();
            Log.WriteLine("  Signed");
            //Test_Not_Int32_SmallPos_Int32();
            Test_Not_Int32_SmallNeg_Int32();
            //Test_Not_Int32_LargePos_Int64();
            //Test_Not_Int32_LargeNeg_Int64();
            Log.WriteLine(" 64");
            Log.WriteLine("  Unsigned");
            //Log.WriteLine("UInt64 cannot be negated in C#.");
            Log.WriteLine("  Signed");
            //Test_Not_Int64_LargePos_Int64();
            //Test_Not_Int64_LargeNeg_Int64();
            Test_Not_Int64_LargestPos_Int64();
            Test_Not_Int64_LargestNeg_Int64();
            Log.WriteLine(" ");

            #endregion

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
                Log.WriteError("Test_Add_UInt32_Zero_UInt32_Zero NOT okay.");
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
                Log.WriteError("Test_Sizeof_Struct NOT okay.");
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
                Log.WriteError("Test_Instance_Struct NOT okay.");
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
                Log.WriteError("Test_Locals_And_Pointers NOT okay.");
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
                Log.WriteError("Test_Mod_UInt32_9_UInt32_3 NOT okay.");
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
                Log.WriteError("Test_Mod_UInt32_10_UInt32_3 NOT okay.");
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
                Log.WriteError("Test_Mod_Int32_Neg9_Int32_3 NOT okay.");
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
                Log.WriteError("Test_Mod_Int32_9_Int32_Neg3 NOT okay.");
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
                Log.WriteError("Test_Mod_Int32_Neg9_Int32_Neg3 NOT okay.");
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
                Log.WriteError("Test_Mod_Int32_9_Int32_3 NOT okay.");
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
                Log.WriteError("Test_Mod_Int32_Neg10_Int32_3 NOT okay.");
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
                Log.WriteError("Test_Mod_Int32_10_Int32_Neg3 NOT okay.");
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
                Log.WriteError("Test_Mod_Int32_Neg10_Int32_Neg3 NOT okay.");
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
                Log.WriteError("Test_Mod_Int32_10_Int32_3 NOT okay.");
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
                Log.WriteError("Test_Div_UInt32_9_UInt32_3 NOT okay.");
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
                Log.WriteError("Test_Div_UInt32_10_UInt32_3 NOT okay.");
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
                Log.WriteError("Test_Div_Int32_Neg9_Int32_3 NOT okay.");
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
                Log.WriteError("Test_Div_Int32_9_Int32_Neg3 NOT okay.");
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
                Log.WriteError("Test_Div_Int32_Neg9_Int32_Neg3 NOT okay.");
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
                Log.WriteError("Test_Div_Int32_9_Int32_3 NOT okay.");
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
                Log.WriteError("Test_Div_Int32_Neg10_Int32_3 NOT okay.");
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
                Log.WriteError("Test_Div_Int32_10_Int32_Neg3 NOT okay.");
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
                Log.WriteError("Test_Div_Int32_Neg10_Int32_Neg3 NOT okay.");
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
                Log.WriteError("Test_Div_Int32_10_Int32_3 NOT okay.");
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
                Log.WriteError("Test_Sub_UInt32_9_UInt32_4 NOT okay.");
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
                Log.WriteError("Test_Sub_Int32_Neg9_Int32_4 NOT okay.");
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
                Log.WriteError("Test_Sub_Int32_Neg9_Int32_Neg4 NOT okay.");
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
                Log.WriteError("Test_Sub_Int32_9_Int32_Neg4 NOT okay.");
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
                Log.WriteError("Test_Sub_Int32_9_Int32_4 NOT okay.");
            }
        } 

        /// <summary>
        /// Tests: Subtraction operation using unsigned 64- and 32-bit integers, 
        /// Inputs: Largest +ve, 4, 
        /// Result: (Largest +ve - 4)
        /// </summary>
        [NoGC]
        public static void Test_Sub_UInt64_LargestPos_UInt32_4()
        {
            UInt64 a = 18446744073709551615;
            UInt32 b = 4;
            a = a - b;
            if (a == 18446744073709551611)
            {
                Log.WriteSuccess("Test_Sub_UInt64_LargestPos_UInt32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_UInt64_LargestPos_UInt32_4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Subtraction operation using signed 64- and 32-bit integers, 
        /// Inputs: Largest +ve, 4, 
        /// Result: (Largest +ve - 4)
        /// </summary>
        /// <remarks>
        /// <para> 
        /// Here a 32-bit signed integer is subtracted from a 64-bit signed integer producing a 64-bit signed value. 
        /// A largest +ve value is used for the first operand, result is (Largest +ve - 4). 
        /// While testing subtraction using 64-bit integers, it is important to handle the "borrow-bit" correctly. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Sub_Int64_LargestPos_Int32_4()
        {
            Int64 a = 9223372036854775807;
            Int32 b = 4;
            a = a - b;
            if (a == 9223372036854775803)
            {
                Log.WriteSuccess("Test_Sub_Int64_LargestPos_Int32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int64_LargestPos_Int32_4 NOT okay.");
            }
        }



        /// <summary>
        /// Tests: Subtraction operation using signed 64- and 32-bit integers, 
        /// Inputs: Largest -ve, 4, 
        /// Result: (Largest +ve - 3)
        /// </summary>
        /// <remarks>
        /// <para> 
        /// Here a 32-bit signed integer is subtracted from a 64-bit signed integer producing a 64-bit signed value. 
        /// The largest -ve value is used for the first operand. Correct result should be (Largest +ve - 3) because of the 
        /// circular nature of signed numbers in two's complement.
        /// While testing subtraction using 64-bit integers, it is important to handle the "borrow-bit" correctly. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Sub_Int64_LargestNeg_Int32_4()
        {
            Int64 a = -9223372036854775808;
            Int32 b = 4;
            a = a - b;
            if (a == 9223372036854775804)
            {
                Log.WriteSuccess("Test_Sub_Int64_LargestNeg_Int32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int64_LargestNeg_Int32_4 NOT okay.");
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
                Log.WriteError("Test_Sub_Int64_Zero_Int32_4 NOT okay.");
            }
        }
        
        /// <summary>
        /// Tests: Subtraction operation using signed 64- and 32-bit integers, 
        /// Inputs: 0, Largest +ve, 
        /// Result: Large -ve
        /// </summary>
        /// <remarks>
        /// <para> 
        /// Here a 32-bit signed integer is subtracted from a 64-bit signed integer producing a 64-bit signed value. 
        /// Zero is used for the first operand and the 64-bit result must be equal to -(op2). 
        /// While testing subtraction using 64-bit integers, it is important to handle the "borrow-bit" correctly. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Sub_Int64_Zero_Int32_LargestPos()
        {
            Int64 a = 0;
            Int32 b = 2147483647;
            a = a - b;
            if (a == -2147483647)
            {
                Log.WriteSuccess("Test_Sub_Int64_Zero_Int32_LargestPos okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int64_Zero_Int32_LargestPos NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Subtraction operation using signed 64- and 32-bit integers, 
        /// Inputs: 0, (Largest -ve - 1), 
        /// Result: Largest +ve
        /// </summary>
        /// <remarks>
        /// <para> 
        /// Here a 32-bit signed integer is subtracted from a 64-bit signed integer producing a 64-bit signed value. 
        /// Zero is used for the first operand and the 64-bit result must be equal to -(op2). 
        /// While testing subtraction using 64-bit integers, it is important to handle the "borrow-bit" correctly. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Sub_Int64_Zero_Int32_LargestNeg()
        {
            Int64 a = 0;
            Int32 b = -2147483647;
            a = a - b;
            if (a == 2147483647)
            {
                Log.WriteSuccess("Test_Sub_Int64_Zero_Int32_LargestNeg okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int64_Zero_Int32_LargestNeg NOT okay.");
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
                Log.WriteError("Test_Sub_Int64_LargePos_Int64_4 NOT okay.");
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
                Log.WriteError("Test_Sub_Int64_Zero_Int64_4 NOT okay.");
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
                Log.WriteError("Test_Sub_UInt64_Large_UInt64_Large NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Subtraction operation using signed 64-bit integers, 
        /// Inputs: Largest -ve, 1, 
        /// Result: Largest +ve
        /// </summary>
        /// <remarks>
        /// <para> 
        /// Here a 64-bit signed integer is subtracted from a 64-bit signed integer producing a 64-bit signed value. 
        /// The first operand is the largest -ve value, while op2 = 1. The result should be the largest +ve.
        /// While testing subtraction using 64-bit integers, it is important to handle the "borrow-bit" correctly. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Sub_Int64_LargestNeg_Int64_1()
        {
            Int64 a = -9223372036854775808;
            Int64 b = 1;
            a = a - b;
            if (a == 9223372036854775807)
            {
                Log.WriteSuccess("Test_Sub_Int64_LargestNeg_Int64_1 okay.");
            }
            else
            {
                Log.WriteError("Test_Sub_Int64_LargestNeg_Int64_1 NOT okay.");
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
                Log.WriteError("Test_Sub_Int64_Zero_Int64_LargePos NOT okay.");
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
                Log.WriteError("Test_Sub_Int64_Zero_Int64_LargeNeg NOT okay.");
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
                Log.WriteError("Test_Sub_Int64_LargeNeg_Int64_LargeNeg NOT okay.");
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
                Log.WriteError("Test_Sub_Int64_LargePos_Int64_LargeNeg NOT okay.");
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
                Log.WriteError("Test_Sub_Int64_LargePos_Int64_LargePos NOT okay.");
            }
        }

        #endregion

        #region Right shift

        /// <summary>
        /// Tests: Right shift operation shifting an unsigned 64-bit value, 
        /// Inputs: 64-bit, 10, 
        /// Result: 64-bit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_RShift_UInt64_Large_Int32_10()
        {
            UInt64 a = 576460752303423488;
            Int32 b = 10;
            a = a >> b;
            if (a == 562949953421312)
            {
                Log.WriteSuccess("Test_RShift_UInt64_Large_Int32_10 okay.");
            }
            else
            {
                Log.WriteError("Test_RShift_UInt64_Large_Int32_10 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Right shift operation shifting a signed 32-bit value, 
        /// Inputs: 32-bit -ve, 6, 
        /// Result: 32-bit -ve (padded with 1s).
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_RShift_Int32_SmallNeg_Int32_6()
        {
            Int32 a = -28416;
            Int32 b = 6;
            a = a >> b;
            if (a == -444)
            {
                Log.WriteSuccess("Test_RShift_Int32_SmallNeg_Int32_6 okay.");
            }
            else
            {
                Log.WriteError("Test_RShift_Int32_SmallNeg_Int32_6 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Right shift operation shifting an unsigned 32-bit value, 
        /// Inputs: 32-bit, 6, 
        /// Result: 32-bit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_RShift_UInt32_Small_Int32_6()
        {
            UInt32 a = 4352;
            Int32 b = 6;
            a = a >> b;
            if (a == 68)
            {
                Log.WriteSuccess("Test_RShift_UInt32_Small_Int32_6 okay.");
            }
            else
            {
                Log.WriteError("Test_RShift_UInt32_Small_Int32_6 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Right shift operation shifting a signed 64-bit value, 
        /// Inputs: 64-bit -ve, 6, 
        /// Result: 64-bit -ve (stored as 64-bit padded with 1s).
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_RShift_Int64_LargeNeg_Int32_6()
        {
            Int64 a = -9185091440022126524;
            Int32 b = 6;
            a = a >> b;
            if (a == -143517053750345727)
            {
                Log.WriteSuccess("Test_RShift_Int64_LargeNeg_Int32_6 okay.");
            }
            else
            {
                Log.WriteError("Test_RShift_Int64_LargeNeg_Int32_6 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Right shift operation shifting a signed 64-bit value, 
        /// Inputs: 64-bit -ve, 40, 
        /// Result: 32-bit -ve (stored as 64-bit padded with 1s).
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_RShift_Int64_LargeNeg_Int32_40()
        {
            Int64 a = -9187343239835811840;
            Int32 b = 40;
            a = a >> b;
            if (a == -8355840)
            {
                Log.WriteSuccess("Test_RShift_Int64_LargeNeg_Int32_40 okay.");
            }
            else
            {
                Log.WriteError("Test_RShift_Int64_LargeNeg_Int32_40 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Right shift operation shifting a signed 64-bit value, 
        /// Inputs: Largest 64-bit +ve, 40, 
        /// Result: 32-bit +ve (stored as 64-bit padded with 0s).
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_RShift_Int64_LargestPos_Int32_40()
        {
            Int64 a = 9223372036854775807;
            Int32 b = 40;
            a = a >> b;
            if (a == 8388607)
            {
                Log.WriteSuccess("Test_RShift_Int64_LargestPos_Int32_40 okay.");
            }
            else
            {
                Log.WriteError("Test_RShift_Int64_LargestPos_Int32_40 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Right shift operation shifting a signed 64-bit value, 
        /// Inputs: Largest 64-bit -ve, 40, 
        /// Result: 32-bit -ve (stored as 64-bit padded with 1s).
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_RShift_Int64_LargestNeg_Int32_40()
        {
            Int64 a = -9223372036854775808;
            Int32 b = 40;
            a = a >> b;
            if (a == -8388608)
            {
                Log.WriteSuccess("Test_RShift_Int64_LargestNeg_Int32_40 okay.");
            }
            else
            {
                Log.WriteError("Test_RShift_Int64_LargestNeg_Int32_40 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Right shift operation shifting a signed 64-bit value, 
        /// Inputs: -1, 63, 
        /// Result: -1 because of circular nature of two's complement.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_RShift_Int64_Neg1_Int32_63()
        {
            Int64 a = -1;
            Int32 b = 63;
            a = a >> b;
            if (a == -1)
            {
                Log.WriteSuccess("Test_RShift_Int64_Neg1_Int32_63 okay.");
            }
            else
            {
                Log.WriteError("Test_RShift_Int64_Neg1_Int32_63 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Right shift operation shifting an unsigned 64-bit value, 
        /// Inputs: Largest, 63, 
        /// Result: 1.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_RShift_UInt64_Largest_Int32_63()
        {
            UInt64 a = 18446744073709551615;
            Int32 b = 63;
            a = a >> b;
            if (a == 1)
            {
                Log.WriteSuccess("Test_RShift_UInt64_Largest_Int32_63 okay.");
            }
            else
            {
                Log.WriteError("Test_RShift_UInt64_Largest_Int32_63 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Right shift operation shifting an unsigned 32-bit value, 
        /// Inputs: Largest, 6, 
        /// Result: 32-bit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_RShift_UInt32_Largest_Int32_6()
        {
            UInt32 a = 4294967295;
            Int32 b = 6;
            a = a >> b;
            if (a == 67108863)
            {
                Log.WriteSuccess("Test_RShift_UInt32_Largest_Int32_6 okay.");
            }
            else
            {
                Log.WriteError("Test_RShift_UInt32_Largest_Int32_6 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Right shift operation shifting a signed 32-bit value, 
        /// Inputs: Small +ve, 6, 
        /// Result: 32-bit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_RShift_Int32_SmallPos_Int32_6()
        {
            Int32 a = 255;
            Int32 b = 6;
            a = a >> b;
            if (a == 3)
            {
                Log.WriteSuccess("Test_RShift_Int32_SmallPos_Int32_6 okay.");
            }
            else
            {
                Log.WriteError("Test_RShift_Int32_SmallPos_Int32_6 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Right shift operation shifting a signed 32-bit value, 
        /// Inputs: Largest +ve, 6, 
        /// Result: 32-bit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_RShift_Int32_LargestPos_Int32_6()
        {
            Int32 a = 2147483647;
            Int32 b = 6;
            a = a >> b;
            if (a == 33554431)
            {
                Log.WriteSuccess("Test_RShift_Int32_LargestPos_Int32_6 okay.");
            }
            else
            {
                Log.WriteError("Test_RShift_Int32_LargestPos_Int32_6 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Right shift operation shifting a signed 32-bit value, 
        /// Inputs: Largest -ve, 6, 
        /// Result: 32-bit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_RShift_Int32_LargestNeg_Int32_6()
        {
            Int32 a = -2147483648;
            Int32 b = 6;
            a = a >> b;
            if (a == -33554432)
            {
                Log.WriteSuccess("Test_RShift_Int32_LargestNeg_Int32_6 okay.");
            }
            else
            {
                Log.WriteError("Test_RShift_Int32_LargestNeg_Int32_6 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Right shift operation shifting an unsigned 64-bit value, 
        /// Inputs: Largest, 10, 
        /// Result: 64-bit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_RShift_UInt64_Largest_Int32_10()
        {
            UInt64 a = 18446744073709551615;
            Int32 b = 10;
            a = a >> b;
            if (a == 18014398509481983)
            {
                Log.WriteSuccess("Test_RShift_UInt64_Largest_Int32_10 okay.");
            }
            else
            {
                Log.WriteError("Test_RShift_UInt64_Largest_Int32_10 NOT okay.");
            }
        }

        #endregion

        #region Left shift

        /// <summary>
        /// Tests: Left shift operation shifting an unsigned 64-bit value, 
        /// Inputs: 64-bit, 2, 
        /// Result: 64-bit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_LShift_UInt64_Large_Int32_2()
        {
            UInt64 a = 576460752303423488;
            Int32 b = 2;
            a = a << b;
            if (a == 2305843009213693952)
            {
                Log.WriteSuccess("Test_LShift_UInt64_Large_Int32_2 okay.");
            }
            else
            {
                Log.WriteError("Test_LShift_UInt64_Large_Int32_2 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Left shift operation shifting a signed 32-bit value, 
        /// Inputs: 32-bit -ve, 6, 
        /// Result: 32-bit -ve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_LShift_Int32_SmallNeg_Int32_6()
        {
            Int32 a = -28416;
            Int32 b = 6;
            a = a << b;
            if (a == -1818624)
            {
                Log.WriteSuccess("Test_LShift_Int32_SmallNeg_Int32_6 okay.");
            }
            else
            {
                Log.WriteError("Test_LShift_Int32_SmallNeg_Int32_6 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Right shift operation shifting an unsigned 32-bit value, 
        /// Inputs: 32-bit, 6, 
        /// Result: 32-bit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_LShift_UInt32_Small_Int32_6()
        {
            UInt32 a = 4352;
            Int32 b = 6;
            a = a << b;
            if (a == 278528)
            {
                Log.WriteSuccess("Test_LShift_UInt32_Small_Int32_6 okay.");
            }
            else
            {
                Log.WriteError("Test_LShift_UInt32_Small_Int32_6 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Left shift operation shifting a signed 64-bit value, 
        /// Inputs: 64-bit -ve, 6, 
        /// Result: 64-bit +ve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_LShift_Int64_LargeNeg_Int32_6()
        {
            Int64 a = -9185091440022126524;
            Int32 b = 6;
            a = a << b;
            if (a == 2449958197289554176)
            {
                Log.WriteSuccess("Test_LShift_Int64_LargeNeg_Int32_6 okay.");
            }
            else
            {
                Log.WriteError("Test_LShift_Int64_LargeNeg_Int32_6 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Left shift operation shifting a signed 64-bit value, 
        /// Inputs: 64-bit -ve, 40, 
        /// Result: 32-bit +ve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_LShift_Int64_LargeNeg_Int32_40()
        {
            Int64 a = -9187343239835811832;
            Int32 b = 40;
            a = a << b;
            if (a == 8796093022208)
            {
                Log.WriteSuccess("Test_LShift_Int64_LargeNeg_Int32_40 okay.");
            }
            else
            {
                Log.WriteError("Test_LShift_Int64_LargeNeg_Int32_40 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Left shift operation shifting a signed 64-bit value, 
        /// Inputs: Largest 64-bit +ve, 40, 
        /// Result: 64-bit -ve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_LShift_Int64_LargestPos_Int32_40()
        {
            Int64 a = 9223372036854775807;
            Int32 b = 40;
            a = a << b;
            if (a == -1099511627776)
            {
                Log.WriteSuccess("Test_LShift_Int64_LargestPos_Int32_40 okay.");
            }
            else
            {
                Log.WriteError("Test_LShift_Int64_LargestPos_Int32_40 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Left shift operation shifting a signed 64-bit value, 
        /// Inputs: Largest -ve, 40, 
        /// Result: 0.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_LShift_Int64_LargestNeg_Int32_40()
        {
            Int64 a = -9223372036854775808;
            Int32 b = 40;
            a = a << b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_LShift_Int64_LargestNeg_Int32_40 okay.");
            }
            else
            {
                Log.WriteError("Test_LShift_Int64_LargestNeg_Int32_40 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Left shift operation shifting a signed 64-bit value, 
        /// Inputs: -1, 63, 
        /// Result: Largest -ve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_LShift_Int64_Neg1_Int32_63()
        {
            Int64 a = -1;
            Int32 b = 63;
            a = a << b;
            if (a == -9223372036854775808)
            {
                Log.WriteSuccess("Test_LShift_Int64_Neg1_Int32_63 okay.");
            }
            else
            {
                Log.WriteError("Test_LShift_Int64_Neg1_Int32_63 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Left shift operation shifting an unsigned 64-bit value, 
        /// Inputs: Largest, 63, 
        /// Result: 0x8000000000000000 (Highest bit set to 1).
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_LShift_UInt64_Largest_Int32_63()
        {
            UInt64 a = 18446744073709551615;
            Int32 b = 63;
            a = a << b;
            if (a == 0x8000000000000000)
            {
                Log.WriteSuccess("Test_LShift_UInt64_Largest_Int32_63 okay.");
            }
            else
            {
                Log.WriteError("Test_LShift_UInt64_Largest_Int32_63 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Left shift operation shifting an unsigned 32-bit value, 
        /// Inputs: Largest, 6, 
        /// Result: 32-bit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_LShift_UInt32_Largest_Int32_6()
        {
            UInt32 a = 4294967295;
            Int32 b = 6;
            a = a << b;
            if (a == 4294967232)
            {
                Log.WriteSuccess("Test_LShift_UInt32_Largest_Int32_6 okay.");
            }
            else
            {
                Log.WriteError("Test_LShift_UInt32_Largest_Int32_6 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Right shift operation shifting a signed 32-bit value, 
        /// Inputs: Small +ve, 6, 
        /// Result: 32-bit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_LShift_Int32_SmallPos_Int32_6()
        {
            Int32 a = 255;
            Int32 b = 6;
            a = a << b;
            if (a == 16320)
            {
                Log.WriteSuccess("Test_LShift_Int32_SmallPos_Int32_6 okay.");
            }
            else
            {
                Log.WriteError("Test_LShift_Int32_SmallPos_Int32_6 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Left shift operation shifting a signed 32-bit value, 
        /// Inputs: Largest +ve, 6, 
        /// Result: 32-bit -ve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_LShift_Int32_LargestPos_Int32_6()
        {
            Int32 a = 2147483647;
            Int32 b = 6;
            a = a << b;
            if (a == -64)
            {
                Log.WriteSuccess("Test_LShift_Int32_LargestPos_Int32_6 okay.");
            }
            else
            {
                Log.WriteError("Test_LShift_Int32_LargestPos_Int32_6 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Right shift operation shifting a signed 32-bit value, 
        /// Inputs: Largest -ve, 6, 
        /// Result: 0.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_LShift_Int32_LargestNeg_Int32_6()
        {
            Int32 a = -2147483648;
            Int32 b = 1;
            a = a << b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_LShift_Int32_LargestNeg_Int32_6 okay.");
            }
            else
            {
                Log.WriteError("Test_LShift_Int32_LargestNeg_Int32_6 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Right shift operation shifting an unsigned 64-bit value, 
        /// Inputs: Largest, 10, 
        /// Result: 64-bit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// C# requires that the distance value is a signed 32-bit integer. 
        /// Only low order 5-bit is used when a 32-bit values is shifted, while low order 6-bit if a 64-bit value is shifted.
        /// In other words, a 32-/64-bit value cannot be pushed by more than 31/63 bits.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_LShift_UInt64_Largest_Int32_10()
        {
            UInt64 a = 18446744073709551615;
            Int32 b = 10;
            a = a << b;
            if (a == 18446744073709550592)
            {
                Log.WriteSuccess("Test_LShift_UInt64_Largest_Int32_10 okay.");
            }
            else
            {
                Log.WriteError("Test_LShift_UInt64_Largest_Int32_10 NOT okay.");
            }
        }

        #endregion

        #region Negation

        /// <summary>
        /// Tests: Negation operation using a signed 64-bit value, 
        /// Input: 64-bit (Largest -ve) - 1, 
        /// Result: 64-bit Largest +ve.
        /// </summary>
        [NoGC]
        public static void Test_Neg_Int64_LargestNeg_Int64()
        {
            Int64 a = -9223372036854775807;
            Int64 b = -a;
            if (b == 9223372036854775807)
            {
                Log.WriteSuccess("Test_Neg_Int64_LargestNeg_Int64 okay.");
            }
            else
            {
                Log.WriteError("Test_Neg_Int64_LargestNeg_Int64 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Negation operation using a signed 32-bit value, 
        /// Input: 32-bit Small -ve, 
        /// Result: 32-bit Small +ve.
        /// </summary>
        [NoGC]
        public static void Test_Neg_Int32_SmallNeg_Int32()
        {
            Int32 a = -100;
            Int32 b = -a;
            if (b == 100)
            {
                Log.WriteSuccess("Test_Neg_Int32_SmallNeg_Int32 okay.");
            }
            else
            {
                Log.WriteError("Test_Neg_Int32_SmallNeg_Int32 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Negation operation using an unsigned 32-bit value, 
        /// Input: 32-bit Largest, 
        /// Result: 64-bit -ve.
        /// </summary>
        [NoGC]
        public static void Test_Neg_UInt32_Largest_Int64()
        {
            UInt32 a = 4294967295;
            Int64 b = -a;
            if (b == -4294967295)
            {
                Log.WriteSuccess("Test_Neg_UInt32_Largest_Int64 okay.");
            }
            else
            {
                Log.WriteError("Test_Neg_UInt32_Largest_Int64 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Negation operation using a signed 64-bit value, 
        /// Input: 64-bit Largest +ve, 
        /// Result: 64-bit (Largest -ve) - 1.
        /// </summary>
        [NoGC]
        public static void Test_Neg_Int64_LargestPos_Int64()
        {
            Int64 a = 9223372036854775807;
            Int64 b = -a;
            if (b == -9223372036854775807)
            {
                Log.WriteSuccess("Test_Neg_Int64_LargestPos_Int64 okay.");
            }
            else
            {
                Log.WriteError("Test_Neg_Int64_LargestPos_Int64 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Negation operation using a signed 64-bit value, 
        /// Input: 64-bit Large +ve, 
        /// Result: 64-bit Large -ve.
        /// </summary>
        [NoGC]
        public static void Test_Neg_Int64_LargePos_Int64()
        {
            Int64 a = 372036854775807;
            Int64 b = -a;
            if (b == -372036854775807)
            {
                Log.WriteSuccess("Test_Neg_Int64_LargePos_Int64 okay.");
            }
            else
            {
                Log.WriteError("Test_Neg_Int64_LargePos_Int64 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Negation operation using a signed 32-bit value, 
        /// Input: 32-bit Small +ve, 
        /// Result: 32-bit Small -ve.
        /// </summary>
        [NoGC]
        public static void Test_Neg_Int32_SmallPos_Int32()
        {
            Int32 a = 100;
            Int32 b = -a;
            if (b == -100)
            {
                Log.WriteSuccess("Test_Neg_Int32_SmallPos_Int32 okay.");
            }
            else
            {
                Log.WriteError("Test_Neg_Int32_SmallPos_Int32 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Negation operation using a signed 64-bit value, 
        /// Input: 64-bit Large -ve, 
        /// Result: 64-bit Large +ve.
        /// </summary>
        [NoGC]
        public static void Test_Neg_Int64_LargeNeg_Int64()
        {
            Int64 a = -3372036854775807;
            Int64 b = -a;
            if (b == 3372036854775807)
            {
                Log.WriteSuccess("Test_Neg_Int64_LargeNeg_Int64 okay.");
            }
            else
            {
                Log.WriteError("Test_Neg_Int64_LargeNeg_Int64 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Negation operation using an unsigned 32-bit value, 
        /// Input: 32-bit Small, 
        /// Result: 64-bit -ve.
        /// </summary>
        [NoGC]
        public static void Test_Neg_UInt32_Small_Int64()
        {
            UInt32 a = 1;
            Int64 b = -a;
            if (b == -1)
            {
                Log.WriteSuccess("Test_Neg_UInt32_Small_Int64 okay.");
            }
            else
            {
                Log.WriteError("Test_Neg_UInt32_Small_Int64 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Negation operation using a signed 32-bit value, 
        /// Input: 32-bit Large +ve, 
        /// Result: 32-bit Large -ve as a 64-bit value.
        /// </summary>
        [NoGC]
        public static void Test_Neg_Int32_LargePos_Int64()
        {
            Int32 a = 1000000000;
            Int64 b = -a;
            if (b == -1000000000)
            {
                Log.WriteSuccess("Test_Neg_Int32_LargePos_Int64 okay.");
            }
            else
            {
                Log.WriteError("Test_Neg_Int32_LargePos_Int64 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Negation operation using a signed 32-bit value, 
        /// Input: 32-bit Large -ve, 
        /// Result: 32-bit Large +ve as a 64-bit value.
        /// </summary>
        [NoGC]
        public static void Test_Neg_Int32_LargeNeg_Int64()
        {
            Int32 a = -1000000000;
            Int64 b = -a;
            if (b == 1000000000)
            {
                Log.WriteSuccess("Test_Neg_Int32_LargeNeg_Int64 okay.");
            }
            else
            {
                Log.WriteError("Test_Neg_Int32_LargeNeg_Int64 NOT okay.");
            }
        }

        #endregion

        #region Not

        /// <summary>
        /// Tests: Not operation using a signed 64-bit value, 
        /// Input: 64-bit (Largest -ve) - 1, 
        /// Result: 64-bit (Largest +ve) - 1 because of two's complement.
        /// </summary>
        [NoGC]
        public static void Test_Not_Int64_LargestNeg_Int64()
        {
            Int64 a = -9223372036854775807;
            Int64 b = ~a;
            if (b == 9223372036854775806)
            {
                Log.WriteSuccess("Test_Not_Int64_LargestNeg_Int64 okay.");
            }
            else
            {
                Log.WriteError("Test_Not_Int64_LargestNeg_Int64 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Not operation using a signed 32-bit value, 
        /// Input: 32-bit Small -ve, 
        /// Result: 32-bit Small +ve.
        /// </summary>
        [NoGC]
        public static void Test_Not_Int32_SmallNeg_Int32()
        {
            Int32 a = -100;
            Int32 b = ~a;
            if (b == 99)
            {
                Log.WriteSuccess("Test_Not_Int32_SmallNeg_Int32 okay.");
            }
            else
            {
                Log.WriteError("Test_Not_Int32_SmallNeg_Int32 NOT okay.");
            }
        }

        ///// <summary>
        ///// Tests: Not operation using an unsigned 32-bit value, 
        ///// Input: 32-bit Largest, 
        ///// Result: 64-bit -ve.
        ///// </summary>
        //[NoGC]
        //public static void Test_Not_UInt32_Largest_Int64()
        //{
        //    UInt32 a = 4294967295;
        //    Int64 b = ~a;
        //    if (b == -4294967296)
        //    {
        //        Log.WriteSuccess("Test_Not_UInt32_Largest_Int64 okay.");
        //    }
        //    else
        //    {
        //        Log.WriteError("Test_Not_UInt32_Largest_Int64 NOT okay.");
        //    }
        //}

        /// <summary>
        /// Tests: Not operation using a signed 64-bit value, 
        /// Input: 64-bit Largest +ve, 
        /// Result: 64-bit Largest -ve.
        /// </summary>
        [NoGC]
        public static void Test_Not_Int64_LargestPos_Int64()
        {
            Int64 a = 9223372036854775807;
            Int64 b = ~a;
            if (b == -9223372036854775808)
            {
                Log.WriteSuccess("Test_Not_Int64_LargestPos_Int64 okay.");
            }
            else
            {
                Log.WriteError("Test_Not_Int64_LargestPos_Int64 NOT okay.");
            }
        }

        ///// <summary>
        ///// Tests: Negation operation using a signed 64-bit value, 
        ///// Input: 64-bit Large +ve, 
        ///// Result: 64-bit Large -ve.
        ///// </summary>
        //[NoGC]
        //public static void Test_Neg_Int64_LargePos_Int64()
        //{
        //    Int64 a = 372036854775807;
        //    Int64 b = -a;
        //    if (b == -372036854775807)
        //    {
        //        Log.WriteSuccess("Test_Neg_Int64_LargePos_Int64 okay.");
        //    }
        //    else
        //    {
        //        Log.WriteError("Test_Neg_Int64_LargePos_Int64 NOT okay.");
        //    }
        //}

        ///// <summary>
        ///// Tests: Negation operation using a signed 32-bit value, 
        ///// Input: 32-bit Small +ve, 
        ///// Result: 32-bit Small -ve.
        ///// </summary>
        //[NoGC]
        //public static void Test_Neg_Int32_SmallPos_Int32()
        //{
        //    Int32 a = 100;
        //    Int32 b = -a;
        //    if (b == -100)
        //    {
        //        Log.WriteSuccess("Test_Neg_Int32_SmallPos_Int32 okay.");
        //    }
        //    else
        //    {
        //        Log.WriteError("Test_Neg_Int32_SmallPos_Int32 NOT okay.");
        //    }
        //}

        ///// <summary>
        ///// Tests: Negation operation using a signed 64-bit value, 
        ///// Input: 64-bit Large -ve, 
        ///// Result: 64-bit Large +ve.
        ///// </summary>
        //[NoGC]
        //public static void Test_Neg_Int64_LargeNeg_Int64()
        //{
        //    Int64 a = -3372036854775807;
        //    Int64 b = -a;
        //    if (b == 3372036854775807)
        //    {
        //        Log.WriteSuccess("Test_Neg_Int64_LargeNeg_Int64 okay.");
        //    }
        //    else
        //    {
        //        Log.WriteError("Test_Neg_Int64_LargeNeg_Int64 NOT okay.");
        //    }
        //}

        ///// <summary>
        ///// Tests: Negation operation using an unsigned 32-bit value, 
        ///// Input: 32-bit Small, 
        ///// Result: 64-bit -ve.
        ///// </summary>
        //[NoGC]
        //public static void Test_Neg_UInt32_Small_Int64()
        //{
        //    UInt32 a = 1;
        //    Int64 b = -a;
        //    if (b == -1)
        //    {
        //        Log.WriteSuccess("Test_Neg_UInt32_Small_Int64 okay.");
        //    }
        //    else
        //    {
        //        Log.WriteError("Test_Neg_UInt32_Small_Int64 NOT okay.");
        //    }
        //}

        ///// <summary>
        ///// Tests: Negation operation using a signed 32-bit value, 
        ///// Input: 32-bit Large +ve, 
        ///// Result: 32-bit Large -ve as a 64-bit value.
        ///// </summary>
        //[NoGC]
        //public static void Test_Neg_Int32_LargePos_Int64()
        //{
        //    Int32 a = 1000000000;
        //    Int64 b = -a;
        //    if (b == -1000000000)
        //    {
        //        Log.WriteSuccess("Test_Neg_Int32_LargePos_Int64 okay.");
        //    }
        //    else
        //    {
        //        Log.WriteError("Test_Neg_Int32_LargePos_Int64 NOT okay.");
        //    }
        //}

        ///// <summary>
        ///// Tests: Negation operation using a signed 32-bit value, 
        ///// Input: 32-bit Large -ve, 
        ///// Result: 32-bit Large +ve as a 64-bit value.
        ///// </summary>
        //[NoGC]
        //public static void Test_Neg_Int32_LargeNeg_Int64()
        //{
        //    Int32 a = -1000000000;
        //    Int64 b = -a;
        //    if (b == 1000000000)
        //    {
        //        Log.WriteSuccess("Test_Neg_Int32_LargeNeg_Int64 okay.");
        //    }
        //    else
        //    {
        //        Log.WriteError("Test_Neg_Int32_LargeNeg_Int64 NOT okay.");
        //    }
        //}

        #endregion
    }
}
