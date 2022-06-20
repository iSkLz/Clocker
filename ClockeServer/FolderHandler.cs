using System;
using System.IO;
using System.Net;

namespace Clocker.Server
{
	public class FolderHandler
	{
		// TODO: Switch to new file cache once complete
		public FileCacheO Cache;
		
		public FolderHandler(FileCacheO cache) {
			Cache = cache;
		}
		
		public void HandleFile(HttpListenerContext ctx, string subpath) {
			string mime;
			byte[] file;
			if (!Cache.TryGet(subpath, out mime, out file))
				ctx.Ratio("File doesn't exist");
			ctx.ServeBytes(file, mime);
		}
		
		public void AttachTo(PathHandler handler) {
			handler.BackupHandler = HandleFile;
		}
	}
}
