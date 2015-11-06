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
    delegate bool BoolDelegate();

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
        Dictionary<string, uint> Registers = new Dictionary<string,uint>();
        uint EIP = 0xFFFFFFFF;
        Tuple<uint, string> NearestLabel;
        string CurrentMethodLabel;
        string CurrentMethodASM;

        List<KeyValuePair<string, List<string>>> FilteredDebugOps = new List<KeyValuePair<string,List<string>>>();
        List<KeyValuePair<string, List<string>>> Breakpoints = new List<KeyValuePair<string, List<string>>>();
        KeyValuePair<string, string> SelectedDebugPointFullLabel;
        KeyValuePair<string, string> SelectedBreakpointFullLabel;

        string ArgumentsStr = "";
        string LocalsStr = "";

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
            Task.Run((Action)Connect);
        }
        private void DestroyButton_Click(object sender, EventArgs e)
        {
            TheDebugger.AbortCommand();
            System.Threading.Thread.Sleep(100);
            TheDebugger.Dispose();
            TheDebugger = null;
            Processes = null;
            UpdateProcessTree();
            PerformingAction = false;
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
            PerformingAction = true;
            Task.Run((Action)RefreshRegisters);
        }
        private void FilterBox_TextChanged(object sender, EventArgs e)
        {
            if (FilterBox.Text.Length > 10)
            {
                RefreshBreakpoints();
            }
        }
        private void BreakpointsTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (BreakpointsTreeView.SelectedNode != null &&
                BreakpointsTreeView.SelectedNode.Parent != null)
            {
                SelectedBreakpointFullLabel = new KeyValuePair<string,string>(BreakpointsTreeView.SelectedNode.Parent.Text, BreakpointsTreeView.SelectedNode.Text);
            }

            UpdateEnableStates();
        }
        private void DebugPointsTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (DebugPointsTreeView.SelectedNode != null &&
                DebugPointsTreeView.SelectedNode.Parent != null)
            {
                SelectedDebugPointFullLabel = new KeyValuePair<string,string>(DebugPointsTreeView.SelectedNode.Parent.Text, DebugPointsTreeView.SelectedNode.Text);
            }

            UpdateEnableStates();
        }
        private void ClearBreakpointButton_Click(object sender, EventArgs e)
        {
            PerformingAction = true;
            Task.Run((Action)ClearBreakpoint);
        }
        private void SetBreakpointButton_Click(object sender, EventArgs e)
        {
            PerformingAction = true;
            Task.Run((Action)SetBreakpoint);
        }

        private void TheDebugger_NotificationEvent(NotificationEventArgs e, object sender)
        {
            while (PerformingAction)
            {
                System.Threading.Thread.Sleep(100);
            }
            RefreshThreads();
        }

        private void Connect()
        {
            TheDebugger = new Debugger();
            TheDebugger.NotificationEvent += TheDebugger_NotificationEvent;
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
        private void ClearBreakpoint()
        {
            uint BPAddress = TheDebugger.GetLabelAddress(SelectedBreakpointFullLabel.Key + SelectedBreakpointFullLabel.Value);
            if (TheDebugger.ClearBreakpoint(BPAddress))
            {
                KeyValuePair<string, List<string>> BP = Breakpoints.Where(x => x.Key == SelectedBreakpointFullLabel.Key).First();
                BP.Value.Remove(SelectedBreakpointFullLabel.Value);
                if (BP.Value.Count == 0)
                {
                    Breakpoints.Remove(BP);
                }

                UpdateBreakpoints();
            }

            PerformingAction = false;
        }
        private void SetBreakpoint()
        {
            uint DPAddress = TheDebugger.GetLabelAddress(SelectedDebugPointFullLabel.Key + SelectedDebugPointFullLabel.Value);
            if (TheDebugger.SetBreakpoint(DPAddress))
            {
                List<KeyValuePair<string, List<string>>> BPs = Breakpoints.Where(x => x.Key == SelectedDebugPointFullLabel.Key).ToList();
                if (BPs.Count > 0)
                {
                    BPs.First().Value.Add(SelectedDebugPointFullLabel.Value);
                }
                else
                {
                    KeyValuePair<string, List<string>> NewBP = new KeyValuePair<string, List<string>>(SelectedDebugPointFullLabel.Key, new List<string>());
                    Breakpoints.Add(NewBP);
                    NewBP.Value.Add(SelectedDebugPointFullLabel.Value);
                }

                UpdateBreakpoints();
            }

            PerformingAction = false;
        }

        private void RefreshThreads()
        {
            Processes = TheDebugger.GetThreads();
            UpdateProcessTree();

            PerformingAction = false;
        }
        private void RefreshRegisters()
        {
            if (IsSelectionSuspended())
            {
                Registers = TheDebugger.GetRegisters(GetSelectedProcessId(), (uint)GetSelectedThreadId());
            }
            else
            {
                Registers.Clear();
            }

            UpdateRegisters();

            RefreshNearestLabel();
        }
        private void RefreshNearestLabel()
        {
            if (Registers.ContainsKey("EIP"))
            {
                EIP = Registers["EIP"];
                NearestLabel = TheDebugger.GetNearestLabel(EIP);
                CurrentMethodLabel = TheDebugger.GetMethodLabel(NearestLabel.Item2);
                CurrentMethodASM = TheDebugger.GetMethodASM(CurrentMethodLabel);
            }
            else
            {
                EIP = 0xFFFFFFFF;
                NearestLabel = null;
                CurrentMethodLabel = "";
                CurrentMethodASM = "";
            }

            UpdateNearestLabel();

            RefreshStackData();
        }
        private void RefreshStackData()
        {
            uint ProcessId = GetSelectedProcessId();

            if (!string.IsNullOrWhiteSpace(CurrentMethodLabel))
            {
                // Refresh arguments
                // - Cannot do yet because the compiler supplies insufficient information
            }
            else
            {
                ArgumentsStr = "";
            }

            if (Registers.ContainsKey("ESP") && Registers.ContainsKey("EBP"))
            {
                // Refresh locals
                //  - Can get values as blocks of 4 bytes but not interpret them yet because compiler
                //    supplies insufficient info

                uint ESP = Registers["ESP"];
                uint EBP = Registers["EBP"];
                int NumLocalBytes = (int)(EBP - ESP);
                LocalsStr = TheDebugger.GetMemoryValues(ProcessId, ESP, NumLocalBytes / 4, 4);
            }
            else
            {
                LocalsStr = "";
            }

            UpdateStackData();

            PerformingAction = false;
        }
        private void RefreshBreakpoints()
        {
            string Filter = FilterBox.Text;
            FilteredDebugOps = TheDebugger.GetDebugOps(Filter);

            UpdateBreakpoints();
            UpdateDebugPoints();
        }

        private bool IsSelectionSuspended()
        {
            if (this.InvokeRequired)
            {
                return (bool)this.Invoke(new BoolDelegate(IsSelectionSuspended));
            }
            else
            {
                bool NodeSuspended = false;
                if (ProcessesTreeView.SelectedNode != null)
                {
                    uint SelectedProcessId = GetSelectedProcessId();
                    int SelectedThreadId = GetSelectedThreadId();
                    NodeSuspended = SelectedThreadId != -1 && Processes[SelectedProcessId].Threads[(uint)SelectedThreadId].State == Thread.States.Suspended;
                }
                return NodeSuspended;
            }
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
                        bool NodeSuspended = IsSelectionSuspended();
                        SuspendButton.Enabled = NodeSelected && !NodeSuspended;
                        ResumeButton.Enabled = NodeSelected && NodeSuspended;
                        StepButton.Enabled = NodeSelected && NodeSuspended;
                        SingleStepButton.Enabled = NodeSelected && NodeSuspended;

                        NodeSelected = DebugPointsTreeView.SelectedNode != null && DebugPointsTreeView.SelectedNode.Parent != null;
                        SetBreakpointButton.Enabled = NodeSelected;

                        NodeSelected = BreakpointsTreeView.SelectedNode != null && BreakpointsTreeView.SelectedNode.Parent != null;
                        ClearBreakpointButton.Enabled = NodeSelected;
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
                uint SelectedProcessId = 0;
                int SelectedThreadId = 0;
                if (ProcessesTreeView.SelectedNode != null)
                {
                    SelectedProcessId = GetSelectedProcessId();
                    SelectedThreadId = GetSelectedThreadId();
                }

                ProcessesTreeView.Nodes.Clear();

                TreeNode NodeToSelect = null;
                if (Processes != null)
                {
                    foreach (Process AProcess in Processes.Values)
                    {
                        TreeNode NewProcessNode = ProcessesTreeView.Nodes.Add(AProcess.Id.ToString(), AProcess.Id.ToString() + ": " + AProcess.Name);
                        if (AProcess.Id == SelectedProcessId && SelectedThreadId == -1)
                        {
                            NodeToSelect = NewProcessNode;
                        }

                        foreach (Thread AThread in AProcess.Threads.Values)
                        {
                            TreeNode NewThreadNode = NewProcessNode.Nodes.Add(AThread.Id.ToString(), AThread.Id.ToString() + ": " + AThread.Name + " : " + AThread.State);
                            if (AProcess.Id == SelectedProcessId && AThread.Id == SelectedThreadId)
                            {
                                NodeToSelect = NewThreadNode;
                            }
                        }
                    }
                }

                ProcessesTreeView.ExpandAll();

                if (NodeToSelect != null)
                {
                    ProcessesTreeView.SelectedNode = NodeToSelect;
                }
            }
        }
        private void UpdateRegisters()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new VoidDelegate(UpdateRegisters));
            }
            else
            {
                Thex86RegistersControl.EAX = 0;
                Thex86RegistersControl.EBX = 0;
                Thex86RegistersControl.ECX = 0;
                Thex86RegistersControl.EDX = 0;
                Thex86RegistersControl.ESP = 0;
                Thex86RegistersControl.EBP = 0;
                Thex86RegistersControl.EIP = 0;

                foreach (KeyValuePair<string, uint> Reg in Registers)
                {
                    switch (Reg.Key)
                    {
                        case "EAX":
                            Thex86RegistersControl.EAX = Reg.Value;
                            break;
                        case "EBX":
                            Thex86RegistersControl.EBX = Reg.Value;
                            break;
                        case "ECX":
                            Thex86RegistersControl.ECX = Reg.Value;
                            break;
                        case "EDX":
                            Thex86RegistersControl.EDX = Reg.Value;
                            break;
                        case "ESP":
                            Thex86RegistersControl.ESP = Reg.Value;
                            break;
                        case "EBP":
                            Thex86RegistersControl.EBP = Reg.Value;
                            break;
                        case "EIP":
                            Thex86RegistersControl.EIP = Reg.Value;
                            break;
                    }
                }
            }
        }
        private void UpdateNearestLabel()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new VoidDelegate(UpdateNearestLabel));
            }
            else
            {
                if (NearestLabel != null)
                {
                    if (TheDebugger.IsBreakpointAddress(EIP-1))
                    {
                        NearestLabelAddessBox.BackColor = Color.Pink;
                    }
                    else if (NearestLabel.Item1 == EIP)
                    {
                        NearestLabelAddessBox.BackColor = Color.LightGreen;
                    }
                    else
                    {
                        NearestLabelAddessBox.BackColor = Color.LightBlue;
                    }

                    NearestLabelAddessBox.Text = NearestLabel.Item1.ToString("X8");
                    NearestLabelBox.Text = NearestLabel.Item2;

                    MethodLabelBox.Text = CurrentMethodLabel;
                    CurrentMethodBox.Text = CurrentMethodASM;

                    if (NearestLabel.Item2.Contains("."))
                    {
                        string LocalLabel = "." + NearestLabel.Item2.Split('.')[1];
                        string[] LocalLabelParts = LocalLabel.Split('_');
                        if (LocalLabelParts.Length == 3)
                        {
                            LocalLabel = LocalLabelParts[0] + "_" + LocalLabelParts[1];
                        }

                        string OffsetStr = "??";

                        try
                        {
                            int LabelIndex = CurrentMethodASM.IndexOf(LocalLabel + "  --");
                            if (LabelIndex > -1)
                            {
                                int EOLIndex = CurrentMethodASM.IndexOf('\n', LabelIndex);
                                string LabelLine = CurrentMethodASM.Substring(LabelIndex, EOLIndex - LabelIndex);
                                OffsetStr = LabelLine.Split(':').Last().Trim();
                            }
                        }
                        catch
                        {
                        }

                        MethodLocalLabelLabel.Text = LocalLabel + " : 0x" + OffsetStr;
                    }
                    else
                    {
                        MethodLocalLabelLabel.Text = "[NO LOCAL]";
                    }
                }
                else
                {
                    NearestLabelAddessBox.Text = "";
                    NearestLabelBox.Text = "";
                    MethodLocalLabelLabel.Text = "[NO LOCAL]";
                    MethodLabelBox.Text = "";
                    CurrentMethodBox.Text = "";
                    NearestLabelAddessBox.BackColor = Color.White;
                }
            }
        }
        private void UpdateBreakpoints()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new VoidDelegate(UpdateBreakpoints));
            }
            else
            {
                string SelectedLocalLabel = null;
                string SelectedMethodLabel = null;
                if (BreakpointsTreeView.SelectedNode != null)
                {
                    if (BreakpointsTreeView.SelectedNode.Parent != null)
                    {
                        SelectedLocalLabel = BreakpointsTreeView.SelectedNode.Text;
                        SelectedMethodLabel = BreakpointsTreeView.SelectedNode.Parent.Text;
                    }
                    else
                    {
                        SelectedMethodLabel = BreakpointsTreeView.SelectedNode.Text;
                    }
                }

                BreakpointsTreeView.Nodes.Clear();

                TreeNode NodeToSelect = null;
                foreach (KeyValuePair<string, List<string>> Breakpoint in Breakpoints)
                {
                    TreeNode MethodNode = BreakpointsTreeView.Nodes.Add(Breakpoint.Key, Breakpoint.Key);
                    if (SelectedMethodLabel == Breakpoint.Key && SelectedLocalLabel == null)
                    {
                        NodeToSelect = MethodNode;
                    }

                    foreach (string LocalLabel in Breakpoint.Value)
                    {
                        TreeNode LocalNode = MethodNode.Nodes.Add(LocalLabel, LocalLabel);
                        if (SelectedMethodLabel == Breakpoint.Key && SelectedLocalLabel == LocalLabel)
                        {
                            NodeToSelect = LocalNode;
                        }
                    }
                }

                if (NodeToSelect != null)
                {
                    BreakpointsTreeView.SelectedNode = NodeToSelect;
                }
            }
        }
        private void UpdateDebugPoints()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new VoidDelegate(UpdateDebugPoints));
            }
            else
            {
                string SelectedLocalLabel = null;
                string SelectedMethodLabel = null;
                if (DebugPointsTreeView.SelectedNode != null)
                {
                    if (DebugPointsTreeView.SelectedNode.Parent != null)
                    {
                        SelectedLocalLabel = DebugPointsTreeView.SelectedNode.Text;
                        SelectedMethodLabel = DebugPointsTreeView.SelectedNode.Parent.Text;
                    }
                    else
                    {
                        SelectedMethodLabel = DebugPointsTreeView.SelectedNode.Text;
                    }
                }

                DebugPointsTreeView.Nodes.Clear();

                TreeNode NodeToSelect = null;
                foreach (KeyValuePair<string, List<string>> DebuggableMethod in FilteredDebugOps)
                {
                    TreeNode MethodNode = DebugPointsTreeView.Nodes.Add(DebuggableMethod.Key, DebuggableMethod.Key);
                    if (SelectedMethodLabel == DebuggableMethod.Key && SelectedLocalLabel == null)
                    {
                        NodeToSelect = MethodNode;
                    }

                    foreach (string LocalLabel in DebuggableMethod.Value)
                    {
                        TreeNode LocalNode = MethodNode.Nodes.Add(LocalLabel, LocalLabel);
                        if (SelectedMethodLabel == DebuggableMethod.Key && SelectedLocalLabel == LocalLabel)
                        {
                            NodeToSelect = LocalNode;
                        }
                    }
                }

                if (NodeToSelect != null)
                {
                    DebugPointsTreeView.SelectedNode = NodeToSelect;
                }
            }
        }
        private void UpdateStackData()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new VoidDelegate(UpdateStackData));
            }
            else
            {
                ArgumentsBox.Text = ArgumentsStr;
                LocalsBox.Text = LocalsStr;
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
