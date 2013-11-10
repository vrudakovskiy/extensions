using System.Threading.Tasks;

namespace System.IO
{
    public static class StreamExtensions
    {
        public static Task<int> ReadAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            return Task<int>.Factory.FromAsync(stream.BeginRead, stream.EndRead, buffer, offset, count, null);
        }

        public static Task WriteAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            return Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, buffer, offset, count, null);
        }

        public static Stream KeepAlive(this Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            return new AliveStream(stream);
        }
    }
}
