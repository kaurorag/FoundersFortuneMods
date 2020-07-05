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
            eButton.InitButton(__instance, __instance.equipmentName, interactable);
            eButton.UpdateUI();
        }

        [HarmonyPostfix]
        [HarmonyPatch("SetEquipment")]
        public static void SetEquipment_Postfix(EquipmentDetailPanel __instance, string equipmentName)
        {
            __instance.button.gameObject.AddComponent<EquipmentButton>();

        }
    }

    [HarmonyPatch(typeof(EquipmentOverview))]
    public static class EquipmentOverviewPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void Start_Postfix(EquipmentOverview __instance)
        {
            EquipmentOverviewInfos.Instance.EquipmentOverview = __instance;
        }
    }
}
