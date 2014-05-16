using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kernel.Debug.Data;
using System.IO;
using Kernel.Compiler;

namespace Kernel.Debug.Debugger
{
    /// <summary>
    /// The list of basic debug commands used to debug the kernel.
    /// </summary>
    /// <remarks>
    /// Duplicated in Commands.*.asm file - any updates to this list
    /// must also be done to the ASM.
    /// </remarks>
    public enum DebugCommands : byte
    {
        /// <summary>
        /// Break command
        /// </summary>
        Break = 1,
        /// <summary>
        /// Continue command
        /// </summary>
        Continue = 2,
        /// <summary>
        /// StepNext command
        /// </summary>
        StepNext = 3,
        /// <summary>
        /// GetBreakAddress command
        /// </summary>
        GetBreakAddress = 4,
        /// <summary>
        /// SendBreakAddress command
        /// </summary>
        SendBreakAddress = 5,
        /// <summary>
        /// GetRegisters command
        /// </summary>
        GetRegisters = 6,
        /// <summary>
        /// SendRegisters command
        /// </summary>
        SendRegisters = 7,
        /// <summary>
        /// GetArguments command
        /// </summary>
        GetArguments = 8,
        /// <summary>
        /// SendArguments command
        /// </summary>
        SendArguments = 9,
        /// <summary>
        /// GetLocals command
        /// </summary>
        GetLocals = 10,
        /// <summary>
        /// SendLocals command
        /// </summary>
        SendLocals = 11,
        /// <summary>
        /// Message command
        /// </summary>
        Message = 12,
        /// <summary>
        /// Set Int3 command
        /// </summary>
        SetInt3 = 13,
        /// <summary>
        /// Clear Int3 command
        /// </summary>
        ClearInt3 = 14,
        /// <summary>
        /// GetMemory command
        /// </summary>
        GetMemory = 15,
        /// <summary>
        /// SendMemory command
        /// </summary>
        SendMemory = 16,
        /// <summary>
        /// Connected command (notification)
        /// </summary>
        Connected = 17
    }

    /// <summary>
    /// Handler for the OnBreak event.
    /// </summary>
    public delegate void BreakHandler();
    /// <summary>
    /// Handler for the OnInvalidCommand event.
    /// </summary>
    /// <param name="command">The invalid command byte.</param>
    public delegate void InvalidCommandHandler(byte command);

    /// <summary>
    /// The main class for handling debug communications with the Fling OS kernel.
    /// </summary>
    public sealed class Debugger : IDisposable
    {
        /// <summary>
        /// The serial connection to the kernel.
        /// </summary>
        private Serial TheSerial;

        /// <summary>
        /// The Elf Map for the kernel being debugged.
        /// </summary>
        private ElfMap TheElfMap;

        /// <summary>
        /// The states the deugger can be in.
        /// </summary>
        public enum States
        {
            None,
            Breaking,
            Stepping,
            Broken,
            Running,
            Stopping
        }
        /// <summary>
        /// The current debugger state.
        /// </summary>
        public  States State = States.None;

        private bool Mode_64bit = false;

        /// <summary>
        /// The address of the last instruction executed before the break.
        /// </summary>
        private UInt64 breakAddress;
        /// <summary>
        /// The address of the last instruction executed before the break.
        /// </summary>
        public UInt64 BreakAddress
        {
            get
            {
                return breakAddress;
            }
        }

        /// <summary>
        /// The nearest labels to the break address.
        /// </summary>
        private List<string> currentNearestLabels;
        /// <summary>
        /// The nearest labels to the break address.
        /// </summary>
        public List<string> CurrentNearestLabels
        {
            get
            {
                if (currentNearestLabels == null)
                {
                    currentNearestLabels = TheElfMap.GetNearestLabels(breakAddress);
                }
                return currentNearestLabels;
            }
        }

        /// <summary>
        /// The nearest label to the break address that is a method-based label e.g.
        /// "method_METHODID.IL_OPNUMBER_ASMNUMBER"
        /// </summary>
        private string currentNearestMethodBasedLabel;
        /// <summary>
        /// The nearest label to the break address that is a method-based label e.g.
        /// "method_METHODID.IL_OPNUMBER_ASMNUMBER"
        /// </summary>
        public string CurrentNearestMethodBasedLabel
        {
            get
            {
                if (currentNearestMethodBasedLabel == null && CurrentNearestLabels != null)
                {
                    foreach (string aLabel in CurrentNearestLabels)
                    {
                        //Look for one beggining with "method_"
                        if (aLabel.StartsWith("method_"))
                        {
                            if(aLabel.EndsWith("True") ||
                               aLabel.EndsWith("Else") ||
                               aLabel.EndsWith("End")  ||
                               aLabel.EndsWith("Skip"))
                            {
                                continue;
                            }
                            currentNearestMethodBasedLabel = aLabel;
                            break;
                        }
                    }
                }
                return currentNearestMethodBasedLabel;
            }
        }

        /// <summary>
        /// The nearest label to the break address that is a method label e.g.
        /// "method_METHODID"
        /// </summary>
        public string CurrentNearestMethodLabel
        {
            get
            {
                if (CurrentNearestMethodBasedLabel != null)
                {
                    return CurrentNearestMethodBasedLabel.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                }
                return null;
            }
        }

        /// <summary>
        /// The IL Op info for the current line assuming we are not in a plugged method.
        /// </summary>
        private DB_ILOpInfo currentILOpInfo;
        /// <summary>
        /// The IL Op info for the current line assuming we are not in a plugged method.
        /// </summary>
        public DB_ILOpInfo CurrentILOpInfo
        {
            get
            {
                if (currentILOpInfo == null && currentMethod != null && (!currentMethod.Plugged.HasValue || !currentMethod.Plugged.Value))
                {
                    LoadCurrentIlOp();
                }
                return currentILOpInfo;
            }
        }
        /// <summary>
        /// The last IL Op info that was found (if any).
        /// </summary>
        private DB_ILOpInfo lastILOpInfo;
        /// <summary>
        /// The last IL Op info that was found (if any).
        /// </summary>
        public DB_ILOpInfo LastILOpInfo
        {
            get
            {
                return lastILOpInfo;
            }
        }
        
