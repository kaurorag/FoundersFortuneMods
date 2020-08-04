using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFModUtils;
using HarmonyLib;
using UnityEngine;
using WitchyMods.AbsoluteProfessionPriorities.Framework;

namespace WitchyMods.AbsoluteProfessionPriorities.Patches {

    [HarmonyPatch]
    public static class ForesterPatch {
        [HarmonyPatch(typeof(ResourceModule), "GetInteractions")]
        [HarmonyPostfix]
        public static void ResourceModule_GetInteractions_Postfix(ResourceModule __instance, ref IEnumerable<InteractionRestricted> __result) {
            if (__instance.GetResource() == Resource.Wood) {
                __result = new List<InteractionRestricted>() {
                    new InteractionRestricted(Specialization.ChopTrees, new List<InteractionRestriction>(){
                        new InteractionRestrictionDesignation(Designation.CutTree),
                        new InteractionRestrictionWoodStockpile()
                    })
                };
            }
        }

        [HarmonyPatch(typeof(GrowingSpotModule), "GetInteractions")]
        [HarmonyPostfix]
        public static void GrowingSpotModule_GetInteractions_Postfix(GrowingSpotModule __instance, ref IEnumerable<InteractionRestricted> __result) {
            List<InteractionRestricted> list = new List<InteractionRestricted>();

            if(__instance.GetFieldValue<int>("currentPhase") >= 0) {
                list.Add(new InteractionRestricted(Interaction.RemovePlant, new InteractionRestrictionSkill(__instance.requiredSkill)));
            }

            list.Add(new InteractionRestricted(Specialization.SowAndCareForTrees, new InteractionRestrictionCareForTrees(
                __instance.requiredSkill, Time.time + __instance.GetFieldValue<float>("timeUntilNextCareInteraction"))));

            __result = list;
        }
    }
}
