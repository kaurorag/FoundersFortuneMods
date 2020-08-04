using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {
    [System.Serializable]
    public class SubSpecialization {
        public string Name { get; set; }
        public int Priority { get; set; }
        public bool Active { get; set; } = true;

        public virtual IEnumerable<Interactable> GetInteractables(HumanAI human) { return null; }
        public virtual InteractionInfo GetNextInteraction(HumanAI human) { return null; }
    }
}
