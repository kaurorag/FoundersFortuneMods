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
    [HarmonyPatch(typeof(FurnitureStatusPanel), "SetSelectedFurniture")]
    public class FurnitureStatusPanel_SetSelectedFurniturePatch
    {
        public static void Postfix(FurnitureStatusPanel __instance, Furniture furniture)
        {
            EquipmentOverviewInfos.Instance.SelectedFurniture = furniture;

            if (furniture != null)
            {
                FurnitureStatusPanelHelper.UpdateDestroyButtonColor(__instance, furniture);

                if (furniture.IsBuilt())
                {
                    String customText = FurnitureStatusPanelHelper.GetCustomText(__instance, furniture);

                    if (customText != null)
                    {
                        __instance.customTextPanel.transform.GetChild(0).GetComponent<Text>().text = customText;
                        __instance.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 790f);
                        __instance.customTextPanel.SetActive(true);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(FurnitureStatusPanel), "Update")]
    public class FurnitureStatusPanel_UpdatePatch
    {
        public static void Postfix(FurnitureStatusPanel __instance)
        {
            Furniture furniture = (Furniture)AccessTools.Field(typeof(FurnitureStatusPanel), "furniture").GetValue(__instance);

            if (furniture != null)
            {
                String customText = FurnitureStatusPanelHelper.GetCustomText(__instance, furniture);

                if (customText != null)
                    __instance.customTextPanel.transform.GetChild(0).GetComponent<Text>().text = customText;
            }
        }
    }

    [HarmonyPatch(typeof(FurnitureStatusPanel), "DestroySelectedFurniture")]
    public class FurnitureStatusPanel_DestroySelectedFurniturePatch
    {
        public static void Postfix(FurnitureStatusPanel __instance)
        {
            Furniture furniture = (Furniture)AccessTools.Field(typeof(FurnitureStatusPanel), "furniture").GetValue(__instance);

            if (furniture != null)
                FurnitureStatusPanelHelper.UpdateDestroyButtonColor(__instance, furniture);
        }
    }

    public static class FurnitureStatusPanelHelper
    {
        public static void UpdateDestroyButtonColor(FurnitureStatusPanel panel, Furniture furniture)
        {
            bool marked = (bool)AccessTools.Field(typeof(Furniture), "isMarkedForDeconstruction").GetValue(furniture);
            panel.destroyFurnitureButton.image.color = (marked ? Color.black : Color.white);
        }

        public static String GetCustomText(FurnitureStatusPanel __instance, Furniture furniture)
        {
            String customText = null;

            SoilModule soil = furniture.GetModule<SoilModule>();
            ResourceModule res = furniture.GetModule<ResourceModule>();
            EnemyHomeModule enemyHome = furniture.GetModule<EnemyHomeModule>();
            GrowingSpotModule spot = furniture.GetModule<GrowingSpotModule>();

            if (soil != null)
                customText = GetSoilText(soil);
            else if (res != null)
                customText = GetResourceText(res);
            else if (enemyHome != null)
                customText = GetEnemyHomeModuleText(enemyHome);
            else if (spot != null)
                customText = GetGrowingSpotModuleText(spot);

            return customText;
        }

        private static String GetSoilText(SoilModule soil)
        {
            SoilModule.PlantStage stage = (SoilModule.PlantStage)AccessTools.Field(typeof(SoilModule), "plantStage").GetValue(soil);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format(Localization.GetText("witchy_UIImprovements_Soil_Stage"), Localization.GetText("witchy_UIImprovements_Soil_Stage_" + stage.ToString())));

            if (stage == SoilModule.PlantStage.Growing)
            {
                float timeLeftToGrow = (float)AccessTools.Field(typeof(SoilModule), "timeLeftToGrow").GetValue(soil);
                float timeToGrowThisTime = (float)AccessTools.Field(typeof(SoilModule), "timeToGrowThisTime").GetValue(soil);

                String strTime = String.Format("{0:00}:{1:00}", (int)Math.Floor(timeLeftToGrow / 60f), (int)Math.Floor(timeLeftToGrow % 60f));
                String prc = timeToGrowThisTime == 0 ? "??" : (100 - (int)Math.Floor(timeLeftToGrow * 100.0 / timeToGrowThisTime)).ToString();

                sb.AppendLine(String.Format(Localization.GetText("witchy_UIImprovements_Soil_Growth"), strTime, prc));

                bool watered1 = (bool)AccessTools.Field(typeof(SoilModule), "watered1").GetValue(soil);
                bool watered2 = (bool)AccessTools.Field(typeof(SoilModule), "watered2").GetValue(soil);

                sb.AppendLine(String.Format(Localization.GetText("witchy_UIImprovements_Soil_Watered"), ((watered1 ? 1 : 0) + (watered2 ? 1 : 0))));
            }

            return sb.ToString();
        }

        private static String GetResourceText(ResourceModule res)
        {
            StringBuilder sb = new StringBuilder();

            if (res.isDepleted)
            {
                sb.AppendLine(Localization.GetText("witchy_UIImprovements_Resource_Depleted"));

                if (res.replenishDuration > 0)
                {
                    float remainingReplenishDuration = (float)AccessTools.Field(typeof(ResourceModule), "remainingReplenishDuration").GetValue(res);

                    sb.AppendLine(String.Format(Localization.GetText("witchy_UIImprovements_Resource_ReplenishTime"),
                        remainingReplenishDuration / 60f,
                        remainingReplenishDuration % 60f));
                }
            }
            else
            {
                sb.AppendLine(String.Format(Localization.GetText("witchy_UIImprovements_Resource_Rounds"), res.remainingRounds));
                sb.AppendLine(String.Format(Localization.GetText("witchy_UIImprovements_Resource_ResPerRound"), res.resourcesPerRound));
                sb.AppendLine(String.Format(Localization.GetText("witchy_UIImprovements_Resource_RemainingRes"), res.resourcesPerRound * res.remainingRounds));
            }

            return sb.ToString();
        }

        private static String GetEnemyHomeModuleText(EnemyHomeModule e)
        {
            if (e.parent.faction == null) return null;

            StringBuilder sb = new StringBuilder();

            GoblinFaction faction = (GoblinFaction)e.parent.faction;
            sb.AppendLine(String.Format(Localization.GetText("witchy_UIImprovements_Enemy_Faction"), faction.name));
            sb.AppendLine(String.Format("Relationship points: {0}", faction.GetRelationshipToColony()));
            sb.AppendLine(faction.GetRelationshipToColonyTitle());

            return sb.ToString();
        }

        private static String GetGrowingSpotModuleText(GrowingSpotModule spot)
        {
            StringBuilder sb = new StringBuilder();

            int currentPhase = (int)AccessTools.Field(typeof(GrowingSpotModule), "currentPhase").GetValue(spot) + 1;
            float timeUntilNextPhase = (float)AccessTools.Field(typeof(GrowingSpotModule), "timeUntilNextPhase").GetValue(spot);
            float timeUntilNextCareInteraction = (float)AccessTools.Field(typeof(GrowingSpotModule), "timeUntilNextCareInteraction").GetValue(spot);

            timeUntilNextPhase = Math.Max(timeUntilNextPhase, 0f);
            timeUntilNextCareInteraction = Math.Max(timeUntilNextCareInteraction, 0f);

            sb.AppendLine(String.Format(Localization.GetText("witchy_UIImprovements_GrowingSpot_Phases"), currentPhase, spot.phases.Length));
            sb.AppendLine(String.Format(Localization.GetText("witchy_UIImprovements_GrowingSpot_NextPhase"),
               (int)Math.Floor( timeUntilNextPhase / 60f), (int)Math.Floor(timeUntilNextPhase % 60f)));
            sb.AppendLine(String.Format(Localization.GetText("witchy_UIImprovements_GrowingSpot_NextCare"),
               (int)Math.Floor(timeUntilNextCareInteraction / 60f), (int)Math.Floor(timeUntilNextCareInteraction % 60f)));

            return sb.ToString();
        }
    }
}
