using System;
using System.Net;
using System.Collections.Generic;

namespace Clocker.Server
{
	/// <summary>
	/// Represents a subserver that handles requests to a specific subroute.
	/// </summary>
	public partial class PathHandler
	{
		Dictionary<string, Action<HttpListenerContext, string>> FileHandlers;
		Action<HttpListenerContext, string> BackupHandler;
		
		/// <summary>
		/// Represents the subroute the handler serves.
		/// </summary>
		public readonly string Subroute;
		
		/// <summary>
		/// Cosntructs a new instance dedicated to the given subroute.
		/// </summary>
		/// <param name="path">The subroute to serve.</param>
		public PathHandler(string path)
		{
			path = path.ToLowerInvariant();
			Subroute = path;
			FileHandlers = new Dictionary<string, Action<HttpListenerContext, string>>();
		}
		
		/// <summary>
		/// Adds a new file handler for the specified path.
		/// </summary>
		/// <param name="subpath">The path the handler serves relative to the subroute of this instance.</param>
		/// <param name="handler">The handler delegate.</param>
		/// <returns>The current instance.</returns>
		public PathHandler Add(string subpath, Action<HttpListenerContext, string> handler) {
			subpath = subpath.ToLowerInvariant();
			
			// Strip out starting and ending slashes
			if (subpath.StartsWith("/")) subpath = subpath.Substring(1);
			if (subpath.EndsWith("/")) subpath = subpath.Substring(0, subpath.Length - 1);
			
			if (Has(subpath)) return this;
			FileHandlers.Add(subpath, handler);
			return this;
		}
		
		/// <summary>
		/// Adds a new file handler for the specified path.
		/// </summary>
		/// <param name="subpath">The path the handler serves relative to the subroute of this instance.</param>
		/// <param name="handler">The handler delegate.</param>
		/// <returns>Returns the current instance.</returns>
		public PathHandler Add(string subpath, Action<HttpListenerContext> handler) {
			return Add(subpath, (HttpListenerContext ctx, string _) => { handler(ctx); });
		}
		
		/// <summary>
		/// Sets a backup handler that serves all requests that no dedicated handler can.
		/// </summary>
		/// <param name="handler">The backup handler.</param>
		/// <returns>The current instance.</returns>
		public PathHandler SetBackup(Action<HttpListenerContext, string> handler) {
			BackupHandler = handler;
			return this;
		}
		
		/// <summary>
		/// Checks whether a dedicated handler exists for the specified path.
		/// </summary>
		/// <param name="subpath">The path to check for its handler relative to the subroute of this instance.</param>
		/// <returns>True if a dedicated handler for the path exists; otherwise false.</returns>
		public bool Has(string subpath) {
			subpath = subpath.ToLowerInvariant();
			
			// Strip out starting and ending slashes
			if (subpath.StartsWith("/")) subpath = subpath.Substring(1);
			if (subpath.EndsWith("/")) subpath = subpath.Substring(0, subpath.Length - 1);
			
			return FileHandlers.ContainsKey(subpath);
		}
		
		/// <summary>
		/// Handles the given request context by passing it to the correct handler.
		/// </summary>
		/// <param name="ctx">The request context to handle.</param>
		public void HandleRequest(HttpListenerContext ctx) {
			// Cut out the subroute so we only have the subpath relative to the subroute left
			var subpath = ctx.Request.Url.AbsolutePath.Substring(Subroute.Length).ToLowerInvariant();
			if (!Has(subpath)) {
				if (BackupHandler != null) BackupHandler(ctx, subpath);
				else ctx.Ratio(subpath);
			} else {
				FileHandlers[subpath](ctx, subpath);
			}
		}
	}
}
