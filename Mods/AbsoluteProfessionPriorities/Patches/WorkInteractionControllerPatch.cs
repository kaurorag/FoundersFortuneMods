using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WitchyMods.AbsoluteProfessionPriorities.Framework;

namespace WitchyMods.AbsoluteProfessionPriorities {
    [HarmonyPatch(typeof(WorkInteractionController), "GetInteractionProposal")]
    public static class WorkInteractionControllerPatch {

        private static WorkInteractionController instance;

        /// <summary>
        /// Overwrites completely the method 'GetInteractionProposal' from the WorkInteractionController
        /// </summary>
        /// <param name="__instance">Instance of the WorkInteractionController</param>
        /// <param name="human">Original HumanAI argument</param>
        /// <param name="__result">The result of the method</param>
        /// <returns>False to prevent the original method from executing</returns>
        public static bool Prefix(WorkInteractionController __instance, HumanAI human, ref InteractionInfo __result) {
            instance = __instance;
            __result = null;

            //This bit is copy/pasted from the original method.  It checks if the 'human' is a colonist and can actually work
            if (human.IsLockedUp() || !human.WillWorkAutomatically() || human.IsLazy() || human.faction.GetFactionType() != FactionType.Colony || human.controlMode == ControlMode.Combat)
                return false;

            //Don't override manual interactions
            InteractionInfo currentInteraction = human.GetCurrentInteractionInfo();
            if (currentInteraction != null && !currentInteraction.issuedByAI) return false;

            //Init the human in case it's new
            AbsoluteProfessionPrioritiesMod.Instance.InitColonist(human);

            //Creates a dictionary of all of the colonist's active professions, grouped by priorities (number of stars)
            Dictionary<int, List<ProfessionType>> profs = human.professionManager.professions.Values.Where(
                x => x.priority > 0).GroupBy(x => x.priority).ToDictionary(
                x => x.Key, x => x.Select(y => y.type).ToList());

            foreach (var priority in profs.Keys.OrderByDescending(x => x)) {
                foreach (var profession in profs[priority].OrderBy(x => UnityEngine.Random.Range(0f, 1f))) {
                    if (!AbsoluteProfessionPrioritiesMod.Instance.ColonistsData[human.GetID()].ContainsKey(profession))
                        continue;

                    foreach (var info in GetNextInteraction(__instance, human, profession)) {
                        if (info != null) {
                            __result = info;
                            return false;
                        }
                    }
                }
            }

            return false;
        }

        private static IEnumerable<InteractionInfo> GetNextInteraction(WorkInteractionController __instance, HumanAI human, ProfessionType profession) {
            //Get all the specializations for this human for this profession
            List<Specialization> specs = AbsoluteProfessionPrioritiesMod.Instance.ColonistsData[human.GetID()][profession].Values.ToList();
            specs = specs.Where(x => x.Active).OrderBy(x => UnityEngine.Random.Range(0f, 1f)).OrderBy(x => x.Priority).ToList();

            foreach(var spec in specs) {
                foreach (var interaction in spec.GetNextInteraction(human))
                    yield return interaction;
            }
        }

        private static Resource[] FieldsResources = new Resource[] { Resource.HealingPlantCultivated, Resource.Strawberry, Resource.Tomato, Resource.Pumpkin, Resource.Potato, Resource.Wheat };
        private static String[] Meds = new string[] { "healingPotion", "medicine" };

        public static InteractionInfo CheckInteraction(InteractionRestricted interactionRestricted, HumanAI human) {
            return AccessTools.Method(instance.GetType(), "CheckInteraction", new Type[] { typeof(InteractionRestricted), typeof(HumanAI) }).Invoke(instance, new object[] { interactionRestricted, human }) as InteractionInfo;
        }

        public static bool CheckInteraction(Interactable interactable, InteractionRestricted interactionRestricted, HumanAI human) {
            return (bool)AccessTools.Method(instance.GetType(), "CheckInteraction", new Type[] { typeof(Interactable), typeof(InteractionRestricted), typeof(HumanAI) }).Invoke(instance, new object[] { interactable, interactionRestricted, human });
        }
    }
}
