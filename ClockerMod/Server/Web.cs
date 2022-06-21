using System;
using System.Net;
using System.Collections.Generic;

using Clocker.Server;

namespace Clocker.Mod
{
	public partial class Server
	{
		// This locking mechanism is to ensure websites are added in the correct time (before intializing)
		// Adding at the wrong time simply throws an exception instead of confusing the programmer about why it isn't working
		bool locked = false;
		static Dictionary<string, FileCache> websites = new Dictionary<string, FileCache>();
		public static Dictionary<string, FileCache> Websites { get { return locked ? null : websites; } }
		
		public void InitStatic() {
			foreach (var site in websites) {
				Http.Add("/web/" + site.Key).SetBackup(WrapWebsite(site.Value));
			}
			locked = true;
		}
		
		public void UnloadStatic() {
			foreach (var site in websites) {
				site.Value.Clear();
			}
			websites.Clear();
		}
		
		Action<HttpListenerContext, string> WrapWebsite(FileCache cache) {
			return (HttpListenerContext ctx, string subpath) => {
				MemoryFile file;
				if (cache.TryGetOrAdd(subpath, out file)) {
					ctx.ServeFile(file);
				} else ctx.Ratio("Couldn't find the file");
			}:
		}
	}
}
