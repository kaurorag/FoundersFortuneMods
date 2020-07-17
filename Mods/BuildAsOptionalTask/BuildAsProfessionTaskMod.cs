using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace WitchyMods.BuildAsProfessionTask
{
    [Serializable]
    public class BuildAsProfessionTaskMod : Mod
    {
        public override void Load()
        {
            Harmony harmony = new Harmony("BuildAsProfessionTask");

            //Check if the AbsoluteProfessionPrioritiesMod is registered
            Mod appMod = ModHandler.mods.scriptMods.FirstOrDefault(x => x.GetType().Name == "AbsoluteProfessionPrioritiesMod");

            if(appMod == null)
            {
                harmony.Patch(
                    AccessTools.Method(typeof(Profession), "GetInteractions"), 
                    new HarmonyMethod(AccessTools.Method(typeof(ProfessionPatch), "GetInteractionsPrefix"))
                    );
            }
            else
            {
                Type aPPProfessionPatch = appMod.GetType().Assembly.GetType("WitchyMods.AbsoluteProfessionPriorities.ProfessionPatch");

                MethodBase mOriginalMethod = AccessTools.Method(aPPProfessionPatch, "GetInteractionsForMods");
                MethodInfo mPatchMethod = AccessTools.Method(typeof(APPProfessionPatch), "GetInteractionsForModsPrefix");

                harmony.Patch(mOriginalMethod, new HarmonyMethod(mPatchMethod));
            }
        }

        public override void Start()
        {
            //Adds the interactions "Construct" and "Deconstruct" to the dictionary of available
            //work interactions for all professions.  
            //Though the dictionary is static and initialized in the static constructor
            //of ProfessionManager, it is not always initialized.
            //Thus, we check if it is and if we had already added those elements before adding them

            if (ProfessionManager.workInteractions == null) return;
            if (ProfessionManager.workInteractions.Count == 0) return;

            foreach (ProfessionType pType in Enum.GetValues(typeof(ProfessionType)))
            {
                if (pType == ProfessionType.NoJob || pType == ProfessionType.Soldier || pType == ProfessionType.Builder) continue;

                if (!ProfessionManager.workInteractions.ContainsKey(pType)) continue;

                if (!ProfessionManager.workInteractions[pType].Contains(Interaction.Construct))
                    ProfessionManager.workInteractions[pType].Add(Interaction.Construct);

                if (!ProfessionManager.workInteractions[pType].Contains(Interaction.Deconstruct))
                    ProfessionManager.workInteractions[pType].Add(Interaction.Deconstruct);
            }
        }

        public override void Update() { }
    }

    public static class ProfessionPatch
    {
        public static void GetInteractionsPrefix(Profession __instance, HumanAI human, List<InteractionRestricted> list)
        {
            //If the colonist has the specialization "Build" enabled, adds Construct and Deconstruct
            //to the list of available interactions
            if (__instance.HasSpecialization("build"))
            {
                list.Add(new InteractionRestricted(Interaction.Construct, __instance.priority * 20));
                list.Add(new InteractionRestricted(Interaction.Deconstruct, __instance.priority * 20));
            }
        }
    }

    public static class APPProfessionPatch
    {
        public static void GetInteractionsForModsPrefix(Profession p, String spec, int index, List<InteractionRestricted> list, int specPriority, ref bool success)
        {
            switch (spec)
            {
                case "build":
                    list.Add(new InteractionRestricted(Interaction.Construct, specPriority));
                    list.Add(new InteractionRestricted(Interaction.Deconstruct, specPriority));
                    success = true;
                    break;
            }
        }
    }
}
