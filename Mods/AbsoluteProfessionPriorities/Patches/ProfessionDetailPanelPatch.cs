#if !MODKIT
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace WitchyMods.AbsoluteProfessionPriorities
{
    [HarmonyPatch(typeof(ProfessionDetailPanel))]
    public static class ProfessionDetailPanelPatch
    {
        /// <summary>
        /// Adds some code at the beginning of the "Init" method.
        /// We disabled the original SecializationTogglePanel and add our own instead
        /// </summary>
        /// <param name="__instance">Instance of the ProfessionDetailPanel</param>
        /// <param name="type">Type of the profession</param>
        [HarmonyPrefix()]
        [HarmonyPatch("Init")]
        public static void InitPrefix(ProfessionDetailPanel __instance, ProfessionType type)
        {
            //We don't add this panel for Soldier or NoJob
            if (type != ProfessionType.Soldier && type != ProfessionType.NoJob)
            {
                //Checks if we already initialized the Specialization Panel.  If so, do nothing
                SpecializationPanel p = __instance.GetComponentInChildren<SpecializationPanel>(true);
                if (p != null) return;

                //Get the parent of the original panel
                GameObject parent = __instance.specializationTogglePanel.transform.parent.gameObject;

                //Create our own panel 
                GameObject newSpecPanel = GameObject.Instantiate(ModHandler.mods.gameObjects["SpecializationPanelModded"], parent.transform);
                p = newSpecPanel.GetComponent<SpecializationPanel>();
                p.Init(type, ModHandler.mods.GetProfessionSpecializations(type).ToList());

                //For the collapse button to work, we need to add our panel to the accordion
                //We also remove the original panel from it
                AccordionPanel aPanel = __instance.GetComponent<AccordionPanel>();
                aPanel.accordionObjects.Add(newSpecPanel);
                aPanel.accordionObjects.Remove(__instance.specializationTogglePanel.gameObject);

                //Removing the original panel from the UI would cause bugs so instead we just disable it
                __instance.specializationTogglePanel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Adds some code at the end of SetHuman
        /// </summary>
        /// <param name="__instance">Instance of ProfessionDetailPanel</param>
        /// <param name="human">Human to be set</param>
        [HarmonyPostfix()]
        [HarmonyPatch("SetHuman")]
        public static void SetHumanPostFix(ProfessionDetailPanel __instance, HumanAI human)
        {
            //Get our new panel
            SpecializationPanel p = __instance.GetComponentInChildren<SpecializationPanel>(true);

            //If it exists (depends on the stage of the lifecycle of the UI)
            //If it does, call SetHuman on our panel
            if(p != null) p.SetHuman(human);
        }
    }
}
#endif