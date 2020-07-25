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

            if (interaction == YieldMicroInteractionHelper.TendToFieldsInteraction) {
                __result = helper.ContinueInteraction(helper.TendToFields(), 20f);
                return false;
            }
            else if(interaction == Interaction.GatherResource && helper.InteractionInfo.isContinuationOrSubtask)  {
                __result = helper.WorkOnFurniture();
                return false;
            }
            else if(interaction == Interaction.WaterPlant) {
                __result = helper.WaterPlant();
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(YieldMicroInteraction), "Continue")]
        [HarmonyPrefix]
        private static bool Continue_Prefix(YieldMicroInteraction __instance, float distance) {
            YieldMicroInteractionHelper helper = new YieldMicroInteractionHelper(__instance);

            if (helper.Interaction == Interaction.Sow) return false;
            else if (helper.Interaction == YieldMicroInteractionHelper.TendToFieldsInteraction) {
                helper.ContinueCustom();
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(SoilModule))]
    public static class SoilModulePatch {

        [HarmonyPatch("GetInteractions")]
        [HarmonyPostfix]
        public static void GetInteractions_Prefix(SoilModule __instance, ref IEnumerable<InteractionRestricted> __result, HumanAI actor, bool issuedByAI, bool objectInteraction) {
            if (__result.Any(x => x.interaction != Interaction.RemovePlant)) {
                __result.AddItem(new InteractionRestricted(YieldMicroInteractionHelper.TendToFieldsInteraction));
            }
        }

        [HarmonyPatch("GetEverPossibleNotRegisteredInteractions")]
        [HarmonyPostfix]
        public static void GetEverPossibleNotRegisteredInteractions_Prefix(SoilModule __instance, ref List<Interaction> __result) {
            __result.Add(YieldMicroInteractionHelper.TendToFieldsInteraction);
        }

    }

    [HarmonyPatch(typeof(HumanAI), "ShouldInterruptInteraction")]
    public static class HumanAIPatch {
        public static void Postfix(HumanAI __instance, ref float __result) {
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
                (Dictionary < ProfessionType, HashSet < Interaction >> )info.GetValue(__instance);

            if (!workInteractions[ProfessionType.Farmer].Contains(YieldMicroInteractionHelper.TendToFieldsInteraction))
                workInteractions[ProfessionType.Farmer].Add(YieldMicroInteractionHelper.TendToFieldsInteraction);
        }
    }

    [HarmonyPatch(typeof(InteractionInfo))]
    public static class InteractablePatch {
        [HarmonyPatch("IsRestricted")]
        [HarmonyPrefix]
        public static bool IsRestrictedPrefix(InteractionInfo __instance, HumanAI actor, ref bool __result) {
            if(__instance.interaction == YieldMicroInteractionHelper.TendToFieldsInteraction) {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Profession))]
    public static class ProfessionPatch {
        [HarmonyPatch("GetInteractions")]
        [HarmonyPrefix]
        public static bool GetInteractions(HumanAI human, List<InteractionRestricted> list) {
            DebugLogger.Log("Original methods was called!");
            return false;
        }
    }
}
