using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Debug
{
    /// <summary>
    /// The basic kernel debugger code.
    /// </summary>
    /// <remarks>
    /// This is entirely made from plugged methods so that even if the 
    /// kernel compiler is broken, the debugger will still work.
    /// </remarks>
    [Compiler.PluggedClass]
    public static class BasicDebug
    {
        /// <summary>
        /// Initialises the basic debugger
        /// </summary>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static void Init()
        {
            BasicConsole.Write("Attempting to init serial port for debug connection...");
            InitSerial();
            BasicConsole.WriteLine("initialised.");
            BasicConsole.WriteLine("Waiting for debug connection signature...");
            uint connectionSignature = Serial_SafeReadUInt32();
            if (connectionSignature != 0)
            {
                BasicConsole.Write("Debug signature received. Enabling debugging...");
                BeginEnableDebug();
                while (connectionSignature != 0)
                {
                    connectionSignature = Serial_SafeReadUInt32();
                }
                EndEnableDebug();
                BasicConsole.WriteLine("enabled.");

                BasicConsole.DisableDelayOutput = true;
            }
            else
            {
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine("Debug signature not received within time limit. Debugging disabled.");
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }
        }
        /// <summary>
        /// Initialises COM1 as serial connection to debug over
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath=@"..\..\ASM\Debug\InitSerial")]
        private static void InitSerial()
        {
        }
        /// <summary>
        /// Begins enabling the debug interrupt handler
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = @"..\..\ASM\Debug\EnableDebug")]
        private static void BeginEnableDebug()
        {
        }
        /// <summary>
        /// Ends enabling the debug interrupt handler
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static void EndEnableDebug()
        {
        }

        /// <summary>
        /// Invokes interrupt 3.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = @"ASM\Debug\Break")]
        public static void Int3()
        {
        }

        /// <summary>
        /// The main execute method for the basic debugger
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath=@"..\..\ASM\Debug\Execute")]
        private static void Execute()
        {
        }

        /// <summary>
        /// Inserts the debug commands into the ASM.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = @"..\..\ASM\Debug\Commands")]
        private static void InsertCommandsList()
        {

        }

        /// <summary>
        /// Sends the Break command.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = @"..\..\ASM\Debug\SendCommands")]
        private static void SendBreakCmd()
        {
        }
        /// <summary>
        /// Sends the address of the last instruction that executed when the 
        /// break occurred.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static void SendBreakAddress()
        {
        }
        /// <summary>
        /// Sends the register values as they were before the interrupt.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static void SendRegisters()
        {
        }
        /// <summary>
        /// Sends the arguments values as they were before the interrupt.
        /// Requires the debugger to send it how many bytes for 
        /// the arguments there are.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static void SendArguments()
        {
        }
        /// <summary>
        /// Sends the locals values as they were before the interrupt.
        /// Requires the debugger to send it how many bytes for 
        /// the arguments there are.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        private static void SendLocals()
        {
        }

        /// <summary>
        /// Waits for a command from the debugger.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = @"..\..\ASM\Debug\ReceiveCommands")]
        private static void WaitForCommand()
        {
        }

        /// <summary>
        /// Writes the specified value to the debug serial port. Not callable from C#.
        /// </summary>
        /// <param name="value">The value to write.</param>
        [Compiler.PluggedMethod(ASMFilePath = @"..\..\ASM\Debug\SerialWrite")]
        public static void Serial_WriteByte(byte value)
        {
        }
        /// <summary>
        /// Writes the specified value to the debug serial port. Not callable from C#.
        /// </summary>
        /// <param name="value">The value to write.</param>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static void Serial_WriteUInt16(UInt16 value)
        {
        }
        /// <summary>
        /// Writes the specified value to the debug serial port. Not callable from C#.
        /// </summary>
        /// <param name="value">The value to write.</param>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static void Serial_WriteUInt32(UInt32 value)
        {
        }
        /// <summary>
        /// Writes the specified value to the debug serial port. Not callable from C#.
        /// </summary>
        /// <param name="value">The value to write.</param>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static void Serial_WriteString(string value)
        {
        }

        /// <summary>
        /// Reads a byte from the debug serial port. Not callable from C#.
        /// </summary>
        /// <returns>The byte read.</returns>
        [Compiler.PluggedMethod(ASMFilePath = @"..\..\ASM\Debug\SerialRead")]
        public static byte Serial_ReadByte()
        {
            //To keep the C# compiler happy
            return 0;
        }
        /// <summary>
        /// Reads a UInt16 from the debug serial port. Not callable from C#.
        /// </summary>
        /// <returns>The UInt16 read.</returns>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static UInt16 Serial_ReadUInt16()
        {
            //To keep the C# compiler happy
            return 0;
        }
        /// <summary>
        /// Reads a UInt32 from the debug serial port. Not callable from C#.
        /// </summary>
        /// <returns>The UInt32 read.</returns>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static UInt32 Serial_ReadUInt32()
        {
            //To keep the C# compiler happy
            return 0;
        }

        /// <summary>
        /// Safely reads a UInt32 from the debug serial port
        /// </summary>
        /// <returns>The UInt32 read.</returns>
        [Compiler.PluggedMethod(ASMFilePath = null)]
        public static UInt32 Serial_SafeReadUInt32()
        {
            //To keep the C# compiler happy
            return 0;
        }
        
        ///// <summary>
        ///// Clears the screen
        ///// </summary>
        //[Compiler.NoDebug]
        //[Compiler.NoGC]
        //public static void ClearMessage()
        //{
        //    PreReqs.WriteDebugVideo(" ", 0x00);
        //}
        ///// <summary>
        ///// Displays the specified message to the screen.
        ///// </summary>
        ///// <param name="message">The message to display.</param>
        //[Compiler.NoDebug]
        //[Compiler.NoGC]
        //public static void DisplayMessage(string message)
        //{
        //    PreReqs.WriteDebugVideo(message, 0x02);
        //}
        
        /// <summary>
        /// Inserts the plug for the Int1 and Int3 interrupt handler.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath = @"..\..\ASM\Debug\InterruptHandler")]
        private static void InterruptHandler()
        {
        }
    }
}
