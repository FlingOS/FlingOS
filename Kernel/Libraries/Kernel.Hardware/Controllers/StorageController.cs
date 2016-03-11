using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.Processes;
using Kernel.Hardware.Devices;

namespace Kernel.Hardware.Controllers
{
    public static class StorageController
    {
        private static Queue DiskQueue;
        private static UInt32List ManagementThreadIds;
        private static int DiskQueueSemaphoreId;
        private static bool Initialised = false;

        private static bool Terminating;

        public static void Init()
        {
            if (!Initialised)
            {
                Initialised = true;
                DiskQueue = new Queue(5, true);
                ManagementThreadIds = new UInt32List(5);
                Terminating = false;

                if (SystemCalls.CreateSemaphore(1, out DiskQueueSemaphoreId) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Storage Controller failed to create a semaphore!");
                    ExceptionMethods.Throw(new FOS_System.Exceptions.NullReferenceException());
                }
            }
        }

        public static void AddDisk(DiskDevice TheDisk)
        {
            if (SystemCalls.WaitSemaphore(DiskQueueSemaphoreId) == SystemCallResults.OK)
            {
                DiskQueue.Push(TheDisk);

                uint NewThreadId;
                if (SystemCalls.StartThread(ManageDisk, out NewThreadId) == SystemCallResults.OK)
                {
                    ManagementThreadIds.Add(NewThreadId);
                }
                else
                {
                    BasicConsole.WriteLine("Storage Controller failed to create management thread!");
                }

                if (SystemCalls.SignalSemaphore(DiskQueueSemaphoreId) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Storage Controller failed to signal a semaphore! (1)");
                }
            }
            else
            {
                BasicConsole.WriteLine("Storage Controller failed to wait on a semaphore! (1)");
            }
        }
        public static void RemoveDisk(DiskDevice TheDisk)
        {
            //TODO: Removal of disk from controller
        }
        public static void Terminate()
        {
            //TODO: Termination of controller
        }

        private static void ManageDisk()
        {
            if (SystemCalls.WaitSemaphore(DiskQueueSemaphoreId) == SystemCallResults.OK)
            {
                DiskDevice TheDisk = (DiskDevice)DiskQueue.Pop();

                if (SystemCalls.SignalSemaphore(DiskQueueSemaphoreId) != SystemCallResults.OK)
                {
                    BasicConsole.WriteLine("Storage Controller failed to signal a semaphore! (2)");
                }

                BasicConsole.WriteLine("Storage Controller started.");

                while (!Terminating)
                {
                    //TODO: Receive & respond to commands
                    SystemCalls.SleepThread(SystemCalls.IndefiniteSleepThread);
                }

            }
            else
            {
                BasicConsole.WriteLine("Storage Controller failed to wait on a semaphore! (2)");
            }
        }
    }
}
