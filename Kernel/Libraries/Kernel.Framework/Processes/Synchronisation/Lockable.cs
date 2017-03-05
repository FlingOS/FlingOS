namespace Kernel.Framework.Processes.Synchronisation
{
    public abstract class Lockable : Object
    {
        private int LockId;

        public bool Locked { get; private set; }


        protected Lockable(int Limit)
        {
            if (SystemCalls.CreateSemaphore(Limit, out LockId) != SystemCallResults.OK)
            {
                ExceptionMethods.Throw(new Exception("Could not obtain semaphore for the lock."));
            }

            Locked = false;
        }


        protected virtual void BeforeLock()
        {
        }

        public virtual void Lock()
        {
            BeforeLock();

            if (SystemCalls.WaitSemaphore(LockId) != SystemCallResults.OK)
            {
                ExceptionMethods.Throw(new Exception("Wait lock failed."));
            }

            Locked = true;

            AfterLock();
        }

        protected virtual void AfterLock()
        {
        }


        protected virtual void BeforeUnlock()
        {
        }

        public virtual void Unlock()
        {
            BeforeUnlock();

            if (SystemCalls.SignalSemaphore(LockId) != SystemCallResults.OK)
            {
                ExceptionMethods.Throw(new Exception("Signal lock failed."));
            }

            Locked = false;

            AfterUnlock();
        }

        protected virtual void AfterUnlock()
        {
        }
    }
}
