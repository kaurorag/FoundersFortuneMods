using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities {
    [HarmonyPatch(typeof(WorkInteractionController), "GetInteractionProposal")]
    public static class WorkInteractionControllerPatch {
        /// <summary>
        /// Overwrites completely the method 'GetInteractionProposal' from the WorkInteractionController
        /// </summary>
        /// <param name="__instance">Instance of the WorkInteractionController</param>
        /// <param name="human">Original HumanAI argument</param>
        /// <param name="__result">The result of the method</param>
        /// <returns>False to prevent the original method from executing</returns>
        public static bool Prefix(WorkInteractionController __instance, HumanAI human, ref InteractionInfo __result) {
            __result = null;

            //This bit is copy/pasted from the original method.  It checks if the 'human' is a colonist and can actually work
            if (human.IsLockedUp() || !human.WillWorkAutomatically() || human.IsLazy() || human.faction.GetFactionType() != FactionType.Colony || human.controlMode == ControlMode.Combat)
                return false;

            //Init the human in case it's new
            AbsoluteProfessionPrioritiesMod.Instance.InitHuman(human.GetID());

            //Creates a dictionary of all of the colonist's active professions, grouped by priorities (number of stars)
            Dictionary<int, List<Profession>> profs = human.professionManager.professions.Values.Where(
                x => x.priority > 0).GroupBy(x => x.priority).ToDictionary(
                x => x.Key, x => x.ToList());

            //For each priority (number of stars), starting by the highest to the lowest
            foreach (var professionPriority in profs.Keys.OrderByDescending(x => x)) {
                foreach (var profession in profs[professionPriority]) {
                    if (profession.type == ProfessionType.Soldier ||
                        profession.type == ProfessionType.NoJob)
                        continue;

                    if (!AbsoluteProfessionPrioritiesMod.Instance.specializationPriorities[human.GetID()].ContainsKey(profession.type))
                        continue;

                    __result = GetNextInteraction(__instance, human, profession);
                    if (__result != null) {
                        return false;
                    }
                }
            }

            return false;
        }

        private static InteractionInfo GetNextInteraction(WorkInteractionController __instance, HumanAI human, Profession profession) {
            //Allows us to call the private method CheckInteraction later on
            MethodInfo checkInteractionMethod = AccessTools.Method(typeof(WorkInteractionController), "CheckInteraction", new Type[] { typeof(InteractionRestricted), typeof(HumanAI) });

            //Get all the specializations for this human for this profession
            List<string> specs = AbsoluteProfessionPrioritiesMod.Instance.specializationPriorities[human.GetID()][profession.type];

            if (specs == null || specs.Count == 0) return null;

            IEnumerable<InteractionRestricted> possibilities = null;

            switch (profession.type) {
                case ProfessionType.Farmer: possibilities = GetFarmerInteractions(profession, specs); break;
                case ProfessionType.Forester: possibilities = GetForesterInteractions(profession, specs); break;
                case ProfessionType.Miner: possibilities = GetMinerInteractions(profession, specs); break;
                case ProfessionType.Craftsman: possibilities = GetCraftsmanInteractions(profession, specs); break;
                case ProfessionType.Doctor: possibilities = GetDoctorInteractions(profession, specs); break;
                case ProfessionType.Scholar: possibilities = GetScholarInteractions(profession, specs); break;
                case ProfessionType.Builder: possibilities = GetBuilderInteractions(profession, specs); break;
                default:
                    throw new NotImplementedException("The profession type is unknown to this mod and probably new.  " +
                        "This mod is thus incompatible with the current version of the game.  Please disable it.");
            }

            foreach (var possibility in possibilities) {
                InteractionInfo info = (InteractionInfo)checkInteractionMethod.Invoke(__instance, new object[] { possibility, human });
                if (info != null) return info;
            }

            return null;
        }

        private static Resource[] FieldsResources = new Resource[] { Resource.Tomato, Resource.Pumpkin, Resource.Potato, Resource.Strawberry, Resource.Wheat, Resource.HealingPlantCultivated };
        private static String[] CookedFood = new string[] { "bakedApple", "bakedPotato", "bakedTomato", "pumpkinStew", "potatoSoup", "fruitSalad", "bread", "strawberryCake", "appleStrudel" };
        private static String[] Meds = new string[] { "healingPotion", "medicine" };

        private static IEnumerable<InteractionRestricted> GetFarmerInteractions(Profession p, List<String> specs) {

            for (int i = 0; i < specs.Count; i++) {
                if (!p.HasSpecialization(specs[i])) continue;

                switch (specs[i]) {
                    case "tendToFields":
                        foreach (var x in GetTendToFieldsInteractions()) yield return x; break;
                    case "cook": foreach (var x in GetCookInteractions()) yield return x; break;
                    case "harvestCotton":
                        yield return new InteractionRestricted(Interaction.GatherResource,
                        new List<InteractionRestriction>()
                        {
                            new InteractionRestrictionResource(Resource.Cotton),
                            new InteractionRestrictionDesignation(Designation.GatherPlant)
                        });
                        break;

                    case "harvestApples":
                        yield return new InteractionRestricted(Interaction.GatherResource,
                        new List<InteractionRestriction>()
                        {
                        new InteractionRestrictionResource(Resource.Apple),
                        new InteractionRestrictionDesignation(Designation.GatherPlant)
                        }); break;

                    case "careForAnimals":
                        yield return new InteractionRestricted(Interaction.Butcher, new InteractionRestrictionAutoButcher());
                        yield return new InteractionRestricted(Interaction.Tame);
                        yield return new InteractionRestricted(Interaction.Milk);
                        yield return new InteractionRestricted(Interaction.Shear);
                        break;

                    default:
                        foreach (var x in GetOtherInteractions(p, specs[i])) yield return x; break;
                }
            }
        }

        private static IEnumerable<InteractionRestricted> GetCookInteractions() {
            //Get all of the cookable resources
            List<Resource> cookables = CookedFood.Select(x => GameState.GetResourceByString(x)).ToList();

            //Build a dictionary of ratios (NbOwned vs Capacity) so that we order the actions
            //By what is most needed.  i.e. if we have lots of baked tomatoes but no baked potatoes, we'll produce potatoes first
            Dictionary<Resource, double> ratios = cookables.ToDictionary(x => x, x => GetResourceRatio(x));

            //Re-obtain the cooked resource as strings and then order them by ratio
            List<String> orderedCookedFood = new List<string>(CookedFood);
            orderedCookedFood.Sort((x, y) => ratios[GameState.GetResourceByString(x)].CompareTo(ratios[GameState.GetResourceByString(y)]));

            //The original mod creates one interaction for all products but we create one interaction per product
            //So that we can order them
            for (int r = 0; r < CookedFood.Length; r++) {
                yield return new InteractionRestricted(Interaction.Produce,
                    new List<InteractionRestriction>()
                    {
                        new InteractionRestrictionProduction(new List<string>() {CookedFood[r]}),
                        new InteractionRestrictionProfession(ProfessionType.Farmer)
                    });
            }
        }

        private static IEnumerable<InteractionRestricted> GetTendToFieldsInteractions() {
            //Build a dictionary of ratios (NbOwned vs Capacity) so that we order the actions
            //By what is most needed.  i.e. if we have lots of tomatoes but no potatoes, we'll harvest potatoes first
            Dictionary<Resource, double> ratios = FieldsResources.ToDictionary(x => x, x => GetResourceRatio(x));

            //Re-obtain the field resource and then order them by ratio 
            List<Resource> sortedFieldRes = new List<Resource>(FieldsResources);
            sortedFieldRes.Sort((x, y) => ratios[x].CompareTo(ratios[y]));

            //We calculate a bonus so that if we're low on food we lower the priority of watering plants and removing dead plants
            bool lowOnFood = (GameState.Instance.GetResource(Resource.FoodRaw) + GameState.Instance.GetResource(Resource.FoodCooked)) < (5 * WorldScripts.Instance.humanManager.GetColonistCount());

            //If low on food, the healing plants and wheat will have the lowest priority
            //Since they're not edible raw foods
            if (lowOnFood) {
                sortedFieldRes.Remove(Resource.HealingPlantCultivated);
                sortedFieldRes.Remove(Resource.Wheat);
                sortedFieldRes.Add(Resource.HealingPlantCultivated);
                sortedFieldRes.Add(Resource.Wheat);
            }


            for (int r = 0; r < sortedFieldRes.Count; r++) {
                InteractionRestrictionResource rr = new InteractionRestrictionResource(sortedFieldRes[r]);
                InteractionRestrictionModule soilR = new InteractionRestrictionModule(typeof(SoilModule), true);

                yield return new InteractionRestricted(
                    Interaction.Sow,
                    new List<InteractionRestriction>() { rr, soilR });

                yield return new InteractionRestricted(
                    Interaction.GatherResource,
                    new List<InteractionRestriction>() { rr });
            }

            yield return new InteractionRestricted(Interaction.WaterPlant);

            //The original code only adds this interaction in winter but since we can have dead plants caused by
            //bug infestations I add it for all seasons
            yield return new InteractionRestricted(Interaction.RemoveDeadPlants);
        }

        private static IEnumerable<InteractionRestricted> GetForesterInteractions(Profession p, List<String> specs) {

            for (int i = 0; i < specs.Count; i++) {
                if (!p.HasSpecialization(specs[i])) continue;

                switch (specs[i]) {
                    case "chopTrees":
                        yield return new InteractionRestricted(Interaction.GatherResource,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionResource(Resource.Wood),
                                new InteractionRestrictionDesignation(Designation.CutTree)
                            });
                        break;

                    case "growTrees":
                        yield return new InteractionRestricted(Interaction.Sow,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionModule(typeof(GrowingSpotModule), true)
                            });

                        yield return new InteractionRestricted(Interaction.Care,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionModule(typeof(GrowingSpotModule), true)
                            });
                        break;

                    case "clearStumps":
                        yield return new InteractionRestricted(Interaction.ClearStumps,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionResource(Resource.Wood),
                                new InteractionRestrictionDesignation(Designation.CutTree)
                            });
                        break;

                    default:
                        foreach (var x in GetOtherInteractions(p, specs[i])) yield return x; break;
                }
            }
        }

        private static IEnumerable<InteractionRestricted> GetMinerInteractions(Profession p, List<String> specs) {

            for (int i = 0; i < specs.Count; i++) {
                if (!p.HasSpecialization(specs[i])) continue;

                switch (specs[i]) {
                    case "mineStone":
                        yield return new InteractionRestricted(Interaction.GatherResource,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionResource(Resource.Stone),
                                new InteractionRestrictionDesignation(Designation.Mine)
                            });
                        break;

                    case "mineIron":
                        yield return new InteractionRestricted(Interaction.GatherResource,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionResource(Resource.IronOre),
                                new InteractionRestrictionDesignation(Designation.Mine)
                            });
                        break;

                    case "mineCrystal":
                        yield return new InteractionRestricted(Interaction.GatherResource,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionResource(Resource.Crystal),
                                new InteractionRestrictionDesignation(Designation.Mine)
                            });
                        break;

                    default:
                        foreach (var x in GetOtherInteractions(p, specs[i])) yield return x; break;
                }
            }
        }

        private static IEnumerable<InteractionRestricted> GetCraftsmanInteractions(Profession p, List<String> specs) {
            for (int i = 0; i < specs.Count; i++) {
                if (!p.HasSpecialization(specs[i])) continue;

                switch (specs[i]) {
                    case "carpenting":
                    case "tailoring":
                    case "masonry":
                    case "forging":
                    case "weaving":
                        yield return new InteractionRestricted(Interaction.Produce,
                        new List<InteractionRestriction>()
                        {
                        new InteractionRestrictionProfessionSpecialization(specs[i]),
                        new InteractionRestrictionProfession(ProfessionType.Craftsman)
                        });
                        break;

                    default:
                        foreach (var x in GetOtherInteractions(p, specs[i])) yield return x; break;
                }
            }
        }

        private static IEnumerable<InteractionRestricted> GetDoctorInteractions(Profession p, List<String> specs) {
            for (int i = 0; i < specs.Count; i++) {
                if (!p.HasSpecialization(specs[i])) continue;

                switch (specs[i]) {
                    case "produceMedicine":
                        List<String> orderedMeds = new List<string>(Meds);
                        orderedMeds.Sort((x, y) => GetResourceRatio(GameState.GetResourceByString(x)).CompareTo(GetResourceRatio(GameState.GetResourceByString(y))));

                        for (int r = 0; r < orderedMeds.Count; r++) {
                            yield return new InteractionRestricted(Interaction.Produce,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionProduction(new List<string>(){ orderedMeds[r] })
                            });
                        }
                        break;

                    case "tendToPatients":
                        yield return new InteractionRestricted(Interaction.TryFluTreatment);
            yield return new InteractionRestricted(Interaction.GiveFood, new InteractionRestrictionFaction());
            yield return new InteractionRestricted(Interaction.GiveHealingPotion, new InteractionRestrictionFaction());
            yield return new InteractionRestricted(Interaction.BandageWounds, new InteractionRestrictionFaction());
            yield return new InteractionRestricted(Interaction.SplintArm, new InteractionRestrictionFaction());
            yield return new InteractionRestricted(Interaction.SplintLeg, new InteractionRestrictionFaction());
                        break;

                    case "buryDead":
                        yield return new InteractionRestricted(Interaction.Bury, new InteractionRestrictionFaction());
                        break;

                    case "brewBeer":
                        yield return new InteractionRestricted(Interaction.Produce, new InteractionRestrictionProduction(new List<string>() { "beer" }));
                        break;

                    default:
                        foreach (var x in GetOtherInteractions(p, specs[i])) yield return x; break;
                }
            }
        }

        private static IEnumerable<InteractionRestricted> GetScholarInteractions(Profession p, List<String> specs) {

            for (int i = 0; i < specs.Count; i++) {
                if (!p.HasSpecialization(specs[i])) continue;

                switch (specs[i]) {
                    case "performResearch":
                        yield return new InteractionRestricted(Interaction.Produce,
                            new InteractionRestrictionProfession(ProfessionType.Scholar));
                        break;

                    case "mineCrystal":
                        yield return new InteractionRestricted(Interaction.GatherResource,
                            new InteractionRestrictionResource(Resource.Crystal));
                        break;

                    default:
                        foreach (var x in GetOtherInteractions(p, specs[i])) yield return x; break;
                }
            }
        }

        private static IEnumerable<InteractionRestricted> GetBuilderInteractions(Profession p, List<String> specs) {
            yield return new InteractionRestricted(Interaction.Construct);
            yield return new InteractionRestricted(Interaction.Deconstruct);
        }


        private static IEnumerable<InteractionRestricted> GetOtherInteractions(Profession p, String spec) {
            IEnumerable<InteractionRestricted> iterator = GetInteractionsForMods(p, spec);

            if (iterator == null)
                throw new NotImplementedException(String.Format("Could not get the interactions for:\r\n" +
                        "Profession:{0}\r\n" +
                        "Specialization:{1}\r\n" +
                        "This means that a mod implements a new specialization but it is not compatible with this mod.\r\n" +
                        "Tell the author of the mod who added the specialization to override the GetModsInteraction of this mod",
                        p.type, spec));

            foreach (var x in iterator) yield return x;
        }

        /// <summary>
        /// Gets the interactions of specializations added by other mods.
        /// Other mods should use Harmony to add a Prefix to this method and handle it.
        /// See my mod BuildAsProfessionTask for an example how on to do it
        /// </summary>
        /// <param name="p">The profession</param>
        /// <param name="spec">The specialization</param>
        /// <param name="index">Index of the specialization</param>
        /// <param name="list">List of Interactions</param>
        /// <param name="specPriority">Priority of the interaction</param>
        private static IEnumerable<InteractionRestricted> GetInteractionsForMods(Profession p, String spec) {
            return null;
        }

        /// <summary>
        /// Gets the ratio (NbOwned / Capacity) of a resource
        /// </summary>
        /// <param name="res">The resource</param>
        /// <returns></returns>
        private static double GetResourceRatio(Resource res) {
            int nbOwned = GameState.Instance.GetResource(res);
            int nbMax = WorldScripts.Instance.furnitureFactory.GetStockpileLimit(res);
            double ratio = nbOwned * 1.0 / nbMax;

            return ratio;
        }
    }
}
