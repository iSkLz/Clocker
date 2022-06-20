using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace Clocker.Server
{
	public class PathedServer
	{
		public class PathSorter : IComparer<string> {
			public int Compare(string a, string b) {
				return Math.Sign(b.Length - a.Length);
			}
		}
		
		public interface ILogger {
			void Log(HttpListenerContext req);
		}
		
		public HttpListener Listener;
		public uint Port;
		public uint ThreadCount = 1;
		
		public SortedDictionary<string, PathHandler> Handlers;
		private PathSorter sorter;
		
		public ILogger Logger = null;
		
		public PathedServer()
		{
			Handlers = new SortedDictionary<string, PathHandler>(sorter = new PathSorter());
			Listener = new HttpListener();
		}
		
		public uint SetPort(params uint[] ports) {
			uint workingPort = 0;
			HttpListener main = null;
			foreach (var port in ports) {
				var listener = new HttpListener();
				listener.Prefixes.Add("http://localhost:" + port + "/test/");
				
				try {
					listener.Start();
				}
				catch {
					continue;
				}
				
				workingPort = port;
				main = listener;
				break;
			}
			
			if (workingPort == 0) {
				Random rng = new Random();
				while (true) {
					var port = rng.RandomPort();
					var listener = new HttpListener();
					listener.Prefixes.Add("http://localhost:" + port + "/test/");
					
					try {
						listener.Start();
					}
					catch {
						continue;
					}
					
					workingPort = port;
					main = listener;
					break;
				}
			}
			
			main.Stop();
			main.Close();
			
			Port = workingPort;
			return workingPort;
		}
		
		public uint SetPort() {
			return SetPort(new uint[] {});
		}
		
		public void SetThreads(uint count) {
			ThreadCount = count;
		}
		
		public void Start() {
			Listener.SetHighTimeout();
			Listener.Prefixes.Add("http://localhost:" + Port + "/");
			Listener.Start();
			
			for (int n = 0; n < ThreadCount; n++)
				Listener.BeginGetContext(new AsyncCallback(HandleContext), this);
		}
		
		public PathHandler Add(string path) {
			path = path.ToLowerInvariant();
			if (!path.StartsWith("/")) path = "/" + path;
			if (!path.EndsWith("/")) path += "/";
			PathHandler handler = new PathHandler(path);
			Handlers.Add(path, handler);
			return handler;
		}
		
		public PathHandler Add<T>() where T : class, new() {
			PathHandler handler;
			T inst;
			if (this.Scan(out handler, out inst)) {
				return handler;
			} else {
				throw new ArgumentException("The provided generic type doesn't have the scannable attribute.", "instance");
			}
		}
		
		public PathHandler Add<T>(out T instance) where T : class, new() {
			PathHandler handler;
			T inst;
			if (this.Scan(out handler, out inst)) {
				instance = inst;
				return handler;
			} else {
				throw new ArgumentException("The provided generic type doesn't have the scannable attribute.", "instance");
			}
		}
		
		public bool Has(string path) {
			path = path.ToLowerInvariant();
			return Handlers.ContainsKey(path);
		}
		
		public PathHandler Get(string path) {
			path = path.ToLowerInvariant();
			return Has(path) ? Handlers[path] : null;
		}
		
		public bool TryGet(string path, out PathHandler handler) {
			handler = Get(path);
			return Has(path);
		}
		
		public static void HandleContext(IAsyncResult res) {
			var server = (PathedServer)res.AsyncState;
			var ctx = server.Listener.EndGetContext(res);
			server.Listener.BeginGetContext(new AsyncCallback(HandleContext), server);
			HandleRequest(ctx, server);
		}
		
		public static void HandleRequest(HttpListenerContext ctx, PathedServer server) {
			if (server.Logger != null) server.Logger.Log(ctx);
			var url = ctx.Request.RawUrl.ToLowerInvariant();
			bool served = false;
			
			foreach (var handler in server.Handlers) {
				if (url.StartsWith(handler.Key)) {
					handler.Value.HandleRequest(ctx);
					served = true;
					break;
				}
			}
			
			if (!served) ctx.Ratio("Server");
		}
	}
}
