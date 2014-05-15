using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kernel.Debug.Data;
using System.Runtime.InteropServices;
using FastColoredTextBoxNS;

namespace Kernel.Debug.Debugger
{
    /// <summary>
    /// The main window for the kernel debugger application
    /// </summary>
    public partial class MainForm : Form
    {
        Debugger TheDebugger;
        bool Terminating = false;
        bool BreakOnStart = false;
        
        IVirtualMachine theVM;
        
        ulong StepToAddress = ulong.MaxValue;

        /// <summary>
        /// Initialises the window
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }


        private delegate string GetTextDelegate(TextBox theBox);
        private string GetText(TextBox theBox)
        {
            if (this.InvokeRequired)
            {
                return (string)this.Invoke(new GetTextDelegate(GetText), new object[] { theBox });
            }
            else
            {
                return theBox.Text;
            }
        }
        private delegate void SetTextDelegate(string text, TextBox theBox);
        private void SetText(string text, TextBox theBox)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetTextDelegate(SetText), new object[] { text, theBox });
            }
            else
            {
                theBox.Text = text;
            }
        }
        private void AddText(string text)
        {
            string origText = GetText(OutputBox);
            origText = text + "\r\n" + origText;
            SetText(origText, OutputBox);
        }
        
        const int WM_USER = 0x400;
        const int EM_HIDESELECTION = WM_USER + 63;
        const int SB_HORZ = 0x0;
        const int SB_VERT = 0x1;
        const int WM_HSCROLL = 0x114;
        const int WM_VSCROLL = 0x115;
        const int SB_THUMBPOSITION = 4;
        const int SB_BOTTOM = 7;
        const int SB_OFFSET = 13;

        [DllImport("user32.dll")]
        static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetScrollPos(IntPtr hWnd, int nBar);
        [DllImport("user32.dll")]
        private static extern bool PostMessageA(IntPtr hWnd, int nBar, int wParam, int lParam);
        [DllImport("user32.dll")]
        static extern bool GetScrollRange(IntPtr hWnd, int nBar, out int lpMinPos, out int lpMaxPos);
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        
        
        private void HideSelection(bool hide)
        {
            SendMessage(ASMBox.Handle, EM_HIDESELECTION, hide ? 1 : 0, 0);
        }
        int StoredASMScrollPos = -1;
        private void StoreASMScrollPos()
        {
            StoredASMScrollPos = GetScrollPos(ASMBox.Handle, SB_VERT);
        }
        private void RestoreASMScrollPos()
        {
            if(StoredASMScrollPos != -1)
            {
                SetScrollPos(ASMBox.Handle, SB_VERT, StoredASMScrollPos, true);
                PostMessageA(ASMBox.Handle, WM_VSCROLL, SB_THUMBPOSITION + 0x10000 * StoredASMScrollPos, 0);
            }
        }

        /*
         // Constants for extern calls to various scrollbar functions
        
        private void LogMessages(string text)
        {
            if (this.logToScreen)
            {
                bool bottomFlag = false;
                int VSmin;
                int VSmax;
                int sbOffset;
                int savedVpos;
                // Make sure this is done in the UI thread
                if (this.txtBoxLogging.InvokeRequired)
                {
                    this.txtBoxLogging.Invoke(new TextBoxLoggerDelegate(LogMessages), new object[] { text });
                }
                else
                {
                    // Win32 magic to keep the textbox scrolling to the newest append to the textbox unless
                    // the user has moved the scrollbox up
                    sbOffset = (int)((this.txtBoxLogging.ClientSize.Height - SystemInformation.HorizontalScrollBarHeight) / (this.txtBoxLogging.Font.Height));
                    savedVpos = GetScrollPos(this.txtBoxLogging.Handle, SB_VERT);
                    GetScrollRange(this.txtBoxLogging.Handle, SB_VERT, out VSmin, out VSmax);
                    if (savedVpos >= (VSmax - sbOffset - 1))
                        bottomFlag = true;
                    this.txtBoxLogging.AppendText(text + Environment.NewLine);
                    if (bottomFlag)
                    {
                        GetScrollRange(this.txtBoxLogging.Handle, SB_VERT, out VSmin, out VSmax);
                        savedVpos = VSmax - sbOffset;
                        bottomFlag = false;
                    }
                    SetScrollPos(this.txtBoxLogging.Handle, SB_VERT, savedVpos, true);
                    PostMessageA(this.txtBoxLogging.Handle, WM_VSCROLL, SB_THUMBPOSITION + 0x10000 * savedVpos, 0);
                }
            }
        }
         */

        DB_Method oldMethod = null;
        private delegate void VoidDelegate();
        private void ReloadASMText()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new VoidDelegate(ReloadASMText));
                return;
            }

            StoreASMScrollPos();
            HideSelection(true);

            int currentBreakLineOffset = TheDebugger.CurrentASMLineStartOffset;
            int currentBreakLineLength = TheDebugger.CurrentASMLineLength;

            int actualCurrBreakLineOffset = -1;
            
            string[] Lines = TheDebugger.CurrentMethodASM.Split('\n');
            int offset = 0;
            int actualOffset = 0;
            bool foundCurBreakLine = false;
            bool shouldAddText = false;
            
            //if (TheDebugger.CurrentMethod != oldMethod)
            {
                oldMethod = TheDebugger.CurrentMethod;
                shouldAddText = true;
                ASMBox.Text = "";
            }
            
            //Set to new text and colour it all

            ASMBox.SelectionBackColor = Color.Transparent;

            for (int i = 0; i < Lines.Length; i++)
            {
                string line = Lines[i];

                if (!shouldAddText)
                {
                    ASMBox.SelectionStart = actualOffset;
                    ASMBox.SelectionLength = line.Length;
                }

                if (line.Trim().StartsWith(";"))
                {
                    //Comment
                    ASMBox.SelectionColor = Color.Green;
                }
                else if (line.Split(';')[0].Trim().EndsWith(":"))
                {
                    //Label
                    ASMBox.SelectionColor = Color.DarkBlue;
                }
                else
                {
                    //Code
                    ASMBox.SelectionColor = Color.Black;
                    
                    if (currentBreakLineOffset != -1)
                    {
                        if (!foundCurBreakLine && offset >= currentBreakLineOffset)
                        {
                            foundCurBreakLine = true;
                            ASMBox.SelectionBackColor = Color.Pink;
                            actualCurrBreakLineOffset = actualOffset;
                        }
                        else
                        {
                            ASMBox.SelectionBackColor = Color.Transparent;
                        }
                    }
                    else
                    {
                        ASMBox.SelectionBackColor = Color.Transparent;
                    }
                }

                if (shouldAddText)
                {
                    //Add a space before newline to replace "\r"
                    //since RTF box doesn't support \r (or at least
                    //not like a normal text box!)
                    ASMBox.AppendText(line + "\n");
                }
                offset += line.Length + 1;
                //Not +1 because line contains \r
                //and actual text doesn't...
                actualOffset += line.Length;
            }

            HideSelection(false);

            if (actualCurrBreakLineOffset != -1)
            {
                ASMBox.SelectionStart = actualCurrBreakLineOffset;
            }
            ASMBox.SelectionLength = 0;

            if (!shouldAddText && TheDebugger.State == Debugger.States.Stepping)
            {
                //RestoreASMScrollPos();
            }
        }

        int oldCurrCSLineHighlightStartPos = -1;
        int oldCurrCSLineHighlightEndPos = -1;
        MarkerStyle CurrCSLineStyle = new FastColoredTextBoxNS.MarkerStyle(Brushes.Pink);
        private void ReloadCSText()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new VoidDelegate(ReloadCSText));
                return;
            }

            if (oldCurrCSLineHighlightStartPos > -1)
            {
                CSBox.GetRange(oldCurrCSLineHighlightStartPos, oldCurrCSLineHighlightEndPos).ClearStyle(CurrCSLineStyle);
                oldCurrCSLineHighlightStartPos = -1;
            }

            if (TheDebugger.CurrentCSMethod != null)
            {
                string CS = TheDebugger.CurrentCSMethod.GetCS();
                CSBox.Text = CS;

                if (TheDebugger.CurrentCSLine != null)
                {
                    string[] CSLines = CSBox.Text.Split('\n');
                    int length = 0;
                    if (TheDebugger.CurrentCSLine.CSLineNum < CSLines.Length)
                    {
                        for (int i = 0; i < CSLines.Length; i++)
                        {
                            if (TheDebugger.CurrentCSLine.CSLineNum == i)
                            {
                                oldCurrCSLineHighlightStartPos = length + (CSLines[i].Length - CSLines[i].TrimStart().Length);
                                oldCurrCSLineHighlightEndPos = length + CSLines[i].Length;
                                CSBox.GetRange(oldCurrCSLineHighlightStartPos, oldCurrCSLineHighlightEndPos)
                                    .SetStyle(CurrCSLineStyle);
                            }
                            length += CSLines[i].Length + 1;
                        }
                    }
                }
            }
            else
            {
                CSBox.Text = "";
            }
        }

        private void ReloadArguments()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new VoidDelegate(ReloadArguments));
                return;
            }

            ArgumentsTreeView.Nodes.Clear();

            List<Argument> AllArguments = TheDebugger.Arguments;
            for (int i = 0; i < AllArguments.Count; i++)
            {
                Argument cArg = AllArguments[i];
                CommonTools.Node argNode = new CommonTools.Node();
                
                object[] nodeData = new object[3];
                nodeData[0] = i;
                nodeData[1] = cArg.dbType.Signature;
                nodeData[2] = Utils.GetValueStr(cArg.value, cArg.dbType.Signature == "System.String" ? null : cArg.dbType.Signature);

                if (!cArg.dbType.IsValueType)
                {
                    cArg.LoadFields(TheDebugger, delegate()
                    {
                        this.Invoke(new VoidDelegate(delegate()
                        {
                            LoadVariablesNode(cArg, argNode, nodeData);
                        }));
                    }, true);
                }

                if(cArg.isReturnArg)
                {
                    nodeData[0] = "Ret";
                }

                argNode.SetData(nodeData);
                ArgumentsTreeView.Nodes.Add(argNode);
            }
            ArgumentsTreeView.Nodes.Add(new CommonTools.Node(""));
        }
        private void ReloadLocals()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new VoidDelegate(ReloadLocals));
                return;
            }

            LocalsTreeView.Nodes.Clear();

            List<Local> AllLocals = TheDebugger.Locals;
            for(int i = 0; i < AllLocals.Count; i++)
            {
                Local cLoc = AllLocals[i];
                CommonTools.Node localNode = new CommonTools.Node();

                object[] nodeData = new object[3];
                nodeData[0] = i;
                nodeData[1] = cLoc.dbType.Signature;
                nodeData[2] = Utils.GetValueStr(cLoc.value, cLoc.dbType.Signature == "System.String" ? null : cLoc.dbType.Signature);

                if(!cLoc.dbType.IsValueType)
                {
                    cLoc.LoadFields(TheDebugger, delegate()
                    {
                        this.Invoke(new VoidDelegate(delegate()
                        {
                            LoadVariablesNode(cLoc, localNode, nodeData);
                        }));
                    }, true);
                }

                localNode.SetData(nodeData);
                LocalsTreeView.Nodes.Add(localNode);
            }
            LocalsTreeView.Nodes.Add(new CommonTools.Node(""));
        }
        private void RefreshTreeViews()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new VoidDelegate(RefreshTreeViews));
                return;
            }

            ArgumentsTreeView.Refresh();
            LocalsTreeView.Refresh();
        }
        private void LoadVariablesNode(Variable theVar, CommonTools.Node parent, object[] parentNodeData)
        {
            parentNodeData[2] = theVar.ToString();
            parent.SetData(parentNodeData);

            int i = 0;
            foreach (Variable aFieldVar in theVar.Fields)
            {
                CommonTools.Node newNode = new CommonTools.Node();

                object[] nodeData = new object[3];
                nodeData[0] = i++;
                nodeData[1] = aFieldVar.dbType.Signature;
                nodeData[2] = Utils.GetValueStr(aFieldVar.value, aFieldVar.dbType.Signature);
                newNode.SetData(nodeData);
                parent.Nodes.Add(newNode);

                LoadVariablesNode(aFieldVar, newNode, nodeData);
            }

            RefreshTreeViews();
        }
        
        private delegate void SetEnabledDelegate(bool enabled, Control aCtrl);
        private void SetEnabled(bool enabled, Control aCtrl)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetEnabledDelegate(SetEnabled), new object[] { enabled, aCtrl });
            }
            else
            {
                aCtrl.Enabled = enabled;
            }
        }
        
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            AddText("Terminating...");
            Terminating = true;

            Stop();

            Debug.Data.DebugDatabase.Dispose();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            AddText("Waiting for connection...");

            SetEnabled(false, ContinueButton);
            SetEnabled(false, StepNextButton);
            SetEnabled(false, BreakButton);
            SetEnabled(false, StartButton);
            SetEnabled(false, StopButton);
            SetEnabled(false, BreakOnStartCheckBox);

            string PipeBoxText = PipeBox.Text;
            string BuildPathBoxText = BuildPathBox.Text;

            Terminating = false;
            BreakOnStart = BreakOnStartCheckBox.Checked;

            new Task(delegate()
            {
                TheDebugger = new Debugger();
                TheDebugger.OnBreak += TheDebugger_OnBreak;
                TheDebugger.OnInvalidCommand += TheDebugger_OnInvalidCommand;
                if (!TheDebugger.Init(PipeBoxText, BuildPathBoxText))
                {
                    AddText("Failed to connect!");
                    SetEnabled(true, StartButton);
                }
                else
                {
                    SetEnabled(true, StopButton);
                    
                    TheDebugger.OnConnected += delegate()
                    {
                        WaitForCommand();
                        TheDebugger.EndInit();

                        if (BreakOnStart)
                        {
                            TheDebugger.BeginBreak();
                            SetEnabled(true, ContinueButton);
                        }
                        else
                        {
                            SetEnabled(true, BreakButton);
                        }

                        AddText("Connected.");
                        WaitForCommand();
                    };

                    theVM = new VMWare_VM();
                    theVM.PowerOn();
                }
            }).Start();
        }
        private void StopButton_Click(object sender, EventArgs e)
        {
            Stop();            
        }
        private void Stop()
        {
            AddText("Stopping...");
            Terminating = true;

            SetEnabled(false, StopButton);
            SetEnabled(true, BreakOnStartCheckBox);
            SetEnabled(true, StartButton);

            if (theVM != null && theVM.PoweredOn)
            {
                AddText("Stopping VM...");

                theVM.PowerOff();
                theVM = null;
            }
            if (TheDebugger != null)
            {
                AddText(string.Format("Stopped debugger : {0}", TheDebugger.Stop()));
                TheDebugger = null;
            }

            AddText("Stopped.");
            AddText("");
        }
        
        private void BreakButton_Click(object sender, EventArgs e)
        {
            StepToAddress = ulong.MaxValue;

            TheDebugger.BeginBreak();

            SetEnabled(true, ContinueButton);
            SetEnabled(false, StepNextButton);
            SetEnabled(false, StepNextILButton);
            SetEnabled(false, BreakButton);
            SetEnabled(false, StepToInt3Button);
        }
        private void ContinueButton_Click(object sender, EventArgs e)
        {
            if (TheDebugger.State == Debugger.States.Broken)
            {
                SetEnabled(false, ContinueButton);
                SetEnabled(false, StepNextButton);
                SetEnabled(false, StepNextILButton);
                SetEnabled(false, StepToInt3Button);

                AddText("Continue...");
                TheDebugger.Continue();
                AddText("");

                WaitForCommand();

                SetEnabled(true, BreakButton);
            }
            else
            {
                SetEnabled(false, ContinueButton);
                SetEnabled(false, StepNextButton);
                SetEnabled(false, StepNextILButton);

                AddText("Continue without break.");
                AddText("");
                
                SetEnabled(true, BreakButton);
            }
        }
        private void StepToInt3Button_Click(object sender, EventArgs e)
        {
            SetEnabled(false, ContinueButton);
            SetEnabled(false, StepNextButton);
            SetEnabled(false, StepNextILButton);
            SetEnabled(false, StepToInt3Button);
            SetEnabled(false, BreakButton);

            AddText("Step to Int3...");
            TheDebugger.Continue();
            TheDebugger.BeginBreak();
            AddText("");

            WaitForCommand();
        }
        private void StepNextButton_Click(object sender, EventArgs e)
        {
            SetEnabled(false, ContinueButton);
            SetEnabled(false, StepNextButton);
            SetEnabled(false, StepNextILButton);
            SetEnabled(false, StepToInt3Button);
            SetEnabled(false, BreakButton);

            AddText("Step next...");
            TheDebugger.StepNext();
            AddText("");

            WaitForCommand();
        }
        private void StepNextILButton_Click(object sender, EventArgs e)
        {
            SetEnabled(false, ContinueButton);
            SetEnabled(false, StepNextButton);
            SetEnabled(false, StepNextILButton);
            SetEnabled(false, StepToInt3Button);
            SetEnabled(false, BreakButton);

            AddText("Step next IL...");
            StepToAddress = TheDebugger.StepToNextIL();
            if (StepToAddress != ulong.MaxValue)
            {
                //+1 as the StepToAddress is the op after the Int3
                StepToAddress += 1;
                AddText(string.Format("Step to: {0:X8}", StepToAddress));
                AddText("");
                WaitForCommand();
            }
            else
            {
                AddText("Cannot step next IL.");

                SetEnabled(true, ContinueButton);
                SetEnabled(true, StepNextButton);
                SetEnabled(true, StepNextILButton);
                SetEnabled(true, StepToInt3Button);
            }
        }

        private void WaitForCommand()
        {
            new Task(delegate()
            {
                try
                {
                    while (TheDebugger == null && !Terminating)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                    if (!Terminating)
                    {
                        TheDebugger.WaitForCommand();
                    }
                }
                catch
                {
                }
            }).Start();
        }
        private void TheDebugger_OnBreak()
        {
            try
            {
                if (TheDebugger.State != Debugger.States.Breaking &&
                    TheDebugger.State != Debugger.States.Stepping)
                {
                    TheDebugger.Continue();
                    WaitForCommand();

                    SetEnabled(false, ContinueButton);
                    SetEnabled(false, StepNextButton);
                    SetEnabled(false, StepNextILButton);
                    SetEnabled(false, StepToInt3Button);
                    SetEnabled(true, BreakButton);

                    return;
                }
                
                AddText("");
                AddText("Break!");

                TheDebugger.GetBreakAddress();
                AddText(string.Format("Break address : 0x{0:X8}", TheDebugger.BreakAddress));

                if (TheDebugger.State == Debugger.States.Stepping &&
                    StepToAddress != ulong.MaxValue)
                {
                    if (TheDebugger.BreakAddress != StepToAddress)
                    {
                        TheDebugger.Continue();
                        TheDebugger.State = Debugger.States.Stepping;
                        WaitForCommand();

                        SetEnabled(false, ContinueButton);
                        SetEnabled(false, StepNextButton);
                        SetEnabled(false, StepNextILButton);
                        SetEnabled(false, StepToInt3Button);
                        SetEnabled(true, BreakButton);

                        return;
                    }
                    else
                    {
                        //-1 as the StepToAddress is the op after the Int3
                        TheDebugger.ClearInt3(StepToAddress - 1);
                        StepToAddress = ulong.MaxValue;
                    }
                }

                DB_Method currMethod = TheDebugger.CurrentMethod;
                if (currMethod != null)
                {
                    if (!currMethod.Plugged.HasValue || !currMethod.Plugged.Value)
                    {
                        ReloadCSText();
                    }

                    TheDebugger.GetArguments();
                    ReloadArguments();
                }

                TheDebugger.GetLocals();
                ReloadLocals();

                ReloadASMText();

                TheDebugger.GetRegisters();
                string regText = "";
                int num = 0;
                var registers = TheDebugger.Registers.OrderBy(x => x.register);
                foreach (Register aRegister in registers)
                {
                    regText += string.Format("{0} : 0x{1:X8}", aRegister.register.ToString(), aRegister.value);
                    regText += "    ";

                    num++;
                    if (num % 2 == 0)
                    {
                        regText += "\r\n";
                        num = 0;
                    }
                }
                SetText(regText, RegistersBox);
            }
            catch (Exception ex)
            {
                AddText("Error! " + ex.Message);
            }

            TheDebugger.EndBreak();

            SetEnabled(true, ContinueButton);
            SetEnabled(true, StepNextButton);
            SetEnabled(true, StepNextILButton);
            SetEnabled(true, StepToInt3Button);
        }
        private void TheDebugger_OnInvalidCommand(byte command)
        {
            AddText(string.Format("Invalid command received : {0}", command));

            SetEnabled(true, ContinueButton);
            SetEnabled(true, StepNextButton);
            SetEnabled(true, StepNextILButton);
            SetEnabled(true, BreakButton);
        }

    }
}