        /// <summary>
        /// The offset from the start of the method ASM to the current ASM line
        /// </summary>
        public int CurrentASMLineStartOffset
        {
            get
            {
                if(CurrentILOpInfo != null)
                {
                    string currLabel = CurrentNearestMethodBasedLabel;
                    int ASMLineNum = int.Parse(currLabel.Split('.')[1].Split('_')[2]);

                    string currASM = TheASMFile.GetILOpASM(CurrentILOpInfo);
                    //+3 for ":\r\n" at end of the label...
                    //We guess that line immediately after label is code 
                    // - properly sorted for display by MainForm later
                    int totalOffset = CurrentILOpInfo.ASMStartPos + currASM.IndexOf(currLabel) + currLabel.Length + 3;
                    return totalOffset;
                }
                return -1;
            }
        }
        /// <summary>
        /// The length of the current ASM line.
        /// </summary>
        public int CurrentASMLineLength
        {
            get
            {
                if (CurrentILOpInfo != null)
                {
                    string currLabel = CurrentNearestMethodBasedLabel;
                    int ASMLineNum = int.Parse(currLabel.Split('.')[1].Split('_')[2]);

                    string currASM = TheASMFile.GetILOpASM(CurrentILOpInfo);
                    string[] currASMLines = currASM.Replace("\r", "").Split('\n');

                    int asmLineNum = 0;
                    for (int i = 0; i < currASMLines.Length; i++)
                    {
                        string currLine = currASMLines[i].Trim();
                        if (!currLine.StartsWith(";"))
                        {
                            if (asmLineNum == ASMLineNum)
                            {
                                return currLine.Length;
                            }
                            asmLineNum++;
                        }
                    }
                }
                return -1;
            }
        }

        /// <summary>
        /// The data of the registers when execution was broken.
        /// </summary>
        private List<Register> registers;
        /// <summary>
        /// The data of the registers when execution was broken.
        /// </summary>
        public List<Register> Registers
        {
            get
            {
                return registers;
            }
        }

        /// <summary>
        /// The current method determined from break address.
        /// </summary>
        private DB_Method currentMethod;
        /// <summary>
        /// The current method determined from break address.
        /// </summary>
        public DB_Method CurrentMethod
        {
            get
            {
                if (currentMethod == null)
                {
                    TryLoadCurrentUnpluggedMethod();

                    if (currentMethod == null)
                    {
                        TryLoadCurrentPluggedMethod();
                    }
                }
                return currentMethod;
            }
        }

        /// <summary>
        /// The current method's ASM text.
        /// </summary>
        private string currentMethodASM = null;
        /// <summary>
        /// The current method's ASM text.
        /// </summary>
        public string CurrentMethodASM
        {
            get
            {
                if (currentMethod != null && currentMethodASM == null)
                {
                    if (currentMethod.Plugged.HasValue && currentMethod.Plugged.Value)
                    {
                        LoadCurrentMethodASM_Plugged();
                    }
                    else
                    {
                        LoadCurrentMethodASM_Unplugged();
                    }
                }
                return currentMethodASM;
            }
        }

        /// <summary>
        /// The current C# line info.
        /// </summary>
        private PDB_LineInfo currentCSLine;
        /// <summary>
        /// The current C# line info.
        /// </summary>
        public PDB_LineInfo CurrentCSLine
        {
            get
            {
                if(currentCSLine == null)
                {
                    LoadCurrentMethodCS();
                }
                return currentCSLine;
            }
        }
        /// <summary>
        /// The current C# method info.
        /// </summary>
        private PDB_MethodInfo currentCSMethod;
        /// <summary>
        /// The current C# method info.
        /// </summary>
        public PDB_MethodInfo CurrentCSMethod
        {
            get
            {
                if (currentCSMethod == null)
                {
                    LoadCurrentMethodCS();
                }
                return currentCSMethod;
            }
        }
        /// <summary>
        /// The current C# symbol info.
        /// </summary>
        private PDB_SymbolInfo currentCSSymbol;
        /// <summary>
        /// The current C# method info.
        /// </summary>
        public PDB_SymbolInfo CurrentCSSymbol
        {
            get
            {
                if (currentCSSymbol == null)
                {
                    LoadCurrentMethodCS();
                }
                return currentCSSymbol;
            }
        }
        
        /// <summary>
        /// The current method's arguments.
        /// </summary>
        private List<Argument> arguments;
        /// <summary>
        /// The current method's arguments.
        /// </summary>
        public List<Argument> Arguments
        {
            get
            {
                return arguments;
            }
        }

        /// <summary>
        /// The current method's locals.
        /// </summary>
        private List<Local> locals;
        /// <summary>
        /// The current method's locals.
        /// </summary>
        public List<Local> Locals
        {
            get
            {
                return locals;
            }
        }
        
        /// <summary>
        /// The kernel's main assembler file.
        /// </summary>
        public ASMFile TheASMFile;

        /// <summary>
        /// The PDB dump manager used to access IL to C# mapping info.
        /// </summary>
        public PDBDumpManager ThePDBDumpManager;

        /// <summary>
        /// Fired when the Break command is received.
        /// </summary>
        public event BreakHandler OnBreak;

        /// <summary>
        /// Fired when an invalid command is received.
        /// </summary>
        public event InvalidCommandHandler OnInvalidCommand;

        /// <summary>
        /// Fired when the debugger first becomes connected.
        /// </summary>
        public event OnConnectedHandler OnConnected;


        /// <summary>
        /// Initialises a new instance on the debugger but does not connect 
        /// it to the kernel. Use <see cref="Init"/>.
        /// </summary>
        public Debugger()
        {
            TheSerial = new Serial();
        }
        /// <summary>
        /// Disposes of the debugger instance cleanly.
        /// </summary>
        public void Dispose()
        {
            State = States.Stopping;
            TheSerial.Dispose();
            TheSerial = null;
            GC.SuppressFinalize(this);
        }

        private bool sendConnectValue = true;

