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
    [HarmonyPatch(typeof(TeamPanel), "AddHuman")]
    public static class TeamPanelPatch 
    {
        public static void Prefix(TeamPanel __instance, HumanAI human, RenderTexture testRenderTexture) 
        {
            PortraitPanel template = __instance.template;

            if (template.background.transform.Find("namePanelWrapper") == null) 
            {
                HorizontalLayoutGroup hGroup = template.background.gameObject.AddComponent<HorizontalLayoutGroup>();
                hGroup.padding = new RectOffset(54, 0, 0, 0);
                hGroup.childForceExpandHeight = hGroup.childForceExpandWidth = false;
                hGroup.childControlWidth = true;
                hGroup.childControlHeight = true;
                hGroup.childAlignment = TextAnchor.MiddleLeft;
                hGroup.spacing = 5;

                GameObject namePanelWrapper = new GameObject("namePanelWrapper", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
                namePanelWrapper.transform.SetParent(template.background.transform);

                LayoutElement wLe = namePanelWrapper.GetComponent<LayoutElement>();
                wLe.minWidth = 105;

                RectTransform rect = namePanelWrapper.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(105, 60);

                VerticalLayoutGroup vGroup = namePanelWrapper.GetComponent<VerticalLayoutGroup>();
                vGroup.padding = new RectOffset(0, 0, 10, 5);
                vGroup.childForceExpandWidth = true;
                vGroup.childControlWidth = vGroup.childControlHeight = vGroup.childForceExpandHeight = true;

                template.nameText.transform.SetParent(namePanelWrapper.transform);
                template.transform.Find("Panel").SetParent(namePanelWrapper.transform);

                RectTransform bgRect = template.background.gameObject.GetComponent<RectTransform>();
                bgRect.sizeDelta = new Vector2(bgRect.sizeDelta.x + 70, bgRect.sizeDelta.y);

                template.nameText.resizeTextForBestFit = true;

                //Statisfaction points object
                {
                    GameObject obj = new GameObject("SatisfactionPoints", typeof(CanvasRenderer), typeof(RectTransform), typeof(SatisfactionPointsText));
                    SatisfactionPointsText  spText = obj.GetComponent<SatisfactionPointsText>();
                    spText.Init(template.nameText.gameObject);
                    obj.transform.SetParent(namePanelWrapper.transform.Find("Panel").transform);

                    RectTransform objRect = obj.GetComponent<RectTransform>();
                    objRect.anchorMin = new Vector2(0, 1);
                    objRect.anchorMax = new Vector2(0, 1);
                    objRect.pivot = new Vector2(0, 0.5f);
                    objRect.sizeDelta = new Vector2(24, 24);
                }

                //Current task in portrait
                {
                    GameObject obj = new GameObject("PortraitCurrentTaskPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TooltipHoverable), typeof(Image), typeof(LayoutElement), typeof(PortraitCurrentTask));
                    PortraitCurrentTask ct = obj.GetComponent<PortraitCurrentTask>();
                    ct.Init();
                    obj.transform.SetParent(template.background.transform);

                    LayoutElement le = obj.GetComponent<LayoutElement>();
                    le.minHeight = 40;
                    le.minWidth = 40;
                }
            }
        }
    }
}
#endif