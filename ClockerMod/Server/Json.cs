using System;
using System.Text;
using Newtonsoft.Json;

namespace Clocker.Mod
{
	public static class Json
	{
		public static string ToJson<T>(this T obj) {
			return JsonConvert.SerializeObject(obj, Formatting.Indented);
		}
	}
}