        /// <summary>
        /// Initialises the debugger, connects to the specified pipe and loads debug info
        /// from the build directory.
        /// </summary>
        /// <param name="pipe">The name of the pipe (excl. "\\.\pipe\")
        /// to connect to.</param>
        /// <param name="buildFolder">The folder path that the kernel was built to.</param>
        /// <returns>True if connected. Otherwise false.</returns>
        public bool Init(string pipe, string buildFolder)
        {
            bool OK = true;

            try
            {
                string elfMapName = Path.Combine(buildFolder, Properties.Settings.Default.ElfMapFileName);
                TheElfMap = new ElfMap(elfMapName);
                string asmName = Path.Combine(buildFolder, Properties.Settings.Default.ASMFileName);
                TheASMFile = new ASMFile(asmName);
                string dllFileName = Path.Combine(buildFolder, Properties.Settings.Default.KernelDLLFileName);
                ThePDBDumpManager = new PDBDumpManager(dllFileName);

                TheSerial.OnConnected += delegate()
                {
                    State = States.Running;

                    new Task(LoadMemoryValue_Run).Start();
                    
                    OnConnected();
                };

                if (OK)
                {
                    OK = TheSerial.Init(pipe);
                }
            }
            catch
            {
                OK = false;
            }
          
            return OK;
        }
        /// <summary>
        /// Finishes off connecting to the OS.
        /// </summary>
        public void EndInit()
        {
            int tries = 20;
            while (sendConnectValue && tries-- > 0)
            {
                TheSerial.Write(0xDEADBEEF);
                Thread.Sleep(100);
            }
        }
        /// <summary>
        /// Stops the debugger and closes the connection.
        /// </summary>
        /// <returns>True if debugger is succesfully stopped.</returns>
        public bool Stop()
        {
            State = States.Stopping;

            bool OK = TheSerial.Disconnect();
            TheSerial.Dispose();
            TheSerial = null;
            return OK;
        }

        /// <summary>
        /// Waits for and then handles an incoming command from the kernel
        /// </summary>
        public void WaitForCommand()
        {
            byte cmdByte = 0;
            do
            {
                cmdByte = TheSerial.ReadBytes(1)[0];
            }
            while (cmdByte == 0 && State != States.Stopping);

            if (TheSerial.Connected)
            {
                HandleCommand(cmdByte);
            }
        }

        /// <summary>
        /// Handles the specified command as though it were received from the OS being debugged.
        /// </summary>
        /// <param name="cmdByte">The command to process.</param>
        private void HandleCommand(byte cmdByte)
        {
            HandleCommand((DebugCommands)cmdByte);
        }
        /// <summary>
        /// Handles the specified command as though it were received from the OS being debugged.
        /// </summary>
        /// <param name="cmd">The command to process.</param>
        private void HandleCommand(DebugCommands cmd)
        {
            switch (cmd)
            {
                case DebugCommands.Break:
                    Handle_BreakCmd();
                    break;
                case DebugCommands.Continue:
                    Handle_ContinueCmd();
                    break;
                case DebugCommands.GetArguments:
                    Handle_GetArgumentsCmd();
                    break;
                case DebugCommands.GetBreakAddress:
                    Handle_GetBreakAddressCmd();
                    break;
                case DebugCommands.GetLocals:
                    Handle_GetLocalsCmd();
                    break;
                case DebugCommands.GetRegisters:
                    Handle_GetRegistersCmd();
                    break;
                case DebugCommands.Message:
                    Handle_MessageCmd();
                    break;
                case DebugCommands.SendArguments:
                    Handle_SendArgumentsCmd();
                    break;
                case DebugCommands.SendBreakAddress:
                    Handle_SendBreakAddressCmd();
                    break;
                case DebugCommands.SendLocals:
                    Handle_SendLocalsCmd();
                    break;
                case DebugCommands.SendRegisters:
                    Handle_SendRegistersCmd();
                    break;
                case DebugCommands.StepNext:
                    Handle_StepNextCmd();
                    break;
                case DebugCommands.SetInt3:
                    Handle_SetInt3Cmd();
                    break;
                case DebugCommands.ClearInt3:
                    Handle_ClearInt3Cmd();
                    break;
                case DebugCommands.GetMemory:
                    Handle_GetMemoryCmd();
                    break;
                case DebugCommands.SendMemory:
                    Handle_SendMemoryCmd();
                    break;
                case DebugCommands.Connected:
                    Handle_ConnectedCmd();
                    break;
                default:
                    OnInvalidCommand((byte)cmd);
                    break;
            }
        }

        /// <summary>
        /// Loads the current method from the debug database 
        /// based on the break address.
        /// </summary>
        private void TryLoadCurrentUnpluggedMethod()
        {
            //Get the method's ID (label) from Elf Map
            //Get the method entry from debug database based on method's ID

            string theLabel = CurrentNearestMethodLabel;
            if (theLabel != null)
            {
                currentMethod = DebugDatabase.GetMethod(theLabel);
            }
            else
            {
                currentMethod = null;
            }
        }
        /// <summary>
        /// Loads the current method's ASM assuming it is not plugged.
        /// </summary>
        private void LoadCurrentMethodASM_Unplugged()
        {
            if (currentMethod != null)
            {
                currentMethodASM = TheASMFile.GetMethodASM(currentMethod);
            }
        }
        /// <summary>
        /// Loads the current line's IL Op info (assuming we aren't in a plugged method)
        /// </summary>
        private void LoadCurrentIlOp()
        {
            if(currentMethod != null && (!currentMethod.Plugged.HasValue || !currentMethod.Plugged.Value))
            {
                string currLabel = currentNearestMethodBasedLabel;
                try
                {
                    int pos = int.Parse(currLabel.Split('.')[1].Split('_')[1]);

                    IEnumerable<DB_ILOpInfo> PotIlOps = (from infos in currentMethod.DB_ILOpInfos
                                                         where (infos.Position == pos)
                                                         select infos);

                    if (PotIlOps.Count() > 0)
                    {
                        if (currentILOpInfo != null)
                        {
                            lastILOpInfo = currentILOpInfo;
                        }
                        currentILOpInfo = PotIlOps.First();
                    }
                }
                catch(IndexOutOfRangeException)
                {
                    //Not an unplugged method (somehow?) so not a valid name 
                    //so splitting fails
                    //Just ignore

                    if (currentILOpInfo != null)
                    {
                        lastILOpInfo = currentILOpInfo;
                    }
                    currentILOpInfo = null;
                }
            }
        }
        /// <summary>
        /// Loads the current method's C# code (if it isn't plugged).
        /// </summary>
        private void LoadCurrentMethodCS()
        {
            if(CurrentILOpInfo != null)
            {
                string methodSig = CurrentMethod.MethodSignature;
                string[] reversedSig = Kernel.Compiler.Utils.ReverseMethodSignature(methodSig);

                string SymbolName = reversedSig[1];
                string MethodName = reversedSig[2];

                try
                {
                    currentCSSymbol= ThePDBDumpManager.Symbols[SymbolName];
                    //TODO - Choosing First here is wrong as one function name can be overridden.
                    //       We need to fix this so we can properly identify which override we are 
                    //       executing.
                    currentCSMethod = (from methods in currentCSSymbol.Methods
                                       where methods.FunctionName == MethodName
                                       select methods).First();
                    currentCSLine = null;

                    for (int i = 0; i < currentCSMethod.Lines.Count; i++)
                    {
                        PDB_LineInfo testLine = currentCSMethod.Lines[i];
                        if(CurrentILOpInfo.Position >= testLine.ILStartNum &&
                            CurrentILOpInfo.Position <= testLine.ILEndNum)
                        {
                            currentCSLine = testLine;
                            break;
                        }
                    }
                }
                catch
                {
                    currentCSLine = null;
                    currentCSSymbol = null;
                    currentCSMethod = null;
                }
            }
            else
            {
                currentCSLine = null;
                currentCSSymbol = null;
                currentCSMethod = null;
            }
        }

