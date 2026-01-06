using System.Reflection.Emit;
using HarmonyLib;
using tairasoul.unity.common.events;
using tairasoul.unity.common.speedrunning.dsl.eventbus;

namespace speedrunningutils.patches;

[HarmonyPatch(typeof(VendorUniversal))]
static class VendorPatches {
	public static void ItemObtained(DATA data, int itemIdx) {
		string name = data.items[itemIdx];
		int value = data.value[itemIdx];
		EventBus.Send(new DslId("ItemPickup"), new DslData([name, 1, value]));
	}

	[HarmonyPatch(nameof(VendorUniversal.BuyIt))]
	[HarmonyTranspiler]
	static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
		return new CodeMatcher(instructions).MatchForward(true,
			new CodeMatch(OpCodes.Ldarg_0),
			new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(VendorUniversal), "data")),
			new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(DATA), "value")),
			new CodeMatch(OpCodes.Ldarg_0),
			new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(VendorUniversal), "CurrentItem")))
		.Repeat(m => m.InsertAndAdvance(
			new CodeInstruction(OpCodes.Ldarg_0),
			new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(VendorUniversal), "data")),
			new CodeInstruction(OpCodes.Ldarg_0),
			new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(VendorUniversal), "CurrentItem")),
			new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(VendorPatches), "ItemObtained"))
		)).InstructionEnumeration();
	}
}