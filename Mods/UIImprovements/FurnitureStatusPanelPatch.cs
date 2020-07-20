using HarmonyLib;
using UnityEngine;

namespace WitchyMods.UIImprovements
{
    [HarmonyPatch(typeof(FurnitureStatusPanel), "SetSelectedFurniture")]
    public class FurnitureStatusPanel_SetSelectedFurniturePatch
    {
        public static void Postfix(FurnitureStatusPanel __instance, Furniture furniture)
        {
            UIImprovementsMod.Instance.SelectedFurniture = furniture;

            if (furniture != null)
            {
                bool marked = (bool)AccessTools.Field(typeof(Furniture), "isMarkedForDeconstruction").GetValue(furniture);
                __instance.destroyFurnitureButton.image.color = (marked ? Color.black : Color.white);
            }
        }
    }
}
