using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities
{
    [HarmonyPatch(typeof(ProfessionManager), "GetInteractions")]
    public static class ProfessionManagerPatch
    {
        public static bool Prefix(HumanAI human, List<InteractionRestricted> list)
        {
            GetInteractionsForMods(human, list);
            return false;
        }

        /// <summary>
        /// Returns the same as the vanilla code but with the Priority reversed since we aggregate.
        /// I created a separate method so it can be overriden by other mods more easily
        /// </summary>
        /// <param name="human"></param>
        /// <param name="list"></param>
        private static void GetInteractionsForMods(HumanAI human, List<InteractionRestricted> list)
        {
            list.Add(new InteractionRestricted(Interaction.Construct, Int32.MaxValue));
            list.Add(new InteractionRestricted(Interaction.Deconstruct, Int32.MaxValue));
        }
    }
}
