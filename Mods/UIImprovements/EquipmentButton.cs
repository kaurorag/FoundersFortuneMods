using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;

namespace WitchyMods.UIImprovements {
    public class EquipmentButton : MonoBehaviour {
        private String EquipmentName = null;

        private bool OldCanEquip = true;

        private HumanAI OldOwner = null;

        private static ColorBlock colorBlock = new ColorBlock() {
            colorMultiplier = 1,
            normalColor = Color.red,
            pressedColor = new Color(0.75f, 0.15f, 0.15f, 1f),
            highlightedColor = Color.red,
            selectedColor = Color.red,
            disabledColor = Color.white,
            fadeDuration = 0.1f,
        };

        public void InitButton(EquipmentDetailPanel detailPanel, String equipmentName, bool interactable) {
            Button button = this.GetComponent<Button>();
            button.colors = colorBlock;

            EquipmentName = equipmentName;

            //reset to the default sprite
            button.transition = Selectable.Transition.SpriteSwap;
            button.image.sprite = ModHandler.mods.sprites["dropdown2"];
            button.image.overrideSprite = interactable ? ModHandler.mods.sprites["dropdown2"] : button.spriteState.disabledSprite;

            OldCanEquip = true;
        }

        private void Update() {
            UpdateUI();
        }

        public void UpdateUI() {

            Button button = this.GetComponent<Button>();
 
            if (!button.interactable || String.IsNullOrEmpty(EquipmentName)) return;

            bool canEquip = true;

            Furniture furniture = UIImprovementsMod.Instance.SelectedFurniture;

            if (furniture != null) {
                OwnershipModule ownershipModule = furniture.GetModule<OwnershipModule>();
                if (ownershipModule == null && OldOwner != null)
                    OldOwner = null;
                else if (ownershipModule.owner != OldOwner) {
                    EquipmentDescription desc = Equipment.GetDescriptions()[EquipmentName];
                    OldOwner = ownershipModule.owner;
                }

                if (OldOwner != null)
                    canEquip = CanOwnerEquip();
                else
                    canEquip = OldCanEquip;
            }

            if (canEquip != OldCanEquip) {
                OldCanEquip = canEquip;

                if (canEquip) {
                    button.transition = Selectable.Transition.SpriteSwap;
                    button.image.sprite = ModHandler.mods.sprites["dropdown2"];
                    button.image.overrideSprite = ModHandler.mods.sprites["dropdown2"];
                } else {
                    button.transition = Selectable.Transition.ColorTint;
                    button.image.sprite = button.spriteState.disabledSprite;
                    button.image.overrideSprite = button.spriteState.disabledSprite;
                }
            }
        }

        private bool CanOwnerEquip() {
            EquipmentDescription desc = Equipment.GetDescriptions()[EquipmentName];

            if (!String.IsNullOrEmpty(desc.requiredSkillName))
                return OldOwner.professionManager.HasSkill(desc.requiredSkillName);

            return true;
        }
    }
}