        /// <summary>
        /// Loads the current method from the debug database 
        /// based on the break address.
        /// </summary>
        private void TryLoadCurrentPluggedMethod()
        {
            //Get the method's nearest label from Elf Map
            //Get the method entry from debug database based on method's ID

            string methodID = null;

            foreach (string theLabel in CurrentNearestLabels)
            {
                int index = TheASMFile.ASM.IndexOf(theLabel + ":");
                if (index > -1)
                {
                    //-2 for \r\n
                    for (int i = index - 2; i > -1; )
                    {
                        //-1 for previously found \n
                        int prevNewLineIndex = TheASMFile.ASM.LastIndexOf("\n", i - 1);
                        int endIndex = i;

                        i = prevNewLineIndex;

                        string currLine = TheASMFile.ASM.Substring(prevNewLineIndex, endIndex - prevNewLineIndex).Trim();
                        if(currLine.StartsWith("; Method ID : "))
                        {
                            methodID = currLine.Substring("; Method ID : ".Length);
                            break;
                        }
                    }
                }
            }

            if (methodID != null)
            {
                currentMethod = DebugDatabase.GetMethod(methodID);
            }
            else
            {
                currentMethod = null;
            }
        }
        /// <summary>
        /// Loads the current method's ASM assuming it is plugged.
        /// </summary>
        private void LoadCurrentMethodASM_Plugged()
        {
            if (currentMethod != null)
            {
                currentMethodASM = TheASMFile.GetMethodASM(currentMethod);
            }
        }

