using System;
using System.Collections.Generic;

namespace Clocker.Mod
{
	public class Mods
	{
		public static Mods Instance;
		
		public static void Init() {
			Instance = new Mods();
		}
		
		public List<ClockerMod> List;
		
		public Mods()
		{
			List = new List<ClockerMod>();
		}
		
		public ClockerMod Add<T>() where T : ClockerMod, new() {
			var inst = new T();
			Add(inst);
			return inst;
		}
		
		public void Add<T>(T mod) where T : ClockerMod {
			List.Add(mod);
		}
		
		public void Do(Action<ClockerMod> each) {
			foreach (var mod in List) {
				each(mod);
			}
		}
	}
}
