#if !MODKIT
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace WitchyMods.UIImprovements
{
    [HarmonyPatch(typeof(PortraitPanel), "SetHuman")]
    public static class PortraitPanelPatch
    {
        public static void Postfix(PortraitPanel __instance, HumanAI human)
        {
            GameObject namePanelWrapper = __instance.background.transform.Find("namePanelWrapper").gameObject;
            SatisfactionPointsText spText = namePanelWrapper.transform.Find("Panel").GetComponentInChildren<SatisfactionPointsText>(true);
            PortraitCurrentTask ct = __instance.background.GetComponentInChildren<PortraitCurrentTask>(); ;

            spText.SetHuman(human);
            ct.SetHuman(human);
        }
    }


}
#endif