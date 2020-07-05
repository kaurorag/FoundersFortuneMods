using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace WitchyMods.AbsoluteProfessionPriorities
{
    /// <summary>
    /// Completetly rewrites the GetInteractions method of the Profession class
    /// </summary>
    [HarmonyPatch(typeof(Profession), "GetInteractions")]
    public static class ProfessionPatch
    {
        /// <summary>
        /// Completetly rewrites the GetInteractions method of the Profession class
        /// </summary>
        public static bool Prefix(Profession __instance, HumanAI human, List<InteractionRestricted> list)
        {
            //If the profession isn't active or the human isn't a colonist, do nothing
            if (__instance.priority <= 0 || human.faction.GetFactionType() != FactionType.Colony)
                return false;

            //Get the instance of our mod
            AbsoluteProfessionPrioritiesMod mod = ModHandler.mods.scriptMods.OfType<AbsoluteProfessionPrioritiesMod>().First();

            //Init the human in case it's new
            mod.InitHuman(human.GetID());

            //Get all the specializations for this human for this profession
            List<String> specs = new List<string>(mod.specializationPriorities[human.GetID()][__instance.type]);

            //Reverse the order so that the higher the priority, the higher its index
            specs.Reverse();

            int basePriority = __instance.priority * 10000;

            //Call a different method depending on the profession type
            switch (__instance.type)
            {
                case ProfessionType.Farmer: GetFarmerInteractions(__instance, specs, list, basePriority); break;
                case ProfessionType.Forester: GetForesterInteractions(__instance, specs, list, basePriority); break;
                case ProfessionType.Miner: GetMinerInteractions(__instance, specs, list, basePriority); break;
                case ProfessionType.Craftsman: GetCraftsmanInteractions(__instance, specs, list, basePriority); break;
                case ProfessionType.Doctor: GetDoctorInteractions(__instance, specs, list, basePriority); break;
                case ProfessionType.Scholar: GetScholarInteractions(__instance, specs, list, basePriority); break;
                default:
                    throw new NotImplementedException("The profession type is unknown to this mod and probably new.  " +
                        "This mod is thus incompatible with the current version of the game.  Please disable it.");
            }

            return false;
        }

        private static Resource[] FieldsResources = new Resource[] { Resource.Tomato, Resource.Pumpkin, Resource.Potato, Resource.Strawberry, Resource.Wheat, Resource.HealingPlantCultivated };
        private static String[] CookedFood = new string[] { "bakedApple", "bakedPotato", "bakedTomato", "pumpkinStew", "potatoSoup", "fruitSalad", "bread", "strawberryCake", "appleStrudel" };
        private static String[] Meds = new string[] { "healingPotion", "medicine" };

        private static void GetFarmerInteractions(Profession p, List<String> specs, List<InteractionRestricted> list, int basePriority)
        {
            int specPriority = basePriority;

            for (int i = 0; i < specs.Count; i++)
            {
                if (!p.HasSpecialization(specs[i])) continue;

                switch (specs[i])
                {
                    case "tendToFields": GetTendToFieldsInteractions(p, list, i, specPriority); break;
                    case "cook": GetCookInteractions(p, list, i, specPriority); break;
                    case "harvestCotton":
                        list.Add(new InteractionRestricted(Interaction.GatherResource,
                        new List<InteractionRestriction>()
                        {
                            new InteractionRestrictionResource(Resource.Cotton),
                            new InteractionRestrictionDesignation(Designation.GatherPlant)
                        },
                        specPriority)); break;

                    case "harvestApples":
                        list.Add(new InteractionRestricted(Interaction.GatherResource,
                        new List<InteractionRestriction>()
                        {
                        new InteractionRestrictionResource(Resource.Apple),
                        new InteractionRestrictionDesignation(Designation.GatherPlant)
                        },
                        specPriority)); break;

                    default:
                        GetOtherInteractions(p, specs[i], i, list, specPriority); break;
                }

                specPriority += 1000;
            }
        }

        private static void GetCookInteractions(Profession p, List<InteractionRestricted> list, int i, int specPriority)
        {
            //Get all of the cookable resources
            List<Resource> cookables = CookedFood.Select(x => GameState.GetResourceByString(x)).ToList();

            //Build a dictionary of ratios (NbOwned vs Capacity) so that we order the actions
            //By what is most needed.  i.e. if we have lots of baked tomatoes but no baked potatoes, we'll produce potatoes first
            Dictionary<Resource, double> ratios = cookables.ToDictionary(x => x, x => GetResourceRatio(x));

            //Re-obtain the cooked resource as strings and then order them by ratio (DESC)
            List<String> orderedCookedFood = new List<string>(CookedFood);
            orderedCookedFood.Sort((x, y) => ratios[GameState.GetResourceByString(y)].CompareTo(ratios[GameState.GetResourceByString(x)]));

            //The original mod creates one interaction for all products but we create one interaction per product
            //So that we can order them
            for (int r = 0; r < CookedFood.Length; r++)
            {
                list.Add(new InteractionRestricted(Interaction.Produce,
                    new List<InteractionRestriction>()
                    {
                        new InteractionRestrictionProduction(new List<string>() {CookedFood[r]}),
                        new InteractionRestrictionProfession(ProfessionType.Farmer)
                    },
                    specPriority + r));
            }
        }

        private static void GetTendToFieldsInteractions(Profession p, List<InteractionRestricted> list, int i, int specPriority)
        {
            //Build a dictionary of ratios (NbOwned vs Capacity) so that we order the actions
            //By what is most needed.  i.e. if we have lots of tomatoes but no potatoes, we'll harvest potatoes first
            Dictionary<Resource, double> ratios = FieldsResources.ToDictionary(x => x, x => GetResourceRatio(x));

            //Re-obtain the field resource and then order them by ratio (DESC)
            List<Resource> sortedFieldRes = new List<Resource>(FieldsResources);
            sortedFieldRes.Sort((x, y) => ratios[y].CompareTo(ratios[x]));

            //We calculate a bonus so that if we're low on food we lower the priority of watering plants and removing dead plants
            bool lowOnFood = (GameState.Instance.GetResource(Resource.FoodRaw) + GameState.Instance.GetResource(Resource.FoodCooked)) < (5 * WorldScripts.Instance.humanManager.GetColonistCount());
            int priorityBonus = lowOnFood ? 300 : 0;

            /* Order of interactions
             *      When low on food            |   Not Low on Food
             *      ¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯|¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯
             *      GatherResource      [400]   |   RemoveDeadPlants    [200]
             *      Sow                 [300]   |   GatherResource      [100]
             *      RemoveDeadPlants    [300]   |   Sow                 [100]
             *      Water               [000]   |   Water               [000]
             */

            //If low on food, the healing plants and wheat will had the lowest priority
            //Since they're not edible raw foods
            if (lowOnFood)
            {
                sortedFieldRes.Remove(Resource.HealingPlantCultivated);
                sortedFieldRes.Remove(Resource.Wheat);
                sortedFieldRes.Insert(0, Resource.HealingPlantCultivated);
                sortedFieldRes.Insert(0, Resource.Wheat);
            }

            //The original mod creates one interaction for all products but we create one interaction per resource
            //So that we can order them.  The gather resource has a tiny bonus so we always harvest before sowing
            bool sowFirst = new Random().Next(0, 2) == 1;

            for (int r = 0; r < sortedFieldRes.Count; r++)
            {
                InteractionRestrictionResource rr = new InteractionRestrictionResource(sortedFieldRes[r]);
                InteractionRestrictionModule soilR = new InteractionRestrictionModule(typeof(SoilModule), true);

                list.Add(new InteractionRestricted(
                    Interaction.Sow,
                    new List<InteractionRestriction>() { rr, soilR },
                    specPriority + r * 10 + priorityBonus + 100 + (sowFirst ? 1 : 0)));

                list.Add(new InteractionRestricted(
                    Interaction.GatherResource,
                    new List<InteractionRestriction>() { rr },
                    specPriority + r * 10 + priorityBonus + 100 + (sowFirst ? 0 : 1)));
            }

            //We add a bonus to watering plants
            list.Add(new InteractionRestricted(Interaction.WaterPlant,
                specPriority));

            //The original code only adds this interaction in winter but since we can have dead plants caused by
            //bug infestations I add it for all seasons
            //Also adds a bonus so we remove dead plants first to maximize the number of growing plants
            list.Add(new InteractionRestricted(Interaction.RemoveDeadPlants,
                specPriority + 200));
        }

        private static void GetForesterInteractions(Profession p, List<String> specs, List<InteractionRestricted> list, int basePriority)
        {
            int specPriority = basePriority;

            for (int i = 0; i < specs.Count; i++)
            {
                if (!p.HasSpecialization(specs[i])) continue;

                switch (specs[i])
                {
                    case "chopTrees":
                        list.Add(new InteractionRestricted(Interaction.GatherResource,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionResource(Resource.Wood),
                                new InteractionRestrictionDesignation(Designation.CutTree)
                            },
                            specPriority));
                        break;

                    case "growTrees":
                        list.Add(new InteractionRestricted(Interaction.Sow,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionModule(typeof(GrowingSpotModule), true)
                            },
                            specPriority));

                        list.Add(new InteractionRestricted(Interaction.Care,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionModule(typeof(GrowingSpotModule), true)
                            },
                            specPriority));
                        break;

                    default:
                        GetOtherInteractions(p, specs[i], i, list, specPriority); break;
                }

                specPriority += 1000;
            }
        }

        private static void GetMinerInteractions(Profession p, List<String> specs, List<InteractionRestricted> list, int basePriority)
        {
            int specPriority = basePriority;

            for (int i = 0; i < specs.Count; i++)
            {
                if (!p.HasSpecialization(specs[i])) continue;

                switch (specs[i])
                {
                    case "mineStone":
                        list.Add(new InteractionRestricted(Interaction.GatherResource,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionResource(Resource.Stone),
                                new InteractionRestrictionDesignation(Designation.Mine)
                            },
                            specPriority));
                        break;

                    case "mineIron":
                        list.Add(new InteractionRestricted(Interaction.GatherResource,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionResource(Resource.IronOre),
                                new InteractionRestrictionDesignation(Designation.Mine)
                            },
                            specPriority));
                        break;

                    case "mineCrystal":
                        list.Add(new InteractionRestricted(Interaction.GatherResource,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionResource(Resource.Crystal),
                                new InteractionRestrictionDesignation(Designation.Mine)
                            },
                            specPriority));
                        break;

                    default:
                        GetOtherInteractions(p, specs[i], i, list, specPriority); break;
                }

                specPriority += 1000;
            }
        }

        private static void GetCraftsmanInteractions(Profession p, List<String> specs, List<InteractionRestricted> list, int basePriority)
        {
            int specPriority = basePriority;

            for (int i = 0; i < specs.Count; i++)
            {
                if (!p.HasSpecialization(specs[i])) continue;

                switch (specs[i])
                {
                    case "carpenting":
                    case "tailoring":
                    case "masonry":
                    case "forging":
                        list.Add(new InteractionRestricted(Interaction.Produce,
                        new List<InteractionRestriction>()
                        {
                        new InteractionRestrictionProfessionSpecialization(specs[i]),
                        new InteractionRestrictionProfession(ProfessionType.Craftsman)
                        },
                        specPriority));
                        break;

                    default:
                        GetOtherInteractions(p, specs[i], i, list, specPriority); break;
                }

                specPriority += 1000;
            }
        }

        private static void GetDoctorInteractions(Profession p, List<String> specs, List<InteractionRestricted> list, int basePriority)
        {
            int specPriority = basePriority;

            for (int i = 0; i < specs.Count; i++)
            {
                if (!p.HasSpecialization(specs[i])) continue;

                switch (specs[i])
                {
                    case "produceMedicine":
                        List<String> orderedMeds = new List<string>(Meds);
                        orderedMeds.Sort((x, y) => GetResourceRatio(GameState.GetResourceByString(y)).CompareTo(GetResourceRatio(GameState.GetResourceByString(x))));

                        for (int r = 0; r < orderedMeds.Count; r++)
                        {
                            list.Add(new InteractionRestricted(Interaction.Produce,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionProduction(new List<string>(){ orderedMeds[r] })
                            },
                            specPriority + r * 10));
                        }
                        break;

                    case "tendToPatients":
                        list.Add(new InteractionRestricted(Interaction.TryFluTreatment, specPriority + 1));
                        list.Add(new InteractionRestricted(Interaction.GiveFood, new InteractionRestrictionFaction(), specPriority + 2));
                        list.Add(new InteractionRestricted(Interaction.GiveHealingPotion, new InteractionRestrictionFaction(), specPriority));
                        list.Add(new InteractionRestricted(Interaction.BandageWounds, new InteractionRestrictionFaction(), specPriority));
                        list.Add(new InteractionRestricted(Interaction.SplintArm, new InteractionRestrictionFaction(), specPriority));
                        list.Add(new InteractionRestricted(Interaction.SplintLeg, new InteractionRestrictionFaction(), specPriority));
                        break;

                    case "buryDead":
                        list.Add(new InteractionRestricted(Interaction.Bury, new InteractionRestrictionFaction(), specPriority));
                        break;

                    case "brewBeer":
                        list.Add(new InteractionRestricted(Interaction.Produce, new InteractionRestrictionProduction(new List<string>() { "beer" }), specPriority));
                        break;

                    default:
                        GetOtherInteractions(p, specs[i], i, list, specPriority); break;
                }

                specPriority += 1000;
            }
        }

        private static void GetScholarInteractions(Profession p, List<String> specs, List<InteractionRestricted> list, int basePriority)
        {
            int specPriority = basePriority;

            for (int i = 0; i < specs.Count; i++)
            {
                if (!p.HasSpecialization(specs[i])) continue;

                switch (specs[i])
                {
                    case "performResearch":
                        list.Add(new InteractionRestricted(Interaction.Produce,
                            new InteractionRestrictionProfession(ProfessionType.Scholar),
                            specPriority));
                        break;

                    case "mineCrystal":
                        list.Add(new InteractionRestricted(Interaction.GatherResource,
                            new InteractionRestrictionResource(Resource.Crystal),
                            specPriority));
                        break;

                    default: GetOtherInteractions(p, specs[i], i, list, specPriority); break;
                }

                specPriority += 1000;
            }
        }

        private static void GetOtherInteractions(Profession p, String spec, int index, List<InteractionRestricted> list, int specPriority)
        {
            bool success = false;

            GetInteractionsForMods(p, spec, index, list, specPriority, ref success);

            if (!success)
                throw new NotImplementedException(String.Format("Could not get the interactions for:\r\n" +
                        "Profession:{0}\r\n" +
                        "Specialization:{1}\r\n" +
                        "This means that a mod implements a new specialization but it is not compatible with this mod.\r\n" +
                        "Tell the author of the mod who added the specialization to override the GetModsInteraction of this mod",
                        p.type, spec));
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
        private static void GetInteractionsForMods(Profession p, String spec, int index, List<InteractionRestricted> list, int specPriority, ref bool success)
        {
        }

        /// <summary>
        /// Gets the ratio (NbOwned / Capacity) of a resource
        /// </summary>
        /// <param name="res">The resource</param>
        /// <returns></returns>
        private static double GetResourceRatio(Resource res)
        {
            int nbOwned = GameState.Instance.GetResource(res);
            int nbMax = WorldScripts.Instance.furnitureFactory.GetStockpileLimit(res);
            double ratio = nbOwned * 1.0 / nbMax;

            return ratio;
        }
    }
}
