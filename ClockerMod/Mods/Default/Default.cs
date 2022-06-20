using System;

namespace Clocker.Mod
{
	public class DefaultMod : ClockerMod
	{
		public override void InitServer(Server server)
		{
			Server.State.Instance.AddStater("vanilla", State);
		}
		
		public object State(bool prev) {
			return prev ? Memory.Instance.StablePrevious : Memory.Instance.StableCurrent;
		}
		
		public override void InitLate()
		{
			Server.State.Instance.UpdateStates();
		}
		
		public override void PostUpdate(Memory memory)
		{
			Server.State.Instance.UpdateStates();
		}
	}
}