        /// <summary>
        /// Handles a received break command.
        /// </summary>
        private void Handle_BreakCmd()
        {
            OnBreak.Invoke();
        }
        /// <summary>
        /// Handles a received Continue command.
        /// </summary>
        private void Handle_ContinueCmd()
        {
            OnInvalidCommand((byte)DebugCommands.Continue);
        }
        /// <summary>
        /// Handles a received GetArguments command.
        /// </summary>
        private void Handle_GetArgumentsCmd()
        {
            OnInvalidCommand((byte)DebugCommands.GetArguments);
        }
        /// <summary>
        /// Handles a received GetBreakAddress command.
        /// </summary>
        private void Handle_GetBreakAddressCmd()
        {
            OnInvalidCommand((byte)DebugCommands.GetBreakAddress);
        }
        /// <summary>
        /// Handles a received GetLocals command.
        /// </summary>
        private void Handle_GetLocalsCmd()
        {
            OnInvalidCommand((byte)DebugCommands.GetLocals);
        }
        /// <summary>
        /// Handles a received GetRegisters command.
        /// </summary>
        private void Handle_GetRegistersCmd()
        {
            OnInvalidCommand((byte)DebugCommands.GetRegisters);
        }
        /// <summary>
        /// Handles a received GetMemory command.
        /// </summary>
        private void Handle_GetMemoryCmd()
        {
            OnInvalidCommand((byte)DebugCommands.GetMemory);
        }
        /// <summary>
        /// Handles a received SetInt3 command.
        /// </summary>
        private void Handle_SetInt3Cmd()
        {
            OnInvalidCommand((byte)DebugCommands.SetInt3);
        }
        /// <summary>
        /// Handles a received ClearInt3 command.
        /// </summary>
        private void Handle_ClearInt3Cmd()
        {
            OnInvalidCommand((byte)DebugCommands.ClearInt3);
        }
        /// <summary>
        /// Handles a received Message command.
        /// </summary>
        private void Handle_MessageCmd()
        {
        }
        /// <summary>
        /// Handles a received SendArguments command.
        /// </summary>
        private void Handle_SendArgumentsCmd()
        {
            int bytesToRead = (from args in arguments
                               select args.value.Length).Sum();
            byte[] bytesRead = TheSerial.ReadBytes(bytesToRead);

            Argument retArg = (from args in arguments
                               where args.isReturnArg
                               select args).First();
            int numBytesForRetArg = retArg.value.Length;
            if (retArg.value.Length > 0)
            {
                Array.Copy(bytesRead, retArg.value, retArg.value.Length);
            }

            int bytesSoFar = numBytesForRetArg;
            for (int i = arguments.Count - 1; i > -1; i--)
            {
                if(arguments[i].isReturnArg)
                {
                    continue;
                }

                for (int j = 0; j < arguments[i].value.Length; j++)
                {
                    arguments[i].value[j] = bytesRead[bytesSoFar + j];
                }
                bytesSoFar += arguments[i].value.Length;
            }
        }
        /// <summary>
        /// Handles a received SendBreakAddress command.
        /// </summary>
        private void Handle_SendBreakAddressCmd()
        {
            //NEXT4 - This is 32 bit specific!!
            if (Mode_64bit)
            {
                byte[] readBytes = TheSerial.ReadBytes(8);
                breakAddress = BitConverter.ToUInt64(readBytes, 0);
            }
            else
            {
                byte[] readBytes = TheSerial.ReadBytes(4);
                breakAddress = BitConverter.ToUInt32(readBytes, 0);
            }
        }
        /// <summary>
        /// Handles a received SendLocals command.
        /// </summary>
        private void Handle_SendLocalsCmd()
        {
            int expectedLocalsSize = (from locs in locals
                                      select locs.value.Length).Sum();
            int bytesToRead = (int)BitConverter.ToUInt32(TheSerial.ReadBytes(4), 0);
            byte[] bytesRead = TheSerial.ReadBytes(bytesToRead);
            //Locals received in reverse order
            int bytesSoFar = bytesToRead - expectedLocalsSize;
            
            for (int i = locals.Count - 1; i > -1; i--)
            {
                for (int j = 0; j < locals[i].value.Length; j++)
                {
                    locals[i].value[j] = bytesRead[bytesSoFar + j];
                }
                bytesSoFar += locals[i].value.Length;
            }

            int bytesForTempLocals = bytesRead.Length - expectedLocalsSize;
            int bytesPerLocal = 0;
            DB_Type dbTempLocalType = null;
            if(bytesForTempLocals % 4 == 0)
            {
                bytesPerLocal = 4;
                dbTempLocalType = DebugDatabase.GetType("System.UInt32");
            }
            else if (bytesForTempLocals % 2 == 0)
            {
                bytesPerLocal = 2;
                dbTempLocalType = DebugDatabase.GetType("System.UInt16");
            }
            else
            {
                bytesPerLocal = 1;
                dbTempLocalType = DebugDatabase.GetType("System.Byte");
            }
            for (int i = bytesForTempLocals - bytesPerLocal; i > -1; i -= bytesPerLocal)
            {
                byte[] valueBytes = new byte[bytesPerLocal];
                for (int j = 0; j < valueBytes.Length; j++)
                {
                    valueBytes[j] = bytesRead[i + j];
                }
                locals.Add(new Local()
                {
                    dbType = dbTempLocalType,
                    value = valueBytes,
                    isTemporary = true
                });
            }
        }
        /// <summary>
        /// Handles a received SendRegisters command.
        /// </summary>
        private void Handle_SendRegistersCmd()
        {
            //NEXT4 - This is x86-32 bit specific!!

            //The values got pushed onto the stack in this order:
            //Push All:
            //  Push(EAX); 28
            //  Push(ECX); 24
            //  Push(EDX); 20
            //  Push(EBX); 16
            //  Push(ESP); 12 But this is invalid because this is ESP in interrupt. See below.
            //  Push(EBP); 8
            //  Push(ESI); 4
            //  Push(EDI); 0
            // We receive them in reverse order to that shown above.
            //
            // Additionally we send ESP manually from our stored value so
            // we get the ESP of the broken code not our interrupt handler
            // ESP 36

            registers = new List<Register>(9);
            byte[] readBytes = TheSerial.ReadBytes(36);
            
            registers.Add(new Register()
            {
                register = Debug.Debugger.Registers.EAX,
                value = BitConverter.ToUInt32(readBytes, 28)
            });
            registers.Add(new Register()
            {
                register = Debug.Debugger.Registers.ECX,
                value = BitConverter.ToUInt32(readBytes, 24)
            });
            registers.Add(new Register()
            {
                register = Debug.Debugger.Registers.EDX,
                value = BitConverter.ToUInt32(readBytes, 20)
            });
            registers.Add(new Register()
            {
                register = Debug.Debugger.Registers.EBX,
                value = BitConverter.ToUInt32(readBytes, 16)
            });
            registers.Add(new Register()
            {
                register = Debug.Debugger.Registers.EBP,
                value = BitConverter.ToUInt32(readBytes, 8)
            });
            registers.Add(new Register()
            {
                register = Debug.Debugger.Registers.ESI,
                value = BitConverter.ToUInt32(readBytes, 4)
            });
            registers.Add(new Register()
            {
                register = Debug.Debugger.Registers.EDI,
                value = BitConverter.ToUInt32(readBytes, 0)
            });

            registers.Add(new Register()
            {
                register = Debug.Debugger.Registers.ESP,
                value = BitConverter.ToUInt32(readBytes, 32)
            });
        }
        /// <summary>
        /// Handles a received SendMemory command.
        /// </summary>
        private void Handle_SendMemoryCmd()
        {
            GetMemory_Data = TheSerial.ReadBytes((int)GetMemory_Length);
        }
        /// <summary>
        /// Handles a received Connected command.
        /// </summary>
        private void Handle_ConnectedCmd()
        {
            sendConnectValue = false;
        }

        /// <summary>
        /// Handles a received StepNext command.
        /// </summary>
        private void Handle_StepNextCmd()
        {
            OnInvalidCommand((byte)DebugCommands.StepNext);
        }


        /// <summary>
        /// Breaks OS execution at the nearest possible moment.
        /// Note: EndBreak has to be called unless execution is immediately continued.
        /// </summary>
        public void BeginBreak()
        {
            State = States.Breaking;
        }
        /// <summary>
        /// Completes breaking of OS execution. Must be called immediately after OnBreak event is fired unless Continue 
        /// is immediately called.
        /// </summary>
        public void EndBreak()
        {
            State = States.Broken;
        }
        /// <summary>
        /// Continues OS execution - can be called immediately after OnBreak without EndBreak being called.
        /// </summary>
        public void Continue()
        {
            SendPendingInt3Changes();

            LoadMemoryRequests.Clear();
            State = States.Running;
            TheSerial.Write((byte)DebugCommands.Continue);
        }

        /// <summary>
        /// Performs an Int1 step
        /// </summary>
        public void StepNext()
        {
            LoadMemoryRequests.Clear();
            State = States.Stepping;
            TheSerial.Write((byte)DebugCommands.StepNext);
        }

