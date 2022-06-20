using System;
using System.Threading;

namespace Clocker.Utils
{
	public class EventResolver
	{
		public static void Wait(uint interval, params EventResolver[] resolvers) {
			foreach (var resolver in resolvers) {
				resolver.Wait(interval);
			}
		}
		
		public static void Wait(TimeSpan interval, params EventResolver[] resolvers) {
			Wait((uint)interval.TotalMilliseconds, resolvers);
		}
		
		public EventResolver(bool init = false)
		{
			Resolved = init;
		}
		
		public bool Resolved { get; private set; }
		
		public void Resolve() {
			Resolved = true;
		}
		
		public void Reset() {
			Resolved = false;
		}
		
		public void Wait(uint interval) {
			while (!Resolved) {
				Thread.Sleep((int)interval);
			}
		}
		
		public void Wait(TimeSpan interval) {
			Wait((uint)interval.TotalMilliseconds);
		}
	}
}
