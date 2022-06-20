using System;
using System.IO;
using System.Collections.Generic;

namespace Clocker.Server
{
	/// <summary>
	/// Represents a buffer of memory.
	/// </summary>
	/// <remarks>
	/// This class is a wrapper of byte[].
	/// </remarks>
	public class Buffer {
		byte[] bfr;
		
		/// <summary>
		/// Returns the contents of the buffer.
		/// </summary>
		public byte[] Bytes {
			get {
				return bfr;
			}
		}
		
		/// <summary>
		/// Represents the size of the buffer (in bytes).
		/// </summary>
		public int Length {
			get {
				return bfr.Length;
			}
		}
		
		/// <summary>
		/// Represents the size of the buffer (in bytes).
		/// </summary>
		public long Length64 {
			get {
				return bfr.LongLength;
			}
		}
		
		/// <summary>
		/// Constructs a new instance from an array of bytes.
		/// </summary>
		/// <param name="bfr">The array of bytes to use.</param>
		public Buffer(byte[] arr) {
			bfr = arr;
		}
		
		/// <summary>
		/// Constructs a new instance from a readable stream.
		/// </summary>
		/// <param name="stream">The stream to read from.</param>
		/// <param name="length">The number of bytes to read. Use a negative value to read the entire stream.</param>
		/// <exception cref="ArgumentNullException">Thrown when a null stream was provided.</exception>
		/// <exception cref="ArgumentException">Thrown when a stream that doesn't support reading was passed.</exception>
		public Buffer(Stream stream, int length = -1) {
			if (stream == null)
				throw new ArgumentNullException("stream", "Input stream cannot be null");
			if (!stream.CanRead)
				throw new ArgumentException("Non-readable stream passed.", "stream");
			
			if (length < 0) length = (int)stream.Length;
			
			if (stream.CanSeek)
				stream.Seek(0, SeekOrigin.Begin);
			
			bfr = new byte[length];
			stream.Read(bfr, 0, length);
			stream.Close();
		}
		
		/// <summary>
		/// Writes the contents of the buffer to a stream.
		/// </summary>
		/// <param name="stream">The output stream.</param>
		/// <param name="start">The index from which in the buffer to start writing.</param>
		/// <param name="length">The number of bytes to write.</param>
		/// <exception cref="ArgumentNullException">Thrown when a null stream was provided.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when a negative starting index was provided.</exception>
		/// <exception cref="IndexOutOfRangeException">Thrown when the provided starting index and length exceed the buffer's length.</exception>
		/// <exception cref="ArgumentException">Thrown when an unwriteable stream was provided.</exception>
		public void WriteTo(Stream stream, int start = 0, int length = -1) {
			if (stream == null)
				throw new ArgumentNullException("stream", "Output stream cannot be null");
			if (start < 0)
				throw new ArgumentOutOfRangeException("start", "Starting index cannot be negative");
			if (length < 0) length = Length;
			if ((start + length) > Length)
				throw new IndexOutOfRangeException("Starting index and length are too big");
			if (!stream.CanWrite)
				throw new ArgumentException("Output stream must be writeable", "stream");
			
			stream.Write(bfr, start, length);
		}
		
		/// <summary>
		/// Writes the contents of the buffer to a bytes array.
		/// </summary>
		/// <param name="target">The array to write to.</param>
		/// <param name="startFrom">The index from which in the buffer to start writing.</param>
		/// <param name="lengthFrom">The number of bytes to write.</param>
		/// <param name="startDest">The index at which in the target to start writing.</param>
		/// <exception cref="ArgumentNullException">Thrown when a null output array was provided.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when a negative starting index was provided.</exception>
		/// <exception cref="IndexOutOfRangeException">Thrown when the provided starting index and length exceed the buffer's length 
		/// or the output array's length.</exception>
		/// <exception cref="ArgumentException">Thrown when an unwriteable stream was provided.</exception>
		public void WriteTo(byte[] target, int startFrom = 0, int startDest = 0, int length = -1) {
			if (target == null)
				throw new ArgumentNullException("target", "Output array cannot be null");
			if (startFrom < 0)
				throw new ArgumentException("Starting index cannot be negative", "startFrom");
			if (startDest < 0)
				throw new ArgumentException("Starting index cannot be negative", "startDest");
			if (length < 0) length = Length;
			if (target.Length < (startDest + length))
				throw new IndexOutOfRangeException("The output array is too small");
			if ((startFrom + length) > Length)
				throw new IndexOutOfRangeException("Starting index and length are too big");
			
			for (int i = 0; i < length; i++) {
				target[i + startDest] = bfr[i + startFrom];
			}
		}
	}
}
