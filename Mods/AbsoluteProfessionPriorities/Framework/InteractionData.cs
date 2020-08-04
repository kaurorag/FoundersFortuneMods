using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {
    public class InteractionMaxDistanceData : InteractionData {
        public float MaxDistance { get; set; }

        public InteractionMaxDistanceData(float maxDistance) {
            this.MaxDistance = maxDistance;
        }
    }
}
