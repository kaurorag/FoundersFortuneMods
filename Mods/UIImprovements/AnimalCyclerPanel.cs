using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WitchyMods.UIImprovements {
    public enum AnimalType {
        Cow,
        Pig,
        Sheep
    }

    public class AnimalCyclerPanel : MonoBehaviour {

        public GameObject AnimalButtonTemplate;

#if !MODKIT
        private List<AnimalButton> animalButtons = new List<AnimalButton>();

        public void Init() {
            foreach(AnimalType animal in Enum.GetValues(typeof(AnimalType))) {
                GameObject obj = GameObject.Instantiate(this.AnimalButtonTemplate, this.transform);
                AnimalButton btn = obj.GetComponent<AnimalButton>();

                btn.AnimalType = animal;
                btn.AnimalIconImage.sprite = ModHandler.mods.sprites[$"animalButtonIcon_{animal}"];
                btn.UpdateUI();
                animalButtons.Add(btn);
            }
        }

        public void UpdateUI(HumanType humanType) {
            AnimalButton btn = this.animalButtons.FirstOrDefault(x => x.HumanType == humanType);
            if(btn != null) {
                btn.UpdateUI();
            }
        }
#endif
    }
}