        /// <summary>
        /// Sends a request for the brwak address.
        /// </summary>
        public void GetBreakAddress()
        {
            currentMethod = null;
            currentMethodASM = null;
            currentNearestLabels = null;
            currentNearestMethodBasedLabel = null;
            if (currentILOpInfo != null)
            {
                lastILOpInfo = currentILOpInfo;
            }
            currentILOpInfo = null;
            arguments = null;
            locals = null;
            currentCSLine = null;
            currentCSMethod = null;
            currentCSSymbol = null;

            TheSerial.Write((byte)DebugCommands.GetBreakAddress);
            WaitForCommand();
        }

        /// <summary>
        /// Gets the register values as they were immediately prior to the break interrupt.
        /// </summary>
        public void GetRegisters()
        {
            TheSerial.Write((byte)DebugCommands.GetRegisters);
            WaitForCommand();
        }

        /// <summary>
        /// Gets the argument values as they were immediately prior to the break interrupt.
        /// </summary>
        public void GetArguments()
        {
            if (CurrentMethod != null)
            {
                //TODO - isn't return value being with args x86 specific? Consider when developing new architectures support.
                
                //Return value is on stack too like argument
                List<DB_Argument> DBArgs = CurrentMethod.DB_Arguments
                                                        .OrderBy(x => x.Index)
                                                        .ToList();
                uint numArgs = (uint)DBArgs.Count;
                arguments = new List<Argument>((int)numArgs);
                uint sizeOfArgs = 0;
                for (int i = 0; i < (int)numArgs; i++)
                {
                    Argument newLoc = new Argument()
                    {
                        value = new byte[DBArgs[i].BytesSize],
                        dbType = DBArgs[i].DB_Type,
                        isReturnArg = DBArgs[i].IsReturnArg
                    };
                    arguments.Add(newLoc);
                    sizeOfArgs += (uint)newLoc.value.Length;
                }

                TheSerial.Write((byte)DebugCommands.GetArguments);
                TheSerial.Write(sizeOfArgs);
                WaitForCommand();
            }
        }

        /// <summary>
        /// Gets the locals values as they were immediately prior to the break interrupt.
        /// </summary>
        public void GetLocals()
        {
            if (CurrentMethod != null)
            {
                List<DB_LocalVariable> DBLocals = CurrentMethod.DB_LocalVariables
                                                        .OrderBy(x => x.Index)
                                                        .ToList();
                uint numLocals = (uint)DBLocals.Count;
                locals = new List<Local>((int)numLocals);
                uint sizeOfArgs = 0;
                for (int i = 0; i < (int)numLocals; i++)
                {
                    Local newLoc = new Local()
                    {
                        value = new byte[DBLocals[i].BytesSize],
                        dbType =  DBLocals[i].DB_Type
                    };
                    locals.Add(newLoc);
                    sizeOfArgs += (uint)newLoc.value.Length;
                }
                TheSerial.Write((byte)DebugCommands.GetLocals);
                WaitForCommand();
            }
            else
            {
                locals = new List<Local>();
                TheSerial.Write((byte)DebugCommands.GetLocals);
                WaitForCommand();
            }
        }

        private uint GetMemory_Length;
        private byte[] GetMemory_Data;
        /// <summary>
        /// Gets the value of the specified memory.
        /// </summary>
        /// <param name="address">The address of the memory to get.</param>
        /// <param name="length">The length of the memory to get.</param>
        /// <returns>The bytes of the value or null.</returns>
        public byte[] GetMemory(byte[] address, uint length)
        {
            GetMemory_Data = null;
            GetMemory_Length = length;

            TheSerial.Write((byte)DebugCommands.GetMemory);
            TheSerial.Write(0x020970c3);
            //for (int i = 0; i < address.Length; i++)
            //{
            //    TheSerial.Write(address[i]);
            //}
            TheSerial.Write(length);

            WaitForCommand();
            
            return GetMemory_Data;
        }

        private List<ulong> PendingSetInt3Addresses = new List<ulong>();
        private List<ulong> PendingClearInt3Addresses = new List<ulong>();
        /// <summary>
        /// Performs a delayed-set of an Int3 at the specified address.
        /// </summary>
        /// <param name="address">The addres to set the int3 at.</param>
        public void SetInt3(ulong address)
        {
            PendingClearInt3Addresses.Remove(address);
            PendingSetInt3Addresses.Add(address);
        }
        /// <summary>
        /// Performs a delayed-clear of an Int3 at the specified address.
        /// </summary>
        /// <param name="address">The addres to set the int3 at.</param>
        public void ClearInt3(ulong address)
        {
            PendingSetInt3Addresses.Remove(address);
            PendingClearInt3Addresses.Add(address);
        }
        private void SendPendingInt3Changes()
        {
            foreach(ulong anAddress in PendingSetInt3Addresses)
            {
                TheSerial.Write((byte)DebugCommands.SetInt3);
                if (Mode_64bit)
                {
                    TheSerial.Write(anAddress);
                }
                else
                {
                    TheSerial.Write((uint)anAddress);
                }
            }
            foreach (ulong anAddress in PendingClearInt3Addresses)
            {
                TheSerial.Write((byte)DebugCommands.ClearInt3);
                if (Mode_64bit)
                {
                    TheSerial.Write(anAddress);
                }
                else
                {
                    TheSerial.Write((uint)anAddress);
                }
            }
            PendingSetInt3Addresses.Clear();
            PendingClearInt3Addresses.Clear();
        }

        /// <summary>
        /// Steps to the beggining of the next line of IL code.
        /// </summary>
        /// <returns>The address to break on.</returns>
        public ulong StepToNextIL()
        {
            ulong StepToAddress = ulong.MaxValue;

            List<DB_ILOpInfo> potOps = null;

            if(CurrentILOpInfo != null)
            {
                int nextPos = CurrentILOpInfo.NextPosition;
                potOps = (from ilOps in CurrentILOpInfo.DB_Method.DB_ILOpInfos
                                            where (ilOps.Position == nextPos)
                                            select ilOps).ToList();
            }
            else if(LastILOpInfo != null)
            {
                int nextPos = LastILOpInfo.NextPosition;
                potOps = (from ilOps in LastILOpInfo.DB_Method.DB_ILOpInfos
                          where (ilOps.Position == nextPos)
                          select ilOps).ToList();
            }
            if(potOps != null && potOps.Count > 0)
            {
                DB_ILOpInfo nextIlOp = potOps.First();
                if(nextIlOp.DebugOpMeta != null)
                {
                    string debugMeta = nextIlOp.DebugOpMeta;
                    string[] debugMetas = debugMeta.Split(';');
                    foreach(string aDebugInfo in debugMetas)
                    {
                        string[] parts = aDebugInfo.Split('=');
                        if (parts[0] == "DebugNopLabel")
                        {
                            string debugNopLabel = parts[1];
                            ulong NopAddress = TheElfMap.GetAddress(debugNopLabel);
                            //If it is a valid address - see GetAddress implementation
                            if(NopAddress != ulong.MaxValue)
                            {
                                StepToAddress = NopAddress;

                                SetInt3(NopAddress);
                                Continue();
                                State = States.Stepping;
                                break;
                            }
                        }
                    }
                }
            }

            return StepToAddress;
        }


