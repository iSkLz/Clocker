using System;
using Celeste;
using Celeste.Mod;
using Monocle;

namespace Clocker.Mod
{
	/// <summary>
	/// Represents a state memory of different vanilla stats.
	/// </summary>
	public partial class Memory
	{
		/// <summary>
		/// The singleton instance of the memory.
		/// </summary>
		public static Memory Instance;
		
		internal static void Init()
		{
			Instance = new Memory();
			Logger.Log("ClockerMemory", "Installing hooks...");
			Hooks.HookMemory();
			
			Logger.Log("ClockerMemory", "Initialized successfully.");
		}
		
		/// <summary>
		/// A copy of the current frame's data, safe to be used across threads.
		/// </summary>
		public Frame StableCurrent;
		
		/// <summary>
		/// A copy of the previous frame's data, safe to be used across threads.
		/// </summary>
		public Frame StablePrevious;
		
		/// <summary>
		/// The current frame's data.
		/// </summary>
		public Frame Current;
		
		/// <summary>
		/// The previous frame's data.
		/// </summary>
		public Frame Previous;
		
		/// <summary>
		/// Holds a reference to the berry that has been collected in the current frame (if it exists).
		/// </summary>
		public Strawberry FBerry = null;
		
		/// <summary>
		/// Holds a reference to the berry that has been picked up in the current frame (if it exists).
		/// </summary>
		public Strawberry FTBerry = null;
		
		/// <summary>
		/// Holds a reference to the crystal heart that has been collected in the current frame (if it exists).
		/// </summary>
		public HeartGem FHeart = null;
		
		/// <summary>
		/// Holds a reference to the cassette tape has been collected in the current frame (if it exists).
		/// </summary>
		public Cassette FTape = null;
		
		/// <summary>
		/// Holds a reference to the summit flag that has been activated in the current frame (if it exists).
		/// </summary>
		public int? FFlag = null;
		
		internal Memory() {
			Previous = new Frame();
			Current = new Frame();
			
			// We need to compare flags to know when the flag changes so we create an impossible flag initially
			Current.Flag = new Flag() { Number = -100 };
			
			StableCurrent = Current;
			StablePrevious = Previous;
		}
		
		// This is public in case a mod needs to do some shenanigans.
		/// <summary>
		/// Updates the memory's state.
		/// </summary>
		public void Update() {
			Mods.Instance.Do((mod) => mod.PreUpdate(this));
			
			Previous = Current;
			
			var frm = new Frame();
			// We're in a level, track in-game data
			if (Engine.Scene is Level) {
				var lvl = (Level)Engine.Scene;
				var player = lvl.Tracker.GetEntity<Player>();
				
				frm.Area = Area.GetCurrent();
				frm.Room = Room.GetCurrent();
				
				// The room has to have a checkpoint entity or it can be the very first in the level
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
				
				// Copy from last frame and whichever heart has been picked up will be overridden correctly
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
				
				// We're in-game, track last variables
				frm.LastArea = frm.Area;
				frm.LastCheckpoint = frm.Checkpoint;
				frm.LastRoom = frm.Room;
			} else {
				// We're not in-game, keep last variables
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
			
			// Stable frames are only updated after all the data crunching is done
			// This way we don't get mid update access issues
			StableCurrent = Current;
			StablePrevious = Previous;
			
			Mods.Instance.Do((mod) => mod.PostUpdate(this));
		}
	}
}
