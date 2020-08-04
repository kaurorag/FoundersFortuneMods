using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FFModUtils.Extensions;
using HarmonyLib;
using UnityEngine;
using WitchyMods.AbsoluteProfessionPriorities.Framework;

namespace WitchyMods.AbsoluteProfessionPriorities.Patches {
    [HarmonyPatch(typeof(HumanAI))]
    public static class SearchInteractablesPatch {
        [HarmonyPatch("SearchForInteractableWithInteraction")]
        [HarmonyPrefix]
        public static bool SearchForInteractableWithInteraction_Prefix(HumanAI __instance, ref Interactable __result, Interaction interaction, float distance, List<InteractionRestriction> restrictions = null, bool probablyReachablesOnly = false, bool objectInteraction = false, Vector3 positionOverride = default(Vector3)) {
            switch (interaction) {
                case Interaction.Construct:
                    __result = __instance.SearchForInteractbleWithConstructPriority(interaction, distance, restrictions, probablyReachablesOnly, objectInteraction, positionOverride);
                    break;
                default:
                    __result = __instance.SearchForInteractablesWithInteraction(interaction, distance, restrictions, probablyReachablesOnly, objectInteraction, positionOverride).FirstOrDefault();
                    break;
            }

            return false;
        }

        [HarmonyPatch("SearchForInteractbleWithConstructPriority")]
        [HarmonyPrefix]
        public static bool SearchForInteractbleWithConstructPriority_Prefix(HumanAI __instance, ref Interactable __result, Interaction interaction, float distance, List<InteractionRestriction> restrictions = null, bool probablyReachablesOnly = false, bool objectInteraction = false, Vector3 positionOverride = default(Vector3)) {
            Vector3 position = (positionOverride == default(Vector3)) ? __instance.GetPosition() : positionOverride;
            int positionFloor = Mathf.RoundToInt(position.y / 2.3f);
            List<Pair<Interactable, float>> priorities = new List<Pair<Interactable, float>>();

            foreach (Interactable interactable in __instance.SearchForInteractablesWithInteraction(Interaction.Construct, distance, restrictions, probablyReachablesOnly, objectInteraction, positionOverride)) {

                float dist = Vector3.Distance(position, interactable.GetWorldPosition());
                float priority = dist;

                if (interactable is FloorInteractable) {
                    priority += 0;
                } else if (interactable is FurnitureInteractable && interactable.GetPrimaryHolder<Furniture>().HasModule<WallPartModule>() && interactable.GetPrimaryHolder<Furniture>().GetModule<WallPartModule>().isDoor) {
                    priority += 20;
                } else if (interactable is WallInteractable) {
                    priority += 30;
                } else if (interactable is FurnitureInteractable && interactable.GetPrimaryHolder<Furniture>().HasModule<StairsModule>()) {
                    priority += 40;
                } else {
                    priority += 120;
                }

                int floor = Mathf.RoundToInt(interactable.GetWorldPosition().y / 2.3f);
                priority += 80 * Mathf.Abs(floor - positionFloor);

                priorities.Add(new Pair<Interactable, float>(interactable, priority));
            }

            __result = priorities.MinBy(x => x.second).first;

            return false;
        }

        [HarmonyPatch("SearchForInteractablesWithInteraction")]
        [HarmonyPrefix]
        public static bool SearchForInteractablesWithInteraction_Prefix(HumanAI __instance, ref IEnumerable<Interactable> __result, Interaction interaction, float distance, List<InteractionRestriction> restrictions = null, bool probablyReachablesOnly = false, bool objectInteraction = false, Vector3 positionOverride = default(Vector3)) {
            __result = InteractableBookkeeperHelper.SearchForInteractablesWithInteraction(__instance, interaction, distance, restrictions, probablyReachablesOnly, objectInteraction, positionOverride);
            return false;
        }
    }
}
