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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Drivers.Debugger
{
    public delegate void NotificationHandler(NotificationEventArgs e, object sender);

    public sealed class Debugger : IDisposable
    {
        private delegate void StringArrayDelegate(string[] Lines);

        private readonly List<uint> BreakpointAddresses = new List<uint>();
        private DebugDataReader DebugData;
        private Serial MsgSerial;

        private bool NotificationReceived;
        private Serial NotifSerial;

        private bool terminating;
        private bool WaitingForNotification;

        private System.Windows.Forms.TextBox LogBox;

        public bool Terminating
        {
            get { return terminating; }
            set
            {
                terminating = true;
                if (NotifSerial != null)
                {
                    NotifSerial.AbortRead = true;
                }
            }
        }

        public bool Ready { get; private set; }

        public Debugger(System.Windows.Forms.TextBox ALogBox)
        {
            LogBox = ALogBox;
            MsgSerial = new Serial();
        }

        public void Dispose()
        {
            Terminating = true;

            MsgSerial.Dispose();
            MsgSerial = null;
            if (NotifSerial != null)
            {
                NotifSerial.Dispose();
                NotifSerial = null;
            }
        }

        public event NotificationHandler NotificationEvent;

        public bool Init(string PipeName, string BinFolderPath, string AssemblyName)
        {
            DebugData = new DebugDataReader();
            DebugData.ReadDataFiles(BinFolderPath, AssemblyName);
            try
            {
                DebugData.ReadLibraryInfo(BinFolderPath, AssemblyName);
            }
            catch (Exception ex)
            {
            }

            MsgSerial = new Serial();
            NotifSerial = new Serial();
            MsgSerial.OnConnected += MsgSerial_OnConnected;
            return MsgSerial.Init(PipeName + "_Msg") && NotifSerial.Init(PipeName + "_Notif");
        }

        private void MsgSerial_OnConnected()
        {
            string str;
            while ((str = MsgSerial.ReadLine()) != "Debug thread :D")
            {
                System.Threading.Thread.Sleep(100);
            }
            Ready = true;

            Task.Run((Action)ProcessNotifications);
        }

        private void ProcessNotifications()
        {
            while (!Terminating)
            {
                try
                {
                    byte NotifByte = NotifSerial.ReadBytes(1)[0];
                    if (NotificationEvent != null)
                    {
                        NotificationReceived = true;
                        if (!WaitingForNotification)
                        {
                            NotificationEvent.Invoke(new NotificationEventArgs
                            {
                                NotificationByte = NotifByte
                            }, this);
                        }
                    }
                }
                catch
                {
                }
            }
        }

        public string[] ExecuteCommand(string cmd)
        {
            MsgSerial.WriteLine(cmd);
            System.Threading.Thread.Sleep(50);

            // First line should be command echo
            {
                string line;
                while ((line = MsgSerial.ReadLine()) != cmd && line != "END OF COMMAND")
                {
                }
            }

            return ReadToEndOfCommand();
        }

        public void AbortCommand()
        {
            MsgSerial.AbortRead = true;
            WaitingForNotification = false;
        }

        public bool GetPing()
        {
            try
            {
                string[] Lines = ExecuteCommand("ping");
                if (Lines[0] == "pong")
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public Dictionary<uint, Process> GetThreads()
        {
            try
            {
                string[] Lines = ExecuteCommand("threads");

                Dictionary<uint, Process> Processes = new Dictionary<uint, Process>();
                Process CurrentProcess = null;

                foreach (string Line in Lines)
                {
                    string[] LineParts = Line.Split(':').Select(x => x.Trim()).ToArray();
                    if (LineParts[0] == "- Process")
                    {
                        uint Id = uint.Parse(LineParts[1].Substring(2), NumberStyles.HexNumber);
                        CurrentProcess = new Process
                        {
                            Id = Id,
                            Name = LineParts[2],
                            Priority = LineParts[3]
                        };
                        Processes.Add(Id, CurrentProcess);
                    }
                    else if (LineParts[0] == "- Thread")
                    {
                        uint Id = uint.Parse(LineParts[1].Substring(2), NumberStyles.HexNumber);
                        CurrentProcess.Threads.Add(Id, new Thread
                        {
                            Id = Id,
                            Name = Line.Substring(Line.IndexOf(LineParts[3])),
                            State = (Thread.States)Enum.Parse(typeof(Thread.States), LineParts[2].Replace(" ", ""))
                        });
                    }
                }

                return Processes;
            }
            catch
            {
                return new Dictionary<uint, Process>();
            }
        }

        public Dictionary<string, uint> GetRegisters(uint ProcessId, uint ThreadId)
        {
            Dictionary<string, uint> Result = new Dictionary<string, uint>();

            try
            {
                string[] Lines = ExecuteCommand("regs " + ProcessId + " " + ThreadId);

                for (int i = 1; i < Lines.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(Lines[i]))
                    {
                        string[] LineParts = Lines[i].Split(':');
                        string Reg = LineParts[0].Trim().Substring(2);
                        string ValStr = LineParts[1].Trim();
                        uint Val = uint.Parse(ValStr.Substring(2), NumberStyles.HexNumber);
                        Result.Add(Reg, Val);
                    }
                }
            }
            catch
            {
                Result.Clear();
            }

            return Result;
        }

        public bool SuspendThread(uint ProcessId, int ThreadId)
        {
            try
            {
                string[] Lines = ExecuteCommand("suspend " + ProcessId + " " + ThreadId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ResumeThread(uint ProcessId, int ThreadId)
        {
            try
            {
                string[] Lines = ExecuteCommand("resume " + ProcessId + " " + ThreadId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool StepThread(uint ProcessId, int ThreadId)
        {
            try
            {
                //BeginWaitForNotification();
                string[] Lines = ExecuteCommand("step " + ProcessId + " " + ThreadId);
                //EndWaitForNotification();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SingleStepThread(uint ProcessId, int ThreadId)
        {
            try
            {
                //BeginWaitForNotification();
                string[] Lines = ExecuteCommand("ss " + ProcessId + " " + ThreadId);
                //EndWaitForNotification();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SingleStepThreadToAddress(uint ProcessId, int ThreadId, uint Address)
        {
            try
            {
                //BeginWaitForNotification();
                string[] Lines =
                    ExecuteCommand("sta " + ProcessId + " " + ThreadId + " " +
                                   Address.ToString("X8"));
                //EndWaitForNotification();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ClearBreakpoint(uint ProcessId, uint Address)
        {
            try
            {
                string[] Lines = ExecuteCommand("bpc " + ProcessId + " " + Address.ToString("X8"));
                BreakpointAddresses.Remove(Address);
                return true;
            }
            catch
            {
            }
            return false;
        }

        public bool SetBreakpoint(uint ProcessId, uint Address)
        {
            try
            {
                if (!BreakpointAddresses.Contains(Address))
                {
                    BreakpointAddresses.Add(Address);
                    string[] Lines = ExecuteCommand("bps " + ProcessId + " " + Address.ToString("X8"));
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

        public string GetMemoryValues(uint ProcessId, uint Address, int Length, int UnitSize)
        {
            try
            {
                string[] Lines =
                    ExecuteCommand("mem " + ProcessId + " " + Address.ToString("X8") + " " +
                                   Length + " " + UnitSize);
                return Lines[1];
            }
            catch
            {
            }
            return "";
        }

        public Tuple<uint, string> GetNearestLabel(uint Address)
        {
            while (!DebugData.AddressMappings.ContainsKey(Address) && Address > 0)
            {
                Address--;
            }

            if (DebugData.AddressMappings.ContainsKey(Address))
            {
                return new Tuple<uint, string>(Address,
                    DebugData.AddressMappings[Address].OrderBy(x => x.Length).First());
            }
            return null;
        }

        public string GetMethodLabel(string FullLabel)
        {
            return FullLabel.Split('.')[0];
        }

        public string GetMethodASM(string MethodLabel)
        {
            return DebugData.ReadMethodASM(MethodLabel);
        }

        public List<KeyValuePair<string, List<string>>> GetDebugOps(string Filter)
        {
            return DebugData.DebugOps.Where(x => x.Key.Contains(Filter)).ToList();
        }

        public List<KeyValuePair<string, List<string>>> GetLabels(string Filter)
        {
            List<KeyValuePair<string, List<string>>> Result = new List<KeyValuePair<string, List<string>>>();
            List<string> FilteredMethodLabels =
                DebugData.Methods.Where(x => x.Key.Contains(Filter)).Select(x => x.Key).ToList();
            foreach (string AMethodLabel in FilteredMethodLabels)
            {
                List<string> AllLabels = DebugData.LabelMappings
                    .Where(delegate(KeyValuePair<string, uint> x)
                    {
                        string[] parts = x.Key.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        return parts.Length == 2 && parts[0] == AMethodLabel;
                    })
                    .Select(x => "." + x.Key.Split(".".ToCharArray())[1])
                    .ToList();
                Result.Add(new KeyValuePair<string, List<string>>(AMethodLabel, AllLabels));
            }
            return Result;
        }

        public uint GetLabelAddress(string FullLabel)
        {
            if (DebugData.LabelMappings.ContainsKey(FullLabel))
            {
                return DebugData.LabelMappings[FullLabel];
            }

            return 0xFFFFFFFF;
        }

        public MethodInfo GetMethodInfo(string MethodLabel)
        {
            if (DebugData.Methods.ContainsKey(MethodLabel))
            {
                return DebugData.Methods[MethodLabel];
            }
            return null;
        }

        public TypeInfo GetTypeInfo(string TypeLabel)
        {
            if (DebugData.Types.ContainsKey(TypeLabel))
            {
                return DebugData.Types[TypeLabel];
            }
            return null;
        }

        public bool IsDebugLabel(string MethodLabel, string LocalLabel)
        {
            return DebugData.DebugOps.ContainsKey(MethodLabel) &&
                   DebugData.DebugOps[MethodLabel].Contains(LocalLabel);
        }

        public bool IsBreakpointAddress(uint Address)
        {
            return BreakpointAddresses.Contains(Address);
        }

        private string[] ReadToEndOfCommand()
        {
            List<string> Result = new List<string>();
            string str;
            while (!(str = MsgSerial.ReadLine()).StartsWith("END OF COM"))
            {
                Result.Add(str);
            }
            AddToLog(Result.ToArray());
            return Result.ToArray();
        }

        private void AddToLog(string[] Lines)
        {
            if (LogBox.InvokeRequired)
            {
                LogBox.Invoke(new StringArrayDelegate(AddToLog), new object[] {Lines});
            }
            else
            {
                LogBox.AppendText(string.Join(Environment.NewLine, Lines) + "\n");
            }
        }

        private void BeginWaitForNotification()
        {
            WaitingForNotification = true;
            NotificationReceived = false;
        }

        private void EndWaitForNotification()
        {
            while (!NotificationReceived && !Terminating && WaitingForNotification)
            {
                System.Threading.Thread.Sleep(50);
            }

            WaitingForNotification = false;
        }
    }

    public class NotificationEventArgs : EventArgs
    {
        public byte NotificationByte;
    }
}