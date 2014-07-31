namespace Kernel.Debug.Debugger.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("G:\\Fling OS\\VMWare\\FlingOS.vmx")]
        public string VMFilePath {
            get {
                return ((string)(this["VMFilePath"]));
            }
            set {
                this["VMFilePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Kernel.map")]
        public string ElfMapFileName {
            get {
                return ((string)(this["ElfMapFileName"]));
            }
            set {
                this["ElfMapFileName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Kernel.asm")]
        public string ASMFileName {
            get {
                return ((string)(this["ASMFileName"]));
            }
            set {
                this["ASMFileName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Kernel.dll")]
        public string KernelDLLFileName {
            get {
                return ((string)(this["KernelDLLFileName"]));
            }
            set {
                this["KernelDLLFileName"] = value;
            }
        }
    }
}
