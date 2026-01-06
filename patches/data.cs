using HarmonyLib;
using tairasoul.unity.common.events;
using tairasoul.unity.common.speedrunning.dsl.eventbus;

namespace speedrunningutils.patches;

[HarmonyPatch(typeof(DATA))]
static class DataPatches {
	[HarmonyPatch(nameof(DATA.AddLoot))]
	[HarmonyPostfix]
	static void AddLootEvent(DATA __instance, int ind, int val) {
		string name = __instance.items[ind];
		int valAfter = __instance.value[ind];
		EventBus.Send(new DslId("ItemPickup"), new DslData([name, val, valAfter]));
	}

	[HarmonyPatch(nameof(DATA.AddWeapon))]
	[HarmonyPostfix]
	static void AddWeaponEvent(DATA __instance, int __0) {
		string name = __instance.items[__0];
		EventBus.Send(new DslId("ItemPickup"), new DslData([name, 1, 1]));
	}
}