using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {
    public class TendToPatientsSpecialization : Specialization {
        public TendToPatientsSpecialization(int priority, bool active, SubSpecialization[] subs)
            : base(ProfessionType.Doctor, "tendToPatients", priority, active, subs) {
        }

        public override IEnumerable<InteractionRestricted> GetInteractionRestricted(HumanAI human) {
            var restrictions = new List<InteractionRestriction>() { new InteractionRestrictionFaction() };

            foreach (var interaction in new Interaction[] { Interaction.TryFluTreatment, Interaction.GiveFood, Interaction.GiveHealingPotion, Interaction.BandageWounds, Interaction.SplintArm, Interaction.SplintLeg }) {
                yield return new InteractionRestricted(interaction, restrictions, 50);
            }
        }
    }
}
