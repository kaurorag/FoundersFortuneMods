using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitchyMods.AbsoluteProfessionPriorities {
    public class InteractionRestrictionEmptyGrave : InteractionRestriction {
        public override bool IsRestricted(Interactable interactable, Interaction interaction, HumanAI actor, bool issuedByAI, out string reason) {
            reason = "";
            if (!(interactable is HumanInteractable)) { return true; }

            HumanInteractable humanInteractable = (HumanInteractable)interactable;
            if (humanInteractable.GetHuman() == null) { return true; }
            if (humanInteractable.GetHuman().alive) { return true; }

            var filledInfo = AccessTools.Field(typeof(GraveModule), "filled");

            if (!WorldScripts.Instance.furnitureFactory.GetModules<GraveModule>().Any(x =>
                !((bool)filledInfo.GetValue(x)))) {
                return true;
            }

            return false;
        }
    }

    public class InteractionRestrictionFactionType : InteractionRestriction {
        public FactionType factionType;
        public bool mustBeThisType = true;

        public InteractionRestrictionFactionType(FactionType factionType, bool mustBeThisType = true) {
            this.factionType = factionType;
            this.mustBeThisType = mustBeThisType;
        }

        public override bool IsRestricted(Interactable interactable, Interaction interaction, HumanAI actor, bool issuedByAI, out string reason) {
            reason = "Doesn't belong to the correct faction";
            return (interactable.GetFaction().GetFactionType() == factionType) != mustBeThisType;
        }
    }
}
