using System;
using System.IO;
using System.Collections.Generic;

namespace Clocker.Server
{
	/// <summary>
	/// Represents a cache that lets you store binary buffers in memory and their MIME types indexed by paths.
	/// </summary>
	public class FileCache
	{
		#region Initials
		Dictionary<string, MemoryFile> CachedFiles = new Dictionary<string, MemoryFile>();
		
		/// <summary>
		/// Gets or sets the resolver used to acquire file contents from a subpath.
		/// </summary>
		public IFileResolver Resolver;
		
		/// <summary>
		/// Constructs a new file cache with no file resolver.
		/// </summary>
		public FileCache() {
		}
		
		/// <summary>
		/// Constructs a new file cache with the given file resolver.
		/// </summary>
		/// <param name="resolver">The resolver to use.</param>
		public FileCache(IFileResolver resolver) {
			Resolver = resolver;
		}
		#endregion
		
		#region Add methods
		/// <summary>
		/// Add a new file to the cache using the set resolver.
		/// </summary>
		/// <param name="subpath">Path of the file to add.</param>
		/// <exception cref="ArgumentNullException">Thrown when a null file path is provided.</exception>
		/// <exception cref="InvalidOperationException">Thrown when no file resolver has been set prior to the method call.</exception>
		public void Add(string subpath) {
			if (subpath == null) throw new ArgumentNullException("subpath", "File path cannot be null");
			if (Resolver == null) throw new InvalidOperationException("No file resolver is set");
			var file = Resolver.Resolve(subpath);
			CachedFiles.Add(subpath, file);
		}
		
		/// <summary>
		/// Attempts to add a new file to the cache using the set resolver.
		/// </summary>
		/// <param name="subpath"></param>
		/// <returns>True if the file was added to the cache successfully; otherwise false.</returns>
		/// <exception cref="ArgumentNullException">Thrown when a null file path is provided.</exception>
		/// <exception cref="InvalidOperationException">Thrown when no file resolver has been set prior to the method call.</exception>
		public bool TryAdd(string subpath) {
			if (subpath == null) throw new ArgumentNullException("subpath", "File path cannot be null");
			if (Resolver == null) throw new InvalidOperationException("No file resolver is set");
			try {
				Add(subpath);
			} catch (Exception) {
				// If any exceptions occur, the method failed
				return false;
			}
			return true;
		}
		
		/// <summary>
		/// Adds a file to the cache labelled by its path.
		/// </summary>
		/// <param name="subpath">Path of the file to add.</param>
		/// <param name="file">The file to add.</param>
		/// <exception cref="ArgumentNullException">Thrown when a null file path is provided.</exception>
		public void Add(string subpath, MemoryFile file) {
			if (subpath == null) throw new ArgumentNullException("subpath", "File path cannot be null");
			CachedFiles.Add(subpath, file);
		}
		
		/// <summary>
		/// Adds a file to the cache from its contents labelled by its path.
		/// </summary>
		/// <remarks>
		/// This method automatically decides the mime type based on the path extension.
		/// </remarks>
		/// <param name="subpath">Path of the file to add.</param>
		/// <param name="buffer">Contents of the file to add.</param>
		/// <exception cref="ArgumentNullException">Thrown when a null file path is provided.</exception>
		/// <exception cref="ArgumentNullException">Thrown when a null buffer is provided.</exception>
		public void Add(string subpath, MemoryBuffer buffer) {
			if (subpath == null) throw new ArgumentNullException("subpath", "File path cannot be null");
			if (buffer == null) throw new ArgumentNullException("buffer", "File buffer cannot be null");
			CachedFiles.Add(subpath, new MemoryFile(buffer, subpath));
		}
		#endregion
		
		#region Utils
		/// <summary>
		/// Checks whether the file labelled by the specified path is in cache.
		/// </summary>
		/// <param name="subpath">Path of the file to check.</param>
		/// <returns>True if the file is cached in memory; otherwise false.</returns>
		public bool Has(string subpath) {
			if (subpath == null) throw new ArgumentNullException("subpath", "File path cannot be null");
			return CachedFiles.ContainsKey(subpath);
		}
		
		/// <summary>
		/// Clears the cache.
		/// </summary>
		public void Clear() {
			lock (CachedFiles) {
				CachedFiles.Clear();
			}
		}
		
		/// <summary>
		/// Enumerates all files in the cache.
		/// </summary>
		/// <returns>An enumerable object that enumertes all files in the cache.</returns>
		public IEnumerable<KeyValuePair<string, MemoryFile>> EnumerateFiles() {
			lock (CachedFiles) {
				foreach (var file in CachedFiles) {
					yield return file;
				}
			}
		}
		#endregion
		
		#region Get methods
		/// <summary>
		/// Retrieves the file labelled by the specified path from the cache.
		/// </summary>
		/// <param name="subpath">Path of the file to retrieve.</param>
		/// <returns>The cached file.</returns>
		/// <exception cref="ArgumentNullException">Thrown when a null file path is provided.</exception>
		/// <exception cref="KeyNotFoundException">Thrown when the file labelled by the specified subpath hasn't been cached prior to the call.</exception>
		public MemoryFile Get(string subpath) {
			if (subpath == null) throw new ArgumentNullException("subpath", "File path cannot be null");
			if (!Has(subpath)) throw new KeyNotFoundException("No data for the specified subpath is cached in memory");
			return CachedFiles[subpath];
		}
		
