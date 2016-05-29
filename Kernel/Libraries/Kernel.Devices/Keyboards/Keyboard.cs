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

using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Processes;
using Kernel.Multiprocessing;

namespace Kernel.Devices.Keyboards
{
    /// <summary>
    ///     Represents a keyboard device.
    /// </summary>
    public abstract partial class Keyboard : Device
    {
        /// <summary>
        ///     The time delay. in milliseconds, between getting each character. Limits the maximum rate at
        ///     which a held down key is detected.
        /// </summary>
        public static uint GetCharDelayTimeMS = 10;

        /// <summary>
        ///     The default keyboard device for the core kernel.
        /// </summary>
        public static Keyboard Default;

        /// <summary>
        ///     Whether the alternate key is currently pressed or not.
        /// </summary>
        protected bool altPressed;

        /// <summary>
        ///     Whether the control key is currently pressed or not.
        /// </summary>
        protected bool ctrlPressed;

        /// <summary>
        ///     Whether the keyboard is enabled or not.
        /// </summary>
        protected bool enabled = false;

        /// <summary>
        ///     The list of key mappings.
        /// </summary>
        protected List KeyMappings;

        /// <summary>
        ///     The buffer of scancodes received from the keyboard.
        ///     Scancode at index 0 is the first received.
        ///     Scancode at index Count-1 is the latest received.
        /// </summary>
        protected UInt32List scancodeBuffer = new UInt32List(512);

        protected int ScancodeBufferSemaphoreId = -1;

        /// <summary>
        ///     Whether the shift key is currently pressed or not.
        /// </summary>
        protected bool shiftPressed;

        /// <summary>
        ///     Initialises a new keyboard instance including setting up the default
        ///     key mappings if they have not already been initialised.
        ///     To change the keyboard mapping, define a compile time symbol
        ///     For US Keyboard, define USKEYBOARD and undefine UKKEYBOARD or SPKEYBOARD
        ///     For UK Keyboard, define UKKEYBOARD and undefine USKEYBOARD or SPKEYBOARD
        ///     For Spanish Keyboard, define SPKEYBOARD and undefine UKKEYBOARD or USKEYBOARD
        ///     This definition is present in the project properties
        /// </summary>
        public Keyboard()
        {
            // The type of keyboard mapping to use
            // Define UKKEYBOARD in project properties to use UK Keyboard layout
            // Define USKEYBOARD in project properties to use US Keyboard layout
            // Define SPKEYBOARD in project properties to use SP Keyboard layout
            if (KeyMappings == null)
            {
#if UKKEYBOARD
                CreateUKKeymap();
#elif USKEYBOARD
                CreateUSKeymap();
#elif SPKEYBOARD
                CreateSPKeymap();
#else
                BasicConsole.WriteLine("No default keymap specified at compile time! Using UK keymap.");
                CreateUKKeymap();
#endif
            }

            SystemCallResults result = SystemCalls.CreateSemaphore(-1, out ScancodeBufferSemaphoreId);
            if (result != SystemCallResults.OK)
            {
                ExceptionMethods.Throw(new Exception("Couldn't create the necessary semaphore!"));
            }
        }

        /// <summary>
        ///     Whether the keyboard is enabled or not.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
        }

        /// <summary>
        ///     Whether the shift key is currently pressed or not.
        /// </summary>
        public bool ShiftPressed
        {
            get { return shiftPressed; }
        }

        /// <summary>
        ///     Whether the control key is currently pressed or not.
        /// </summary>
        public bool CtrlPressed
        {
            get { return ctrlPressed; }
        }

        /// <summary>
        ///     Whether the alternate key is currently pressed or not.
        /// </summary>
        public bool AltPressed
        {
            get { return altPressed; }
        }

        public void Destroy()
        {
            if (ScancodeBufferSemaphoreId != -1)
            {
                SystemCalls.ReleaseSemaphore(ScancodeBufferSemaphoreId);
            }
        }

        /// <summary>
        ///     Enables the keyboard.
        /// </summary>
        public abstract void Enable();

        /// <summary>
        ///     Disables the keyboard.
        /// </summary>
        public abstract void Disable();

        public abstract void HandleScancode(uint scancode);

        /// <summary>
        ///     Adds a new keyboard mapping.
        /// </summary>
        /// <param name="scancode">The scancode received from the keyboard.</param>
        /// <param name="character">The character to represent the scancode or \0.</param>
        /// <param name="key">The keyboard key to respresent the scancode.</param>
        protected void AddKey(uint scancode, char character, KeyboardKey key)
        {
            KeyMappings.Add(new KeyMapping(scancode, character, key));
        }

