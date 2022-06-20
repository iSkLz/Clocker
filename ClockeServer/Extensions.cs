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
		#region MIME types
		/// <summary>
		/// Holds MIME types indexed by their respective file extensions.
		/// </summary>
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
		#endregion
		
		#region HTTP context helpers
		/// <summary>
		/// Retrieves the query string with the given name.
		/// </summary>
		/// <param name="ctx">The request context to use.</param>
		/// <param name="name">Name of the query string.</param>
		/// <returns>The query value string if it exists; otherwise null.</returns>
		public static string Query(this HttpListenerContext ctx, string name) {
			var val = ctx.Request.QueryString.GetValues(name);
			if (val == null || val.Length == 0) return null;
			return val[0];
		}
		
		/// <summary>
		/// Reads the request body as a string.
		/// </summary>
		/// <param name="ctx">The request context to use.</param>
		/// <returns>The resultant string.</returns>
		public static string ReadAsStr(this HttpListenerContext ctx) {
			var input = ctx.Request.InputStream;
			return new StreamReader(input, ctx.Request.ContentEncoding).ReadToEnd();
		}
		
		/// <summary>
		/// Serves a string of text in UTF-8 encoding and closes the connection.
		/// </summary>
		/// <param name="ctx">The request context to use.</param>
		/// <param name="txt">The text to serve.</param>
		/// <param name="mime">The MIME type to use (defaults to plain text) or a dot prefixed file extension.</param>
		public static void ServeText(this HttpListenerContext ctx, string txt, string mime = "text/plain") {
			ctx.Response.ContentEncoding = Encoding.UTF8;
			ctx.ServeBytes(Encoding.UTF8.GetBytes(txt), mime);
		}
		
		// 1MB in bytes
		const int MEGABYTE = 1024 * 1024;
		
		/// <summary>
		/// Serves a memory file and closes the connection.
		/// </summary>
		/// <param name="ctx">The request context to use.</param>
		/// <param name="file">The file to serve.</param>
		public static void ServeFile(this HttpListenerContext ctx, MemoryFile file) {
			ctx.ServeBytes(file.Content.Bytes, file.MimeType);
		}
		
		/// <summary>
		/// Serves the given binary buffer and closes the connection.
		/// </summary>
		/// <param name="ctx">The request context to use.</param>
		/// <param name="buffer">The bytes to serve.</param>
		/// <param name="mime">The MIME type to use (defaults to plain text) or a dot prefixed file extension.</param>
		public static void ServeBytes(this HttpListenerContext ctx, byte[] buffer, string mime = "text/plain")
		{
			// File extension, resolve
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
				
				// Buffers larger than a megabyte for some reason seem to break often with a "connection lost/network not available" error
				// (Note: This error is actually thrown by the OS indicating that the server connection was actually cut by some mechanism)
				// However, browsers also seem to (somehow) correctly continue the download if we just return a null stream
				// in the case of said error (even though this implementation doesn't account for partial requests)
				// So, in the case of an error happening when a file is above a megabyte, we don't do anything ¯\_(ツ)_/¯
				// TODO: Investigate into this
			}
			
			ctx.Response.Close();
		}
		
		// TODO: Implement a proper error page for any error (not just 404)
		/// <summary>
		/// Ratios the user, throwing a 404 error in their face.
		/// </summary>
		/// <param name="ctx">The request context to use.</param>
		/// <param name="sign">The note to provide the user with.</param>
		public static void Ratio(this HttpListenerContext ctx, string sign = null) {
			ctx.Response.StatusCode = 404;
			ctx.ServeText("404'd, ratio.\n" + (sign ?? ""));
		}
		#endregion
		
		#region PathedServer utils
		/// <summary>
		/// Sets one hour to all server time-outs and removes the minimum rate floor.
		/// </summary>
		/// <param name="server">The server to configure.</param>
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
		
		/// <summary>
		/// Picks a random port number between 100 and 65535 (both inclusive).
		/// </summary>
		/// <param name="rng"></param>
		/// <returns></returns>
		public static uint RandomPort(this Random rng) {
			return (uint)rng.Next(100, 65536);
		}
		
		/// <summary>
		/// Returns the mime type of the given extension.
		/// </summary>
		/// <param name="ext">The file extension to use (can be dot prefixed).</param>
		/// <returns></returns>
		public static string MimeOf(this string ext) {
			if (ext.StartsWith(".")) ext = ext.Substring(1);
			return MimeRef.ContainsKey(ext) ? MimeRef[ext] :
				MimeRef.ContainsKey(ext.Substring(1)) ? MimeRef[ext.Substring(1)] : "application/octet-stream";
		}
		
		/// <summary>
		/// Returns the directiory's path in web style (lower case/forward slashes).
		/// </summary>
		/// <param name="dir">The directory whose path to get.</param>
		/// <returns>The resultant path.</returns>
		public static string WebName(this DirectoryInfo dir) {
			return dir.FullName.Replace('\\', '/').ToLowerInvariant();
		}
		
		/// <summary>
		/// Returns the file's path in web style (lower case/forward slashes).
		/// </summary>
		/// <param name="file">The file whose path to get.</param>
		/// <returns>The resultant path.</returns>
		public static string WebName(this FileInfo file) {
			return file.FullName.Replace('\\', '/').ToLowerInvariant();
		}
		
		/// <summary>
		/// Returns the file's extension in web style (lower case/not prefixed).
		/// </summary>
		/// <param name="file">The file whose extension to get.</param>
		/// <returns>The resultant extension.</returns>
		public static string WebExt(this FileInfo file) {
			return file.Extension.ToLowerInvariant().Substring(1);
		}
		
		/// <summary>
		/// Normalizes the line breaks to the Unix style (CR or \n).
		/// </summary>
		/// <param name="str">The string to normalize.</param>
		/// <returns>The resultant string.</returns>
		public static string NormalizeLines(this string str) {
			return str.Replace("\r", "");
		}
		#endregion
		
		#region Other utils
		/// <summary>
		/// Joins a set of strings with a given delimeter.
		/// </summary>
		/// <param name="arr">The strings to join.</param>
		/// <param name="joiner">The delimeter to separate the strings with.</param>
		/// <returns>A string composed of the given strings separated by the given delimeter.</returns>
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
		
		/// <summary>
		/// Enumerates the given enumerable and calls the given action with each element.
		/// </summary>
		/// <param name="enu">The enumerable to go through.</param>
		/// <param name="act">The action to call with elements.</param>
		public static void Each<T>(this IEnumerable<T> enu, Action<T> act) {
			foreach (var x in enu) act(x);
		}
		#endregion
	}
}
