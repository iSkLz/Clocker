using System;
using System.Threading;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Clocker.Server;
using Monocle;
using Microsoft.Xna.Framework;

// The greal wall of general TODOs

// TODO: Implement a way to provide map info without coding a clocker mod
// Probably some ModAsset shenanigans
// Edit: There's a convenient place to put that now at Content.cs

// TODO: Implement vanilla map info

namespace Clocker.Mod
{
	public class Entry : EverestModule
	{
		public static Entry Instance;
		
		public Entry() {
			Instance = this;
		}
		
		public override void Load()
		{
			Logger.Log("Clocker", "Installing initial hooks...");
			Hooks.HookEntry();
			
			Logger.Log("Clocker", "Initializing the mods registry...");
			Mods.Init();
		}
		
		public override void Initialize()
		{
			Logger.Log("Clocker", "Adding the default mod...");
			Mods.Instance.Add<DefaultMod>();
			Logger.Log("Clocker", "Adding the game component...");
			Engine.Instance.Components.Add(new EntryComp(Engine.Instance));
		}
		
		public void PostInitialize()
		{
			Logger.Log("Clocker", "Initializing mods...");
			Mods.Instance.Do((mod) => mod.Init());
		}
		
		public void FullyLoaded() {
			Logger.Log("Clocker", "Initializing Clocker services...");
			Lang.Init();
			Memory.Init();
			Server.Init();
			
			Logger.Log("Clocker", "All services initialized. Notifying mods...");
			Mods.Instance.Do((mod) => { mod.GameLoaded(); mod.InitMemory(Memory.Instance); mod.InitServer(Server.Instance); });
			Mods.Instance.Do((mod) => mod.InitLate());
		}
		
		public override void Unload()
		{
			Hooks.UnhookAll();
		}
	}
	
	public partial class EntryComp : GameComponent {
		public static EntryComp Instance;
		public EntryComp(Game game) : base(game) {
			Instance = this;
			Init();
		}
	}
}