using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System.IO.Streams
{
    public class CachedFileStream : FileStream
    {
        protected FileStream UnderlyingStream;

        public override long Position
        {
            get
            {
                return UnderlyingStream.Position;
            }
            set
            {
                UnderlyingStream.Position = value;
            }
        }

        byte[] CachedData;

        public CachedFileStream(FileStream AnUnderlyingStream)
            : base(AnUnderlyingStream.TheFile)
        {
            UnderlyingStream = AnUnderlyingStream;

            CachedData = new byte[(uint)TheFile.Size];
            UnderlyingStream.Read(CachedData, 0, CachedData.Length);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int maxToRead = FOS_System.Math.Min(count, CachedData.Length - (int)Position);

            if (count < 0)
            {
                ExceptionMethods.Throw(new Exceptions.ArgumentException("CachedFileStream.Read: count must be > 0"));
            }
            else if (offset < 0)
            {
                ExceptionMethods.Throw(new Exceptions.ArgumentException("CachedFileStream.Read: offset must be > 0"));
            }
            else if (buffer == null)
            {
                ExceptionMethods.Throw(new Exceptions.ArgumentException("CachedFileStream.Read: buffer must not be null!"));
            }
            else if (buffer.Length - offset < count)
            {
                ExceptionMethods.Throw(new Exceptions.ArgumentException("CachedFileStream.Read: Invalid offset / length values!"));
            }

            FOS_System.Array.Copy(CachedData, (int)Position, buffer, offset, maxToRead);

            return maxToRead;
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count < 0)
            {
                ExceptionMethods.Throw(new Exceptions.ArgumentException("CachedFileStream.Write: count must be > 0"));
            }
            else if (offset < 0)
            {
                ExceptionMethods.Throw(new Exceptions.ArgumentException("CachedFileStream.Write: offset must be > 0"));
            }
            else if (buffer == null)
            {
                ExceptionMethods.Throw(new Exceptions.ArgumentException("CachedFileStream.Write: buffer must not be null!"));
            }
            else if (buffer.Length - offset < count)
            {
                ExceptionMethods.Throw(new Exceptions.ArgumentException("CachedFileStream.Write: Invalid offset / length values!"));
            }


            if (CachedData.Length < ((int)Position + count))
            {
                byte[] NewCachedData = new byte[offset + count];
                FOS_System.Array.Copy(CachedData, 0, NewCachedData, 0, CachedData.Length);
                CachedData = NewCachedData;
            }

            FOS_System.Array.Copy(buffer, offset, CachedData, (int)Position, count);

            UnderlyingStream.Write(buffer, offset, count);
        }
    }
}
