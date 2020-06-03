using System;
using System.IO;
using NAudio.Wave;

namespace Locutius.Capture.Providers
{
    class StreamSampleProvider : Stream, ISampleProvider
    {
        private readonly MemoryStream innerStream;
        private long readPosition;
        private long writePosition;

        public StreamSampleProvider(WaveFormat WaveFormat)
        {
            innerStream = new MemoryStream();
            this.WaveFormat = WaveFormat;
        }

        public WaveFormat WaveFormat { get; }

        /// <summary>
        ///  Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead { get { return true; } }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite { get { return true; } }

        public override void Flush()
        {
            innerStream.Flush();
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                return innerStream.Length;
            }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// This method reads a sequence of bytes from the current stream and advances the position 
        /// within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            innerStream.Position = readPosition;
            int red = innerStream.Read(buffer, offset, count);
            readPosition = innerStream.Position;

            return red;
        }

        /// <summary>
        /// Fill the specified buffer with 32 bit floating point samples.
        /// </summary>
        /// <param name="buffer">An array of float.</param>
        /// <param name="offset">Offset in buffer at which to begin storing the data read.</param>
        /// <param name="count">The number of samples to be read from the current stream.</param>
        /// <returns>The total number of floating point samples read into the buffer.</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            int bytesNeeded = count * 4;
            byte[] localbuffer = new byte[bytesNeeded];
            int bytesRead = this.Read(localbuffer, 0, bytesNeeded);
            int samplesRead = bytesRead / 4;
            int outputIndex = offset;
            for (int n = 0; n < bytesRead; n += 4)
            {
                buffer[outputIndex++] = BitConverter.ToSingle(localbuffer, n);
            }
            return samplesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            innerStream.Position = writePosition;
            innerStream.Write(buffer, offset, count);
            writePosition = innerStream.Position;
        }
    }
}
