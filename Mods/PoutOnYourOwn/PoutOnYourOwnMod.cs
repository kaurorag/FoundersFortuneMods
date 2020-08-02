using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection;

namespace PoutOnYourOwn {
    [Serializable]
    public class PoutOnYourOwnMod : Mod {
        public override void Load() {
            Harmony harmony = new Harmony("PoutOnYourOwn");
            harmony.PatchAll();

            UnityEngine.MonoBehaviour.print("PoutOnYourOwn loaded");
        }

        public override void Start() {
        }

        public override void Update() {
        }
    }

    [HarmonyPatch(typeof(IdleInteractionController), "GetRandomHumanIdleInteraction")]
    public static class IdleInteractionControllerPatch {
        private static Interaction[] _AllowedInteractions = new Interaction[] { Interaction.Relax, Interaction.Sit, Interaction.Enjoy, Interaction.Dance };

        public static void Postfix(IdleInteractionController __instance, ref Interaction __result) {
            FieldInfo fInfo = typeof(IdleInteractionController).GetField("human", BindingFlags.NonPublic | BindingFlags.Instance);
            HumanAI human = (HumanAI)fInfo.GetValue(__instance);

            if ((human.IsLazy() || human.uncontrollableReasons.Count > 0) && !_AllowedInteractions.Contains(__result)) {
                __result = _AllowedInteractions[new Random().Next(0, _AllowedInteractions.Length)];
            }
        }
    }

    [HarmonyPatch(typeof(NeedsInteractionController), "GetInteractionProposal")]
    public static class NeedsInteractionControllerPatch {

        public static void Postfix(NeedsInteractionController __instance, HumanAI human, ref InteractionInfo __result) {
            if ((human.IsLazy() || human.uncontrollableReasons.Count > 0) && !human.IsFullyHealthy() && !human.IsLockedUp()) {
                Interactable interactable = null;
                OwnershipModule module = human.GetOwnedStuff(OwnershipCategory.Bed);

                if (module != null && module.parent.GetInteractable().GetPossibleInteractions(human, true, false).Contains(Interaction.Sleep) && 
                    human.humanNavigation.IsInteractableProbablyReachable(module.parent.GetID())) {
                    interactable = module.parent.GetInteractable();
                } 

                if (interactable != null && interactable.IsValid()) {
                    __result = new InteractionInfo(Interaction.SleepUntilHealthy, interactable, null, true, 120, true, true);
                }
            }
        }
    }
}
