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
    public class SatisfactionPointsText : MonoBehaviour
    {
        public Text Text = null;
        public TooltipHoverable Tooltip = null;
        public Image Image = null;

        private HumanAI _human = null;

        private String _LastHumanName = null;
        private int _LastSatisfaction = -1;

        public void Init(GameObject labelTemplate)
        {
            GameObject obj = this.gameObject;

            HorizontalLayoutGroup hlg = obj.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = hlg.childForceExpandWidth = true;
            hlg.childControlHeight = hlg.childForceExpandHeight = true;
            hlg.padding.left = -1;
            hlg.padding.top = -1;

            this.Image = obj.AddComponent<Image>();
            this.Image.sprite = ModHandler.mods.sprites["SatisfactionIcon"];
            this.Image.type = Image.Type.Simple;

            GameObject textObj = GameObject.Instantiate(labelTemplate, obj.transform);

            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1, -1);

            this.Text = textObj.GetComponent<Text>();
            this.Text.text = "0";
            this.Text.alignment = TextAnchor.MiddleCenter;
            this.Text.fontSize = 14;
            this.Text.horizontalOverflow = HorizontalWrapMode.Overflow;
            this.Text.verticalOverflow = VerticalWrapMode.Overflow;
            this.Text.alignByGeometry = true;

            this.Tooltip = obj.AddComponent<TooltipHoverable>();
            this.Tooltip.titleCode = "witchy_UIImprovements_satisfactionTooltip_Title";
        }

        private void Update()
        {
            if (_LastSatisfaction != _human.wishManager.lifetimeSatisfactionPoints ||
                _LastHumanName != _human.GetFullName())
            {
                _LastSatisfaction = _human.wishManager.lifetimeSatisfactionPoints;
                _LastHumanName = _human.GetFullName();

                this.UpdateUI();
            }
        }

        public void SetHuman(HumanAI human)
        {
            _human = human;
            this.UpdateUI();
        }

        private void UpdateUI()
        {
            int p = _human.wishManager.lifetimeSatisfactionPoints;

            this.Text.text = p.ToString();

            if (p == 0)
            {
                this.Image.color = new Color(0.783f, 0.303f, 0.303f);
            }
            else
            {
                bool enoughToAddRemove = false;

                if (p != 1)
                {
                    bool hasTrait = false;
                    int cost = 0;
                    int value = 0;

                    foreach (TraitType trait in Enum.GetValues(typeof(TraitType)))
                    {
                        hasTrait = _human.traitSet.HasTrait(trait);
                        cost = TraitSet.GetTraitCost(trait);
                        value = TraitSet.GetTraitPoints(trait);

                        if ((hasTrait && value >= 0 && p >= cost) ||  //If we already have it but it's a "negative" trait and we have enough points to remove it
                            (!hasTrait && value <= 0 && p >= cost))     //If we don't have it but it's a positive trait and we have enough points to add it
                        {
                            enoughToAddRemove = true;
                            break;
                        }
                    }
                }

                this.Image.color = enoughToAddRemove ? new Color(0.302f, 0.784f, 0.392f) : new Color(0.800f, 0.800f, 0f);
            }

            this.Tooltip.Init(Localization.GetText("witchy_UIImprovements_satisfactionTooltip_Title"),
                String.Format(Localization.GetText("witchy_UIImprovements_satisfactionTooltip_Desc"), _human.firstName, p));
        }
    }
}
