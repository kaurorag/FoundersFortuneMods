using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace WitchyMods.AbsoluteProfessionPriorities.Patches {
    [HarmonyPatch]
    public static class InteractionLockPatch {

        [HarmonyPatch(typeof(HumanAI), "StopSpecificTask")]
        [HarmonyPostfix]
        public static void StopSpecificTask_Postfix(HumanAI __instance, Interactable interactable) {
            if (interactable != null && interactable.interactionLockID < 0 && interactable.interactionLockID == Math.Abs(__instance.GetID()))
                interactable.SetInteractionLockID(0);
        }

        [HarmonyPatch(typeof(Interactable), "GetInteractions")]
        [HarmonyPostfix]
        public static void GetInteractions_Postfix(Interactable __instance, ref IEnumerable<InteractionRestricted> __result, HumanAI actor, bool issuedByAI, bool objectInteraction = false) {
            if(__result != null && __instance.interactionLockID < 0) {
                bool shouldAddRestriction = Math.Abs(__instance.interactionLockID) != actor.GetID();

                foreach(var x in __result) {
                    var lockedRestriction = x.restrictions == null ? null : x.restrictions.OfType<InteractionRestrictionLocked>().FirstOrDefault();

                    if (lockedRestriction != null && !shouldAddRestriction)
                        x.restrictions.Remove(lockedRestriction);
                    else if (lockedRestriction == null && shouldAddRestriction)
                        x.AddRestriction(new InteractionRestrictionLocked());
                }
            }
        }
    }
}
