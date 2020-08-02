using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WitchyMods.AbsoluteProfessionPriorities {
    [HarmonyPatch(typeof(WorkInteractionController), "GetInteractionProposal")]
    public static class WorkInteractionControllerPatch {

        private static WorkInteractionController instance;

        /// <summary>
        /// Overwrites completely the method 'GetInteractionProposal' from the WorkInteractionController
        /// </summary>
        /// <param name="__instance">Instance of the WorkInteractionController</param>
        /// <param name="human">Original HumanAI argument</param>
        /// <param name="__result">The result of the method</param>
        /// <returns>False to prevent the original method from executing</returns>
        public static bool Prefix(WorkInteractionController __instance, HumanAI human, ref InteractionInfo __result) {
            instance = __instance;
            __result = null;

            //This bit is copy/pasted from the original method.  It checks if the 'human' is a colonist and can actually work
            if (human.IsLockedUp() || !human.WillWorkAutomatically() || human.IsLazy() || human.faction.GetFactionType() != FactionType.Colony || human.controlMode == ControlMode.Combat)
                return false;

            //Don't override manual interactions
            InteractionInfo currentInteraction = human.GetCurrentInteractionInfo();
            if (currentInteraction != null && !currentInteraction.issuedByAI) return false;

            //Init the human in case it's new
            AbsoluteProfessionPrioritiesMod.Instance.InitColonist(human);

            //Creates a dictionary of all of the colonist's active professions, grouped by priorities (number of stars)
            Dictionary<int, List<ProfessionType>> profs = human.professionManager.professions.Values.Where(
                x => x.priority > 0).GroupBy(x => x.priority).ToDictionary(
                x => x.Key, x => x.Select(y => y.type).ToList());

            foreach (var priority in profs.Keys.OrderByDescending(x => x)) {
                foreach (var profession in profs[priority].OrderBy(x => UnityEngine.Random.Range(0f, 1f))) {
                    if (!AbsoluteProfessionPrioritiesMod.Instance.ColonistsData[human.GetID()].ContainsKey(profession))
                        continue;

                    foreach (var info in GetNextInteraction(__instance, human, profession)) {
                        if (info != null) {
                            __result = info;
                            return false;
                        }
                    }
                }
            }

            return false;
        }

        private static IEnumerable<InteractionInfo> GetNextInteraction(WorkInteractionController __instance, HumanAI human, ProfessionType profession) {
            //Get all the specializations for this human for this profession
            List<Specialization> specs = AbsoluteProfessionPrioritiesMod.Instance.ColonistsData[human.GetID()][profession].Values.ToList();
            specs = specs.Where(x => x.Active).OrderBy(x => UnityEngine.Random.Range(0f, 1f)).OrderBy(x => x.Priority).ToList();

            switch (profession) {
                case ProfessionType.Farmer: return GetFarmerInteractions(specs, human);
                case ProfessionType.Forester: return GetForesterInteractions(specs, human);
                case ProfessionType.Miner: return GetMinerInteractions(specs, human);
                case ProfessionType.Craftsman: return GetCraftsmanInteractions(specs, human);
                case ProfessionType.Doctor: return GetDoctorInteractions(specs, human);
                case ProfessionType.Scholar: return GetScholarInteractions(specs, human);
                case ProfessionType.Builder: return GetBuilderInteractions(specs, human);
                default:
                    throw new NotImplementedException("The profession type is unknown to this mod and probably new.  " +
                        "This mod is thus incompatible with the current version of the game.  Please disable it.");
            }
        }

        private static Resource[] FieldsResources = new Resource[] { Resource.HealingPlantCultivated, Resource.Strawberry, Resource.Tomato, Resource.Pumpkin, Resource.Potato, Resource.Wheat };
        private static String[] Meds = new string[] { "healingPotion", "medicine" };

        private static IEnumerable<InteractionInfo> GetFarmerInteractions(List<Specialization> specs, HumanAI human) {

            foreach (var spec in specs) {
                switch (spec.Name) {
                    case "tendToFields": {
                            List<Resource> sortedFieldRes = new List<Resource>();

                            foreach (var sub in spec.GetOrderedSubSpecializations()) {
                                Resource res = (Resource)Enum.Parse(typeof(Resource), sub.Name.Split('_').Last());
                                sortedFieldRes.Add(res);
                            }

                            if (spec.AutoManageSubSpecializations) {
                                Dictionary<Resource, double> ratios = FieldsResources.ToDictionary(x => x, x => GetResourceRatio(x));
                                sortedFieldRes.Sort((x, y) => ratios[x].CompareTo(ratios[y]));
                            }

                            bool lowOnFood = (GameState.Instance.GetResource(Resource.FoodRaw) + GameState.Instance.GetResource(Resource.FoodCooked)) < (5 * WorldScripts.Instance.humanManager.GetColonistCount());

                            if (lowOnFood) {
                                List<Resource> edibleResources = sortedFieldRes.Where(x => x != Resource.Wheat && x != Resource.HealingPlantCultivated).ToList();

                                if (edibleResources.Any()) {
                                    InteractionRestricted harvestInteraction = new InteractionRestricted(Interaction.GatherResource,
                                    new List<InteractionRestriction>()
                                    {
                                        new InteractionRestrictionResource(edibleResources)
                                    });

                                    yield return CheckInteraction(harvestInteraction, human);
                                }
                            }

                            foreach (var res in sortedFieldRes) {
                                InteractionRestricted interaction = new InteractionRestricted(YieldMicroInteractionHelper.TendToFieldsInteraction,
                                    new List<InteractionRestriction>()
                                    {
                                    new InteractionRestrictionModule(typeof(SoilModule), true),
                                    new InteractionRestrictionResource(res)
                                    });

                                yield return CheckInteraction(interaction, human);
                            }
                        }
                        break;
                    case "cook": foreach (var x in GetCookInteractions(human, spec)) yield return CheckInteraction(x, human); break;
                    case "harvestApples":
                    case "harvestCotton": {
                            InteractionRestricted interaction = new InteractionRestricted(Interaction.GatherResource,
                                new List<InteractionRestriction>()
                                {
                                    new InteractionRestrictionResource(spec.Name == "harvestCotton" ?  Resource.Cotton : Resource.Apple),
                                    new InteractionRestrictionDesignation(Designation.GatherPlant)
                                });

                            yield return CheckInteraction(interaction, human);
                        }
                        break;

                    case "careForAnimals": foreach (var x in GetCareForAnimalsInteractions(human, spec)) yield return CheckInteraction(x, human); break;

                    default:
                        foreach (var x in GetOtherInteractions(spec)) {
                            yield return CheckInteraction(x, human);
                        }
                        break;
                }
            }
        }

        private static IEnumerable<InteractionRestricted> GetCareForAnimalsInteractions(HumanAI human, Specialization spec) {
            List<Interaction> interactions = new List<Interaction>();

            foreach (var sub in spec.GetOrderedSubSpecializations()) {
                switch (sub.Name) {
                    case "Farmer_CareForAnimals_Butcher": interactions.Add(Interaction.Butcher); break;
                    case "Farmer_CareForAnimals_Shear": interactions.Add(Interaction.Shear); break;
                    case "Farmer_CareForAnimals_Milk": interactions.Add(Interaction.Milk); break;
                    case "Farmer_CareForAnimals_Tame": interactions.Add(Interaction.Tame); break;
                }
            }

            Vector3 humanPos = human.GetPosition();
            IEnumerable<HumanAI> animals = human.faction.GetLivingHumans().Where(x => x.animal != null)
                .OrderBy(x => Vector3.Distance(humanPos, x.GetPosition()));

            if (spec.AutoManageSubSpecializations) {
                //Start with taming
                if (interactions.Contains(Interaction.Tame)) {
                    interactions.Remove(Interaction.Tame);

                    foreach (var animal in animals.OrderBy(x => x.animal.tameness)) {
                        foreach (var tameIntRes in animal.GetInteractable().GetInteractions(human, true, false)
                            .Where(x => x.interaction == Interaction.Tame)) {
                            yield return tameIntRes;
                        }
                    }
                }

                foreach (var animal in animals) {
                    foreach (var intRes in animal.GetInteractable().GetInteractions(human, true, false)
                        .Where(x => interactions.Contains(x.interaction))) {
                        yield return intRes;
                    }
                }
            } else {
                foreach (var interaction in interactions) {
                    foreach (var animal in animals) {
                        foreach (var intRes in animal.GetInteractable().GetInteractions(human, true, false)
                            .Where(x => x.interaction == interaction)) {
                            yield return intRes;
                        }
                    }
                }
            }

        }

        private static IEnumerable<InteractionRestricted> GetCookInteractions(HumanAI human, Specialization spec) {
            List<Resource> meals = new List<Resource>();

            foreach (var sub in spec.GetOrderedSubSpecializations()) {
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

            if (spec.AutoManageSubSpecializations) {
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

        private static IEnumerable<InteractionInfo> GetForesterInteractions(List<Specialization> specs, HumanAI human) {
            Vector3 humanPos = human.GetPosition();

            foreach (var spec in specs) {
                var orderedSubs = spec.GetOrderedSubSpecializations();

                switch (spec.Name) {
                    case "chopTrees": {

                            IEnumerable<ResourceModule> woodModules = WorldScripts.Instance.furnitureFactory.GetModules<ResourceModule>()
                                    .Where(x => x.GetResource() == Resource.Wood && !x.isDepleted)
                                    .OrderBy(x => Vector3.Distance(humanPos, x.parent.GetPosition())); ;

                            List<string> activeSubs = spec.GetOrderedSubSpecializations().Select(x => x.Name).ToList();

                            if (activeSubs.Count == 0) continue;


                            if (spec.AutoManageSubSpecializations) {
                                if (!activeSubs.Contains("Forester_ChopTrees_Stumps"))
                                    woodModules = woodModules.Where(x => x.resourcesPerRound != 0);
                                if (!activeSubs.Contains("Forester_ChopTrees_BigTrees"))
                                    woodModules = woodModules.Where(x => x.resourcesPerRound != AbsoluteProfessionPrioritiesMod.MaxTreeWoodAmount);
                                if (!activeSubs.Contains("Forester_ChopTrees_SmallTrees"))
                                    woodModules = woodModules.Where(x => x.resourcesPerRound == 0 || x.resourcesPerRound == AbsoluteProfessionPrioritiesMod.MaxTreeWoodAmount);

                                bool lowOnWood = GetResourceRatio(Resource.Wood) < 0.20;
                                if (lowOnWood) woodModules = woodModules.OrderByDescending(x => x.resourcesPerRound);

                                foreach (var woodModule in woodModules) {
                                    InteractionRestricted intRes = woodModule.GetInteractions(human, true, false).First();
                                    if (!CheckInteraction(woodModule.parent.GetInteractable(), intRes, human)) continue;
                                    yield return new InteractionInfo(intRes.interaction, woodModule.parent.GetInteractable(), intRes.restrictions, true, 50);
                                }
                            } else {
                                IEnumerable<ResourceModule> subModules = null;
                                foreach (var sub in activeSubs) {
                                    switch (sub) {
                                        case "Forester_ChopTrees_Stumps": subModules = woodModules.Where(x => x.resourcesPerRound == 0); break;
                                        case "Forester_ChopTrees_BigTrees": subModules = woodModules.Where(x => x.resourcesPerRound == AbsoluteProfessionPrioritiesMod.MaxTreeWoodAmount); break;
                                        case "Forester_ChopTrees_SmallTrees": subModules = woodModules.Where(x => x.resourcesPerRound != 0 && x.resourcesPerRound != AbsoluteProfessionPrioritiesMod.MaxTreeWoodAmount); break;
                                    }

                                    foreach (var woodModule in subModules) {
                                        InteractionRestricted intRes = woodModule.GetInteractions(human, true, false).First();
                                        if (!CheckInteraction(woodModule.parent.GetInteractable(), intRes, human)) continue;
                                        yield return new InteractionInfo(intRes.interaction, woodModule.parent.GetInteractable(), intRes.restrictions, true, 50);
                                    }
                                }
                            }
                        }
                        break;

                    case "growTrees": {
                            List<string> activeSubs = spec.GetOrderedSubSpecializations().Select(x => x.Name).ToList();

                            if (activeSubs.Count == 0) continue;

                            IEnumerable<GrowingSpotModule> growingSpotModules = WorldScripts.Instance.furnitureFactory.GetModules<GrowingSpotModule>()
                                .Where(x => x.parent.IsBuilt() && x.parent.IsValid() &&
                                human.professionManager.HasSkill(x.requiredSkill))
                                .OrderBy(x => Vector3.Distance(humanPos, x.parent.GetPosition()));

                            if (spec.AutoManageSubSpecializations) {
                                if (!activeSubs.Contains("Forester_GrowTrees_PineTrees"))
                                    growingSpotModules = growingSpotModules.Where(x => x.requiredSkill != "growingTrees");

                                if (!activeSubs.Contains("Forester_GrowTrees_Apples"))
                                    growingSpotModules = growingSpotModules.Where(x => x.requiredSkill != "growingAppleTrees");

                                if (!activeSubs.Contains("Forester_GrowTrees_Cotton"))
                                    growingSpotModules = growingSpotModules.Where(x => x.requiredSkill != "growingCotton");

                                foreach (var spot in growingSpotModules) {
                                    foreach (var intRes in spot.GetInteractions(human, true, false)
                                        .Where(x => x.interaction != Interaction.RemovePlant)) {
                                        if (CheckInteraction(spot.parent.GetInteractable(), intRes, human)) {
                                            yield return new InteractionInfo(intRes.interaction, spot.parent.GetInteractable(), intRes.restrictions, true, 50);
                                        }
                                    }
                                }
                            } else {
                                IEnumerable<GrowingSpotModule> subModules = null;

                                foreach(var sub in activeSubs) {
                                    switch (sub) {
                                        case "Forester_GrowTrees_PineTrees": subModules = growingSpotModules.Where(x => x.requiredSkill == "growingTrees"); break;
                                        case "Forester_GrowTrees_Apples": subModules = growingSpotModules.Where(x => x.requiredSkill == "growingAppleTrees"); break;
                                        case "Forester_GrowTrees_Cotton": subModules = growingSpotModules.Where(x => x.requiredSkill == "growingCotton"); break;
                                    }

                                    foreach (var spot in subModules) {
                                        foreach (var intRes in spot.GetInteractions(human, true, false)
                                            .Where(x => x.interaction != Interaction.RemovePlant)) {
                                            if (CheckInteraction(spot.parent.GetInteractable(), intRes, human)) {
                                                yield return new InteractionInfo(intRes.interaction, spot.parent.GetInteractable(), intRes.restrictions, true, 50);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    default:
                        foreach (var x in GetOtherInteractions(spec)) {
                            yield return CheckInteraction(x, human);
                        }
                        break;
                }
            }
        }

        private static IEnumerable<InteractionInfo> GetMinerInteractions(List<Specialization> specs, HumanAI human) {
            foreach (var spec in specs) {
                Resource resource = Resource.None;

                switch (spec.Name) {
                    case "mineStone": resource = Resource.Stone; break;

                    case "mineIron": resource = Resource.IronOre; break;

                    case "mineCrystal": resource = Resource.Crystal; break;

                    default:
                        foreach (var x in GetOtherInteractions(spec)) {
                            yield return CheckInteraction(x, human);
                        }
                        break;
                }

                Vector3 humanPos = human.GetPosition();

                if(resource != Resource.None) {
                    foreach(var rm in WorldScripts.Instance.furnitureFactory.GetModules<ResourceModule>()
                        .Where(x=>x.GetResource() == resource && !x.isDepleted && x.IsValid())
                        .OrderBy(x=> Vector3.Distance(humanPos, x.parent.GetPosition()))) {
                        foreach(var intRes in rm.GetInteractions(human, true, false)) {
                            if(CheckInteraction(rm.parent.GetInteractable(), intRes, human)) {
                                yield return new InteractionInfo(intRes.interaction, rm.parent.GetInteractable(), intRes.restrictions, true, 50);
                            }
                        }
                    }
                }
            }
        }

        private static IEnumerable<InteractionInfo> GetCraftsmanInteractions(List<Specialization> specs, HumanAI human) {
            foreach (var spec in specs) {
                switch (spec.Name) {
                    case "carpenting":
                    case "tailoring":
                    case "masonry":
                    case "forging":
                    case "weaving":
                        yield return CheckInteraction(new InteractionRestricted(Interaction.Produce,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionProfessionSpecialization(spec.Name),
                                new InteractionRestrictionProfession(ProfessionType.Craftsman)
                            }), human);
                        break;

                    default:
                        foreach (var x in GetOtherInteractions(spec)) {
                            yield return CheckInteraction(x, human);
                        }
                        break;
                }
            }
        }

        private static IEnumerable<InteractionInfo> GetDoctorInteractions(List<Specialization> specs, HumanAI human) {
            foreach (var spec in specs) {
                switch (spec.Name) {
                    case "produceMedicine":
                        List<String> orderedMeds = new List<string>();

                        foreach (var sub in spec.GetOrderedSubSpecializations()) {
                            switch (sub.Name) {
                                case "Doctor_ProduceMedicine_FluVaccines": orderedMeds.Add("medicine"); break;
                                case "Doctor_ProduceMedicine_HealingPotions": orderedMeds.Add("healingPotion"); break;
                            }
                        }

                        if (spec.AutoManageSubSpecializations) {
                            orderedMeds.Sort((x, y) => GetResourceRatio(GameState.GetResourceByString(x)).CompareTo(GetResourceRatio(GameState.GetResourceByString(y))));
                        } 

                         for (int r = 0; r < orderedMeds.Count; r++) {
                            var result = CheckInteraction(new InteractionRestricted(Interaction.Produce,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionProduction(new List<string>(){ orderedMeds[r] })
                            }), human);

                            DebugLogger.Log($"result:{result}");
                            yield return result;
                        }
                        break;

                    case "tendToPatients":
                        foreach (var interaction in new Interaction[] { Interaction.TryFluTreatment, Interaction.GiveFood, Interaction.GiveHealingPotion, Interaction.BandageWounds, Interaction.SplintArm, Interaction.SplintLeg }) {
                            yield return CheckInteraction(new InteractionRestricted(interaction,
                                interaction == Interaction.TryFluTreatment ? null : new InteractionRestrictionFaction()), human);
                        }
                        break;

                    case "buryDead":
                        //bury colonists
                        InteractionInfo info = CheckInteraction(new InteractionRestricted(Interaction.Bury, new InteractionRestrictionFaction()),
                            human);

                        if (info != null) {
                            var filledInfo = AccessTools.Field(typeof(GraveModule), "filled");

                            if (!WorldScripts.Instance.furnitureFactory.GetModules<GraveModule>().Any(x =>
                                    !((bool)filledInfo.GetValue(x)) && x.parent.IsBuilt())) {
                                if (AbsoluteProfessionPrioritiesMod.Instance.buryColonistFailCooldown.HasValue) {
                                    info = null;
                                } else yield return info;

                            } else
                                AbsoluteProfessionPrioritiesMod.Instance.buryColonistFailCooldown = null;
                        }

                        if (info == null) {
                            //bury non-colonists
                            yield return CheckInteraction(new InteractionRestricted(Interaction.Bury, new List<InteractionRestriction>() {
                                new InteractionRestrictionEmptyGrave(),
                                new InteractionRestrictionFactionType(FactionType.Colony, false)}),
                                human);
                        }
                        break;

                    case "brewBeer":
                        yield return CheckInteraction(new InteractionRestricted(Interaction.Produce, new InteractionRestrictionProduction(new List<string>() { "beer" })), human);
                        break;

                    default:
                        foreach (var x in GetOtherInteractions(spec)) {
                            yield return CheckInteraction(x, human);
                        }
                        break;
                }
            }
        }

        private static IEnumerable<InteractionInfo> GetScholarInteractions(List<Specialization> specs, HumanAI human) {
            foreach (var spec in specs) {
                switch (spec.Name) {
                    case "performResearch":
                        Vector3 humanPos = human.GetPosition();
                        IEnumerable<ChangeProfessionModule> scholarModule = WorldScripts.Instance.furnitureFactory.GetModules<ChangeProfessionModule>()
                            .Where(x => x.profession == "Scholar");

                        IEnumerable<ProductionModule> productionModules = scholarModule
                            .Where(x => x.parent.IsBuilt() && x.parent.IsValid())
                            .Select(x => x.parent.GetModule<ProductionModule>());

                        IEnumerable<ProductionModule> filteredModules = null;

                        foreach (var sub in spec.GetOrderedSubSpecializations()) {
                            switch (sub.Name) {
                                case "Scholar_PerformResearch_BookStand":
                                    filteredModules =productionModules.Where(x => x.interactionNameKey == "analyzeCrystal");
                                    break;

                                case "Scholar_PerformResearch_ScrollStand":
                                    if (human.professionManager.HasSkill("researchScrolls")) {
                                        filteredModules=productionModules.Where(x => x.interactionNameKey == "analyzeScroll");
                                    }
                                    break;
                            }

                            foreach (var module in filteredModules
                                        .OrderBy(x => Vector3.Distance(humanPos, x.parent.GetPosition()))) {
                                foreach (var intRes in module.GetInteractions(human, true, false)
                                    .Where(x => x.interaction == Interaction.Produce)) {
                                    if (CheckInteraction(module.parent.GetInteractable(), intRes, human)) {
                                        yield return new InteractionInfo(intRes.interaction, module.parent.GetInteractable(),
                                            intRes.restrictions, true, 50);
                                    }
                                }
                            }
                        }
                        break;

                    case "mineCrystal":
                        yield return CheckInteraction(new InteractionRestricted(Interaction.GatherResource,
                            new InteractionRestrictionResource(Resource.Crystal)), human);
                        break;

                    default:
                        foreach (var x in GetOtherInteractions(spec)) {
                            yield return CheckInteraction(x, human);
                        }
                        break;
                }
            }
        }

        private static IEnumerable<InteractionInfo> GetBuilderInteractions(List<Specialization> specs, HumanAI human) {

            foreach (var spec in specs) {
                switch (spec.Name) {
                    case "buildFields": yield return CheckInteraction(new InteractionRestricted(Interaction.Construct, new InteractionRestrictionConstructableType(ConstructableType.Fields)), human); break;

                    case "buildStructures": yield return CheckInteraction(new InteractionRestricted(Interaction.Construct, new InteractionRestrictionConstructableType(ConstructableType.Structures)), human); break;

                    case "buildFurnitures": yield return CheckInteraction(new InteractionRestricted(Interaction.Construct, new InteractionRestrictionConstructableType(ConstructableType.Furniture)), human); break;

                    default:
                        foreach (var x in GetOtherInteractions(spec)) {
                            yield return CheckInteraction(x, human);
                        }
                        break;
                }
            }

            yield return CheckInteraction(new InteractionRestricted(Interaction.Deconstruct), human);
        }

        private static IEnumerable<InteractionRestricted> GetOtherInteractions(Specialization spec) {
            IEnumerable<InteractionRestricted> iterator = GetInteractionsForMods(spec.Name);

            if (iterator == null)
                throw new NotImplementedException(String.Format("Could not get the interactions for:\r\n" +
                        "Specialization:{0}\r\n" +
                        "This means that a mod implements a new specialization but it is not compatible with this mod.\r\n" +
                        "Tell the author of the mod who added the specialization to override the GetModsInteraction of this mod",
                        spec.Name));

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
        private static IEnumerable<InteractionRestricted> GetInteractionsForMods(string spec) {
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

        public static InteractionInfo CheckInteraction(InteractionRestricted interactionRestricted, HumanAI human) {
            return AccessTools.Method(instance.GetType(), "CheckInteraction", new Type[] { typeof(InteractionRestricted), typeof(HumanAI) }).Invoke(instance, new object[] { interactionRestricted, human }) as InteractionInfo;
        }

        public static bool CheckInteraction(Interactable interactable, InteractionRestricted interactionRestricted, HumanAI human) {
            return (bool)AccessTools.Method(instance.GetType(), "CheckInteraction", new Type[] { typeof(Interactable), typeof(InteractionRestricted), typeof(HumanAI) }).Invoke(instance, new object[] { interactable, interactionRestricted, human });
        }
    }
}