        /// <summary>
        /// Represents a callback method for when requested memory data has been loaded.
        /// </summary>
        /// <param name="data">The loaded data.</param>
        /// <param name="state">The state is actually the request (tuple) object.</param>
        public delegate void LoadMemoryCompletedDelegate(byte[] data, Tuple<byte[], uint, LoadMemoryCompletedDelegate, object> state);
        /// <summary>
        /// The list of requests for memory.
        /// </summary>
        private List<Tuple<byte[], uint, LoadMemoryCompletedDelegate, object>> LoadMemoryRequests = new List<Tuple<byte[], uint, LoadMemoryCompletedDelegate, object>>();
        /// <summary>
        /// Adds a memory request to the queue of memory requests.
        /// </summary>
        /// <param name="address">The bytes representing the address of the memory to load.</param>
        /// <param name="length">The length of the data to load.</param>
        /// <param name="onLoaded">The callback to call when the data has been loaded.</param>
        /// <param name="state">The request state object.</param>
        public void LoadMemoryValue(byte[] address, uint length, LoadMemoryCompletedDelegate onLoaded, object state)
        {
            LoadMemoryRequests.Add(new Tuple<byte[], uint, LoadMemoryCompletedDelegate, object>(address, length, onLoaded, state));
        }
        /// <summary>
        /// Underlying method that asynchronously executes all the memory requests.
        /// </summary>
        private void LoadMemoryValue_Run()
        {
            while (State != States.Stopping)
            {
                while (State != States.Broken && State != States.Stopping)
                {
                    System.Threading.Thread.Sleep(100);
                }

                if (State != States.Stopping)
                {
                    while (LoadMemoryRequests.Count > 0 && State == States.Broken)
                    {
                        Tuple<byte[], uint, LoadMemoryCompletedDelegate, object> request = null;
                        //lock (LoadMemoryRequests)
                        {
                            request = LoadMemoryRequests[0];
                            LoadMemoryRequests.RemoveAt(0);
                        }
                        if (request != null)
                        {
                            byte[] data = GetMemory(request.Item1, request.Item2);

                            request.Item3.BeginInvoke(data, request, new AsyncCallback(delegate(IAsyncResult result)
                            {
                                try
                                {
                                    LoadMemoryCompletedDelegate state = (LoadMemoryCompletedDelegate)result.AsyncState;
                                    state.EndInvoke(result);
                                }
                                catch
                                {
                                }
                            }), request.Item3);
                        }
                    }
                }
            }
            LoadMemoryRequests.Clear();
        }
    }

    //NEXT4 - This is 32bit specific!

    /// <summary>
    /// All the supported registers
    /// </summary>
    public enum Registers
    {
        /// <summary>
        /// EAX register
        /// </summary>
        EAX,
        /// <summary>
        /// EBX register
        /// </summary>
        EBX,
        /// <summary>
        /// ECX register
        /// </summary>
        ECX,
        /// <summary>
        /// EDX register
        /// </summary>
        EDX,

        /// <summary>
        /// ESI register
        /// </summary>
        ESI,
        /// <summary>
        /// EDI register
        /// </summary>
        EDI,
        /// <summary>
        /// EBP register
        /// </summary>
        EBP,
        /// <summary>
        /// EIP register
        /// </summary>
        EIP,
        /// <summary>
        /// ESP register
        /// </summary>
        ESP
    }
    //NEXT4 - This should not be 32bit specific!
    /// <summary>
    /// Represents a register value
    /// </summary>
    public class Register
    {
        /// <summary>
        /// The register this value if from.
        /// </summary>
        public Registers register = Registers.EAX;
        /// <summary>
        /// The value of the register.
        /// </summary>
        public UInt32 value = 0;
    }
    /// <summary>
    /// Represents any variable e.g. arguments or locals
    /// </summary>
    public class Variable
    {
        /// <summary>
        /// Inidicates whether the value of the string has been loaded or only the address.
        /// </summary>
        public bool StringValueLoaded = false;

        /// <summary>
        /// The bytes value of the variable
        /// </summary>
        public byte[] value;
        
        /// <summary>
        /// The type of the variable
        /// </summary>
        public DB_Type dbType;

        /// <summary>
        /// The variables that represent the fields of this variable.
        /// </summary>
        public List<Variable> Fields = new List<Variable>();

