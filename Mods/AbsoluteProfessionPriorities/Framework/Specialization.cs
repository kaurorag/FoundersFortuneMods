using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities {

    public class SpecializationDescriptor {
        public string Name { get; set; }
        public bool CanAutoManageSubSpecializations { get; set; } = true;

        public Dictionary<string, SubSpecializationDescriptor> SubSpecializations { get; } = new Dictionary<string, SubSpecializationDescriptor>();

        public SpecializationDescriptor(string name) {
            this.Name = name;
        }

        public string GetDisplayName() {
            return Localization.GetText("professionSpecialization_" + this.Name);
        }

        public Specialization ToSpecialization(HumanAI human, int priority) {
            return new Specialization(this.Name, priority,
                human.professionManager.IsSpecializationActive(this.Name),
                this.SubSpecializations.Values.Select(x=>x.ToSubSpecialization()).ToArray()
            );
        }
    }

    [System.Serializable]
    public class Specialization {
        public string Name { get; set; }
        public int Priority { get; set; }
        public bool Active { get; set; } = true;
        public bool AutoManageSubSpecializations { get; set; } = true;
        public Dictionary<string, SubSpecialization> SubSpecializations { get; } = new Dictionary<string, SubSpecialization>();

        public Specialization(string name, int priority, bool active,  SubSpecialization[] subs) {
            this.Name = name;
            this.Priority = priority;
            this.Active = active;
            
            for(int i = 0; i < subs.Length; i++) {
                this.SubSpecializations.Add(subs[i].Name, subs[i]);
                subs[i].Priority = i;
            }
        }

        public IEnumerable<SubSpecialization> GetOrderedSubSpecializations() {
            return this.SubSpecializations.Values.Where(x => x.Active).OrderBy(x => UnityEngine.Random.Range(0f, 1f)).OrderBy(x => x.Priority);
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

        public SubSpecialization ToSubSpecialization(int priority=0) {
            return new SubSpecialization() {
                Name = this.Name,
                Priority = priority
            };
        }
    }

    [System.Serializable]
    public class SubSpecialization {
        public string Name { get; set; }
        public int Priority { get; set; }
        public bool Active { get; set; } = true;
    }
}
