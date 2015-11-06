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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Drivers.Debugger.App
{
    delegate void VoidDelegate();
    delegate int IntDelegate();
    delegate uint UIntDelegate();

    public partial class MainForm : Form
    {
        Debugger TheDebugger;

        bool performingAction = false;
        bool PerformingAction
        {
            get
            {
                return performingAction;
            }
            set
            {
                performingAction = value;
                UpdateEnableStates();
            }
        }

        Dictionary<uint, Process> Processes;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateEnableStates();
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            ConnectButton.Enabled = false;
            Task.Run((Action)Init);
        }
        private void DestroyButton_Click(object sender, EventArgs e)
        {
            TheDebugger.AbortCommand();
            System.Threading.Thread.Sleep(100);
            TheDebugger.Dispose();
            TheDebugger = null;
            Processes = null;
            UpdateProcessTree();
            UpdateEnableStates();
        }
        private void AbortButton_Click(object sender, EventArgs e)
        {
            TheDebugger.AbortCommand();
            UpdateEnableStates();
        }
        private void RefreshButton_Click(object sender, EventArgs e)
        {
            PerformingAction = true;
            Task.Run((Action)RefreshThreads);
        }
        private void SuspendButton_Click(object sender, EventArgs e)
        {
            PerformingAction = true;
            Task.Run((Action)SuspendThread);
        }
        private void ResumeButton_Click(object sender, EventArgs e)
        {
            PerformingAction = true;
            Task.Run((Action)ResumeThread);
        }
        private void StepButton_Click(object sender, EventArgs e)
        {
            PerformingAction = true;
            Task.Run((Action)StepThread);
        }
        private void SingleStepButton_Click(object sender, EventArgs e)
        {
            PerformingAction = true;
            Task.Run((Action)SingleStepThread);
        }
        private void ProcessesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateEnableStates();
        }

        private void Init()
        {
            TheDebugger = new Debugger();
            UpdateEnableStates();
            
            if (!TheDebugger.Init(PipeNameBox.Text, BinPathBox.Text, AssemblyNameBox.Text))
            {
                TheDebugger.Dispose();
                TheDebugger = null;

                UpdateEnableStates();
            }

            while (!TheDebugger.Ready)
            {
                System.Threading.Thread.Sleep(100);
            }

            UpdateEnableStates();
        }
        private void RefreshThreads()
        {
            Processes = TheDebugger.GetThreads();
            UpdateProcessTree();

            PerformingAction = false;
        }
        private void SuspendThread()
        {
            TheDebugger.SuspendThread(GetSelectedProcessId(), GetSelectedThreadId());
            
            RefreshThreads();
        }
        private void ResumeThread()
        {
            TheDebugger.ResumeThread(GetSelectedProcessId(), GetSelectedThreadId());
            
            RefreshThreads();
        }
        private void StepThread()
        {
            TheDebugger.StepThread(GetSelectedProcessId(), GetSelectedThreadId());
            
            RefreshThreads();
        }
        private void SingleStepThread()
        {
            TheDebugger.SingleStepThread(GetSelectedProcessId(), GetSelectedThreadId());
            
            RefreshThreads();
        }

        private void UpdateEnableStates()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new VoidDelegate(UpdateEnableStates));
            }
            else
            {
                if (TheDebugger != null)
                {
                    DestroyButton.Enabled = true;
                    PipeNameBox.Enabled = false;
                    AssemblyNameBox.Enabled = false;
                    BinPathBox.Enabled = false;
                    ConnectButton.Enabled = false;

                    AbortButton.Enabled = performingAction;

                    if (TheDebugger.Ready && !performingAction)
                    {
                        ConnectingProgressBar.Style = ProgressBarStyle.Blocks;
                        ConnectingProgressBar.Value = 100;
                        MainPanel.Enabled = true;

                        bool NodeSelected = ProcessesTreeView.SelectedNode != null;
                        bool NodeSuspended = false;
                        if(NodeSelected)
                        {
                            uint SelectedProcessId = GetSelectedProcessId();
                            int SelectedThreadId = GetSelectedThreadId();
                            NodeSelected = SelectedThreadId != -1 && Processes[SelectedProcessId].Threads[(uint)SelectedThreadId].State == Thread.States.Suspended;
                        }
                        SuspendButton.Enabled = NodeSelected && !NodeSuspended;
                        ResumeButton.Enabled = NodeSelected && NodeSuspended;
                        StepButton.Enabled = NodeSelected && NodeSuspended;
                        SingleStepButton.Enabled = NodeSelected && NodeSuspended;
                    }
                    else
                    {
                        ConnectingProgressBar.Style = ProgressBarStyle.Marquee;
                        MainPanel.Enabled = false;
                    }
                }
                else
                {
                    DestroyButton.Enabled = false;
                    PipeNameBox.Enabled = true;
                    AssemblyNameBox.Enabled = true;
                    BinPathBox.Enabled = true;
                    ConnectButton.Enabled = true;
                    MainPanel.Enabled = false;
                    ConnectingProgressBar.Style = ProgressBarStyle.Blocks;
                    ConnectingProgressBar.Value = 0;
                    AbortButton.Enabled = false;
                }
            }
        }
        private void UpdateProcessTree()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new VoidDelegate(UpdateProcessTree));
            }
            else
            {
                ProcessesTreeView.Nodes.Clear();

                if (Processes != null)
                {
                    foreach (Process AProcess in Processes.Values)
                    {
                        TreeNode NewNode = ProcessesTreeView.Nodes.Add(AProcess.Id.ToString(), AProcess.Id.ToString() + ": " + AProcess.Name);
                        foreach (Thread AThread in AProcess.Threads.Values)
                        {
                            NewNode.Nodes.Add(AThread.Id.ToString(), AThread.Id.ToString() + ": " + AThread.Name + " : " + AThread.State);
                        }
                    }
                }

                ProcessesTreeView.ExpandAll();
            }
        }

        private uint GetSelectedProcessId()
        {
            if (this.InvokeRequired)
            {
                return (uint)this.Invoke(new UIntDelegate(GetSelectedProcessId));
            }
            else
            {
                TreeNode ProcessNode = ProcessesTreeView.SelectedNode;
                if (ProcessNode.Parent != null)
                {
                    ProcessNode = ProcessNode.Parent;
                }
                return uint.Parse(ProcessNode.Text.Split(':')[0]);
            }
        }
        private int GetSelectedThreadId()
        {
            if (this.InvokeRequired)
            {
                return (int)this.Invoke(new IntDelegate(GetSelectedThreadId));
            }
            else
            {
                TreeNode ThreadNode = ProcessesTreeView.SelectedNode;
                if (ThreadNode.Parent == null)
                {
                    return -1;
                }
                return int.Parse(ThreadNode.Text.Split(':')[0]);
            }
        }
    }
}
