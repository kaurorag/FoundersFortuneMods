using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {
    public class CareForAnimalsSpecialization : Specialization {
        public CareForAnimalsSpecialization( int priority, bool active, SubSpecialization[] subs)
            : base(ProfessionType.Farmer, "careForAnimals", priority, active, subs) {
        }

        public override IEnumerable<InteractionRestricted> GetInteractionRestricted(HumanAI human) {
            List<Interaction> interactions = new List<Interaction>();

            foreach (var sub in this.GetOrderedSubSpecializations()) {
                switch (sub.Name) {
                    case "Farmer_CareForAnimals_Butcher": interactions.Add(Interaction.Butcher); break;
                    case "Farmer_CareForAnimals_Shear": interactions.Add(Interaction.Shear); break;
                    case "Farmer_CareForAnimals_Milk": interactions.Add(Interaction.Milk); break;
                    case "Farmer_CareForAnimals_Tame": interactions.Add(Interaction.Tame); break;
                }
            }

            if (this.AutoManageSubSpecializations) {
                if (interactions.Contains(Interaction.Tame)) {
                    interactions.Remove(Interaction.Tame);
                    interactions.Insert(0, Interaction.Tame);
                }
            }

            foreach(var interaction in interactions) {
                if (interaction == Interaction.Butcher)
                    yield return new InteractionRestricted(Interaction.Butcher, new InteractionRestrictionAutoButcher());
                else
                    yield return new InteractionRestricted(interaction, 50);
            }
        }
    }
}
