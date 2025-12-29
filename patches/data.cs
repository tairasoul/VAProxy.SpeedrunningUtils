using HarmonyLib;
using tairasoul.unity.common.events;
using tairasoul.unity.common.speedrunning.dsl.eventbus;

namespace speedrunningutils.patches;

[HarmonyPatch(typeof(DATA))]
static class DataPatches {
	[HarmonyPatch(nameof(DATA.AddLoot))]
	[HarmonyPrefix]
	static void AddLootEvent(DATA __instance, int ind, int val) {
		string name = __instance.items[ind];
		int valBefore = __instance.value[ind];
		EventBus.Send(new DslId("ItemPickup"), new DslData([name, val, valBefore]));
	}
}