using System;
using System.Reflection;
using System.Linq;
using System.Collections;

using Celeste;
using Celeste.Mod;

using Monocle;

using Microsoft.Xna.Framework;

using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using MonoMod.Cil;

using Mono.Cecil.Cil;

namespace Clocker.Mod
{
	public static class Hooks
	{
		public static event Action OnEngineDraw;
		public static event Action OnEngineDrawLate;
		
		public static void UnhookAll() {
			On.Celeste.Strawberry.CollectRoutine -= Strawberry_OnCollect;
			On.Celeste.Strawberry.OnPlayer -= Strawberry_OnTouch;
			On.Celeste.AutoSplitterInfo.Update -= AutoSplitterInfo_Update;
			On.Celeste.HeartGem.RegisterAsCollected -= HeartGem_Collected;
			
			// TODO: How do you unhook the flag IL .-.
			
			TapeCollectHook.Dispose();
			
			
			On.Celeste.Mod.Everest.Initialize -= Everest_InitMods;
			On.Monocle.Engine.Draw -= Engine_Draw;
		}
		
		public static void HookEntry() {
			On.Celeste.Mod.Everest.Initialize += Everest_InitMods;
			On.Monocle.Engine.Draw += Engine_Draw;
		}
		
		public static void Engine_Draw(On.Monocle.Engine.orig_Draw orig, Monocle.Engine self, GameTime time) {
			if (OnEngineDraw != null) OnEngineDraw();
			orig(self, time);
			if (OnEngineDrawLate != null) OnEngineDrawLate();
		}
		
		public static void Everest_InitMods(On.Celeste.Mod.Everest.orig_Initialize orig) {
			orig();
			Entry.Instance.PostInitialize();
		}
		
		public static Hook TapeCollectHook;
		static MethodInfo TapeRoutineInfo = typeof(Cassette).GetMethod("CollectRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
		static FieldInfo TapeThisInfo = TapeRoutineInfo.DeclaringType.GetFields().First((fd) => fd.Name.Contains("this"));
		
		public static void HookMemory() {
			On.Celeste.Strawberry.CollectRoutine += Strawberry_OnCollect;
			On.Celeste.Strawberry.OnPlayer += Strawberry_OnTouch;
			On.Celeste.AutoSplitterInfo.Update += AutoSplitterInfo_Update;
			On.Celeste.HeartGem.RegisterAsCollected += HeartGem_Collected;
			
			IL.Celeste.SummitCheckpoint.Update += Flag_Update_IL;
			
			TapeCollectHook = new Hook(TapeRoutineInfo, typeof(Hooks).GetMethod("Tape_Routine", BindingFlags.Static | BindingFlags.Public));
		}
		
		public static void Flag_Update_IL(ILContext ctx) {
			var csr = new ILCursor(ctx);
			
			csr.GotoNext(MoveType.Before, (Instruction instr) => instr.MatchStfld<SummitCheckpoint>("Activated"));
			csr.Emit(OpCodes.Ldarg_0);
			csr.Emit(OpCodes.Ldfld, typeof(SummitCheckpoint).GetField("Number", BindingFlags.Public | BindingFlags.Instance));
			csr.EmitDelegate<Action<int>>(Flag_Update);
		}
		
		public static void Flag_Update(int num) {
			Memory.Instance.FFlag = num;
		}
		
		public static void AutoSplitterInfo_Update(On.Celeste.AutoSplitterInfo.orig_Update orig, AutoSplitterInfo self) {
			orig(self);
			Memory.Instance.Update();
		}
		
		public static IEnumerator Strawberry_OnCollect(On.Celeste.Strawberry.orig_CollectRoutine orig, Strawberry self, int index) {
			Memory.Instance.FBerry = self;
			return orig(self, index);
		}
		
		public static void Strawberry_OnTouch(On.Celeste.Strawberry.orig_OnPlayer orig, Strawberry self, Player player) {
			orig(self, player);
			Memory.Instance.FTBerry = self;
		}
		
		public static void HeartGem_Collected(On.Celeste.HeartGem.orig_RegisterAsCollected orig, HeartGem self, Level lvl, string poem) {
			orig(self, lvl, poem);
			Memory.Instance.FHeart = self;
		}
		
		public delegate bool orig_TapeRoutine(object stateMachine);
		public static bool Tape_Routine(orig_TapeRoutine orig, object stateMachine) {
			var tape = TapeThisInfo.GetValue(stateMachine);
			Memory.Instance.FTape = (Cassette)tape;
			return orig(stateMachine);
		}
	}
}
