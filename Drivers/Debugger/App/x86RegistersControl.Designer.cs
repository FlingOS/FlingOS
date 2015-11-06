namespace Drivers.Debugger.App
{
    partial class x86RegistersControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.EIPRegisterControl = new Drivers.Debugger.App.RegisterControl();
            this.EBPRegisterControl = new Drivers.Debugger.App.RegisterControl();
            this.ESPRegisterControl = new Drivers.Debugger.App.RegisterControl();
            this.EDXRegisterControl = new Drivers.Debugger.App.RegisterControl();
            this.ECXRegisterControl = new Drivers.Debugger.App.RegisterControl();
            this.EBXRegisterControl = new Drivers.Debugger.App.RegisterControl();
            this.EAXRegisterControl = new Drivers.Debugger.App.RegisterControl();
            this.SuspendLayout();
            // 
            // EIPRegisterControl
            // 
            this.EIPRegisterControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.EIPRegisterControl.Location = new System.Drawing.Point(139, 70);
            this.EIPRegisterControl.Name = "EIPRegisterControl";
            this.EIPRegisterControl.Register = "EIP :";
            this.EIPRegisterControl.Size = new System.Drawing.Size(113, 27);
            this.EIPRegisterControl.TabIndex = 6;
            this.EIPRegisterControl.Value = ((uint)(0u));
            this.EIPRegisterControl.Load += new System.EventHandler(this.EIPRegisterControl_Load);
            // 
            // EBPRegisterControl
            // 
            this.EBPRegisterControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.EBPRegisterControl.Location = new System.Drawing.Point(139, 37);
            this.EBPRegisterControl.Name = "EBPRegisterControl";
            this.EBPRegisterControl.Register = "EBP :";
            this.EBPRegisterControl.Size = new System.Drawing.Size(113, 27);
            this.EBPRegisterControl.TabIndex = 5;
            this.EBPRegisterControl.Value = ((uint)(0u));
            this.EBPRegisterControl.Load += new System.EventHandler(this.EBPRegisterControl_Load);
            // 
            // ESPRegisterControl
            // 
            this.ESPRegisterControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ESPRegisterControl.Location = new System.Drawing.Point(139, 4);
            this.ESPRegisterControl.Name = "ESPRegisterControl";
            this.ESPRegisterControl.Register = "ESP :";
            this.ESPRegisterControl.Size = new System.Drawing.Size(113, 27);
            this.ESPRegisterControl.TabIndex = 4;
            this.ESPRegisterControl.Value = ((uint)(0u));
            this.ESPRegisterControl.Load += new System.EventHandler(this.ESPRegisterControl_Load);
            // 
            // EDXRegisterControl
            // 
            this.EDXRegisterControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.EDXRegisterControl.Location = new System.Drawing.Point(4, 103);
            this.EDXRegisterControl.Name = "EDXRegisterControl";
            this.EDXRegisterControl.Register = "EDX :";
            this.EDXRegisterControl.Size = new System.Drawing.Size(113, 27);
            this.EDXRegisterControl.TabIndex = 3;
            this.EDXRegisterControl.Value = ((uint)(0u));
            this.EDXRegisterControl.Load += new System.EventHandler(this.EDXRegisterControl_Load);
            // 
            // ECXRegisterControl
            // 
            this.ECXRegisterControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ECXRegisterControl.Location = new System.Drawing.Point(4, 70);
            this.ECXRegisterControl.Name = "ECXRegisterControl";
            this.ECXRegisterControl.Register = "ECX :";
            this.ECXRegisterControl.Size = new System.Drawing.Size(113, 27);
            this.ECXRegisterControl.TabIndex = 2;
            this.ECXRegisterControl.Value = ((uint)(0u));
            this.ECXRegisterControl.Load += new System.EventHandler(this.ECXRegisterControl_Load);
            // 
            // EBXRegisterControl
            // 
            this.EBXRegisterControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.EBXRegisterControl.Location = new System.Drawing.Point(4, 37);
            this.EBXRegisterControl.Name = "EBXRegisterControl";
            this.EBXRegisterControl.Register = "EBX :";
            this.EBXRegisterControl.Size = new System.Drawing.Size(113, 27);
            this.EBXRegisterControl.TabIndex = 1;
            this.EBXRegisterControl.Value = ((uint)(0u));
            this.EBXRegisterControl.Load += new System.EventHandler(this.EBXRegisterControl_Load);
            // 
            // EAXRegisterControl
            // 
            this.EAXRegisterControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.EAXRegisterControl.Location = new System.Drawing.Point(4, 4);
            this.EAXRegisterControl.Name = "EAXRegisterControl";
            this.EAXRegisterControl.Register = "EAX :";
            this.EAXRegisterControl.Size = new System.Drawing.Size(113, 27);
            this.EAXRegisterControl.TabIndex = 0;
            this.EAXRegisterControl.Value = ((uint)(0u));
            this.EAXRegisterControl.Load += new System.EventHandler(this.EAXRegisterControl_Load);
            // 
            // x86RegistersControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.EIPRegisterControl);
            this.Controls.Add(this.EBPRegisterControl);
            this.Controls.Add(this.ESPRegisterControl);
            this.Controls.Add(this.EDXRegisterControl);
            this.Controls.Add(this.ECXRegisterControl);
            this.Controls.Add(this.EBXRegisterControl);
            this.Controls.Add(this.EAXRegisterControl);
            this.Name = "x86RegistersControl";
            this.Size = new System.Drawing.Size(257, 137);
            this.ResumeLayout(false);

        }

        #endregion

        private RegisterControl EAXRegisterControl;
        private RegisterControl EBXRegisterControl;
        private RegisterControl ECXRegisterControl;
        private RegisterControl EDXRegisterControl;
        private RegisterControl ESPRegisterControl;
        private RegisterControl EBPRegisterControl;
        private RegisterControl EIPRegisterControl;
    }
}
