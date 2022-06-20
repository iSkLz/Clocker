using System;
using System.IO;

namespace Clocker.Server
{
	/// <summary>
	/// Represents a file stored in memory.
	/// </summary>
	public struct MemoryFile {
		/// <summary>
		/// The contents of the file.
		/// </summary>
		public Buffer Content;
		
		/// <summary>
		/// The MIME type of the file.
		/// </summary>
		public string MimeType;
		
		/// <summary>
		/// Constructs a new instance from the given buffer and optionally given MIME type/hint.
		/// </summary>
		/// <param name="buffer">The buffer containing the contents of the file.</param>
		/// <param name="nameOrMime">The MIME type of the file or its name to guess from.</param>
		/// <exception cref="ArgumentNullException">Thrown when a null buffer was provided.</exception>
		public MemoryFile(Buffer buffer, string nameOrMime) {
			if (buffer == null)
				throw new ArgumentNullException("buffer", "File buffer cannot be null");
			Content = buffer;
			if (String.IsNullOrWhiteSpace(nameOrMime)) MimeType = (".bin").MimeOf();
			if (nameOrMime.StartsWith(".") || Extensions.MimeRef.ContainsKey(nameOrMime)) MimeType = nameOrMime.MimeOf();
			else MimeType = new FileInfo(nameOrMime).WebExt().MimeOf();
		}
	}
}