        public delegate void OnLoadFieldsCompleteDelegate();
        /// <summary>
        /// Loads the values of the fields of this variable.
        /// </summary>
        /// <param name="debugger">The debugger to use for loading.</param>
        public void LoadFields(Debugger debugger, OnLoadFieldsCompleteDelegate callback, bool recursive = false)
        {
            if (dbType.Signature == "System.String")
            {
                #region String Load

                StringValueLoaded = false;
                debugger.LoadMemoryValue(value, (uint)dbType.StackBytesSize, delegate(byte[] data, Tuple<byte[], uint, Debugger.LoadMemoryCompletedDelegate, object> state)
                {
                    Variable fieldVar1 = (Variable)state.Item4;

                    //This stuff relies entirely on how string data is compiled into the kernel

                    uint stringLength = BitConverter.ToUInt32(data, 0);

                    //TODO - Create a proper test condition
                    //This was inserted because when stepping into a method,
                    //the first two instructions set up EBP/ESP correctly
                    //but during that time the returned values are all screwy
                    //usually resulting in incorrect values, in this case the length 
                    //ends up insanely big!
                    if (stringLength < 500)
                    {
                        Tuple<ulong, byte> fieldAddress1 = Utils.BytesToAddress(fieldVar1.value);
                        fieldAddress1 = new Tuple<ulong, byte>(fieldAddress1.Item1 + 4, fieldAddress1.Item2);
                        fieldVar1.value = Utils.AddressToBytes(fieldAddress1);

                        debugger.LoadMemoryValue(fieldVar1.value, stringLength * 2, delegate(byte[] data1, Tuple<byte[], uint, Debugger.LoadMemoryCompletedDelegate, object> state1)
                        {
                            Variable fieldVar2 = (Variable)state1.Item4;
                            fieldVar2.value = data1;
                            fieldVar2.StringValueLoaded = true;

                            callback();
                        }, fieldVar1);
                    }
                }, this);

                #endregion
            }
            //If this is a reference type, it will have fields / further data to load
            else if (!dbType.IsValueType)
            {
                //The value bytes are in fact a reference (pointer)
                byte[] thisAddressBytes = value;
                //Convert the address to an actual number 
                //  - second value indicates the size of thr address in number of bytes (i.e. 4 for 32-bit, 8 for 64-bit)
                Tuple<ulong, byte> thisAddress = Utils.BytesToAddress(thisAddressBytes);
                //Check the address is valid
                if (thisAddress.Item1 != 0)
                {
                    List<DB_ComplexTypeLink> children = dbType.ChildTypes.OrderBy(x => x.ParentIndex).ToList();
                    uint offset = 0;
                    int fieldsLoading = children.Count;
                    foreach (DB_ComplexTypeLink aChild in children)
                    {
                        Tuple<ulong, byte> fieldAddress = new Tuple<ulong, byte>(thisAddress.Item1 + offset, thisAddress.Item2);
                        byte[] fieldAddressBytes = Utils.AddressToBytes(fieldAddress);

                        Variable fieldVar = new Variable()
                        {
                            dbType = aChild.ChildType,
                            value = fieldAddressBytes
                        };
                        Fields.Add(fieldVar);

                        if (fieldVar.dbType.IsValueType || fieldVar.dbType.Signature == "System.String")
                        {
                            //Load the actual value

                            if (fieldVar.dbType.Signature == "System.String")
                            {
                                #region String Load

                                fieldVar.StringValueLoaded = false;
                                debugger.LoadMemoryValue(fieldVar.value, (uint)fieldVar.dbType.StackBytesSize, delegate(byte[] data, Tuple<byte[], uint, Debugger.LoadMemoryCompletedDelegate, object> state)
                                {
                                    Variable fieldVar1 = (Variable)state.Item4;

                                    //This stuff relies entirely on how string data is compiled into the kernel

                                    uint stringLength = BitConverter.ToUInt32(data, 0);

                                    //TODO - Create a proper test condition
                                    //This was inserted because when stepping into a method,
                                    //the first two instructions set up EBP/ESP correctly
                                    //but during that time the returned values are all screwy
                                    //usually resulting in incorrect values, in this case the length 
                                    //ends up insanely big!
                                    if (stringLength < 500)
                                    {
                                        Tuple<ulong, byte> fieldAddress1 = Utils.BytesToAddress(fieldVar1.value);
                                        fieldAddress1 = new Tuple<ulong, byte>(fieldAddress1.Item1 + 4, fieldAddress1.Item2);
                                        fieldVar1.value = Utils.AddressToBytes(fieldAddress1);

                                        debugger.LoadMemoryValue(fieldVar1.value, stringLength * 2, delegate(byte[] data1, Tuple<byte[], uint, Debugger.LoadMemoryCompletedDelegate, object> state1)
                                        {
                                            Variable fieldVar2 = (Variable)state1.Item4;
                                            fieldVar2.value = data1;

                                            fieldsLoading--;
                                            if (fieldsLoading == 0)
                                            {
                                                callback();
                                            }
                                        }, fieldVar1);
                                    }
                                }, fieldVar);

                                #endregion
                            }
                            else
                            {
                                debugger.LoadMemoryValue(fieldVar.value, (uint)fieldVar.dbType.BytesSize, delegate(byte[] data, Tuple<byte[], uint, Debugger.LoadMemoryCompletedDelegate, object> state)
                                {
                                    Variable fieldVar1 = (Variable)state.Item4;
                                    fieldVar1.value = data;
                                    fieldVar1.StringValueLoaded = true;

                                    fieldsLoading--;
                                    if (fieldsLoading == 0)
                                    {
                                        callback();
                                    }                                    
                                }, fieldVar);
                            }
                        }
                        else if (recursive)
                        {
                            //Load the field's fields - i.e. recursive

                            debugger.LoadMemoryValue(fieldVar.value, (uint)fieldVar.dbType.StackBytesSize, delegate(byte[] data, Tuple<byte[], uint, Debugger.LoadMemoryCompletedDelegate, object> state)
                            {
                                Variable fieldVar1 = (Variable)state.Item4;
                                fieldVar1.value = data;

                                fieldVar1.LoadFields(debugger, delegate()
                                {
                                    fieldsLoading--;
                                    if (fieldsLoading == 0)
                                    {
                                        callback();
                                    }
                                }, recursive);
                            }, fieldVar);
                        }

                        offset += (uint)fieldVar.dbType.StackBytesSize;
                    }
                }
            }
        }

        /// <summary>
        /// Converts the variable's value to a string
        /// </summary>
        /// <returns>The variable's value represented as a string</returns>
        public override string ToString()
        {
            if (dbType.Signature == "System.String")
            {
                if (StringValueLoaded)
                {
                    return Utils.GetValueStr(value, dbType.Signature);
                }
                else
                {
                    return Utils.GetValueStr(value);
                }
            }
            else
            {
                return Utils.GetValueStr(value, dbType.Signature);
            }
        }
    }
    /// <summary>
    /// Represents a method argument
    /// </summary>
    public class Argument : Variable
    {
        /// <summary>
        /// Whether the argument is actually the return value.
        /// </summary>
        public bool isReturnArg;
    }
    /// <summary>
    /// Represents a local variable
    /// </summary>
    public class Local : Variable
    {
        /// <summary>
        /// Whether the local is a temporary value rather than a built-in local.
        /// </summary>
        public bool isTemporary;
    }
}
