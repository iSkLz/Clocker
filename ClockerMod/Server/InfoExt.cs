using System;
using System.Collections.Generic;

using Celeste;

namespace Clocker.Mod
{
	public static partial class Info
	{
		public static string GetSide(this AreaMode mode) {
			return (mode == AreaMode.Normal) ? "A-Side" : ((mode == AreaMode.BSide) ? "B-Side" : "C-Side");
		}
		
		public static string GetName(this int ID) {
			return (AreaData.Get(ID).Name).DialogLevelEn();
		}
		
		public static string GetSet(this int ID) {
			return (AreaData.Get(ID).LevelSet).DialogLevelEn();
		}
		
		public static string GetName(this CheckpointData cp) {
			return cp.Name.DialogEn();
		}
	}
}
