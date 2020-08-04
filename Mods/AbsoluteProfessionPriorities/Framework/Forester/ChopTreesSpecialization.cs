using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {
    public class ChopTreesSpecialization : Specialization {
        public ChopTreesSpecialization(int priority, bool active, SubSpecialization[] subs)
            : base(ProfessionType.Forester, "chopTrees", priority, active, subs) {
        }

        private static List<string> SubsByAmount = new List<string> { "Forester_ChopTrees_BigTrees", "Forester_ChopTrees_SmallTrees", "Forester_ChopTrees_Stumps" };

        public override IEnumerable<InteractionRestricted> GetInteractionRestricted(HumanAI human) {
            List<string> activeSubs = this.GetOrderedSubSpecializations().Select(x => x.Name).ToList();

            if (this.AutoManageSubSpecializations) {
                bool lowOnWood = GetResourceRatio(Resource.Wood) < 0.20;
                if (lowOnWood)
                    activeSubs.Sort((x, y) => SubsByAmount.IndexOf(x).CompareTo(SubsByAmount.IndexOf(y)));
            }

            foreach (var sub in activeSubs) {
                switch (sub) {
                    case "Forester_ChopTrees_Stumps":
                        yield return new InteractionRestricted(Interaction.GatherResource, new List<InteractionRestriction>() {
                            new InteractionRestrictionResource(Resource.Wood),
                            new InteractionRestrictionDesignation(Designation.CutTree),
                            new InteractionRestrictionResourcePerRound(new int[]{0 })
                        });
                        break;
                    case "Forester_ChopTrees_BigTrees":
                        yield return new InteractionRestricted(Interaction.GatherResource, new List<InteractionRestriction>() {
                            new InteractionRestrictionResource(Resource.Wood),
                            new InteractionRestrictionDesignation(Designation.CutTree),
                            new InteractionRestrictionStockpile(Resource.Wood),
                            new InteractionRestrictionResourcePerRound(new int[]{AbsoluteProfessionPrioritiesMod.MaxTreeWoodAmount })
                        });
                        break;
                    case "Forester_ChopTrees_SmallTrees":
                        yield return new InteractionRestricted(Interaction.GatherResource, new List<InteractionRestriction>() {
                            new InteractionRestrictionResource(Resource.Wood),
                            new InteractionRestrictionDesignation(Designation.CutTree),
                            new InteractionRestrictionStockpile(Resource.Wood),
                            new InteractionRestrictionResourcePerRound(null, new int[]{AbsoluteProfessionPrioritiesMod.MaxTreeWoodAmount, 0 })
                        });
                        break;
                }
            }
        }
    }
}
