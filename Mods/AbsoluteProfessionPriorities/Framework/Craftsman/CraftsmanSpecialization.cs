using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {
    public class CraftsmanSpecialization : Specialization {
        public CraftsmanSpecialization(string name, int priority, bool active, SubSpecialization[] subs)
            : base(ProfessionType.Craftsman, name, priority, active, subs) {
        }

        public override IEnumerable<InteractionRestricted> GetInteractionRestricted(HumanAI human) {
            yield return new InteractionRestricted(Interaction.Produce,
                    new List<InteractionRestriction>() {
                        new InteractionRestrictionProfessionSpecialization(this.Name),
                        new InteractionRestrictionProfession(ProfessionType.Craftsman)
                    });
        }
    }
}
