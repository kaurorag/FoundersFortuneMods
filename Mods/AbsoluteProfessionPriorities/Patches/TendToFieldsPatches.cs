using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using WitchyMods.AbsoluteProfessionPriorities.Framework;

namespace WitchyMods.AbsoluteProfessionPriorities {

    [HarmonyPatch(typeof(SoilModule))]
    public static class SoilModulePatch {

        [HarmonyPatch("GetInteractions")]
        [HarmonyPostfix]
        public static void GetInteractions_Postfix(SoilModule __instance, ref IEnumerable<InteractionRestricted> __result, HumanAI actor, bool issuedByAI, bool objectInteraction) {
            if (__result.Any(x => x.interaction != Interaction.RemovePlant && x.interaction != Interaction.RemoveInfestedPlants)) {
                __result = __result.Concat(new[] { new InteractionRestricted(Specialization.TendToFieldsInteraction) });
            }
        }

        [HarmonyPatch("GetEverPossibleNotRegisteredInteractions")]
        [HarmonyPostfix]
        public static void GetEverPossibleNotRegisteredInteractions_Postfix(SoilModule __instance, ref List<Interaction> __result) {
            __result.Add(Specialization.TendToFieldsInteraction);
        }
    }

    //[HarmonyPatch(typeof(InteractionInfo))]
    //public static class InteractablePatch {
    //    [HarmonyPatch("IsRestricted")]
    //    [HarmonyPrefix]
    //    public static bool IsRestrictedPrefix(InteractionInfo __instance, HumanAI actor, ref bool __result) {
    //        if (__instance.interaction == Specialization.TendToFieldsInteraction) {
    //            __result = false;
    //            return false;
    //        }
    //        return true;
    //    }
    //}
}
