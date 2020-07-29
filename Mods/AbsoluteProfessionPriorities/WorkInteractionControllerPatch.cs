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

            //Don't override manual interactions
            InteractionInfo currentInteraction = human.GetCurrentInteractionInfo();
            if (currentInteraction != null && !currentInteraction.issuedByAI) return false;

            //Init the human in case it's new
            AbsoluteProfessionPrioritiesMod.Instance.InitHuman(human.GetID());

            //Creates a dictionary of all of the colonist's active professions, grouped by priorities (number of stars)
            Dictionary<int, List<Profession>> profs = human.professionManager.professions.Values.Where(
                x => x.priority > 0).GroupBy(x => x.priority).ToDictionary(
                x => x.Key, x => x.ToList());

            foreach (var p in human.professionManager.professions.Values.Where(x => x.priority > 0))

                //For each priority (number of stars), starting by the highest to the lowest
                foreach (var professionPriority in profs.Keys.OrderByDescending(x => x)) {
                    foreach (var profession in profs[professionPriority].OrderBy(x => UnityEngine.Random.Range(0, profs[professionPriority].Count))) {
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
            //Get all the specializations for this human for this profession
            List<string> specs = AbsoluteProfessionPrioritiesMod.Instance.specializationPriorities[human.GetID()][profession.type];

            switch (profession.type) {
                case ProfessionType.Farmer: return GetFarmerInteractions(profession, specs, human);
                case ProfessionType.Forester: return GetForesterInteractions(profession, specs, human);
                case ProfessionType.Miner: return GetMinerInteractions(profession, specs, human);
                case ProfessionType.Craftsman: return GetCraftsmanInteractions(profession, specs, human);
                case ProfessionType.Doctor: return GetDoctorInteractions(profession, specs, human);
                case ProfessionType.Scholar: return GetScholarInteractions(profession, specs, human);
                case ProfessionType.Builder: return GetBuilderInteractions(profession, specs, human);
                default:
                    throw new NotImplementedException("The profession type is unknown to this mod and probably new.  " +
                        "This mod is thus incompatible with the current version of the game.  Please disable it.");
            }
        }

        private static Resource[] FieldsResources = new Resource[] { Resource.HealingPlantCultivated, Resource.Strawberry, Resource.Tomato, Resource.Pumpkin, Resource.Potato, Resource.Wheat };
        private static String[] CookedFood = new string[] { "bakedApple", "bakedPotato", "bakedTomato", "pumpkinStew", "potatoSoup", "fruitSalad", "bread", "strawberryCake", "appleStrudel" };
        private static String[] Meds = new string[] { "healingPotion", "medicine" };

        private static InteractionInfo GetFarmerInteractions(Profession p, List<String> specs, HumanAI human) {

            InteractionInfo info = null;

            for (int i = 0; i < specs.Count && info == null; i++) {
                if (!p.HasSpecialization(specs[i])) continue;

                switch (specs[i]) {
                    case "tendToFields": info = GetTendToFieldsInteraction(human); break;
                    case "cook": {
                            foreach (var interaction in GetCookInteractions()) {
                                info = CheckInteraction(interaction, human);
                                if (info != null) break;
                            }
                        }
                        break;
                    case "harvestCotton":
                    case "harvestApples": {
                            InteractionRestricted interaction = new InteractionRestricted(Interaction.GatherResource,
                               new List<InteractionRestriction>()
                               {
                            new InteractionRestrictionResource(specs[i] == "harvestCotton" ?  Resource.Cotton : Resource.Apple),
                            new InteractionRestrictionDesignation(Designation.GatherPlant)
                               });

                            info = CheckInteraction(interaction, human);
                        }
                        break;

                    case "careForAnimals": {
                            foreach (var interactionType in new Interaction[] { Interaction.Butcher, Interaction.Tame, Interaction.Milk, Interaction.Shear }) {
                                InteractionRestricted interaction = new InteractionRestricted(interactionType, interactionType == Interaction.Butcher ? new InteractionRestrictionAutoButcher() : null);

                                info = CheckInteraction(interaction, human);
                                if (info != null) break;
                            }
                        }
                        break;

                    default:
                        foreach (var x in GetOtherInteractions(p, specs[i])) {
                            info = CheckInteraction(x, human);
                            if (info != null) break;
                        }
                        break;
                }
            }

            return info;
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

        public static InteractionInfo GetTendToFieldsInteraction(HumanAI human) {
            //Build a dictionary of ratios (NbOwned vs Capacity) so that we order the actions
            //By what is most needed.  i.e. if we have lots of tomatoes but no potatoes, we'll harvest potatoes first
            Dictionary<Resource, double> ratios = FieldsResources.ToDictionary(x => x, x => GetResourceRatio(x));

            //Re-obtain the field resource and then order them by ratio 
            List<Resource> sortedFieldRes = new List<Resource>(FieldsResources);
            sortedFieldRes.Sort((x, y) => ratios[x].CompareTo(ratios[y]));

            bool lowOnFood = (GameState.Instance.GetResource(Resource.FoodRaw) + GameState.Instance.GetResource(Resource.FoodCooked)) < (5 * WorldScripts.Instance.humanManager.GetColonistCount());
            Vector3 position = human.GetPosition();

            //If low on food, the healing plants and wheat will have the lowest priority
            //Since they're not edible raw foods
            if (lowOnFood) {
                sortedFieldRes.Remove(Resource.HealingPlantCultivated);
                sortedFieldRes.Remove(Resource.Wheat);

                foreach (Resource edibleRes in sortedFieldRes) {
                    foreach (SoilModule soil in WorldScripts.Instance.furnitureFactory.GetFurnitureOwnedBy(WorldScripts.Instance.humanManager.colonyFaction)
                    .Where(x => x.HasModule<SoilModule>() && x.IsValid() && x.IsBuilt()).Select(x => x.GetModule<SoilModule>())
                    .Where(x => x.GetResource() == edibleRes && 
                    ((SoilModule.PlantStage)AccessTools.Field(x.GetType(), "plantStage").GetValue(x)) == SoilModule.PlantStage.Grown)
                    .OrderBy(x => Vector3.Distance(position, x.parent.GetPosition()))) {
                        return new InteractionInfo(Interaction.GatherResource, soil.parent.GetInteractable(), null, true, 50, false);
                    }
                }

                sortedFieldRes.Add(Resource.HealingPlantCultivated);
                sortedFieldRes.Add(Resource.Wheat);
            }


            for (int r = 0; r < sortedFieldRes.Count; r++) {


                foreach (SoilModule soil in WorldScripts.Instance.furnitureFactory.GetFurnitureOwnedBy(WorldScripts.Instance.humanManager.colonyFaction)
                    .Where(x => x.HasModule<SoilModule>() && x.IsValid() && x.IsBuilt()).Select(x => x.GetModule<SoilModule>())
                    .Where(x => x.GetResource() == sortedFieldRes[r])
                    .OrderBy(x => Vector3.Distance(position, x.parent.GetPosition()))) {

                    foreach (var interactionRestricted in soil.GetInteractions(human, true, false)) {
                        if (interactionRestricted.interaction != Interaction.RemovePlant &&
                            interactionRestricted.interaction != Interaction.RemoveInfestedPlants &&
                            interactionRestricted.interaction != YieldMicroInteractionHelper.TendToFieldsInteraction &&
                            CheckInteraction(soil.parent.GetInteractable(), interactionRestricted, human)) {
                            return new InteractionInfo(YieldMicroInteractionHelper.TendToFieldsInteraction, soil.parent.GetInteractable(), null, true, 50, false);
                        }
                    }
                }
            }

            return null;
        }

        private static InteractionInfo GetForesterInteractions(Profession p, List<String> specs, HumanAI human) {

            InteractionInfo info = null;

            for (int i = 0; i < specs.Count && info == null; i++) {
                if (!p.HasSpecialization(specs[i])) continue;

                switch (specs[i]) {
                    case "chopTrees":
                    case "clearStumps":

                        //If the colonist has both specializations, get the nearest tree
                        if (p.HasSpecialization("chopTrees") && p.HasSpecialization("clearStumps")) {
                            InteractionInfo treeInfo = CheckInteraction(new InteractionRestricted(Interaction.GatherResource,
                                new List<InteractionRestriction>()
                                {
                                new InteractionRestrictionResource(Resource.Wood),
                                new InteractionRestrictionDesignation(Designation.CutTree), 
                                }), human);

                            InteractionInfo stumpInfo = CheckInteraction(new InteractionRestricted(Interaction.ClearStumps,
                                new List<InteractionRestriction>()
                                {
                                new InteractionRestrictionResource(Resource.Wood),
                                new InteractionRestrictionDesignation(Designation.CutTree)
                                }), human);

                            if (treeInfo == null) info = stumpInfo;
                            else if (stumpInfo == null) info = treeInfo;
                            else {
                                float distanceTree = Vector3.Distance(human.GetPosition(), treeInfo.interactable.GetWorldPosition());
                                float distanceStump = Vector3.Distance(human.GetPosition(), stumpInfo.interactable.GetWorldPosition());

                                if (distanceTree <= distanceStump)
                                    info = treeInfo;
                                else
                                    info = stumpInfo;
                            }
                        } else {

                            info = CheckInteraction(new InteractionRestricted(specs[i] == "chopTrees" ? Interaction.GatherResource : Interaction.ClearStumps,
                                new List<InteractionRestriction>()
                                {
                                new InteractionRestrictionResource(Resource.Wood),
                                new InteractionRestrictionDesignation(Designation.CutTree)
                                }), human);
                        }

                        break;

                    case "growTrees":
                        foreach (var type in new Interaction[] { Interaction.Sow, Interaction.Care }) {
                            info = CheckInteraction(new InteractionRestricted(type,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionModule(typeof(GrowingSpotModule), true)
                            }), human);

                            if (info != null) break;
                        }
                        break;

                    default:
                        foreach (var x in GetOtherInteractions(p, specs[i])) {
                            info = CheckInteraction(x, human);
                            if (info != null) break;
                        }
                        break;
                }
            }

            return info;
        }

        private static InteractionInfo GetMinerInteractions(Profession p, List<String> specs, HumanAI human) {
            InteractionInfo info = null;

            for (int i = 0; i < specs.Count && info == null; i++) {
                if (!p.HasSpecialization(specs[i])) continue;

                switch (specs[i]) {
                    case "mineStone":
                        info = CheckInteraction(new InteractionRestricted(Interaction.GatherResource,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionResource(Resource.Stone),
                                new InteractionRestrictionDesignation(Designation.Mine)
                            }), human);
                        break;

                    case "mineIron":
                        info = CheckInteraction(new InteractionRestricted(Interaction.GatherResource,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionResource(Resource.IronOre),
                                new InteractionRestrictionDesignation(Designation.Mine)
                            }), human);
                        break;

                    case "mineCrystal":
                        info = CheckInteraction(new InteractionRestricted(Interaction.GatherResource,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionResource(Resource.Crystal),
                                new InteractionRestrictionDesignation(Designation.Mine)
                            }), human);
                        break;

                    default:
                        foreach (var x in GetOtherInteractions(p, specs[i])) {
                            info = CheckInteraction(x, human);
                            if (info != null) break;
                        }
                        break;
                }
            }

            return info;
        }

        private static InteractionInfo GetCraftsmanInteractions(Profession p, List<String> specs, HumanAI human) {
            InteractionInfo info = null;

            for (int i = 0; i < specs.Count && info == null; i++) {
                if (!p.HasSpecialization(specs[i])) continue;

                switch (specs[i]) {
                    case "carpenting":
                    case "tailoring":
                    case "masonry":
                    case "forging":
                    case "weaving":
                        info = CheckInteraction(new InteractionRestricted(Interaction.Produce,
                        new List<InteractionRestriction>()
                        {
                            new InteractionRestrictionProfessionSpecialization(specs[i]),
                            new InteractionRestrictionProfession(ProfessionType.Craftsman)
                        }), human);
                        break;

                    default:
                        foreach (var x in GetOtherInteractions(p, specs[i])) {
                            info = CheckInteraction(x, human);
                            if (info != null) break;
                        }
                        break;
                }
            }

            return info;
        }

        private static InteractionInfo GetDoctorInteractions(Profession p, List<String> specs, HumanAI human) {
            InteractionInfo info = null;

            for (int i = 0; i < specs.Count && info == null; i++) {
                if (!p.HasSpecialization(specs[i])) continue;

                switch (specs[i]) {
                    case "produceMedicine":
                        List<String> orderedMeds = new List<string>(Meds);
                        orderedMeds.Sort((x, y) => GetResourceRatio(GameState.GetResourceByString(x)).CompareTo(GetResourceRatio(GameState.GetResourceByString(y))));

                        for (int r = 0; r < orderedMeds.Count; r++) {
                            info = CheckInteraction(new InteractionRestricted(Interaction.Produce,
                            new List<InteractionRestriction>()
                            {
                                new InteractionRestrictionProduction(new List<string>(){ orderedMeds[r] })
                            }), human);
                        }
                        break;

                    case "tendToPatients":
                        foreach (var interaction in new Interaction[] { Interaction.TryFluTreatment, Interaction.GiveFood, Interaction.GiveHealingPotion, Interaction.BandageWounds, Interaction.SplintArm, Interaction.SplintLeg }) {
                            info = CheckInteraction(new InteractionRestricted(interaction,
                                interaction == Interaction.TryFluTreatment ? null : new InteractionRestrictionFaction()), human);

                            if (info != null) break;
                        }
                        break;

                    case "buryDead":
                        //bury colonists
                        info = CheckInteraction(new InteractionRestricted(Interaction.Bury, new InteractionRestrictionFaction()),
                            human);

                        if(info != null) {
                            var filledInfo = AccessTools.Field(typeof(GraveModule), "filled");

                            if (!WorldScripts.Instance.furnitureFactory.GetModules<GraveModule>().Any(x =>
                                    !((bool)filledInfo.GetValue(x)))) {
                                if (AbsoluteProfessionPrioritiesMod.Instance.buryColonistFailCooldown.HasValue) {
                                    info = null;
                                }
                            } else
                                AbsoluteProfessionPrioritiesMod.Instance.buryColonistFailCooldown = null;
                        }
                        
                        if(info == null){
                            //bury non-colonists
                            info = CheckInteraction(new InteractionRestricted(Interaction.Bury, new List<InteractionRestriction>() {
                                new InteractionRestrictionEmptyGrave(),
                                new InteractionRestrictionFactionType(FactionType.Colony, false)}),
                                human);
                        } 
                        break;

                    case "brewBeer":
                        info = CheckInteraction(new InteractionRestricted(Interaction.Produce, new InteractionRestrictionProduction(new List<string>() { "beer" })), human);
                        break;

                    default:
                        foreach (var x in GetOtherInteractions(p, specs[i])) {
                            info = CheckInteraction(x, human);
                            if (info != null) break;
                        }
                        break;
                }
            }

            return info;
        }

        private static InteractionInfo GetScholarInteractions(Profession p, List<String> specs, HumanAI human) {
            InteractionInfo info = null;

            for (int i = 0; i < specs.Count && info == null; i++) {
                if (!p.HasSpecialization(specs[i])) continue;

                switch (specs[i]) {
                    case "performResearch":
                        info = CheckInteraction(new InteractionRestricted(Interaction.Produce,
                            new InteractionRestrictionProfession(ProfessionType.Scholar)), human);
                        break;

                    case "mineCrystal":
                        info = CheckInteraction(new InteractionRestricted(Interaction.GatherResource,
                            new InteractionRestrictionResource(Resource.Crystal)), human);
                        break;

                    default:
                        foreach (var x in GetOtherInteractions(p, specs[i])) {
                            info = CheckInteraction(x, human);
                            if (info != null) break;
                        }
                        break;
                }
            }

            return info;
        }

        private static InteractionInfo GetBuilderInteractions(Profession p, List<String> specs, HumanAI human) {
            InteractionInfo info = CheckInteraction(new InteractionRestricted(Interaction.Construct), human);
            if (info == null) {
                info = CheckInteraction(new InteractionRestricted(Interaction.Deconstruct), human);
            }

            return info;
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

        public static InteractionInfo CheckInteraction(InteractionRestricted interactionRestricted, HumanAI human, float distance = 40f) {
            if (interactionRestricted.interaction == Interaction.Tame) {
                foreach (HumanAI animal in human.faction.GetLivingHumans().Where(x => x.animal != null).OrderBy(x => x.animal.tameness)) {
                    if (CheckInteraction(animal.GetInteractable(), interactionRestricted, human)) {
                        return new InteractionInfo(interactionRestricted.interaction, animal.GetInteractable(), interactionRestricted.restrictions, true, 50, false);
                    }
                }
                return null;
            }

            Interactable interactable = SearchForInteractableWithInteraction(human, interactionRestricted.interaction, distance, interactionRestricted.restrictions, true);

            if (interactable != null && interactable.IsValid()) {
                return new InteractionInfo(interactionRestricted.interaction, interactable, interactionRestricted.restrictions, true, 50, false);
            }
            return null;
        }

        public static Interactable SearchForInteractableWithInteraction(HumanAI human, Interaction interaction, float distance, List<InteractionRestriction> restrictions = null, bool probablyReachablesOnly = false, bool objectInteraction = false, Vector3 positionOverride = default(Vector3)) {
            if (interaction == Interaction.Construct) { return human.SearchForInteractbleWithConstructPriority(interaction, distance, restrictions, probablyReachablesOnly, objectInteraction, positionOverride); }
            return InteractableBookkeeperHelper.SearchForInteractablesWithInteraction(human, interaction, distance, restrictions, probablyReachablesOnly, objectInteraction, positionOverride).FirstOrDefault();
        }

        public static bool CheckInteraction(Interactable interactable, InteractionRestricted interactionRestricted, HumanAI human) {
            if (!interactable.IsPossibleInteraction(interactionRestricted.interaction, human, true, false)) { return false; }
            if (interactionRestricted.restrictions != null && interactionRestricted.restrictions.Any(x => x.IsRestrictedFast(interactable, interactionRestricted.interaction, human, true))) { return false; }
            return true;
        }
    }
}
