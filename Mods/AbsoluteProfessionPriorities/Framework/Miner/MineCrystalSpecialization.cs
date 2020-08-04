using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {
    public class MineCrystalSpecialization : Specialization {
        public MineCrystalSpecialization(ProfessionType profession, int priority, bool active, SubSpecialization[] subs)
            : base(profession, "mineCrystal", priority, active, subs) {
        }

        public override IEnumerable<InteractionRestricted> GetInteractionRestricted(HumanAI human) {
            yield return new InteractionRestricted(Interaction.GatherResource, new List<InteractionRestriction>() {
                new InteractionRestrictionResource(Resource.Crystal),
                new InteractionRestrictionDesignation(Designation.Mine)
            });
        }
    }
}
