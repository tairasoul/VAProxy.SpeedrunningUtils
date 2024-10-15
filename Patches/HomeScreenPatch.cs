using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace SpeedrunningUtils 
{
	[HarmonyPatch(typeof(HomeScreen))]
	internal static class HomeScreenPatches {
		[HarmonyPatch("Star")]
		[HarmonyPrefix]
		internal static void Star(HomeScreen __instance) {
			SaveSlotSelect select = GameObject.FindObjectOfType<SaveSlotSelect>();
			Plugin.CurrentSaveSlot = select.currentSlot;
		}
	}
}