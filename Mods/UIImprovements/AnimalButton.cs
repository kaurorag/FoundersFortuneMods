using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System.Collections;

namespace WitchyMods.UIImprovements {

    [RequireComponent(typeof(Button))]
    public class AnimalButton : MonoBehaviour {

        public AnimalType AnimalType = AnimalType.Cow;

        public Text CountText;

        public Image AnimalIconImage;
#if !MODKIT

        public HumanType HumanType { get { return (HumanType)Enum.Parse(typeof(HumanType), this.AnimalType.ToString()); } }

        private List<HumanAI> animals = null;

        private void Awake() {
            this.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(OnClick));
        }

        public void UpdateUI() {
            animals = WorldScripts.Instance.humanManager.colonyFaction.GetLivingHumans().Where(x => x.humanType == this.HumanType).ToList();
            CountText.text = animals.Count.ToString();
            this.GetComponent<Button>().interactable = animals.Count != 0;

            int limit = WorldScripts.Instance.humanManager.colonyFaction.GetAnimalSpeciesManager(this.HumanType).breedingLimit;
            this.CountText.color = animals.Count > limit ? new Color(0.726f, 0.318f, 0.318f) : Color.white;
        }

        public void OnClick() {
            if (animals == null || animals.Count == 0) return;

            HumanAI activeHuman = WorldScripts.Instance.humanManager.GetActiveHuman();
            int index = activeHuman != null ? animals.IndexOf(activeHuman) : -1;
            index = (index + 1) % animals.Count;
            WorldScripts.Instance.humanManager.SwitchActiveHuman(animals[index], true, true);
#endif
        }
}

#if !MODKIT
    [HarmonyPatch()]
    public static class PatchesForAnimalButton {

        [HarmonyPatch(typeof(HumanManager), "SpawnHuman")]
        [HarmonyPatch(new Type[] { typeof(HumanConfiguration), typeof(Vector3), typeof(Faction), typeof(bool), typeof(HumanAI.Serializable) })]
        [HarmonyPostfix]
        public static void SpawnHuman_Postfix(HumanManager __instance, ref HumanAI __result, HumanConfiguration config, Vector3 position, Faction faction, bool alive = true, HumanAI.Serializable serial = null) {
            if (UIImprovementsMod.Instance == null || UIImprovementsMod.Instance.AnimalCyclerPanel == null) return;

            if (__result != null && __result.faction != null && !__result.IsHumanoid() && __result.animal != null && __result.faction is ColonyFaction) {
                HumanType type = __result.humanType;
                UIImprovementsMod.Instance.AnimalCyclerPanel.UpdateUI(type);
            }
        }

        [HarmonyPatch(typeof(HumanManager), "MoveHumanToFaction")]
        [HarmonyPostfix]
        public static void MoveHumanToFaction_Postfix(HumanAI human, Faction factionNext) {
            if (UIImprovementsMod.Instance == null || UIImprovementsMod.Instance.AnimalCyclerPanel == null) return;

            if (human.IsValid() && !human.IsHumanoid() && human.animal != null && human.faction is ColonyFaction) {
                UIImprovementsMod.Instance.AnimalCyclerPanel.UpdateUI(human.humanType);
            }
        }

        [HarmonyPatch(typeof(HumanAI), "Die")]
        [HarmonyPostfix]
        public static void Die_Postfix(HumanAI __instance, bool oldAge, bool force = false) {
            if (UIImprovementsMod.Instance == null || UIImprovementsMod.Instance.AnimalCyclerPanel == null) return;

            if (!__instance.IsHumanoid() && __instance.animal != null && __instance.faction is ColonyFaction) {
                UIImprovementsMod.Instance.AnimalCyclerPanel.UpdateUI(__instance.humanType);
            }
        }
    }
}
#endif