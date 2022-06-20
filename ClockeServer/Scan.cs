using System;
using System.Reflection;
using System.Net;

namespace Clocker.Server
{
	#region Attributes
	[AttributeUsage(AttributeTargets.Class)]
	/// <summary>
	/// Labels the class as scannable.
	/// </summary>
	public class ScannableAttribute : Attribute {
		public string Route { get; set; }
		
		public ScannableAttribute(string route) {
			Route = route;
		}
	}
	
	[AttributeUsage(AttributeTargets.Method)]
	/// <summary>
	/// Labels the method as a fallback handler.
	/// </summary>
	public class ScanBackupAttribute : Attribute {}
	
	[AttributeUsage(AttributeTargets.Method)]
	/// <summary>
	/// Labels the method as the handler for the specified subpath.
	/// </summary>
	public class ScanRouteAttribute : Attribute {
		public string Subpath { get; set; }
		
		public ScanRouteAttribute(string subpath) {
			Subpath = subpath;
		}
	}
	#endregion
	
	internal static class Scanner
	{
		internal static bool Scan<T>(this PathedServer server, out PathHandler handler, out T inst) where T : class, new() {
			var res = Scan(typeof(T), server, out handler) as T;
			if (res == null) {
				inst = null;
				return false;
			} else {
				inst = res;
				return true;
			}
		}
		
		static object Scan(Type type, PathedServer server, out PathHandler ph) {
			var scanattr = type.GetCustomAttribute<ScannableAttribute>();
			if (scanattr == null) { // Unscannable class, peace out
				ph = null;
				return null;
			}
			
			// Get a handler from the server and an instance of the scannable class
			var handler = server.Add(scanattr.Route);
			var inst = Activator.CreateInstance(type);
			
			// Check each public instance method
			foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance)) {
				var routeattr = method.GetCustomAttribute<ScanRouteAttribute>();
				var backattr = method.GetCustomAttribute<ScanBackupAttribute>();
				// Check the route and backup attributes and set the method's role accordingly
				if (routeattr != null) {
					handler.Add(routeattr.Subpath, GetHandler(method, inst));
				} else if (backattr != null) {
					handler.SetBackup(GetHandler(method, inst));
				}
			}
			
			ph = handler;
			return inst;
		}
		
		// Creates a delegate that wraps the methodinfo we want to call
		static Action<HttpListenerContext, string> GetHandler(MethodInfo mthd, object instance) {
			if (mthd.GetParameters().Length == 1) // The method only has the context argument
				return (HttpListenerContext ctx, string subpath) => {
					mthd.Invoke(instance, new object[] {ctx}); // So we call it with only the context we get
				};
			else
				return (HttpListenerContext ctx, string subpath) => {
					mthd.Invoke(instance, new object[] {ctx, subpath});
				};
		}
	}
}
