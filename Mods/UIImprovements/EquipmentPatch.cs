#if !MODKIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace WitchyMods.UIImprovements
{
    [HarmonyPatch(typeof(EquipmentDetailPanel))]
    public static class EquipmentDetailPanelPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("SetButtonInteractable")]
        public static void SetButtonInteractable_Postfix(EquipmentDetailPanel __instance, bool interactable)
        {
            EquipmentButton eButton = __instance.button.GetComponent<EquipmentButton>();
            if (eButton == null) eButton = __instance.gameObject.AddComponent<EquipmentButton>();

            eButton.InitButton(__instance, __instance.equipmentName, interactable);
            eButton.UpdateUI();
        }
    }

    [HarmonyPatch(typeof(EquipmentOverview))]
    public static class EquipmentOverviewPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void Start_Postfix(EquipmentOverview __instance)
        {
            UIImprovementsMod.Instance.EquipmentOverview = __instance;
        }
    }
}
#endif