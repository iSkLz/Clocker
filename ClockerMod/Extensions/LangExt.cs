using System;
using Celeste;
using Celeste.Mod;

namespace Clocker.Mod
{
	public static class Lang
	{
		public static Language English;
		
		public static void Init() {
			English = Dialog.Languages["english"];
			Logger.Log("ClockerLang", "Initialized successfully.");
		}
		
		public static string DialogSym(this string name) {
			return "{" + name + "}";
		}
		
		public static string DialogEn(this string name) {
			if (name == null) return null;
			return name.DialogEnEx() ?? name.DialogKeyify().DialogSym();
		}
		
		public static string DialogEnEx(this string name) {
			if (name == null) return null;
			string dialog;
			if (English.Cleaned.TryGetValue(name.DialogKeyify(), out dialog))
				return dialog;
			return null;
		}
		
		public static string DialogEn(this string[] names) {
			string dialog;
			foreach (var name in names) {
				if (English.Cleaned.TryGetValue(name.DialogKeyify(), out dialog)) {
					return dialog;
				}
			}
			return null;
		}
		
		public static string DialogEnEx(params string[] names) {
			return names.DialogEn();
		}
		
		public static string DialogLevelEn(this string name) {
			return DialogEnEx("levelset_" + name, name) ?? name.SpacedPascalCase();
		}
		
		public static string DialogPoemEn(this string poem) {
			return ("poem_" + poem).DialogEn();
		}
	}
}
