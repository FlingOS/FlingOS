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
using Kernel.Framework.Exceptions;

namespace Kernel.FileSystems.Streams
{
    public class CachedFileStream : FileStream
    {
        private byte[] CachedData;
        protected FileStream UnderlyingStream;

        public override long Position
        {
            get { return UnderlyingStream.Position; }
            set { UnderlyingStream.Position = value; }
        }

        public CachedFileStream(FileStream AnUnderlyingStream)
            : base(AnUnderlyingStream.TheFile)
        {
            UnderlyingStream = AnUnderlyingStream;

            CachedData = new byte[(uint)TheFile.Size];
            UnderlyingStream.Read(CachedData, 0, CachedData.Length);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int maxToRead = Math.Min(count, CachedData.Length - (int)Position);

            if (count < 0)
            {
                ExceptionMethods.Throw(new ArgumentException("CachedFileStream.Read: count must be > 0"));
            }
            else if (offset < 0)
            {
                ExceptionMethods.Throw(new ArgumentException("CachedFileStream.Read: offset must be > 0"));
            }
            else if (buffer == null)
            {
                ExceptionMethods.Throw(new ArgumentException("CachedFileStream.Read: buffer must not be null!"));
            }
            else if (buffer.Length - offset < count)
            {
                ExceptionMethods.Throw(new ArgumentException("CachedFileStream.Read: Invalid offset / length values!"));
            }

            Array.Copy(CachedData, (int)Position, buffer, offset, maxToRead);

            return maxToRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count < 0)
            {
                ExceptionMethods.Throw(new ArgumentException("CachedFileStream.Write: count must be > 0"));
            }
            else if (offset < 0)
            {
                ExceptionMethods.Throw(new ArgumentException("CachedFileStream.Write: offset must be > 0"));
            }
            else if (buffer == null)
            {
                ExceptionMethods.Throw(new ArgumentException("CachedFileStream.Write: buffer must not be null!"));
            }
            else if (buffer.Length - offset < count)
            {
                ExceptionMethods.Throw(new ArgumentException("CachedFileStream.Write: Invalid offset / length values!"));
            }


            if (CachedData.Length < (int)Position + count)
            {
                byte[] NewCachedData = new byte[offset + count];
                Array.Copy(CachedData, 0, NewCachedData, 0, CachedData.Length);
                CachedData = NewCachedData;
            }

            Array.Copy(buffer, offset, CachedData, (int)Position, count);

            UnderlyingStream.Write(buffer, offset, count);
        }
    }
}