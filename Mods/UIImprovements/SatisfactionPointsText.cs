#if !MODKIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace WitchyMods.UIImprovements {
    public class SatisfactionPointsText : MonoBehaviour {
        public Text Text = null;
        public TooltipHoverable Tooltip = null;
        public Image Image = null;

        private HumanAI _human = null;

        private String _LastHumanName = null;
        private int _LastSatisfaction = -1;

        public void Init(GameObject labelTemplate) {
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

        private void Update() {
            if (_LastSatisfaction != _human.wishManager.lifetimeSatisfactionPoints ||
                _LastHumanName != _human.GetFullName()) {
                _LastSatisfaction = _human.wishManager.lifetimeSatisfactionPoints;
                _LastHumanName = _human.GetFullName();

                this.UpdateUI();
            }
        }

        public void SetHuman(HumanAI human) {
            _human = human;
            this.UpdateUI();
        }

        private void UpdateUI() {
            int p = _human.wishManager.lifetimeSatisfactionPoints;

            this.Text.text = p.ToString();
            bool enoughForMajor = false;
            bool enoughForMinor = false;

            if (p != 0) {

                //If we already have it but it's a "negative" trait and we have enough points to remove it or
                //   we don't have it and it's a positive trait with a value higher than 1
                enoughForMajor = _human.traitSet.traits.Any(x => p >= TraitSet.GetTraitCost(x) && TraitSet.GetTraitPoints(x) > 0) ||
                    TraitSet.changableTraits.Any(x => !_human.traitSet.HasTrait(x) && TraitSet.TraitAllowed(x, _human.traitSet.traits)  && p >= TraitSet.GetTraitCost(x) && TraitSet.GetTraitPoints(x) < -1);

                //If we don't already have it and it's a positive trait
                if (!enoughForMajor)
                    enoughForMinor = TraitSet.changableTraits.Any(x => !_human.traitSet.HasTrait(x) && TraitSet.TraitAllowed(x, _human.traitSet.traits) && p >= TraitSet.GetTraitCost(x) && TraitSet.GetTraitPoints(x) < 0);
            }

            if (enoughForMajor)
                this.Image.color = new Color(0.302f, 0.784f, 0.392f); //green
            else if (enoughForMinor)
                this.Image.color = new Color(0.800f, 0.800f, 0f); //yellow
            else
                this.Image.color = new Color(0.783f, 0.303f, 0.303f); //red

            this.Tooltip.Init(Localization.GetText("witchy_UIImprovements_satisfactionTooltip_Title"),
                String.Format(Localization.GetText("witchy_UIImprovements_satisfactionTooltip_Desc"), _human.firstName, p));
        }
    }
}
#endif