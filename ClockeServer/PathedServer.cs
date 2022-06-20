using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace Clocker.Server
{
	/// <summary>
	/// Represents an HTTP server that supports attaching subservers.
	/// </summary>
	public class PathedServer
	{
		#region Nested utils
		/// <summary>
		/// Defines a method that sorts path by priority (longest to shortest).
		/// </summary>
		public class PathSorter : IComparer<string> {
			public int Compare(string a, string b) {
				// The difference's sign signifies which one to put first
				return Math.Sign(b.Length - a.Length);
			}
		}
		
		/// <summary>
		/// Defines a method for logging an incoming server request.
		/// </summary>
		public interface ILogger {
			void Log(HttpListenerContext req);
		}
		#endregion
		
		#region Fields/Properties/Constructor
		/// <summary>
		/// Represents the listener the server receives requests from.
		/// </summary>
		public HttpListener Listener;
		
		/// <summary>
		/// Returns the port the server uses.
		/// </summary>
		public uint Port { get; private set; }
		
		/// <summary>
		/// Returns the number of threads the server uses.
		/// </summary>
		public uint ThreadCount { get; private set; }
		
		/// <summary>
		/// Contains the subservers/handlers attached to the server.
		/// </summary>
		public readonly SortedDictionary<string, PathHandler> Handlers;
		private PathSorter sorter;
		
		/// <summary>
		/// Represents the logger the server uses.
		/// </summary>
		public ILogger Logger = null;
		
		/// <summary>
		/// Constructs a new instance.
		/// </summary>
		public PathedServer()
		{
			Handlers = new SortedDictionary<string, PathHandler>(sorter = new PathSorter());
			Listener = new HttpListener();
			ThreadCount = 1;
		}
		#endregion
		
		#region Configurational methods
		/// <summary>
		/// Sets the port of the server to the first available port of the list, or a random one if all are unavailable.
		/// </summary>
		/// <param name="ports">The list of ports to use.</param>
		/// <returns>The port that was actually set.</returns>
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
		
		/// <summary>
		/// Configures the server to boot up the specified number of threadings when starting.
		/// </summary>
		/// <param name="count">The number of threads to use.</param>
		public void SetThreads(uint count) {
			ThreadCount = count;
		}
		
		public void Start() {
			// Assign a random port if no one has been set
			if (Port == 0) SetPort();
			
			Listener.SetHighTimeout();
			Listener.Prefixes.Add("http://localhost:" + Port + "/");
			Listener.Start();
			
			for (int n = 0; n < ThreadCount; n++)
				Listener.BeginGetContext(new AsyncCallback(HandleContext), this);
		}
		#endregion
		
		#region Adders
		/// <summary>
		/// Creates and attaches a subserver for the specified path.
		/// </summary>
		/// <param name="path">The path for the subserver to handle.</param>
		/// <returns>The attached PathHandler instance.</returns>
		public PathHandler Add(string path) {
			// Format the path
			path = path.ToLowerInvariant();
			if (!path.StartsWith("/")) path = "/" + path;
			if (!path.EndsWith("/")) path += "/";
			// Create & add
			PathHandler handler = new PathHandler(path);
			Handlers.Add(path, handler);
			return handler;
		}
		
		/// <summary>
		/// Scans, creates an instace of and adds respective handlers from the specified class.
		/// </summary>
		/// <returns>The attached PathHandler that wraps the class instance.</returns>
		public PathHandler Add<T>() where T : class, new() {
			PathHandler handler;
			T inst;
			if (this.Scan(out handler, out inst)) {
				return handler;
			} else {
				throw new ArgumentException("The provided generic type doesn't have the scannable attribute.", "instance");
			}
		}
		
		/// <summary>
		/// Scans, creates an instace of and adds respective handlers from the specified class.
		/// </summary>
		/// <param name="instance">The variable to output the class instance to.</param>
		/// <returns>The attached PathHandler that wraps the class instance.</returns>
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
		#endregion
		
		#region Getters
		/// <summary>
		/// Checks if the server has a subserver that handles the specified path.
		/// </summary>
		/// <param name="path">The path to check.</param>
		/// <returns>True if a handler for the path exists; otherwise false.</returns>
		public bool Has(string path) {
			path = path.ToLowerInvariant();
			return Handlers.ContainsKey(path);
		}
		
		/// <summary>
		/// Retrieves the subserver that handles the specified path.
		/// </summary>
		/// <param name="path">The path whose handler to get.</param>
		/// <returns>The handler instance if it exists; otherwise null.</returns>
		public PathHandler Get(string path) {
			path = path.ToLowerInvariant();
			return Has(path) ? Handlers[path] : null;
		}
		
		/// <summary>
		/// Attempts to retrieve the subserver that handles the specified path.
		/// </summary>
		/// <param name="path">The path whose handler to get.</param>
		/// <param name="handler">The variable to place the handler reference in.</param>
		/// <returns>True if the handler exists; otherwise false.</returns>
		public bool TryGet(string path, out PathHandler handler) {
			handler = Get(path);
			return handler != null;
		}
		#endregion
		
		#region Handlers
		// Wraps the asynchronous stuff so we that we have a tidy set of arguments in the actual handler
		static void HandleContext(IAsyncResult res) {
			var server = (PathedServer)res.AsyncState;
			var ctx = server.Listener.EndGetContext(res);
			server.Listener.BeginGetContext(new AsyncCallback(HandleContext), server);
			HandleRequest(ctx, server);
		}
		
		static void HandleRequest(HttpListenerContext ctx, PathedServer server) {
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
		#endregion
	}
}
