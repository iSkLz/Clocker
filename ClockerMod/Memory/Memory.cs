using System;
using Celeste;
using Celeste.Mod;
using Monocle;

namespace Clocker.Mod
{
	public partial class Memory
	{
		public static Memory Instance;
		
		public static void Init()
		{
			Instance = new Memory();
			Logger.Log("ClockerMemory", "Installing hooks...");
			Hooks.HookMemory();
			
			Logger.Log("ClockerMemory", "Initialized successfully.");
		}
		
		public Frame StableCurrent;
		public Frame StablePrevious;
		
		public Frame Current;
		public Frame Previous;
		
		public Strawberry FBerry = null;
		public Strawberry FTBerry = null;
		public HeartGem FHeart = null;
		public Cassette FTape = null;
		public int? FFlag = null;
		
		public Memory() {
			Previous = new Frame();
			Current = new Frame();
			
			Current.Flag = new Flag() { Number = -100 };
			
			StableCurrent = Current;
			StablePrevious = Previous;
		}
		
		public void Update() {
			Mods.Instance.Do((mod) => mod.PreUpdate(this));
			
			Previous = Current;
			
			var frm = new Frame();
			if (Engine.Scene is Level) {
				var lvl = (Level)Engine.Scene;
				var player = lvl.Tracker.GetEntity<Player>();
				
				frm.Area = Area.GetCurrent();
				frm.Room = Room.GetCurrent();
				if (frm.Room.Value.Checkpoint || (!Previous.Checkpoint.HasValue && lvl.Session.FirstLevel))
					frm.Checkpoint = Cpoint.Get(frm.Room.Value);
				else
					frm.Checkpoint = Previous.Checkpoint;
				
				if (FBerry != null) {
					if (!FBerry.Golden && !FBerry.Moon)
						frm.LastBerry = Berry.GetBerry(FBerry);
					else if (FBerry.Golden)
						frm.LastGolden = frm.Area;
					else if (FBerry.Moon)
						frm.LastMoon = frm.Area;
					FBerry = null;
				} else {
					frm.LastBerry = Previous.LastBerry;
					frm.LastGolden = Previous.LastGolden;
					frm.LastMoon = Previous.LastMoon;
				}
				
				if (FTBerry != null) {
					if (!FTBerry.Golden && !FTBerry.Moon)
						frm.LastTouchBerry = Berry.GetBerry(FTBerry);
					else if (FBerry.Golden)
						frm.LastTouchGolden = frm.Area;
					else if (FBerry.Moon)
						frm.LastTouchMoon = frm.Area;
					FTBerry = null;
				} else {
					frm.LastTouchBerry = Previous.LastTouchBerry;
					frm.LastTouchGolden = Previous.LastTouchGolden;
					frm.LastTouchMoon = Previous.LastTouchMoon;
				}
				
				frm.LastAHeart = Previous.LastAHeart;
				frm.LastBHeart = Previous.LastBHeart;
				frm.LastCHeart = Previous.LastCHeart;
				
				if (FHeart != null) {
					if (frm.Area.Value.Mode == AreaMode.Normal)
						frm.LastAHeart = frm.Room;
					if (frm.Area.Value.Mode == AreaMode.BSide)
						frm.LastBHeart = frm.Room;
					if (frm.Area.Value.Mode == AreaMode.CSide)
						frm.LastCHeart = frm.Room;
					FHeart = null;
				} else {
				}
				
				if (FTape != null) {
					frm.LastTape = frm.Room;
					FTape = null;
				} else frm.LastTape = Previous.LastTape;
				
				if (FFlag.HasValue) {
					frm.Flag = Flag.GetFlag(FFlag.Value);
					FFlag = null;
				} else frm.Flag = Previous.Flag;
				
				frm.Frozen = lvl.Frozen;
				frm.Paused = lvl.Paused;
				frm.ILPaused = (player == null) ? false : player.TimePaused;
				
				frm.LastArea = frm.Area;
				frm.LastCheckpoint = frm.Checkpoint;
				frm.LastRoom = frm.Room;
			} else {
				frm.LastArea = Previous.LastArea;
				frm.LastCheckpoint = Previous.LastCheckpoint;
				frm.LastRoom = Previous.LastRoom;
				
				frm.LastBerry = Previous.LastBerry;
				frm.LastGolden = Previous.LastGolden;
				frm.LastMoon = Previous.LastMoon;
				
				frm.LastTouchBerry = Previous.LastTouchBerry;
				frm.LastTouchGolden = Previous.LastTouchGolden;
				frm.LastTouchMoon = Previous.LastTouchMoon;
				
				frm.LastAHeart = Previous.LastAHeart;
				frm.LastBHeart = Previous.LastBHeart;
				frm.LastCHeart = Previous.LastCHeart;
				frm.LastTape = Previous.LastTape;
				
				frm.Flag = Previous.Flag;
			}
			
			Current = frm;
			
			StableCurrent = Current;
			StablePrevious = Previous;
			
			Mods.Instance.Do((mod) => mod.PostUpdate(this));
		}
		
