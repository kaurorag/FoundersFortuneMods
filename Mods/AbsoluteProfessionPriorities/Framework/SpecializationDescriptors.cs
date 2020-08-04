using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {
    public class SpecializationDescriptor {
        public ProfessionType Profession { get; set; }
        public string Name { get; set; }
        public bool CanAutoManageSubSpecializations { get; set; } = true;

        public Dictionary<string, SubSpecializationDescriptor> SubSpecializations { get; } = new Dictionary<string, SubSpecializationDescriptor>();

        public SpecializationDescriptor(ProfessionType profession, string name) {
            this.Profession = profession;
            this.Name = name;
        }

        public string GetDisplayName() {
            return Localization.GetText("professionSpecialization_" + this.Name);
        }

        public Specialization ToSpecialization(HumanAI human, int priority) {
            bool isActive = human.professionManager.IsSpecializationActive(this.Name);
            SubSpecialization[] subs = this.SubSpecializations.Values.Select(x => x.ToSubSpecialization()).ToArray();

            switch (this.Profession) {
                case ProfessionType.Farmer: return GetFarmerSpecialization(human, priority, isActive, subs);
                default: return null;
            }
        }

        public Specialization GetFarmerSpecialization(HumanAI human, int priority, bool active, SubSpecialization[] subs) {
            switch (this.Name) {
                case "tendToFields": return new TendToFieldsSpecialization(priority, active, subs);
                case "cook": return new CookSpecialization(priority, active, subs);
                case "careForAnimals": return new CareForAnimalsSpecialization(priority, active, subs);
                case "harvestApples": return new HarvestApplesSpecialization(priority, active, subs);
                case "harvestCotton": return new HarvestCottonSpecialization(priority, active, subs);
                default: return null;
            }
        }

        public Specialization GetCraftmanSpecialization(HumanAI human, int priority, bool active, SubSpecialization[] subs) {
            switch (this.Name) {
                case "carpenting": 
                case "weaving": 
                case "tailoring": 
                case "masonry":
                case "forging": return new CraftsmanSpecialization(this.Name, priority, active, subs);
                default: return null;
            }
        }

        public Specialization GetForesterSpecialization(HumanAI human, int priority, bool active, SubSpecialization[] subs) {
            switch (this.Name) {
                case "chopTrees": return new ChopTreesSpecialization(priority, active, subs);
                case "growTrees": return new GrowTreesSpecialization(priority, active, subs);
                default: return null;
            }
        }

        public Specialization GetMinerSpecialization(HumanAI human, int priority, bool active, SubSpecialization[] subs) {
            switch (this.Name) {
                case "mineStone": return new MineStoneSpecialization(priority, active, subs);
                case "mineIron": return new MineIronSpecialization(priority, active, subs);
                case "mineCrystal": return new MineCrystalSpecialization(this.Profession, priority, active, subs);
                default: return null;
            }
        }

        public Specialization GetScholarSpecialization(HumanAI human, int priority, bool active, SubSpecialization[] subs) {
            switch (this.Name) {
                case "performResearch": return new PerformResearchSpecialization(priority, active, subs);
                case "mineCrystal": return new MineCrystalSpecialization(this.Profession, priority, active, subs);
                default: return null;
            }
        }

        public Specialization GetDoctorSpecialization(HumanAI human, int priority, bool active, SubSpecialization[] subs) {
            switch (this.Name) {
                case "produceMedicine": return new ProduceMedicineSpecialization(priority, active, subs);
                case "tendToPatients": return new TendToPatientsSpecialization(priority, active, subs);
                case "buryDead": return new BuryDeadSpecialization(priority, active, subs);
                case "brewBeer": return new BrewBeerSpecialization(priority, active, subs);
                default: return null;
            }
        }

        public Specialization GetBuilderSpecialization(HumanAI human, int priority, bool active, SubSpecialization[] subs) {
            switch (this.Name) {
                case "buildFields": return new BuildFieldsSpecialization(priority, active, subs);
                case "buildStructures": return new BuildStructuresSpecialization(priority, active, subs);
                case "buildFurnitures": return new BuildFurnitureSpecialization(priority, active, subs);
                default: return null;
            }
        }
    }

    public class SubSpecializationDescriptor {
        public string Name { get; set; }

        public SubSpecializationDescriptor(string name) {
            this.Name = name;
        }

        public string GetDisplayName() {
            return Localization.GetText("subSpecialization_" + this.Name);
        }

        public SubSpecialization ToSubSpecialization(int priority = 0) {
            return new SubSpecialization() {
                Name = this.Name,
                Priority = priority
            };
        }
    }

}
