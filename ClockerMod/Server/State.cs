using System;
using System.Net;
using System.Collections.Generic;
using Celeste;
using Clocker.Server;

namespace Clocker.Mod
{
	public partial class Server
	{
		public StateModule State;
		
		public void InitState() {
			Http.Add<StateModule>(out State);
			// The memory update should be subscribed to here with a call to UpdateStates but it is handled in the default mod
		}
		
		public void UnloadState() {
			State.StateHistory.Clear();
			State.Staters.Clear();
			State = null;
		}
		
		[Scannable("/state/")]
		public class StateModule {
			public Dictionary<string, Func<bool, object>> Staters = new Dictionary<string, Func<bool, object>>();
			
			internal Queue<Dictionary<string, object>> StateHistory = new Queue<Dictionary<string, object>>();
			
			public void AddStater(string name, Func<bool, object> stater) {
				Staters.Add(name, stater);
			}
			
			const int HISTORYLIMIT = 60 * 60 * 30;
			internal void UpdateStates() {
				if (StateHistory.Count >= HISTORYLIMIT) StateHistory.Clear();
				var states = GetStates(false);
				StateHistory.Enqueue(states);
			}
			
			Dictionary<string, object> GetStates(bool prev) {
				var states = new Dictionary<string, object>();
				foreach (var stater in Staters) {
					states.Add(stater.Key, stater.Value(prev));
				}
				return states;
			}
			
			[ScanRoute("history.json")]
			public void History(HttpListenerContext ctx) {
				var len = StateHistory.Count;
				var arr = new Dictionary<string, object>[len];
				for (int i = 0; i < len; i++)
					arr[i] = StateHistory.Dequeue();
				ctx.ServeText(arr.ToJson(), ".json");
			}
			
			// TODO: Remove the GetStates call so that mod staters are always only called on the game thread during the gameloop
			[ScanRoute("current.json")]
			public void Current(HttpListenerContext ctx) {
				var states = GetStates(false);
				ctx.ServeText(states.ToJson(), ".json");
			}
			
			[ScanRoute("previous.json")]
			public void Previous(HttpListenerContext ctx) {
				var states = GetStates(true);
				ctx.ServeText(states.ToJson(), ".json");
			}
		}
	}
}
