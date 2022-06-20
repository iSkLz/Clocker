using System;

namespace Clocker.Mod
{
	public class DefaultMod : ClockerMod
	{
		public override void InitServer(Server server)
		{
			Server.Instance.State.AddStater("vanilla", State);
		}
		
		public object State(bool prev) {
			return prev ? Memory.Instance.StablePrevious : Memory.Instance.StableCurrent;
		}
		
		public override void InitLate()
		{
			Server.Instance.State.UpdateStates();
		}
		
		public override void PostUpdate(Memory memory)
		{
			Server.Instance.State.UpdateStates();
		}
	}
}
