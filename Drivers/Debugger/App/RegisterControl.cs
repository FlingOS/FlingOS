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
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace Drivers.Debugger.App
{
    public delegate void ValueChangedHandler(EventArgs e, object sender);

    public partial class RegisterControl : UserControl
    {
        [Description("Name of the register."), Category("Data")]
        public string Register
        {
            get { return RegisterNameLabel.Text; }
            set { RegisterNameLabel.Text = value; }
        }

        [Description("Value of the register."), Category("Data")]
        public uint Value
        {
            get
            {
                try
                {
                    return uint.Parse(ValueBox.Text, NumberStyles.HexNumber);
                }
                catch
                {
                    return 0;
                }
            }
            set { ValueBox.Text = value.ToString("X8"); }
        }

        public RegisterControl()
        {
            InitializeComponent();
        }

        [Description("Value changed event."), Category("Event")]
        public event ValueChangedHandler ValueChangedEvent;

        private void ValueBox_TextChanged(object sender, EventArgs e)
        {
            if (ValueChangedEvent != null)
            {
                try
                {
                    // Attempt the parse
                    uint.Parse(ValueBox.Text, NumberStyles.HexNumber);
                    // If the value is valid, we will reach this stage
                    ValueChangedEvent.Invoke(new EventArgs(), this);
                }
                catch
                {
                    // Otherwise, do nothing
                }
            }
        }
    }
}