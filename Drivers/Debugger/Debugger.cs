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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Debugger
{
    public sealed class Debugger : IDisposable
    {
        private Serial TheSerial;
        private DebugDataReader DebugData;

        public bool Ready
        {
            get;
            private set;
        }

        public Debugger()
        {
            TheSerial = new Serial();
        }
        public void Dispose()
        {
            TheSerial.Dispose();
            TheSerial = null;
        }

        public bool Init(string PipeName, string BinFolderPath, string AssemblyName)
        {
            DebugData = new DebugDataReader();
            DebugData.ReadDataFiles(BinFolderPath, AssemblyName);

            TheSerial = new Serial();
            TheSerial.OnConnected += TheSerial_OnConnected;
            return TheSerial.Init(PipeName);
        }

        private void TheSerial_OnConnected()
        {
            string str;
            while((str = TheSerial.ReadLine()) != "Debug thread :D")
            {
                Console.WriteLine(str);
                System.Threading.Thread.Sleep(100);
            }
            Console.WriteLine(str);
            Ready = true;
        }

        public string[] ExecuteCommand(string cmd)
        {
            TheSerial.WriteLine(cmd);

            // First line should be command echo
            {
                string line = TheSerial.ReadLine();
                if (line.Trim().ToLower() != cmd.Trim().ToLower())
                {
                    while ((line = TheSerial.ReadLine()) != "END OF COMMAND")
                    {
                    }
                }
            }

            string[] Lines = ReadToEndOfCommand();
            foreach (string line in Lines)
            {
                Console.WriteLine(line);
            }

            return Lines;
        }
        public void AbortCommand()
        {
            TheSerial.AbortRead = true;
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
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        public List<Process> GetThreads()
        {
            try
            {
                string[] Lines = ExecuteCommand("threads");

                List<Process> Processes = new List<Process>();
                Process CurrentProcess = null;

                foreach (string Line in Lines)
                {
                    string[] LineParts = Line.Split(':').Select(x => x.Trim()).ToArray();
                    if (LineParts[0] == "- Process")
                    {
                        CurrentProcess = new Process()
                        {
                            Id = uint.Parse(LineParts[1].Substring(2), System.Globalization.NumberStyles.HexNumber),
                            Name = LineParts[2]
                        };
                        Processes.Add(CurrentProcess);
                    }
                    else if (LineParts[0] == "- Thread")
                    {
                        CurrentProcess.Threads.Add(new Thread()
                        {
                            Id = uint.Parse(LineParts[1].Substring(2), System.Globalization.NumberStyles.HexNumber),
                            Name = LineParts[3],
                            State = (Thread.States)Enum.Parse(typeof(Thread.States), LineParts[2])
                        });
                    }
                }

                return Processes;
            }
            catch
            {
                return new List<Process>();
            }
        }
        public bool SuspendThread(uint ProcessId, int ThreadId)
        {
            try
            {
                string[] Lines = ExecuteCommand("suspend " + ProcessId.ToString() + " " + ThreadId.ToString());
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
                string[] Lines = ExecuteCommand("resume " + ProcessId.ToString() + " " + ThreadId.ToString());
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
                string[] Lines = ExecuteCommand("step " + ProcessId.ToString() + " " + ThreadId.ToString());
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
                string[] Lines = ExecuteCommand("ss " + ProcessId.ToString() + " " + ThreadId.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string[] ReadToEndOfCommand()
        {
            List<string> Result = new List<string>();
            string str;
            while ((str = TheSerial.ReadLine()) != "END OF COMMAND")
            {
                Result.Add(str);
            }
            return Result.ToArray();
        }
    }
}
