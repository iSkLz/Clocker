using System;
using System.IO;
using System.Collections.Generic;

namespace Clocker.Server
{
	public class Buffer {
		byte[] bfr;
		
		public int Length {
			get {
				return bfr.Length;
			}
		}
		
		public long Length64 {
			get {
				return bfr.LongLength;
			}
		}
		
		public Buffer(byte[] bfr) {
			this.bfr = bfr;
		}
		
		public Buffer(Stream stream) {
			if (stream == null)
				throw new ArgumentNullException("stream", "Input stream cannot be null");
			if (!stream.CanSeek)
				throw new ArgumentException("Non-seekable stream passed.", "stream");
			if (!stream.CanRead)
				throw new ArgumentException("Non-readable stream passed.", "stream");
			
			stream.Seek(0, SeekOrigin.Begin);
			bfr = new byte[(int)stream.Length];
			stream.Read(bfr, 0, (int)stream.Length);
			stream.Close();
		}
		
		public void WriteTo(Stream stream) {
			if (stream == null)
				throw new ArgumentNullException("stream", "Output stream cannot be null");
			stream.Write(bfr, 0, Length);
		}
	}
	
	public struct MemoryFile {
		public Buffer Content;
		public string MimeType;
		
		public MemoryFile(Buffer buffer, string nameOrMime) {
			if (buffer == null)
				throw new ArgumentNullException("buffer", "File buffer cannot be null");
			Content = buffer;
			if (String.IsNullOrWhiteSpace(nameOrMime)) MimeType = (".bin").MimeOf();
			if (nameOrMime.StartsWith(".") || Extensions.MimeRef.ContainsKey(nameOrMime)) MimeType = nameOrMime.MimeOf();
			else MimeType = new FileInfo(nameOrMime).WebExt().MimeOf();
		}
	}
	
	public interface IFileResolver {
		MemoryFile Resolve(string subpath);	
		bool TryResolve(string subpath, out MemoryFile file);
	}
	
	public class FileResolverGroup : IFileResolver {
		List<IFileResolver> Resolvers = new List<IFileResolver>();
		
		public void Add(IFileResolver resolver) {
			Resolvers.Add(resolver);
		}
		
		public void Clear() {
			Resolvers.Clear();
		}
		
		public MemoryFile Resolve(string subpath) {
			foreach (var resolver in Resolvers) {
				MemoryFile file;
				if (!resolver.TryResolve(subpath, out file)) continue;
				return file;
			}
			throw new KeyNotFoundException("No resolver in the group was able to resolve the file");
		}
		
		public bool TryResolve(string subpath, out MemoryFile file) {
			foreach (var resolver in Resolvers) {
				if (!resolver.TryResolve(subpath, out file)) continue;
				return true;
			}
			file = default(MemoryFile);
			return false;
		}
	}
}
