using System;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Clocker.Server;
using Celeste.Mod;

namespace Clocker.Mod
{
	/// <summary>
	/// Defines methods for resolving files from the operating system's file system.
	/// </summary>
	public class SystemResolver : IFileResolver {
		/// <summary>
		/// The root of the resolver.
		/// </summary>
		public string Root { get; private set; }
		
		/// <summary>
		/// Constructs a new instance with the given root.
		/// </summary>
		/// <param name="root">The root to use.</param>
		public SystemResolver(string root) {
			Root = root;
		}
		
		/// <inheritdoc/>
		public bool TryResolve(string path, out MemoryFile output) {
			var file = new FileInfo(Path.Combine(Root, path));
			output = default(MemoryFile);
			if (!file.Exists) return false;
			output = new MemoryFile(new MemoryBuffer(file.OpenRead()), file.WebExt());
			return true;
		}
		
		/// <inheritdoc/>
		public MemoryFile Resolve(string path) {
			MemoryFile file;
			TryResolve(path, out file);
			return file;
		}
		
		/// <inheritdoc/>
		public IFileResolver SubResolver(string subdir) {
			return new SystemResolver(Path.Combine(Root, subdir));
		}
	}
	
	/// <summary>
	/// Defines methods for resolving files from inside a zip file object.
	/// </summary>
	public class ZipResolver : IFileResolver {
		/// <summary>
		/// The zip of the resolver.
		/// </summary>
		public ZipFile Zip { get; private set; }
		
		/// <summary>
		/// The root of the resolver (inside the zip).
		/// </summary>
		public string Root {
			get {
				return root;
			}
			set {
				root = value.WebName();
			}
		}
		string root = "/";
		
		/// <summary>
		/// Constructs a new instance for the given zip file.
		/// </summary>
		/// <param name="zip">The zip file to use.</param>
		public ZipResolver(ZipFile zip) {
			Zip = zip;
		}
		
		/// <inheritdoc/>
		public bool TryResolve(string path, out MemoryFile output) {
			path = Root + path.WebName();
			
			foreach (var e in Zip.Entries) {
				if (e.IsDirectory && e.FileName == path) {
					output = new MemoryFile(new MemoryBuffer(e.ExtractStream()), path);
					return true;
				}
			}
			
			output = default(MemoryFile);
			return false;
		}
		
		/// <inheritdoc/>
		public MemoryFile Resolve(string path) {
			MemoryFile file = default(MemoryFile);
			TryResolve(path, out file);
			return file;
		}
	}
}