        /// <summary>
        ///     Adds a new keyboard mapping for the same key with and without the shift key.
        /// </summary>
        /// <param name="scancode">The scancode received from the keyboard (without the shift key).</param>
        /// <param name="character">The character to represent the scancode or \0.</param>
        /// <param name="key">The keyboard key to respresent the scancode.</param>
        protected void AddKeyWithAndWithoutShift(uint scancode, char character, KeyboardKey key)
        {
            //Add normal key
            AddKey(scancode, character, key);
            //Add scancode for key with shift-key pressed
            AddKey(scancode << 16, character, key);
        }

        /// <summary>
        ///     Adds a new keyboard mapping for a key which has no character representation.
        /// </summary>
        /// <param name="scancode">The scancode received from the keyboard.</param>
        /// <param name="key">The keyboard key to respresent the scancode.</param>
        protected void AddKey(uint scancode, KeyboardKey key)
        {
            AddKey(scancode, '\0', key);
        }

        /// <summary>
        ///     Adds a new keyboard mapping for a key which has no character representation.
        ///     Adds entries for the key with and without the shift key modifier.
        /// </summary>
        /// <param name="scancode">The scancode received from the keyboard (without the shift key).</param>
        /// <param name="key">The keyboard key to respresent the scancode.</param>
        protected void AddKeyWithShift(uint scancode, KeyboardKey key)
        {
            AddKeyWithAndWithoutShift(scancode, '\0', key);
        }

        /// <summary>
        ///     Replaces the keyboard mapping with the one specified.
        /// </summary>
        /// <param name="aKeys">The new keyboard mapping to use.</param>
        public void ChangeKeyMap(List aKeys)
        {
            KeyMappings = aKeys;
        }

        /// <summary>
        ///     Queues a scancode on the scancode buffer.
        /// </summary>
        /// <param name="scancode">The scancode to queue.</param>
        protected void Enqueue(uint scancode)
        {
            if (scancodeBuffer.Count < scancodeBuffer.Capacity)
            {
                scancodeBuffer.Add(scancode);
            }

            ProcessManager.Semaphore_Signal(ScancodeBufferSemaphoreId, ProcessManager.CurrentProcess);
        }

        /// <summary>
        ///     Dequeues the oldest scancode from the scancode buffer.
        /// </summary>
        /// <returns>The dequeued scancode.</returns>
        public uint Dequeue()
        {
            //Pops the first item off the top of the queue
            try
            {
                uint result = scancodeBuffer[0];
                scancodeBuffer.RemoveAt(0);
                return result;
            }
            catch
            {
                for (int i = 0; i < 20; i++)
                {
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                    BasicConsole.DelayOutput(1);
                }
                ExceptionMethods.Rethrow();
            }
            return 0xFFFFFFFF;
        }

        /// <summary>
        ///     Gets the first non-\0 character which represents the specified scancode.
        /// </summary>
        /// <param name="aScanCode">The scancode to get the character for.</param>
        /// <param name="aValue">Output. The character which represents the scancode or \0 if none found.</param>
        /// <returns>True if a character to represent the scancode was found. Otherwise false.</returns>
        public bool GetCharValue(uint aScanCode, out char aValue)
        {
            //Loops through all the key mappings to find the one which matches
            //  the specified scancode. Output value goes in aValue, return true
            //  indicates a valid character was found. Return false indicates key 
            //  mapping was not found.

            //We ignore scancodes for which the character is \0 since they are "no character" 
            //  and so not a valid return value from this method.

            for (int i = 0; i < KeyMappings.Count; i++)
            {
                if (((KeyMapping) KeyMappings[i]).Scancode == aScanCode)
                {
                    if (((KeyMapping) KeyMappings[i]).Value != '\0')
                    {
                        aValue = ((KeyMapping) KeyMappings[i]).Value;
                        return true;
                    }
                    break;
                }
            }

            aValue = '\0';
            return false;
        }

        /// <summary>
        ///     Gets the first KeyboardKey which represents the specified scancode.
        /// </summary>
        /// <param name="aScanCode">The scancode to get the character for.</param>
        /// <param name="aValue">
        ///     Output. The KeyboardKey which represents the scancode or KeyboardKey.NoName if none found.
        /// </param>
        /// <returns>True if a KeyboardKey to represent the scancode was found. Otherwise false.</returns>
        public bool GetKeyValue(uint aScanCode, out KeyboardKey aValue)
        {
            //Loops through all the key mappings to find the one which matches
            //  the specified scancode. Output value goes in aValue, return true
            //  indicates key mapping was found. Return false indicates key 
            //  mapping was not found.

            for (int i = 0; i < KeyMappings.Count; i++)
            {
                if (((KeyMapping) KeyMappings[i]).Scancode == aScanCode)
                {
                    aValue = ((KeyMapping) KeyMappings[i]).Key;
                    return true;
                }
            }

            aValue = KeyboardKey.NoName;
            return false;
        }

