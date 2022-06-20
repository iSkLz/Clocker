using System;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace Clocker.Server
{
	public static class Extensions
	{
		public static Dictionary<string, string> MimeRef = new Dictionary<string, string>() {
			{ "bin", "application/octet-stream" },
			
			{ "css", "text/css" },
			{ "html", "text/html" },
			{ "htm", "text/html" },
			{ "js", "text/javascript" },
			{ "mjs", "text/javascript" },
			
			{ "json", "application/json" },
			{ "rtf", "application/rtf" },
			{ "txt", "text/plain" },
			{ "xml", "application/xml" },
			
			{ "otf", "font/otf" },
			{ "ttf", "font/ttf" },
			{ "woff", "font/woff" },
			{ "woff2", "font/woff2" },
			
			{ "mp3", "audio/mpeg" },
			{ "oga", "audio/ogg" },
			{ "ogg", "audio/ogg" },
			{ "weba", "audio/webm" },
			{ "wav", "audio/wav" },
			
			{ "mp4", "video/mp4" },
			{ "mpeg", "video/mpeg" },
			{ "ogv", "video/ogg" },
			{ "webm", "video/webm" },
			
			{ "bmp", "image/bmp" },
			{ "ico", "image/vnd.microsoft.icon" },
			{ "jpeg", "image/jpeg" },
			{ "jpg", "image/jpeg" },
			{ "png", "image/png" },
			{ "svg", "image/svg+xml" },
			{ "webp", "image/webp" },
			
			{ "zip", "application/zip" },
		};
		
		public static string Join(this string[] arr, string joiner) {
			if (arr.Length == 0) return "";
			var build = new StringBuilder();
			build.Append(arr[0]);
			
			for (int i = 1; i < arr.Length; i++) {
				build.Append(joiner);
				build.Append(arr[i]);
			}
			
			return build.ToString();
		}
		
		public static string Query(this HttpListenerContext ctx, string name) {
			var val = ctx.Request.QueryString.GetValues(name);
			if (val == null || val.Length == 0) return null;
			return val[0];
		}
		
		public static string ReadAsStr(this HttpListenerContext ctx) {
			var input = ctx.Request.InputStream;
			return new StreamReader(input, ctx.Request.ContentEncoding).ReadToEnd();
		}
		
		public static string MimeOf(this string ext) {
			return MimeRef.ContainsKey(ext) ? MimeRef[ext] :
				MimeRef.ContainsKey(ext.Substring(1)) ? MimeRef[ext.Substring(1)] : "application/octet-stream";
		}
		
		public static void ServeText(this HttpListenerContext ctx, string txt, string mime = "text/plain") {
			ctx.Response.ContentEncoding = Encoding.UTF8;
			ctx.ServeBytes(Encoding.UTF8.GetBytes(txt), mime);
		}
		
		const int MEGABYTE = 1024 * 1024;
		
		public static void ServeBytes(this HttpListenerContext ctx, byte[] buffer, string mime = "text/plain")
		{
			if (mime.StartsWith(".")) mime = mime.MimeOf();
			var len = buffer.Length;
			
			ctx.Response.ContentLength64 = (long)len;
			ctx.Response.ContentType = mime;
			
			ctx.Response.KeepAlive = true;
			try {
				ctx.Response.OutputStream.Write(buffer, 0, len);
			} catch {
				if (buffer.Length < MEGABYTE)
					// TODO: Verify 504 is a server side error
					ctx.Response.StatusCode = 504;
				
				// ¯\_(ツ)_/¯
				// Buffers larger than a megabyte for some reason seem to break very often with a "connection lost" error
				// However, browsers also seem to correctly continue the download if we just return a null stream
				// in the case of said error (even though this implementation doesn't account for partial requests)
				// TODO: Investigate into this
			}
			
			ctx.Response.Close();
		}
		
		public static uint RandomPort(this Random rng) {
			return (uint)rng.Next(100, 65536);
		}
		
		public static void SetHighTimeout(this HttpListener server) {
			var mgr = server.TimeoutManager;
			var lim = TimeSpan.FromHours(1);
			
			// suuuuuuuuuuuurely one hour is enough
			mgr.DrainEntityBody = lim;
			mgr.EntityBody = lim;
			mgr.HeaderWait = lim;
			mgr.IdleConnection = lim;
			mgr.RequestQueue = lim;
			
			mgr.MinSendBytesPerSecond = 0;
		}
		
		public static void Receive<TP>(this HttpListener listener, Action<HttpListenerContext, TP> handler, TP state) {
			AsyncCallback callback = new AsyncCallback((_) => {});
			
			callback = new AsyncCallback(
				(IAsyncResult res) => {
					var substate = (Tuple<HttpListener, TP>)res.AsyncState;
					var ctx = substate.Item1.EndGetContext(res);
					substate.Item1.BeginGetContext(callback, res.AsyncState);
					handler(ctx, substate.Item2);
				}
			);
			
			listener.BeginGetContext(
				callback,
				new Tuple<HttpListener, TP>(listener, state)
			);
		}
		
		public static void Ratio(this HttpListenerContext ctx, string sign = null) {
			ctx.Response.StatusCode = 404;
			ctx.ServeText("404'd, ratio.\n" + (sign ?? ""));
		}
		
		public static string WebName(this DirectoryInfo dir) {
			return dir.FullName.Replace('\\', '/').ToLowerInvariant();
		}
		
		public static string WebName(this FileInfo file) {
			return file.FullName.Replace('\\', '/').ToLowerInvariant();
		}
		
		public static string WebExt(this FileInfo file) {
			return file.Extension.ToLowerInvariant().Substring(1);
		}
		
		public static string NormalizeLines(this string str) {
			return str.Replace("\r", "");
		}
		
		public static void Each<T>(this IEnumerable<T> enu, Action<T> act) {
			foreach (var x in enu) act(x);
		}
	}
}
