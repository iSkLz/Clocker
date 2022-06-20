using System;
using System.Reflection;
using System.Net;

namespace Clocker.Server
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ScannableAttribute : Attribute {
		public string Route { get; set; }
		
		public ScannableAttribute(string route) {
			Route = route;
		}
	}
	
	[AttributeUsage(AttributeTargets.Method)]
	public class ScanBackupAttribute : Attribute {}
	
	[AttributeUsage(AttributeTargets.Method)]
	public class ScanRouteAttribute : Attribute {
		public string Subroute { get; set; }
		
		public ScanRouteAttribute(string subroute) {
			Subroute = subroute;
		}
	}
	
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
			if (scanattr == null) {
				ph = null;
				return null;
			}
			
			var handler = server.Add(scanattr.Route);
			var inst = Activator.CreateInstance(type);
			
			foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance)) {
				var routeattr = method.GetCustomAttribute<ScanRouteAttribute>();
				var backattr = method.GetCustomAttribute<ScanBackupAttribute>();
				if (routeattr != null) {
					handler.Add(routeattr.Subroute, GetHandler(method, inst));
				} else if (backattr != null) {
					handler.SetBackup(GetHandler(method, inst));
				}
			}
			
			ph = handler;
			return inst;
		}
		
		static Action<HttpListenerContext, string> GetHandler(MethodInfo mthd, object instance) {
			if (mthd.GetParameters().Length == 1)
				return (HttpListenerContext ctx, string subpath) => {
					mthd.Invoke(instance, new object[] {ctx});
				};
			else
				return (HttpListenerContext ctx, string subpath) => {
					mthd.Invoke(instance, new object[] {ctx, subpath});
				};
		}
	}
}
