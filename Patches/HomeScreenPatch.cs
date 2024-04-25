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
            FieldInfo currentSlotInfo = typeof(SaveSlotSelect).GetField("currentSlot", BindingFlags.Instance | BindingFlags.NonPublic);
            Plugin.CurrentSaveSlot = (int)currentSlotInfo.GetValue(select);
        }
    }
}