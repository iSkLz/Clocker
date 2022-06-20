using System;
using System.Collections.Generic;

using Clocker.Server;

namespace Clocker.Mod
{
	public class ClockerMod {
		public ClockerMod() {
		}
		
		/// <summary>
		/// Called after the server was initialized.
		/// </summary>
		public virtual void InitServer(Server server) {}
		
		/// <summary>
		/// Called after the memory was initialized.
		/// </summary>
		public virtual void InitMemory(Memory memory) {}
		
		/// <summary>
		/// Called after all mods were registered but before any services are initialized.
		/// </summary>
		public virtual void Init() {}
		
		/// <summary>
		/// Called after all services were initialized.
		/// </summary>
		public virtual void InitLate() {}
		
		/// <summary>
		/// Called before the memory updates.
		/// </summary>
		public virtual void PreUpdate(Memory memory) {}
		
		/// <summary>
		/// Called after the memory updates.
		/// </summary>
		public virtual void PostUpdate(Memory memory) {}
		
		/// <summary>
		/// Called after the game has fully loaded all its assets (Vanilla & Everest).
		/// This is the same time the game starts to transition to the titie screen.
		/// </summary>
		public virtual void GameLoaded() {}
		
		/// <summary>
		/// Called when Everest unloads Clocker, use to dispose of all system resources used in the mod.
		/// </summary>
		public virtual void Unload() {}
	}
}
