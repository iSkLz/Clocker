using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Celeste;

namespace Clocker.Mod
{
	public struct ChapterInfo {
		public CheckpointInfo[] ASide;
		public CheckpointInfo[] BSide;
		public CheckpointInfo[] CSide;
	}
	
	public struct CheckpointInfo {
		public bool HasHeart;
		public bool HasTape;
		
		public string Poem;
		
		public string RoomsRegex;
		public List<string> Rooms;
		
		public void FillIn(AreaKey area) {
			if (Rooms == null) Rooms = new List<string>();
			
			var chap = AreaData.Get(area);
			var props = chap.Mode[(int)area.Mode];
			var map = props.MapData;
			
			if (RoomsRegex != null) {
				var regex = new Regex(RoomsRegex, RegexOptions.Compiled);
				foreach (var room in map.Levels) {
					if (regex.IsMatch(room.Name)) Rooms.Add(room.Name);
					if (Rooms.Contains(room.Name) && room.HasHeartGem) HasHeart = true;
					// Celeste already does this for us I think but the code doesn't have access to the cp data soooo
					// Laziness killed the little kitty
					if (Rooms.Contains(room.Name) && room.Entities.Exists((edata) => edata.Name == "cassette")) HasTape = true;
				}
			}
		}
	}
}
