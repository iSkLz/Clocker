using System;
using System.Collections.Generic;
using System.Net;
using System.IO;

namespace Clocker.Server
{
	[Obsolete]
	public class PathedServerO
	{
		public Dictionary<string, Tuple<Action<HttpListenerContext>, HttpListener>> Listeners;
		
		public Dictionary<string, Tuple<Dictionary<string, Action<HttpListenerContext>>, HttpListener>> FileListeners;
		public uint Port;
		
		public PathedServerO() {
			Listeners = new Dictionary<string, Tuple<Action<HttpListenerContext>, HttpListener>>();
			FileListeners = new Dictionary<string, Tuple<Dictionary<string, Action<HttpListenerContext>>, HttpListener>>();
		}
		
		public uint SetPort(uint[] ports) {
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
			Listeners.Add("/test/", new Tuple<Action<HttpListenerContext>, HttpListener>(
				(HttpListenerContext ctx) => ctx.ServeText("Online!"),
				main
			));
			
			Port = workingPort;
			return workingPort;
		}
		
		public void Add(string path, Action<HttpListenerContext> handler) {
			var listener = new HttpListener();
			Listeners.Add(path, new Tuple<Action<HttpListenerContext>, HttpListener>(handler, listener));
			listener.Prefixes.Add("http://localhost:" + Port + path);
		}
		
		public void AddSingle(string path, string filename, Action<HttpListenerContext> handler) {
			if (!FileListeners.ContainsKey(path)) {
				var listener = new HttpListener();
				listener.Prefixes.Add("http://localhost:" + Port + path);
				listener.SetHighTimeout();
				listener.Start();
				
				listener.BeginGetContext(
					new AsyncCallback(ReceiveSingleRequest),
					new Tuple<string, PathedServer, HttpListener>(path, this, listener)
				);
				
				FileListeners.Add(
					path,
					new Tuple<Dictionary<string, Action<HttpListenerContext>>, HttpListener>(
						new Dictionary<string, Action<HttpListenerContext>>(),
						listener
					)
				);
			}
			
			var server = FileListeners[path];
			server.Item1.Add(filename, handler);
		}
		
		public void AddStatic(string path, string dir) {
			var info = new DirectoryInfo(dir);
			var cache = new FileCache(info.FullName);
			var files = info.EnumerateFiles("*", SearchOption.AllDirectories);
			
			foreach (var file in files) {
				cache.Add(file.FullName);
			}
			
			Action<IAsyncResult> handle = (res) => {};
			
			handle = (res) => {
				var listener = (HttpListener)res.AsyncState;
				var context = listener.EndGetContext(res);
				listener.BeginGetContext(new AsyncCallback(handle), listener);
				
				if (!cache.Has(context.Request.RawUrl.ToLowerInvariant().Substring(path.Length))) {
					context.Response.StatusCode = 404;
					context.Response.Close();
				} else {
					var file = cache.Cache[context.Request.RawUrl.ToLowerInvariant().Substring(path.Length)];
					context.ServeBytes(file.Item2, file.Item1);
				}
			};
			
			handle = (res) => {
				var listener = (HttpListener)res.AsyncState;
				var context = listener.EndGetContext(res);
				listener.BeginGetContext(new AsyncCallback(handle), listener);
				
				if (!cache.Has(context.Request.RawUrl.ToLowerInvariant().Substring(path.Length))) {
					context.Response.StatusCode = 404;
					context.Response.Close();
				} else {
					var file = cache.Cache[context.Request.RawUrl.ToLowerInvariant().Substring(path.Length)];
					context.ServeBytes(file.Item2, file.Item1);
				}
			};
			
			var listenerB = new HttpListener();
			listenerB.Prefixes.Add("http://localhost:" + Port + path + "/");
			listenerB.Prefixes.Add("http://localhost:" + Port + path + "/+/");
			listenerB.SetHighTimeout();
			listenerB.Start();
			listenerB.BeginGetContext(new AsyncCallback(handle), listenerB);
		}
		
		public void Start() {
			foreach (var pair in Listeners) {
				pair.Value.Item2.SetHighTimeout();
				pair.Value.Item2.Start();
				pair.Value.Item2.BeginGetContext(new AsyncCallback(ReceiveRequest), pair.Value);
			}
		}
		
		public static void ReceiveSingleRequest(IAsyncResult res) {
			var state = (Tuple<string, PathedServer, HttpListener>)res.AsyncState;
			var listener = state.Item3;
			var server = state.Item2;
			var path = state.Item1;
			var ctx = listener.EndGetContext(res);
			listener.BeginGetContext(new AsyncCallback(ReceiveSingleRequest), state);
			
			var file = ctx.Request.RawUrl.Substring(path.Length);
			if (server.FileListeners.ContainsKey(path) && server.FileListeners[path].Item1.ContainsKey(file)) {
				var handler = server.FileListeners[path].Item1[file];
				handler(ctx);
			} else {
				ctx.Response.StatusCode = 404;
				ctx.Response.Close();
			}
		}
		
		public static void ReceiveRequest(IAsyncResult res) {
			var pair = (Tuple<Action<HttpListenerContext>, HttpListener>)res.AsyncState;
			var listener = pair.Item2;
			var context = listener.EndGetContext(res);
			listener.BeginGetContext(new AsyncCallback(ReceiveRequest), pair);
			var handler = pair.Item1;
			handler(context);
		}
	}
}