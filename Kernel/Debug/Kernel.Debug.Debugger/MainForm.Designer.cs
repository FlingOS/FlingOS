namespace Kernel.Debug.Debugger
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            CommonTools.TreeListColumn treeListColumn1 = new CommonTools.TreeListColumn("Locals_IndexColumn", "Index");
            CommonTools.TreeListColumn treeListColumn2 = new CommonTools.TreeListColumn("Locals_TypeColumn", "Type");
            CommonTools.TreeListColumn treeListColumn3 = new CommonTools.TreeListColumn("Locals_ValueColumn", "Value");
            CommonTools.TreeListColumn treeListColumn4 = new CommonTools.TreeListColumn("Args_IndexColumn", "Index");
            CommonTools.TreeListColumn treeListColumn5 = new CommonTools.TreeListColumn("Args_TypeColumn", "Type");
            CommonTools.TreeListColumn treeListColumn6 = new CommonTools.TreeListColumn("Args_ValueColumn", "Value");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.StartButton = new System.Windows.Forms.Button();
            this.PipeBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.OutputBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.BuildPathBox = new System.Windows.Forms.TextBox();
            this.ContinueButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.CSBox = new FastColoredTextBoxNS.FastColoredTextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.RegistersBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.StepNextButton = new System.Windows.Forms.Button();
            this.ASMBox = new System.Windows.Forms.RichTextBox();
            this.BreakButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.BreakOnStartCheckBox = new System.Windows.Forms.CheckBox();
            this.StepToInt3Button = new System.Windows.Forms.Button();
            this.StepNextILButton = new System.Windows.Forms.Button();
            this.LocalsTreeView = new CommonTools.TreeListView();
            this.ArgumentsTreeView = new CommonTools.TreeListView();
            this.label9 = new System.Windows.Forms.Label();
            this.RunToAddressBox = new System.Windows.Forms.NumericUpDown();
            this.GoButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.CSBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LocalsTreeView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ArgumentsTreeView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RunToAddressBox)).BeginInit();
            this.SuspendLayout();
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(787, 15);
            this.StartButton.Margin = new System.Windows.Forms.Padding(4);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(100, 28);
            this.StartButton.TabIndex = 0;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // PipeBox
            // 
            this.PipeBox.Location = new System.Drawing.Point(69, 17);
            this.PipeBox.Margin = new System.Windows.Forms.Padding(4);
            this.PipeBox.Name = "PipeBox";
            this.PipeBox.Size = new System.Drawing.Size(259, 22);
            this.PipeBox.TabIndex = 1;
            this.PipeBox.Text = "flingos";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 21);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Pipe :";
            // 
            // OutputBox
            // 
            this.OutputBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.OutputBox.Location = new System.Drawing.Point(16, 98);
            this.OutputBox.Margin = new System.Windows.Forms.Padding(4);
            this.OutputBox.Multiline = true;
            this.OutputBox.Name = "OutputBox";
            this.OutputBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.OutputBox.Size = new System.Drawing.Size(531, 480);
            this.OutputBox.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(355, 21);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Build path :";
            // 
            // BuildPathBox
            // 
            this.BuildPathBox.Location = new System.Drawing.Point(443, 17);
            this.BuildPathBox.Margin = new System.Windows.Forms.Padding(4);
            this.BuildPathBox.Name = "BuildPathBox";
            this.BuildPathBox.Size = new System.Drawing.Size(335, 22);
            this.BuildPathBox.TabIndex = 5;
            this.BuildPathBox.Text = "G:\\Fling OS\\Fling OS\\Kernel\\Kernel\\bin\\Debug";
            // 
            // ContinueButton
            // 
            this.ContinueButton.Enabled = false;
            this.ContinueButton.Location = new System.Drawing.Point(1275, 15);
            this.ContinueButton.Margin = new System.Windows.Forms.Padding(4);
            this.ContinueButton.Name = "ContinueButton";
            this.ContinueButton.Size = new System.Drawing.Size(100, 28);
            this.ContinueButton.TabIndex = 6;
            this.ContinueButton.Text = "Continue";
            this.ContinueButton.UseVisualStyleBackColor = true;
            this.ContinueButton.Click += new System.EventHandler(this.ContinueButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(556, 79);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 17);
            this.label3.TabIndex = 8;
            this.label3.Text = "ASM :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 79);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 17);
            this.label4.TabIndex = 9;
            this.label4.Text = "Log :";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(1100, 79);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(33, 17);
            this.label5.TabIndex = 11;
            this.label5.Text = "C# :";
            // 
            // CSBox
            // 
            this.CSBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.CSBox.AutoScrollMinSize = new System.Drawing.Size(31, 18);
            this.CSBox.BackBrush = null;
            this.CSBox.BracketsHighlightStrategy = FastColoredTextBoxNS.BracketsHighlightStrategy.Strategy2;
            this.CSBox.CharHeight = 18;
            this.CSBox.CharWidth = 10;
            this.CSBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.CSBox.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.CSBox.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.CSBox.IsReplaceMode = false;
            this.CSBox.Language = FastColoredTextBoxNS.Language.CSharp;
            this.CSBox.LeftBracket = '(';
            this.CSBox.LeftBracket2 = '{';
            this.CSBox.Location = new System.Drawing.Point(1096, 98);
            this.CSBox.Margin = new System.Windows.Forms.Padding(4);
            this.CSBox.Name = "CSBox";
            this.CSBox.Paddings = new System.Windows.Forms.Padding(0);
            this.CSBox.RightBracket = ')';
            this.CSBox.RightBracket2 = '}';
            this.CSBox.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.CSBox.Size = new System.Drawing.Size(532, 481);
            this.CSBox.TabIndex = 10;
            this.CSBox.Zoom = 100;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 583);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 17);
            this.label6.TabIndex = 12;
            this.label6.Text = "Registers :";
            // 
            // RegistersBox
            // 
            this.RegistersBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RegistersBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RegistersBox.Location = new System.Drawing.Point(16, 603);
            this.RegistersBox.Margin = new System.Windows.Forms.Padding(4);
            this.RegistersBox.Multiline = true;
            this.RegistersBox.Name = "RegistersBox";
            this.RegistersBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.RegistersBox.Size = new System.Drawing.Size(531, 143);
            this.RegistersBox.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(556, 583);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(84, 17);
            this.label7.TabIndex = 14;
            this.label7.Text = "Arguments :";
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(1100, 583);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(57, 17);
            this.label8.TabIndex = 16;
            this.label8.Text = "Locals :";
            // 
            // StepNextButton
            // 
            this.StepNextButton.Enabled = false;
            this.StepNextButton.Location = new System.Drawing.Point(1491, 15);
            this.StepNextButton.Margin = new System.Windows.Forms.Padding(4);
            this.StepNextButton.Name = "StepNextButton";
            this.StepNextButton.Size = new System.Drawing.Size(100, 28);
            this.StepNextButton.TabIndex = 18;
            this.StepNextButton.Text = "Step Next";
            this.StepNextButton.UseVisualStyleBackColor = true;
            this.StepNextButton.Click += new System.EventHandler(this.StepNextButton_Click);
            // 
            // ASMBox
            // 
            this.ASMBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ASMBox.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ASMBox.Location = new System.Drawing.Point(556, 98);
            this.ASMBox.Margin = new System.Windows.Forms.Padding(4);
            this.ASMBox.Name = "ASMBox";
            this.ASMBox.Size = new System.Drawing.Size(531, 480);
            this.ASMBox.TabIndex = 19;
            this.ASMBox.Text = "";
            this.ASMBox.WordWrap = false;
            // 
            // BreakButton
            // 
            this.BreakButton.Enabled = false;
            this.BreakButton.Location = new System.Drawing.Point(1167, 15);
            this.BreakButton.Margin = new System.Windows.Forms.Padding(4);
            this.BreakButton.Name = "BreakButton";
            this.BreakButton.Size = new System.Drawing.Size(100, 28);
            this.BreakButton.TabIndex = 20;
            this.BreakButton.Text = "Break";
            this.BreakButton.UseVisualStyleBackColor = true;
            this.BreakButton.Click += new System.EventHandler(this.BreakButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.Enabled = false;
            this.StopButton.Location = new System.Drawing.Point(895, 15);
            this.StopButton.Margin = new System.Windows.Forms.Padding(4);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(100, 28);
            this.StopButton.TabIndex = 21;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // BreakOnStartCheckBox
            // 
            this.BreakOnStartCheckBox.AutoSize = true;
            this.BreakOnStartCheckBox.Checked = true;
            this.BreakOnStartCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.BreakOnStartCheckBox.Location = new System.Drawing.Point(1036, 20);
            this.BreakOnStartCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.BreakOnStartCheckBox.Name = "BreakOnStartCheckBox";
            this.BreakOnStartCheckBox.Size = new System.Drawing.Size(119, 21);
            this.BreakOnStartCheckBox.TabIndex = 22;
            this.BreakOnStartCheckBox.Text = "Break on start";
            this.BreakOnStartCheckBox.UseVisualStyleBackColor = true;
            // 
            // StepToInt3Button
            // 
            this.StepToInt3Button.Enabled = false;
            this.StepToInt3Button.Location = new System.Drawing.Point(1383, 15);
            this.StepToInt3Button.Margin = new System.Windows.Forms.Padding(4);
            this.StepToInt3Button.Name = "StepToInt3Button";
            this.StepToInt3Button.Size = new System.Drawing.Size(100, 28);
            this.StepToInt3Button.TabIndex = 23;
            this.StepToInt3Button.Text = "Step to Int3";
            this.StepToInt3Button.UseVisualStyleBackColor = true;
            this.StepToInt3Button.Click += new System.EventHandler(this.StepToInt3Button_Click);
            // 
            // StepNextILButton
            // 
            this.StepNextILButton.Enabled = false;
            this.StepNextILButton.Location = new System.Drawing.Point(1491, 50);
            this.StepNextILButton.Margin = new System.Windows.Forms.Padding(4);
            this.StepNextILButton.Name = "StepNextILButton";
            this.StepNextILButton.Size = new System.Drawing.Size(100, 28);
            this.StepNextILButton.TabIndex = 24;
            this.StepNextILButton.Text = "Step Next IL";
            this.StepNextILButton.UseVisualStyleBackColor = true;
            this.StepNextILButton.Click += new System.EventHandler(this.StepNextILButton_Click);
            // 
            // LocalsTreeView
            // 
            treeListColumn1.AutoSizeMinSize = 0;
            treeListColumn1.Width = 50;
            treeListColumn2.AutoSizeMinSize = 0;
            treeListColumn2.Width = 125;
            treeListColumn3.AutoSizeMinSize = 0;
            treeListColumn3.Width = 200;
            this.LocalsTreeView.Columns.AddRange(new CommonTools.TreeListColumn[] {
            treeListColumn1,
            treeListColumn2,
            treeListColumn3});
            this.LocalsTreeView.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.LocalsTreeView.Images = null;
            this.LocalsTreeView.Location = new System.Drawing.Point(1096, 603);
            this.LocalsTreeView.Margin = new System.Windows.Forms.Padding(4);
            this.LocalsTreeView.Name = "LocalsTreeView";
            this.LocalsTreeView.Size = new System.Drawing.Size(532, 271);
            this.LocalsTreeView.TabIndex = 25;
            this.LocalsTreeView.ViewOptions.Indent = 10;
            // 
            // ArgumentsTreeView
            // 
            treeListColumn4.AutoSizeMinSize = 0;
            treeListColumn4.Width = 50;
            treeListColumn5.AutoSizeMinSize = 0;
            treeListColumn5.Width = 125;
            treeListColumn6.AutoSizeMinSize = 0;
            treeListColumn6.Width = 200;
            this.ArgumentsTreeView.Columns.AddRange(new CommonTools.TreeListColumn[] {
            treeListColumn4,
            treeListColumn5,
            treeListColumn6});
            this.ArgumentsTreeView.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ArgumentsTreeView.Images = null;
            this.ArgumentsTreeView.Location = new System.Drawing.Point(556, 603);
            this.ArgumentsTreeView.Margin = new System.Windows.Forms.Padding(4);
            this.ArgumentsTreeView.Name = "ArgumentsTreeView";
            this.ArgumentsTreeView.Size = new System.Drawing.Size(532, 271);
            this.ArgumentsTreeView.TabIndex = 26;
            this.ArgumentsTreeView.ViewOptions.Indent = 10;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(886, 56);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(109, 17);
            this.label9.TabIndex = 27;
            this.label9.Text = "Run to address:";
            // 
            // RunToAddressBox
            // 
            this.RunToAddressBox.Location = new System.Drawing.Point(1001, 54);
            this.RunToAddressBox.Maximum = new decimal(new int[] {
            -1,
            0,
            0,
            0});
            this.RunToAddressBox.Name = "RunToAddressBox";
            this.RunToAddressBox.Size = new System.Drawing.Size(120, 22);
            this.RunToAddressBox.TabIndex = 28;
            // 
            // GoButton
            // 
            this.GoButton.Location = new System.Drawing.Point(1127, 53);
            this.GoButton.Name = "GoButton";
            this.GoButton.Size = new System.Drawing.Size(75, 23);
            this.GoButton.TabIndex = 29;
            this.GoButton.Text = "Go";
            this.GoButton.UseVisualStyleBackColor = true;
            this.GoButton.Click += new System.EventHandler(this.GoButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1644, 895);
            this.Controls.Add(this.GoButton);
            this.Controls.Add(this.RunToAddressBox);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.ArgumentsTreeView);
            this.Controls.Add(this.LocalsTreeView);
            this.Controls.Add(this.StepNextILButton);
            this.Controls.Add(this.StepToInt3Button);
            this.Controls.Add(this.BreakOnStartCheckBox);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.BreakButton);
            this.Controls.Add(this.ASMBox);
            this.Controls.Add(this.StepNextButton);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.RegistersBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.CSBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ContinueButton);
            this.Controls.Add(this.BuildPathBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.OutputBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PipeBox);
            this.Controls.Add(this.StartButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FlingOS Kernel Debugger";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.CSBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LocalsTreeView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ArgumentsTreeView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RunToAddressBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.TextBox PipeBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox OutputBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox BuildPathBox;
        private System.Windows.Forms.Button ContinueButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private FastColoredTextBoxNS.FastColoredTextBox CSBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox RegistersBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button StepNextButton;
        private System.Windows.Forms.RichTextBox ASMBox;
        private System.Windows.Forms.Button BreakButton;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.CheckBox BreakOnStartCheckBox;
        private System.Windows.Forms.Button StepToInt3Button;
        private System.Windows.Forms.Button StepNextILButton;
        private CommonTools.TreeListView LocalsTreeView;
        private CommonTools.TreeListView ArgumentsTreeView;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown RunToAddressBox;
        private System.Windows.Forms.Button GoButton;
    }
}

