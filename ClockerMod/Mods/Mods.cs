using System;
using System.Collections.Generic;

namespace Clocker.Mod
{
	/// <summary>
	/// Keeps tracks of the mods attached to Clocker.
	/// </summary>
	public class Mods
	{
		/// <summary>
		/// The singleton instance of the modskeeper.
		/// </summary>
		public static Mods Instance;
		
		/// <summary>
		/// Initializes the modskeeper.
		/// </summary>
		public static void Init() {
			Instance = new Mods();
		}
		
		/// <summary>
		/// Enlists the attached mods.
		/// </summary>
		public List<ClockerMod> List { get; private set; }
		
		// Keeps mods indexed by type
		Dictionary<Type, ClockerMod> IndexedList;
		
		internal Mods()
		{
			List = new List<ClockerMod>();
			IndexedList = new Dictionary<Type, ClockerMod>();
		}
		
		/// <summary>
		/// Attaches a mod of the specified type.
		/// </summary>
		/// <returns>The created instance of the mod.</returns>
		public ClockerMod Add<T>() where T : ClockerMod, new() {
			var inst = new T();
			Add(inst);
			return inst;
		}
		
		/// <summary>
		/// Attaches the provided instance of the mod.
		/// </summary>
		/// <param name="mod">An instance of the mod.</param>
		public void Add<T>(T mod) where T : ClockerMod {
			List.Add(mod);
			IndexedList.Add(mod.GetType(), mod);
		}
		
		/// <summary>
		/// Retrieves the attached mod of the provided type.
		/// </summary>
		/// <returns>The mod instance.</returns>
		public ClockerMod Get<T>() where T : ClockerMod {
			return IndexedList[typeof(T)];
		}
		
		/// <summary>
		/// Retrieves the attached mod of the provided type.
		/// </summary>
		/// <param name="type">Type of the mod.</param>
		/// <returns>The mod instance.</returns>
		public ClockerMod Get(Type type) {
			return IndexedList[type];
		}
		
		/// <summary>
		/// Calls an action on each mod.
		/// </summary>
		/// <param name="each">The delegate each mod gets passed to.</param>
		public void Do(Action<ClockerMod> each) {
			foreach (var mod in List) {
				each(mod);
			}
		}
	}
}
