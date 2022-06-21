using System;
using System.Linq;
using Celeste;
using Monocle;

namespace Clocker.Mod
{
	public struct Area {
		public static Area GetCurrent() {
			var level = new Area();
			var key = ((Level)Engine.Scene).Session.Area;
			level.ID = key.ID;
			level.SID = key.SID;
			level.Mode = key.Mode;
			return level;
		}
		
		public string SID;
		public int ID;
		public AreaMode Mode;
		
		public AreaKey Key {
			get {
				return new AreaKey(ID, Mode);
			}
		}
		
		public AreaData GetChap() {
			return AreaData.Get(SID);
		}
		
		public MapData GetData() {
			return GetChap().Mode[(int)Mode].MapData;
		}
	}
	
	public struct Room {
		public static Room GetCurrent() {
			var level = Area.GetCurrent();
			var room = new Room();
			var session = ((Level)Engine.Scene).Session;
			room.Name = session.Level;
			room.Area = level;
			room.Checkpoint = session.LevelData.HasCheckpoint;
			return room;
		}
		
		public Area Area;
		public string Name;
		public bool Checkpoint;
		
		public LevelData GetData() {
			var name = Name;
			return Area.GetData().Levels.Find((room) => room.Name == name);
		}
		
		public bool SameAs(Room other) {
			return other.Area.SID == Area.SID && Name == other.Name;
		}
	}
	
	public struct Cpoint {
		public static Cpoint Get(Room room) {
			var cps = Area.GetCurrent().GetData().ModeData.Checkpoints;
			
			int i = 0;
			if (cps != null) {
				for (i = 0; i < cps.Length; i++) {
					if (cps[i].Level == room.Name) break;
				}
			}
			
			var cpoint = new Cpoint();
			cpoint.Area = Area.GetCurrent();
			cpoint.Order = 0;
			
			if (i == cps.Length) {
				return cpoint;
			}
			cpoint.Order = i + 1;
			
			return cpoint;
		}
		
		public Area Area;
		public int Order;
		
		public bool SameAs(Cpoint other) {
			return other.Area.SID == Area.SID && other.Order == this.Order;
		}
	}
	
	public struct Berry {
		public static Berry GetBerry(Strawberry entity) {
			var berry = new Berry();
			berry.Room = Room.GetCurrent();
			
			var data = berry.Room.Area.GetData().Strawberries.Find((bdata) => bdata.ID == entity.ID.ID);
			berry.Checkpoint = data.Int("checkpointIDParented", data.Int("checkpointID"));
			berry.Order = data.Int("order");
			
			return berry;
		}
		
		public Room Room;
		public int Checkpoint;
		public int Order;
		
		public bool SameAs(Berry other) {
			return this.Room.SameAs(other.Room) && this.Checkpoint == other.Checkpoint && this.Order == other.Order;
		}
	}
	
	public struct Flag {
		public static Flag GetFlag(int num) {
			return new Flag {
				Number = num,
				Room = Room.GetCurrent()
			};
		}
		
		public Room Room;
		public int Number;
	}
	
	public struct Frame {
		public Area? Area;
		public Room? Room;
		public Cpoint? Checkpoint;
		
		public Area? LastArea;
		public Room? LastRoom;
		public Cpoint? LastCheckpoint;
		
		public Flag? Flag;
		
		public Berry? LastBerry;
		public Berry? LastTouchBerry;
		public Room? LastAHeart;
		public Room? LastBHeart;
		public Room? LastCHeart;
		public Room? LastTape;
		
		public Area? LastGolden;
		public Area? LastTouchGolden;
		public Area? LastMoon;
		public Area? LastTouchMoon;
		
		public bool Frozen;
		public bool Paused;
		public bool ILPaused;
	}
}
