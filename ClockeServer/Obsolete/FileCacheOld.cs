using System;
using System.Collections.Generic;
using System.IO;

namespace Clocker.Server
{
	[Obsolete]
	public class FileCacheO : IDisposable
	{
		public Dictionary<string, Tuple<string, byte[]>> Cache;
		public DirectoryInfo Root;
		
		public FileCacheO(DirectoryInfo root)
		{
			Cache = new Dictionary<string, Tuple<string, byte[]>>();
			Root = root;
		}
		
		public void Dispose() {
			Cache.Clear();
		}
		
		public string GetMIME(string subpath) {
			var ext = new FileInfo(NormalizeEx(subpath)).WebExt();
			return ext.MimeOf();
		}
		
		public bool Exists(string subpath) {
			return Info(subpath).Exists;
		}
		
		public bool Has(string subpath) {
			return Cache.ContainsKey(subpath);
		}
		
		public void ForceCache(string subpath) {
			string _;
			byte[] __;
			TryGet(subpath, out _, out __);
		}
		
		public void ForceCache(string subfolder, bool recursive) {
			var info = new DirectoryInfo(subfolder);
			if (!info.Exists) return;
			
			var files = info.EnumerateFiles("*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
			foreach (var file in files) {
				ForceCache(NormalizeRoot(file.FullName));
			}
		}
		
		public byte[] Get(string subpath, out string mime) {
			byte[] result;
			if (TryGet(subpath, out mime, out result)) {
				return result;
			} else throw new FileNotFoundException("The specified file couldn't be found.", NormalizeEx(subpath));
		}
		
		public bool TryGet(string subpath, out string mime, out byte[] contents) {
			NormalizeWeb(ref subpath);
			
			if (!Has(subpath) && Exists(subpath)) {
				UnsafeCache(subpath);
			} else if (!Exists(subpath)) {
				mime = null;
				contents = new byte[0];
				return false;
			}
			
			mime = Cache[subpath].Item1;
			contents = Cache[subpath].Item2;
			return true;
		}
		
		void UnsafeCache(string subpath) {
			var info = Info(subpath);
			var stream = info.OpenRead();
			var buffer = new byte[(int)stream.Length];
			stream.Read(buffer, 0, (int)stream.Length);
			stream.Close();
			
			Cache.Add(subpath, new Tuple<string, byte[]>(GetMIME(subpath), buffer));
		}
		
		FileInfo Info(string subpath) {
			return new FileInfo(NormalizeEx(subpath));
		}
		
		string NormalizeWeb(ref string subpath) {
			return NormalizeWeb(subpath);
		}
		
		string NormalizeWeb(string subpath) {
			return subpath.ToLowerInvariant().Replace('\\', '/');
		}
		
		string NormalizeEx(ref string subpath) {
			return NormalizeEx(subpath);
		}
		
		string NormalizeEx(string subpath) {
			return Path.Combine(Root.FullName, subpath);
		}
		
		string NormalizeRoot(string fullpath) {
			return fullpath.Substring(Root.FullName.Length);
		}
	}
}
