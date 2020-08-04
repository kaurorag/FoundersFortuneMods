using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {
    public class ProduceMedicineSpecialization : Specialization {
        public ProduceMedicineSpecialization( int priority, bool active, SubSpecialization[] subs)
            : base(ProfessionType.Doctor, "produceMedicine", priority, active, subs) {
        }

        public override IEnumerable<InteractionRestricted> GetInteractionRestricted(HumanAI human) {
            List<String> orderedMeds = new List<string>();

            foreach (var sub in this.GetOrderedSubSpecializations()) {
                switch (sub.Name) {
                    case "Doctor_ProduceMedicine_FluVaccines": orderedMeds.Add("medicine"); break;
                    case "Doctor_ProduceMedicine_HealingPotions": orderedMeds.Add("healingPotion"); break;
                }
            }

            if (this.AutoManageSubSpecializations) {
                orderedMeds.Sort((x, y) => GetResourceRatio(GameState.GetResourceByString(x)).CompareTo(GetResourceRatio(GameState.GetResourceByString(y))));
            }

            foreach (var med in orderedMeds) {
                var intRes = new InteractionRestricted(Interaction.Produce, new InteractionRestrictionProduction(new List<string>() { med }));
                yield return intRes;
            }
        }
    }
}
