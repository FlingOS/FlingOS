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
using System.IO.Pipes;
using System.Text;

namespace Drivers.Debugger
{
    /// <summary>
    ///     Handler for methods called when the serial pipe is connected.
    /// </summary>
    public delegate void OnConnectedHandler();

    /// <summary>
    ///     A serial pipe wrapper.
    /// </summary>
    /// <remarks>
    ///     Implements IDisposable to cleanly close the pipe connection.
    /// </remarks>
    public sealed class Serial : IDisposable
    {
        public bool AbortRead;

        /// <summary>
        ///     The underlying pipe.
        /// </summary>
        private NamedPipeServerStream ThePipe;

        /// <summary>
        ///     Whether the serial pipe is connected.
        /// </summary>
        public bool Connected
        {
            get { return ThePipe != null && ThePipe.IsConnected; }
        }

        /// <summary>
        ///     Disposes of the serial class. Calls <see cref="Disconnect" />.
        /// </summary>
        public void Dispose()
        {
            Disconnect();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Fired when the serial pipe gains a connection.
        /// </summary>
        public event OnConnectedHandler OnConnected;

        /// <summary>
        ///     Initialises the named pipe server and waits for a connection
        /// </summary>
        /// <param name="pipe">The name of the pipe to create</param>
        /// <returns>True if a connection is received. Otherwise false.</returns>
        public bool Init(string pipe)
        {
            bool OK = Disconnect();
            if (OK)
            {
                try
                {
                    ThePipe = new NamedPipeServerStream(pipe, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);
                    ThePipe.ReadMode = PipeTransmissionMode.Byte;
                    ThePipe.BeginWaitForConnection(delegate(IAsyncResult result)
                    {
                        try
                        {
                            ThePipe.EndWaitForConnection(result);

                            if (OnConnected != null)
                            {
                                OnConnected();
                            }
                        }
                        catch
                        {
                            //Ignore as probably error while terminating
                        }
                    }, null);
                }
                catch
                {
                    OK = false;
                    ThePipe = null;
                }
            }
            return OK;
        }

        /// <summary>
        ///     Cleanly disconnects the pipe and terminates reading.
        /// </summary>
        /// <returns>True if disconnected successfully.</returns>
        public bool Disconnect()
        {
            bool OK = true;

            if (ThePipe != null)
            {
                ThePipe.Close();
                ThePipe.Dispose();
                ThePipe = null;
            }

            return OK;
        }

        /// <summary>
        ///     Reads the specified number of bytes from the pipe.
        /// </summary>
        /// <param name="numToRead">The number of bytes to read.</param>
        /// <returns>The bytes read.</returns>
        public byte[] ReadBytes(int numToRead)
        {
            AbortRead = false;

            byte[] readBuffer = new byte[numToRead];
            for (int i = 0; i < numToRead && !AbortRead; i++)
            {
                readBuffer[i] = (byte)ThePipe.ReadByte();
            }
            return readBuffer;
        }

        public string ReadLine()
        {
            AbortRead = false;

            string result = "";

            char c = '\0';
            do
            {
                c = (char)ThePipe.ReadByte();

                if (c == '\n' || c == '\r')
                {
                    break;
                }

                if (c != '\0')
                {
                    result += c;
                }
            } while (Connected && !AbortRead);

            return result;
        }

        /// <summary>
        ///     Writes a byte to the serial pipe.
        /// </summary>
        /// <param name="aByte">The byte to write.</param>
        public void Write(byte aByte)
        {
            if (Connected)
            {
                ThePipe.Write(new[] {aByte}, 0, 1);
            }
        }

        /// <summary>
        ///     Writes a UInt32 to the serial pipe.
        /// </summary>
        /// <param name="anInt">The UInt32 to write.</param>
        public void Write(uint anInt)
        {
            if (Connected)
            {
                ThePipe.Write(BitConverter.GetBytes(anInt), 0, 4);
            }
        }

        /// <summary>
        ///     Writes a UInt64 to the serial pipe.
        /// </summary>
        /// <param name="anInt">The UInt64 to write.</param>
        public void Write(ulong anInt)
        {
            if (Connected)
            {
                ThePipe.Write(BitConverter.GetBytes(anInt), 0, 8);
            }
        }

        public void Write(string aStr)
        {
            if (Connected)
            {
                ThePipe.Write(Encoding.ASCII.GetBytes(aStr), 0, aStr.Length);
            }
        }

        public void WriteLine(string aStr)
        {
            if (Connected)
            {
                ThePipe.Write(Encoding.ASCII.GetBytes(aStr + "\n"), 0, aStr.Length + 1);
            }
        }
    }
}