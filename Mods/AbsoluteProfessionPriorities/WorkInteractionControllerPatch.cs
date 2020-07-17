using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities
{
    [HarmonyPatch(typeof(WorkInteractionController), "GetInteractionProposal")]
    public static class WorkInteractionControllerPatch
    {
        /// <summary>
        /// Overwrites completely the method 'GetInteractionProposal' from the WorkInteractionController
        /// </summary>
        /// <param name="__instance">Instance of the WorkInteractionController</param>
        /// <param name="human">Original HumanAI argument</param>
        /// <param name="__result">The result of the method</param>
        /// <returns>False to prevent the original method from executing</returns>
        public static bool Prefix(WorkInteractionController __instance, HumanAI human, ref InteractionInfo __result)
        {
            __result = null;

            //This bit is copy/pasted from the original method.  It checks if the 'human' is a colonist and can actually work
            if (human.IsLockedUp() || !human.WillWorkAutomatically() || human.IsLazy() || human.faction.GetFactionType() != FactionType.Colony || human.controlMode == ControlMode.Combat)
                return false;

            //Allows us to call the private method CheckInteraction later on
            MethodInfo mInfo = AccessTools.Method(typeof(WorkInteractionController),"CheckInteraction", new Type[] {typeof(InteractionRestricted), typeof(HumanAI) });

            //Creates a dictionary of all of the colonist's active professions, grouped by priorities (number of stars)
            Dictionary<int, List<Profession>> profs = human.professionManager.professions.Values.Where(
                x => x.priority > 0).GroupBy(x => x.priority).ToDictionary(
                x => x.Key, x => x.ToList());

            List<InteractionRestricted> availableInteractions = new List<InteractionRestricted>();

            //For each priority (number of stars), starting by the highest to the lowest
            foreach (var professionPriority in profs.Keys.OrderByDescending(x => x))
            {
                //Build a list of available interactions for all professions with the same priority
                availableInteractions.Clear();
                foreach (var profession in profs[professionPriority])
                {
                    profession.GetInteractions(human, availableInteractions);
                }

                //For all found interactions, test each.  If it's 'doable', we have found the next interaction and return it
                //to be queued
                foreach (var task in availableInteractions.OrderByDescending(x=>x.priority))
                {
                    InteractionInfo info = (InteractionInfo)mInfo.Invoke(__instance, new Object[] { task, human });
                    if (info != null)
                    {
                        __result = info;
                        return false;
                    }
                }
            }

            return false;
        }
    }
}
