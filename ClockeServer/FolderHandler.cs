using System;
using System.Net;

namespace Clocker.Server
{
	public class FolderHandler
	{
		// TODO: Add IO methods and document
		public FileCache Cache;
		
		public FolderHandler(FileCache cache) {
			Cache = cache;
		}
		
		public void HandleFile(HttpListenerContext ctx, string subpath) {
			MemoryFile file;
			if (!Cache.TryGet(subpath, out file))
				ctx.Ratio("File doesn't exist");
			ctx.ServeFile(file);
		}
		
		public void AttachTo(PathHandler handler) {
			handler.SetBackup(HandleFile);
		}
	}
}