		#region I didn't realize these were useless
		public bool LevelChanged() {
			return StableCurrent.Area.HasValue &&
				(StablePrevious.Area.HasValue
					? (StablePrevious.Area.Value.SID != StableCurrent.Area.Value.SID)
					: true
				);
		}
		
		public bool LevelEntered(string SID) {
			return StableCurrent.Area.HasValue && StableCurrent.Area.Value.SID == SID &&
				(StablePrevious.Area.HasValue
					? (StablePrevious.Area.Value.SID != SID)
					: true
				);
		}
		
		public bool LevelExitted(string SID) {
			return StablePrevious.Area.HasValue && StablePrevious.Area.Value.SID == SID
				&& StableCurrent.Area.Value.SID != SID;
		}
		
		public bool CheckpointChanged() {
			return StableCurrent.Checkpoint.HasValue &&
				(StablePrevious.Checkpoint.HasValue
					? (StablePrevious.Checkpoint.Value.SameAs(StableCurrent.Checkpoint.Value))
					: true
				);
		}
		
		public bool CheckpointEntered(Cpoint cpoint) {
			return StableCurrent.Checkpoint.HasValue && StableCurrent.Checkpoint.Value.SameAs(cpoint) &&
				(StablePrevious.Checkpoint.HasValue
					? !StablePrevious.Checkpoint.Value.SameAs(cpoint)
					: true
				);
		}
		
		public bool CheckpointExitted(Cpoint cpoint) {
			return StablePrevious.Checkpoint.HasValue && StablePrevious.Checkpoint.Value.SameAs(cpoint)
				&& !StableCurrent.Checkpoint.Value.SameAs(cpoint);
		}
		
		public bool RoomChanged() {
			return StableCurrent.Room.HasValue &&
				(StablePrevious.Room.HasValue
					? (StablePrevious.Room.Value.SameAs(StableCurrent.Room.Value))
					: true
				);
		}
		
		public bool RoomEntered(Room room) {
			return StableCurrent.Room.HasValue && StableCurrent.Room.Value.SameAs(room) &&
				(StablePrevious.Room.HasValue
					? !StablePrevious.Room.Value.SameAs(room)
					: true
				);
		}
		
		public bool RoomExitted(Room room) {
			return StablePrevious.Room.HasValue && StablePrevious.Room.Value.SameAs(room)
				&& !StableCurrent.Room.Value.SameAs(room);
		}
		
		public bool BerryCollected(Berry berry) {
			return StableCurrent.LastBerry.HasValue && StableCurrent.LastBerry.Value.SameAs(berry)
				&& (StablePrevious.LastBerry.HasValue
				    ? !StablePrevious.LastBerry.Value.SameAs(berry)
					: true
				);
		}
		
		public bool BerryTouched(Berry berry) {
			return StableCurrent.LastTouchBerry.HasValue && StableCurrent.LastTouchBerry.Value.SameAs(berry)
				&& (StablePrevious.LastTouchBerry.HasValue
				    ? !StablePrevious.LastTouchBerry.Value.SameAs(berry)
					: true
				);
		}
		
		public bool GoldenCollected(Area area) {
			return StableCurrent.LastGolden.HasValue && StableCurrent.LastGolden.Value.SID == area.SID
				&& (StablePrevious.LastGolden.HasValue
				    ? StablePrevious.LastGolden.Value.SID != area.SID
					: true
				);
		}
		
		public bool GoldenTouched(Area area) {
			return StableCurrent.LastTouchGolden.HasValue && StableCurrent.LastTouchGolden.Value.SID == area.SID
				&& (StablePrevious.LastTouchGolden.HasValue
				    ? StablePrevious.LastTouchGolden.Value.SID != area.SID
					: true
				);
		}
		
		public bool MoonCollected(Area area) {
			return StableCurrent.LastMoon.HasValue && StableCurrent.LastMoon.Value.SID == area.SID
				&& (StablePrevious.LastMoon.HasValue
				    ? StablePrevious.LastMoon.Value.SID != area.SID
					: true
				);
		}
		
		public bool MoonTouched(Area area) {
			return StableCurrent.LastTouchMoon.HasValue && StableCurrent.LastTouchMoon.Value.SID == area.SID
				&& (StablePrevious.LastTouchMoon.HasValue
				    ? StablePrevious.LastTouchMoon.Value.SID != area.SID
					: true
				);
		}
		#endregion
	}
}
