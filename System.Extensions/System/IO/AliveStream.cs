namespace System.IO
{
    public sealed class AliveStream : Stream
    {
        private readonly Stream _underlyingStream;

        public AliveStream(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            _underlyingStream = stream;
        }

        public override bool CanRead
        {
            get { return _underlyingStream.CanRead; }
        }

        public override bool CanWrite
        {
            get { return _underlyingStream.CanWrite; }
        }

        public override bool CanSeek
        {
            get { return _underlyingStream.CanSeek; }
        }

        public override bool CanTimeout
        {
            get { return _underlyingStream.CanTimeout; }
        }

        public override long Length
        {
            get { return _underlyingStream.Length; }
        }

        public override long Position
        {
            get
            {
                return _underlyingStream.Position;
            }
            set
            {
                _underlyingStream.Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return _underlyingStream.ReadTimeout;
            }
            set
            {
                _underlyingStream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return _underlyingStream.WriteTimeout;
            }
            set
            {
                _underlyingStream.WriteTimeout = value;
            }
        }

        public override int ReadByte()
        {
            return _underlyingStream.ReadByte();
        }

        public override void WriteByte(byte value)
        {
            _underlyingStream.WriteByte(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _underlyingStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _underlyingStream.Write(buffer, offset, count);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _underlyingStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return _underlyingStream.EndRead(asyncResult);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _underlyingStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            _underlyingStream.EndWrite(asyncResult);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _underlyingStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _underlyingStream.SetLength(value);
        }

        public override void Flush()
        {
            _underlyingStream.Flush();
        }

        public override void Close()
        {
            Flush();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }
    }
}
