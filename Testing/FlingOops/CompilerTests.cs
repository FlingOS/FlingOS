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
            #region 1. Addition calls

            Log.WriteLine("---Addition:");
            Log.WriteLine(" 32-32");
            Log.WriteLine("  Unsigned");
            Test_Add_UInt32_Zero_UInt32_Zero();
            Test_Add_UInt32_9_UInt32_4();
            Log.WriteLine("  Signed");
            Test_Add_Int32_9_Int32_4();
            Test_Add_Int32_9_Int32_Neg4();
            Test_Add_Int32_Neg9_Int32_4();
            Test_Add_Int32_Neg9_Int32_Neg4();
            Log.WriteLine(" 64-32");
            Log.WriteLine("  Unsigned");
            Test_Add_UInt64_LargestPos_UInt32_4();
            Log.WriteLine("  Signed");
            Test_Add_Int64_LargestPos_Int32_4();
            Test_Add_Int64_LargestNeg_Int32_4();
            Test_Add_Int64_Zero_Int32_LargestNeg();
            Log.WriteLine(" 64-64");
            Log.WriteLine("  Unsigned");
            Test_Add_UInt64_Large_UInt64_Large();
            Log.WriteLine("  Signed");
            Test_Add_Int64_LargePos_Int64_4();
            Test_Add_Int64_LargePos_Int64_LargePos();
            Test_Add_Int64_LargePos_Int64_LargeNeg();
            Test_Add_Int64_LargestNeg_Int64_Neg1();
            Test_Add_Int64_LargeNeg_Int64_LargeNeg();
            Test_Add_Int64_Zero_Int64_Neg4();
            Test_Add_Int64_Zero_Int64_LargePos();
            Test_Add_Int64_Zero_Int64_LargeNeg();
            Log.WriteLine(" ");

            #endregion

            #region 2. Subtraction calls

            Log.WriteLine("---Subtraction:");
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
            Test_Sub_Int64_LargestPos_Int32_4();
            Test_Sub_Int64_LargestNeg_Int32_4();
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

            #region 3. Multiplication calls

            Log.WriteLine("---Multiplication:");
            Log.WriteLine(" 32-32");
            Log.WriteLine("  Unsigned");
            Test_Mul_UInt32_Zero_UInt3_Zero();
            Test_Mul_UInt32_9_UInt32_4();
            Log.WriteLine("  Signed");
            Test_Mul_Int32_9_Int32_4();
            Test_Mul_Int32_9_Int32_Neg4();
            Test_Mul_Int32_Neg9_Int32_4();
            Test_Mul_Int32_Neg9_Int32_Neg4();
            Log.WriteLine(" 64-32");
            Log.WriteLine("  Unsigned");
            Test_Mul_UInt64_Zero_UInt32_Zero();
            Test_Mul_UInt64_Large_UInt32_4();
            Log.WriteLine("  Signed");
            Test_Mul_Int64_Zero_Int32_LargestNeg();
            Test_Mul_Int64_Zero_Int32_LargestPos();
            Test_Mul_Int64_LargestPos_Int32_4();
            Test_Mul_Int64_LargestPos_Int32_Neg4();
            Test_Mul_Int64_LargestNeg_Int32_4();
            Test_Mul_Int64_LargestNeg_Int32_Neg4();
            Log.WriteLine(" 64-64");
            Log.WriteLine("  Unsigned");
            Test_Mul_UInt64_Large_UInt64_Large();
            Log.WriteLine("  Signed");
            Test_Mul_Int64_Zero_Int64_LargePos();
            Test_Mul_Int64_Zero_Int64_Neg4();
            Test_Mul_Int64_LargePos_Int64_4();
            Test_Mul_Int64_LargePos_Int64_Neg4();
            Test_Mul_Int64_LargeNeg_Int64_4();
            Test_Mul_Int64_LargeNeg_Int64_Neg4();
            Test_Mul_Int64_LargePos_Int64_1000();
            Test_Mul_Int64_LargeNeg_Int64_1000();
            Test_Mul_Int64_Neg1_Int64_LargePos();
            Test_Mul_Int64_Neg1_Int64_LargeNeg();
            Test_Mul_Int64_LargestPos_Int64_Neg4();
            Test_Mul_Int64_LargestNeg_Int64_Neg4();
            Log.WriteLine(" ");

            #endregion

            #region 4. Division calls

            Log.WriteLine("---Division:");
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

            #region 5. Modulus calls

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

            #region 6. Negation calls

            Log.WriteLine("---Negation:");
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

            #region 7. Not calls

            Log.WriteLine("---Not:");
            Log.WriteLine(" 32");
            Log.WriteLine("  Unsigned");
            Log.WriteLine("UInt32 cannot be not-ed into Int32, only into Int64 in C#.");
            Test_Not_UInt32_Small_Int64();
            Test_Not_UInt32_Largest_Int64();
            Log.WriteLine("  Signed");
            Test_Not_Int32_SmallPos_Int32();
            Test_Not_Int32_SmallNeg_Int32();
            Test_Not_Int32_LargePos_Int64();
            Test_Not_Int32_LargeNeg_Int64();
            Log.WriteLine(" 64");
            Log.WriteLine("  Unsigned");
            Test_Not_UInt64_Smallest_UInt64();
            Test_Not_UInt64_Largest_UInt64();
            Log.WriteLine("  Signed");
            Test_Not_Int64_LargePos_Int64();
            Test_Not_Int64_LargeNeg_Int64();
            Test_Not_Int64_LargestPos_Int64();
            Test_Not_Int64_LargestNeg_Int64();
            Log.WriteLine(" ");

            #endregion

            #region 8. Array calls

            Log.WriteLine("---Array:");
            Log.WriteLine(" 32");
            Log.WriteLine("  Unsigned");
            Test_Array_UInt32();
            Log.WriteLine("  Signed");
            Test_Array_Int32();
            Log.WriteLine(" 64");
            Log.WriteLine("  Unsigned");
            Test_Array_UInt64();
            Log.WriteLine("  Signed");
            Test_Array_Int64();
            Log.WriteLine(" Strings");
            Test_Array_String();
            Log.WriteLine(" Structs");
            Test_Array_Struct();
            Log.WriteLine(" Objects");
            Test_Array_Object();
            Log.WriteLine(" ");

            #endregion

            #region 9. Strings calls

            Log.WriteLine("---String:");
            Test_Strings();
            Log.WriteLine(" ");

            #endregion

            #region 10. Argument calls

            // Variables used as arguments to test methods
            {
                Int32 sign32 = 6;
                Int64 sign64 = 1441151880758558720;
                UInt32 unsign32 = 100;
                UInt64 unsign64 = 10223372036854775807;
                FlingOops.String str = "I am a string";
                FlingOops.String str2 = "I am a string too";
                Log.WriteLine("---Argument:");
                Test_Arg_Int32(sign32);
                Test_Arg_Int64(sign64);
                Test_Arg_UInt32(unsign32);
                Test_Arg_UInt64(unsign64);
                Test_Arg_String(str);
                AStruct Inst = new AStruct();
                Inst.a = 1;
                Inst.b = 2;
                Inst.c = 3;
                Inst.d = 4;
                Test_Arg_Struct(Inst);
                Test_Arg_Param(sign32, sign64, unsign32, unsign64, str, str2); 
                Log.WriteLine(" ");
            }

            #endregion

            #region 11. Right shift calls

            Log.WriteLine("---Right shift:");
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

            #region 12. Left shift calls

            Log.WriteLine("---Left shift:");
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

            #region 13. Struct calls

            Log.WriteLine("---Struct:");
            Test_Sizeof_Struct();
            Test_Instance_Struct();
            Log.WriteLine(" ");

            #endregion

            #region 14. Variables and pointers calls

            Log.WriteLine("---Variables and pointers:");
            Test_Locals_And_Pointers();
            Log.WriteLine(" ");

            #endregion

            #region 15. Switch calls

            Log.WriteLine("---Switch:");
            Log.WriteLine(" Integers");
            Test_Switch_Int32_Case_0();
            Test_Switch_Int32_Case_1();
            Test_Switch_Int32_Case_2();
            Test_Switch_Int32_Case_Default();
            Test_Switch_Int32_Case_0_Ret_NoValue();
            Log.WriteLine("  Successfully returned from Test_Switch_Int32_Case_0_Ret_NoValue()");
            Test_Switch_Int32_Case_0_Ret_IntValue();
            Log.WriteLine("  Successfully returned from Test_Switch_Int32_Case_0_Ret_IntValue()");
            Test_Switch_Int32_Case_0_Ret_StringValue();
            Log.WriteLine("  Successfully returned from Test_Switch_Int32_Case_0_Ret_StringValue()");
            Log.WriteLine(" Strings");
            Test_Switch_String_Case_0();
            Log.WriteLine(" ");

            #endregion

            #region 16. Heap calls

            Log.WriteLine("---Heap:");
            Test_Heap();
            Log.WriteLine(" ");

            #endregion
            
            #region 17. Object calls

            Log.WriteLine("---Object:");
            Test_Objects();
            Log.WriteLine(" ");

            #endregion
            
            #region 18. Try-Catch-Finally calls

            Log.WriteLine("---Try-Catch-Finally:");
            Log.WriteLine(" TCF 0");
            Test_TCF_0();
            Log.WriteLine(" TCF 1");
            Test_TCF_1();
            Log.WriteLine(" TCF 2");
            Test_TCF_2();
            Log.WriteLine(" TCF 3");
            Test_TCF_3();
            Log.WriteLine(" TCF 4");
            Test_TCF_4();
            Log.WriteLine(" TCF 5");
            Test_TCF_5();
            Log.WriteLine(" ");

            #endregion

            #region Xor calls

            Log.WriteLine("---Xor:");
            Test_Xor_Int();
            Log.WriteLine(" ");

            #endregion

            Log.WriteLine("Tests completed.");
        }

        #region 1. Addition

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

        /// <summary>
        /// Tests: Addition operation using unsigned 32-bit integers, 
        /// Inputs: Small, Small, 
        /// Result: Small
        /// </summary>
        [NoGC]
        public static void Test_Add_UInt32_9_UInt32_4()
        {
            UInt32 a = 9;
            UInt32 b = 4;
            a = a + b;
            if (a == 13)
            {
                Log.WriteSuccess("Test_Add_UInt32_9_UInt32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Add_UInt32_9_UInt32_4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Addition operation using signed 32-bit integers, 
        /// Inputs: Small -ve, Small +ve, 
        /// Result: Small -ve
        /// </summary>
        [NoGC]
        public static void Test_Add_Int32_Neg9_Int32_4()
        {
            Int32 a = -9;
            Int32 b = 4;
            a = a + b;
            if (a == -5)
            {
                Log.WriteSuccess("Test_Add_Int32_Neg9_Int32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Add_Int32_Neg9_Int32_4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Addition operation using signed 32-bit integers, 
        /// Inputs: Small -ve, Small -ve, 
        /// Result: Small -ve
        /// </summary>
        [NoGC]
        public static void Test_Add_Int32_Neg9_Int32_Neg4()
        {
            Int32 a = -9;
            Int32 b = -4;
            a = a + b;
            if (a == -13)
            {
                Log.WriteSuccess("Test_Add_Int32_Neg9_Int32_Neg4 okay.");
            }
            else
            {
                Log.WriteError("Test_Add_Int32_Neg9_Int32_Neg4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Addition operation using signed 32-bit integers, 
        /// Inputs: Small +ve, Small-ve, 
        /// Result: Small +ve
        /// </summary>
        [NoGC]
        public static void Test_Add_Int32_9_Int32_Neg4()
        {
            Int32 a = 9;
            Int32 b = -4;
            a = a + b;
            if (a == 5)
            {
                Log.WriteSuccess("Test_Add_Int32_9_Int32_Neg4 okay.");
            }
            else
            {
                Log.WriteError("Test_Add_Int32_9_Int32_Neg4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Addition operation using signed 32-bit integers, 
        /// Inputs: Small +ve, Small +ve, 
        /// Result: Small +ve
        /// </summary>
        [NoGC]
        public static void Test_Add_Int32_9_Int32_4()
        {
            Int32 a = 9;
            Int32 b = 4;
            a = a + b;
            if (a == 13)
            {
                Log.WriteSuccess("Test_Add_Int32_9_Int32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Add_Int32_9_Int32_4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Addition operation using unsigned 64- and 32-bit integers, 
        /// Inputs: Largest - 4, 4, 
        /// Result: Largest
        /// </summary>
        /// <remarks>
        /// <para>
        /// When adding 64-bit values, care must be taken to handle the carry-bit correctly
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Add_UInt64_LargestPos_UInt32_4()
        {
            UInt64 a = 18446744073709551611;
            UInt32 b = 4;
            a = a + b;
            if (a == 18446744073709551615)
            {
                Log.WriteSuccess("Test_Add_UInt64_LargestPos_UInt32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Add_UInt64_LargestPos_UInt32_4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Addition operation using signed 64- and 32-bit integers, 
        /// Inputs: Largest +ve, Small +ve, 
        /// Result: (Largest +ve) + 4 = (Largest -ve) + 3 - circularity of two's complement 
        /// </summary>
        /// <remarks>
        /// <para>
        /// When adding 64-bit values, care must be taken to handle the carry-bit correctly
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Add_Int64_LargestPos_Int32_4()
        {
            Int64 a = 9223372036854775807;
            Int32 b = 4;
            a = a + b;
            if (a == -9223372036854775805)
            {
                Log.WriteSuccess("Test_Add_Int64_LargestPos_Int32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Add_Int64_LargestPos_Int32_4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Addition operation using signed 64- and 32-bit integers, 
        /// Inputs: Largest -ve, Small +ve, 
        /// Result: (Largest -ve) + 4
        /// </summary>
        /// <remarks>
        /// <para>
        /// When adding 64-bit values, care must be taken to handle the carry-bit correctly
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Add_Int64_LargestNeg_Int32_4()
        {
            Int64 a = -9223372036854775808;
            Int32 b = 4;
            a = a + b;
            if (a == -9223372036854775804)
            {
                Log.WriteSuccess("Test_Add_Int64_LargestNeg_Int32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Add_Int64_LargestNeg_Int32_4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Addition operation using signed 64- and 32-bit integers, 
        /// Inputs: 0, Largest -ve, 
        /// Result: Largest +ve
        /// </summary>
        /// <remarks>
        /// <para>
        /// When adding 64-bit values, care must be taken to handle the carry-bit correctly
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Add_Int64_Zero_Int32_LargestNeg()
        {
            Int64 a = 0;
            Int32 b = -2147483648;
            a = a + b;
            if (a == -2147483648)
            {
                Log.WriteSuccess("Test_Add_Int64_Zero_Int32_LargestNeg okay.");
            }
            else
            {
                Log.WriteError("Test_Add_Int64_Zero_Int32_LargestNeg NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Addition operation using signed 64-bit integers, 
        /// Inputs: Large +ve, 4, 
        /// Result: Large +ve
        /// </summary>
        /// <remarks>
        /// <para>
        /// When adding 64-bit values, care must be taken to handle the carry-bit correctly
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Add_Int64_LargePos_Int64_4()
        {
            Int64 a = 1080863910568919040;
            Int64 b = 4;
            a = a + b;
            if (a == 1080863910568919044)
            {
                Log.WriteSuccess("Test_Add_Int64_LargePos_Int64_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Add_Int64_LargePos_Int64_4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Addition operation using signed 64-bit integers, 
        /// Inputs: 0, -4, 
        /// Result: -4
        /// </summary>
        /// <remarks>
        /// <para>
        /// When adding 64-bit values, care must be taken to handle the carry-bit correctly
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Add_Int64_Zero_Int64_Neg4()
        {
            Int64 a = 0;
            Int64 b = -4;
            a = a + b;
            if (a == -4)
            {
                Log.WriteSuccess("Test_Add_Int64_Zero_Int64_Neg4 okay.");
            }
            else
            {
                Log.WriteError("Test_Add_Int64_Zero_Int64_Neg4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Addition operation using unsigned 64-bit integers, 
        /// Inputs: Large +ve, Large +ve, 
        /// Result: +ve
        /// </summary>
        /// <remarks>
        /// <para>
        /// When adding 64-bit values, care must be taken to handle the carry-bit correctly
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Add_UInt64_Large_UInt64_Large()
        {
            UInt64 a = 108086391056891904;
            UInt64 b = 844424930131968;
            a = a + b;
            if (a == 108930815987023872)
            {
                Log.WriteSuccess("Test_Add_UInt64_Large_UInt64_Large okay.");
            }
            else
            {
                Log.WriteError("Test_Add_UInt64_Large_UInt64_Large NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Addition operation using signed 64-bit integers, 
        /// Inputs: Largest -ve, -1, 
        /// Result: Largest +ve
        /// </summary>
        /// <remarks>
        /// <para>
        /// When adding 64-bit values, care must be taken to handle the carry-bit correctly
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Add_Int64_LargestNeg_Int64_Neg1()
        {
            Int64 a = -9223372036854775808;
            Int64 b = -1;
            a = a + b;
            if (a == 9223372036854775807)
            {
                Log.WriteSuccess("Test_Add_Int64_LargestNeg_Int64_Neg1 okay.");
            }
            else
            {
                Log.WriteError("Test_Add_Int64_LargestNeg_Int64_Neg1 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Addition operation using signed 64-bit integers, 
        /// Inputs: 0, Large +ve, 
        /// Result: Large +ve
        /// </summary>
        /// <remarks>
        /// <para>
        /// When adding 64-bit values, care must be taken to handle the carry-bit correctly
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Add_Int64_Zero_Int64_LargePos()
        {
            Int64 a = 0;
            Int64 b = 844424930131968;
            a = a + b;
            if (a == 844424930131968)
            {
                Log.WriteSuccess("Test_Add_Int64_Zero_Int64_LargePos okay.");
            }
            else
            {
                Log.WriteError("Test_Add_Int64_Zero_Int64_LargePos NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Addition operation using signed 64-bit integers, 
        /// Inputs: 0, Large -ve, 
        /// Result: Large -ve
        /// </summary>
        /// <remarks>
        /// <para>
        /// When adding 64-bit values, care must be taken to handle the carry-bit correctly
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Add_Int64_Zero_Int64_LargeNeg()
        {
            Int64 a = 0;
            Int64 b = -844424930131968;
            a = a + b;
            if (a == -844424930131968)
            {
                Log.WriteSuccess("Test_Add_Int64_Zero_Int64_LargeNeg okay.");
            }
            else
            {
                Log.WriteError("Test_Add_Int64_Zero_Int64_LargeNeg NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Addition operation using signed 64-bit integers, 
        /// Inputs: Large -ve, Large -ve, 
        /// Result: Large -ve
        /// </summary>
        /// <remarks>
        /// <para>
        /// When adding 64-bit values, care must be taken to handle the carry-bit correctly
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Add_Int64_LargeNeg_Int64_LargeNeg()
        {
            Int64 a = -1080863910568919040;
            Int64 b = -844424930131968;
            a = a + b;
            if (a == -1081708335499051008)
            {
                Log.WriteSuccess("Test_Add_Int64_LargeNeg_Int64_LargeNeg okay.");
            }
            else
            {
                Log.WriteError("Test_Add_Int64_LargeNeg_Int64_LargeNeg NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Addition operation using signed 64-bit integers, 
        /// Inputs: Large +ve, Large -ve, 
        /// Result: Large +ve
        /// </summary>
        /// <remarks>
        /// <para>
        /// When adding 64-bit values, care must be taken to handle the carry-bit correctly
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Add_Int64_LargePos_Int64_LargeNeg()
        {
            Int64 a = 1080863910568919040;
            Int64 b = -844424930131968;
            a = a + b;
            if (a == 1080019485638787072)
            {
                Log.WriteSuccess("Test_Add_Int64_LargePos_Int64_LargeNeg okay.");
            }
            else
            {
                Log.WriteError("Test_Add_Int64_LargePos_Int64_LargeNeg NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Addition operation using signed 64-bit integers, 
        /// Inputs: Large +ve, Large +ve, 
        /// Result: Large +ve
        /// </summary>
        /// <remarks>
        /// <para>
        /// When adding 64-bit values, care must be taken to handle the carry-bit correctly
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Add_Int64_LargePos_Int64_LargePos()
        {
            Int64 a = 1080863910568919040;
            Int64 b = 844424930131968;
            a = a + b;
            if (a == 1081708335499051008)
            {
                Log.WriteSuccess("Test_Add_Int64_LargePos_Int64_LargePos okay.");
            }
            else
            {
                Log.WriteError("Test_Add_Int64_LargePos_Int64_LargePos NOT okay.");
            }
        }

        #endregion

        #region 2. Subtraction

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

        #region 3. Multiplication

        /// <summary>
        /// Tests: Multiplication operation using unsigned 32-bit integers, 
        /// Inputs: 0, 0, 
        /// Result: 0.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes.
        /// </para>
        /// </remarks>
        public static void Test_Mul_UInt32_Zero_UInt3_Zero()
        {
            UInt32 a = 0;
            UInt32 b = 0;
            a = a * b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_Mul_UInt32_Zero_Zero okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_UInt32_Zero_Zero NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using unsigned 32-bit integers, 
        /// Inputs: Small, Small, 
        /// Result: Small.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_UInt32_9_UInt32_4()
        {
            UInt32 a = 9;
            UInt32 b = 4;
            a = a * b;
            if (a == 36)
            {
                Log.WriteSuccess("Test_Mul_UInt32_9_UInt32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_UInt32_9_UInt32_4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 32-bit integers, 
        /// Inputs: Small -ve, Small +ve, 
        /// Result: Small -ve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int32_Neg9_Int32_4()
        {
            Int32 a = -9;
            Int32 b = 4;
            a = a * b;
            if (a == -36)
            {
                Log.WriteSuccess("Test_Mul_Int32_Neg9_Int32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int32_Neg9_Int32_4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 32-bit integers, 
        /// Inputs: Small -ve, Small -ve, 
        /// Result: Small +ve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int32_Neg9_Int32_Neg4()
        {
            Int32 a = -9;
            Int32 b = -4;
            a = a * b;
            if (a == 36)
            {
                Log.WriteSuccess("Test_Mul_Int32_Neg9_Int32_Neg4 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int32_Neg9_Int32_Neg4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 32-bit integers, 
        /// Inputs: Small +ve, Small-ve, 
        /// Result: Small -ve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int32_9_Int32_Neg4()
        {
            Int32 a = 9;
            Int32 b = -4;
            a = a * b;
            if (a == -36)
            {
                Log.WriteSuccess("Test_Mul_Int32_9_Int32_Neg4 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int32_9_Int32_Neg4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 32-bit integers, 
        /// Inputs: Small +ve, Small +ve, 
        /// Result: Small +ve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int32_9_Int32_4()
        {
            Int32 a = 9;
            Int32 b = 4;
            a = a * b;
            if (a == 36)
            {
                Log.WriteSuccess("Test_Mul_Int32_9_Int32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int32_9_Int32_4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using unsigned 64- and 32-bit integers, 
        /// Inputs: Large, 4, 
        /// Result: Large.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_UInt64_Large_UInt32_4()
        {
            UInt64 a = 184467440737095516;
            UInt32 b = 4;
            a = a * b;
            if (a == 737869762948382064)
            {
                Log.WriteSuccess("Test_Mul_UInt64_Large_UInt32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_UInt64_Large_UInt32_4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 64- and 32-bit integers, 
        /// Inputs: Largest +ve, 4, 
        /// Result: -4 (Overflow).
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int64_LargestPos_Int32_4()
        {
            Int64 a = 9223372036854775807;
            Int32 b = 4;
            a = a * b;
            if (a == -4)
            {
                Log.WriteSuccess("Test_Mul_Int64_LargestPos_Int32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int64_LargestPos_Int32_4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 64- and 32-bit integers, 
        /// Inputs: Largest -ve, 4, 
        /// Result: 0 (Overflow).
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int64_LargestNeg_Int32_4()
        {
            Int64 a = -9223372036854775808;
            Int32 b = 4;
            a = a * b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_Mul_Int64_LargestNeg_Int32_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int64_LargestNeg_Int32_4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 64- and 32-bit integers, 
        /// Inputs: 0, Largest -ve, 
        /// Result: 0.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int64_Zero_Int32_LargestNeg()
        {
            Int64 a = 0;
            Int32 b = -2147483648;
            a = a * b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_Mul_Int64_Zero_Int32_LargestNeg okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int64_Zero_Int32_LargestNeg NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 64-bit integers, 
        /// Inputs: Large +ve, 4, 
        /// Result: Large +ve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int64_LargePos_Int64_4()
        {
            Int64 a = 1080863910568919040;
            Int64 b = 4;
            a = a * b;
            if (a == 4323455642275676160)
            {
                Log.WriteSuccess("Test_Mul_Int64_LargePos_Int64_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int64_LargePos_Int64_4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 64-bit integers, 
        /// Inputs: 0, -4, 
        /// Result: -4.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int64_Zero_Int64_Neg4()
        {
            Int64 a = 0;
            Int64 b = -4;
            a = a * b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_Mul_Int64_Zero_Int64_Neg4 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int64_Zero_Int64_Neg4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using unsigned 64-bit integers, 
        /// Inputs: Large +ve, 4, 
        /// Result: Large +ve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_UInt64_Large_UInt64_Large()
        {
            UInt64 a = 108086391056891904;
            UInt64 b = 4;
            a = a * b;
            if (a == 432345564227567616)
            {
                Log.WriteSuccess("Test_Mul_UInt64_Large_UInt64_Large okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_UInt64_Large_UInt64_Large NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 64-bit integers, 
        /// Inputs: Largest -ve, -4, 
        /// Result: 0 (Overflow).
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int64_LargestNeg_Int64_Neg4()
        {
            Int64 a = -9223372036854775808;
            Int64 b = -4;
            a = a * b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_Mul_Int64_LargestNeg_Int64_Neg4 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int64_LargestNeg_Int64_Neg4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 64-bit integers, 
        /// Inputs: 0, Large +ve, 
        /// Result: 0.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int64_Zero_Int64_LargePos()
        {
            Int64 a = 0;
            Int64 b = 844424930131968;
            a = a * b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_Mul_Int64_Zero_Int64_LargePos okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int64_Zero_Int64_LargePos NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 64-bit integers, 
        /// Inputs: -1, Large -ve, 
        /// Result: Large +ve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int64_Neg1_Int64_LargeNeg()
        {
            Int64 a = -1;
            Int64 b = -844424930131968;
            a = a * b;
            if (a == 844424930131968)
            {
                Log.WriteSuccess("Test_Mul_Int64_Neg1_Int64_LargeNeg okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int64_Neg1_Int64_LargeNeg NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 64-bit integers, 
        /// Inputs: -1, Large +ve, 
        /// Result: Large -ve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int64_Neg1_Int64_LargePos()
        {
            Int64 a = -1;
            Int64 b = 844424930131968;
            a = a * b;
            if (a == -844424930131968)
            {
                Log.WriteSuccess("Test_Mul_Int64_Neg1_Int64_LargePos okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int64_Neg1_Int64_LargePos NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 64-bit integers, 
        /// Inputs: Large +ve, 1000, 
        /// Result: Large -ve (Overflow).
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int64_LargePos_Int64_1000()
        {
            Int64 a = 1080863910568919040;
            Int64 b = 1000;
            a = a * b;
            if (a == -7493989779944505344)
            {
                Log.WriteSuccess("Test_Mul_Int64_LargePos_Int64_1000 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int64_LargePos_Int64_1000 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 64-bit integers, 
        /// Inputs: Large -ve, 1000, 
        /// Result: Large +ve
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int64_LargeNeg_Int64_1000()
        {
            Int64 a = -1080863910568919040;
            Int64 b = 1000;
            a = a * b;
            if (a == 7493989779944505344)
            {
                Log.WriteSuccess("Test_Mul_Int64_LargeNeg_Int64_1000 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int64_LargeNeg_Int64_1000 NOT okay.");
            }
        }


        /// <summary>
        /// Tests: Multiplication operation using signed 64- and 32-bit integers, 
        /// Inputs: Largest +ve, -4, 
        /// Result: 4 (Overflow).
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int64_LargestPos_Int32_Neg4()
        {
            Int64 a = 9223372036854775807;
            Int32 b = -4;
            a = a * b;
            if (a == 4)
            {
                Log.WriteSuccess("Test_Mul_Int64_LargestPos_Int32_Neg4 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int64_LargestPos_Int32_Neg4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 64- and 32-bit integers, 
        /// Inputs: Largest -ve, -4, 
        /// Result: 0 (Overflow).
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int64_LargestNeg_Int32_Neg4()
        {
            Int64 a = -9223372036854775808;
            Int32 b = -4;
            a = a * b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_Mul_Int64_LargestNeg_Int32_Neg4 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int64_LargestNeg_Int32_Neg4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 64-bit integers, 
        /// Inputs: Large -ve, 4, 
        /// Result: Large -ve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int64_LargeNeg_Int64_4()
        {
            Int64 a = -1080863910568919040;
            Int64 b = 4;
            a = a * b;
            if (a == -4323455642275676160)
            {
                Log.WriteSuccess("Test_Mul_Int64_LargeNeg_Int64_4 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int64_LargeNeg_Int64_4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using unsigned 64- and 32-bit integers, 
        /// Inputs: 0, 0, 
        /// Result: 0.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_UInt64_Zero_UInt32_Zero()
        {
            UInt64 a = 0;
            UInt32 b = 0;
            a = a * b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_Mul_UInt64_Zero_UInt32_Zero okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_UInt64_Zero_UInt32_Zero NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 64- and 32-bit integers, 
        /// Inputs: 0, Largest +ve, 
        /// Result: 0.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int64_Zero_Int32_LargestPos()
        {
            Int64 a = 0;
            Int32 b = 2147483647;
            a = a * b;
            if (a == 0)
            {
                Log.WriteSuccess("Test_Mul_Int64_Zero_Int32_LargestPos okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int64_Zero_Int32_LargestPos NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 64-bit integers, 
        /// Inputs: Largest +ve, -4, 
        /// Result: 4 (Overflow).
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int64_LargestPos_Int64_Neg4()
        {
            Int64 a = 9223372036854775807;
            Int64 b = -4;
            a = a * b;
            if (a == 4)
            {
                Log.WriteSuccess("Test_Mul_Int64_LargestPos_Int64_Neg4 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int64_LargestPos_Int64_Neg4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 64-bit integers, 
        /// Inputs: Large +ve, -4, 
        /// Result: Large -ve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int64_LargePos_Int64_Neg4()
        {
            Int64 a = 1080863910568919040;
            Int64 b = -4;
            a = a * b;
            if (a == -4323455642275676160)
            {
                Log.WriteSuccess("Test_Mul_Int64_LargePos_Int64_Neg4 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int64_LargePos_Int64_Neg4 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Multiplication operation using signed 64-bit integers, 
        /// Inputs: Large -ve, -4, 
        /// Result: Large +ve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS treats all 32-/64-bit multiplication as signed/unsigned. 
        /// Different operand sizes aren't allowed.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Mul_Int64_LargeNeg_Int64_Neg4()
        {
            Int64 a = -1080863910568919040;
            Int64 b = -4;
            a = a * b;
            if (a == 4323455642275676160)
            {
                Log.WriteSuccess("Test_Mul_Int64_LargeNeg_Int64_Neg4 okay.");
            }
            else
            {
                Log.WriteError("Test_Mul_Int64_LargeNeg_Int64_Neg4 NOT okay.");
            }
        }

        #endregion

        #region 4. Division

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
            if (a == 3)
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

        #region 5. Modulus

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
            if (a == 0)
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

        #region 6. Negation

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

        #region 7. Not

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

        /// <summary>
        /// Tests: Not operation using an unsigned 32-bit value, 
        /// Input: 32-bit Largest, 
        /// Result: 64-bit -ve.
        /// </summary>
        [NoGC]
        public static void Test_Not_UInt32_Largest_Int64()
        {
            UInt32 a = 4294967295;
            Int64 b = ~a;
            if (b == 0)
            {
                Log.WriteSuccess("Test_Not_UInt32_Largest_Int64 okay.");
            }
            else
            {
                Log.WriteError("Test_Not_UInt32_Largest_Int64 NOT okay.");
            }
        }

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

        /// <summary>
        /// Tests: Not operation using a signed 64-bit value, 
        /// Input: 64-bit Large +ve, 
        /// Result: 64-bit Large -ve.
        /// </summary>
        [NoGC]
        public static void Test_Not_Int64_LargePos_Int64()
        {
            Int64 a = 372036854775807;
            Int64 b = ~a;
            if (b == -372036854775808)
            {
                Log.WriteSuccess("Test_Not_Int64_LargePos_Int64 okay.");
            }
            else
            {
                Log.WriteError("Test_Not_Int64_LargePos_Int64 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Not operation using a signed 32-bit value, 
        /// Input: 32-bit Small +ve, 
        /// Result: 32-bit Small -ve.
        /// </summary>
        [NoGC]
        public static void Test_Not_Int32_SmallPos_Int32()
        {
            Int32 a = 100;
            Int32 b = ~a;
            if (b == -101)
            {
                Log.WriteSuccess("Test_Not_Int32_SmallPos_Int32 okay.");
            }
            else
            {
                Log.WriteError("Test_Not_Int32_SmallPos_Int32 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Not operation using a signed 64-bit value, 
        /// Input: 64-bit Large -ve, 
        /// Result: 64-bit Large +ve.
        /// </summary>
        [NoGC]
        public static void Test_Not_Int64_LargeNeg_Int64()
        {
            Int64 a = -3372036854775807;
            Int64 b = -a;
            if (b == 3372036854775807)
            {
                Log.WriteSuccess("Test_Not_Int64_LargeNeg_Int64 okay.");
            }
            else
            {
                Log.WriteError("Test_Not_Int64_LargeNeg_Int64 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Not operation using an unsigned 32-bit value, 
        /// Input: 32-bit Small, 
        /// Result: 32-bit Small +ve as a 64-bit value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// First the not operation is applied to the 32-bit value then it is expanded to 64-bit by padding the high 32 bits with 0s.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Not_UInt32_Small_Int64()
        {
            UInt32 a = 1;
            Int64 b = ~a;
            if (b == 4294967294)
            {
                Log.WriteSuccess("Test_Not_UInt32_Small_Int64 okay.");
            }
            else
            {
                Log.WriteError("Test_Not_UInt32_Small_Int64 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Not operation using a signed 32-bit value, 
        /// Input: 32-bit Large +ve, 
        /// Result: 32-bit Large +ve as a 64-bit value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// First the not operation is applied to the 32-bit value then it is expanded to 64-bit by padding the high 32 bits with 1s.
        /// In this case it is padded with 1s because not(a)'s highest bit is set to 1, therefore C# expands the value to 64-bit according to the
        /// sign of the not-ed value. I.e.: not(+ve) is padded with 1s, while not(-ve) is padded with 0s.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Not_Int32_LargePos_Int64()
        {
            Int32 a = 1000000000;
            Int64 b = ~a;
            if (b == -1000000001)
            {
                Log.WriteSuccess("Test_Not_Int32_LargePos_Int64 okay.");
            }
            else
            {
                Log.WriteError("Test_Not_Int32_LargePos_Int64 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Not operation using a signed 32-bit value, 
        /// Input: 32-bit Large -ve, 
        /// Result: 32-bit Large +ve as a 64-bit value.
        /// </summary>
        /// /// <remarks>
        /// <para>
        /// First the not operation is applied to the 32-bit value then it is expanded to 64-bit by padding the high 32 bits with 0s.
        /// In this case it is padded with 0s because not(a)'s highest bit is set to 0, therefore C# expands the value to 64-bit according to the
        /// sign of the not-ed value. I.e.: not(+ve) is padded with 1s, while not(-ve) is padded with 0s.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Not_Int32_LargeNeg_Int64()
        {
            Int32 a = -1000000000;
            Int64 b = ~a;
            if (b == 999999999)
            {
                Log.WriteSuccess("Test_Not_Int32_LargeNeg_Int64 okay.");
            }
            else
            {
                Log.WriteError("Test_Not_Int32_LargeNeg_Int64 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Not operation using an unsigned 64-bit value, 
        /// Input: 64-bit Largest, 
        /// Result: 64-bit Smallest.
        /// </summary>
        [NoGC]
        public static void Test_Not_UInt64_Largest_UInt64()
        {
            UInt64 a = 0xFFFFFFFFFFFFFFFF;
            UInt64 b = ~a;
            if (b == 0)
            {
                Log.WriteSuccess("Test_Not_UInt64_Largest_UInt64 okay.");
            }
            else
            {
                Log.WriteError("Test_Not_UInt64_Largest_UInt64 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Not operation using an unsigned 64-bit value, 
        /// Input: 64-bit Smallest, 
        /// Result: 64-bit Largest.
        /// </summary>
        [NoGC]
        public static void Test_Not_UInt64_Smallest_UInt64()
        {
            UInt64 a = 0;
            UInt64 b = ~a;
            if (b == 18446744073709551615)
            {
                Log.WriteSuccess("Test_Not_UInt64_Smallest_UInt64 okay.");
            }
            else
            {
                Log.WriteError("Test_Not_UInt64_Smallest_UInt64 NOT okay.");
            }
        }

        #endregion

        #region 8. Arrays

        /// <summary>
        /// Tests: Array declaration using signed 32-bit elements, 
        /// Input: An array with four elements, 
        /// Result: Correct values for each element.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS does allow array declaration of the form: 
        /// int[] array = new int[4] {5, 10, 15, 20} or 
        /// int[] array = new int[] {5, 10, 15, 20}. 
        /// Array elements must be explicitly declared as in this test case. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Array_Int32()
        {
            Int32[] array = new Int32[4];
            array[0] = 5;
            array[1] = -10;
            array[2] = -15;
            array[3] = 20;
            Int32 a = array.Length;
            if (a == 4)
            {
                Log.WriteSuccess("Test_Array_Length_Int32 okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Length_Int32 NOT okay.");
            }
            if (array[0] == 5)
            {
                Log.WriteSuccess("Test_Array_Decl_Int32[0] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Int32[0] Not okay.");
            }

            if (array[1] == -10)
            {
                Log.WriteSuccess("Test_Array_Decl_Int32[1] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Int32[1] Not okay");
            }

            if (array[2] == -15)
            {
                Log.WriteSuccess("Test_Array_Decl_Int32[2] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Int32[2] Not okay");
            }

            if (array[3] == 20)
            {
                Log.WriteSuccess("Test_Array_Decl_Int32[3] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Int32[3] Not okay");
            }
        }

        /// <summary>
        /// Tests: Array declaration using signed 64-bit elements, 
        /// Input: An array with four elements, 
        /// Result: Correct values for each element.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS does allow array declaration of the form: 
        /// int[] array = new int[4] {5, 10, 15, 20} or 
        /// int[] array = new int[] {5, 10, 15, 20}. 
        /// Array elements must be explicitly declared as in this test case. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Array_Int64()
        {
            Int64[] array = new Int64[4];
            array[0] = 4611686018427387903;
            array[1] = -4611686018427387905;
            array[2] = -15;
            array[3] = 20;
            Int32 a = array.Length;
            if (a == 4)
            {
                Log.WriteSuccess("Test_Array_Length_Int64 okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Length_Int64 NOT okay.");
            }
            if (array[0] == 4611686018427387903)
            {
                Log.WriteSuccess("Test_Array_Decl_Int64[0] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Int64[0] Not okay.");
            }

            if (array[1] == -4611686018427387905)
            {
                Log.WriteSuccess("Test_Array_Decl_Int64[1] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Int64[1] Not okay");
            }

            if (array[2] == -15)
            {
                Log.WriteSuccess("Test_Array_Decl_Int64[2] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Int64[2] Not okay");
            }

            if (array[3] == 20)
            {
                Log.WriteSuccess("Test_Array_Decl_Int64[3] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Int64[3] Not okay");
            }
        }

        /// <summary>
        /// Tests: Array declaration using unsigned 64-bit elements, 
        /// Input: An array with four elements, 
        /// Result: Correct values for each element.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS does allow array declaration of the form: 
        /// int[] array = new int[4] {5, 10, 15, 20} or 
        /// int[] array = new int[] {5, 10, 15, 20}. 
        /// Array elements must be explicitly declared as in this test case. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Array_UInt64()
        {
            UInt64[] array = new UInt64[4];
            array[0] = 4611686018427387903;
            array[1] = 18446744073709551615;
            array[2] = 0;
            array[3] = 20;
            Int32 a = array.Length;
            if (a == 4)
            {
                Log.WriteSuccess("Test_Array_Length_UInt64 okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Length_UInt64 NOT okay.");
            }
            if (array[0] == 4611686018427387903)
            {
                Log.WriteSuccess("Test_Array_Decl_UInt64[0] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_UInt64[0] Not okay.");
            }

            if (array[1] == 18446744073709551615)
            {
                Log.WriteSuccess("Test_Array_Decl_UInt64[1] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_UInt64[1] Not okay");
            }

            if (array[2] == 0)
            {
                Log.WriteSuccess("Test_Array_Decl_UInt64[2] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_UInt64[2] Not okay");
            }

            if (array[3] == 20)
            {
                Log.WriteSuccess("Test_Array_Decl_UInt64[3] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_UInt64[3] Not okay");
            }
        }

        /// <summary>
        /// Tests: Array declaration using usigned 32-bit elements, 
        /// Input: An array with four elements, 
        /// Result: Correct values for each element.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS does allow array declaration of the form: 
        /// int[] array = new int[4] {5, 10, 15, 20} or 
        /// int[] array = new int[] {5, 10, 15, 20}. 
        /// Array elements must be explicitly declared as in this test case. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Array_UInt32()
        {
            UInt32[] array = new UInt32[4];
            array[0] = 4294967295;
            array[1] = 4294967294;
            array[2] = 0;
            array[3] = 20;
            Int32 a = array.Length;
            if (a == 4)
            {
                Log.WriteSuccess("Test_Array_Length_UInt32 okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Length_UInt32 NOT okay.");
            }
            if (array[0] == 4294967295)
            {
                Log.WriteSuccess("Test_Array_Decl_UInt32[0] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_UInt32[0] Not okay.");
            }

            if (array[1] == 4294967294)
            {
                Log.WriteSuccess("Test_Array_Decl_UInt32[1] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_UInt32[1] Not okay");
            }

            if (array[2] == 0)
            {
                Log.WriteSuccess("Test_Array_Decl_UInt32[2] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_UInt32[2] Not okay");
            }

            if (array[3] == 20)
            {
                Log.WriteSuccess("Test_Array_Decl_UInt32[3] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_UInt32[3] Not okay");
            }
        }

        /// <summary>
        /// Tests: Array declaration using strings as elements, 
        /// Input: An array with four elements, 
        /// Result: Correct values for each element.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS does allow array declaration of the form: 
        /// int[] array = new int[4] {5, 10, 15, 20} or 
        /// int[] array = new int[] {5, 10, 15, 20}. 
        /// Array elements must be explicitly declared as in this test case. 
        /// To declare an array of strings, we need to use the FlingOS built-in string type, NOT just string because that is part of .NET.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Array_String()
        {
            FlingOops.String[] array = new FlingOops.String[4];
            array[0] = "elementZero";
            array[1] = "elementOne";
            array[2] = "elementTwo";
            array[3] = "elementThree";
            Int32 a = array.Length;
            if (a == 4)
            {
                Log.WriteSuccess("Test_Array_Length_String okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Length_String NOT okay.");
            }
            if (array[0] == "elementZero")
            {
                Log.WriteSuccess("Test_Array_Decl_String[0] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_String[0] Not okay.");
            }

            if (array[1] == "elementOne")
            {
                Log.WriteSuccess("Test_Array_Decl_String[1] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_String[1] Not okay");
            }

            if (array[2] == "elementTwo")
            {
                Log.WriteSuccess("Test_Array_Decl_String[2] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_String[2] Not okay");
            }

            if (array[3] == "elementThree")
            {
                Log.WriteSuccess("Test_Array_Decl_String[3] okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_String[3] Not okay");
            }
        }

        /// <summary>
        /// Tests: Array declaration using structs as elements, 
        /// Input: An array with four elements, 
        /// Result: Correct values for each element.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS does allow array declaration of the form: 
        /// int[] array = new int[4] {5, 10, 15, 20} or 
        /// int[] array = new int[] {5, 10, 15, 20}. 
        /// Array elements must be explicitly declared as in this test case. 
        /// To declare an array of strings, we need to use the FlingOS built-in string type, NOT just string because that is part of .NET.
        /// </para>
        /// </remarks>
        [NoGC]
        public static unsafe void Test_Array_Struct()
        {
            AStruct[] array = new AStruct[3];
            array[0].a = 1;
            array[0].b = 2;
            array[0].c = 4;
            array[0].d = 8;
            array[1].a = 10;
            array[1].b = 20;
            array[1].c = 40;
            array[1].d = 80;
            array[2].a = 100;
            array[2].b = 200;
            array[2].c = 400;
            array[2].d = 800;
            Int32 a = array.Length;
            if (a == 3)
            {
                Log.WriteSuccess("Test_Array_Length_Struct okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Length_Struct NOT okay.");
            }
            if (array[0].a == 1)
            {
                Log.WriteSuccess("Test_Array_Decl_Struct[0].a okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Struct[0].a Not okay.");
            }

            if (array[0].b == 2)
            {
                Log.WriteSuccess("Test_Array_Decl_Struct[0].b okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Struct[0].b Not okay");
            }

            if (array[0].c == 4)
            {
                Log.WriteSuccess("Test_Array_Decl_Struct[0].c okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Struct[0].c Not okay");
            }

            if (array[0].d == 8)
            {
                Log.WriteSuccess("Test_Array_Decl_Struct[0].d okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Struct[0].d Not okay");
            }
            if (array[1].a == 10)
            {
                Log.WriteSuccess("Test_Array_Decl_Struct[1].a okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Struct[1].a Not okay.");
            }

            if (array[1].b == 20)
            {
                Log.WriteSuccess("Test_Array_Decl_Struct[1].b okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Struct[1].b Not okay");
            }

            if (array[1].c == 40)
            {
                Log.WriteSuccess("Test_Array_Decl_Struct[1].c okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Struct[1].c Not okay");
            }

            if (array[1].d == 80)
            {
                Log.WriteSuccess("Test_Array_Decl_Struct[1].d okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Struct[1].d Not okay");
            }
            if (array[2].a == 100)
            {
                Log.WriteSuccess("Test_Array_Decl_Struct[2].a okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Struct[2].a Not okay.");
            }

            if (array[2].b == 200)
            {
                Log.WriteSuccess("Test_Array_Decl_Struct[2].b okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Struct[2].b Not okay");
            }

            if (array[2].c == 400)
            {
                Log.WriteSuccess("Test_Array_Decl_Struct[2].c okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Struct[2].c Not okay");
            }

            if (array[2].d == 800)
            {
                Log.WriteSuccess("Test_Array_Decl_Struct[2].d okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Decl_Struct[2].d Not okay");
            }
        }

        /// <summary>
        /// Tests: Array declaration using objects as elements, 
        /// Input: An array with two elements, 
        /// Result: Correct values for each element.
        /// </summary>
        /// <remarks>
        /// <para>
        /// FlingOS does allow array declaration of the form: 
        /// int[] array = new int[4] {5, 10, 15, 20} or 
        /// int[] array = new int[] {5, 10, 15, 20}. 
        /// Array elements must be explicitly declared as in this test case. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Array_Object()
        {
            TestClass[] aClass = new TestClass[2];
            Int32 a = aClass.Length;
            if (a == 2)
            {
                Log.WriteSuccess("Test_Array_Length_Object okay.");
            }
            else
            {
                Log.WriteError("Test_Array_Length_Object NOT okay.");
            }

            // aClass[0]
            aClass[0] = new TestClass();
            aClass[0].aField0 = -1111111111;
            aClass[0].aField1 = -1222222222;
            aClass[0].aField2 = -1333333333;
            aClass[0].aField3 = -1444444444;
            aClass[0].aField4 = 1111111111;
            aClass[0].aField5 = 1222222222;
            aClass[0].aField6 = 1333333333;
            aClass[0].aField7 = 1444444444;

            int fld0 = aClass[0].aField0;
            if (fld0 != -1111111111)
            {
                Log.WriteError("Class[0] field0 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[0] field0 right.");
            }
            int fld1 = aClass[0].aField1;
            if (fld1 != -1222222222)
            {
                Log.WriteError("Class[0] field1 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[0] field1 right.");
            }
            int fld2 = aClass[0].aField2;
            if (fld2 != -1333333333)
            {
                Log.WriteError("Class[0] field2 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[0] field2 right.");
            }
            int fld3 = aClass[0].aField3;
            if (fld3 != -1444444444)
            {
                Log.WriteError("Class[0] field3 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[0] field3 right.");
            }
            int fld4 = aClass[0].aField4;
            if (fld4 != 1111111111)
            {
                Log.WriteError("Class[0] field4 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[0] field4 right.");
            }
            int fld5 = aClass[0].aField5;
            if (fld5 != 1222222222)
            {
                Log.WriteError("Class[0] field5 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[0] field5 right.");
            }
            int fld6 = aClass[0].aField6;
            if (fld6 != 1333333333)
            {
                Log.WriteError("Class[0] field6 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[0] field6 right.");
            }
            int fld7 = aClass[0].aField7;
            if (fld7 != 1444444444)
            {
                Log.WriteError("Class[0] field7 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[0] field7 right.");
            }
            int arg = 1431655765;
            int arg1 = aClass[0].aMethodInt(arg);
            if (arg1 != -1)
            {
                Log.WriteError("Class[0] method int wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[0] method int right.");
            }
            aClass[0].aMethodVoid();
            int arg2 = aClass[0].aMethodField(arg);
            if (arg2 != 987211321)
            {
                Log.WriteError("Class[0] method field wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[0] method field right.");
            }

            //aClass[1]
            aClass[1] = new TestClass();
            aClass[1].aField0 = -1234567890;
            aClass[1].aField1 = -1987654321;
            aClass[1].aField2 = -1928374650;
            aClass[1].aField3 = -1657483920;
            aClass[1].aField4 = 1234567890;
            aClass[1].aField5 = 1987654321;
            aClass[1].aField6 = 1928374650;
            aClass[1].aField7 = 1657483920;

            fld0 = aClass[1].aField0;
            if (fld0 != -1234567890)
            {
                Log.WriteError("Class[1] field0 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[1] field0 right.");
            }
            fld1 = aClass[1].aField1;
            if (fld1 != -1987654321)
            {
                Log.WriteError("Class[1] field1 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[1] field1 right.");
            }
            fld2 = aClass[1].aField2;
            if (fld2 != -1928374650)
            {
                Log.WriteError("Class[1] field2 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[1] field2 right.");
            }
            fld3 = aClass[1].aField3;
            if (fld3 != -1657483920)
            {
                Log.WriteError("Class[1] field3 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[1] field3 right.");
            }
            fld4 = aClass[1].aField4;
            if (fld4 != 1234567890)
            {
                Log.WriteError("Class[1] field4 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[1] field4 right.");
            }
            fld5 = aClass[1].aField5;
            if (fld5 != 1987654321)
            {
                Log.WriteError("Class[1] field5 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[1] field5 right.");
            }
            fld6 = aClass[1].aField6;
            if (fld6 != 1928374650)
            {
                Log.WriteError("Class[1] field6 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[1] field6 right.");
            }
            fld7 = aClass[1].aField7;
            if (fld7 != 1657483920)
            {
                Log.WriteError("Class[1] field7 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[1] field7 right.");
            }

            arg = 223344556;
            arg1 = aClass[1].aMethodInt(arg);
            if (arg1 != 670033668)
            {
                Log.WriteError("Class[1] method int wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[1] method int right.");
            }
            aClass[1].aMethodVoid();
            arg2 = aClass[1].aMethodField(arg);
            if (arg2 != -337865736)
            {
                Log.WriteError("Class[1] method field wrong.");
            }
            else
            {
                Log.WriteSuccess("Class[1] method field right.");
            }
        }

        #endregion

        #region 9. Strings

        /// <summary>
        /// Tests: String operations, 
        /// Inputs: Character strings, 
        /// Result: Strings correctly stored and displayed.
        /// </summary>
        /// <remarks> 
        /// <para>
        /// In testing kernel, strings must be declared as FlingOops.String to use the built-in string type of FlingOS. 
        /// Integer value is displayed as a hexadecimal in console.
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Strings()
        {
            Int32 a = 5;
            Log.WriteLine("Test Console write line!");
            Log.WriteLine(" ");
            FlingOops.String ATestString = "Hello, world!";
            Log.WriteLine("Display stored string ATestString:");
            Log.WriteLine(ATestString);
            Log.WriteLine(" ");
            if (ATestString != "Hello, world!")
            {
                Log.WriteError("String equality does not work!");
            }
            else
            {
                Log.WriteSuccess("String equality works.");
            }
            ATestString += " But wait! There's more...";
            Log.WriteLine(" ");
            Log.WriteLine("Concatenate to ATestString:");
            Log.WriteLine(ATestString);
            Log.WriteLine(" ");
            ATestString += " We can even append numbers: " + (FlingOops.String)a;
            Log.WriteLine("Concatenate value stored in variable to ATestString:");
            Log.WriteLine(ATestString);
        }

        #endregion

        #region 10. Argument

        /// <summary>
        /// Tests: Passing signed 32-bit integer argument to method, 
        /// Input: Small, 
        /// Result: Argument correctly passed to method.
        /// </summary>
        [NoGC]
        public static void Test_Arg_Int32(Int32 a)
        {
            if (a == 6)
            {
                Log.WriteSuccess("Test_Arg_Int32 okay.");
            }
            else
            {
                Log.WriteError("Test_Arg_Int32 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Passing signed 64-bit integer argument to method, 
        /// Input: Large, 
        /// Result: Argument correctly passed to method.
        /// </summary>
        [NoGC]
        public static void Test_Arg_Int64(Int64 a)
        {
            if (a == 1441151880758558720)
            {
                Log.WriteSuccess("Test_Arg_Int64 okay.");
            }
            else
            {
                Log.WriteError("Test_Arg_Int64 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Passing unsigned 32-bit integer argument to method, 
        /// Input: Small, 
        /// Result: Argument correctly passed to method.
        /// </summary>
        [NoGC]
        public static void Test_Arg_UInt32(UInt32 a)
        {
            if (a == 100)
            {
                Log.WriteSuccess("Test_Arg_UInt32 okay.");
            }
            else
            {
                Log.WriteError("Test_Arg_UInt32 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Passing unsigned 64-bit integer argument to method, 
        /// Input: Large, 
        /// Result: Argument correctly passed to method.
        /// </summary>
        [NoGC]
        public static void Test_Arg_UInt64(UInt64 a)
        {
            if (a == 10223372036854775807)
            {
                Log.WriteSuccess("Test_Arg_UInt64 okay.");
            }
            else
            {
                Log.WriteError("Test_Arg_UInt64 NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Passing string argument to method, 
        /// Input: A string, 
        /// Result: Argument correctly passed to method.
        /// </summary>
        [NoGC]
        public static void Test_Arg_String(FlingOops.String a)
        {
            if (a == "I am a string")
            {
                Log.WriteSuccess("Test_Arg_String okay.");
            }
            else
            {
                Log.WriteError("Test_Arg_String NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Passing struct argument to method, 
        /// Input: A struct, 
        /// Result: Argument correctly passed to method.
        /// </summary>
        [NoGC]
        public static void Test_Arg_Struct(AStruct struc)
        {
            if ((struc.a == 1) && (struc.b == 2) && (struc.c == 3) && (struc.d == 4))
            {
                Log.WriteSuccess("Test_Arg_Struct okay.");
            }
            else
            {
                Log.WriteError("Test_Arg_Struct NOT okay.");
            }
        }

        /// <summary>
        /// Tests: Passing multiple arguments to method, 
        /// Input: Six arguments, 
        /// Result: Argument correctly passed to method.
        /// </summary>
        [NoGC]
        public static void Test_Arg_Param(Int32 sign32, Int64 sign64, UInt32 unsign32, UInt64 unsign64, FlingOops.String str, FlingOops.String str2)
        {
            if ((sign32 == 6) && (sign64 == 1441151880758558720) && (unsign32 == 100) && (unsign64 == 10223372036854775807) && (str == "I am a string") && (str2 == "I am a string too"))
            {
                Log.WriteSuccess("Test_Arg_Param okay.");
            }
            else
            {
                Log.WriteError("Test_Arg_Param NOT okay.");
            }
        }

        #endregion

        #region 11. Right shift

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

        #region 12. Left shift

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

        #region 13. Struct

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

        #region 14. Variables and pointers

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

        #region 15. Switch

        /// <summary>
        /// Tests: Switch statement using signed 32-bit integers, 
        /// Inputs: 0, 1, 2, 
        /// Result: Case 0
        /// </summary>
        [NoGC]
        public static void Test_Switch_Int32_Case_0()
        {
            Int32 a = 0;
            Int32 b = 1;
            Int32 c = 2;
            int res = a;
            switch (res)
            {
                case 0:
                    Log.WriteSuccess("Test_Switch_Int32_Case_0 okay.");
                    break;
                case 1:
                    Log.WriteError("Test_Switch_Int32_Case_0 NOT okay.");
                    break;
                case 2:
                    Log.WriteError("Test_Switch_Int32_Case_0 NOT okay.");
                    break;
                default:
                    Log.WriteError("Test_Switch_Int32_Case_0 NOT okay.");
                    break;
            }
        }

        /// <summary>
        /// Tests: Switch statement using signed 32-bit integers, 
        /// Inputs: 0, 1, 2, 
        /// Result: Case 1
        /// </summary>
        [NoGC]
        public static void Test_Switch_Int32_Case_1()
        {
            Int32 a = 0;
            Int32 b = 1;
            Int32 c = 2;
            int res = b;
            switch (res)
            {
                case 0:
                    Log.WriteError("Test_Switch_Int32_Case_1 NOT okay.");
                    break;
                case 1:
                    Log.WriteSuccess("Test_Switch_Int32_Case_1 okay.");
                    break;
                case 2:
                    Log.WriteError("Test_Switch_Int32_Case_1 NOT okay.");
                    break;
                default:
                    Log.WriteError("Test_Switch_Int32_Case_1 NOT okay.");
                    break;
            }
        }

        /// <summary>
        /// Tests: Switch statement using signed 32-bit integers, 
        /// Inputs: 0, 1, 2, 
        /// Result: Case 2
        /// </summary>
        [NoGC]
        public static void Test_Switch_Int32_Case_2()
        {
            Int32 a = 0;
            Int32 b = 1;
            Int32 c = 2;
            int res = c;
            switch (res)
            {
                case 0:
                    Log.WriteError("Test_Switch_Int32_Case_2 NOT okay.");
                    break;
                case 1:
                    Log.WriteError("Test_Switch_Int32_Case_2 NOT okay.");
                    break;
                case 2:
                    Log.WriteSuccess("Test_Switch_Int32_Case_2 okay.");
                    break;
                default:
                    Log.WriteError("Test_Switch_Int32_Case_2 NOT okay.");
                    break;
            }
        }

        /// <summary>
        /// Tests: Switch statement using signed 32-bit integers, 
        /// Inputs: 0, 1, 2, 
        /// Result: Case default
        /// </summary>
        [NoGC]
        public static void Test_Switch_Int32_Case_Default()
        {
            Int32 a = 0;
            Int32 b = 1;
            Int32 c = 2;
            int res = a + b + c;
            switch (res)
            {
                case 0:
                    Log.WriteError("Test_Switch_Int32_Case_Default NOT okay.");
                    break;
                case 1:
                    Log.WriteError("Test_Switch_Int32_Case_Default NOT okay.");
                    break;
                case 2:
                    Log.WriteError("Test_Switch_Int32_Case_Default NOT okay.");
                    break;
                default:
                    Log.WriteSuccess("Test_Switch_Int32_Case_Default okay.");
                    break;
            }
        }

        /// <summary>
        /// Tests: Switch statement using signed 32-bit integers and return statement with no value, 
        /// Inputs: 0, 1, 2, 
        /// Result: Case 0
        /// </summary>
        [NoGC]
        public static void Test_Switch_Int32_Case_0_Ret_NoValue()
        {
            Int32 a = 0;
            Int32 b = 1;
            Int32 c = 2;
            int res = a;
            switch (res)
            {
                case 0:
                    Log.WriteSuccess("Test_Switch_Int32_Case_0_Ret_NoValue okay.");
                    return;
                case 1:
                    Log.WriteError("Test_Switch_Int32_Case_0_Ret_NoValue NOT okay.");
                    return;
                case 2:
                    Log.WriteError("Test_Switch_Int32_Case_0_Ret_NoValue NOT okay.");
                    return;
                default:
                    Log.WriteError("Test_Switch_Int32_Case_0_Ret_NoValue NOT okay.");
                    return;
            }
        }

        /// <summary>
        /// Tests: Switch statement using signed 32-bit integers and return statement with value, 
        /// Inputs: 0, 1, 2, 
        /// Result: Case 0
        /// </summary>
        [NoGC]
        public static int Test_Switch_Int32_Case_0_Ret_IntValue()
        {
            Int32 a = 0;
            Int32 b = 1;
            Int32 c = 2;
            int res = a;
            switch (res)
            {
                case 0:
                    Log.WriteSuccess("Test_Switch_Int32_Case_0_Ret_IntValue okay.");
                    return 0;
                case 1:
                    Log.WriteError("Test_Switch_Int32_Case_0_Ret_IntValue NOT okay.");
                    return 0;
                case 2:
                    Log.WriteError("Test_Switch_Int32_Case_0_Ret_IntValue NOT okay.");
                    return 0;
                default:
                    Log.WriteError("Test_Switch_Int32_Case_0_Ret_IntValue NOT okay.");
                    return 0;
            }
        }

        /// <summary>
        /// Tests: Switch statement using signed 32-bit integers and return statement with value, 
        /// Inputs: 0, 1, 2, 
        /// Result: Case 0
        /// </summary>
        [NoGC]
        public static string Test_Switch_Int32_Case_0_Ret_StringValue()
        {
            Int32 a = 0;
            Int32 b = 1;
            Int32 c = 2;
            int res = a;
            switch (res)
            {
                case 0:
                    Log.WriteSuccess("Test_Switch_Int32_Case_0_Ret_StringValue okay.");
                    return "I shall return";
                case 1:
                    Log.WriteError("Test_Switch_Int32_Case_0_Ret_StringValue NOT okay.");
                    return "I shall return";
                case 2:
                    Log.WriteError("Test_Switch_Int32_Case_0_Ret_StringValue NOT okay.");
                    return "I shall return";
                default:
                    Log.WriteError("Test_Switch_Int32_Case_0_Ret_StringValue NOT okay.");
                    return "I shall return";
            }
        }

        /// <summary>
        /// Tests: Switch statement using strings, 
        /// Inputs: "zero", "one", "two", 
        /// Result: Case 0
        /// </summary>
        /// <remarks>
        /// <para>
        /// Standard strings are allowed to be used in a switch statement but FlingOS does not use the .NET framework to generate strings. 
        /// FlingOS's string type is an object type which is not allowed to be used in a switch statement by C#. 
        /// Therefore in FlingOS, strings cannot be used in a switch statement. 
        /// </para>
        /// </remarks>
        [NoGC]
        public static void Test_Switch_String_Case_0()
        {
            Log.WriteLine("  Test_Switch_String_Case_0() is not allowed, see remarks.");
            return;

            string a = "zero";
            string b = "one";
            string c = "two";
            string res = a;
            switch (res)
            {
                case "zero":
                    Log.WriteSuccess("Test_Switch_String_Case_0 okay.");
                    break;
                case "one":
                    Log.WriteError("Test_Switch_String_Case_0 NOT okay.");
                    break;
                case "two":
                    Log.WriteError("Test_Switch_String_Case_0 NOT okay.");
                    break;
                default:
                    Log.WriteError("Test_Switch_String_Case_0 NOT okay.");
                    break;
            }
        }

        #endregion

        #region 16. Heaps

        /// <summary>
        /// Tests: Heap management, 
        /// Inputs:A struct, 
        /// Result: Struct allocated on heap correctly. 
        /// </summary>
        [NoGC]
        public static unsafe void Test_Heap()
        {
            AStruct* HeapInst = (AStruct*)Heap.AllocZeroed((uint)sizeof(AStruct), "FlingOops:Test_Heap");
            if (HeapInst == null)
            {
                Log.WriteError("HeapInst null.");
            }
            else
            {
                Log.WriteSuccess("HeapInst not null.");
            }
            HeapInst->a = 85;
            HeapInst->b = 30720;
            HeapInst->c = 806092800;
            HeapInst->d = 1085086035219578880;
            if (HeapInst->a == 85)
            {
               Log.WriteSuccess("HeapInst->a not null.");
            }
            else
            {
                Log.WriteError("HeapInst->a null.");
            }
            if (HeapInst->b == 30720)
            {
                Log.WriteSuccess("HeapInst->b not null.");
            }
            else
            {
                Log.WriteError("HeapInst->b null.");
            }
            if (HeapInst->c == 806092800)
            {
                Log.WriteSuccess("HeapInst->c not null.");
            }
            else
            {
                Log.WriteError("HeapInst->c null.");
            }
            if (HeapInst->d == 1085086035219578880)
            {
                Log.WriteSuccess("HeapInst->d not null");
            }
            else
            {
                Log.WriteError("HeapInst->d null.");
            }
        }

        #endregion

        #region 17. Objects

        /// <summary>
        /// Tests: Objects, 
        /// Inputs: New object, fields, arguments, 
        /// Result: Object created and arguments are returned as expected.
        /// </summary>
        [NoGC]
        public static void Test_Objects()
        {
            TestClass aClass = new TestClass();
            int fld0 = aClass.aField0;
            if (fld0 != 2013265920)
            {
                Log.WriteError("Class field0 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class field0 right.");
            }
            int fld1 = aClass.aField1;
            if (fld1 != 1717567488)
            {
                Log.WriteError("Class field1 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class field1 right.");
            }
            int fld2 = aClass.aField2;
            if (fld2 != -2040528896)
            {
                Log.WriteError("Class field2 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class field2 right.");
            }
            int fld3 = aClass.aField3;
            if (fld3 != -16777216)
            {
                Log.WriteError("Class field3 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class field3 right.");
            }
            int fld4 = aClass.aField4;
            if (fld4 != -16448112)
            {
                Log.WriteError("Class field4 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class field4 right.");
            }
            int fld5 = aClass.aField5;
            if (fld5 != 2133914880)
            {
                Log.WriteError("Class field5 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class field5 right.");
            }
            int fld6 = aClass.aField6;
            if (fld6 != 1879105792)
            {
                Log.WriteError("Class field6 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class field6 right.");
            }
            int fld7 = aClass.aField7;
            if (fld7 != 1894834447)
            {
                Log.WriteError("Class field7 wrong.");
            }
            else
            {
                Log.WriteSuccess("Class field7 right.");
            }
            int arg = 1431655765;
            int arg1 = aClass.aMethodInt(arg);
            if (arg1 != -1)
            {
                Log.WriteError("Class method int wrong.");
            }
            else
            {
                Log.WriteSuccess("Class method int right.");
            }
            aClass.aMethodVoid();
            int arg2 = aClass.aMethodField(arg);
            if (arg2 != 805287168)
            {
                Log.WriteError("Class method field wrong.");
            }
            else
            {
                Log.WriteSuccess("Class method field right.");
            }
        }

        #endregion

        #region 18. Try-Catch-Finally

        /// <summary>
        /// Tests: Testing Try-catch-finally blocks. 
        /// </summary>
        [NoGC]
        public static void Test_TCF_0()
        {
            try
            {
                Log.WriteSuccess("Entered try.");
            }
            catch
            {
                Log.WriteSuccess("Entered catch.");
            }
            finally
            {
                Log.WriteSuccess("Entered finally.");
            }
        }

        /// <summary>
        /// Tests: Testing throwing exceptions within catch section of Try-catch-finally blocks. 
        /// </summary>
        [NoGC]
        public static unsafe void Test_TCF_1()
        {
            try
            {
                Log.WriteSuccess("Entered try.");

                subMethod1();
            }
            catch
            {
                Log.WriteSuccess("Entered catch.");
            }
            finally
            {
                Log.WriteSuccess("Entered finally.");
            }

            Log.WriteSuccess("Executed end of test cleanly.");
        }
        public static void subMethod1()
        {
            try
            {
                Log.WriteSuccess("Entered subMethod1 try.");
                ExceptionMethods.Throw(new Exception("Exception 1"));
            }
            catch
            {
                Log.WriteSuccess("Entered subMethod1 catch.");
                ExceptionMethods.Throw(new Exception("Exception 2"));
            }
            finally
            {
                Log.WriteSuccess("Entered subMethod1 finally.");
            }

            Log.WriteError("Executed end of subMethod1 which shouldn't have!");
        }

        /// <summary>
        /// Tests: Testing Try-catch-finally blocks. 
        /// </summary>
        [NoGC]
        public static unsafe void Test_TCF_2()
        {
            try
            {
                Log.WriteSuccess("Entered try.");

                subMethod2();
            }
            catch
            {
                Log.WriteSuccess("Entered catch.");
            }
            finally
            {
                Log.WriteSuccess("Entered finally.");
            }

            Log.WriteSuccess("Executed end of test cleanly.");
        }
        public static void subMethod2()
        {
            try
            {
                Log.WriteSuccess("Entered subMethod2 try.");
                ExceptionMethods.Throw(new Exception("Exception 1"));
            }
            catch
            {
                Log.WriteSuccess("Entered subMethod2 catch.");
            }
            finally
            {
                Log.WriteSuccess("Entered subMethod2 finally.");
                ExceptionMethods.Throw(new Exception("Exception 2"));
            }

            Log.WriteError("Executed end of subMethod2 which shouldn't have!");
        }
        
        /// <summary>
        /// Tests: Testing Try-catch-finally blocks. 
        /// </summary>
        [NoGC]
        public static unsafe void Test_TCF_3()
        {
            try
            {
                Log.WriteSuccess("Entered try.");

                subMethod3();
            }
            catch
            {
                Log.WriteSuccess("Entered catch.");
            }
            finally
            {
                Log.WriteSuccess("Entered finally.");
            }

            Log.WriteSuccess("Executed end of test cleanly.");
        }
        public static void subMethod3()
        {
            try
            {
                Log.WriteSuccess("Entered subMethod3 try.");
                ExceptionMethods.Throw(new Exception("Exception 1"));
            }
            finally
            {
                Log.WriteSuccess("Entered subMethod3 finally.");
            }

            Log.WriteError("Executed end of subMethod3 which shouldn't have!");
        }

        /// <summary>
        /// Tests: Testing Try-catch-finally blocks. 
        /// </summary>
        [NoGC]
        public static unsafe void Test_TCF_4()
        {
            try
            {
                Log.WriteSuccess("Entered try.");

                subMethod4();
            }
            catch
            {
                Log.WriteError("Entered catch when we shouldn't have any unhandled exceptions.");
            }
            finally
            {
                Log.WriteSuccess("Entered finally.");
            }

            Log.WriteSuccess("Executed end of test cleanly.");
        }
        public static void subMethod4()
        {
            try
            {
                Log.WriteSuccess("Entered subMethod4 try 1.");
                try
                {
                    Log.WriteSuccess("Entered subMethod4 try 2.");

                    ExceptionMethods.Throw(new Exception("Exception 1"));
                }
                finally
                {
                    Log.WriteSuccess("Entered subMethod4 try 2 finally.");
                }
                
                Log.WriteError("Continued execution of subMethod4 try 2!");
            }
            catch
            {
                Log.WriteSuccess("Entered subMethod4 try 1 catch.");
            }

            Log.WriteSuccess("Executed end of subMethod4 correctly.");
        }

        /// <summary>
        /// Tests: Testing Try-catch-finally blocks. 
        /// </summary>
        [NoGC]
        public static unsafe void Test_TCF_5()
        {
            try
            {
                Log.WriteSuccess("Entered try.");

                subMethod5();
            }
            catch
            {
                Log.WriteError("Entered catch when we shouldn't have any unhandled exceptions.");
            }
            finally
            {
                Log.WriteSuccess("Entered finally.");
            }

            Log.WriteSuccess("Executed end of test cleanly.");
        }
        public static void subMethod5()
        {
            try
            {
                Log.WriteSuccess("Entered subMethod5 try 1.");
                try
                {
                    Log.WriteSuccess("Entered subMethod5 try 2.");

                    ExceptionMethods.Throw(new Exception("Exception 1"));
                }
                catch
                {
                    Log.WriteSuccess("Entered subMethod5 try 2 catch.");
                }

                Log.WriteSuccess("Continued execution of subMethod5 try 2.");
            }
            catch
            {
                Log.WriteError("Entered subMethod5 try 1 catch.");
            }

            Log.WriteSuccess("Executed end of subMethod5 correctly.");
        }

        #endregion

        #region Xor

        /// <summary>
        /// Tests: Xor operation using integer operands, 
        /// Inputs:  , 
        /// Result: Correct bit pattern produced by Xor.
        /// </summary>
        [NoGC]
        public static void Test_Xor_Int()
        {
            UInt32 unsign32_a;
            UInt32 unsign32_b;
            UInt64 unsign64_a;
            UInt64 unsign64_b;
            Int32 sign32_a;
            Int32 sign32_b;
            Int64 sign64_a;
            Int64 sign64_b;

            #region 32-32

            Log.WriteLine(" 32-32");
            Log.WriteLine("  Unsigned");
            
            // 0 ^ 0
            unsign32_a = 0;
            unsign32_b = 0;
            unsign32_a = unsign32_a ^ unsign32_b;
            if (unsign32_a == 0)
            {
                Log.WriteSuccess("Test_Xor_UInt32_0_UInt32_0 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt32_0_UInt32_0 not okay.");
            }

            // 0 ^ 1
            unsign32_a = 0;
            unsign32_b = 1;
            unsign32_a = unsign32_a ^ unsign32_b;
            if (unsign32_a == 1)
            {
                Log.WriteSuccess("Test_Xor_UInt32_0_UInt32_1 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt32_0_UInt32_1 not okay.");
            }

            // 1 ^ 0
            unsign32_a = 1;
            unsign32_b = 0;
            unsign32_a = unsign32_a ^ unsign32_b;
            if (unsign32_a == 1)
            {
                Log.WriteSuccess("Test_Xor_UInt32_1_UInt32_0 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt32_1_UInt32_0 not okay.");
            }

            // 1 ^ 1
            unsign32_a = 1;
            unsign32_b = 1;
            unsign32_a = unsign32_a ^ unsign32_b;
            if (unsign32_a == 0)
            {
                Log.WriteSuccess("Test_Xor_UInt32_1_UInt32_1 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt32_1_UInt32_1 not okay.");
            }

            // 0xFFFFFFFF ^ 0
            unsign32_a = 0xFFFFFFFF;
            unsign32_b = 0;
            unsign32_a = unsign32_a ^ unsign32_b;
            if (unsign32_a == 0xFFFFFFFF)
            {
                Log.WriteSuccess("Test_Xor_UInt32_0xFFFFFFFF_UInt32_0 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt32_0xFFFFFFFF_UInt32_0 not okay.");
            }

            // 0xFFFFFFFF ^ 1
            unsign32_a = 0xFFFFFFFF;
            unsign32_b = 1;
            unsign32_a = unsign32_a ^ unsign32_b;
            if (unsign32_a == 0xFFFFFFFE)
            {
                Log.WriteSuccess("Test_Xor_UInt32_0xFFFFFFFF_UInt32_1 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt32_0xFFFFFFFF_UInt32_1 not okay.");
            }

            // 0xFFFFFFFF ^ 0xFFFFFFFF
            unsign32_a = 0xFFFFFFFF;
            unsign32_b = 0xFFFFFFFF;
            unsign32_a = unsign32_a ^ unsign32_b;
            if (unsign32_a == 0)
            {
                Log.WriteSuccess("Test_Xor_UInt32_0xFFFFFFFF_UInt32_0xFFFFFFFF okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt32_0xFFFFFFFF_UInt32_0xFFFFFFFF not okay.");
            }

            // 0xFFFFFFFF ^ 0x44444444
            unsign32_a = 0xFFFFFFFF;
            unsign32_b = 0x44444444;
            unsign32_a = unsign32_a ^ unsign32_b;
            if (unsign32_a == 0xBBBBBBBB)
            {
                Log.WriteSuccess("Test_Xor_UInt32_0xFFFFFFFF_UInt32_0x44444444 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt32_0xFFFFFFFF_UInt32_0x44444444 not okay.");
            }

            // 0xFFFFFFFF ^ 1000
            unsign32_a = 0xFFFFFFFF;
            unsign32_b = 1000;
            unsign32_a = unsign32_a ^ unsign32_b;
            if (unsign32_a == 0xFFFFFC17)
            {
                Log.WriteSuccess("Test_Xor_UInt32_0xFFFFFFFF_UInt32_1000 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt32_0xFFFFFFFF_UInt32_1000 not okay.");
            }

            // 0 ^ 0x44444444
            unsign32_a = 0;
            unsign32_b = 0x44444444;
            unsign32_a = unsign32_a ^ unsign32_b;
            if (unsign32_a == 0x44444444)
            {
                Log.WriteSuccess("Test_Xor_UInt32_0_UInt32_0x44444444 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt32_0_UInt32_0x44444444 not okay.");
            }

            // 0 ^ 1000
            unsign32_a = 0;
            unsign32_b = 1000;
            unsign32_a = unsign32_a ^ unsign32_b;
            if (unsign32_a == 1000)
            {
                Log.WriteSuccess("Test_Xor_UInt32_0_UInt32_1000 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt32_0_UInt32_1000 not okay.");
            }

            // 1000 ^ 0x44444444
            unsign32_a = 1000;
            unsign32_b = 0x44444444;
            unsign32_a = unsign32_a ^ unsign32_b;
            if (unsign32_a == 0x444447AC)
            {
                Log.WriteSuccess("Test_Xor_UInt32_1000_UInt32_0x44444444 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt32_1000_UInt32_0x44444444 not okay.");
            }

            Log.WriteLine("  Signed");

            #endregion

            #region 64-32

            Log.WriteLine(" 64-32");
            Log.WriteLine("  Unsigned");

            // 0 ^ 0
            unsign64_a = 0;
            unsign32_b = 0;
            unsign64_a = unsign64_a ^ unsign32_b;
            if (unsign64_a == 0)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0_UInt32_0 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0_UInt32_0 not okay.");
            }

            // 0 ^ 1
            unsign64_a = 0;
            unsign32_b = 1;
            unsign64_a = unsign64_a ^ unsign32_b;
            if (unsign64_a == 1)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0_UInt32_1 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0_UInt32_1 not okay.");
            }

            // 1 ^ 0
            unsign64_a = 1;
            unsign32_b = 0;
            unsign64_a = unsign64_a ^ unsign32_b;
            if (unsign64_a == 1)
            {
                Log.WriteSuccess("Test_Xor_UInt64_1_UInt32_0 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_1_UInt32_0 not okay.");
            }

            // 1 ^ 1
            unsign64_a = 1;
            unsign32_b = 1;
            unsign64_a = unsign64_a ^ unsign32_b;
            if (unsign64_a == 0)
            {
                Log.WriteSuccess("Test_Xor_UInt64_1_UInt32_1 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_1_UInt32_1 not okay.");
            }

            // 0xFFFFFFFFFFFFFFFF ^ 0
            unsign64_a = 0xFFFFFFFFFFFFFFFF;
            unsign32_b = 0;
            unsign64_a = unsign64_a ^ unsign32_b;
            if (unsign64_a == 0xFFFFFFFFFFFFFFFF)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt32_0 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt32_0 not okay.");
            }

            // 0 ^ 0xFFFFFFFF
            unsign64_a = 0;
            unsign32_b = 0xFFFFFFFF;
            unsign64_a = unsign64_a ^ unsign32_b;
            if (unsign64_a == 0xFFFFFFFF)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0_UInt32_0xFFFFFFFF okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0_UInt32_0xFFFFFFFF not okay.");
            }

            // 0xFFFFFFFFFFFFFFFF ^ 1
            unsign64_a = 0xFFFFFFFFFFFFFFFF;
            unsign32_b = 1;
            unsign64_a = unsign64_a ^ unsign32_b;
            if (unsign64_a == 0xFFFFFFFFFFFFFFFE)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt32_1 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt32_1 not okay.");
            }

            // 1 ^ 0xFFFFFFFF
            unsign64_a = 1;
            unsign32_b = 0xFFFFFFFF;
            unsign64_a = unsign64_a ^ unsign32_b;
            if (unsign64_a == 0xFFFFFFFE)
            {
                Log.WriteSuccess("Test_Xor_UInt64_1_UInt32_0xFFFFFFFF okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_1_UInt32_0xFFFFFFFF not okay.");
            }

            // 0xFFFFFFFFFFFFFFFF ^ 0xFFFFFFFF
            unsign64_a = 0xFFFFFFFFFFFFFFFF;
            unsign32_b = 0xFFFFFFFF;
            unsign64_a = unsign64_a ^ unsign32_b;
            if (unsign64_a == 0xFFFFFFFF00000000)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt32_0xFFFFFFFF okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt32_0xFFFFFFFF not okay.");
            }

            // 0xFFFFFFFFFFFFFFFF ^ 0x44444444
            unsign64_a = 0xFFFFFFFFFFFFFFFF;
            unsign32_b = 0x44444444;
            unsign64_a = unsign64_a ^ unsign32_b;
            if (unsign64_a == 0xFFFFFFFFBBBBBBBB)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt32_0x44444444 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt32_0x44444444 not okay.");
            }

            // 0xFFFFFFFFFFFFFFFF ^ 0x08000800
            unsign64_a = 0xFFFFFFFFFFFFFFFF;
            unsign32_b = 0x08000800;
            unsign64_a = unsign64_a ^ unsign32_b;
            if (unsign64_a == 0xFFFFFFFFF7FFF7FF)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt32_0x08000800 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt32_0x08000800 not okay.");
            }

            // 0 ^ 0x44444444
            unsign64_a = 0;
            unsign32_b = 0x44444444;
            unsign64_a = unsign64_a ^ unsign32_b;
            if (unsign64_a == 0x44444444)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0_UInt32_0x44444444 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0_UInt32_0x44444444 not okay.");
            }

            // 0x4444444444444444 ^ 0
            unsign64_a = 0x4444444444444444;
            unsign32_b = 0;
            unsign64_a = unsign64_a ^ unsign32_b;
            if (unsign64_a == 0x4444444444444444)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0x4444444444444444_UInt32_0 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0x4444444444444444_UInt32_0 not okay.");
            }

            // 0 ^ 0x08000800
            unsign64_a = 0;
            unsign32_b = 0x08000800;
            unsign64_a = unsign64_a ^ unsign32_b;
            if (unsign64_a == 0x08000800)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0_UInt32_0x08000800 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0_UInt32_0x08000800 not okay.");
            }

            // 0x0800080008000800 ^ 0
            unsign64_a = 0x0800080008000800;
            unsign32_b = 0;
            unsign64_a = unsign64_a ^ unsign32_b;
            if (unsign64_a == 0x0800080008000800)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0x0800080008000800_UInt32_0 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0x0800080008000800_UInt32_0 not okay.");
            }

            // 0x0800080008000800 ^ 0x44444444
            unsign64_a = 0x0800080008000800;
            unsign64_b = 0x44444444;
            unsign64_a = unsign64_a ^ unsign64_b;
            if (unsign64_a == 0x080008004C444C44)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0x0800080008000800_UInt32_0x44444444 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0x0800080008000800_UInt32_0x44444444 not okay.");
            }

            Log.WriteLine("  Signed");

            #endregion

            #region 64-64

            Log.WriteLine(" 64-64");
            Log.WriteLine("  Unsigned");
            
            // 0 ^ 0
            unsign64_a = 0;
            unsign64_b = 0;
            unsign64_a = unsign64_a ^ unsign64_b;
            if (unsign64_a == 0)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0_UInt64_0 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0_UInt64_0 not okay.");
            }

            // 0 ^ 1
            unsign64_a = 0;
            unsign64_b = 1;
            unsign64_a = unsign64_a ^ unsign64_b;
            if (unsign64_a == 1)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0_UInt64_1 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0_UInt64_1 not okay.");
            }

            // 1 ^ 0
            unsign64_a = 1;
            unsign64_b = 0;
            unsign64_a = unsign64_a ^ unsign64_b;
            if (unsign64_a == 1)
            {
                Log.WriteSuccess("Test_Xor_UInt64_1_UInt64_0 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_1_UInt64_0 not okay.");
            }

            // 1 ^ 1
            unsign64_a = 1;
            unsign64_b = 1;
            unsign64_a = unsign64_a ^ unsign64_b;
            if (unsign64_a == 0)
            {
                Log.WriteSuccess("Test_Xor_UInt64_1_UInt64_1 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_1_UInt64_1 not okay.");
            }

            // 0xFFFFFFFFFFFFFFFF ^ 0
            unsign64_a = 0xFFFFFFFFFFFFFFFF;
            unsign64_b = 0;
            unsign64_a = unsign64_a ^ unsign64_b;
            if (unsign64_a == 0xFFFFFFFFFFFFFFFF)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt64_0 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt64_0 not okay.");
            }

            // 0xFFFFFFFFFFFFFFFF ^ 1
            unsign64_a = 0xFFFFFFFFFFFFFFFF;
            unsign64_b = 1;
            unsign64_a = unsign64_a ^ unsign64_b;
            if (unsign64_a == 0xFFFFFFFFFFFFFFFE)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt64_1 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt64_1 not okay.");
            }

            // 0xFFFFFFFFFFFFFFFF ^ 0xFFFFFFFFFFFFFFFF
            unsign64_a = 0xFFFFFFFFFFFFFFFF;
            unsign64_b = 0xFFFFFFFFFFFFFFFF;
            unsign64_a = unsign64_a ^ unsign64_b;
            if (unsign64_a == 0)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt64_0xFFFFFFFFFFFFFFFF okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt64_0xFFFFFFFFFFFFFFFF not okay.");
            }

            // 0xFFFFFFFFFFFFFFFF ^ 0x4444444444444444
            unsign64_a = 0xFFFFFFFFFFFFFFFF;
            unsign64_b = 0x4444444444444444;
            unsign64_a = unsign64_a ^ unsign64_b;
            if (unsign64_a == 0xBBBBBBBBBBBBBBBB)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt64_0x4444444444444444 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt64_0x4444444444444444 not okay.");
            }

            // 0xFFFFFFFFFFFFFFFF ^ 0x0800080008000800
            unsign64_a = 0xFFFFFFFFFFFFFFFF;
            unsign64_b = 0x0800080008000800;
            unsign64_a = unsign64_a ^ unsign64_b;
            if (unsign64_a == 0xF7FFF7FFF7FFF7FF)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt64_0x0800080008000800 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0xFFFFFFFFFFFFFFFF_UInt64_0x0800080008000800 not okay.");
            }

            // 0 ^ 0x4444444444444444
            unsign64_a = 0;
            unsign64_b = 0x4444444444444444;
            unsign64_a = unsign64_a ^ unsign64_b;
            if (unsign64_a == 0x4444444444444444)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0_UInt64_0x4444444444444444 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0_UInt64_0x4444444444444444 not okay.");
            }

            // 0 ^ 0x0800080008000800
            unsign64_a = 0;
            unsign64_b = 0x0800080008000800;
            unsign64_a = unsign64_a ^ unsign64_b;
            if (unsign64_a == 0x0800080008000800)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0_UInt64_0x0800080008000800 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0_UInt64_0x0800080008000800 not okay.");
            }

            // 0x0800080008000800 ^ 0x4444444444444444
            unsign64_a = 0x0800080008000800;
            unsign64_b = 0x4444444444444444;
            unsign64_a = unsign64_a ^ unsign64_b;
            if (unsign64_a == 0x4C444C444C444C44)
            {
                Log.WriteSuccess("Test_Xor_UInt64_0x0800080008000800_UInt64_0x4444444444444444 okay.");
            }
            else
            {
                Log.WriteError("Test_Xor_UInt64_0x0800080008000800_UInt64_0x4444444444444444 not okay.");
            }

            Log.WriteLine("  Signed");

            #endregion

        }

        #endregion

    }
}

/// <summary>
/// Test class for testing objects.
/// </summary>
/// <remarks>
/// <para>
/// Class must inherit from the FlingOS object type using FlingOops.Object.
/// </para>
/// </remarks>
public class TestClass : FlingOops.Object
{
    public int aField0 = 2013265920;
    public int aField1 = 1717567488;
    public int aField2 = -2040528896;
    public int aField3 = -16777216;
    public int aField4 = -16448112;
    public int aField5 = 2133914880;
    public int aField6 = 1879105792;
    public int aField7 = 1894834447;
    
    public int aMethodInt(int arg)
    {
        return arg * 3;
    }

    public void aMethodVoid()
    {
        Log.WriteSuccess("Class method void right.");
    }

    public int aMethodField(int arg)
    {
        return arg * aField6;
    }
}