		/// <summary>
		/// Attempts to retrieve the file labelled by the specified path from the cache.
		/// </summary>
		/// <param name="subpath">Path of the file to retrieve.</param>
		/// <param name="file">The variable to write the file reference to if found.</param>
		/// <returns>True if the file was found; otherwise false.</returns>
		/// <exception cref="ArgumentNullException">Thrown when a null file path is provided.</exception>
		public bool TryGet(string subpath, out MemoryFile file) {
			if (subpath == null) throw new ArgumentNullException("subpath", "File path cannot be null");
			if (!Has(subpath)) {
				file = default(MemoryFile);
				return false;
			}
			file = Get(subpath);
			return true;
		}
		
		/// <summary>
		/// Attempts to retrieve the file labelled by the specified from the cache, and adds it if it's not cached.
		/// </summary>
		/// <param name="subpath">Path of the file to retrieve or add.</param>
		/// <returns>The cached file or the newly added file.</returns>
		/// <exception cref="ArgumentNullException">Thrown when a null file path is provided.</exception>
		public MemoryFile GetOrAdd(string subpath) {
			if (subpath == null) throw new ArgumentNullException("subpath", "File path cannot be null");
			MemoryFile file;
			if (TryGet(subpath, out file)) return file;
			file = Resolver.Resolve(subpath);
			Add(subpath, file);
			return file;
		}
		
		/// <summary>
		/// Attempts to retrieve the file labelled by the specified from the cache, and adds it if it's not cached.
		/// </summary>
		/// <param name="subpath">Path of the file to retrieve or add.</param>
		/// <param name="output">The cached file or the newly added file.</param>
		/// <returns>True if the file was returned; otherwise false.</returns>
		/// <exception cref="ArgumentNullException">Thrown when a null file path is provided.</exception>
		public bool TryGetOrAdd(string subpath, out MemoryFile output) {
			if (subpath == null) throw new ArgumentNullException("subpath", "File path cannot be null");
			MemoryFile file;
			
			if (TryGet(subpath, out file)) {
				output = file;
				return true;
			}
			
			if (Resolver.TryResolve(subpath, out file)) {
				Add(subpath, file);
				output = file;
				return true;
			}
			
			output = default(MemoryFile);
			return false;
		}
		#endregion
		
		#region Refresh methods
		/// <summary>
		/// Refreshes the file specified by the path in the cache using the set resolver.
		/// </summary>
		/// <param name="subpath">Path of the file to refresh.</param>
		/// <exception cref="ArgumentNullException">Thrown when a null file path is provided.</exception>
		/// <exception cref="InvalidOperationException">Thrown when no file resolver has been set prior to the method call.</exception>
		/// <exception cref="InvalidOperationException">Thrown when the file wasn't cached prior to the call.</exception>
		public void Refresh(string subpath) {
			if (subpath == null) throw new ArgumentNullException("subpath", "File path cannot be null");
			if (Resolver == null) throw new InvalidOperationException("No file resolver is set");
			if (!Has(subpath)) throw new InvalidOperationException("The file hasn't been cached yet");
			var file = Resolver.Resolve(subpath);
			CachedFiles[subpath] = file;
		}
		
		/// <summary>
		/// Refreshes the file specified by the path in the cache using the given memory file.
		/// </summary>
		/// <param name="subpath">Path of the file to add.</param>
		/// <param name="file">The file to add.</param>
		/// <exception cref="ArgumentNullException">Thrown when a null file path is provided.</exception>
		/// <exception cref="InvalidOperationException">Thrown when the file wasn't cached prior to the call.</exception>
		public void Refresh(string subpath, MemoryFile file) {
			if (subpath == null) throw new ArgumentNullException("subpath", "File path cannot be null");
			if (!Has(subpath)) throw new InvalidOperationException("The file hasn't been cached yet");
			CachedFiles[subpath] = file;
		}
		
		/// <summary>
		/// Refreshes the file specified by the path in the cache using the given buffer.
		/// </summary>
		/// <param name="subpath">Path of the file to refresh.</param>
		/// <param name="buffer">Contents of the file to refresh.</param>
		/// <exception cref="ArgumentNullException">Thrown when a null file path is provided.</exception>
		/// <exception cref="ArgumentNullException">Thrown when a null buffer is provided.</exception>
		/// <exception cref="InvalidOperationException">Thrown when the file wasn't cached prior to the call.</exception>
		public void Refresh(string subpath, MemoryBuffer buffer) {
			if (subpath == null) throw new ArgumentNullException("subpath", "File path cannot be null");
			if (buffer == null) throw new ArgumentNullException("buffer", "File buffer cannot be null");
			if (!Has(subpath)) throw new InvalidOperationException("The file hasn't been cached yet");
			var file = CachedFiles[subpath];
			file.Content = buffer;
		}
		#endregion
	}
}
