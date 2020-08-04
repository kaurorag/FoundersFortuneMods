using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {
    public class HarvestApplesSpecialization : Specialization {
        public HarvestApplesSpecialization(int priority, bool active, SubSpecialization[] subs)
            : base(ProfessionType.Farmer, "harvestApples", priority, active, subs) {
        }

        public override IEnumerable<InteractionRestricted> GetInteractionRestricted(HumanAI human) {
            yield return new InteractionRestricted(Interaction.GatherResource, new List<InteractionRestriction>() {
                new InteractionRestrictionResource(Resource.Apple),
                new InteractionRestrictionDesignation(Designation.GatherPlant)
            });
        }
    }
}
