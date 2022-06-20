using System;
using System.Net;
using System.Collections.Generic;

namespace Clocker.Server
{
	public partial class PathHandler
	{
		public Dictionary<string, Action<HttpListenerContext, string>> FileHandlers;
		public Action<HttpListenerContext, string> BackupHandler;
		public readonly string FolderPath;
		
		public PathHandler(string path)
		{
			path = path.ToLowerInvariant();
			FolderPath = path;
			FileHandlers = new Dictionary<string, Action<HttpListenerContext, string>>();
		}
		
		public PathHandler Add(string subpath, Action<HttpListenerContext, string> handler) {
			subpath = subpath.ToLowerInvariant();
			if (subpath.StartsWith("/")) subpath = subpath.Substring(1);
			if (subpath.EndsWith("/")) subpath = subpath.Substring(0, subpath.Length - 1);
			if (Has(subpath)) return this;
			FileHandlers.Add(subpath, handler);
			return this;
		}
		
		public PathHandler Add(string subpath, Action<HttpListenerContext> handler) {
			return Add(subpath, (HttpListenerContext ctx, string _) => { handler(ctx); });
		}
		
		public PathHandler SetBackup(Action<HttpListenerContext, string> handler) {
			BackupHandler = handler;
			return this;
		}
		
		public bool Has(string subpath) {
			subpath = subpath.ToLowerInvariant();
			if (subpath.StartsWith("/")) subpath = subpath.Substring(1);
			if (subpath.EndsWith("/")) subpath = subpath.Substring(0, subpath.Length - 1);
			return FileHandlers.ContainsKey(subpath);
		}
		
		public void HandleRequest(HttpListenerContext ctx) {
			//var subpath = ctx.Request.RawUrl.Substring(FolderPath.Length).ToLowerInvariant();
			var subpath = ctx.Request.Url.AbsolutePath.Substring(FolderPath.Length).ToLowerInvariant();
			if (!Has(subpath)) {
				if (BackupHandler != null) BackupHandler(ctx, subpath);
				else ctx.Ratio(subpath);
			} else {
				FileHandlers[subpath](ctx, subpath);
			}
		}
	}
}
