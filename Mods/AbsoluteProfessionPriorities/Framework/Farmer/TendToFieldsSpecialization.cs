using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {
    public class TendToFieldsSpecialization : Specialization {

        public TendToFieldsSpecialization( int priority, bool active, SubSpecialization[] subs) 
            : base(ProfessionType.Farmer, "tendToFields", priority, active, subs) {
        }

        private static Resource[] FieldsResources = new Resource[] { Resource.HealingPlantCultivated, Resource.Strawberry, Resource.Tomato, Resource.Pumpkin, Resource.Potato, Resource.Wheat };

        public override IEnumerable<InteractionRestricted> GetInteractionRestricted(HumanAI human) {
            List<Resource> sortedFieldRes = new List<Resource>();

            foreach (var sub in this.GetOrderedSubSpecializations()) {
                Resource res = (Resource)Enum.Parse(typeof(Resource), sub.Name.Split('_').Last());
                sortedFieldRes.Add(res);
            }

            if (this.AutoManageSubSpecializations) {
                Dictionary<Resource, double> ratios = FieldsResources.ToDictionary(x => x, x => GetResourceRatio(x));
                sortedFieldRes.Sort((x, y) => ratios[x].CompareTo(ratios[y]));
            }

            bool lowOnFood = (GameState.Instance.GetResource(Resource.FoodRaw) + GameState.Instance.GetResource(Resource.FoodCooked)) < (5 * WorldScripts.Instance.humanManager.GetColonistCount());

            if (lowOnFood) {
                List<Resource> edibleResources = sortedFieldRes.Where(x => x != Resource.Wheat && x != Resource.HealingPlantCultivated).ToList();

                if (edibleResources.Any()) {
                    yield return new InteractionRestricted(Interaction.GatherResource, new InteractionRestrictionResource(edibleResources));
                }
            }

            foreach (var res in sortedFieldRes) {
                yield return new InteractionRestricted(Specialization.TendToFieldsInteraction,
                    new List<InteractionRestriction>()
                    {
                        new InteractionRestrictionModule(typeof(SoilModule), true),
                        new InteractionRestrictionResource(res)
                    });
            }
        }
    }
}
