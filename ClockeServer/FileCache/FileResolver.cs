using System.Collections.Generic;

namespace Clocker.Server
{
	/// <summary>
	/// Defines methods for resolving a file given its path.
	/// </summary>
	public interface IFileResolver {
		/// <summary>
		/// Resolves the file from the specified path.
		/// </summary>
		/// <param name="subpath">Path of the file.</param>
		/// <returns>The resolved file.</returns>
		MemoryFile Resolve(string subpath);
		
		/// <summary>
		/// Attempts to resolve the file from the specified path.
		/// </summary>
		/// <param name="subpath">Path of the file.</param>
		/// <param name="file">The variable to output the resolved file to.</param>
		/// <returns>Whether the file was resolved successfully or not.</returns>
		bool TryResolve(string subpath, out MemoryFile file);
	}
	
	/// <summary>
	/// Represents a list of fallback-in-order file resolvers.
	/// </summary>
	public class FileResolverGroup : IFileResolver {
		List<IFileResolver> Resolvers = new List<IFileResolver>();
		
		/// <summary>
		/// Add a file resolver to the group.
		/// </summary>
		/// <param name="resolver">The resolver to add.</param>
		public void Add(IFileResolver resolver) {
			Resolvers.Add(resolver);
		}
		
		/// <summary>
		/// Clears the group out of resolvers.
		/// </summary>
		public void Clear() {
			Resolvers.Clear();
		}
		
		/// <inheritdoc/>
		public MemoryFile Resolve(string subpath) {
			foreach (var resolver in Resolvers) {
				MemoryFile file;
				if (!resolver.TryResolve(subpath, out file)) continue;
				return file;
			}
			throw new KeyNotFoundException("No resolver in the group was able to resolve the file");
		}
		
		/// <inheritdoc/>
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
