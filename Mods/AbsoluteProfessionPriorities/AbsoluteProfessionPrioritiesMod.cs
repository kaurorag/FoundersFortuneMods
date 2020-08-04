using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WitchyMods.AbsoluteProfessionPriorities.Framework;

namespace WitchyMods.AbsoluteProfessionPriorities {
    [Serializable]
    public class AbsoluteProfessionPrioritiesMod : Mod {

        public Dictionary<long, Dictionary<ProfessionType, Dictionary<string, Specialization>>> ColonistsData;

        [NonSerialized]
        public static AbsoluteProfessionPrioritiesMod Instance;

        [NonSerialized]
        public static int MaxTreeWoodAmount = 10;

        public float? buryColonistFailCooldown;



        public override void Load() {
            Harmony harmony = new Harmony("AbsoluteProfessionPriorities");
            harmony.PatchAll();
        }

        public override void Start() {
            Instance = this;

            if (this.ColonistsData == null)
                this.ColonistsData = new Dictionary<long, Dictionary<ProfessionType, Dictionary<string, Specialization>>>();

            //Get all of the IDs of the current colonists
            HumanManager humanManager = WorldScripts.Instance.humanManager;
            List<HumanAI> colonists = humanManager.colonyFaction.GetLivingHumanoids().ToList();

            //Remove all of the saved IDs that are not in the colony anymore
            foreach (var removedId in this.ColonistsData.Keys.Where(x => !colonists.Any(y=>y.GetID() ==x)).ToArray()) {
                this.ColonistsData.Remove(removedId);
            }

            //Init all of the IDs
            foreach (var colonist in colonists) {
                InitColonist(colonist);
            }

            MaxTreeWoodAmount = ModHandler.mods.furnitures.Values
                .Where(x => x.modules.ContainsKey("resource"))
                .Select(x => x.GetCachedModule<ResourceModule>())
                .Where(x => x.GetResource() == Resource.Wood).Max(x => x.resourcesPerRound);
        }

        public void InitColonist(HumanAI human) {

            var descriptors = GetSpecializationDescriptors();
            long id = human.GetID();

            if (!ColonistsData.ContainsKey(id))
                this.ColonistsData.Add(id, new Dictionary<ProfessionType, Dictionary<string, Specialization>>());
            else {
                foreach (var profession in this.ColonistsData[id].Keys.ToArray().Where(x => !descriptors.ContainsKey(x)))
                    this.ColonistsData[id].Remove(profession);
            }            

            foreach(var profession in descriptors.Keys.ToArray()) {
                InitColonistForProfession(human, profession, descriptors[profession], this.ColonistsData[id]);
            }            
        }

        private void InitColonistForProfession(HumanAI human, ProfessionType profession, 
            Dictionary<string, SpecializationDescriptor> descriptors,
            Dictionary<ProfessionType, Dictionary<string, Specialization>> profSpecs) {

            if (!profSpecs.ContainsKey(profession))
                profSpecs.Add(profession, new Dictionary<string, Specialization>());
            else {
                foreach (var specName in profSpecs[profession].Keys.ToArray().Where(x => !descriptors.ContainsKey(x)))
                    profSpecs[profession].Remove(specName);
            }

            foreach (var specName in descriptors.Keys.ToArray()) {
                InitColonistForSpecialization(human, specName, descriptors[specName], profSpecs[profession]);
            }
        }

        private void InitColonistForSpecialization(HumanAI human, string specName, 
            SpecializationDescriptor desc, 
            Dictionary<string, Specialization> specs) {

            if (!specs.ContainsKey(specName)) {
                specs.Add(specName, desc.ToSpecialization(human,
                    specs.Count));
            } else {
                foreach (var subSpecName in specs[specName].SubSpecializations.Keys.ToArray()
                    .Where(x => !desc.SubSpecializations.Keys.Contains(x))) {
                    specs[specName].SubSpecializations.Remove(subSpecName);
                }
            }

            foreach (var subSpecName in desc.SubSpecializations.Keys.ToArray()) {
                InitColonistForSubSpecialization(human, subSpecName, desc.SubSpecializations[subSpecName], specs[specName]);
            }
        }

