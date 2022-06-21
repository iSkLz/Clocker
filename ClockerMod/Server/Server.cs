using System;
using System.Net;
using System.Collections.Generic;

using Clocker.Server;
using Celeste.Mod;

namespace Clocker.Mod
{
	public partial class Server
	{
		public static Server Instance;
		
		public static void Init() {
			Logger.Log("ClockerServer", "Initializing the server...");
			Instance = new Server();
			
			Logger.Log("ClockerServer", "Adding state handler...");
			Instance.InitState();
			Logger.Log("ClockerServer", "Adding static handler...");
			Instance.InitStatic();
			Logger.Log("ClockerServer", "Adding graphics handler...");
			Instance.InitGraphics();
			Logger.Log("ClockerServer", "Adding info handler...");
			Instance.InitInfo();
			
			Instance.Http.Start();
			Logger.Log("ClockerServer", "The server is now online at port " + Instance.Http.Port + ".");
			Logger.Log("ClockerServer", "Initialized successfully.");
		}
		
		public static void Unload() {
			Instance.Http.Listener.Stop();
			Instance.Http.Listener.Close();
			
			// TODO: Make sure the request queue has been cleared before unloading modules
			// Any left request maaaaaaay be able to cause some null trouble here
			
			Instance.UnloadState();
			Instance.UnloadStatic();
			Instance.UnloadGraphics();
			Instance.UnloadInfo();
		}
		
		public PathedServer Http;
		
		public Server()
		{
			Http = new PathedServer();
			Http.SetPort(17490, 17491, 17492, 17493, 17494, 17495, 17496, 17497, 17498, 17499, 17500);
			Http.SetThreads(8);
		}
	}
}
