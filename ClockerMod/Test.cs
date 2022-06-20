using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Net;
using System.Text;
using Clocker.Server;
using Celeste;
using Celeste.Mod;

namespace Clocker.Mod
{
	public static class Test
	{
		public static void TestAreas() {
			Console.WriteLine("NEVER GONNA GIVEYOU UP");
			foreach (var area in AreaData.Areas) {
				Console.WriteLine(area.SID);
				Console.WriteLine(area.ID);
				Console.WriteLine(area.Name);
				Console.WriteLine();
			}
		}
	}
}
