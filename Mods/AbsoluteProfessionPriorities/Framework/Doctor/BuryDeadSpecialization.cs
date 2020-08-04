using FFModUtils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WitchyMods.AbsoluteProfessionPriorities.Framework {
    public class BuryDeadSpecialization : Specialization {
        public BuryDeadSpecialization( int priority, bool active, SubSpecialization[] subs)
            : base(ProfessionType.Doctor, "buryDead", priority, active, subs) {
        }

        public override IEnumerable<InteractionRestricted> GetInteractionRestricted(HumanAI human) {
            yield return new InteractionRestricted(Interaction.Bury, new List<InteractionRestriction>(){
                new InteractionRestrictionFaction(), 
                new InteractionRestrictionEmptyGraveCooldown()});
            yield return new InteractionRestricted(Interaction.Bury, new List<InteractionRestriction>() {
                                new InteractionRestrictionEmptyGrave(),
                                new InteractionRestrictionFactionType(FactionType.Colony, false)});
        }
    }
}
