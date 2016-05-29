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
using System.Windows.Forms;

namespace Drivers.Debugger.App
{
    public delegate void RegisterChangedHandler(RegisterChangedEventArgs e, object sender);

    public partial class x86RegistersControl : UserControl
    {
        [Description("Value of the register."), Category("Data")]
        public uint EAX
        {
            get { return EAXRegisterControl.Value; }
            set { EAXRegisterControl.Value = value; }
        }

        [Description("Value of the register."), Category("Data")]
        public uint EBX
        {
            get { return EBXRegisterControl.Value; }
            set { EBXRegisterControl.Value = value; }
        }

        [Description("Value of the register."), Category("Data")]
        public uint ECX
        {
            get { return ECXRegisterControl.Value; }
            set { ECXRegisterControl.Value = value; }
        }

        [Description("Value of the register."), Category("Data")]
        public uint EDX
        {
            get { return EDXRegisterControl.Value; }
            set { EDXRegisterControl.Value = value; }
        }

        [Description("Value of the register."), Category("Data")]
        public uint ESP
        {
            get { return ESPRegisterControl.Value; }
            set { ESPRegisterControl.Value = value; }
        }

        [Description("Value of the register."), Category("Data")]
        public uint EBP
        {
            get { return EBPRegisterControl.Value; }
            set { EBPRegisterControl.Value = value; }
        }

        [Description("Value of the register."), Category("Data")]
        public uint EIP
        {
            get { return EIPRegisterControl.Value; }
            set { EIPRegisterControl.Value = value; }
        }

        public x86RegistersControl()
        {
            InitializeComponent();
        }

        [Description("Value changed event."), Category("Event")]
        public event RegisterChangedHandler RegisterChangedEvent;

        private void EAXRegisterControl_Load(object sender, EventArgs e)
        {
            if (RegisterChangedEvent != null)
            {
                RegisterChangedEvent.Invoke(new RegisterChangedEventArgs
                {
                    Register = "EAX",
                    Value = EAXRegisterControl.Value
                }, this);
            }
        }

        private void EBXRegisterControl_Load(object sender, EventArgs e)
        {
            if (RegisterChangedEvent != null)
            {
                RegisterChangedEvent.Invoke(new RegisterChangedEventArgs
                {
                    Register = "EBX",
                    Value = EBXRegisterControl.Value
                }, this);
            }
        }

        private void ECXRegisterControl_Load(object sender, EventArgs e)
        {
            if (RegisterChangedEvent != null)
            {
                RegisterChangedEvent.Invoke(new RegisterChangedEventArgs
                {
                    Register = "ECX",
                    Value = ECXRegisterControl.Value
                }, this);
            }
        }

        private void EDXRegisterControl_Load(object sender, EventArgs e)
        {
            if (RegisterChangedEvent != null)
            {
                RegisterChangedEvent.Invoke(new RegisterChangedEventArgs
                {
                    Register = "EDX",
                    Value = EDXRegisterControl.Value
                }, this);
            }
        }

        private void ESPRegisterControl_Load(object sender, EventArgs e)
        {
            if (RegisterChangedEvent != null)
            {
                RegisterChangedEvent.Invoke(new RegisterChangedEventArgs
                {
                    Register = "ESP",
                    Value = ESPRegisterControl.Value
                }, this);
            }
        }

        private void EBPRegisterControl_Load(object sender, EventArgs e)
        {
            if (RegisterChangedEvent != null)
            {
                RegisterChangedEvent.Invoke(new RegisterChangedEventArgs
                {
                    Register = "EBP",
                    Value = EBPRegisterControl.Value
                }, this);
            }
        }

        private void EIPRegisterControl_Load(object sender, EventArgs e)
        {
            if (RegisterChangedEvent != null)
            {
                RegisterChangedEvent.Invoke(new RegisterChangedEventArgs
                {
                    Register = "EIP",
                    Value = EIPRegisterControl.Value
                }, this);
            }
        }
    }

    public class RegisterChangedEventArgs : EventArgs
    {
        public string Register { get; set; }

        public uint Value { get; set; }
    }
}