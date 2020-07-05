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
            //Satisfaction points object
            SatisfactionPointsText spText = __instance.gameObject.GetComponentInChildren<SatisfactionPointsText>(true);
            if (spText == null)
            {
                RectTransform bgRect = __instance.background.gameObject.GetComponent<RectTransform>();
                bgRect.sizeDelta = new Vector2(bgRect.sizeDelta.x + 24, bgRect.sizeDelta.y);

                GameObject obj = new GameObject();
                obj.AddComponent<CanvasRenderer>();
                obj.AddComponent<RectTransform>();
                spText = obj.AddComponent<SatisfactionPointsText>();
                spText.Init(__instance.nameText.gameObject);
                obj.transform.SetParent(__instance.transform.Find("Panel").transform);

                RectTransform rect = obj.GetComponent<RectTransform>();                
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 0.5f);
                rect.sizeDelta = new Vector2(24, 24);
                
            }

            spText.SetHuman(human);

            //Current task in portrait
            PortraitCurrentTask ct = __instance.gameObject.GetComponentInChildren<PortraitCurrentTask>();

            if (ct == null)
            {
                RectTransform bgRect = __instance.background.gameObject.GetComponent<RectTransform>();
                bgRect.sizeDelta = new Vector2(bgRect.sizeDelta.x + 24, bgRect.sizeDelta.y);

                GameObject obj = new GameObject("PortraitCurrentTaskPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TooltipHoverable), typeof(Image), typeof(Button));
                obj.transform.SetParent(__instance.transform.Find("background").transform);
                ct = obj.AddComponent<PortraitCurrentTask>();

                RectTransform rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1, 0);
                rect.sizeDelta = new Vector2(40, 40);
                rect.anchoredPosition = new Vector2(-15, 10);

                obj.SetActive(true);
            }

            ct.SetHuman(human);
        }
    }


}
