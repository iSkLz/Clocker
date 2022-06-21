using System;
using Celeste;
using Celeste.Mod;

namespace Clocker.Mod
{
	/// <summary>
	/// Defines extension methods for dialog strings
	/// </summary>
	public static class Lang
	{
		/// <summary>
		/// The English language instance.
		/// </summary>
		/// <remarks>
		/// This is only assigned to once the game fully loads.
		/// </remarks>
		public static Language English;
		
		internal static void Init() {
			// The game has loaded, assign the lang
			English = Dialog.Languages["english"];
			Logger.Log("ClockerLang", "Initialized successfully.");
		}
		
		/// <summary>
		/// Encloses the provided string in curly brackets {}.
		/// </summary>
		/// <param name="name">The string to use.</param>
		/// <returns>The string between two curly brackets.</returns>
		public static string DialogSym(this string name) {
			return "{" + name + "}";
		}
		
		/// <summary>
		/// Resolves the specified dialog name in English.
		/// </summary>
		/// <param name="name">The dialog's name.</param>
		/// <returns>The resultant dialog; or its symbolic name if it doesn't exist.</returns>
		public static string DialogEn(this string name) {
			if (name == null) return null;
			return name.DialogEnEx() ?? name.DialogKeyify().DialogSym();
		}
		
		/// <summary>
		/// Resolves the specified dialog name in English.
		/// </summary>
		/// <param name="name">The dialog's name.</param>
		/// <returns>The resultant dialog; or null if it doesn't exist.</returns>
		public static string DialogEnEx(this string name) {
			if (name == null) return null;
			string dialog;
			if (English.Cleaned.TryGetValue(name.DialogKeyify(), out dialog))
				return dialog;
			return null;
		}
		
		/// <summary>
		/// Resolves a dialog from a list of fallback-in-order names.
		/// </summary>
		/// <param name="names">The list of names in order of resolving.</param>
		/// <returns>The dialog labelled by the first name from the list that exists; or null if all don't exist.</returns>
		public static string DialogEn(this string[] names) {
			string dialog;
			foreach (var name in names) {
				if (English.Cleaned.TryGetValue(name.DialogKeyify(), out dialog)) {
					return dialog;
				}
			}
			return null;
		}
		
		// This is only different from DialogEn in that it has a params array argument and isn't an extension method
		/// <summary>
		/// Resolves a dialog from a list of fallback-in-order names.
		/// </summary>
		/// <param name="names">The list of names in order of resolving.</param>
		/// <returns>The dialog labelled by the first name from the list that exists; or null if all don't exist.</returns>
		public static string DialogEnEx(params string[] names) {
			return names.DialogEn();
		}
		
		/// <summary>
		/// Resolves the given level's name in English.
		/// </summary>
		/// <param name="name">The level's name.</param>
		/// <returns>The resolved name; or the name in proper case if it's not defined.</returns>
		public static string DialogLevelEn(this string name) {
			return DialogEnEx("levelset_" + name, name) ?? name.SpacedPascalCase();
		}
		
		/// <summary>
		/// Resolves the given poem's name in English.
		/// </summary>
		/// <param name="name">The poem's name.</param>
		/// <returns>The resolved poem; or null if it doesn't exist.</returns>
		public static string DialogPoemEn(this string poem) {
			return ("poem_" + poem).DialogEn();
		}
	}
}
