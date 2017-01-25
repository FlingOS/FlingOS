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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Drivers.Debugger.App
{
    internal delegate void VoidDelegate();

    internal delegate int IntDelegate();

    internal delegate uint UIntDelegate();

    internal delegate bool BoolDelegate();

    public partial class MainForm : Form
    {
        private readonly List<KeyValuePair<string, List<string>>> Breakpoints =
            new List<KeyValuePair<string, List<string>>>();

        private List<VariableData> ArgumentDatas = new List<VariableData>();

        private int ArgumentsDepthLoaded = 1;

        private string CurrentMethodASM;
        private string CurrentMethodLabel;
        private uint EIP = 0xFFFFFFFF;

        private string FileToLoadBreakpointsFrom;

        private List<KeyValuePair<string, List<string>>> FilteredLabels = new List<KeyValuePair<string, List<string>>>();

        private string FullLabelToStepTo;
        private List<VariableData> LocalDatas = new List<VariableData>();
        private int LocalsDepthLoaded = 1;
        private Tuple<uint, string> NearestLabel;

        private bool performingAction;

        private Dictionary<uint, Process> Processes;
        private Dictionary<string, uint> Registers = new Dictionary<string, uint>();
        private KeyValuePair<string, string> SelectedBreakpointFullLabel;
        private KeyValuePair<string, string> SelectedDebugPointFullLabel;
        private Debugger TheDebugger;

        private bool PerformingAction
        {
            get { return performingAction; }
            set
            {
                performingAction = value;
                UpdateEnableStates();
            }
        }

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
            Breakpoints.Clear();
            FilteredLabels.Clear();
            UpdateProcessTree();
            UpdateBreakpoints();
            PerformingAction = false;
        }

        private void AbortButton_Click(object sender, EventArgs e)
        {
            TheDebugger.AbortCommand();

            PerformingAction = false;
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            PerformingAction = true;
            Task.Run((Action)RefreshThreads);
        }

        private void DebugButton_Click(object sender, EventArgs e)
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

        private void StepThreadToLabelButton_Click(object sender, EventArgs e)
        {
            PerformingAction = true;

            if (LabelsTreeView.SelectedNode != null)
            {
                if (LabelsTreeView.SelectedNode.Parent != null)
                {
                    FullLabelToStepTo = LabelsTreeView.SelectedNode.Parent.Text + LabelsTreeView.SelectedNode.Text;
                }
                else
                {
                    FullLabelToStepTo = LabelsTreeView.SelectedNode.Text;
                }
            }

            Task.Run((Action)StepThreadToLabel);
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
                SelectedBreakpointFullLabel =
                    new KeyValuePair<string, string>(BreakpointsTreeView.SelectedNode.Parent.Text,
                        BreakpointsTreeView.SelectedNode.Text);
            }

            UpdateEnableStates();
        }

        private void LabelsTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (LabelsTreeView.SelectedNode != null &&
                LabelsTreeView.SelectedNode.Parent != null)
            {
                if (TheDebugger.IsDebugLabel(LabelsTreeView.SelectedNode.Parent.Text, LabelsTreeView.SelectedNode.Text))
                {
                    SelectedDebugPointFullLabel =
                        new KeyValuePair<string, string>(LabelsTreeView.SelectedNode.Parent.Text,
                            LabelsTreeView.SelectedNode.Text);
                }
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

        private void LoadLayerButton_Click(object sender, EventArgs e)
        {
            PerformingAction = true;
            Task.Run((Action)LoadNextStackDataLayer);
        }

        private void SetFromFileListButton_Click(object sender, EventArgs e)
        {
            TheOpenFileDialog.Filter = "Breakpoints List (.txt)|*.txt|All Files (*.*)|*.*";
            TheOpenFileDialog.InitialDirectory = BinPathBox.Text;

            if (TheOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                FileToLoadBreakpointsFrom = TheOpenFileDialog.FileName;

                PerformingAction = true;
                Task.Run((Action)SetBreakpointsFromFile);
            }
        }

        private void SaveToFileListButton_Click(object sender, EventArgs e)
        {
            TheSaveFileDialog.Filter = "Breakpoints List (.txt)|*.txt|All Files (*.*)|*.*";
            TheSaveFileDialog.FileName = FileToLoadBreakpointsFrom;

            if (TheSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter Str = new StreamWriter(TheSaveFileDialog.FileName, false))
                {
                    foreach (KeyValuePair<string, List<string>> MethodOfBPS in Breakpoints)
                    {
                        foreach (string LocalLabel in MethodOfBPS.Value)
                        {
                            string FullLabel = MethodOfBPS.Key + LocalLabel;
                            Str.WriteLine(FullLabel);
                        }
                    }
                }
            }
        }

        private void ViewBPASMCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            LoadASMPreview();
        }

        private void LoadASMPreview()
        {
            if (ViewBPASMCheckBox.Checked)
            {
                if (LabelsTreeView.SelectedNode != null)
                {
                    TreeNode TheNode = LabelsTreeView.SelectedNode;
                    if (TheNode.Parent != null)
                    {
                        TheNode = TheNode.Parent;
                    }

                    CurrentMethodASM = TheDebugger.GetMethodASM(TheNode.Text);
                }
                else if (CurrentMethodLabel != null)
                {
                    CurrentMethodASM = TheDebugger.GetMethodASM(CurrentMethodLabel);
                }
            }
            else if (CurrentMethodLabel != null)
            {
                CurrentMethodASM = TheDebugger.GetMethodASM(CurrentMethodLabel);
            }

            UpdateNearestLabel();
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
            if (!Directory.Exists(BinPathBox.Text))
            {
                MessageBox.Show("Invalid bin path! Please set the path to the Kernel's Debug build directory.");
            }
            else
            {
                TheDebugger = new Debugger(LogBox);
                TheDebugger.NotificationEvent += TheDebugger_NotificationEvent;
                UpdateEnableStates();

                if (!TheDebugger.Init(PipeNameBox.Text, BinPathBox.Text, AssemblyNameBox.Text))
                {
                    TheDebugger.Dispose();
                    TheDebugger = null;

                    UpdateEnableStates();
                }

                try
                {
                    while (!TheDebugger.Ready)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
                catch (NullReferenceException)
                {
                }
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

            PerformingAction = false;
        }

        private void SingleStepThread()
        {
            TheDebugger.SingleStepThread(GetSelectedProcessId(), GetSelectedThreadId());

            PerformingAction = false;
        }

        private void StepThreadToLabel()
        {
            TheDebugger.SingleStepThreadToAddress(GetSelectedProcessId(), GetSelectedThreadId(),
                TheDebugger.GetLabelAddress(FullLabelToStepTo));

            PerformingAction = false;
        }

        private void ClearBreakpoint()
        {
            uint BPAddress =
                TheDebugger.GetLabelAddress(SelectedBreakpointFullLabel.Key + SelectedBreakpointFullLabel.Value);
            if (TheDebugger.ClearBreakpoint(GetSelectedProcessId(), BPAddress))
            {
                KeyValuePair<string, List<string>> BP =
                    Breakpoints.Where(x => x.Key == SelectedBreakpointFullLabel.Key).First();
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
            DoSetBreakpoint(SelectedDebugPointFullLabel.Key + SelectedDebugPointFullLabel.Value);

            UpdateBreakpoints();

            PerformingAction = false;
        }

        private void DoSetBreakpoint(string FullLabel)
        {
            uint DPAddress = TheDebugger.GetLabelAddress(FullLabel);
            if (TheDebugger.SetBreakpoint(GetSelectedProcessId(), DPAddress))
            {
                List<KeyValuePair<string, List<string>>> BPs =
                    Breakpoints.Where(x => x.Key == FullLabel.Split('.')[0]).ToList();
                if (BPs.Count > 0)
                {
                    BPs.First().Value.Add("." + FullLabel.Split('.')[1]);
                }
                else
                {
                    KeyValuePair<string, List<string>> NewBP =
                        new KeyValuePair<string, List<string>>(FullLabel.Split('.')[0], new List<string>());
                    Breakpoints.Add(NewBP);
                    NewBP.Value.Add("." + FullLabel.Split('.')[1]);
                }
            }
        }

        private void SetBreakpointsFromFile()
        {
            if (FileToLoadBreakpointsFrom != null)
            {
                string[] LabelsOfBreakpointsToSet = File.ReadAllLines(FileToLoadBreakpointsFrom);
                foreach (string FullLabel in LabelsOfBreakpointsToSet)
                {
                    if (!string.IsNullOrWhiteSpace(FullLabel))
                    {
                        try
                        {
                            DoSetBreakpoint(FullLabel);
                        }
                        catch
                        {
                        }
                    }
                }

                UpdateBreakpoints();
            }

            PerformingAction = false;
        }

        private void RefreshThreads()
        {
            Processes = TheDebugger.GetThreads();

            PerformingAction = false;

            UpdateProcessTree();
        }

        private void RefreshRegisters()
        {
            if (IsSelectionDebugging())
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

            ArgumentDatas.Clear();
            LocalDatas.Clear();

            ArgumentsDepthLoaded = 1;
            LocalsDepthLoaded = 1;

            if (Registers.ContainsKey("EBP"))
            {
                uint EBP = Registers["EBP"];

                if (!string.IsNullOrWhiteSpace(CurrentMethodLabel))
                {
                    // Refresh arguments

                    MethodInfo TheMethod = TheDebugger.GetMethodInfo(CurrentMethodLabel);
                    if (TheMethod.Arguments.Count > 0)
                    {
                        try
                        {
                            int MaxOffset = TheMethod.Arguments.Select(x => x.Value.Offset).Max();
                            string MaxTypeID =
                                TheMethod.Arguments.Where(x => x.Value.Offset == MaxOffset)
                                    .Select(x => x.Value.TypeID)
                                    .First();
                            TypeInfo MaxArgType = TheDebugger.GetTypeInfo(MaxTypeID);

                            string ArgumentValuesStr = TheDebugger.GetMemoryValues(ProcessId, EBP,
                                MaxOffset + MaxArgType.SizeOnStackInBytes, 1);
                            byte[] ArgumentValuesArr =
                                ArgumentValuesStr.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                    .Select(x => byte.Parse(x.Substring(2), NumberStyles.HexNumber))
                                    .ToArray();
                            List<VariableInfo> ArgInfos = TheMethod.Arguments.Values.OrderBy(x => x.Offset).ToList();
                            foreach (VariableInfo AnArgInfo in ArgInfos)
                            {
                                TypeInfo ArgType = TheDebugger.GetTypeInfo(AnArgInfo.TypeID);
                                VariableData NewVarData = new VariableData
                                {
                                    Address = (uint)(EBP + AnArgInfo.Offset),
                                    OffsetFromParent = AnArgInfo.Offset,
                                    Info = ArgType,
                                    Value = new byte[ArgType.SizeOnStackInBytes]
                                };
                                for (int i = 0; i < ArgType.SizeOnStackInBytes; i++)
                                {
                                    NewVarData.Value[i] = ArgumentValuesArr[i + AnArgInfo.Offset];
                                }
                                ArgumentDatas.Add(NewVarData);
                            }
                        }
                        catch (Exception ex)
                        {
                            ArgumentDatas.Add(new VariableData
                            {
                                Name = "Error! " + ex.Message
                            });
                        }
                    }

                    if (Registers.ContainsKey("ESP"))
                    {
                        // Refresh locals

                        try
                        {
                            int MaxOffset = TheMethod.Locals.Count == 0
                                ? 0
                                : TheMethod.Locals.Select(x => x.Value.Offset).Max();
                            string MaxTypeID = TheMethod.Locals.Count == 0
                                ? ""
                                : TheMethod.Locals.Where(x => x.Value.Offset == MaxOffset)
                                    .Select(x => x.Value.TypeID)
                                    .First();
                            TypeInfo MaxLocType = TheMethod.Locals.Count == 0
                                ? null
                                : TheDebugger.GetTypeInfo(MaxTypeID);

                            uint ESP = Registers["ESP"];
                            int NumLocalBytes = (int)(EBP - ESP);
                            string LocalValuesStr = TheDebugger.GetMemoryValues(ProcessId, ESP, NumLocalBytes, 1);
                            byte[] LocalValuesArr =
                                LocalValuesStr.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                    .Select(x => byte.Parse(x.Substring(2), NumberStyles.HexNumber))
                                    .ToArray();

                            if (LocalValuesArr.Length > 0)
                            {
                                int max = LocalValuesArr.Length -
                                          (MaxOffset + (TheMethod.Locals.Count == 0 ? 0 : MaxLocType.SizeOnStackInBytes));
                                for (int i = 0; i < max;)
                                {
                                    VariableData NewVarData = new VariableData
                                    {
                                        Address = (uint)(ESP + i),
                                        Temporary = true,
                                        Value = new byte[4],
                                        OffsetFromParent = i
                                    };
                                    LocalDatas.Add(NewVarData);

                                    if (max - i >= 4)
                                    {
                                        for (int j = 0; j < 4; j++)
                                        {
                                            NewVarData.Value[j] = LocalValuesArr[j + i];
                                        }

                                        i += 4;
                                    }
                                    else
                                    {
                                        for (int j = 0; j < max - i; j++)
                                        {
                                            NewVarData.Value[j] = LocalValuesArr[j + i];
                                        }

                                        i = max;
                                    }
                                }

                                List<VariableInfo> LocInfos = TheMethod.Locals.Values.OrderBy(x => x.Offset).ToList();
                                foreach (VariableInfo ALocInfo in LocInfos)
                                {
                                    TypeInfo LocType = TheDebugger.GetTypeInfo(ALocInfo.TypeID);
                                    int position = LocalValuesArr.Length -
                                                   (-ALocInfo.Offset + LocType.SizeOnStackInBytes);
                                    VariableData NewVarData = new VariableData
                                    {
                                        Address = (uint)(EBP + ALocInfo.Offset),
                                        OffsetFromParent = position,
                                        Info = LocType,
                                        Value = new byte[LocType.SizeOnStackInBytes]
                                    };
                                    for (int i = 0; i < LocType.SizeOnStackInBytes; i++)
                                    {
                                        NewVarData.Value[i] = LocalValuesArr[i + position];
                                    }
                                    LocalDatas.Add(NewVarData);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LocalDatas.Add(new VariableData
                            {
                                Name = "Error! " + ex.Message
                            });
                        }
                    }
                }
                else
                {
                    if (Registers.ContainsKey("ESP"))
                    {
                        // Refresh locals
                        //  - Can get values as blocks of 4 bytes but not interpret them because we don't have the method info

                        try
                        {
                            uint ESP = Registers["ESP"];
                            int NumLocalBytes = (int)(EBP - ESP);
                            string LocalValuesStr = TheDebugger.GetMemoryValues(ProcessId, ESP, NumLocalBytes/4, 4);
                            uint[] LocalValuesArr =
                                LocalValuesStr.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                    .Select(x => uint.Parse(x.Substring(2), NumberStyles.HexNumber))
                                    .ToArray();
                            int offset = 0;
                            foreach (uint ALocalVal in LocalValuesArr)
                            {
                                LocalDatas.Add(new VariableData
                                {
                                    Address = ESP,
                                    Temporary = true,
                                    Value = BitConverter.GetBytes(ALocalVal),
                                    OffsetFromParent = offset
                                });
                                ESP += 4;
                                offset += 4;
                            }
                        }
                        catch (Exception ex)
                        {
                            LocalDatas.Add(new VariableData
                            {
                                Name = "Error! " + ex.Message
                            });
                        }
                    }
                }
            }

            UpdateStackData();

            PerformingAction = false;
        }

        private void RefreshBreakpoints()
        {
            string Filter = FilterBox.Text;
            FilteredLabels = TheDebugger.GetLabels(Filter);

            UpdateBreakpoints();
            UpdateDebugPoints();
        }

        private void LoadNextStackDataLayer()
        {
            uint ProcessId = GetSelectedProcessId();

            foreach (VariableData ArgData in ArgumentDatas)
            {
                ArgData.LoadFields(TheDebugger, ProcessId, ArgumentsDepthLoaded);
            }
            ArgumentsDepthLoaded++;

            foreach (VariableData LocData in LocalDatas)
            {
                LocData.LoadFields(TheDebugger, ProcessId, LocalsDepthLoaded);
            }
            LocalsDepthLoaded++;

            UpdateStackData();

            PerformingAction = false;
        }

        private bool IsSelectionDebugging()
        {
            if (InvokeRequired)
            {
                return (bool)Invoke(new BoolDelegate(IsSelectionDebugging));
            }
            bool NodeSuspended = false;
            if (ProcessesTreeView.SelectedNode != null)
            {
                uint SelectedProcessId = GetSelectedProcessId();
                int SelectedThreadId = GetSelectedThreadId();
                NodeSuspended = SelectedThreadId != -1 &&
                                Processes[SelectedProcessId].Threads[(uint)SelectedThreadId].State ==
                                Thread.States.Debugging;
            }
            return NodeSuspended;
        }

        private bool IsSelectionSuspended()
        {
            if (InvokeRequired)
            {
                return (bool)Invoke(new BoolDelegate(IsSelectionSuspended));
            }
            bool NodeSuspended = false;
            if (ProcessesTreeView.SelectedNode != null)
            {
                uint SelectedProcessId = GetSelectedProcessId();
                int SelectedThreadId = GetSelectedThreadId();
                NodeSuspended = SelectedThreadId != -1 &&
                                Processes[SelectedProcessId].Threads[(uint)SelectedThreadId].State ==
                                Thread.States.Suspended;
            }
            return NodeSuspended;
        }

        private void UpdateEnableStates()
        {
            if (InvokeRequired)
            {
                Invoke(new VoidDelegate(UpdateEnableStates));
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
                        bool NodeDebugging = IsSelectionDebugging();
                        DebugButton.Enabled = NodeSelected && !NodeDebugging;
                        ResumeButton.Enabled = NodeSelected && NodeDebugging;
                        StepButton.Enabled = NodeSelected && NodeDebugging;
                        SingleStepButton.Enabled = NodeSelected && NodeDebugging;
                        StepThreadToLabelButton.Enabled = NodeSelected && NodeDebugging &&
                                                          LabelsTreeView.SelectedNode != null;

                        NodeSelected = LabelsTreeView.SelectedNode != null &&
                                       LabelsTreeView.SelectedNode.Parent != null &&
                                       TheDebugger.IsDebugLabel(LabelsTreeView.SelectedNode.Parent.Text,
                                           LabelsTreeView.SelectedNode.Text);
                        SetBreakpointButton.Enabled = NodeSelected;

                        NodeSelected = BreakpointsTreeView.SelectedNode != null &&
                                       BreakpointsTreeView.SelectedNode.Parent != null;
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
            if (InvokeRequired)
            {
                Invoke(new VoidDelegate(UpdateProcessTree));
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
                        TreeNode NewProcessNode = ProcessesTreeView.Nodes.Add(AProcess.Id.ToString(),
                            AProcess.Id + ": " + AProcess.Name + " (" + AProcess.Priority + ")");
                        if (AProcess.Id == SelectedProcessId && SelectedThreadId == -1)
                        {
                            NodeToSelect = NewProcessNode;
                        }

                        foreach (Thread AThread in AProcess.Threads.Values)
                        {
                            TreeNode NewThreadNode = NewProcessNode.Nodes.Add(AThread.Id.ToString(),
                                AThread.Id + ": " + AThread.Name + " : " + AThread.State);
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
            if (InvokeRequired)
            {
                Invoke(new VoidDelegate(UpdateRegisters));
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
            if (InvokeRequired)
            {
                Invoke(new VoidDelegate(UpdateNearestLabel));
            }
            else
            {
                if (NearestLabel != null)
                {
                    if (TheDebugger.IsBreakpointAddress(EIP - 1))
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
            if (InvokeRequired)
            {
                Invoke(new VoidDelegate(UpdateBreakpoints));
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
            if (InvokeRequired)
            {
                Invoke(new VoidDelegate(UpdateDebugPoints));
            }
            else
            {
                string SelectedLocalLabel = null;
                string SelectedMethodLabel = null;
                if (LabelsTreeView.SelectedNode != null)
                {
                    if (LabelsTreeView.SelectedNode.Parent != null)
                    {
                        SelectedLocalLabel = LabelsTreeView.SelectedNode.Text;
                        SelectedMethodLabel = LabelsTreeView.SelectedNode.Parent.Text;
                    }
                    else
                    {
                        SelectedMethodLabel = LabelsTreeView.SelectedNode.Text;
                    }
                }

                LabelsTreeView.Nodes.Clear();

                TreeNode NodeToSelect = null;
                foreach (KeyValuePair<string, List<string>> AMethod in FilteredLabels)
                {
                    TreeNode MethodNode = LabelsTreeView.Nodes.Add(AMethod.Key, AMethod.Key);
                    if (SelectedMethodLabel == AMethod.Key && SelectedLocalLabel == null)
                    {
                        NodeToSelect = MethodNode;
                    }

                    foreach (string LocalLabel in AMethod.Value)
                    {
                        TreeNode LocalNode = MethodNode.Nodes.Add(LocalLabel, LocalLabel);
                        if (SelectedMethodLabel == AMethod.Key && SelectedLocalLabel == LocalLabel)
                        {
                            NodeToSelect = LocalNode;
                        }
                    }
                }

                if (NodeToSelect != null)
                {
                    LabelsTreeView.SelectedNode = NodeToSelect;
                }
            }
        }

        private void UpdateStackData()
        {
            if (InvokeRequired)
            {
                Invoke(new VoidDelegate(UpdateStackData));
            }
            else
            {
                ArgumentsTreeView.Nodes.Clear();
                ArgumentDatas = ArgumentDatas.OrderBy(x => x.OffsetFromParent).ToList();
                foreach (VariableData ArgData in ArgumentDatas)
                {
                    ArgumentsTreeView.Nodes.Add(ArgData.ToNode());
                }
                ArgumentsTreeView.ExpandAll();

                LocalsTreeView.Nodes.Clear();
                LocalDatas = LocalDatas.OrderBy(x => x.OffsetFromParent).ToList();
                foreach (VariableData LocData in LocalDatas)
                {
                    LocalsTreeView.Nodes.Add(LocData.ToNode());
                }
                LocalsTreeView.ExpandAll();
            }
        }

        private uint GetSelectedProcessId()
        {
            if (InvokeRequired)
            {
                return (uint)Invoke(new UIntDelegate(GetSelectedProcessId));
            }
            TreeNode ProcessNode = ProcessesTreeView.SelectedNode;
            if (ProcessNode.Parent != null)
            {
                ProcessNode = ProcessNode.Parent;
            }
            return uint.Parse(ProcessNode.Text.Split(':')[0]);
        }

        private int GetSelectedThreadId()
        {
            if (InvokeRequired)
            {
                return (int)Invoke(new IntDelegate(GetSelectedThreadId));
            }
            TreeNode ThreadNode = ProcessesTreeView.SelectedNode;
            if (ThreadNode.Parent == null)
            {
                return -1;
            }
            return int.Parse(ThreadNode.Text.Split(':')[0]);
        }
    }

    public class VariableData
    {
        public uint Address;

        public List<VariableData> Fields = new List<VariableData>();
        public TypeInfo Info;
        public string Name = "";
        public int OffsetFromParent;
        public bool Temporary;
        public byte[] Value;

        public void LoadFields(Debugger TheDebugger, uint ProcessId, int Depth)
        {
            if (Depth == 1)
            {
                if (Info != null &&
                    !Info.IsPointer &&
                    !Info.IsValueType &&
                    Value.Length == 4)
                {
                    uint AddressFromValue = BitConverter.ToUInt32(Value, 0);

                    if (AddressFromValue != 0)
                    {
                        string BaseTypeID = Info.BaseTypeID;
                        while (BaseTypeID != null)
                        {
                            TypeInfo BaseInfo = TheDebugger.GetTypeInfo(BaseTypeID);

                            ProcessFields(TheDebugger, ProcessId, AddressFromValue, BaseInfo.Fields.Values.ToList());

                            BaseTypeID = BaseInfo.BaseTypeID;
                        }

                        ProcessFields(TheDebugger, ProcessId, AddressFromValue, Info.Fields.Values.ToList());

                        if (Info.ID == "type_Kernel_Framework_String")
                        {
                            // Special treatment

                            // Find the length field
                            int length = 0;
                            bool found = false;
                            foreach (VariableData AField in Fields)
                            {
                                if (AField.Name == "length")
                                {
                                    found = true;
                                    length = BitConverter.ToInt32(AField.Value, 0);
                                    break;
                                }
                            }

                            // Load string bytes
                            //                                                              +8 for bytes for string fields
                            //                                                                                      *2 - 2 bytes/char
                            string StringBytesStr = TheDebugger.GetMemoryValues(ProcessId, AddressFromValue + 8,
                                length*2, 1);
                            byte[] StringBytesArr =
                                StringBytesStr.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                    .Select(x => byte.Parse(x.Substring(2), NumberStyles.HexNumber))
                                    .ToArray();
                            string StringVal = Encoding.Unicode.GetString(StringBytesArr);
                            Fields.Add(new VariableData
                            {
                                Address = AddressFromValue + 8,
                                Info = null,
                                OffsetFromParent = 8,
                                Value = new byte[0],
                                Name = "\"" + StringVal + "\""
                            });
                        }
                    }
                }
            }

            if (Depth > 1)
            {
                foreach (VariableData Field in Fields)
                {
                    Field.LoadFields(TheDebugger, ProcessId, Depth - 1);
                }
            }
        }

        private void ProcessFields(Debugger TheDebugger, uint ProcessId, uint BaseAddress,
            List<FieldInfo> FieldsToProcess)
        {
            foreach (FieldInfo AFieldInfo in FieldsToProcess)
            {
                if (!AFieldInfo.IsStatic)
                {
                    uint FieldAddress = (uint)(BaseAddress + AFieldInfo.OffsetInBytes);
                    TypeInfo FieldTypeInfo = TheDebugger.GetTypeInfo(AFieldInfo.TypeID);

                    if (FieldTypeInfo.SizeOnHeapInBytes > 0)
                    {
                        string FieldValueStr = TheDebugger.GetMemoryValues(ProcessId, FieldAddress,
                            FieldTypeInfo.IsValueType
                                ? FieldTypeInfo.SizeOnHeapInBytes
                                : FieldTypeInfo.SizeOnStackInBytes, 1);
                        byte[] FieldValueArr =
                            FieldValueStr.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => byte.Parse(x.Substring(2), NumberStyles.HexNumber))
                                .ToArray();
                        Fields.Add(new VariableData
                        {
                            Address = FieldAddress,
                            Info = FieldTypeInfo,
                            OffsetFromParent = AFieldInfo.OffsetInBytes,
                            Value = FieldValueArr,
                            Name = AFieldInfo.Name
                        });
                    }
                }
            }
        }

        public TreeNode ToNode()
        {
            string ValueStr = "";

            if (Info != null &&
                !Info.IsPointer &&
                !Info.IsValueType &&
                Value.Length == 4)
            {
                uint AddressFromValue = BitConverter.ToUInt32(Value, 0);

                if (AddressFromValue != 0)
                {
                    ValueStr = "0x" + AddressFromValue.ToString("X8");
                }
                else
                {
                    ValueStr = "Null";
                }
            }
            else if (Value != null && Value.Length > 0)
            {
                for (int i = 0; i < Value.Length; i++)
                {
                    ValueStr = Value[i].ToString("X2") + ValueStr;
                }
                ValueStr = "0x" + ValueStr;
            }
            else if (ValueStr == null)
            {
                ValueStr = "[NULL]";
            }

            TreeNode NewNode =
                new TreeNode((!string.IsNullOrEmpty(ValueStr) ? ValueStr + " : " : "") +
                             (!string.IsNullOrWhiteSpace(Name) ? Name : "") +
                             (Temporary ? " : Temp" : (Info != null ? " : " + Info.ID : "")));

            Fields = Fields.OrderBy(x => x.OffsetFromParent).ToList();
            foreach (VariableData Field in Fields)
            {
                NewNode.Nodes.Add(Field.ToNode());
            }

            return NewNode;
        }
    }
}