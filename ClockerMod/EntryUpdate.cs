using System;
using System.Reflection;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Clocker.Server;
using Monocle;
using Microsoft.Xna.Framework;

namespace Clocker.Mod
{
	public partial class EntryComp {
		IEnumerator<bool> VanillaLoader;
		public void Init() {
			VanillaLoader = VanillaLoadRoutine();
		}
		
		public static event Action OnUpdate;
		
		public override void Update(GameTime gameTime)
		{
			if (VanillaLoader != null) {
				if (!VanillaLoader.Current) VanillaLoader.MoveNext();
				else {
					Entry.Instance.FullyLoaded();
					VanillaLoader = null;
				}
			}
			
			if (OnUpdate != null) OnUpdate();
		}
		
		FieldInfo VanillaLoaded = typeof(GameLoader).GetField("loaded", BindingFlags.Instance | BindingFlags.NonPublic);
		public IEnumerator<bool> VanillaLoadRoutine() {
			while (!(Engine.Scene is GameLoader))
				yield return false;
			
			var loader = (GameLoader)Engine.Scene;
			Thread thrd = null;
			
			while (thrd == null || thrd.Name != "GAME_LOADER") {
				yield return false;
				RunThread.Current.TryGetTarget(out thrd);
			}
			
			while (!((bool)VanillaLoaded.GetValue(loader)))
				yield return false;
			
			yield return true;
		}
	}
}
