using System;
using Kernel.Hardware.Processes;

namespace Kernel.Core.Processes
{
    public static class SystemCalls
    {
        private static UInt32 sysCallNum = 0;
        private static UInt32 param1 = 0;
        private static UInt32 param2 = 0;
        private static UInt32 param3 = 0;
        public static UInt32 SysCallNumber
        {
            get
            {
                return sysCallNum;
            }
        }
        public static UInt32 Param1
        {
            get
            {
                return param1;
            }
        }
        public static UInt32 Param2
        {
            get
            {
                return param2;
            }
        }
        public static UInt32 Param3
        {
            get
            {
                return param3;
            }
        }
        public static UInt32 Return1
        {
            get
            {
                return ProcessManager.CurrentThread.EAXFromInterruptStack;
            }
            set
            {
                ProcessManager.CurrentThread.EAXFromInterruptStack = value;
            }
        }
        public static UInt32 Return2
        {
            get
            {
                return ProcessManager.CurrentThread.EBXFromInterruptStack;
            }
            set
            {
                ProcessManager.CurrentThread.EBXFromInterruptStack = value;
            }
        }
        public static UInt32 Return3
        {
            get
            {
                return ProcessManager.CurrentThread.ECXFromInterruptStack;
            }
            set
            {
                ProcessManager.CurrentThread.ECXFromInterruptStack = value;
            }
        }
        public static UInt32 Return4
        {
            get
            {
                return ProcessManager.CurrentThread.EDXFromInterruptStack;
            }
            set
            {
                ProcessManager.CurrentThread.EDXFromInterruptStack = value;
            }
        }


        private static int Int48HandlerId = 0;
        public static void Init()
        {
            if (Int48HandlerId == 0)
            {
                // We want to ignore process state so that we handle the interrupt in the context of
                //  the calling process.
                Int48HandlerId = Hardware.Interrupts.Interrupts.AddISRHandler(48, Int48, null, true);
            }
        }

        private static void Int48(FOS_System.Object state)
        {
            //Temp store because return value 1 is put in EAX
            sysCallNum = ProcessManager.CurrentThread.EAXFromInterruptStack;
            param1 = ProcessManager.CurrentThread.EBXFromInterruptStack;
            param2 = ProcessManager.CurrentThread.ECXFromInterruptStack;
            param3 = ProcessManager.CurrentThread.EDXFromInterruptStack;

            Console.Default.Write("Sys call ");
            Console.Default.Write_AsDecimal(SysCallNumber);
            Console.Default.Write(" : ");
            Console.Default.WriteLine(ProcessManager.CurrentProcess.Name);
            Console.Default.WriteLine(((FOS_System.String)" > Param1: ") + Param1);
            Console.Default.WriteLine(((FOS_System.String)" > Param2: ") + Param2);
            Console.Default.WriteLine(((FOS_System.String)" > Param3: ") + Param3);
            ++ProcessManager.CurrentThread.EAXFromInterruptStack;
            Console.Default.WriteLine(((FOS_System.String)" > Return: ") + ProcessManager.CurrentThread.EAXFromInterruptStack);
        }
    }
}
