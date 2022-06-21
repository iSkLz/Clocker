using System;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Clocker.Server;
using Ionic.Zip;

namespace Clocker.Mod
{
	public class ClockerContent
	{
		internal void Crawl()
		{
			var enu = new DirectoryInfo(Everest.Loader.PathMods).EnumerateFiles("*", SearchOption.TopDirectoryOnly);
			var zips = new List<ZipFile>();
			
			foreach (var fileinfo in enu) {
				if (fileinfo.WebExt() == "zip") {
					var zip = new ZipFile(fileinfo.FullName);
					var res = new ZipResolver(zip);
					Process(res, from entry in zip.Entries where !entry.IsDirectory select entry.FileName);
				}
			}
			
			var iores = new SystemResolver(Everest.Loader.PathMods);
			enu = new DirectoryInfo(Everest.Loader.PathMods).EnumerateFiles("*", SearchOption.AllDirectories);
			Process(iores, from file in enu where file.WebExt() != "zip" select file.FullName);
		}
		
		void Process(IFileResolver resolver, IEnumerable<string> files) {
			foreach (var file in files) {
				var ext = file.WebExt();
				if (ext == "clocker") {
					var name = file.WebName();
					switch (name) {
						case "web":
							var dir = file.WebDir();
							var route = resolver.Resolve(file).Content.ToString();
							Server.Websites.Add(route, new FileCache(resolver.SubResolver(dir)));
							break;
					}
				}
			}
		}
	}
}