        /// <summary>
        ///     Gets the first KeyboardMapping which represents the specified scancode.
        /// </summary>
        /// <param name="aScanCode">The scancode to get the character for.</param>
        /// <param name="aValue">
        ///     Output. The KeyboardMapping which represents the scancode or null if none found.
        /// </param>
        /// <returns>True if a KeyboardMapping to represent the scancode was found. Otherwise false.</returns>
        public bool GetKeyMapping(uint aScanCode, out KeyMapping aValue)
        {
            //Loops through all the key mappings to find the one which matches
            //  the specified scancode. Output value goes in aValue, return true
            //  indicates key mapping was found. Return false indicates key 
            //  mapping was not found.

            for (int i = 0; i < KeyMappings.Count; i++)
            {
                if (((KeyMapping) KeyMappings[i]).Scancode == aScanCode)
                {
                    aValue = (KeyMapping) KeyMappings[i];
                    return true;
                }
            }

            aValue = null;
            return false;
        }

        /// <summary>
        ///     Blocking. Reads the oldest recognised key pressed from the buffer or waits until a recognised key
        ///     is pressed then returns it.
        /// </summary>
        /// <returns>The dequeued key mapping.</returns>
        public KeyMapping ReadMapping()
        {
            KeyMapping xResult = null;

            //Wait until a recognised key mapping is found
            while (!GetKeyMapping(ReadScancode(), out xResult))
            {
            }
            return xResult;
        }

        /// <summary>
        ///     Blocking. Reads the oldest recognised character pressed from the buffer or waits until a
        ///     recognised character is pressed then returns it.
        /// </summary>
        /// <returns>The dequeued character.</returns>
        public char ReadChar()
        {
            char xResult = '\0';
            //Wait until a recognised character is found
            while (!GetCharValue(ReadScancode(), out xResult))
            {
            }
            return xResult;
        }

        /// <summary>
        ///     Blocking. Reads the oldest recognised key pressed from the buffer or waits until a
        ///     recognised key is pressed then returns it.
        /// </summary>
        /// <returns>The dequeued key.</returns>
        public KeyboardKey ReadKey()
        {
            KeyboardKey xResult = KeyboardKey.NoName;
            //Wait until a recognised keyboard key is found
            while (!GetKeyValue(ReadScancode(), out xResult))
            {
            }
            return xResult;
        }

        /// <summary>
        ///     Blocking. Reads the oldest scancode from the buffer or waits until a
        ///     scancode is received then returns it.
        /// </summary>
        /// <returns>The dequeued scancode.</returns>
        public uint ReadScancode()
        {
            SystemCalls.WaitSemaphore(ScancodeBufferSemaphoreId);

            return Dequeue();
        }

        /// <summary>
        ///     Non-blocking. Gets the oldest character pressed (which may be \0) or \0 if none queued.
        /// </summary>
        /// <param name="c">The dequeued character or \0.</param>
        /// <returns>True if a character was dequeued. Otherwise false.</returns>
        public bool GetChar(out char c)
        {
            //This is a non-blocking method.

            //If a scancode is immediately available:
            if (scancodeBuffer.Count > 0)
            {
                //Dequeue the scancode and return the character for it.
                GetCharValue(Dequeue(), out c);
                //Return that we dequeued a character
                return true;
            }
            c = '\0';

            //Otherwise just return that we didn't dequeue a character
            return false;
        }

        /// <summary>
        ///     Non-blocking. Gets the oldest key pressed (which may be NoName) or NoName if none queued.
        /// </summary>
        /// <param name="c">The dequeued key or NoName.</param>
        /// <returns>True if a key was dequeued. Otherwise false.</returns>
        public bool GetKey(out KeyboardKey c)
        {
            //Same idea as GetChar - see that for docs.

            if (scancodeBuffer.Count > 0)
            {
                GetKeyValue(Dequeue(), out c);
                return true;
            }
            c = KeyboardKey.NoName;

            return false;
        }

        /// <summary>
        ///     Non-blocking. Gets the oldest key mapping pressed or null.
        /// </summary>
        /// <param name="c">The dequeued key mapping or null.</param>
        /// <returns>True if a key mapping was dequeued. Otherwise false.</returns>
        public bool GetMapping(out KeyMapping c)
        {
            //Same idea as GetChar - see that for docs.

            if (scancodeBuffer.Count > 0)
            {
                GetKeyMapping(Dequeue(), out c);
                return true;
            }
            c = null;

            return false;
        }

        /// <summary>
        ///     Non-blocking. Gets the oldest scancode received or 0 if none queued.
        /// </summary>
        /// <param name="c">The dequeued scancode or 0.</param>
        /// <returns>True if a scancode was dequeued. Otherwise false.</returns>
        public bool GetScancode(out uint c)
        {
            //Same idea as GetChar - see that for docs.

            if (scancodeBuffer.Count > 0)
            {
                c = Dequeue();
                return true;
            }
            c = 0;
            return false;
        }
    }
}