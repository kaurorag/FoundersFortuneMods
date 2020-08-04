using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFModUtils;
using HarmonyLib;
using WitchyMods.AbsoluteProfessionPriorities.Framework;

namespace WitchyMods.AbsoluteProfessionPriorities.Patches {
    [HarmonyPatch()]
    public static class NewSpecializationsPatch {
        [HarmonyPatch(typeof(ProfessionManager),MethodType.Constructor)]
        [HarmonyPostfix]
        public static void workInteractions_Postfix(ProfessionManager __instance) {
            var workInteractions = __instance.GetFieldValue<Dictionary<ProfessionType, HashSet<Interaction>>>("workInteractions");

            if (!workInteractions[ProfessionType.Farmer].Contains(Specialization.TendToFieldsInteraction))
                workInteractions[ProfessionType.Farmer].Add(Specialization.TendToFieldsInteraction);

            if (!workInteractions[ProfessionType.Forester].Contains(Specialization.ChopTrees))
                workInteractions[ProfessionType.Forester].Add(Specialization.ChopTrees);

            if (!workInteractions[ProfessionType.Forester].Contains(Specialization.SowAndCareForTrees))
                workInteractions[ProfessionType.Forester].Add(Specialization.SowAndCareForTrees);
        }
    }
}
