using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using WitchyMods.AbsoluteProfessionPriorities.Framework;

namespace WitchyMods.AbsoluteProfessionPriorities.Patches {

    [HarmonyPatch(typeof(YieldMicroInteraction))]
    public static class YieldMicroInteractionPatch {
        [HarmonyPatch("GetInteractionEnumerable")]
        [HarmonyPrefix]
        private static bool GetInteractionEnumerable_Prefix(Interaction interaction, YieldMicroInteraction __instance, ref IEnumerable<YieldResult> __result) {
            YieldMicroInteractionHelper helper = new YieldMicroInteractionHelper(__instance);
            __result = helper.StartGetInteractionEnumerable(interaction);

            return __result == null;
        }

        [HarmonyPatch("Handle", new Type[] { })]
        [HarmonyPrefix]
        private static bool Handle_Prefix(YieldMicroInteraction __instance, ref YieldMicroInteraction __result) {
            YieldMicroInteractionHelper helper = new YieldMicroInteractionHelper(__instance);
            __result = helper.New_Handle();
            return false;
        }

        [HarmonyPatch("LockInteraction")]
        [HarmonyPrefix]
        private static bool LockInteraction_Prefix(YieldMicroInteraction __instance, ref YieldResult __result) {
            YieldMicroInteractionHelper helper = new YieldMicroInteractionHelper(__instance);
            __result = helper.New_LockInteraction(false);
            return false;
        }
    }
}
