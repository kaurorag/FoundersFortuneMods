using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {

    [System.Serializable]
    public abstract class Specialization {
        [NonSerialized]
        public const Interaction TendToFieldsInteraction = (Interaction)(-1);
        public const Interaction ChopTrees = (Interaction)(-2);
        public const Interaction SowAndCareForTrees = (Interaction)(-3);

        private float maxDistance = -1;

        public ProfessionType Profession { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }
        public float MaxDistance { get { return maxDistance; } set { maxDistance = value <= 0 ? -1 : value; } }
        public bool Active { get; set; } = true;
        public bool AutoManageSubSpecializations { get; set; } = true;
        public Dictionary<string, SubSpecialization> SubSpecializations { get; } = new Dictionary<string, SubSpecialization>();

        public Specialization(ProfessionType profession, string name, int priority, bool active, SubSpecialization[] subs) {
            this.Profession = profession;
            this.Name = name;
            this.Priority = priority;
            this.Active = active;

            for (int i = 0; i < subs.Length; i++) {
                this.SubSpecializations.Add(subs[i].Name, subs[i]);
                subs[i].Priority = i;
            }
        }

        public abstract IEnumerable<InteractionRestricted> GetInteractionRestricted(HumanAI human);

        public IEnumerable<InteractionInfo> GetNextInteraction(HumanAI human) {
            foreach (var intRes in GetInteractionRestricted(human)) {
                foreach (var interactable in human.SearchForInteractablesWithInteraction(intRes.interaction, this.MaxDistance, intRes.restrictions, true)) {
                    if (WorkInteractionControllerPatch.CheckInteraction(interactable, intRes, human)) {
                        yield return GetInteractionInfo(intRes, interactable, human);
                    }
                }
            }
        }

        public IEnumerable<SubSpecialization> GetOrderedSubSpecializations() {
            return this.SubSpecializations.Values.Where(x => x.Active).OrderBy(x => (x.Priority + 10) + UnityEngine.Random.Range(0f, 1f));
        }

        private InteractionInfo GetInteractionInfo(InteractionRestricted intRes, Interactable interactable, HumanAI human) {
            if (WorkInteractionControllerPatch.CheckInteraction(interactable, intRes, human))
                return new InteractionInfo(intRes.interaction, interactable, intRes.restrictions, true, 50, false, true) {
                    data = new InteractionMaxDistanceData(this.MaxDistance)
                };
            return null;
        }

        public static double GetResourceRatio(Resource res) {
            int nbOwned = GameState.Instance.GetResource(res);
            int nbMax = WorldScripts.Instance.furnitureFactory.GetStockpileLimit(res);
            double ratio = nbOwned * 1.0 / nbMax;

            return ratio;
        }
    }
}
