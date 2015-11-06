namespace Drivers.Debugger.App
{
    partial class RegisterControl
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
            this.RegisterNameLabel = new System.Windows.Forms.Label();
            this.ValueBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // RegisterNameLabel
            // 
            this.RegisterNameLabel.AutoSize = true;
            this.RegisterNameLabel.Location = new System.Drawing.Point(5, 5);
            this.RegisterNameLabel.Name = "RegisterNameLabel";
            this.RegisterNameLabel.Size = new System.Drawing.Size(36, 13);
            this.RegisterNameLabel.TabIndex = 0;
            this.RegisterNameLabel.Text = "REG :";
            // 
            // ValueBox
            // 
            this.ValueBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ValueBox.Location = new System.Drawing.Point(46, 2);
            this.ValueBox.Name = "ValueBox";
            this.ValueBox.Size = new System.Drawing.Size(94, 20);
            this.ValueBox.TabIndex = 1;
            this.ValueBox.TextChanged += new System.EventHandler(this.ValueBox_TextChanged);
            // 
            // RegisterControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.ValueBox);
            this.Controls.Add(this.RegisterNameLabel);
            this.Name = "RegisterControl";
            this.Size = new System.Drawing.Size(143, 23);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label RegisterNameLabel;
        private System.Windows.Forms.TextBox ValueBox;
    }
}
