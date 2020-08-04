using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {
    public class MineIronSpecialization : Specialization {
        public MineIronSpecialization( int priority, bool active, SubSpecialization[] subs)
            : base(ProfessionType.Miner, "mineIron", priority, active, subs) {
        }

        public override IEnumerable<InteractionRestricted> GetInteractionRestricted(HumanAI human) {
            yield return new InteractionRestricted(Interaction.GatherResource, new List<InteractionRestriction>() {
                new InteractionRestrictionResource(Resource.IronOre),
                new InteractionRestrictionDesignation(Designation.Mine)
            });
        }
    }
}
