using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace WitchyMods.AbsoluteProfessionPriorities {


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
    }

    [HarmonyPatch(typeof(SoilModule))]
    public static class SoilModulePatch {

        [HarmonyPatch("GetInteractions")]
        [HarmonyPostfix]
        public static void GetInteractions_Postfix(SoilModule __instance, ref IEnumerable<InteractionRestricted> __result, HumanAI actor, bool issuedByAI, bool objectInteraction) {
            __result = __result.Concat(new[] { new InteractionRestricted(YieldMicroInteractionHelper.TendToFieldsInteraction) });
        }

        [HarmonyPatch("GetEverPossibleNotRegisteredInteractions")]
        [HarmonyPostfix]
        public static void GetEverPossibleNotRegisteredInteractions_Postfix(SoilModule __instance, ref List<Interaction> __result) {
            __result.Add(YieldMicroInteractionHelper.TendToFieldsInteraction);
        }
    }

    [HarmonyPatch(typeof(HumanAI))]
    public static class HumanAIPatch {
        [HarmonyPatch("ShouldInterruptInteraction")]
        [HarmonyPostfix]
        public static void ShouldInterruptInteraction(HumanAI __instance, ref float __result) {
            if (__instance.GetCurrentInteraction() == YieldMicroInteractionHelper.TendToFieldsInteraction) {
                __result = 0f;
            }
        }
    }

    [HarmonyPatch(typeof(ProfessionManager))]
    public static class ProfessionManagerPatch {
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPostfix]
        public static void workInteractions_Postfix(ProfessionManager __instance) {
            System.Reflection.FieldInfo info = AccessTools.Field(typeof(ProfessionManager), "workInteractions");

            Dictionary<ProfessionType, HashSet<Interaction>> workInteractions =
                (Dictionary<ProfessionType, HashSet<Interaction>>)info.GetValue(__instance);

            if (!workInteractions[ProfessionType.Farmer].Contains(YieldMicroInteractionHelper.TendToFieldsInteraction))
                workInteractions[ProfessionType.Farmer].Add(YieldMicroInteractionHelper.TendToFieldsInteraction);
        }
    }

    [HarmonyPatch(typeof(InteractionInfo))]
    public static class InteractablePatch {
        [HarmonyPatch("IsRestricted")]
        [HarmonyPrefix]
        public static bool IsRestrictedPrefix(InteractionInfo __instance, HumanAI actor, ref bool __result) {
            if (__instance.interaction == YieldMicroInteractionHelper.TendToFieldsInteraction) {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
