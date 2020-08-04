using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {
    public class GrowTreesSpecialization : Specialization {
        public GrowTreesSpecialization(int priority, bool active, SubSpecialization[] subs)
            : base(ProfessionType.Forester, "growTrees", priority, active, subs) {
        }

        public override IEnumerable<InteractionRestricted> GetInteractionRestricted(HumanAI human) {
            List<string> filters = new List<string>();

            foreach(var sub in this.GetOrderedSubSpecializations().Select(x => x.Name)) {
                switch (sub) {
                    case "Forester_GrowTrees_PineTrees": filters.Add("growingTrees"); break;
                    case "Forester_GrowTrees_Apples": filters.Add("growingAppleTrees"); break;
                    case "Forester_GrowTrees_Cotton": filters.Add("growingCotton"); break;
                }
            }

            if (this.AutoManageSubSpecializations) {
                yield return new InteractionRestricted(SowAndCareForTrees, new List<InteractionRestriction>() {
                    new InteractionRestrictionModule(typeof(GrowingSpotModule), true), 
                    new InteractionRestrictionGrowingSpotFilter(filters.ToArray())
                });
            } else {
                foreach(var filter in filters) {
                    yield return new InteractionRestricted(SowAndCareForTrees, new List<InteractionRestriction>() {
                    new InteractionRestrictionModule(typeof(GrowingSpotModule), true),
                    new InteractionRestrictionGrowingSpotFilter(filter)
                });
                }
            }
        }
    }
}
