using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {
    public class CookSpecialization : Specialization {
        public CookSpecialization( int priority, bool active, SubSpecialization[] subs)
            : base(ProfessionType.Farmer, "cook", priority, active, subs) {
        }

        public override IEnumerable<InteractionRestricted> GetInteractionRestricted(HumanAI human) {
            List<Resource> meals = new List<Resource>();

            foreach (var sub in this.GetOrderedSubSpecializations()) {
                Resource parentResource = Resource.None;

                switch (sub.Name) {
                    case "Farmer_Cook_CampireFire":
                        parentResource = Resource.SimpleMeal;
                        break;
                    case "Farmer_Cook_Kitchen":
                        parentResource = Resource.GoodMeal;
                        break;
                    case "Farmer_Cook_Bakery":
                        parentResource = Resource.BakedMeal;
                        break;
                    default:
                        yield break;
                }

                meals.AddRange(GameState.GetChildResources(parentResource, false));
            }

            if (this.AutoManageSubSpecializations) {
                Dictionary<Resource, double> ratios = meals.ToDictionary(x => x, x => GetResourceRatio(x));
                meals.Sort((x, y) => ratios[x].CompareTo(ratios[y]));
            }

            List<string> mealsToProduce = meals.Select(x => Char.ToLower(x.ToString()[0]) + x.ToString().Substring(1)).ToList();

            yield return new InteractionRestricted(Interaction.Produce,
                new List<InteractionRestriction>()
                {
                    new InteractionRestrictionProduction(mealsToProduce),
                    new InteractionRestrictionProfession(ProfessionType.Farmer)
                });
        }
    }
}
