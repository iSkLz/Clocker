using System;
using System.Net;
using System.Collections.Generic;
using Celeste;
using Clocker.Server;

namespace Clocker.Mod
{
	public partial class Server
	{
		public InfoModule Info;
		
		public void InitInfo() {
			Http.Add<InfoModule>(out info);
		}
		
		public void UnloadInfo() {
			Info = null;
		}
		
		[Scannable("/info/")]
		public class InfoModule {
			#region Mod map info
			static Dictionary<int, ChapterInfo> AreaInfo = new Dictionary<int, ChapterInfo>();
			
			public static void RegisterInfo(ChapterInfo info, int id) {
				info.ASide.Each((x) => x.FillIn(new AreaKey(id, AreaMode.Normal)));
				if (info.BSide != null) info.BSide.Each((x) => x.FillIn(new AreaKey(id, AreaMode.BSide)));
				if (info.CSide != null) info.CSide.Each((x) => x.FillIn(new AreaKey(id, AreaMode.CSide)));
				AreaInfo.Add(id, info);
			}
			
			public static bool HasInfo(int id) {
				return AreaInfo.ContainsKey(id);
			}
			
			public static bool HasInfo(int id, AreaMode mode) {
				return HasInfo(id) && (
					mode == AreaMode.Normal ||
					mode == AreaMode.BSide && AreaInfo[id].BSide != null ||
					mode == AreaMode.CSide && AreaInfo[id].CSide != null
				);
			}
			
			public static CheckpointInfo GetCheckpointInfo(int id, AreaMode mode, int cp) {
				return (mode == AreaMode.Normal) ? (AreaInfo[id].ASide[cp]) : (
					(mode == AreaMode.BSide) ? (AreaInfo[id].BSide[cp]) : (AreaInfo[id].CSide[cp])
				);
			}
			#endregion
			
			#region Maps
			[ScanRoute("maps.json")]
			public void HandleMaps(HttpListenerContext ctx) {
				var res = new Dictionary<string, object>[AreaData.Areas.Count];
				for (int id = 0; id < AreaData.Areas.Count; id++)
					res[id] = SerializeChapter(id);
				ctx.ServeText(res.ToJson(), ".json");
			}
			
			Dictionary<string, object> SerializeChapter(int id) {
				// TODO: Add mod support to add data for each part
				var chapter = AreaData.Get(id);
				var berrytotal = 0;
				
				var res = new Dictionary<string, object>();
				res.Add("name", id.GetName());
				res.Add("set", id.GetSet());
				res.Add("icon", chapter.Icon);
				
				var modea = chapter.Mode[0];
				var aside = SerializeArea(id, AreaMode.Normal, modea);
				berrytotal += modea.TotalStrawberries;
				res.Add("aside", aside);
				
				if (chapter.HasMode(AreaMode.BSide)) {
					var modeb = chapter.Mode[1];
					var bside = SerializeArea(id, AreaMode.BSide, modeb);
					berrytotal += modeb.TotalStrawberries;
					res.Add("bside", bside);
				}
				
				if (chapter.HasMode(AreaMode.CSide)) {
					var modec = chapter.Mode[2];
					var cside = SerializeArea(id, AreaMode.CSide, modec);
					berrytotal += modec.TotalStrawberries;
					res.Add("cside", cside);
				}
				
				res.Add("berries", berrytotal);
				
				return res;
			}
			
			Dictionary<string, object> SerializeArea(int id, AreaMode _mode, ModeProperties mode) {
				var res = new Dictionary<string, object>();
				res.Add("berries", mode.TotalStrawberries);
				res.Add("poem", mode.PoemID.DialogPoemEn());
				res.Add("checkpoints", SerializeAreaCheckpoints(id, _mode, mode));
				res.Add("rooms", SerializeAreaRooms(id, _mode, mode));
				return res;
			}
			
			Dictionary<string, object>[] SerializeAreaRooms(int id, AreaMode _mode, ModeProperties mode) {
				var rooms = new Dictionary<string, object>[mode.MapData.Levels.Count];
				
				int i = 0;
				foreach (var room in mode.MapData.Levels) {
					var res = new Dictionary<string, object>();
					res.Add("name", room.Name);
					res.Add("berries", room.Strawberries);
					res.Add("heart", room.HasHeartGem);
					res.Add("tape", room.Entities.Exists((ent) => ent.Name == "cassette"));
					rooms[i] = res;
					i++;
				}
				
				return rooms;
			}
			
			Dictionary<string, object>[] SerializeAreaCheckpoints(int id, AreaMode _mode, ModeProperties mode) {
				var flag = HasInfo(id, _mode);
				
				if (mode.Checkpoints == null) return new Dictionary<string, object>[0];
				var cps = new Dictionary<string, object>[mode.Checkpoints.Length + 1];
				
				var start = new Dictionary<string, object>();
				start.Add("name", "Start");
				start.Add("berries", mode.StartStrawberries);
				if (flag) SerializeCheckpointInfo(start, GetCheckpointInfo(id, _mode, 0));
				
				cps[0] = start;
				
				int i = 1;
				foreach (var cp in mode.Checkpoints) {
					var res = new Dictionary<string, object>();
					res.Add("name", cp.Name.DialogEn());
					res.Add("berries", cp.Strawberries);
					if (flag) SerializeCheckpointInfo(res, GetCheckpointInfo(id, _mode, i));
					cps[i] = res;
					i++;
				}
				
				return cps;
			}
			
			void SerializeCheckpointInfo(Dictionary<string, object> output, CheckpointInfo info) {
				output.Add("heart", info.HasHeart);
				output.Add("tape", info.HasTape);
				output.Add("rooms", info.Rooms);
			}
			#endregion
		}
	}
}
