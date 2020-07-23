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
    public class PortraitCurrentTask : MonoBehaviour
    {
        private HumanAI Human = null;

        public TooltipHoverable ToolTip = null;

        private InteractionInfo _LastInteraction = null;

        private const float SIZEDIFF = -13f;
        private const float FRAMEOFFSET = 9;

        public Image backgroundImage = null;
        public Image iconImage = null;
        public Image frameImage = null;

        private static Color emptyColor = new Color(0, 0, 0, 0);
        private static Color backgroundColor = new Color(0.286f, 0.259f, 0.212f, 1);

        public void Init()
        {
            Sprite smallBorders = Resources.FindObjectsOfTypeAll<Sprite>().First(x => x.name == "smallBorder");

            backgroundImage = this.GetComponent<Image>();
            backgroundImage.color = backgroundColor;

            GameObject iconObj = new GameObject("icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            iconObj.transform.SetParent(this.transform);
            iconImage = iconObj.GetComponent<Image>();
            iconImage.raycastTarget = false;
            iconImage.type = Image.Type.Simple;

            GameObject frameObj = new GameObject("frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            frameObj.transform.SetParent(this.transform);
            frameImage = frameObj.GetComponent<Image>();
            frameImage.sprite = smallBorders;
            frameImage.raycastTarget = false;

            RectTransform rectIcon = iconObj.GetComponent<RectTransform>();
            RectTransform rectFrame = frameObj.GetComponent<RectTransform>();

            rectFrame.anchorMin = rectIcon.anchorMin = new Vector2(0, 0);
            rectFrame.anchorMax = rectIcon.anchorMax = new Vector2(1, 1);
            rectFrame.pivot = rectIcon.pivot = new Vector2(1, 1);
            rectFrame.sizeDelta = new Vector2(FRAMEOFFSET, FRAMEOFFSET);
            rectFrame.anchoredPosition = new Vector2(FRAMEOFFSET / 2f, FRAMEOFFSET / 2f);

            rectIcon.sizeDelta = new Vector2(SIZEDIFF, SIZEDIFF);
            rectIcon.anchoredPosition = new Vector2(SIZEDIFF / 2f, SIZEDIFF / 2f);

            this.ToolTip = this.GetComponent<TooltipHoverable>();

            UpdateInteraction(null);
        }

        public void SetHuman(HumanAI human)
        {
            this.Human = human;

            if (this.ToolTip != null)
                UpdateInteraction(this.Human.GetCurrentInteractionInfo());
        }

        private void Update()
        {
            InteractionInfo current = this.Human.GetCurrentInteractionInfo();

            if (current != _LastInteraction)
                this.UpdateInteraction(current);
        }

        public void UpdateInteraction(InteractionInfo current)
        {
            _LastInteraction = current;

            this.ToolTip.enabled = current != null;

            if (current != null)
            {
                Sprite sprite = current.interactable.GetInteractionIcon(current.interaction);
                iconImage.sprite = sprite;
                iconImage.color = (sprite == null ? emptyColor : Color.white);
                backgroundImage.color = backgroundColor;
                frameImage.color = Color.white;

                Pair<string, string>[] substitutions = new Pair<string, string>[] { new Pair<string, string>("keyBinding", Helper.GetKeyCodeName(SettingsHandler.GetCurrentSettings().keybindings[Keybinding.MultipleOrders])) };

                String tooltipTitle = current.interactable.GetInteractionName(current.interaction, this.Human);
                String tooltipText = Helper.SubstituteCode("interactionChainIconDesc", substitutions);

                this.ToolTip.Init(tooltipTitle, tooltipText);
            }
            else
            {
                iconImage.color = emptyColor;
                backgroundImage.color = emptyColor;
                frameImage.color = emptyColor;
            }
        }
    }
}
#endif