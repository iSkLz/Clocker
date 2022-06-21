using System;
using System.Collections.Generic;

using Celeste;

namespace Clocker.Mod
{
	/// <summary>
	/// Defines extension methods for querying dialogs for map entities.
	/// </summary>
	public static partial class Info
	{
		/// <summary>
		/// Serializes the given AreaMode instance into an English string.
		/// </summary>
		/// <param name="mode">The mode to serialize.</param>
		/// <returns>"A-Side", B-Side" or "C-Side" depending on the mode's value.</returns>
		public static string GetSide(this AreaMode mode) {
			return (mode == AreaMode.Normal) ? "A-Side" : ((mode == AreaMode.BSide) ? "B-Side" : "C-Side");
		}
		
		/// <summary>
		/// Returns the name of the chapter indexed by the given number.
		/// </summary>
		/// <param name="ID">Index of the chapter.</param>
		/// <returns>The resolved name in English.</returns>
		public static string GetName(this int ID) {
			return (AreaData.Get(ID).Name).DialogLevelEn();
		}
		
		/// <summary>
		/// Returns the name of the levelset of the chapter indexed by the given number.
		/// </summary>
		/// <param name="ID">Index of the chapter.</param>
		/// <returns>The resolved name in English.</returns>
		public static string GetSet(this int ID) {
			return (AreaData.Get(ID).LevelSet).DialogLevelEn();
		}
		
		/// <summary>
		/// Resolves the name of the given checkpoint instance.
		/// </summary>
		/// <param name="cp">The checkpoint instance.</param>
		/// <returns>The resolved name in English.</returns>
		public static string GetName(this CheckpointData cp) {
			return cp.Name.DialogEn();
		}
	}
}
