using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {
    public class BrewBeerSpecialization : Specialization {
        public BrewBeerSpecialization(int priority, bool active, SubSpecialization[] subs)
            : base(ProfessionType.Doctor, "brewBeer", priority, active, subs) {
        }

        public override IEnumerable<InteractionRestricted> GetInteractionRestricted(HumanAI human) {
            yield return new InteractionRestricted(Interaction.Produce, new InteractionRestrictionProduction(new List<string>() { "beer" }));
        }
    }
}
