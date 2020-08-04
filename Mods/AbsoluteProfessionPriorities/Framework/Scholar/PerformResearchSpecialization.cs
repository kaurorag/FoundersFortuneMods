using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {
    public class PerformResearchSpecialization : Specialization {
        public PerformResearchSpecialization(int priority, bool active, SubSpecialization[] subs)
            : base(ProfessionType.Scholar, "performResearch", priority, active, subs) {
        }

        public override IEnumerable<InteractionRestricted> GetInteractionRestricted(HumanAI human) {
            List<string> list = new List<string>();

            foreach(var sub in this.GetOrderedSubSpecializations().Select(x => x.Name)) {
                switch (sub) {
                    case "Scholar_PerformResearch_BookStand": list.Add("analyzeCrystal"); break;
                    case "Scholar_PerformResearch_ScrollStand": list.Add("analyzeScroll"); break;
                }
            }

            if (this.AutoManageSubSpecializations) {
                yield return new InteractionRestricted(Interaction.Produce, new InteractionRestrictionNameKey(list.ToArray()));
            } else {
                foreach(var filter in list) {
                    yield return new InteractionRestricted(Interaction.Produce, new InteractionRestrictionNameKey(filter));
                }
            }
        }
    }
}