        private void InitColonistForSubSpecialization(HumanAI human, string subSpecName, 
            SubSpecializationDescriptor desc, Specialization spec) {

            if (!spec.SubSpecializations.ContainsKey(subSpecName))
                spec.SubSpecializations.Add(subSpecName,
                    desc.ToSubSpecialization(spec.SubSpecializations.Count));
        }

        public override void Update() {
            if (buryColonistFailCooldown.HasValue) {
                buryColonistFailCooldown -= Time.deltaTime;
                if (buryColonistFailCooldown < 0) buryColonistFailCooldown = null;
            }
        }

        public static Dictionary<ProfessionType, Dictionary<string,SpecializationDescriptor>> GetSpecializationDescriptors() {
            Dictionary<ProfessionType, Dictionary<string, SpecializationDescriptor>> descriptors = new Dictionary<ProfessionType, Dictionary<string, SpecializationDescriptor>>();

            List<string> subSpecializations = GetSubSpecializations();

            foreach(ProfessionType profession in Enum.GetValues(typeof(ProfessionType))) {
                if (profession == ProfessionType.NoJob || profession == ProfessionType.Soldier) continue;

                descriptors.Add(profession, new Dictionary<string, SpecializationDescriptor>());

                foreach(var spec in ModHandler.mods.professionSpecializations.Values
                    .Where(x=>x.professionNames.Contains(profession.ToString().ToLower()))) {
                    descriptors[profession].Add(spec.name, new SpecializationDescriptor(profession, spec.name));

                    foreach(var subName in subSpecializations.Where(x => x.StartsWith($"{profession}_{spec.name}_", StringComparison.InvariantCultureIgnoreCase))) {
                        descriptors[profession][spec.name].SubSpecializations.Add(subName, new SubSpecializationDescriptor(subName));
                    }

                    switch (spec.name) {
                        case "tendToFields":
                        case "cook":
                        case "careForAnimals":
                        case "produceMedicine":
                        case "chopTrees":
                        case "growTrees":
                        case "performResearch":
                            descriptors[profession][spec.name].CanAutoManageSubSpecializations = true;
                            break;

                        default:
                            descriptors[profession][spec.name].CanAutoManageSubSpecializations = false;
                            break;
                    }
                }
            }

            return descriptors;
        }

        public static List<string> GetSubSpecializations() {
            return new List<string>() {
                //Farmer
                "Farmer_TendToFields_Tomato", 
                "Farmer_TendToFields_Potato", 
                "Farmer_TendToFields_Strawberry", 
                "Farmer_TendToFields_Wheat", 
                "Farmer_TendToFields_Pumpkin",
                "Farmer_TendToFields_HealingPlantCultivated", 

                "Farmer_Cook_CampireFire", 
                "Farmer_Cook_Kitchen", 
                "Farmer_Cook_Bakery", 

                "Farmer_CareForAnimals_Butcher", 
                "Farmer_CareForAnimals_Shear", 
                "Farmer_CareForAnimals_Milk", 
                "Farmer_CareForAnimals_Tame",

                "Forester_ChopTrees_Stumps",
                "Forester_ChopTrees_BigTrees",
                "Forester_ChopTrees_SmallTrees",

                "Forester_GrowTrees_PineTrees",
                "Forester_GrowTrees_Apples", 
                "Forester_GrowTrees_Cotton", 

                "Doctor_ProduceMedicine_FluVaccines", 
                "Doctor_ProduceMedicine_HealingPotions", 

                "Scholar_PerformResearch_BookStand", 
                "Scholar_PerformResearch_ScrollStand"
            };
        }
    }
}
