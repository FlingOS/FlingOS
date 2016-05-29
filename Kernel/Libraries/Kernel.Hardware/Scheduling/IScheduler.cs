#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //

#endregion

using Kernel.Framework;

namespace Kernel.Multiprocessing.Scheduling
{
    public delegate void PreemptionHandler(Object state);

    public interface IScheduler : IObject
    {
        long PreemptionPeriod { get; }

        void InitProcess(Process process, Scheduler.Priority priority);
        void InitThread(Process process, Thread t);

        void Init();
        PreemptionHandler Start();

        void HandlePageFault(uint eip, uint errorCode, uint address);

        void UpdateCurrentState();

        void Enable();
        void Disable();
        bool IsEnabled();

        void UpdateList(Thread t);
    }
}